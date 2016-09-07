using UnityEngine;
using System.Collections;

public class Connect : MonoBehaviour {

    string connectToIP = "127.0.0.1";
    int connectPort = 25001;

    void OnGUI() {
        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            GUILayout.Label("Connection status: Disconnected");

            connectToIP = GUILayout.TextField(connectToIP, GUILayout.MinWidth(100));
            connectPort = int.Parse(GUILayout.TextField(connectPort.ToString()));

            GUILayout.BeginVertical();
            if (GUILayout.Button("Connect as Client"))
            {
                Network.useNat = false;
                Network.Connect(connectToIP, connectPort);
            }

            if (GUILayout.Button("Start Server"))
            {
                Network.InitializeServer(32, connectPort, false);
            }
            GUILayout.EndVertical();
        }
        else {
            if (Network.peerType == NetworkPeerType.Connecting) {
                GUILayout.Label("Connection status: Connecting");
            } else if (Network.peerType == NetworkPeerType.Client) {
                GUILayout.Label("Connection status: Client!");
                GUILayout.Label("Ping to server: " + Network.GetAveragePing(Network.connections[0]));
            } else if (Network.peerType == NetworkPeerType.Server) {
                GUILayout.Label("Connection status: Server!");
                GUILayout.Label("Connections: " + Network.connections.Length);
                if (Network.connections.Length >= 1)
                {
                    GUILayout.Label("Ping to first player: " + Network.GetAveragePing(Network.connections[0]));
                }
            }

            if (GUILayout.Button("Disconnect"))
            {
                Network.Disconnect(200);
            }
        }
    }

    //Server functions called by Unity
    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player connected from: " + player.ipAddress + ":" + player.port);
    }

    void OnServerInitialized()
    {
        Debug.Log("Server initialized and ready");
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Player disconnected from: " + player.ipAddress + ":" + player.port);
    }


    // OTHERS:
    // To have a full overview of all network functions called by unity
    // the next four have been added here too, but they can be ignored for now

    void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        Debug.Log("Could not connect to master server: " + info);
    }

    //void OnNetworkInstantiate(NetworkConnectionInfo info)
    //{
    //    Debug.Log("New object instantiated by " + info.ToString());
    //}

}
