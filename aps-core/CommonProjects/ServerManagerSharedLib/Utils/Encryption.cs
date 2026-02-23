using System.Data.SqlClient;

namespace PT.ServerManagerSharedLib.Utils
{
    public static class Encryption
    {
        public static string Encrypt(string a_clearText)
        {
            return PtEncryptor.Encrypt( a_clearText );
        }

        public static string Decrypt(string a_encryptedText)
        {
            return PtEncryptor.Decrypt(a_encryptedText);
        }

        public static string RemovePasswordPortionFromConnectionString(string a_originalConnectionString)
        {
            SqlConnectionStringBuilder tempBuilder;

            try
            {
                tempBuilder = new SqlConnectionStringBuilder(a_originalConnectionString);
                tempBuilder.Remove("Password");
                return tempBuilder.ConnectionString;
            }
            catch (System.ArgumentException e)
            {
                //TODO: LOG THIS
            }
            return a_originalConnectionString;
        }
    }
}
