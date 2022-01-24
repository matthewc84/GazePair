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
///     The default player object, spawned for each host and client in the Connection Scene.  Holds the functionality to record if the player has gathered a shared secret and is ready to proceed and the ciphertext the Host will use to test keys.
/// </summary>
/// 
public class GazePairCandidate : NetworkBehaviour
{
   
    Camera mainCamera;

    public NetworkVariableString CipherText = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableBool SharedSecretReady = new NetworkVariableBool(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    void Start()
    {
        if (IsOwner)
        {
            SharedSecretReady.Value = false;
        }

    }

    public override void NetworkStart()
    {

    }

    [ServerRpc]
    public void SubmitEncryptedValue_ServerRpc(string encryptedValue)
    {
        CipherText.Value = encryptedValue;

    }

    void Update()
    {

    }



}
