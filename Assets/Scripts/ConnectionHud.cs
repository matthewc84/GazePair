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

public class ConnectionHud : MonoBehaviour
{

    Dictionary<IPAddress, DiscoveryResponseData> discoveredServers = new Dictionary<IPAddress, DiscoveryResponseData>();

    GameObject buttonParent;
    GameObject buttonChild;

    public Vector2 DrawOffset = new Vector2(10, 210);

    private SceneSwitchProgress m_SceneProgress;

    void Awake()
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
            NetworkManager.Singleton.StartHost(maincam.transform.position, null, null, null, null);
            GazePairNetworkDiscovery.Instance.StartServer();
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
            GazePairNetworkDiscovery.Instance.StopDiscovery();
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
        if (!NetworkManager.Singleton.IsClient)
        {
            //When Button pressed, start server
            //NetworkManager.Singleton.StartClient();
            GazePairNetworkDiscovery.Instance.StartClient();
            GazePairNetworkDiscovery.Instance.ClientBroadcast(new DiscoveryBroadcastData());
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
            GazePairNetworkDiscovery.Instance.StopDiscovery();
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


}
