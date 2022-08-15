#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using ABI_RC.Core;
using UnityEngine;

namespace CVRParamLib;

public class ParameterManager
{
    public static readonly List<FullAvatarParameter> CurrentAvatarParameters = new();
    public static readonly Dictionary<string, float> Parameters = new();

    public static bool IsParameterRegistered(string name) => Parameters.ContainsKey(name);
    
    private static bool IsAnAvatarParameter(string name, AnimatorControllerParameterType parameterType) =>
        CurrentAvatarParameters
            .Where(x => x.Name == name && x.ParameterType == FullAvatarParameter.CastType(parameterType)).ToList()
            .Count > 0;
    
    internal static void UpdateParameter(string name, float value)
    {
        if (IsParameterRegistered(name))
        {
            Parameters[name] = value;
            return;
        }
        Parameters.Add(name, value);
    }

    internal static void DeleteParameter(string name)
    {
        if (IsParameterRegistered(name))
            Parameters.Remove(name);
    }

    internal static float? GetParameterValue(string name, Dictionary<string, float>? parameters = null)
    {
        if (parameters == null)
            parameters = new(Parameters);
        if (!parameters.ContainsKey(name))
            return null;
        return parameters[name];
    }

    private static bool DoesAnimatorControllerParameterReplicate(string parameterName)
    {
        if (Config.LoadedConfig.AddAllAnimatorParameters)
            return true;
        if (parameterName == String.Empty)
            return true;
        if (parameterName[0] != '#')
            return true;
        return false;
    }

    private static readonly List<FullAvatarParameter> previousAvatarParameters = new();

    internal static void OnLocalAvatarChange(CVRAnimatorManager animatorManager)
    {
        previousAvatarParameters.Clear();
        CurrentAvatarParameters.Clear();
        foreach (AnimatorControllerParameter animatorControllerParameter in animatorManager.animator.parameters)
        {
            if (!IsAnAvatarParameter(animatorControllerParameter.name, animatorControllerParameter.type) &&
                DoesAnimatorControllerParameterReplicate(animatorControllerParameter.name) &&
                animatorControllerParameter.type != AnimatorControllerParameterType.Trigger)
            {
                previousAvatarParameters.Add(new (animatorManager, animatorControllerParameter));
                CurrentAvatarParameters.Add(new (animatorManager, animatorControllerParameter));
            }
        }
    }

    internal static void Update()
    {
        foreach (FullAvatarParameter currentAvatarParameter in CurrentAvatarParameters)
            currentAvatarParameter.UpdateParameterValue();
        try
        {
            // CurrentAvatarParameters should previousAvatarParameters should always be the same
            for (int x = 0; x < CurrentAvatarParameters.Count; x++)
            {
                FullAvatarParameter currentFullAvatarParameter = CurrentAvatarParameters[x];
                FullAvatarParameter previousFullAvatarParameter = previousAvatarParameters[x];
                if (Math.Abs(currentFullAvatarParameter.Value - previousFullAvatarParameter.Value) > 0.01f)
                {
                    OSCMessageHandler.HandleParameterUpdate(currentFullAvatarParameter);
                    previousFullAvatarParameter.UpdateParameterValue();
                }
            }
        }
        catch (Exception e)
        {
            CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Error,
                $"Failed to loop through avatar parameters! {e}");
        }
    }
}

public record FullAvatarParameter
{
    public string Name { get; set; } = String.Empty;
    public float Value { get; private set; } = 0.00f;
    public Type ParameterType { get; set; } = typeof(object);
    public bool IsCachedOverOSC { get; set; }
    
    private CVRAnimatorManager? _animatorManager = CVRParameterInstance.Instance?.PlayerSetup.animatorManager;
    
    public FullAvatarParameter(){}

    public FullAvatarParameter(AvatarParameter avatarParameter, bool useOutput = false)
    {
        Name = avatarParameter.name;
        Value = CVRParameterInstance.Instance?.PlayerSetup.GetAnimatorParam(Name) ?? 0.00f;
        if (useOutput)
            switch (avatarParameter.output.type)
            {
                case "Float":
                    ParameterType = typeof(float);
                    break;
                case "Int":
                    ParameterType = typeof(int);
                    break;
                case "Bool":
                    ParameterType = typeof(bool);
                    break;
            }
        else
            switch (avatarParameter.input.type)
            {
                case "Float":
                    ParameterType = typeof(float);
                    break;
                case "Int":
                    ParameterType = typeof(int);
                    break;
                case "Bool":
                    ParameterType = typeof(bool);
                    break;
            }
        IsCachedOverOSC = ParameterManager.IsParameterRegistered(Name);
    }

    public FullAvatarParameter(CVRAnimatorManager animator, AnimatorControllerParameter animatorControllerParameter)
    {
        _animatorManager = animator;
        Name = animatorControllerParameter.name;
        switch (animatorControllerParameter.type)
        {
            case AnimatorControllerParameterType.Float:
                Value = animator.GetAnimatorParameterFloat(Name) ?? 0.00f;
                ParameterType = typeof(float);
                break;
            case AnimatorControllerParameterType.Int:
                Value = animator.GetAnimatorParameterInt(Name) ?? 0;
                ParameterType = typeof(int);
                break;
            case AnimatorControllerParameterType.Bool:
                bool val = animator.GetAnimatorParameterBool(Name) ?? false;
                if (val)
                    Value = 1.00f;
                else
                    Value = 0.00f;
                ParameterType = typeof(bool);
                break;
            default:
                ParameterType = typeof(object);
                break;
        }
        IsCachedOverOSC = ParameterManager.IsParameterRegistered(Name);
    }

    public void UpdateParameterValue()
    {
        if (CVRParameterInstance.Instance == null)
            return;
        if (ParameterType == typeof(float))
            Value = _animatorManager?.GetAnimatorParameterFloat(Name) ?? 0.00f;
        else if(ParameterType == typeof(int))
            Value = _animatorManager?.GetAnimatorParameterInt(Name) ?? 0;
        else if (ParameterType == typeof(bool))
        {
            bool val = _animatorManager?.GetAnimatorParameterBool(Name) ?? false;
            if (val)
                Value = 1.00f;
            else
                Value = 0.00f;
        }
        IsCachedOverOSC = ParameterManager.IsParameterRegistered(Name);
    }
    
    public static Type CastType(AnimatorControllerParameterType type)
    {
        switch (type)
        {
            case AnimatorControllerParameterType.Float:
                return typeof(float);
            case AnimatorControllerParameterType.Int:
                return typeof(int);
            case AnimatorControllerParameterType.Bool:
                return typeof(bool);
            case AnimatorControllerParameterType.Trigger:
                return typeof(object);
        }
        return typeof(float);
    }
}