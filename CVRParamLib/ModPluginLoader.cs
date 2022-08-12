#nullable enable

using System;
using System.IO;
using ABI_RC.Core.Networking.IO.UserGeneratedContent;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using MelonLoader;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace CVRParamLib;

internal static class ReplicatedModInfo
{
    public static CVRParameterInstance? _instance;
    public static HarmonyPatches hi = new();
}

internal class MelonLoaderMod : MelonMod
{
    private readonly HarmonyLib.Harmony _harmony = new("CVRParamLib.MelonLoader-patch");
    private static readonly string ConfigLocation = Path.Combine(MelonUtils.UserDataDirectory, "CVRParamLibConfig.cfg");
    
    public override void OnApplicationStart()
    {
        HarmonyPatches.MelonLoaderHarmonyBug = true;
        InitHarmony();
        CVRParameterInstance.PrepareLog((level, o) =>
        {
            switch (level)
            {
                case CVRParameterInstance.LogLevel.Debug:
                    if(!Config.LoadedConfig.ShowDebug)
                        break;
                    MelonLogger.Msg(ConsoleColor.Gray, o);
                    break;
                case CVRParameterInstance.LogLevel.Log:
                    MelonLogger.Msg(o);
                    break;
                case CVRParameterInstance.LogLevel.Warning:
                    MelonLogger.Warning(o);
                    break;
                case CVRParameterInstance.LogLevel.Error:
                    MelonLogger.Error(o);
                    break;
            }
        });
        CVRParameterInstance.PrepareCoroutine(enumerator =>
        {
            MelonCoroutines.Start(enumerator);
        });
        Config.Init(ConfigLocation, () =>
        {
            if(Config.LoadedConfig.EnableOSC)
                OSCManager.Init();
        });
        base.OnApplicationStart();
    }

    public override void OnUpdate()
    {
        ReplicatedModInfo._instance?.Update();
        base.OnUpdate();
    }

    public override void OnGUI()
    {
        if(Config.LoadedConfig.ShowDebug)
            GUIDebug.OnGUI();
        base.OnGUI();
    }

    public override void OnApplicationQuit()
    {
        Config.SaveConfig(ConfigLocation);
        base.OnApplicationQuit();
    }
    
    // bug? Why does MelonLoader's Harmony Invoke twice, but BepInEx's only once?

    private void InitHarmony()
    {
        _harmony.PatchAll();
        MelonLogger.Msg("Patched Harmony");
    }
}

[BepInPlugin("cvrparamlib.fortnite.lol", "CVRParamLib", "1.3.0")]
[BepInProcess("ChilloutVR.exe")]
internal class BepInExMod : BaseUnityPlugin
{
    private readonly HarmonyLib.Harmony _harmony = new("CVRParamLib.BepInEx-patch");

    private static readonly string ConfigLocation =
        Path.Combine(Path.GetDirectoryName(Paths.BepInExConfigPath), "CVRParamLibConfig.cfg");
    
    private void Awake()
    {
        InitHarmony();
        CVRParameterInstance.PrepareLog((level, o) =>
        {
            switch (level)
            {
                case CVRParameterInstance.LogLevel.Debug:
                    if(!CVRParamLib.Config.LoadedConfig.ShowDebug)
                        break;
                    Logger.LogDebug(o);
                    break;
                case CVRParameterInstance.LogLevel.Log:
                    Logger.LogInfo(o);
                    break;
                case CVRParameterInstance.LogLevel.Warning:
                    Logger.LogWarning(o);
                    break;
                case CVRParameterInstance.LogLevel.Error:
                    Logger.LogError(o);
                    break;
            }
        });
        CVRParameterInstance.PrepareCoroutine(enumerator =>
        {
            StartCoroutine(enumerator);
        });
        CVRParamLib.Config.Init(ConfigLocation, () =>
        {
            if(CVRParamLib.Config.LoadedConfig.EnableOSC)
                OSCManager.Init();
        });
    }
    
    private void Update() => ReplicatedModInfo._instance?.Update();

    private void OnGUI()
    {
        if(CVRParamLib.Config.LoadedConfig.ShowDebug)
            GUIDebug.OnGUI();
    }

    private void OnApplicationQuit()
    {
        CVRParamLib.Config.SaveConfig(ConfigLocation);
    }

    private void InitHarmony()
    {
        _harmony.PatchAll();
        Logger.LogInfo("Patched Harmony");
    }
}

internal class HarmonyPatches
{
    public static bool MelonLoaderHarmonyBug;
    
    [HarmonyPatch(typeof(PlayerSetup), "Start")]
    class PlayerSetupStartHook
    {
        static bool didStart;
        
        [HarmonyPostfix]
        static void Start(PlayerSetup __instance)
        {
            if (MelonLoaderHarmonyBug)
            {
                if (!didStart)
                {
                    didStart = true;
                    if (ReplicatedModInfo._instance == null)
                        ReplicatedModInfo._instance = new CVRParameterInstance(__instance);
                    else
                        ReplicatedModInfo._instance.UpdatePlayerSetup(__instance);
                }
                else
                    didStart = false;
            }
            else
            {
                if (ReplicatedModInfo._instance == null)
                    ReplicatedModInfo._instance = new CVRParameterInstance(__instance);
                else
                    ReplicatedModInfo._instance.UpdatePlayerSetup(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerSetup), "SetupAvatar")]
    class ContentHook
    {
        static bool didLoad;
        
        [HarmonyPostfix]
        static void SetupAvatar(GameObject inAvatar)
        {
            if (MelonLoaderHarmonyBug)
            {
                if (!didLoad)
                {
                    didLoad = true;
                    ReplicatedModInfo._instance?.HarmonyAvatarChange(MetaPort.Instance.currentAvatarGuid);
                }
                else
                    didLoad = false;
            }
            else
                ReplicatedModInfo._instance?.HarmonyAvatarChange(MetaPort.Instance.currentAvatarGuid);
        }
    }

    [HarmonyPatch(typeof(AvatarDetails_t), "Recycle")]
    class AvatarDetails_tHook
    {
        [HarmonyPrefix]
        static void Recycle(AvatarDetails_t __instance) => AvatarHandler.CacheAvatar(__instance);
    }
}