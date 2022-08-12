#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SharpOSC;

namespace CVRParamLib;

internal class OSCManager
{
    private static UDPListener? listener;
    public static Action<OscMessage?> OnOscMessage = oscm => { };

    public static void Init()
    {
        listener = new UDPListener(Config.LoadedConfig.OSCServerPort,
            packet => OnOscMessage.Invoke((OscMessage?) packet));
        OnOscMessage += message =>
        {
            if (message != null)
            {
                if (message.Address.ToLower().Contains("/avatar/parameter"))
                    OSCMessageHandler.HandleAvatarParameter(message.Address, message.Arguments);
                if (message.Address.ToLower().Contains("/input"))
                {
                    try
                    {
                        // get the parameter name
                        string[] s = message.Address.Split('/');
                        string parameterName = s.Last();
                        OSCInputManagement.OSCInputs i = OSCInputManagement.OSCInputFromString(parameterName);
                        // get the parameter value as a float
                        float value = (float) Convert.ToDouble(message.Arguments[0]);
                        OSCInputManagement.HandleAvatarInput(i, value);
                    }
                    catch (Exception e)
                    {
                        CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Error,
                            $"Failed to Handle Input! Address: {message.Address} Exception: {e}");
                    }
                }
            }
        };
        CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Log, "Initialized OSC Server");
    }
    
    public static void SendMessage(string destination, params object[] data)
    {
        if (Config.LoadedConfig.EnableOSC)
        {
            OscMessage message = new OscMessage(destination, data);
            UDPSender sender = new UDPSender(Config.LoadedConfig.OSCSendingIP, Config.LoadedConfig.OSCSendingPort);
            sender.Send(message);
        }
    }
}

internal static class OSCMessageHandler
{
    public static void HandleAvatarParameter(string address, List<object> data)
    {
        try
        {
            // get the parameter name
            string parameterName = address.Split('/').Last();
            // get the parameter value as a float
            float value = (float) Convert.ToDouble(data[0]);
            // apply to cache
            ParameterManager.UpdateParameter(parameterName, value);
        }
        catch (Exception e)
        {
            CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Error,
                $"Failed to HandleAvatarParameter! Exception: {e}");
        }
    }

    public static void HandleParameterUpdate(FullAvatarParameter fullAvatarParameter)
    {
        object data = Convert.ChangeType(fullAvatarParameter.Value, fullAvatarParameter.ParameterType);
        OSCManager.SendMessage($"/avatar/parameters/{fullAvatarParameter.Name}", data);
    }

    public static void HandleAvatarChange(string avatarId)
    {
        CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Debug, $"Avatar changed to {avatarId}");
        string aid = avatarId;
        if (Config.LoadedConfig.UseVRChatIds)
            aid = $"avtr_{avatarId}";
        OSCManager.SendMessage("/avatar/change", aid);
    }
}

internal static class OSCInputManagement
{
    public static OSCInputs OSCInputFromString(string input) => (OSCInputs) Enum.Parse(typeof(OSCInputs), input);
    
    public enum OSCInputs
    {
        Vertical,
        Horizontal,
        LookHorizontal,
        UseAxisRight,
        GrabAxisRight,
        MoveHoldFB,
        SpinHoldCwCcw,
        SpinHoldUD,
        SpinHoldLR,
        MoveForward,
        MoveBackward,
        MoveLeft,
        MoveRight,
        LookLeft,
        LookRight,
        Jump,
        Run,
        ComfortLeft,
        ComfortRight,
        DropRight,
        UseRight,
        GrabRight,
        DropLeft,
        UseLeft,
        GrabLeft,
        PanicButton,
        QuickMenuToggleLeft,
        QuickMenuToggleRight,
        Voice
    }

    private static bool didTryInput;
    public static void HandleAvatarInput(OSCInputManagement.OSCInputs input, float value)
    {
        // TODO: Handle Input
        if (!didTryInput)
        {
            didTryInput = true;
            CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Warning, "Sorry, OSC Inputs are not quite done yet!");
        }
    }
}