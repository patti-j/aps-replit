using System.CodeDom;
using System.Data;

using Microsoft.Data.SqlClient;

using Newtonsoft.Json;

using Dapper;

using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.Data
{
    public class SqlDatabaseConnectionHelper
    {
        private string m_connectionString;

        public SqlDatabaseConnectionHelper(string a_connectionString)
        {
            Initialize(a_connectionString);
        }

        public void Initialize(string a_connectionString)
        {
            m_connectionString = a_connectionString;
        }

        public List<T> GetDeserializedJsonRows<T>(string a_sql, int a_retryCount = 3)
        {
            return ExecuteSqlWithRetry(a_sql, (sqlCommand, connection) =>
            {
                List<T> result = new List<T>();

                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string instanceJson = reader.GetString(0);
                        T instance = JsonConvert.DeserializeObject<T>(instanceJson);
                        result.Add(instance);
                    }
                    return result;
                }
            }, new Dictionary<string, object>());
        }

        public object ExecuteScalar(string a_sql, Dictionary<string, object> a_params = null, int a_retryCount = 3)
        {
            return ExecuteSqlWithRetry(a_sql, (sqlCommand, connection) => sqlCommand.ExecuteScalar(), a_params, a_retryCount);
        }

        public APSInstanceEntity GetFullInstanceDataFromDb(string a_sql, Dictionary<string, object> a_params, int a_retryCount = 3)
        {
            return ExecuteSqlWithRetry(a_sql, (sqlCommand, connection) => 
                connection
                    .Query<string, ServerWideInstanceSettings, APSInstanceEntity>(sqlCommand.CommandText,
                        (instanceJson, serversettings) =>
                        {
                            var instance = JsonConvert.DeserializeObject<APSInstanceEntity>(instanceJson);
                            instance.ServerWideSettings = serversettings;
                            return instance;
                        },
                        splitOn: "ServerName", // name an extra, redundant value in the select this, between the joins. Dapper will split on that value
                        param: a_params)
                    .FirstOrDefault(), a_params, a_retryCount);
        }

        public int Insert(string a_sql, Dictionary<string, object> a_params, int a_retryCount = 3)
        {
            return ExecuteSqlWithRetry(a_sql, (sqlCommand, connection) => sqlCommand.ExecuteNonQuery(), a_params, a_retryCount);
        }

        public bool IsValid()
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(m_connectionString);

            return !string.IsNullOrEmpty(sb.DataSource);
        }

        public ServerWideInstanceSettings GetServerSettings(string a_sql, Dictionary<string, object> a_params, int a_retryCount = 3)
        {
            return ExecuteSqlWithRetry(a_sql, (sqlCommand, connection) =>
            {
                return connection
                       .Query<ServerWideInstanceSettings>(sqlCommand.CommandText, param: a_params)
                       .FirstOrDefault();
            }, a_params, a_retryCount);
        }

        private T ExecuteSqlWithRetry<T>(string a_sql, Func<SqlCommand, SqlConnection, T> a_sqlFunc, Dictionary<string, object> a_params = null, int a_retryCount = 3)
        {
            a_retryCount = Math.Max(0, a_retryCount);

            using (SqlConnection connection = new SqlConnection(m_connectionString))
            {
                using (SqlCommand sqlCmd = new SqlCommand(a_sql, connection))
                {
                    if (a_params != null)
                    {
                        foreach (KeyValuePair<string, object> paramKeyValue in a_params)
                        {
                            sqlCmd.Parameters.AddWithValue(paramKeyValue.Key, paramKeyValue.Value);
                        }
                    }

                    while (a_retryCount >= 0)
                    {
                        try
                        {
                            connection.Open();
                            return a_sqlFunc(sqlCmd, connection);
                        }
                        catch (Exception e)
                        {
                            a_retryCount--;
                            connection.Close();

                            if (a_retryCount < 0)
                            {
                                throw;
                            }
                        }
                    }
                }
            }

            // The structure above should guarantee that we have already returned or thrown an exception, so this should never be hit.
            throw new InvalidOperationException("An unexpected error occured when connecting to the database.");
        }
    }
}
