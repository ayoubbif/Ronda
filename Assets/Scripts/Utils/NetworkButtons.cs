using Unity.Netcode;
using UnityEngine;

public class NetworkButtons : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 200, 200));
        
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        }
        
        GUILayout.EndArea();
    }
}
