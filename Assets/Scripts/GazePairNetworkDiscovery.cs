using TMPro;
using System;
using System.Net;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using System.Collections;


public class GazePairNetworkDiscovery : NetworkDiscovery<DiscoveryBroadcastData, DiscoveryResponseData>
{

    [Serializable]
    public class ServerFoundEvent : UnityEvent<IPEndPoint, DiscoveryResponseData>
    {
    };

    [SerializeField]
    [Tooltip("If true NetworkDiscovery will make the server visible and answer to client broadcasts as soon as MLAPI starts running as server.")]
    bool m_StartWithServer = true;

    public string ServerName = "EnterName";

    public ServerFoundEvent OnServerFound;
    public static GazePairNetworkDiscovery Instance = null;


    public void Awake()
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
    }

    public void Update()
    {

    }


    protected override bool ProcessBroadcast(IPEndPoint sender, DiscoveryBroadcastData broadCast, out DiscoveryResponseData response)
    {
        response = new DiscoveryResponseData()
        {
            ServerName = ServerName,
            Port = (ushort)((UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport).ConnectPort,
        };
        return true;
    }

    protected override void ResponseReceived(IPEndPoint sender, DiscoveryResponseData response)
    {
        OnServerFound.Invoke(sender, response);
    }

    public void connectToServer(int index)
    {
        UNetTransport transport = (UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        //transport.ConnectAddress = discoveredServers.Key.ToString();
        // transport.ConnectPort = discoveredServer.Value.Port;
        // m_NetworkManager.StartClient();
    }

}
