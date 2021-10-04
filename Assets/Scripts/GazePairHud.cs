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
    public GameObject myPrefab;
    public GameObject GazeMonitorPrefab;

    private SceneSwitchProgress m_SceneProgress;

    void Awake()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            GameObject spawnButton = GameObject.Find("ButtonParent/SpawnTarget");
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
            GameObject go = Instantiate(myPrefab);
            go.GetComponent<NetworkObject>().Spawn();
            //GameObject GazeMonitor = Instantiate(GazeMonitorPrefab);
            //GazeMonitor.GetComponent<NetworkObject>().Spawn();
        }

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
