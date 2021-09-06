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


[RequireComponent(typeof(NetworkManager))]
public class ExampleNetworkDiscovery : NetworkDiscovery<DiscoveryBroadcastData, DiscoveryResponseData>
{

    [Serializable]
    public class ServerFoundEvent : UnityEvent<IPEndPoint, DiscoveryResponseData>
    {
    };

    GameObject buttonParent;
    GameObject buttonChild;

    public GameObject serverButtonPrefab;
    public GameObject clientButtonPrefab;


    NetworkManager m_NetworkManager;

    [SerializeField]
    [Tooltip("If true NetworkDiscovery will make the server visible and answer to client broadcasts as soon as MLAPI starts running as server.")]
    bool m_StartWithServer = true;

    public string ServerName = "EnterName";

    public ServerFoundEvent OnServerFound;
    //ExampleNetworkDiscovery m_Discovery;


    public void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        //m_Discovery = GetComponent<NetworkDiscovery>();

    }

    public void Update()
    {
        
    }

    public void serverButtonPressed()
    {
        if (!m_NetworkManager.IsHost && !m_NetworkManager.IsClient)
        {
            //When Button pressed, start server
            NetworkManager.Singleton.StartHost(null, null, null, null, null);
            StartServer();
            //Find Button Parent and its child in hierarchy
            buttonParent = GameObject.Find("ButtonParent");
            buttonChild = GameObject.Find("ButtonParent/StartHost");
            //create new button to replace the old
            var TMP = GameObject.Find("ButtonParent/StartHost");
            //If the child was found.
            if (TMP != null)
            {
                GameObject.Find("ButtonParent/StartHost").GetComponentInChildren<TextMeshPro>().SetText("End Client Discovery");
            }
            else Debug.Log("No text child attached");
        }
        else
        {
            NetworkManager.Singleton.StopServer();
            StopDiscovery();
            buttonParent = GameObject.Find("ButtonParent");
            buttonChild = GameObject.Find("ButtonParent/StartHost");
            var TMP = GameObject.Find("ButtonParent/StartHost");
            //If the child was found.
            if (TMP != null)
            {
                GameObject.Find("ButtonParent/StartHost").GetComponentInChildren<TextMeshPro>().SetText("Start Host");
            }
            else Debug.Log("No text child attached");
        }
    }


    public void clientButtonPressed()
    {
        if (!m_NetworkManager.IsServer)
        {
            //When Button pressed, start server
            NetworkManager.Singleton.StartClient();
            StartClient();
            //Find Button Parent and its child in hierarchy
            buttonParent = GameObject.Find("ButtonParent");
            buttonChild = GameObject.Find("ButtonParent/StartClient");
            //create new button to replace the old
            var TMP = GameObject.Find("ButtonParent/StartClient");
            //If the child was found.
            if (TMP != null)
            {
                GameObject.Find("ButtonParent/StartClient").GetComponentInChildren<TextMeshPro>().SetText("End Host Discovery");
            }
            else Debug.Log("No text child attached");
        }
        else
        {
            NetworkManager.Singleton.StopClient();
            StopDiscovery();
            buttonParent = GameObject.Find("ButtonParent");
            buttonChild = GameObject.Find("ButtonParent/StartClient");
            var TMP = GameObject.Find("ButtonParent/StartClient");
            //If the child was found.
            if (TMP != null)
            {
                GameObject.Find("ButtonParent/StartClient").GetComponentInChildren<TextMeshPro>().SetText("Start Client");
            }
            else Debug.Log("No text child attached");
        }

       // m_Discovery.ClientBroadcast(new DiscoveryBroadcastData());

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


}
