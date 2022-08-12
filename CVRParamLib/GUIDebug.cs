using System.Collections.Generic;
using UnityEngine;

namespace CVRParamLib;

internal class GUIDebug
{
    private static void DrawList(List<FullAvatarParameter> fullAvatarParameters)
    {
        for (int i = 0; i < fullAvatarParameters.Count; i++)
        {
            FullAvatarParameter fullAvatarParameter = fullAvatarParameters[i];
            GUI.Label(new Rect(1, 10 * i + 30, 500, 25),
                $"{fullAvatarParameter.Name}, {fullAvatarParameter.Value}, {fullAvatarParameter.ParameterType}");
        }
    }
    
    public static void OnGUI()
    {
        List<FullAvatarParameter> fullAvatarParameters = new(ParameterManager.CurrentAvatarParameters);
        int fapc = fullAvatarParameters.Count * 10;
        GUI.Box(new Rect(Vector2.one, new Vector2(500, fapc + 40)), "AvatarParameters");
        GUI.Label(new Rect(1, 20, 500, 25), "FullAvatarParameter(s):");
        DrawList(fullAvatarParameters);
    }
}