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

//[RequireComponent(typeof(GazePairNetworkDiscovery))]
//[RequireComponent(typeof(NetworkManager))]
public class GazePairHud : MonoBehaviour
{
    [SerializeField, HideInInspector]
    NetworkManager m_NetworkManager;

    Dictionary<IPAddress, DiscoveryResponseData> discoveredServers = new Dictionary<IPAddress, DiscoveryResponseData>();

    //public GameObject buttonPrefab;
    GameObject buttonParent;
    GameObject buttonChild;

    void Awake()
    {
       // m_NetworkManager = GetComponent<NetworkManager>();
    }

#if UNITY_EDITOR
    void OnValidate()
    {

    }
#endif

    void OnServerFound(IPEndPoint sender, DiscoveryResponseData response)
    {
        discoveredServers[sender.Address] = response;
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
            NetworkManager.Singleton.StartClient();
            GazePairNetworkDiscovery.Instance.StartClient();
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

        GazePairNetworkDiscovery.Instance.ClientBroadcast(new DiscoveryBroadcastData());

    }


}
