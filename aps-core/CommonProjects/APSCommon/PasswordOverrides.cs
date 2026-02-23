using PT.Common.Encryption;

namespace PT.APSCommon.Debug;

public static class PasswordOverrides
{
    #if LoginAdmin
    private const string c_magicWord = "chicago";
    #endif

    public static bool IsOverride(string a_userName)
    {
        #if LoginAdmin
        return c_magicWord == a_userName;
        #else
        return false;
        #endif
    }
    public static bool IsOverride(byte[] a_passwordHash, byte[] a_salt)
    {
        #if LoginAdmin
        byte[] word = StringHasher.Hash(c_magicWord, a_salt);
        return word.SequenceEqual(a_passwordHash);
        #else
        return false;
        #endif
    }
}