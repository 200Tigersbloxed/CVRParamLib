using System;
using System.Collections.Generic;
using ABI.CCK.Components;

namespace CVRParamLib;

public static class ParameterSDK
{
    /// <summary>
    /// Invoked whenever the LocalPLayer changes their avatar
    /// </summary>
    public static Action<CVRAvatar, AvatarInfo, List<FullAvatarParameter>> OnLocalAvatarChanged =
        (cvrAvatar, avatarInfo, avatarParameters) => { };

    /// <summary>
    /// Adds, or updates, Parameter with value to cache to be updated on the main Avatar
    /// </summary>
    /// <param name="name">The name of the Avatar Parameter</param>
    /// <param name="value">The value for the Avatar Parameter</param>
    public static void SetAvatarParameter(string name, float value) => ParameterManager.UpdateParameter(name, value);

    /// <summary>
    /// Returns the value for the Animator Parameter.
    /// </summary>
    /// <param name="name">The name of the Parameter</param>
    /// <returns>The Parameter Value. Null only if CVRParameterInstance is null.</returns>
    public static float? GetAvatarParameterValue(string name) =>
        CVRParameterInstance.Instance?.PlayerSetup.GetAnimatorParam(name);

    /// <summary>
    /// Returns the value for the cached Parameter
    /// </summary>
    /// <param name="name">The name of the Parameter</param>
    /// <returns>The Parameter Value. Null if the parameter is not cached</returns>
    public static float? GetCachedAvatarParameterValue(string name) => ParameterManager.GetParameterValue(name);

    /// <summary>
    /// Removes a Parameter from cache; will stop updating on the Avatar.
    /// NOTE: This does NOT delete the Avatar's Parameter!
    /// </summary>
    /// <param name="name">The name of the Parameter</param>
    public static void RemoveCachedAvatarParameter(string name) => ParameterManager.DeleteParameter(name);

    /// <summary>
    /// Gets a List of the Current Avatar's Parameters
    /// </summary>
    public static List<FullAvatarParameter> GetAvatarParameters => new(ParameterManager.CurrentAvatarParameters);
}