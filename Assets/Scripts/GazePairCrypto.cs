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


public class GazePairCrypto : NetworkBehaviour
{
    Aes aes = Aes.Create();
    static string pwd1;
    static byte[] salt1 = new byte[8];
    string data1 = "";
    static byte[] localIV = new byte[8];
    static byte[] localkey = new byte[8];
    int counter = 0;
    static byte[] edata1 = new byte[8];
    static byte[] edata2 = new byte[8];
    Rfc2898DeriveBytes k1;
    Aes AesAlg;
    private NetworkObject player;
    private int iterations = 10000;
    string plaintext;

    public NetworkVariableString CipherText = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    void Start()
    {

        salt1 = new System.Text.UTF8Encoding(false).GetBytes("Thisis8bytesbutthelongerthebetter"); ;

        localIV = new System.Text.UTF8Encoding(false).GetBytes("ThisIVmustbe16by");

        pwd1 = GameObject.Find("GazeCapture(Clone)").GetComponent<GazeCapture>().sharedSecret;


        if (IsHost)
        {
            data1 = "Message from the Host";
        }
        else
        {
            data1 = "Message from the Client";
        }

        AesAlg = SetCryptoParams(k1);

        if (IsHost)
        {
            Encrypt(data1, AesAlg);
        }





    }

    void Update()
    {
        if (counter == 100)
        {
            try
            {
                plaintext = Decrypt(CipherText.Value, AesAlg);
            }
            catch(CryptographicException e)
            {
                plaintext = "Incorrect Key";
            }

            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("This is the Hosts's pwd: " + pwd1 + " and this is the encrypted message: " + plaintext);

            }

            else if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("This is the Client's pwd: " + pwd1 + " and this is the encrypted message: " + plaintext);

            }
            
            counter = 0;
        }

        counter++;
    }

    Aes SetCryptoParams(Rfc2898DeriveBytes k1)
    {
        Aes encAlg = Aes.Create();
        encAlg.KeySize = 256;
        k1 = new Rfc2898DeriveBytes(pwd1, salt1, iterations);
        encAlg.Key = k1.GetBytes(32);
        Debug.Log("Key: " + ByteArrayToString(encAlg.Key));
        Debug.Log("Length: " + encAlg.Key.Length);
        encAlg.IV = localIV;
        return encAlg;
    }

    void Encrypt(string data1, Aes encAlg)
    {

            MemoryStream encryptionStream = new MemoryStream();
            CryptoStream encrypt = new CryptoStream(encryptionStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(data1);
            encrypt.Write(utfD1, 0, utfD1.Length);
            encrypt.FlushFinalBlock();
            encrypt.Close();
            edata1 = encryptionStream.ToArray();
            CipherText.Value = Convert.ToBase64String(edata1);

    }

    string Decrypt(string cipherText, Aes decAlg)
    {
        // Try to decrypt, thus showing it can be round-tripped.

        MemoryStream decryptionStreamBacking = new MemoryStream();
        CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
        edata2 = Convert.FromBase64String(cipherText);
        decrypt.Write(edata2, 0, edata2.Length);
        decrypt.Flush();
        decrypt.Close();
        string data2 = new UTF8Encoding(false).GetString(decryptionStreamBacking.ToArray());

        return data2;
    }

    public static string ByteArrayToString(byte[] ba)
    {
        StringBuilder hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }


}
