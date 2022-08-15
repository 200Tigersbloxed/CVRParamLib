using System;
using System.Collections.Generic;
using System.Linq;
using ABI_RC.Core.Networking.API.Responses;
using ABI_RC.Core.Networking.IO.UserGeneratedContent;

namespace CVRParamLib;

public static class AvatarHandler
{
    private static readonly List<AvatarInfo> CachedAvatars = new();
    public static string CurrentAvatarId { get; private set; } = String.Empty;

    private static bool IsAvatarCached(Avatar_t avatar) =>
        CachedAvatars.Where(x => x.AvatarId == avatar.AvatarId).ToList().Count > 0;
    
    private static bool IsAvatarCached(AvatarDetails_t avatar) =>
        CachedAvatars.Where(x => x.AvatarId == avatar.AvatarId).ToList().Count > 0;
    
    private static bool IsAvatarCached(AvatarDetailsResponse avatar) =>
        CachedAvatars.Where(x => x.AvatarId == avatar.Id).ToList().Count > 0;
    
    public static bool IsAvatarCached(string avatarId) =>
        CachedAvatars.Where(x => x.AvatarId == avatarId).ToList().Count > 0;

    public static bool IsAvatarOwnedByLocalPlayer(AvatarInfo avatarInfo) => avatarInfo.AvatarId == CurrentAvatarId;

    public static AvatarInfo GetAvatarInfoFromId(string avatarId)
    {
        foreach (AvatarInfo cachedAvatar in CachedAvatars)
        {
            if (cachedAvatar.AvatarId == avatarId)
                return cachedAvatar;
        }
        return null;
    }
    
    private static void CacheAvatar(Avatar_t avatar)
    {
        if (!IsAvatarCached(avatar))
        {
            AvatarInfo avatarInfo = new(avatar);
            if (IsAvatarOwnedByLocalPlayer(avatarInfo) || Config.LoadedConfig.CacheAllLoadedAvatars)
            {
                CachedAvatars.Add(avatarInfo);
                CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Debug,
                    $"Cached avatar with Id {avatar.AvatarId}");
                ParameterWriter.OnAvatarInfoCached(avatarInfo);
            }
        }
    }
    
    internal static void CacheAvatar(AvatarDetails_t avatar)
    {
        if (!IsAvatarCached(avatar))
        {
            AvatarInfo avatarInfo = new(avatar);
            if (IsAvatarOwnedByLocalPlayer(avatarInfo) || Config.LoadedConfig.CacheAllLoadedAvatars)
            {
                CachedAvatars.Add(avatarInfo);
                CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Debug,
                    $"Cached avatar with Id {avatar.AvatarId}");
                ParameterWriter.OnAvatarInfoCached(avatarInfo);
            }
        }
    }

    public static void Update()
    {
        List<Avatar_t> al = new(Avatars.List);
        foreach (Avatar_t avatarT in al)
        {
            if(!IsAvatarCached(avatarT))
                CacheAvatar(avatarT);
        }
    }

    public static void OnLocalAvatarChanged(string avatarId) => CurrentAvatarId = avatarId;
}

public record AvatarInfo
{
    public string AvatarId { get; }
    public string AvatarName { get; }
    public string AvatarDesc { get; }
    public string AvatarImageUrl { get; }
    public int AvatarVersion { get; }
    public string AvatarAuthorId { get; }
    public string FilterTags { get; }

    public AvatarInfo(Avatar_t avatar)
    {
        AvatarId = avatar.AvatarId;
        AvatarName = avatar.AvatarName;
        AvatarDesc = avatar.AvatarDesc;
        AvatarImageUrl = avatar.AvatarImageUrl;
        AvatarVersion = avatar.AvatarVersion;
        AvatarAuthorId = avatar.AvatarAuthorId;
        FilterTags = avatar.FilterTags;
    }

    public AvatarInfo(AvatarDetails_t avatarDetails)
    {
        AvatarId = avatarDetails.AvatarId;
        AvatarName = avatarDetails.AvatarName;
        AvatarDesc = avatarDetails.AvatarDesc;
        AvatarImageUrl = avatarDetails.AvatarImageUrl;
        AvatarVersion = 0;
        AvatarAuthorId = avatarDetails.AuthorId;
        FilterTags = avatarDetails.FilterTags;
    }

    public AvatarInfo(AvatarDetailsResponse avatarDetailsResponse)
    {
        AvatarId = avatarDetailsResponse.Id;
        AvatarName = avatarDetailsResponse.Name;
        AvatarDesc = avatarDetailsResponse.Description;
        AvatarImageUrl = avatarDetailsResponse.ImageUrl;
        AvatarVersion = 0;
        AvatarAuthorId = avatarDetailsResponse.User.Id;
        FilterTags = string.Join(",", avatarDetailsResponse.Categories);
    }
}