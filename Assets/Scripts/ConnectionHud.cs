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

    public GameObject ConnectionManagementPrefab;

    public Vector2 DrawOffset = new Vector2(10, 210);

    public GameObject startClientButton;
    public GameObject startHostButton;
    public GameObject nextSceneButton;
    private SceneSwitchProgress m_SceneProgress;

    void Start()
    {

    }

    void Update()
    {

    }
    public void OnServerFound(IPEndPoint sender, DiscoveryResponseData response)
    {
        Debug.Log("Found Server");
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

    void ServerControlsGUI()
    {
        if (GazePairNetworkDiscovery.Instance.IsRunning)
        {
            if (GUILayout.Button("Stop Server Discovery"))
            {
                GazePairNetworkDiscovery.Instance.StopDiscovery();
            }
        }
        else
        {
            if (GUILayout.Button("Start Server Discovery"))
            {
                GazePairNetworkDiscovery.Instance.StartServer();
            }
        }
    }

    public void serverButtonPressed()
    {
        Camera maincam = GameObject.Find("Main Camera").GetComponent<Camera>();

        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient)
        {
            //When Button pressed, start host
            //NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost(maincam.transform.position, null, null, null, null);

            //GazePairNetworkDiscovery.Instance.StartServer();
            startHostButton.GetComponentInChildren<TextMeshPro>().SetText("End Client Discovery");
            Instantiate(ConnectionManagementPrefab);
            nextSceneButton.SetActive(true);
            startClientButton.SetActive(false);
        }
        else
        {
            NetworkManager.Singleton.StopServer();
            //GazePairNetworkDiscovery.Instance.StopDiscovery();
            startHostButton.GetComponentInChildren<TextMeshPro>().SetText("Start Host");
            nextSceneButton.SetActive(false);
            startClientButton.SetActive(true);
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

    public void nextScene()
    {

        if (NetworkManager.Singleton.IsListening)
        {
            m_SceneProgress = NetworkSceneManager.SwitchScene("PairScene");
        }
        else
        {
            SceneManager.LoadSceneAsync("PairScene");
        }
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        //Your logic here
        bool approve = true;
        bool createPlayerObject = true;

        Vector3 vect = Vector3.zero;
        vect.x = System.BitConverter.ToSingle(connectionData, 0 * sizeof(float));
        vect.y = System.BitConverter.ToSingle(connectionData, 1 * sizeof(float));
        vect.z = System.BitConverter.ToSingle(connectionData, 2 * sizeof(float));
        //If approve is true, the connection gets added. If it's false. The client gets disconnected
        callback(createPlayerObject, null, approve, vect, null);
    }


}