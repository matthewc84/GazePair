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

public class GazePairCryptoHud : NetworkBehaviour
{

    private SceneSwitchProgress m_SceneProgress;

    void Awake()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            //spawnButton.SetActive(false);
        }
    }
    void Update()
    {

    }


    public void restartGazePair()
    {
        if (IsHost)
        {
            NetworkManager.Singleton.StopHost();
        }
        else if (IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }
        else if (IsServer)
        {
            NetworkManager.Singleton.StopServer();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Connection Scene");

    }


}
