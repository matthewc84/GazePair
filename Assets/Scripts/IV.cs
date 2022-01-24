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

public class IV : NetworkBehaviour
{

    static byte[] IVValue = new byte[16];
    public static IV Instance = null;

    public NetworkVariableString RandomIV = new NetworkVariableString(new NetworkVariableSettings
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
                rngCsp.GetBytes(IVValue);
            }
            RandomIV.Value = Encoding.ASCII.GetString(IVValue);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
