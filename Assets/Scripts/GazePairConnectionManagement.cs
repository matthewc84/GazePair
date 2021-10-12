using System;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Spawning;
using UnityEngine;

/// <summary>
///     There is no good way to keep track of connected clients in MLAPI, so this Class
///     created a Dictionary that is updated when OnCLientConnectedCallback fires after a client connection, and adds the client ID
///     Can be called by Host to query client NetworkVariables
/// </summary>
/// 
public class GazePairConnectionManagement : NetworkBehaviour
{
    public static GazePairConnectionManagement Instance = null;
    private Dictionary<ulong, bool> m_ClientsInLobby;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }

        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        m_ClientsInLobby = new Dictionary<ulong, bool>();

            if (NetworkManager.Singleton.IsHost)
                {
                m_ClientsInLobby.Add(NetworkManager.Singleton.LocalClientId, false);
                //Server will be notified when a client connects
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            
                }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            
            if (!m_ClientsInLobby.ContainsKey(clientId)) m_ClientsInLobby.Add(clientId, false);

        }
    }

    public Dictionary<ulong, bool> getClientsInLobby()
    {
        return m_ClientsInLobby;
    }


}
