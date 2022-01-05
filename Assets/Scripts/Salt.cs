using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

public class Salt : NetworkBehaviour
{

    static byte[] saltValue = new byte[64];
    public static Salt Instance = null;

    public NetworkVariableString SaltValue = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    // Start is called before the first frame update
    void Start()
    {

        if (IsHost)
        {
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with a random value.
                rngCsp.GetBytes(saltValue);
            }
            SaltValue.Value = Encoding.ASCII.GetString(saltValue);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
