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
    private Vector3 prevGazeDirectionVector;
    private Vector3 oneSecondGazeChange;
    private String oneSecondGazeChangeString = null;
    public float errorThreshold = .02f;
    public GameObject gazeTargetPrefab;
    int counter = 0;
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

    public NetworkVariableString SharedSecret = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.ServerOnly
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

    /*public void UpdateGazeDirectionChange()
    {
        var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;


        if (NetworkManager.Singleton.IsHost)
        {
            if (eyeGazeProvider != null)
            {
                //GazeDirectionChange.Value = eyeGazeProvider.GazeDirection - prevGazeDirectionVector;
                GazeDirectionChange.Value = eyeGazeProvider.GazeDirection;
            }
        }
        else
        {
            if(eyeGazeProvider != null)
            {
                //SubmitGazeDirectionRequestServerRpc((eyeGazeProvider.GazeDirection - prevGazeDirectionVector));
                SubmitGazeDirectionRequestServerRpc(eyeGazeProvider.GazeDirection);
            }
        }

        prevGazeDirectionVector = eyeGazeProvider.GazeDirection;
    }*/

    public void UpdateSharedSecret()
    {

        if (NetworkManager.Singleton.IsHost)
        {
            //SharedSecret.Value = oneSecondGazeChangeString;
            //SharedSecret.Value = ((int)(oneSecondGazeChange.x * 1000000000)).ToString() + " " + ((int)(oneSecondGazeChange.y * 1000000000)).ToString() + " " + ((int)(oneSecondGazeChange.z * 1000000000)).ToString();
            //SharedSecret.Value = ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeDirection.x / errorThreshold)).ToString() + " " + ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeDirection.y / errorThreshold)).ToString() + " " + ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeDirection.z / errorThreshold)).ToString();
            if (CoreServices.InputSystem.EyeGazeProvider.GazeTarget)
            {
                if (CoreServices.InputSystem.EyeGazeProvider.GazeTarget.name == "target_yellow(Clone)")
                {
                    SharedSecret.Value = ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeTarget.transform.position.x / errorThreshold)).ToString() + " " + ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeTarget.transform.position.y / errorThreshold)).ToString() + " " + ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeTarget.transform.position.z / errorThreshold)).ToString();
                }
            }
            else
            {
                SharedSecret.Value = ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeDirection.x / errorThreshold)).ToString() + " " + ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeDirection.y / errorThreshold)).ToString() + " " + ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeDirection.z / errorThreshold)).ToString();
            }
        }
        else
        {
            if (CoreServices.InputSystem.EyeGazeProvider.GazeTarget)
            {
                if (CoreServices.InputSystem.EyeGazeProvider.GazeTarget.name == "target_yellow(Clone)")
                {
                    SubmitSharedSecret_ServerRpc(((int)(CoreServices.InputSystem.EyeGazeProvider.GazeTarget.transform.position.x / errorThreshold)).ToString() + " " + ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeTarget.transform.position.y / errorThreshold)).ToString() + " " + ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeTarget.transform.position.z / errorThreshold)).ToString());
                }
            }
            else
            {
                SubmitSharedSecret_ServerRpc(((int)(CoreServices.InputSystem.EyeGazeProvider.GazeDirection.x / errorThreshold)).ToString() + " " + ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeDirection.y / errorThreshold)).ToString() + " " + ((int)(CoreServices.InputSystem.EyeGazeProvider.GazeDirection.z / errorThreshold)).ToString());
            }
        }
    }

    [ServerRpc]
    void SubmitSharedSecret_ServerRpc(String clientSharedSecretString)
    {
        SharedSecret.Value = clientSharedSecretString;
    }

    [ServerRpc]
    void SubmitPositionRequest_ServerRpc(Vector3 clientPosition)
    {
        Position.Value = clientPosition;
    }

    /*[ServerRpc]
    void SubmitGazeDirectionRequestServerRpc(Vector3 gazeDirectionChange)
    {
        GazeDirectionChange.Value = gazeDirectionChange;

    }*/

    void Update()
    {
        
        transform.position = Position.Value;
        //oneSecondGazeChange = oneSecondGazeChange + GazeDirectionChange.Value;
        //oneSecondGazeChange = GazeDirectionChange.Value - oneSecondGazeChange;
        counter++;
        if (IsOwner)
        {
            Move();
            UpdateSharedSecret();
            //UpdateGazeDirectionChange();
            if (counter == 60)
            {
                
                counter = 0;
            }
        }


    }

    static string FloatToBinary(float f)
    {
        StringBuilder sb = new StringBuilder();
        Byte[] ba = BitConverter.GetBytes(f);
        foreach (Byte b in ba)
            for (int i = 0; i < 8; i++)
            {
                sb.Insert(0, ((b >> i) & 1) == 1 ? "1" : "0");
            }
        string s = sb.ToString();
        string r = s.Substring(0, 1) + " " + s.Substring(1, 8) + " " + s.Substring(9); //sign exponent mantissa
        return r;
    }






}
