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
    static string SharedSecret;
    static byte[] salt = new byte[64];
    string data1 = "";
    static byte[] localIV = new byte[16];
    static byte[] edata1 = new byte[8];
    static byte[] edata2 = new byte[8];
    Aes AesAlg;
    //private NetworkObject player;
    private int iterations = 10000;
    string plaintext;
    bool keysTested = false;
    public string ciphertext = null;

    

    void Start()
    {

        localIV = new System.Text.UTF8Encoding(false).GetBytes("ThisIVmustbe16by");
        plaintext = "Success!";
        SharedSecret = GameObject.Find("GazeLocationCapture(Clone)").GetComponent<GazeLocationCapture>().sharedSecret;
        SharedSecret = SharedSecret + GameObject.Find("GazeCapture(Clone)").GetComponent<GazeCapture>().sharedSecret;

        if (IsHost)
        {
            data1 = "Message from the Host Decrpyted - Pairing Sucessful!";
        }

        if (IsClient)
        {
            data1 = "Message from the Client Decrpyted - Pairing Sucessful!";
        }

        salt = new System.Text.UTF8Encoding(false).GetBytes(GameObject.Find("Salt(Clone)").GetComponent<Salt>().SaltValue.Value);
        while (salt == null)
        {
            salt = new System.Text.UTF8Encoding(false).GetBytes(GameObject.Find("Salt(Clone)").GetComponent<Salt>().SaltValue.Value);
        }

        if (IsHost)
        {
            Destroy(GameObject.Find("Salt(Clone)"));
        }
        
        Destroy(GameObject.Find("GazeLocationCapture(Clone)"));
        Destroy(GameObject.Find("GazeCapture(Clone)"));

        AesAlg = SetCryptoParams();
        ciphertext = Encrypt(data1, AesAlg);


    }

    void Update()
    {
        if(keysTested == false && IsHost)
        {
            var gazePairManagementComponent = GameObject.Find("GazePairConnectionManagement(Clone)");
            foreach (ulong client in gazePairManagementComponent.GetComponent<GazePairConnectionManagement>().getClientsInLobby().Keys)
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(client, out var networkedClient))
                {
                    var player = networkedClient.PlayerObject.GetComponent<GazePairCandidate>();
                    if (player)
                    {
                        try
                        {
                            //Debug.Log("Decrypting");
                            Decrypt(player.CipherText.Value, AesAlg);
                        }
                        catch (CryptographicException e)
                        {
                            plaintext = "Incorrect Key for atleast one pairing partner, pairing failed!";
                        }

                    }

                }
            }
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log(plaintext + " " + SharedSecret);

            }

            else if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log(plaintext + " " + SharedSecret);

            }
            keysTested = true;
        }

        if (keysTested == false && IsClient)
        {
            Debug.Log(plaintext + " " + SharedSecret);
            keysTested = true;
        }

        }

    Aes SetCryptoParams()
    {
        Aes encAlg = Aes.Create();
        encAlg.KeySize = 256;
        byte[] SharedSecretBytes = new System.Text.UTF8Encoding(false).GetBytes(SharedSecret);
        encAlg.Key = PBKDF2Sha256GetBytes(32, SharedSecretBytes, salt, iterations);
        encAlg.IV = localIV;
        return encAlg;
    }

    string Encrypt(string data1, Aes encAlg)
    {

        MemoryStream encryptionStream = new MemoryStream();
        CryptoStream encrypt = new CryptoStream(encryptionStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write);
        byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(data1);
        encrypt.Write(utfD1, 0, utfD1.Length);
        encrypt.FlushFinalBlock();
        encrypt.Close();
        edata1 = encryptionStream.ToArray();
       return Convert.ToBase64String(edata1);

    }

    string Decrypt(string cipherText, Aes decAlg)
    {

        MemoryStream decryptionStreamBacking = new MemoryStream();
        CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
        edata2 = Convert.FromBase64String(cipherText);
        decrypt.Write(edata2, 0, edata2.Length);
        decrypt.Flush();
        decrypt.Close();
        string data2 = new UTF8Encoding(false).GetString(decryptionStreamBacking.ToArray());

        return data2;
    }


    //No native .NET standard PBKDF2 implementation exists with SHA256, credit to:
    //https://stackoverflow.com/questions/18648084/rfc2898-pbkdf2-with-sha256-as-digest-in-c-sharp/18649357#18649357

    public static byte[] PBKDF2Sha256GetBytes(int dklen, byte[] password, byte[] salt, int iterationCount)
    {
        using (var hmac = new System.Security.Cryptography.HMACSHA256(password))
        {
            int hashLength = hmac.HashSize / 8;
            if ((hmac.HashSize & 7) != 0)
                hashLength++;
            int keyLength = dklen / hashLength;
            if ((long)dklen > (0xFFFFFFFFL * hashLength) || dklen < 0)
                throw new ArgumentOutOfRangeException("dklen");
            if (dklen % hashLength != 0)
                keyLength++;
            byte[] extendedkey = new byte[salt.Length + 4];
            Buffer.BlockCopy(salt, 0, extendedkey, 0, salt.Length);
            using (var ms = new System.IO.MemoryStream())
            {
                for (int i = 0; i < keyLength; i++)
                {
                    extendedkey[salt.Length] = (byte)(((i + 1) >> 24) & 0xFF);
                    extendedkey[salt.Length + 1] = (byte)(((i + 1) >> 16) & 0xFF);
                    extendedkey[salt.Length + 2] = (byte)(((i + 1) >> 8) & 0xFF);
                    extendedkey[salt.Length + 3] = (byte)(((i + 1)) & 0xFF);
                    byte[] u = hmac.ComputeHash(extendedkey);
                    Array.Clear(extendedkey, salt.Length, 4);
                    byte[] f = u;
                    for (int j = 1; j < iterationCount; j++)
                    {
                        u = hmac.ComputeHash(u);
                        for (int k = 0; k < f.Length; k++)
                        {
                            f[k] ^= u[k];
                        }
                    }
                    ms.Write(f, 0, f.Length);
                    Array.Clear(u, 0, u.Length);
                    Array.Clear(f, 0, f.Length);
                }
                byte[] dk = new byte[dklen];
                ms.Position = 0;
                ms.Read(dk, 0, dklen);
                ms.Position = 0;
                for (long i = 0; i < ms.Length; i++)
                {
                    ms.WriteByte(0);
                }
                Array.Clear(extendedkey, 0, extendedkey.Length);
                return dk;
            }
        }
    }
}
