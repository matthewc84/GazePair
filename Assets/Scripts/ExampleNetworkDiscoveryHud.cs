using System.Collections.Generic;
using System.Net;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using Object = UnityEngine.Object;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

[RequireComponent(typeof(ExampleNetworkDiscovery))]
[RequireComponent(typeof(NetworkManager))]
public class ExampleNetworkDiscoveryHud : MonoBehaviour
{
    public GameObject buttonPrefab;
    private GameObject buttonParent;
    public string serverName;

    [SerializeField, HideInInspector]
    ExampleNetworkDiscovery m_Discovery;


    Dictionary<IPAddress, DiscoveryResponseData> discoveredServers = new Dictionary<IPAddress, DiscoveryResponseData>();

    public Vector3 DrawOffset = new Vector3(10, 210, 0);

    void Awake()
    {
        m_Discovery = GetComponent<ExampleNetworkDiscovery>();
        //m_NetworkManager = GetComponent<NetworkManager>();


    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (m_Discovery == null) // This will only happen once because m_Discovery is a serialize field
        {
            m_Discovery = GetComponent<ExampleNetworkDiscovery>();
            UnityEventTools.AddPersistentListener(m_Discovery.OnServerFound, OnServerFound);
            Undo.RecordObjects(new Object[] { this, m_Discovery}, "Set NetworkDiscovery");
        }
    }
#endif

    void OnServerFound(IPEndPoint sender, DiscoveryResponseData response)
    {
        buttonParent = GameObject.Find("ButtonParent");
        GameObject myButton = Instantiate(buttonPrefab, buttonParent.transform.position, buttonParent.transform.rotation) as GameObject;
        myButton.transform.SetParent(buttonParent.transform);
        myButton.GetComponentInChildren<TextMeshPro>().text = "la di da";
        discoveredServers[sender.Address] = response;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(DrawOffset, new Vector2(200, 600)));

        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                ServerControlsGUI();
            }
        }
        else
        {
            ClientSearchGUI();
        }

        GUILayout.EndArea();
    }

    void ClientSearchGUI()
    {
        if (m_Discovery.IsRunning)
        {
            if (GUILayout.Button("Stop Client Discovery"))
            {
                m_Discovery.StopDiscovery();
                discoveredServers.Clear();
            }

            if (GUILayout.Button("Refresh List"))
            {
                discoveredServers.Clear();
                m_Discovery.ClientBroadcast(new DiscoveryBroadcastData());
            }

            GUILayout.Space(40);

            foreach (var discoveredServer in discoveredServers)
            {
                if (GUILayout.Button($"{discoveredServer.Value.ServerName}[{discoveredServer.Key.ToString()}]"))
                {
                    UNetTransport transport = (UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
                    transport.ConnectAddress = discoveredServer.Key.ToString();
                    transport.ConnectPort = discoveredServer.Value.Port;
                    NetworkManager.Singleton.StartClient();
                }
            }
        }
        else
        {
            if (GUILayout.Button("Discover Servers"))
            {
                m_Discovery.StartClient();
                m_Discovery.ClientBroadcast(new DiscoveryBroadcastData());
            }
        }
    }

    void ServerControlsGUI()
    {

            if (GUILayout.Button("Start Server Discovery"))
            {
                m_Discovery.StartServer();
            }
        }
    }

