using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System;


public class GazePairCandidate : NetworkBehaviour
{
    private Vector3 prevGazeDirectionVector;
    Camera mainCamera;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableVector3 GazeDirectionChange = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    void Start()
    {
        var prevGazeDirectionVector = CoreServices.InputSystem?.EyeGazeProvider.GazeDirection;
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

    public void UpdateGazeDirectionChange()
    {
        var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;


        if (NetworkManager.Singleton.IsHost)
        {
            if (eyeGazeProvider != null)
            {
                GazeDirectionChange.Value = eyeGazeProvider.GazeDirection - prevGazeDirectionVector;
            }
        }
        else
        {
            if(eyeGazeProvider != null)
            {
                SubmitGazeDirectionRequestServerRpc((eyeGazeProvider.GazeDirection - prevGazeDirectionVector));
            }
        }

        prevGazeDirectionVector = eyeGazeProvider.GazeDirection;
    }


    [ServerRpc]
    void SubmitPositionRequest_ServerRpc(Vector3 clientPosition)
    {
        Position.Value = clientPosition;
    }

    [ServerRpc]
    void SubmitGazeDirectionRequestServerRpc(Vector3 gazeDirectionChange)
    {
        GazeDirectionChange.Value = gazeDirectionChange;

    }

    void Update()
    {
        transform.position = Position.Value;

        if (IsOwner)
        {
            Move();
            UpdateGazeDirectionChange();
        }



    }

}
