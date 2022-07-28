#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SharpOSC;

namespace CVRParamLib;

public class OSCManager
{
    private static UDPListener listener = new UDPListener(9000, packet => OnOscMessage.Invoke((OscMessage?) packet));
    public static Action<OscMessage?> OnOscMessage = oscm => { };

    public static void Init()
    {
        OnOscMessage += message =>
        {
            if (message != null)
            {
                if (message.Address.ToLower().Contains("/avatar/parameter"))
                    OSCMessageHandler.HandleAvatarParameter(message.Address, message.Arguments);
            }
        };
        CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Debug, "Initialized OSC Server");
    }
}

public static class OSCMessageHandler
{
    public static void HandleAvatarParameter(string address, List<object> data)
    {
        try
        {
            // get the parameter name
            string[] s = address.Split('/');
            string parameterName = s.Last();
            // get the parameter value as a float
            float value = (float) Convert.ToDouble(data[0]);
            // apply to cache
            ParameterManager.UpdateParameter(parameterName, value);
        }
        catch (Exception e)
        {
            CVRParameterInstance.WriteLog(CVRParameterInstance.LogLevel.Error,
                "Failed to HandleAvatarParameter! Exception: " + e);
        }
    }
}