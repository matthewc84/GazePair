using System;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class LobbyControl : NetworkBehaviour
{
    [HideInInspector]
    public static bool isHosting;// = true;

    [SerializeField]
    private string m_InGameSceneName = "SampleScene";

    // Minimum player count required to transition to next level
    [SerializeField]
    private int m_MinimumPlayerCount = 2;

    public Text LobbyText;
    private bool m_AllPlayersInLobby;

    private Dictionary<ulong, bool> m_ClientsInLobby;
    private string m_UserLobbyStatusText;


    public static LobbyControl Instance;

    /// <summary>
    ///     Awake
    ///     This is one way to kick off a multiplayer session
    /// </summary>
    /// 
    private void Start()
    {
        Debug.Log("LobbyControl created");

        m_ClientsInLobby = new Dictionary<ulong, bool>();

        //We added this information to tell us if we are going to host a game or join an the game session
        if (isHosting)
        {
            NetworkManager.Singleton.StartHost(); //Spin up the host
            //isHosting = false;
            Debug.Log("Started Server");
        }
        else
            NetworkManager.Singleton.StartClient(); //Spin up the client

        if (NetworkManager.Singleton.IsListening)
        {
            //Always add ourselves to the list at first
            m_ClientsInLobby.Add(NetworkManager.Singleton.LocalClientId, false);

            //If we are hosting, then handle the server side for detecting when clients have connected
            //and when their lobby scenes are finished loading.
            if (IsServer)
            {
                m_AllPlayersInLobby = false;

                //Server will be notified when a client connects
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
                
            }

            //Update our lobby
            GenerateUserStatsForLobby();
        }

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(Instance.gameObject);
                Instance = this;
            }
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnUpdate()
    {
        //StatusLabels();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        if (LobbyText != null) LobbyText.text = m_UserLobbyStatusText;
        StatusLabels();
        GUILayout.EndArea();

    }


    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    /// <summary>
    ///     GenerateUserStatsForLobby
    ///     Psuedo code for setting player state
    ///     Just updating a text field, this could use a lot of "refactoring"  :)
    /// </summary>
    private void GenerateUserStatsForLobby()
    {
        m_UserLobbyStatusText = string.Empty;
        foreach (var clientLobbyStatus in m_ClientsInLobby)
        {
            m_UserLobbyStatusText += "Player_" + clientLobbyStatus.Key + "          ";
            if (clientLobbyStatus.Value)
                m_UserLobbyStatusText += "(Ready)\n";
            else
                m_UserLobbyStatusText += "(Not Ready)\n";
        }
    }

    /// <summary>
    ///     UpdateAndCheckPlayersInLobby
    ///     Checks to see if we have at least 2 or more people to start
    /// </summary>
    private void UpdateAndCheckPlayersInLobby()
    {
        m_AllPlayersInLobby = m_ClientsInLobby.Count >= m_MinimumPlayerCount;

        foreach (var clientLobbyStatus in m_ClientsInLobby)
        {
            SendClientReadyStatusUpdatesClientRpc(clientLobbyStatus.Key, clientLobbyStatus.Value);
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientLobbyStatus.Key))

                //If some clients are still loading into the lobby scene then this is false
                m_AllPlayersInLobby = false;
        }

        CheckForAllPlayersReady();
    }

    /// <summary>
    ///     ClientLoadedScene
    ///     Invoked when a client has loaded this scene
    /// </summary>
    /// <param name="clientId"></param>
    private void ClientLoadedScene(ulong clientId)
    {
        if (IsServer)
        {
            if (!m_ClientsInLobby.ContainsKey(clientId))
            {
                m_ClientsInLobby.Add(clientId, false);
                GenerateUserStatsForLobby();
            }

            UpdateAndCheckPlayersInLobby();
        }
    }

    /// <summary>
    ///     OnClientConnectedCallback
    ///     Since we are entering a lobby and MLAPI NetowrkingManager is spawning the player,
    ///     the server can be configured to only listen for connected clients at this stage.
    /// </summary>
    /// <param name="clientId">client that connected</param>
    private void OnClientConnectedCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (!m_ClientsInLobby.ContainsKey(clientId)) m_ClientsInLobby.Add(clientId, false);
            GenerateUserStatsForLobby();

            UpdateAndCheckPlayersInLobby();
        }
    }

    /// <summary>
    ///     SendClientReadyStatusUpdatesClientRpc
    ///     Sent from the server to the client when a player's status is updated.
    ///     This also populates the connected clients' (excluding host) player state in the lobby
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="isReady"></param>
    [ClientRpc]
    private void SendClientReadyStatusUpdatesClientRpc(ulong clientId, bool isReady)
    {
        if (!IsServer)
        {
            if (!m_ClientsInLobby.ContainsKey(clientId))
                m_ClientsInLobby.Add(clientId, isReady);
            else
                m_ClientsInLobby[clientId] = isReady;
            GenerateUserStatsForLobby();
        }
    }

    /// <summary>
    ///     CheckForAllPlayersReady
    ///     Checks to see if all players are ready, and if so launches the game
    /// </summary>
    private void CheckForAllPlayersReady()
    {
        if (m_AllPlayersInLobby)
        {
            var allPlayersAreReady = true;
            foreach (var clientLobbyStatus in m_ClientsInLobby)
                if (!clientLobbyStatus.Value)

                    //If some clients are still loading into the lobby scene then this is false
                    allPlayersAreReady = false;

            //Only if all players are ready
            if (allPlayersAreReady)
            {
                //Remove our client connected callback
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;

            }
        }
    }

    /// <summary>
    ///     PlayerIsReady
    ///     Tied to the Ready button in the InvadersLobby scene
    /// </summary>
    public void PlayerIsReady()
    {
        if (IsServer)
        {
            m_ClientsInLobby[NetworkManager.Singleton.ServerClientId] = true;
            UpdateAndCheckPlayersInLobby();
        }
        else
        {
            m_ClientsInLobby[NetworkManager.Singleton.LocalClientId] = true;
            OnClientIsReadyServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        GenerateUserStatsForLobby();
    }

    /// <summary>
    ///     OnClientIsReadyServerRpc
    ///     Sent to the server when the player clicks the ready button
    /// </summary>
    /// <param name="clientid">clientId that is ready</param>
    [ServerRpc(RequireOwnership = false)]
    private void OnClientIsReadyServerRpc(ulong clientid)
    {
        if (m_ClientsInLobby.ContainsKey(clientid))
        {
            m_ClientsInLobby[clientid] = true;
            UpdateAndCheckPlayersInLobby();
            GenerateUserStatsForLobby();
        }
    }
}