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
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
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

    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        //Your logic here
        bool approve = true;
        bool createPlayerObject = true;
        
        Vector3 vect = Vector3.zero;
        vect.x = BitConverter.ToSingle(connectionData, 0 * sizeof(float));
        vect.y = BitConverter.ToSingle(connectionData, 1 * sizeof(float));
        vect.z = BitConverter.ToSingle(connectionData, 2 * sizeof(float));
        //If approve is true, the connection gets added. If it's false. The client gets disconnected
        callback(createPlayerObject, null, approve, vect, null);
    }
}
