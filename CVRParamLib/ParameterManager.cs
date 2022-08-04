using System.Collections.Generic;
using System.Linq;
using ABI.CCK.Scripts;

namespace CVRParamLib;

public class ParameterManager
{
    public static readonly List<AvatarParameter> CurrentAvatarParameters = new();
    public static readonly Dictionary<string, float> Parameters = new();

    public static bool IsParameterRegistered(string name) => Parameters.ContainsKey(name);

    public static bool IsAnAvatarParameter(string name) =>
        CurrentAvatarParameters.Where(x => x.name == name).ToList().Count > 0;
    
    public static void UpdateParameter(string name, float value)
    {
        if (IsParameterRegistered(name))
        {
            Parameters[name] = value;
            return;
        }
        Parameters.Add(name, value);
    }

    public static void DeleteParameter(string name)
    {
        if (IsParameterRegistered(name))
            Parameters.Remove(name);
    }

    public static float? GetParameterValue(string name, Dictionary<string, float> parameters = null)
    {
        if (parameters == null)
            parameters = new(Parameters);
        if (!parameters.ContainsKey(name))
            return null;
        return parameters[name];
    }
    
    public static bool? AreDifferent(string name, float value)
    {
        if (!IsParameterRegistered(name))
            return null;
        // this will never be null/default
        float registeredValue = GetParameterValue(name) ?? value;
        // TODO: Make Better
        if (registeredValue != value)
            return true;
        return false;
    }

    public static void OnLocalAvatarChange(List<CVRAdvancedSettingsEntry> settingsEntries)
    {
        CurrentAvatarParameters.Clear();
        foreach (CVRAdvancedSettingsEntry cvrAdvancedSettingsEntry in settingsEntries)
            if(!IsAnAvatarParameter(cvrAdvancedSettingsEntry.name))
                CurrentAvatarParameters.Add(new(cvrAdvancedSettingsEntry));
    }
}