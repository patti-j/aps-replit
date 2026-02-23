namespace PT.ServerManagerSharedLib.Utils
{
    /// <summary>
    /// Summary description for PtEncryptor.
    /// </summary>
    public class PtEncryptor
    {
        private const string c_password = "ljjfljlfjlkjfe34234";

        public static string Encrypt(string a_clearText)
        {
            //If clearText is null then set it to an empty string to prevent ArgumentNullException in SimpleStringEncryptor.Encrypt
            if (a_clearText == null)
            {
                a_clearText = string.Empty;
            }

            return SimpleStringEncryptor.Encrypt(a_clearText, c_password);
        }

        public static string Decrypt(string a_encryptedText)
        {
            //Return an empty string if the encrypted text is null in order to prevent ArgumentNullException in SimpleStringEncryptor.Decrypt
            return a_encryptedText == null ? string.Empty : SimpleStringEncryptor.Decrypt(a_encryptedText, c_password);
        }


        /// <summary>
        /// Attempts to encrypt a string. If the string was successfully encrypted the method will return true and out the encrypted string, if not then the method will return false and out the input string as the text is already unencrypted.
        /// </summary>
        /// <param name="a_string">String to decrypt</param>
        /// <param name="o_outputString">Outs the decrypted text if successful, outs the input string if not successful.</param>
        /// <returns></returns>
        public static bool TryEncrypt(string a_string, out string o_ouputString)
        {
            o_ouputString = a_string;

            try
            {
                o_ouputString = Encrypt(a_string);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to decrypt a string. If the string was successfully decrypted the method will return true and out the decrypted string, if not then the method will return false and out the input string as the text is already unencrypted.
        /// </summary>
        /// <param name="a_string">String to decrypt</param>
        /// <param name="o_outputString">Outs the decrypted text if successful, outs the input string if not successful.</param>
        /// <returns></returns>
        public static bool TryDecrypt(string a_string, out string o_outputString)
        {
            o_outputString = a_string;

            try
            {
                o_outputString = Decrypt(a_string);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}