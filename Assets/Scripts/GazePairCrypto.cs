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
using System.Security.Cryptography;


public class GazePairCrypto : NetworkBehaviour
{
    string test1 = "test1";
    Aes aes = Aes.Create();
    static string pwd1;
    static byte[] salt1 = new byte[8];
    void Start()
    {
        using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
        {
            // Fill the array with a random value.
            rngCsp.GetBytes(salt1);
        }

        if (IsOwner)
        {
            pwd1 = GameObject.Find("PlayerPrefab(Clone)").sharedSecret;
        }

        string data1 = "Some test data";
        //The default iteration count is 1000 so the two methods use the same iteration count.
        int myIterations = 1000;
        try
        {
            Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(pwd1, salt1, myIterations);
            Rfc2898DeriveBytes k2 = new Rfc2898DeriveBytes(pwd1, salt1);
            // Encrypt the data.
            Aes encAlg = Aes.Create();
            encAlg.Key = k1.GetBytes(16);
            MemoryStream encryptionStream = new MemoryStream();
            CryptoStream encrypt = new CryptoStream(encryptionStream,encAlg.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(data1);

            encrypt.Write(utfD1, 0, utfD1.Length);
            encrypt.FlushFinalBlock();
            encrypt.Close();
            byte[] edata1 = encryptionStream.ToArray();
            k1.Reset();

            // Try to decrypt, thus showing it can be round-tripped.
            Aes decAlg = Aes.Create();
            decAlg.Key = k2.GetBytes(16);
            decAlg.IV = encAlg.IV;
            MemoryStream decryptionStreamBacking = new MemoryStream();
            CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
            decrypt.Write(edata1, 0, edata1.Length);
            decrypt.Flush();
            decrypt.Close();
            k2.Reset();
            string data2 = new UTF8Encoding(false).GetString(decryptionStreamBacking.ToArray());

            if (!data1.Equals(data2))
            {
                Console.WriteLine("Error: The two values are not equal.");
            }
            else
            {
                Console.WriteLine("The two values are equal.");
                Console.WriteLine("k1 iterations: {0}", k1.IterationCount);
                Console.WriteLine("k2 iterations: {0}", k2.IterationCount);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
        }

    }

    
    


}
