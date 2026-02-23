using System.Security.Cryptography;
using System.Text;

using PT.Common.Encryption;

namespace PT.Common.OTPCodeGeneration;

//This password validation method is no longer user as part of V12. This was replaced by resetting the admin user password from InstanceManager.
///Time-Based One-Time Password
///Code referenced from https://srchub.org/p/otpnet/downloads/97/  (IntToByteString and Split functions)
internal class TotpGenerator
{
    /// <summary>
    /// Check if the provided code is correct for the provided key.
    /// </summary>
    private static bool GenAndCheckCode(string a_key, byte[] a_hashedCode, byte[] a_salt)
    {
        if (a_key == null)
        {
            return false;
        }

        try
        {
            string code = GetCode(a_key);
            byte[] hashedCode = StringHasher.Hash(code, a_salt);
            if (hashedCode.SequenceEqual(a_hashedCode))
            {
                return true;
            }

            //Try the previous time period in case there was too much delay
            code = GetCode(a_key, -1);
            hashedCode = StringHasher.Hash(code, a_salt);
            if (hashedCode.SequenceEqual(a_hashedCode))
            {
                return true;
            }
        }
        catch (Exception)
        {
            //TODO: This shouldn't happen.
        }

        return false;
    }

    /// <summary>
    /// Generates a 6 digit code based on the current time and the provided key.
    /// </summary>
    private static string GetCode(string a_key, int a_offset = 0)
    {
        //Use utc time
        DateTime currentDateTime = DateTime.UtcNow;

        //Hour boundries
        DateTime modifiedTime = new (currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0);
        modifiedTime += TimeSpan.FromHours(a_offset);
        byte[] timeCode = IntToByteString(modifiedTime.Ticks);

        byte[] bytes = Encoding.ASCII.GetBytes(a_key);

        HMACMD5 hashGenerator = new (bytes);
        byte[] computeHash = hashGenerator.ComputeHash(timeCode);

        List<int> hmac = new ();
        foreach (byte b in computeHash)
        {
            string s = b.ToString("x2");
            hmac.Add(int.Parse(s, System.Globalization.NumberStyles.HexNumber));
        }

        int offset = hmac[hmac.Count - 1] % (hmac.Count - 3);
        int code = ((hmac[offset + 0] & 0x7F) << 24) | ((hmac[offset + 1] & 0xFF) << 16) | ((hmac[offset + 2] & 0xFF) << 8) | (hmac[offset + 3] & 0xFF);

        int finalCode = code % (int)Math.Pow(10, c_codeLength);
        return finalCode.ToString();
    }

    private const short c_codeLength = 6;

    private static byte[] IntToByteString(long a_i)
    {
        List<byte> res = new ();

        while (a_i != 0)
        {
            res.Add((byte)(a_i & 0xFF));
            a_i >>= 8;
        }

        int rcount = res.Count;
        for (int z = 0; z < 8 - rcount; z++)
        {
            res.Add(0);
        }

        res.Reverse();

        return res.ToArray();
    }

    /// <summary>Returns a string array that contains the substrings in this string that are seperated a given fixed length.</summary>
    /// <param name="a_s">This string object.</param>
    /// <param name="a_length">
    /// Size of each substring.
    /// <para>CASE: length &gt; 0 , RESULT: String is split from left to right.</para>
    /// <para>CASE: length == 0 , RESULT: String is returned as the only entry in the array.</para>
    /// <para>CASE: length &lt; 0 , RESULT: String is split from right to left.</para>
    /// </param>
    /// <returns>String array that has been split into substrings of equal length.</returns>
    /// <example>
    /// <code>
    ///         string s = "1234567890";
    ///         string[] a = s.Split(4); // a == { "1234", "5678", "90" }
    ///     </code>
    /// </example>
    private static string[] Split(string a_s, int a_length)
    {
        System.Globalization.StringInfo str = new (a_s);

        int lengthAbs = Math.Abs(a_length);

        if (str.LengthInTextElements == 0 || lengthAbs == 0 || str.LengthInTextElements <= lengthAbs)
        {
            return new[] { str.ToString() };
        }

        string[] array = new string[str.LengthInTextElements % lengthAbs == 0 ? str.LengthInTextElements / lengthAbs : str.LengthInTextElements / lengthAbs + 1];

        if (a_length > 0)
        {
            for (int iStr = 0, iArray = 0; iStr < str.LengthInTextElements && iArray < array.Length; iStr += lengthAbs, iArray++)
            {
                array[iArray] = str.SubstringByTextElements(iStr, str.LengthInTextElements - iStr < lengthAbs ? str.LengthInTextElements - iStr : lengthAbs);
            }
        }
        else // if (length < 0)
        {
            for (int iStr = str.LengthInTextElements - 1, iArray = array.Length - 1; iStr >= 0 && iArray >= 0; iStr -= lengthAbs, iArray--)
            {
                array[iArray] = str.SubstringByTextElements(iStr - lengthAbs < 0 ? 0 : iStr - lengthAbs + 1, iStr - lengthAbs < 0 ? iStr + 1 : lengthAbs);
            }
        }

        return array;
    }
}