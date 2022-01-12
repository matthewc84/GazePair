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
using TMPro;
using System.Linq;

public class GazeLocationCaptureGrid : MonoBehaviour
{
    public static GazeLocationCaptureGrid Instance = null;
    public int clickMax;
    public int clickCount = 0;
    private SceneSwitchProgress m_SceneProgress;
    public GameObject SaltPrefab;
    GameObject SaltInstance;
    public GameObject keypadPrefab;
    public GameObject key0Prefab;
    public GameObject key1Prefab;
    public GameObject key2Prefab;
    public GameObject key3Prefab;
    public GameObject key4Prefab;
    public GameObject key5Prefab;
    public GameObject key6Prefab;
    public GameObject key7Prefab;
    public GameObject key8Prefab;
    public GameObject key9Prefab;
    GameObject keyInstance;
    private float randomX;
    private float randomY;
    private float randomZ;
    Vector3 randomPosition;
    List<float> usedXValues = new List<float>();
    List<float> usedYValues = new List<float>();
    List<int> usedPasswordValues = new List<int>();
    private string password;
    Dictionary<int, GameObject> keyPrefab = new Dictionary<int, GameObject>();
    Dictionary<int, bool> clientsReady = new Dictionary<int, bool>();

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

        keyPrefab.Add(0, key0Prefab);
        keyPrefab.Add(1, key1Prefab);
        keyPrefab.Add(2, key2Prefab);
        keyPrefab.Add(3, key3Prefab);
        keyPrefab.Add(4, key4Prefab);
        keyPrefab.Add(5, key5Prefab);
        keyPrefab.Add(6, key6Prefab);
        keyPrefab.Add(7, key7Prefab);
        keyPrefab.Add(8, key8Prefab);
        keyPrefab.Add(9, key9Prefab);

        foreach (ulong client in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            clientsReady.Add((int)client, false);


        }

            if (NetworkManager.Singleton.IsHost)
        {
            for (int i = 0; i < clickMax; i++)
            {
                int randomInt = UnityEngine.Random.Range(0, 9);
                while (usedPasswordValues.Contains(randomInt))
                {
                    randomInt = UnityEngine.Random.Range(0, 9);
                }
                usedPasswordValues.Add(randomInt);
                password = password + randomInt.ToString();
                
            }
            this.GetComponent<TextMeshPro>().SetText("Tell the group the pairing password is: " + password);
            randomZ = UnityEngine.Random.Range(6, 9)/2;

            for (int i = 0; i < 10; i++)
            {
                randomX = (float)UnityEngine.Random.Range(-5, 5) / 2;
                //Debug.Log(randomX);
                while (usedXValues.Contains(randomX))
                {
                    randomX = (float)UnityEngine.Random.Range(-5, 5) / 2;
                }
                usedXValues.Add(randomX);

                randomY = (float)UnityEngine.Random.Range(-5, 5) / 2;
                //Debug.Log(randomY);
                while (usedYValues.Contains(randomY))
                {
                    randomY = (float)UnityEngine.Random.Range(-5, 5) / 2;
                }
                usedYValues.Add(randomY);

                randomPosition = new Vector3(randomX, randomY, randomZ);
                GameObject keyPrefabInstance;
                keyPrefab.TryGetValue(i, out keyPrefabInstance);
                keyInstance = Instantiate(keyPrefabInstance, randomPosition, Quaternion.identity);
                keyInstance.GetComponent<NetworkObject>().Spawn();
            }

            SaltInstance = Instantiate(SaltPrefab);
            SaltInstance.GetComponent<NetworkObject>().Spawn();

        }

    }

    // Update is called once per frame
    void Update()
    {
        if(clickCount == clickMax)
        {
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<GazePairCandidate>().SharedSecretReady.Value = true;
            if (NetworkManager.Singleton.IsHost)
            {
                foreach (ulong client in NetworkManager.Singleton.ConnectedClients.Keys)
                {
                    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(client, out var networkedClient))
                    {
                        var player = networkedClient.PlayerObject.GetComponent<GazePairCandidate>();
                        if (player)
                        {
                            //If a client has entered a sequence of the required length, set the List used to keep track to the correct value.
                            if (player.SharedSecretReady.Value)
                            {
                                clientsReady[(int)client] = true;
                            }

                        }

                    }
                }

                //If all clients are ready change to the final scene
                if (NetworkManager.Singleton.IsListening && clientsReady.All(x => x.Value == true))
                {
                    m_SceneProgress = NetworkSceneManager.SwitchScene("CryptoScene");
                }
                else
                {
                    this.GetComponent<TextMeshPro>().SetText("Tell the group the pairing password is: " + password + ". Waiting on Clients to complete passwork entry.....");

                }
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                this.GetComponent<TextMeshPro>().SetText("Code entered, waiting on Host and all Clients to finish.....");
            }

        }



    }

}
