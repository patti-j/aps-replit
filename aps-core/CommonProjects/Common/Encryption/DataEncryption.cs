using System.Security.Cryptography;

namespace PT.Common.Encryption;

/// <summary>
/// Symmetric encryption class. Manages encryption parameters and matching structure for encryption and decryption
/// InitializationVector is managed. Key must be provided.
/// </summary>
public static class DataEncryption
{
    /// <summary>
    /// Creates a new random encryption key
    /// </summary>
    /// <returns></returns>
    public static byte[] GenerateEncryptionKey()
    {
        using (AesManaged aesEncryptor = new ())
        {
            InitializeAesProperties(aesEncryptor);
            aesEncryptor.GenerateKey();
            return aesEncryptor.Key;
        }
    }

    public static byte[] GenerateIV()
    {
        using (AesManaged aesEncryptor = new ())
        {
            InitializeAesProperties(aesEncryptor);
            aesEncryptor.GenerateIV();
            return aesEncryptor.IV;
        }
    }

    private static void InitializeAesProperties(AesManaged a_encryptor)
    {
        a_encryptor.KeySize = c_keySize;
        a_encryptor.Mode = CipherMode.CBC;
        a_encryptor.BlockSize = 128;
        a_encryptor.Padding = PaddingMode.PKCS7;
    }

    //Key sizes must match
    private const int c_keySize = 256;

    public static byte[] EncryptData(byte[] a_key, byte[] a_data)
    {
        // Check arguments.
        if (a_data == null || a_data.Length <= 0)
        {
            throw new ArgumentNullException("a_data");
        }

        if (a_key == null || a_key.Length != c_keySize / 8)
        {
            throw new ArgumentOutOfRangeException("a_key");
        }

        byte[] encrypted;


        // Create an AesManaged object
        // with the specified key and IV.
        using (AesManaged aesAlg = new ())
        {
            InitializeAesProperties(aesAlg);
            aesAlg.Key = a_key;
            aesAlg.GenerateIV();

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new ())
            {
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                using (CryptoStream csEncrypt = new (msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new (csEncrypt))
                    {
                        //Convert to sting so the streamready can read it back
                        string stringData = Convert.ToBase64String(a_data);
                        //Write all data to the stream.
                        swEncrypt.Write(stringData);
                    }

                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    public static CryptoStream GetCryptoStreamWriter(Stream a_ms, byte[] a_key, byte[] a_iv)
    {
        // Check arguments.
        if (a_key == null || a_key.Length != c_keySize / 8)
        {
            throw new ArgumentOutOfRangeException("a_key");
        }

        // Create an AesManaged object
        // with the specified key and IV.
        using (AesManaged aesAlg = new ())
        {
            InitializeAesProperties(aesAlg);
            aesAlg.Key = a_key;
            aesAlg.IV = a_iv;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            return new CryptoStream(a_ms, encryptor, CryptoStreamMode.Write);
        }
    }

    public static CryptoStream GetCryptoStreamReader(Stream a_ms, byte[] a_key, byte[] a_iv)
    {
        // Check arguments.
        if (a_key == null || a_key.Length != c_keySize / 8)
        {
            throw new ArgumentOutOfRangeException("a_key");
        }

        // Create an AesManaged object
        // with the specified key and IV.
        using (AesManaged aesAlg = new ())
        {
            InitializeAesProperties(aesAlg);
            aesAlg.Key = a_key;
            aesAlg.IV = a_iv;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            return new CryptoStream(a_ms, encryptor, CryptoStreamMode.Read);
        }
    }

    public static byte[] DecryptData(byte[] a_key, byte[] a_encryptedData)
    {
        // Check arguments.
        if (a_encryptedData == null || a_encryptedData.Length <= 0)
        {
            throw new ArgumentNullException("a_encryptedData");
        }

        if (a_key == null || a_key.Length <= 0)
        {
            throw new ArgumentNullException("a_key");
        }

        byte[] decryptedData;

        // Create an AesManaged object
        // with the specified key.
        using (AesManaged aesAlg = new ())
        {
            InitializeAesProperties(aesAlg);
            aesAlg.Key = a_key;

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new (a_encryptedData))
            {
                //Read IV from the unencrypted portion of the data
                byte[] iv = new byte[aesAlg.IV.Length];
                msDecrypt.Read(iv, 0, iv.Length);
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (CryptoStream csDecrypt = new (msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new (csDecrypt))
                    {
                        // Read the decrypted bytes from the decrypting stream
                        // This was the best method I found to read an unknown data size from the stream
                        string plaintext = srDecrypt.ReadToEnd();
                        //Must be converted back to bytes
                        decryptedData = Convert.FromBase64String(plaintext);
                    }
                }
            }
        }

        return decryptedData;
    }
}