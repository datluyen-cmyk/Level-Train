#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// One-shot reconnect for this project's interrupted local QA session.
[InitializeOnLoad]
public static class RotateRingsMcpReconnect {
    static RotateRingsMcpReconnect(){EditorApplication.delayCall+=Reconnect;}
    static async void Reconnect(){
        const string key="RotateRings.V3.Reconnect.20261106";
        if(SessionState.GetBool(key,false))return;
        SessionState.SetBool(key,true);
        try{
            bool connected=await MCPForUnity.Editor.Services.MCPServiceLocator.Bridge.StartAsync();
            Debug.Log("Rotate Rings source QA MCP reconnect: "+connected);
        }catch(System.Exception e){Debug.LogWarning("Rotate Rings source QA MCP reconnect: "+e.Message);}
    }
}
#endif
