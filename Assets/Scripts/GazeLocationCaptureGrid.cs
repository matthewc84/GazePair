using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System;
using System.Text;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine.SceneManagement;
using MLAPI.SceneManagement;

public class GazeLocationCaptureGrid : MonoBehaviour
{
    public static GazeLocationCaptureGrid Instance = null;
    public int clickMax;
    public int clickCount = 0;
    private SceneSwitchProgress m_SceneProgress;
    public GameObject SaltPrefab;
    GameObject SaltInstance;
    public GameObject keypadPrefab;
    GameObject keypadInstance;
    private int randomX;
    private int randomY;
    private int randomZ;
    Vector3 randomPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        randomX = UnityEngine.Random.Range(-5, 5);
        randomY = UnityEngine.Random.Range(-2, 4);
        randomZ = UnityEngine.Random.Range(3, 5);
        randomPosition = new Vector3(randomX / 2, randomY / 2, randomZ / 2);
        
        if (NetworkManager.Singleton.IsHost)
        {
            SaltInstance = Instantiate(SaltPrefab);
            SaltInstance.GetComponent<NetworkObject>().Spawn();
            keypadInstance = Instantiate(keypadPrefab, randomPosition, Quaternion.identity);
            keypadInstance.GetComponent<NetworkObject>().Spawn();
        }

    }

    // Update is called once per frame
    void Update()
    {


        if(clickCount == clickMax)
        {
            if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<GazePairCandidate>().SharedSecretReady.Value = true;
            }
            if (NetworkManager.Singleton.IsHost)
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

    }

}
