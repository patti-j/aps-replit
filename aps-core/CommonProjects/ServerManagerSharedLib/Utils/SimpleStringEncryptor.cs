using System;
using System.IO;
using System.Security.Cryptography;

namespace PT.ServerManagerSharedLib.Utils
{
    // 
    // Sample encrypt/decrypt functions 
    //    Parameter checks and error handling are ommited for better readability 
    // 
    public class SimpleStringEncryptor
    {
        private static byte[] GetSalt()
        {
            return new byte[]
            {
                0xBD, 0x5D, 0xC1, 0x69, 0x25, 0x45, 0x4D, 0x8D, 0x8B, 0xEA, 0x2, 0x5E, 0xC, 0xE7, 0xF7, 0xCB, 0x2A, 0xE6, 0xE9, 0x8B, 0x0, 0x60, 0xFB,
                0xE9, 0xE2
            };
        }

        // Encrypt a byte array into a byte array using a key and an IV 
        private static byte[] Encrypt(byte[] a_clearData, byte[] a_key, byte[] a_iv)
        {
            // Create a MemoryStream that is going to accept the encrypted bytes 
            MemoryStream ms = new MemoryStream();
            // Create a symmetric algorithm. 
            // We are going to use Rijndael because it is strong and available on all platforms. 
            // You can use other algorithms, to do so substitute the next line with something like 
            //                      TripleDES alg = TripleDES.Create(); 
            Rijndael alg = Rijndael.Create();

            // Now set the key and the IV. 
            // We need the IV (Initialization Vector) because the algorithm is operating in its default 
            // mode called CBC (Cipher Block Chaining). The IV is XORed with the first block (8 byte) 
            // of the data before it is encrypted, and then each encrypted block is XORed with the 
            // following block of plaintext. This is done to make encryption more secure. 
            // There is also a mode called ECB which does not need an IV, but it is much less secure. 
            alg.Key = a_key;
            alg.IV = a_iv;

            // Create a CryptoStream through which we are going to be pumping our data. 
            // CryptoStreamMode.Write means that we are going to be writing data to the stream 
            // and the output will be written in the MemoryStream we have provided. 
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);

            // Write the data and make it do the encryption 
            cs.Write(a_clearData, 0, a_clearData.Length);

            // Close the crypto stream (or do FlushFinalBlock). 
            // This will tell it that we have done our encryption and there is no more data coming in, 
            // and it is now a good time to apply the padding and finalize the encryption process. 
            cs.Close();
            // Now get the encrypted data from the MemoryStream. 
            // Some people make a mistake of using GetBuffer() here, which is not the right way. 
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        private static void ValidatePassword(string a_password)
        {
            const int MIN_PASSWORD_LEN = 8;
            if (a_password == null || a_password.Length < MIN_PASSWORD_LEN)
            {
                throw new Exception(string.Format("2727: Minimum password length={0}", MIN_PASSWORD_LEN));
            }
        }

        /// <summary>
        /// Encrypt a string into a string using a password 
        ///    Uses Encrypt(byte[], byte[], byte[]) 
        /// Upd: Note, that the IV we are using in this sample comes from PasswordDeriveBytes and is statically linked to 
        /// the password. This may be ok when you only encrypt a few files or use different passwords for every file you 
        /// encrypt, but the moment you start mass file encryption with the same password this scheme isn't secure anymore. 
        /// To make it more secure make IVs random and distribute them along with your ciphertext (thanks to Mitch Gallant 
        /// for drawing attention to this).
        /// </summary>
        /// <param name="a_clearText">The text to encrypt.</param>
        /// <param name="a_password">The password to use to encrypt the text.</param>
        /// <returns></returns>
        public static string Encrypt(string a_clearText, string a_password)
        {
            ValidatePassword(a_password);

            // First we need to turn the input string into a byte array. 
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(a_clearText);

            // Then, we need to turn the password into Key and IV 
            // We are using salt to make it harder to guess our key using a dictionary attack - 
            // trying to guess a password by enumerating all possible words. 
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(a_password,
                GetSalt());

            // Now get the key/IV and do the encryption using the function that accepts byte arrays. 
            // Using PasswordDeriveBytes object we are first getting 32 bytes for the Key 
            // (the default Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV. 
            // IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael. 
            // If you are using DES/TripleDES/RC2 the block size is 8 bytes and so should be the IV size. 
            // You can also read KeySize/BlockSize properties off the algorithm to find out the sizes. 
            byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            // Now we need to turn the resulting byte array into a string. 
            // A common mistake would be to use an Encoding class for that. It does not work 
            // because not all byte values can be represented by characters. 
            // We are going to be using Base64 encoding that is designed exactly for what we are 
            // trying to do. 
            return Convert.ToBase64String(encryptedData);
        }

