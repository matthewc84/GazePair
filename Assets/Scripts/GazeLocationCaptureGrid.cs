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
    public GameObject IVPrefab;
    GameObject IVInstance;
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
    private float randomZ;
    Vector3 randomPosition;
    List<Tuple<float, float>> usedKeyLocationValues = new List<Tuple<float, float>>();
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

        if (NetworkManager.Singleton.IsHost)
        {

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
        

        
            GameObject.Find("LoggerScript(Clone)").GetComponent<LoggerScript>().startGridPairAttempt();

            for (int i = 0; i < clickMax; i++)
            {
                int randomInt = UnityEngine.Random.Range(0, 10);
                while (usedPasswordValues.Contains(randomInt))
                {
                    randomInt = UnityEngine.Random.Range(0, 10);
                }
                usedPasswordValues.Add(randomInt);
                password = password + randomInt.ToString();
                
            }
            this.GetComponent<TextMeshPro>().SetText("Tell the group the pairing password is: " + password);
            //To prevent hiding a key, we spawn all of them on the same Z value, randomly chosen
            randomZ = UnityEngine.Random.Range(9, 14) / 2;
            //Assign a random, non repeating location to each key and spawn them
            for (int i = 0; i < 10; i++)
            {
                Tuple<float, float> testKeyLocation = new Tuple<float, float>((float)UnityEngine.Random.Range(-3, 4) / 2, (float)UnityEngine.Random.Range(-4, 2) / 2);
                //If X,Y value in use, try again, also, we dont use 0,0, so we dont obscure the Hosts passcode prompt
                while (usedKeyLocationValues.Contains(testKeyLocation) || (testKeyLocation.Item1 == 0 && testKeyLocation.Item2 == 0))
                {
                    testKeyLocation = new Tuple<float, float>((float)UnityEngine.Random.Range(-3, 4) / 2, (float)UnityEngine.Random.Range(-4, 2) / 2);
                }
                usedKeyLocationValues.Add(testKeyLocation);

                randomPosition = new Vector3(testKeyLocation.Item1, testKeyLocation.Item2, randomZ);
                GameObject keyPrefabInstance;
                keyPrefab.TryGetValue(i, out keyPrefabInstance);
                keyInstance = Instantiate(keyPrefabInstance, randomPosition, Quaternion.identity);
                keyInstance.GetComponent<NetworkObject>().Spawn();
            }
            //The networked Salt value for Crypto purposes
            SaltInstance = Instantiate(SaltPrefab);
            SaltInstance.GetComponent<NetworkObject>().Spawn();
            IVInstance = Instantiate(IVPrefab);
            IVInstance.GetComponent<NetworkObject>().Spawn();

        }

    }

    // Update is called once per frame
    void Update()
    {
        if(clickCount == clickMax)
        {
            //Once click count reached, set progress variable to true and wait for other members of pairing...
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
