using System.Collections;
using System.Collections.Generic;

using MLAPI;
using UnityEngine;

namespace HelloWorld

{
    public class SessionManager : MonoBehaviour
    {
        void OnAwake()
        {

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.StartServer();
            }
            else
            {

                NetworkManager.Singleton.StartClient();
            }

        }

        void OnUpdate()
        {
            SubmitNewPosition();
            StatusLabels();
        }

        static void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        static void SubmitNewPosition()
        {
            if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
                    out var networkedClient))
                {
                    var player = networkedClient.PlayerObject.GetComponent<HelloWorldPlayer>();
                    if (player)
                    {
                        player.Move();
                    }
                }
            }
        }
    }
}