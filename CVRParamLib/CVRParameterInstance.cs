#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABI.CCK.Components;
using ABI_RC.Core;
using ABI_RC.Core.Networking.API;
using ABI_RC.Core.Networking.API.Responses;
using ABI_RC.Core.Networking.IO.UserGeneratedContent;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
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

    internal void Update()
    {
        Dictionary<string, float> p = new(ParameterManager.Parameters);
        foreach (string parametersKey in p.Keys)
        {
            float paramValue = ParameterManager.GetParameterValue(parametersKey, p) ?? default;
            ReplicatedModInfo._instance?.UpdateParameter(parametersKey, paramValue);
        }
        AvatarHandler.Update();
        if (PlayerSetup._avatar != null && _avatarGO != PlayerSetup._avatar)
        {
            _avatarGO = PlayerSetup._avatar;
            avatar = _avatarGO.GetComponent<CVRAvatar>();
            ParameterManager.OnLocalAvatarChange(PlayerSetup.animatorManager);
            ParameterWriter.OnLocalAvatarChange();
        }
        ParameterManager.Update();
    }

    internal static void PrepareLog(Action<LogLevel, object> _log) => Log = _log;
    internal static void PrepareCoroutine(Action<IEnumerator> _coroutine) => Coroutine = _coroutine;

    internal void HarmonyAvatarChange(string avatarId)
    {
        avatar = null;
        AvatarHandler.OnLocalAvatarChanged(avatarId);
        CacheAvatarToAvatarList(avatarId);
    }

    private IEnumerator cacheEnum(string avatarId)
    {
        // ViewManager.cs Task RequestAvatarDetailsPageTask(string avatarID)
        Task t = Task.Run(async () =>
        {
            AvatarDetails_t avatarDetailsT = new AvatarDetails_t();
            BaseResponse<AvatarDetailsResponse> baseResponse = await ApiConnection.MakeRequest<AvatarDetailsResponse>(
                ApiConnection.ApiOperation.AvatarDetail, new
                {
                    avatarID = avatarId
                });
            if (baseResponse == null)
            {
                WriteLog(LogLevel.Warning, $"Avatar with ID {avatarId} had a null BaseRepsonse");
                return;
            }
            avatarDetailsT.AvatarId = baseResponse.Data.Id;
            avatarDetailsT.AvatarName = baseResponse.Data.Name;
            avatarDetailsT.AvatarDesc = baseResponse.Data.Description;
            avatarDetailsT.AvatarImageUrl = baseResponse.Data.ImageUrl;
            avatarDetailsT.AuthorId = baseResponse.Data.User.Id;
            avatarDetailsT.AuthorName = baseResponse.Data.User.Name;
            avatarDetailsT.AuthorImageUrl = baseResponse.Data.ImageUrl;
            avatarDetailsT.Tags = string.Join(",", baseResponse.Data.Tags);
            avatarDetailsT.FilterTags = string.Join(",", baseResponse.Data.Categories);
            avatarDetailsT.UploadedAt = baseResponse.Data.UploadedAt.ToString("yyyy-MM-dd");
            avatarDetailsT.UpdatedAt = baseResponse.Data.UpdatedAt.ToString("yyyy-MM-dd");
            avatarDetailsT.FileSize = CVRTools.HumanReadableFilesize(baseResponse.Data.FileSize);
            avatarDetailsT.IsSharedWithMe = baseResponse.Data.SwitchPermitted;
            avatarDetailsT.IsMine = baseResponse.Data.User.Id == MetaPort.Instance.ownerId;
            avatarDetailsT.IsPublic = baseResponse.Data.IsPublished;
            AvatarHandler.CacheAvatar(avatarDetailsT);
        });
        WriteLog(LogLevel.Debug, $"Sent out request for avatarId {avatarId}");
        while (!t.IsCompleted)
            yield return new WaitForSeconds(0.01f);
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
    internal static void CreateCoroutine(IEnumerator enumerator) => Coroutine?.Invoke(enumerator);

    internal void UpdatePlayerSetup(PlayerSetup _playerSetup)
    {
        PlayerSetup = _playerSetup;
        WriteLog(LogLevel.Debug, "PlayerSetup was updated");
    }

    internal void UpdateParameter(string name, float value) => PlayerSetup.changeAnimatorParam(name, value);

    public enum LogLevel
    {
        Debug,
        Log,
        Warning,
        Error
    }
}