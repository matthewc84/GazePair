using System.Collections.Generic;
using System.Net;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using Object = UnityEngine.Object;
using TMPro;
using UnityEngine.SceneManagement;
using MLAPI.SceneManagement;
using Microsoft.MixedReality.Toolkit;

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
    public GameObject gridSceneButton;
    public GameObject loggerPrefab;
    private SceneSwitchProgress m_SceneProgress;

    void Start()
    {
        //CoreServices.DiagnosticsSystem.ShowDiagnostics = false;
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

    public void serverButtonPressed()
    {
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient)
        {
            //When Button pressed, start host
            NetworkManager.Singleton.StartHost(new Vector3(0,0,0), null, null, null, null);
            //GazePairNetworkDiscovery.Instance.StartServer();
            startHostButton.GetComponentInChildren<TextMeshPro>().SetText("End Client Discovery");
            Instantiate(ConnectionHudPrefab);
            gridSceneButton.SetActive(true);
            startClientButton.SetActive(false);
            var loggerInstance = Instantiate(loggerPrefab);
            


        }
        else
        {
            NetworkManager.Singleton.StopServer();
            //GazePairNetworkDiscovery.Instance.StopDiscovery();
            startHostButton.GetComponentInChildren<TextMeshPro>().SetText("Start Host");
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