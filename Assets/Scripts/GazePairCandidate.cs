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
    bool CipherTextUpdated = false;

    public NetworkVariableString CipherText = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    void Start()
    {
        

    }

    public override void NetworkStart()
    {

    }


    public void UpdateCipherText()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if(GameObject.Find("GazePairCrypto") != null && !CipherTextUpdated)
            {
                CipherText.Value = GameObject.Find("GazePairCrypto").GetComponent<GazePairCrypto>().ciphertext;
                CipherTextUpdated = true;
            }

        }
        else if (NetworkManager.Singleton.IsClient)
        {
            if (GameObject.Find("GazePairCrypto") != null && !CipherTextUpdated)
            {
                SubmitEncryptedValue_ServerRpc(GameObject.Find("GazePairCrypto").GetComponent<GazePairCrypto>().ciphertext);
                CipherTextUpdated = true;
            }
        }
    }

    [ServerRpc]
    void SubmitEncryptedValue_ServerRpc(string encryptedValue)
    {
        CipherText.Value = encryptedValue;
    }


    void Update()
    {
        //Camera.main.transform.position = Vector3.zero;

        if (IsOwner)
        {
            UpdateCipherText();
        }

    }



}