        //	// Encrypt bytes into bytes using a password 
        //	//    Uses Encrypt(byte[], byte[], byte[]) 
        //	public static byte[] Encrypt(byte[] clearData, string Password) 
        //	{ 
        //		// We need to turn the password into Key and IV. 
        //		// We are using salt to make it harder to guess our key using a dictionary attack - 
        //		// trying to guess a password by enumerating all possible words. 
        //		PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, 
        //			GetSalt()); 
        //
        //		// Now get the key/IV and do the encryption using the function that accepts byte arrays. 
        //		// Using PasswordDeriveBytes object we are first getting 32 bytes for the Key 
        //		// (the default Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV. 
        //		// IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael. 
        //		// If you are using DES/TripleDES/RC2 the block size is 8 bytes and so should be the IV size. 
        //		// You can also read KeySize/BlockSize properties off the algorithm to find out the sizes. 
        //		return Encrypt(clearData, pdb.GetBytes(32), pdb.GetBytes(16)); 
        //	} 

        //	// Encrypt a file into another file using a password 
        //	public static void Encrypt(string fileIn, string fileOut, string Password) 
        //	{ 
        //		// First we are going to open the file streams 
        //		FileStream fsIn = new FileStream(fileIn, FileMode.Open, FileAccess.Read); 
        //		FileStream fsOut = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write); 
        //
        //		// Then we are going to derive a Key and an IV from the Password and create an algorithm 
        //		PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, 
        //			GetSalt()); 
        //
        //		Rijndael alg = Rijndael.Create(); 
        //		alg.Key = pdb.GetBytes(32); 
        //		alg.IV = pdb.GetBytes(16); 
        //
        //		// Now create a crypto stream through which we are going to be pumping data. 
        //		// Our fileOut is going to be receiving the encrypted bytes. 
        //		CryptoStream cs = new CryptoStream(fsOut, alg.CreateEncryptor(), CryptoStreamMode.Write); 
        //
        //		// Now will will initialize a buffer and will be processing the input file in chunks. 
        //		// This is done to avoid reading the whole file (which can be huge) into memory. 
        //		int bufferLen = 4096; 
        //		byte[] buffer = new byte[bufferLen]; 
        //		int bytesRead; 
        //		do 
        //		{ 
        //			// read a chunk of data from the input file 
        //			bytesRead = fsIn.Read(buffer, 0, bufferLen); 
        //			// encrypt it 
        //			cs.Write(buffer, 0, bytesRead); 
        //		} while(bytesRead != 0); 
        //
        //		// close everything 
        //		cs.Close(); // this will also close the unrelying fsOut stream 
        //		fsIn.Close();     
        //	} 

