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


    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    void Start()
    {
        

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
            Position.Value = Camera.main.transform.position;
        }
        else if(NetworkManager.Singleton.IsClient)
        {
           SubmitPositionRequest_ServerRpc(Camera.main.transform.position);
        }
    }

   

    [ServerRpc]
    void SubmitPositionRequest_ServerRpc(Vector3 clientPosition)
    {
        Position.Value = clientPosition;
    }


    void Update()
    {

        transform.position = Position.Value;
        if (IsOwner)
        {
            Move();
        }

    }



}
