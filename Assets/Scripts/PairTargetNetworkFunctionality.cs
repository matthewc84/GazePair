using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using System;

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

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });


    void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
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
