using System;
using System.IO;
using System.Reflection;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;

namespace CVRParamLib;

/*
 * You may be asking...
 * "Why write your own Config?"
 * Here's some reasons,
 * 1. This mod is a cross-compatible mod between BepInEx and MelonLoader, and designed to be able to run more mod loaders
 * if needed; this mod is dynamic, as so should the config be too.
 * 2. MelonLoader's Config didn't work properly with how dynamic the Config Values were.
 * (Even reading 0 as UpperLeft, and not as a string...)
 * 3. This Config is for the library, not the mod(s).
 */

public class Config
{
    public static Config LoadedConfig = new();

    [TomlProperty("EnableOSC")]
    [TomlPrecedingComment("Enable the OSC Server")]
    public bool EnableOSC { get; set; } = true;

    [TomlProperty("OSCServerPort")]
    [TomlPrecedingComment("The Port for the OSC Server to listen to")]
    public int OSCServerPort { get; set; } = 9000;

    [TomlProperty("OSCSendingIP")]
    [TomlPrecedingComment("The IP to send OSC messages to")]
    public string OSCSendingIP { get; set; } = "127.0.0.1";

    [TomlProperty("OSCSendingPort")]
    [TomlPrecedingComment("The Port to send OSC Messages to")]
    public int OSCSendingPort { get; set; } = 9001;

    [TomlProperty("ShowDebug")]
    [TomlPrecedingComment("Show any Debug Logs")]
    public bool ShowDebug { get; set; } = false;

    [TomlProperty("ParameterFileTargetLocation")]
    [TomlPrecedingComment("\"both\" for VRChat and ChilloutVR, \"cvr\" for ChilloutVR, and \"vrc\" for VRChat OSC Directory")]
    public string ParameterFileTargetLocation { get; set; } = "both";

    [TomlProperty("UseVRChatIds")]
    [TomlPrecedingComment("Whether or not to append usr_ or _avtr at the beginning of a CVR GUID. Does not affect TargetUserIdFolder Override")]
    public bool UseVRChatIds { get; set; } = true;

    [TomlProperty("TargetUserIdFolder")]
    [TomlPrecedingComment("The target UserId folder to write the parameters file to. " +
                          "Set to \"all\" if you want to put the file in all folders, or set to \"cvr\" to use your ChilloutVR UserId.")]
    public string TargetUserIdFolder { get; set; } = "cvr";

    [TomlProperty("CacheAllLoadedAvatars")]
    [TomlPrecedingComment("Whether or not to cache all avatars that are loaded. Recommended false as to only cache avatars the LocalPlayer has used.")]
    public bool CacheAllLoadedAvatars { get; set; } = false;

    public static void Init(string location, Action OnFinish = null)
    {
        // Create the config
        if (File.Exists(location))
        {
            try
            {
                // Read the file
                string filetext = File.ReadAllText(location);
                LoadedConfig = TomletMain.To<Config>(filetext);
            }
            catch (Exception e)
            {
                CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Error,
                    $"Failed to load Config! Using Default Values Exception: {e}");
            }
        }
        CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Log, "Loaded Config");
        CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Debug, LoadedConfig);
        OnFinish?.Invoke();
    }

    public static void SaveConfig(string location)
    {
        TomlDocument document = TomletMain.DocumentFrom(typeof(Config), LoadedConfig);
        string filetext = document.SerializedValue;
        File.WriteAllText(location, filetext);
        CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Log, "Saved Config.");
    }

    public override string ToString()
    {
        string s = "Config {\n";
        foreach (PropertyInfo propertyInfo in GetType().GetProperties())
            s += $"{propertyInfo.Name} : {propertyInfo.GetValue(this)}\n";
        s += "}";
        return s;
    }
}