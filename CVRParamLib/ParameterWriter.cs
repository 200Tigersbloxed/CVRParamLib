using System;
using System.Collections.Generic;
using System.IO;
using ABI.CCK.Scripts;
using Newtonsoft.Json;

namespace CVRParamLib;

public static class ParameterWriter
{
    private static List<AvatarSheet> waitingAvatarInfo = new();

    private static List<AvatarParameter> GetAvatarParameters()
    {
        if (CVRParameterInstance.Instance == null)
        {
            CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Error, "CVRParameterInstance cannot be null!");
            return null;
        }
        List<AvatarParameter> aps = new(ParameterManager.CurrentAvatarParameters);
        return aps;
    }

    private static void WriteFile(string location, string text)
    {
        File.WriteAllText(location, text);
        CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Debug, $"Wrote file {location}");
    }

    private static List<string> GetEssentialDirectory(string targets)
    {
        if (CVRParameterInstance.Instance == null)
            throw new Exception("CVRParameterInstance cannot be null!");
        List<string> targetDirectories = new();
        // bribi made me do this
        // shoutout to bribi btw
        string appdatalocallow = HelpfulTools.GetLocalLow();
        string vrcpath = Path.Combine(appdatalocallow, "VRChat", "VRChat", "OSC");
        string cvrpath = Path.Combine(appdatalocallow, "Alpha Blend Interactive", "ChilloutVR", "OSC");
        string userid = CVRParameterInstance.Instance.UserId;
        if (Config.LoadedConfig.UseVRChatIds)
            userid = $"usr_{CVRParameterInstance.Instance.UserId}";
        bool cantvrc = false;
        if (targets == "vrc" || targets == "both")
        {
            if (Directory.Exists(vrcpath))
            {
                switch (Config.LoadedConfig.TargetUserIdFolder)
                {
                    case "all":
                        foreach (string directory in Directory.GetDirectories(vrcpath))
                            targetDirectories.Add(Path.Combine(directory, "Avatars"));
                        break;
                    case "cvr":
                        targetDirectories.Add(Path.Combine(vrcpath, userid, "Avatars"));
                        break;
                    default:
                        targetDirectories.Add(Path.Combine(vrcpath, Config.LoadedConfig.TargetUserIdFolder, "Avatars"));
                        break;
                }
            }
            else
                cantvrc = true;
        }
        if(targets == "cvr" || targets == "both" || cantvrc)
        {
            if (!Directory.Exists(cvrpath))
                Directory.CreateDirectory(cvrpath);
            if (Directory.GetDirectories(cvrpath).Length <= 0)
                Directory.CreateDirectory(Path.Combine(cvrpath, userid));
            switch (Config.LoadedConfig.TargetUserIdFolder)
            {
                case "all":
                    foreach (string directory in Directory.GetDirectories(cvrpath))
                        targetDirectories.Add(Path.Combine(directory, "Avatars"));
                    break;
                case "cvr":
                    targetDirectories.Add(Path.Combine(cvrpath, userid, "Avatars"));
                    break;
                default:
                    targetDirectories.Add(Path.Combine(cvrpath, Config.LoadedConfig.TargetUserIdFolder, "Avatars"));
                    break;
            }
        }
        foreach (string targetDirectory in targetDirectories)
            if(!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);
        return targetDirectories;
    }

    private static void TryWriteToFile(AvatarSheet avatarSheet)
    {
        List<string> dirs = GetEssentialDirectory(Config.LoadedConfig.ParameterFileTargetLocation.ToLower());
        AvatarSheet newAvatarSheet = new AvatarSheet(avatarSheet);
        if (Config.LoadedConfig.UseVRChatIds)
            newAvatarSheet.id = $"avtr_{avatarSheet.id}";
        string json = JsonConvert.SerializeObject(newAvatarSheet, Formatting.Indented);
        string filename = $"{avatarSheet.id}.json";
        if (Config.LoadedConfig.UseVRChatIds)
            filename = $"avtr_{avatarSheet.id}.json";
        foreach (string dir in dirs)
            WriteFile(Path.Combine(dir, filename), json);
    }

    private static void FinalizeAvatarSheet(AvatarInfo avatarInfo, AvatarSheet avatarSheet)
    {
        avatarSheet.name = avatarInfo.AvatarName;
        try
        {
            TryWriteToFile(avatarSheet);
            CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Debug,
                $"Finalized AvatarSheet with Id {avatarSheet.id}");
            if(avatarSheet.id == AvatarHandler.CurrentAvatarId)
                OSCMessageHandler.HandleAvatarChange(AvatarHandler.CurrentAvatarId);
        }
        catch (Exception e)
        {
            CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Error,
                $"Failed to write AvatarSheet with Id {avatarSheet.id} to file! Exception: {e}");
        }
    }

    public static void OnAvatarInfoCached(AvatarInfo avatarInfo)
    {
        foreach (AvatarSheet avatarSheet in waitingAvatarInfo)
        {
            if(avatarSheet.id == avatarInfo.AvatarId)
                FinalizeAvatarSheet(avatarInfo, avatarSheet);
        }
    }

    public static void OnLocalAvatarChange()
    {
        AvatarInfo avatarInfo = AvatarHandler.GetAvatarInfoFromId(AvatarHandler.CurrentAvatarId);
        string id = AvatarHandler.CurrentAvatarId;
        List<AvatarParameter> parameters = GetAvatarParameters();
        AvatarSheet avatarSheet = new AvatarSheet
        {
            id = id,
            parameters = parameters
        };
        if (avatarInfo == null)
            waitingAvatarInfo.Add(avatarSheet);
        else
            FinalizeAvatarSheet(avatarInfo, avatarSheet);
    }
}

public record AvatarSheet
{
    public string id { get; set; }
    public string name { get; set; }
    public List<AvatarParameter> parameters { get; set; }

    public AvatarSheet(){}

    public AvatarSheet(AvatarSheet avatarSheet)
    {
        id = avatarSheet.id;
        name = avatarSheet.name;
        parameters = avatarSheet.parameters;
    }
}

public record AvatarParameter
{
    public string name { get; set; }
    public AvatarParameterInputOutput input { get; set; } = null;
    public AvatarParameterInputOutput output { get; set; } = null;
    
    public AvatarParameter(){}
    public AvatarParameter(CVRAdvancedSettingsEntry cvrAdvancedSettingsEntry)
    {
        name = cvrAdvancedSettingsEntry.name;
        Type type = typeof(float);
        switch (cvrAdvancedSettingsEntry.setting.usedType)
        {
            case CVRAdvancesAvatarSettingBase.ParameterType.GenerateFloat:
                type = typeof(float);
                break;
            case CVRAdvancesAvatarSettingBase.ParameterType.GenerateInt:
                type = typeof(int);
                break;
            case CVRAdvancesAvatarSettingBase.ParameterType.GenerateBool:
                type = typeof(bool);
                break;
        }
        input = new AvatarParameterInputOutput(name, type);
        output = new AvatarParameterInputOutput(name, type);
    }
}

public record AvatarParameterInputOutput
{
    public string address { get; set; }
    public string type { get; set; }

    public AvatarParameterInputOutput(){}
    public AvatarParameterInputOutput(string paramname, Type parameterType)
    {
        address = $"/avatar/parameters/{paramname}";
        if (parameterType == typeof(bool))
            type = "Bool";
        if (parameterType == typeof(int))
            type = "Int";
        if (parameterType == typeof(float))
            type = "Float";
        if (string.IsNullOrEmpty(type))
        {
            CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Warning,
                $"Parameter {paramname} had no target type ({parameterType})! Assuming float.");
            type = "Float";
        }
    }
}