        /// <summary>
        /// Decrypt a byte array into a byte array using a key and an IV 
        /// </summary>
        private static byte[] Decrypt(byte[] a_cipherData, byte[] a_key, byte[] a_iv)
        {
            // Create a MemoryStream that is going to accept the decrypted bytes 
            MemoryStream ms = new MemoryStream();

            // Create a symmetric algorithm. 
            // We are going to use Rijndael because it is strong and available on all platforms. 
            // You can use other algorithms, to do so substitute the next line with something like 
            //                      TripleDES alg = TripleDES.Create(); 
            Rijndael alg = Rijndael.Create();

            // Now set the key and the IV. 
            // We need the IV (Initialization Vector) because the algorithm is operating in its default 
            // mode called CBC (Cipher Block Chaining). The IV is XORed with the first block (8 byte) 
            // of the data after it is decrypted, and then each decrypted block is XORed with the previous 
            // cipher block. This is done to make encryption more secure. 
            // There is also a mode called ECB which does not need an IV, but it is much less secure. 
            alg.Key = a_key;
            alg.IV = a_iv;

            // Create a CryptoStream through which we are going to be pumping our data. 
            // CryptoStreamMode.Write means that we are going to be writing data to the stream 
            // and the output will be written in the MemoryStream we have provided. 
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);

            // Write the data and make it do the decryption 
            cs.Write(a_cipherData, 0, a_cipherData.Length);

            // Close the crypto stream (or do FlushFinalBlock). 
            // This will tell it that we have done our decryption and there is no more data coming in, 
            // and it is now a good time to remove the padding and finalize the decryption process. 
            cs.Close();

            // Now get the decrypted data from the MemoryStream. 
            // Some people make a mistake of using GetBuffer() here, which is not the right way. 
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        /// <summary>
        /// Decrypt an encrypted string.
        /// </summary>
        /// <param name="a_cipherText">The encrypted text.</param>
        /// <param name="a_password">The string you used to encrypt the text.</param>
        /// <returns></returns>
        public static string Decrypt(string a_cipherText, string a_password)
        {
            ValidatePassword(a_password);

            // First we need to turn the input string into a byte array. 
            // We presume that Base64 encoding was used 
            byte[] cipherBytes = Convert.FromBase64String(a_cipherText);

            // Then, we need to turn the password into Key and IV 
            // We are using salt to make it harder to guess our key using a dictionary attack - 
            // trying to guess a password by enumerating all possible words. 
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(a_password,
                GetSalt());

            // Now get the key/IV and do the decryption using the function that accepts byte arrays. 
            // Using PasswordDeriveBytes object we are first getting 32 bytes for the Key 
            // (the default Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV. 
            // IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael. 
            // If you are using DES/TripleDES/RC2 the block size is 8 bytes and so should be the IV size. 
            // You can also read KeySize/BlockSize properties off the algorithm to find out the sizes. 
            byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            // Now we need to turn the resulting byte array into a string. 
            // A common mistake would be to use an Encoding class for that. It does not work 
            // because not all byte values can be represented by characters. 
            // We are going to be using Base64 encoding that is designed exactly for what we are 
            // trying to do. 
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        //	// Decrypt bytes into bytes using a password 
        //	//    Uses Decrypt(byte[], byte[], byte[]) 
        //	public static byte[] Decrypt(byte[] cipherData, string Password) 
        //	{ 
        //		// We need to turn the password into Key and IV. 
        //		// We are using salt to make it harder to guess our key using a dictionary attack - 
        //		// trying to guess a password by enumerating all possible words. 
        //		PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, 
        //			GetSalt()); 
        //
        //		// Now get the key/IV and do the Decryption using the function that accepts byte arrays. 
        //		// Using PasswordDeriveBytes object we are first getting 32 bytes for the Key 
        //		// (the default Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV. 
        //		// IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael. 
        //		// If you are using DES/TripleDES/RC2 the block size is 8 bytes and so should be the IV size. 
        //		// You can also read KeySize/BlockSize properties off the algorithm to find out the sizes. 
        //		return Decrypt(cipherData, pdb.GetBytes(32), pdb.GetBytes(16)); 
        //	} 
        //
        //	// Decrypt a file into another file using a password 
        //	public static void Decrypt(string fileIn, string fileOut, string Password) 
        //	{ 
        //		// First we are going to open the file streams 
        //		FileStream fsIn = new FileStream(fileIn, FileMode.Open, FileAccess.Read); 
        //		FileStream fsOut = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write); 
        //
        //		// Then we are going to derive a Key and an IV from the Password and create an algorithm 
        //		PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, 
        //			GetSalt()); 
        //
        //		Rijndael alg = Rijndael.Create(); 
        //		alg.Key = pdb.GetBytes(32); 
        //		alg.IV = pdb.GetBytes(16); 
        //
        //		// Now create a crypto stream through which we are going to be pumping data. 
        //		// Our fileOut is going to be receiving the Decrypted bytes. 
        //		CryptoStream cs = new CryptoStream(fsOut, alg.CreateDecryptor(), CryptoStreamMode.Write); 
        //
        //		// Now will will initialize a buffer and will be processing the input file in chunks. 
        //		// This is done to avoid reading the whole file (which can be huge) into memory. 
        //		int bufferLen = 4096; 
        //		byte[] buffer = new byte[bufferLen]; 
        //		int bytesRead; 
        //
        //		do 
        //		{ 
        //			// read a chunk of data from the input file 
        //			bytesRead = fsIn.Read(buffer, 0, bufferLen); 
        //
        //			// Decrypt it 
        //			cs.Write(buffer, 0, bytesRead); 
        //		} while(bytesRead != 0); 
        //
        //		// close everything 
        //		cs.Close(); // this will also close the unrelying fsOut stream 
        //		fsIn.Close();     
        //	} 

        // 
        // Testing function 
        //    I am sure you will be able to figure out what it does! 
        // 
        //	public static void Main(string[] args) 
        //	{ 
        //		if (args.Length == 0) 
        //		{ 
        //			string plainText = "This is some plain textThis is some plain textThis is some plain text"; 
        //			string Password = "qwertyuio"; 
        //			Console.WriteLine("Plain text: \"" + plainText + "\", Password: \"" + Password + "\""); 
        //			string cipherText = Encrypt(plainText, Password ); 
        //			Console.WriteLine("Encrypted text: " + cipherText); 
        //			string decryptedText = Decrypt(cipherText, Password); 
        //			Console.WriteLine("Decrypted: " + decryptedText); 
        //		} 
        //		else 
        //		{ 
        ////			Encrypt(args[0], args[0]+".encrypted", "Password"); 
        ////			Decrypt(args[0]+".encrypted", args[0]+".decrypted", "Password"); 
        //		} 
        //		Console.WriteLine("Done."); 
        //	
    }

