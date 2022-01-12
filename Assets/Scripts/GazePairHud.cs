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

public class GazePairHud : MonoBehaviour
{
    
    public GameObject spawnButton;
    public GameObject timerPrefab;

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
            
            GameObject timer = Instantiate(timerPrefab);
            timer.GetComponent<NetworkObject>().Spawn();

        }


    }



    public void nextScene()
    {

        if (NetworkManager.Singleton.IsListening)
        {
            m_SceneProgress = NetworkSceneManager.SwitchScene("CryptoScene");
            
        }
        else
        {
            SceneManager.LoadSceneAsync("CryptoScene");
        }
    }


}
