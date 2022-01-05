using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using MLAPI.SceneManagement;
using MLAPI.Transports.UNET;

public class PairTargetNetworkFunctionality : NetworkBehaviour
{
    //GameObject target;
    public float force;
    private Vector3 randomDirection;
    private Vector3 movement;
    int randomDegrees;
    double randomRadian;
    public int errorThreshold;
    int binnedDegrees;
    public float timeRemaining;
    bool timerIsRunning = false;
    private GameObject GazeMonitor;
    public GameObject GazeCapturePrefab;
    GameObject gazeCapture;
    private SceneSwitchProgress m_SceneProgress;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableVector3 Scale = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });


    void Start()
    {
        gazeCapture = Instantiate(GazeCapturePrefab);
        timerIsRunning = true;

        if (NetworkManager.Singleton.IsHost)
        {
            randomDegrees = UnityEngine.Random.Range(1, 360);
            binnedDegrees = ((int)(((randomDegrees + (errorThreshold-1)) / errorThreshold)) * errorThreshold) - (errorThreshold/2);
            randomRadian = binnedDegrees * (System.Math.PI / 180);
            randomDirection = new Vector3((float)System.Math.Cos(randomRadian), (float)System.Math.Sin(randomRadian) , 0);

            GetComponent<Rigidbody>().drag = 0;

            //ForceMode.VelocityChange will ignore mass of object and apply velocity instantly
            GetComponent<Rigidbody>().AddForce(randomDirection * force, ForceMode.VelocityChange);
        }
    }

    void Update()
    {

        if (NetworkManager.Singleton.IsHost)
        {
            Position.Value = this.transform.position;
            Scale.Value = this.transform.localScale;

        }

        if (NetworkManager.Singleton.IsClient)
        {
            this.transform.position = Position.Value;
            this.transform.localScale = Scale.Value;
        }

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                
                if (NetworkManager.Singleton.IsHost)
                {
                    
                    timeRemaining = 0;
                    timerIsRunning = false;
                    if (NetworkManager.Singleton.IsListening)
                    {
                        m_SceneProgress = NetworkSceneManager.SwitchScene("CryptoScene");
                    }
                    else
                    {
                        SceneManager.LoadSceneAsync("CryptoScene");
                    }
                    var obj = this.GetComponent<NetworkObject>();
                    obj.Despawn(true);
                    Destroy(this);

                }



            }
        }


    }

    void OnCollisionEnter(Collision other)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            //Check if you have to be specific to an object to bounce
            this.transform.GetComponent<Rigidbody>().velocity = Vector3.Reflect(other.relativeVelocity * -1, other.contacts[0].normal);
        }
    }

}
