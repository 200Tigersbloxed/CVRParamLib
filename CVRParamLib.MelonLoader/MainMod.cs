#nullable enable

using System;
using System.Collections.Generic;
using ABI_RC.Core.Player;
using HarmonyLib;
using MelonLoader;

namespace CVRParamLib.MelonLoader;

public class MainMod : MelonMod
{
    private readonly HarmonyLib.Harmony _harmony = new("CVRParamLib.MelonLoader-patch");
    private static CVRParameterInstance? _instance;
        
    public override void OnApplicationStart()
    {
        InitHarmony();
        CVRParameterInstance.PrepareLog((level, o) =>
        {
            switch (level)
            {
                case CVRParameterInstance.LogLevel.Debug:
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
        OSCManager.Init();
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
        base.OnUpdate();
    }

    private void InitHarmony()
    {
        _harmony.PatchAll();
        MelonLogger.Msg("Patched Harmony!");
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