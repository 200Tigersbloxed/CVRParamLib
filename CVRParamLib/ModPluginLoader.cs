#nullable enable

using System;
using System.IO;
using ABI_RC.Core;
using ABI_RC.Core.Networking.IO.UserGeneratedContent;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.MovementSystem;
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
    [HarmonyPatch(typeof(PlayerSetup), "Start")]
    class PlayerSetupStartHook
    {
        [HarmonyPostfix]
        static void Start(PlayerSetup __instance)
        {
            if (ReplicatedModInfo._instance == null)
                ReplicatedModInfo._instance = new CVRParameterInstance(__instance);
            ReplicatedModInfo._instance.UpdatePlayerSetup(__instance);
        }
    }

    /*
    [HarmonyPatch(typeof(PlayerSetup), "PreSetupAvatarGeneral")]
    class ContentHook
    {
        [HarmonyPostfix]
        static void PreSetupAvatarGeneral()
        {
            ReplicatedModInfo._instance?.HarmonyAvatarChange(ReplicatedModInfo._instance.PlayerSetup._avatar, MetaPort.Instance.currentAvatarGuid);
        }
    }*/

    [HarmonyPatch(typeof(MovementSystem), "UpdateAnimatorManager")]
    class MovementSystemHook
    {
        [HarmonyPostfix]
        static void UpdateAnimatorManager(CVRAnimatorManager manager)
        {
            ReplicatedModInfo._instance?.HarmonyAvatarChange(manager);
        }
    }

    [HarmonyPatch(typeof(AvatarDetails_t), "Recycle")]
    class AvatarDetails_tHook
    {
        [HarmonyPrefix]
        static void Recycle(AvatarDetails_t __instance) => AvatarHandler.CacheAvatar(__instance);
    }
    
    [HarmonyPatch(typeof(PlayerSetup), "changeAnimatorParam")]
    class ChangeAnimatorParam_Hook
    {
        [HarmonyPostfix]
        static void changeAnimatorParam(string name, float value) {
            CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Debug,
                $"Parameter {name} changed to {value}.");
            // Todo: Use this to drive parameter changes instead of checking in update loop
            // Note1: Parameters driven by the CVR Stream component will spam the hell out of this method.
            // Note2: When a slider is being used, will also spam as well.
            // So a check with a dictionary far values change is advised, also for comparision of floats I would
            // recommend using this: https://docs.unity3d.com/ScriptReference/Mathf.Approximately.html
        }
    }
}