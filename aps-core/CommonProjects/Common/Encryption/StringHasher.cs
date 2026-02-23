using System.Security.Cryptography;

namespace PT.Common.Encryption;

public static class StringHasher
{
    public static byte[] Hash(string a_key, byte[] a_salt)
    {
        Rfc2898DeriveBytes pbkdf2 = new (a_key, a_salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        return hash;
    }

    public static byte[] GenerateSalt()
    {
        byte[] salt = new byte[16];
        new RNGCryptoServiceProvider().GetBytes(salt);
        return salt;
    }
}