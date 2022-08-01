#nullable enable

using System.Collections.Generic;
using ABI_RC.Core.Player;
using BepInEx;
using HarmonyLib;

namespace CVRParamLib.BepInEx;

[BepInPlugin("bepinex.cvrparamlib.fortnite.lol", "CVRParamLib.BepInEx", "1.0.0")]
[BepInProcess("ChilloutVR.exe")]
public class MainPlugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new("CVRParamLib.BepInEx-patch");
    private static CVRParameterInstance? _instance;
    
    private void Awake()
    {
        InitHarmony();
        CVRParameterInstance.PrepareLog((level, o) =>
        {
            switch (level)
            {
                case CVRParameterInstance.LogLevel.Debug:
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
        OSCManager.Init();
    }
    
    private void Update()
    {
        Dictionary<string, float> p = new(ParameterManager.Parameters);
        foreach (string parametersKey in p.Keys)
        {
            float paramValue = ParameterManager.GetParameterValue(parametersKey, p) ?? default;
            _instance?.UpdateParameter(parametersKey, paramValue);
        }
    }

    private void InitHarmony()
    {
        _harmony.PatchAll();
        Logger.LogInfo("Patched Harmony!");
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
}