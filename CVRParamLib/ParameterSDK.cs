namespace CVRParamLib;

public static class ParameterSDK
{
    /// <summary>
    /// Adds, or updates, Parameter with value to cache to be updated on the main Avatar
    /// </summary>
    /// <param name="name">The name of the Avatar Parameter</param>
    /// <param name="value">The value for the Avatar Parameter</param>
    public static void SetAvatarParameter(string name, float value) => ParameterManager.UpdateParameter(name, value);

    /// <summary>
    /// Removes a Parameter from cache; will stop updating on the Avatar.
    /// NOTE: This does NOT delete the Avatar's Parameter!
    /// </summary>
    /// <param name="name"></param>
    public static void RemoveAvatarParameter(string name) => ParameterManager.DeleteParameter(name);
}