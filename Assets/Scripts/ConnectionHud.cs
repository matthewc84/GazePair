using System.Collections.Generic;
using System.Net;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using Object = UnityEngine.Object;
using TMPro;
using UnityEngine.SceneManagement;
using MLAPI.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

/// <summary>
///     Handles the creation of the Host or connection of the client, and changes schene when host is ready.
/// </summary>
/// 
public class ConnectionHud : MonoBehaviour
{

    Dictionary<IPAddress, DiscoveryResponseData> discoveredServers = new Dictionary<IPAddress, DiscoveryResponseData>();

    public GameObject SharedSecretCapturePrefab;
    public GameObject ConnectionHudPrefab;
    public GameObject startClientButton;
    public GameObject startHostButton;
    public GameObject directionSceneButton;
    public GameObject gridSceneButton;
    private SceneSwitchProgress m_SceneProgress;

    void Start()
    {

    }

    void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            var gazePairHudComponent = GameObject.Find("ConnectionHud(Clone)");
            gazePairHudComponent.GetComponent<TextMeshPro>().SetText("The Number of Pairing Partners in the Lobby is: " + ((int)NetworkManager.Singleton.ConnectedClients.Keys.Count-1).ToString());
        }
    }
    public void OnServerFound(IPEndPoint sender, DiscoveryResponseData response)
    {
        Debug.Log("Found Server");
        discoveredServers[sender.Address] = response;
    }

    void ClientSearchGUI()
    {
        if (GazePairNetworkDiscovery.Instance.IsRunning)
        {
            if (GUILayout.Button("Stop Client Discovery"))
            {
                GazePairNetworkDiscovery.Instance.StopDiscovery();
                discoveredServers.Clear();
            }

            if (GUILayout.Button("Refresh List"))
            {
                discoveredServers.Clear();
                GazePairNetworkDiscovery.Instance.ClientBroadcast(new DiscoveryBroadcastData());
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
                GazePairNetworkDiscovery.Instance.StartClient();
                GazePairNetworkDiscovery.Instance.ClientBroadcast(new DiscoveryBroadcastData());
            }
        }
    }

    public void serverButtonPressed()
    {
        //Camera maincam = GameObject.Find("Main Camera").GetComponent<Camera>();

        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient)
        {
            //When Button pressed, start host
            NetworkManager.Singleton.StartHost(new Vector3(0,0,0), null, null, null, null);
            //GazePairNetworkDiscovery.Instance.StartServer();
            startHostButton.GetComponentInChildren<TextMeshPro>().SetText("End Client Discovery");
            Instantiate(ConnectionHudPrefab);
            directionSceneButton.SetActive(true);
            gridSceneButton.SetActive(true);
            startClientButton.SetActive(false);
            
            
        }
        else
        {
            NetworkManager.Singleton.StopServer();
            //GazePairNetworkDiscovery.Instance.StopDiscovery();
            startHostButton.GetComponentInChildren<TextMeshPro>().SetText("Start Host");
            directionSceneButton.SetActive(false);
            gridSceneButton.SetActive(false);
            startClientButton.SetActive(true);
            var gazePairHudComponent = GameObject.Find("ConnectionHud(Clone)");
            Destroy(gazePairHudComponent);
        }
    }


    public void clientButtonPressed()
    {
        if (!NetworkManager.Singleton.IsClient)
        {

            NetworkManager.Singleton.StartClient();
            startClientButton.GetComponentInChildren<TextMeshPro>().SetText("End Host Discovery");
            startHostButton.SetActive(false);
        }
        else
        {
            NetworkManager.Singleton.StopClient();
            //GazePairNetworkDiscovery.Instance.StopDiscovery();
            startClientButton.GetComponentInChildren<TextMeshPro>().SetText("Start Client");
            startHostButton.SetActive(true);

        }

        //GazePairNetworkDiscovery.Instance.ClientBroadcast(new DiscoveryBroadcastData());

    }

    public void pairScene()
    {
        //var sharedSecretInstance = Instantiate(SharedSecretCapturePrefab);
        if (NetworkManager.Singleton.IsHost)
        {
            //var sharedSecretInstance = Instantiate(SharedSecretCapturePrefab);
            //sharedSecretInstance.GetComponent<NetworkObject>().Spawn();
        }
            
        if (NetworkManager.Singleton.IsListening)
        {
            m_SceneProgress = NetworkSceneManager.SwitchScene("PairScene");
        }
        else
        {
            SceneManager.LoadSceneAsync("PairScene");
        }
    }

    public void gridScene()
    {
        //var sharedSecretInstance = Instantiate(SharedSecretCapturePrefab);
        if (NetworkManager.Singleton.IsHost)
        {
            //var sharedSecretInstance = Instantiate(SharedSecretCapturePrefab);
            //sharedSecretInstance.GetComponent<NetworkObject>().Spawn();
        }

        if (NetworkManager.Singleton.IsListening)
        {
            m_SceneProgress = NetworkSceneManager.SwitchScene("PairScene2");
        }
        else
        {
            SceneManager.LoadSceneAsync("PairScene2");
        }
    }



}