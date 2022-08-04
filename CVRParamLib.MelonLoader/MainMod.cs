#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using ABI_RC.Core.Base;
using ABI_RC.Core.Networking.IO.UserGeneratedContent;
using ABI_RC.Core.Player;
using HarmonyLib;
using MelonLoader;

namespace CVRParamLib.MelonLoader;

public class MainMod : MelonMod
{
    private readonly HarmonyLib.Harmony _harmony = new("CVRParamLib.MelonLoader-patch");
    private static CVRParameterInstance? _instance;
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
                    MelonLogger.Msg(ConsoleColor.Gray, $"[CVRParamLib] {o}");
                    break;
                case CVRParameterInstance.LogLevel.Log:
                    MelonLogger.Msg($"[CVRParamLib] {o}");
                    break;
                case CVRParameterInstance.LogLevel.Warning:
                    MelonLogger.Warning($"[CVRParamLib] {o}");
                    break;
                case CVRParameterInstance.LogLevel.Error:
                    MelonLogger.Error($"[CVRParamLib] {o}");
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
        Dictionary<string, float> p = new(ParameterManager.Parameters);
        foreach (string parametersKey in p.Keys)
        {
            float paramValue = ParameterManager.GetParameterValue(parametersKey, p) ?? default;
            _instance?.UpdateParameter(parametersKey, paramValue);
        }
        _instance?.Update();
        base.OnUpdate();
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

    [HarmonyPatch(typeof(PlayerSetup), "Start")]
    class PlayerSetupStartHook
    {
        static bool didStart = false;
        
        [HarmonyPostfix]
        static void Start(PlayerSetup __instance)
        {
            if (!didStart)
            {
                didStart = true;
                if (_instance == null)
                    _instance = new CVRParameterInstance(__instance);
                else
                    _instance.UpdatePlayerSetup(__instance);
            }
            else
                didStart = false;
        }
    }

    [HarmonyPatch(typeof(Content), "LoadIntoAvatar")]
    class ContentHook
    {
        static bool didLoad = false;
        
        [HarmonyPostfix]
        static void LoadIntoAvatar(string avatarId)
        {
            if (!didLoad)
            {
                didLoad = true;
                _instance?.HarmonyAvatarChange(avatarId);
            }
            else
                didLoad = false;
        }
    }

    [HarmonyPatch(typeof(AvatarDetails_t), "Recycle")]
    class AvatarDetails_tHook
    {
        [HarmonyPrefix]
        static void Recycle(AvatarDetails_t __instance)
        {
            AvatarHandler.CacheAvatar(__instance);
        }
    }
}