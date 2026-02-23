using System.Data;

using Microsoft.Data.SqlClient;

namespace PT.Common.Sql.SqlServer;

//TODO Move to new file when Common project is not checked out.
public static class Filtering
{
    public static string FilterString(string a_sourceString)
    {
        if (a_sourceString == null)
        {
            return "null";
        }

        return a_sourceString.Replace("'", "''");
    }

    public static string CleanString(this string a_sourceString)
    {
        if (a_sourceString == null)
        {
            return "null";
        }

        return a_sourceString.Replace("'", "''");
    }
}

public class DatabaseConnections
{
    public DatabaseConnections(string a_connectionString)
    {
        m_connectionString = a_connectionString;
    }

    private readonly string m_connectionString;

    /// <summary>
    /// Builds an SQL transaction from the sqlCommands list and Commits it.
    /// Affected rows must be => the number of sql actions for a success.
    /// </summary>
    /// <param name="a_sqlCommands">List of SQL Non queries</param>
    /// <param name="a_ignoreAffectedRowCount">Number of statements that may return 0 affected rows and should not be checked </param>
    /// <returns>bool success</returns>
    public bool SendSQLTransaction(string[] a_sqlCommands, int a_ignoreAffectedRowCount = 0, int a_retryCount = 3)
    {
        SqlTransaction transaction = null;
        SqlConnection connection = null;
        int affectedRows = 0;
        a_retryCount = Math.Max(0, a_retryCount);
        try
        {
            connection = new SqlConnection(m_connectionString);
            connection.Open();
            while (a_retryCount >= 0)
            {
                try
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    using (SqlCommand sqlCmd = new ("", connection))
                    {
                        sqlCmd.Transaction = transaction;
                        for (int i = 0; i < a_sqlCommands.Length; i++)
                        {
                            sqlCmd.CommandText = a_sqlCommands[i];
                            if (sqlCmd.ExecuteNonQuery() > 0)
                            {
                                affectedRows++;
                            }
                        }
                    }

                    transaction.Commit();
                    break;
                }
                catch (SqlException e) when (e.Number == -2 || e.Number == 1205 || e.Number == 11)
                {
                    transaction?.Rollback();
                    a_retryCount--;
                }
            }
        }
        catch
        {
            transaction?.Rollback();
            //Send the exception back to webmethods so exception message isn't lost.
            throw;
        }
        finally
        {
            connection?.Dispose(); //it is also closed
        }

