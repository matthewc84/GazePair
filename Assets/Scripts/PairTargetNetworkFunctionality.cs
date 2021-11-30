using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using System;
using System.Collections;

public class PairTargetNetworkFunctionality : NetworkBehaviour
{
    GameObject target;
    public float force;
    private Vector3 randomDirection;
    private Vector3 movement;
    public int randomDegrees;
    double randomRadian;
    public int errorThreshold;
    public int binnedDegrees;
    public int bin;
    public float timeRemaining = 10;
    bool timerIsRunning = false;
    public GameObject logger;
    private GameObject GazeMonitor;
    private NetworkObject player;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });


    void Start()
    {
        timerIsRunning = true;
        player = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;
        player.GetComponent<GazePairCandidate>().targetSpawned =  true;

        if (NetworkManager.Singleton.IsHost)
        {
            //GazeMonitor = Instantiate(logger);
            randomDegrees = UnityEngine.Random.Range(1, 360);
            bin = (int)(((randomDegrees + (errorThreshold - 1)) / errorThreshold));
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

        }

        if (NetworkManager.Singleton.IsClient)
        {
            this.transform.position = Position.Value;
            //Debug.Log(Position.Value);
        }

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                player.GetComponent<GazePairCandidate>().targetSpawned = false;
                player.GetComponent<GazePairCandidate>().targetDestroyed = true;

                if (NetworkManager.Singleton.IsHost)
                {
                    StartCoroutine(wait());
                    timeRemaining = 0;
                    timerIsRunning = false;
                    var obj = this.GetComponent<NetworkObject>();
                    obj.Despawn(true);
                    Destroy(this);

                }



            }
        }

        IEnumerator wait()
        {
            yield return new WaitForSecondsRealtime(3);

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
