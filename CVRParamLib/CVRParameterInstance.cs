#nullable enable

using System;
using System.Collections;
using ABI.CCK.Components;
using ABI_RC.Core.Networking;
using ABI_RC.Core.Networking.IO.UserGeneratedContent;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using DarkRift;
using UnityEngine;

namespace CVRParamLib;

public class CVRParameterInstance
{
    public static CVRParameterInstance? Instance { get; private set; }
    public PlayerSetup PlayerSetup { get; private set; }
    private static Action<LogLevel, object>? Log { get; set; }
    private static Action<IEnumerator>? Coroutine { get; set; }

    public string UserId => MetaPort.Instance.ownerId;
    private GameObject? _avatarGO;
    public CVRAvatar? avatar { get; private set; }

    public CVRParameterInstance(PlayerSetup _playerSetup)
    {
        Instance = this;
        PlayerSetup = _playerSetup;
        WriteLog(LogLevel.Log, "CVRParameterInstance Created!");
    }

    public void Update()
    {
        AvatarHandler.Update();
        if (PlayerSetup._avatar != null && _avatarGO != PlayerSetup._avatar)
        {
            _avatarGO = PlayerSetup._avatar;
            avatar = _avatarGO.GetComponent<CVRAvatar>();
            ParameterManager.OnLocalAvatarChange(avatar.avatarSettings.settings);
            ParameterWriter.OnLocalAvatarChange();
        }
    }

    public static void PrepareLog(Action<LogLevel, object> _log) => Log = _log;
    public static void PrepareCoroutine(Action<IEnumerator> _coroutine) => Coroutine = _coroutine;

    public void HarmonyAvatarChange(string avatarId)
    {
        avatar = null;
        OSCMessageHandler.HandleAvatarChange(avatarId);
        AvatarHandler.OnLocalAvatarChanged(avatarId);
        CacheAvatarToAvatarList(avatarId);
        ParameterSDK.OnLocalAvatarChanged.Invoke(avatarId);
    }

    private IEnumerator cacheEnum(string avatarId)
    {
        while (NetworkManager.Instance.Api.ConnectionState != ConnectionState.Connected)
        {
            yield return new WaitForSeconds(0.01f);
        }
        Avatars.RequestDetails(avatarId);
        WriteLog(LogLevel.Debug, $"Sent out request for avatarId {avatarId}");
    }

    private void CacheAvatarToAvatarList(string avatarId)
    {
        bool found = AvatarHandler.IsAvatarCached(avatarId);
        if (!found)
        {
            WriteLog(LogLevel.Debug, $"No Avatar with id {avatarId} was cached; Requesting Cache.");
            CreateCoroutine(cacheEnum(avatarId));
        }
    }

    public static void WriteLog(LogLevel logLevel, object msg) => Log?.Invoke(logLevel, msg);
    public static void CreateCoroutine(IEnumerator enumerator) => Coroutine?.Invoke(enumerator);

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