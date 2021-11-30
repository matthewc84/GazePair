using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Spawning;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System;
using System.Text;

/// <summary>
///     The default player object, spawned for each host and client in the Connection Scene.  Holds the functionality for both tracking the location of the player, 
///     and for the NetworkVariableVector3 that has their individual GazeDirectionChange.
/// </summary>
/// 
public class GazePairCandidate : NetworkBehaviour
{
   
    Camera mainCamera;
    Vector3 previousGazeValue;
    Vector3 tempGazeVector;
    double tempSecret;
    Vector3 GazeData;
    public int sampleRate;
    int sampleCounter;
    public int errorThreshold;
    public string sharedSecret;
    public bool targetSpawned = false;
    public bool targetDestroyed = false;
    private bool initialGazeVector = true;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    void Start()
    {
        sampleCounter = 0;

    }

    static Vector3 GetPositionOnPlane()
    {
        return GameObject.Find("Main Camera").transform.position;

    }

    public override void NetworkStart()
    {

        if (IsOwner)
        {
            Move();
        }
    }

    public void Move()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Position.Value = GetPositionOnPlane();
        }
        else if(NetworkManager.Singleton.IsClient)
        {
           SubmitPositionRequest_ServerRpc(GameObject.Find("Main Camera").transform.position);
        }
    }

    public void UpdateSharedSecret(Vector3 previousGazeValue, Vector3 GazeData)
    {

        tempGazeVector = GazeData;// - previousGazeValue;
            tempSecret = Math.Atan2(tempGazeVector.y, tempGazeVector.x);
            if(tempSecret > 0)
            {
                sharedSecret = ((int)(((tempSecret * 180/Math.PI) + errorThreshold-1) / errorThreshold)).ToString();
            }
            else
            {
                sharedSecret = ((int)((((tempSecret + (2*Math.PI)) * 180/Math.PI) + errorThreshold - 1) / errorThreshold)).ToString();
            }

    }

    [ServerRpc]
    void SubmitPositionRequest_ServerRpc(Vector3 clientPosition)
    {
        Position.Value = clientPosition;
    }


    void Update()
    {

        if (targetSpawned && initialGazeVector)
        {
            previousGazeValue = CoreServices.InputSystem.EyeGazeProvider.GazeDirection + CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
            initialGazeVector = false;
        }

        transform.position = Position.Value;
        //GazeData = GazeData + CoreServices.InputSystem.EyeGazeProvider.GazeDirection;// + CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
        sampleCounter++;
        
        if (IsOwner)
        {
            Move();

            if (targetDestroyed)
            {
                GazeData = CoreServices.InputSystem.EyeGazeProvider.GazeDirection + CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
                UpdateSharedSecret(previousGazeValue, GazeData);
                targetDestroyed = false;

            }
        }
    }

    //sampleCounter >= sampleRate && 





}
