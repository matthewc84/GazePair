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

public class GazePairCryptoHud : MonoBehaviour
{

    public GameObject CryptoPrefab;
    public GameObject spawnButton;

    private SceneSwitchProgress m_SceneProgress;

    void Awake()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            spawnButton.SetActive(false);
        }
    }
    void Update()
    {

    }

    public void spawnButtonPressed()
    {

        if (NetworkManager.Singleton.IsHost)
        {
            GameObject go = Instantiate(CryptoPrefab);
            go.GetComponent<NetworkObject>().Spawn();

        }


    }


    public void nextScene()
    {

        if (NetworkManager.Singleton.IsListening)
        {
            m_SceneProgress = NetworkSceneManager.SwitchScene("WorldSpace");

        }
        else
        {
            SceneManager.LoadSceneAsync("WorldSpace");
        }
    }


}
