using Microsoft.Data.SqlClient;

namespace PT.Common.Sql;

public class ConnectionValidation
{
    public static bool ValidateConnectionString(string a_connectionString)
    {
        try
        {
            SqlConnectionStringBuilder builder = new (a_connectionString);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool TestConnectionString(string a_connectionString)
    {
        try
        {
            if (!ValidateConnectionString(a_connectionString))
            {
                return false;
            }

            using SqlConnection conn = new (a_connectionString);
            conn.Open();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}