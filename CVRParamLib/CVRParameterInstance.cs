#nullable enable

using System;
using ABI_RC.Core.Player;

namespace CVRParamLib;

public class CVRParameterInstance
{
    public static CVRParameterInstance? Instance { get; private set; }
    public PlayerSetup PlayerSetup { get; private set; }
    private static Action<LogLevel, object>? Log { get; set; }

    public CVRParameterInstance(PlayerSetup _playerSetup)
    {
        Instance = this;
        PlayerSetup = _playerSetup;
        WriteLog(LogLevel.Log, "CVRParameterInstance Created!");
    }

    public static void PrepareLog(Action<LogLevel, object> _log) => Log = _log;

    public static void WriteLog(LogLevel logLevel, object msg) => Log?.Invoke(logLevel, msg);

    public void UpdatePlayerSetup(PlayerSetup _playerSetup)
    {
        PlayerSetup = _playerSetup;
        WriteLog(LogLevel.Debug, "PlayerSetup was updated");
    }

    public bool DoesParamNeedUpdated(string name)
    {
        bool areDifferent = ParameterManager.AreDifferent(name, PlayerSetup.GetAnimatorParam(name)) ?? false;
        return areDifferent;
    }

    public void UpdateParameter(string name, float value) => PlayerSetup.changeAnimatorParam(name, value);

    public enum LogLevel
    {
        Debug,
        Log,
        Warning,
        Error
    }
}