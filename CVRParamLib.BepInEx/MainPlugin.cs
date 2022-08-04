#nullable enable

using System.Collections.Generic;
using System.IO;
using ABI_RC.Core.Base;
using ABI_RC.Core.Networking.IO.UserGeneratedContent;
using ABI_RC.Core.Player;
using HarmonyLib;
using BepInEx;

namespace CVRParamLib.BepInEx;

[BepInPlugin("bepinex.cvrparamlib.fortnite.lol", "CVRParamLib.BepInEx", "1.1.0")]
[BepInProcess("ChilloutVR.exe")]
public class MainPlugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new("CVRParamLib.BepInEx-patch");
    private static CVRParameterInstance? _instance;

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
                    Logger.LogDebug($"[CVRParamLib] {o}");
                    break;
                case CVRParameterInstance.LogLevel.Log:
                    Logger.LogInfo($"[CVRParamLib] {o}");
                    break;
                case CVRParameterInstance.LogLevel.Warning:
                    Logger.LogWarning($"[CVRParamLib] {o}");
                    break;
                case CVRParameterInstance.LogLevel.Error:
                    Logger.LogError($"[CVRParamLib] {o}");
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
    
    private void Update()
    {
        Dictionary<string, float> p = new(ParameterManager.Parameters);
        foreach (string parametersKey in p.Keys)
        {
            float paramValue = ParameterManager.GetParameterValue(parametersKey, p) ?? default;
            _instance?.UpdateParameter(parametersKey, paramValue);
        }
        _instance?.Update();
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
    
    [HarmonyPatch(typeof(PlayerSetup), "Start")]
    class PlayerSetupStartHook
    {
        [HarmonyPostfix]
        static void Start(PlayerSetup __instance)
        {
            if (_instance == null)
                _instance = new CVRParameterInstance(__instance);
            else
                _instance.UpdatePlayerSetup(__instance);
        }
    }

    [HarmonyPatch(typeof(Content), "LoadIntoAvatar")]
    class ContentHook
    {
        [HarmonyPostfix]
        static void LoadIntoAvatar(string avatarId)
        {
            _instance?.HarmonyAvatarChange(avatarId);
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