    // Making it "industry strength"
    // The encryption sample above had a very defined purpose - being extremely easy to read and understand. 
    // While it explains how to use symmetric encryption classes and gives some ideas on how to start implementing 
    // encryption in your applications, there are things you will have to do before it becomes a shippable piece 
    // of code. One of them I have already mentioned in the posting below - parameter checking and error handling.
    // Check the parameters for being valid, wrap calls that can potentially fail into try/catch blocks, use 
    // finally blocks to release resources (close files) if something goes wrong, etc. 
    // 
    // Some cryptography specific considerations should also be there. For example, the salt values in 
    // PasswordDeriveBytes should better be random rather than hardcoded (sometimes it is ok to have them 
    // hardcoded, for example, when encryption happens rarely and the code is not accessible by attackers). If the 
    // salt is random and changed frequently, you don't even have to keep it secret. Also, when possible, use byte[] 
    // keys as opposed to passwords. Because of the human factor, password-based encryption is not the most secure 
    // way to protect information. In order to get 128bit of key information out of a password it has to be long. 
    // If you are using just small letters that gives you about 5 bits of information per character and your password 
    // will have to be over 25 characters long to get to 128bit. If you are using capital letters and some symbols you 
    // can get to about 7 bits per character and your password minimum length to around 18 characters 
    // (how long is your password? ;-)). 
    // 
    // Upd: Note, that the IV we are using in this sample comes from PasswordDeriveBytes and is statically linked to 
    // the password. This may be ok when you only encrypt a few files or use different passwords for every file you 
    // encrypt, but the moment you start mass file encryption with the same password this scheme isn't secure anymore. 
    // To make it more secure make IVs random and distribute them along with your ciphertext (thanks to Mitch Gallant 
    // for drawing attention to this).

    // From: http://www.dotnetthis.com/Articles/Crypto.htm
}