        if (affectedRows + a_ignoreAffectedRowCount < a_sqlCommands.Length)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Builds an SQL transaction from the sqlCommands list and Commits it.
    /// Affected rows must be => the number of sql actions for a success.
    /// </summary>
    /// <param name="a_sqlCommands">List of SQL Non queries and any paramaters</param>
    /// <param name="a_ignoreAffectedRowCount">Number of statements that may return 0 affected rows and should not be checked </param>
    /// <returns>bool success</returns>
    public async Task<bool> SendSQLTransactionAsync(SqlCommandHelper[] a_sqlCommands, int a_ignoreAffectedRowCount = 0, int a_retryCount = 3)
    {
        SqlTransaction transaction = null;
        int affectedRows = 0;
        a_retryCount = Math.Max(0, a_retryCount);
        try
        {
            await using (SqlConnection connection = new SqlConnection(m_connectionString))
            {
                await connection.OpenAsync();
                while (a_retryCount >= 0)
                {
                    try
                    {
                        transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                        await using (SqlCommand sqlCmd = connection.CreateCommand())
                        {
                            sqlCmd.Transaction = transaction;
                            foreach (SqlCommandHelper commandHelper in a_sqlCommands)
                            {
                                commandHelper.SetCommand(sqlCmd);
                                if (await sqlCmd.ExecuteNonQueryAsync() > 0)
                                {
                                    affectedRows++;
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        break;
                    }
                    catch (SqlException e) when (e.Number == -2 || e.Number == 1205 || e.Number == 11)
                    {
                        await transaction?.RollbackAsync();
                        a_retryCount--;
                    }
                }
            }
        }
        catch
        {
            transaction?.Rollback();
            //Send the exception back to webmethods so exception message isn't lost.
            throw;
        }

        if (affectedRows + a_ignoreAffectedRowCount < a_sqlCommands.Length)
        {
            return false;
        }

        return true;
    }


    public int InsertSQLRowAndReturnValue(string[] a_sqlCommand, string a_returnValueCmd, int a_retryCount = 5)
    {
        SqlTransaction transaction = null;
        SqlConnection connection = null;
        DataTable dt = null;
        int playerId = 0;
        a_retryCount = Math.Max(0, a_retryCount);
        try
        {
            connection = new SqlConnection(m_connectionString);
            connection.Open();
            while (a_retryCount >= 0)
            {
                try
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
                    using (SqlCommand sqlCmd = new ("", connection))
                    {
                        sqlCmd.Transaction = transaction;
                        for (int i = 0; i < a_sqlCommand.Length; i++)
                        {
                            sqlCmd.CommandText = a_sqlCommand[i];
                            sqlCmd.ExecuteNonQuery();
                        }
                    }

                    dt = new DataTable();
                    using (SqlCommand sqlCmd = new ("", connection))
                    {
                        sqlCmd.Transaction = transaction;
                        sqlCmd.CommandText = a_returnValueCmd;
                        SqlDataAdapter da = new (sqlCmd);
                        da.Fill(dt);
                        playerId = Convert.ToInt32(dt.Rows[0][0]);
                    }

                    transaction.Commit();
                    break;
                }
                catch (SqlException e) when (e.Number == -2 || e.Number == 1205 || e.Number == 11)
                {
                    transaction?.Rollback();
                    a_retryCount--;
                }
            }
        }
        catch
        {
            transaction?.Rollback();
            //Send the exception back to webmethods so exception message isn't lost.
            throw;
        }
        finally
        {
            connection?.Dispose(); //it is also closed
        }

        return playerId;
    }

    /// <summary>
    /// Builds an SQL transaction from the sqlCommands list and Commits it.
    /// Affected rows must be => the number of sql actions for a success.
    /// </summary>
    /// <param name="a_sqlCommands">List of SQL Non queries</param>
    /// <param name="a_ignoreAffectedRowCount">Number of statements that may return 0 affected rows and should not be checked </param>
    /// <returns>bool success</returns>
    public DataTable SelectSQLTable(string a_sqlCommand, int a_retryCount = 3)
    {
        SqlTransaction transaction = null;
        SqlConnection connection = null;
        DataTable dt = null;
        a_retryCount = Math.Max(0, a_retryCount);
        try
        {
            connection = new SqlConnection(m_connectionString);
            connection.Open();
            while (a_retryCount >= 0)
            {
                try
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    dt = new DataTable();
                    using (SqlCommand sqlCmd = new ("", connection))
                    {
                        sqlCmd.Transaction = transaction;
                        sqlCmd.CommandText = a_sqlCommand;
                        SqlDataAdapter da = new (sqlCmd);
                        da.Fill(dt);
                    }

                    transaction.Commit();
                    break;
                }
                catch (SqlException e) when (e.Number == -2 || e.Number == 1205 || e.Number == 11)
                {
                    transaction?.Rollback();
                    a_retryCount--;
                }
            }
        }
        catch
        {
            transaction?.Rollback();
            //Send the exception back to webmethods so exception message isn't lost.
            throw;
        }
        finally
        {
            connection?.Dispose(); //it is also closed
        }

        return dt;
    }

    /// <summary>
    /// Runs a Select Statement on the database and returns a DataSet
    /// This does not manage exceptions.
    /// </summary>
    /// <param name="a_sqlSelectCmd">[string] Select Command</param>
    public DataSet SelectSQLSet(string a_sqlSelectCmd)
    {
        //Compare Data
        DataSet ds;
        ds = new DataSet("PT");
        SqlDataAdapter da = new (a_sqlSelectCmd, m_connectionString);
        da.Fill(ds);

        return ds;
    }

    public static string RemovePasswordPortionFromConnectionString(string a_originalConnectionString)
    {
        SqlConnectionStringBuilder tempBuilder;

        tempBuilder = new SqlConnectionStringBuilder(a_originalConnectionString);
        tempBuilder.Remove("Password");
        return tempBuilder.ConnectionString;
    }

    public static bool CheckTableExistance(SqlConnection a_conn, string a_tableName)
    {
        string existanceCmd = $@"IF EXISTS (SELECT object_id FROM sys.tables where name = '{a_tableName}') SELECT 1 ELSE SELECT 0";
        try
        {
            using (SqlCommand sqlCmd = new (existanceCmd, a_conn))
            {
                return sqlCmd.ExecuteScalar().ToString() == "1";
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    /// <summary>
    /// Whether opening this connection could be successful.
    /// Invalid DataSource will return false
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(m_connectionString))
            {
                conn.Open();
                return true;
            }
        }
        catch (SqlException)
        {
            // Connection string is valid but connection failed (bad server, login, etc.)
            return false;
        }
        catch (InvalidOperationException)
        {
            // Bad format or invalid state
            return false;
        }
    }
}