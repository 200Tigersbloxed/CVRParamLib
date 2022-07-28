using System;
using System.Collections.Generic;

namespace CVRParamLib;

public class ParameterManager
{
    public static readonly Dictionary<string, float> Parameters = new();

    public static bool IsParameterRegistered(string name) => Parameters.ContainsKey(name);
    
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

    public static float? GetParameterValue(string name)
    {
        if (!Parameters.ContainsKey(name))
            return null;
        return Parameters[name];
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
}