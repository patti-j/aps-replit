using System.Data;

using Microsoft.Data.SqlClient;

using PT.Common.Exceptions;

namespace PT.SchedulerData;

internal class SystemDataTableException : PTHandleableException
{
    internal SystemDataTableException() { }

    public SystemDataTableException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl) { }
}

/// <summary>
/// Reads SystemData table in APS publish database using the provided connection.
/// If table doesn't exist or doesn't have data, it create/populates it.
/// Provides a method for updating the Version field. Call this after database
/// has been synchronize.
/// </summary>
internal class SystemDataTable
{
    // This attempts to read data from sql. If table or row don't exist, it creates them.
    internal SystemDataTable(SqlConnection a_conn)
    {
        try
        {
            DataSet ds = new ("PT");
            SqlDataAdapter da = new ("SELECT Version, ValidateDB, PrepareData, ClearCustomTables FROM SystemData", a_conn);
            da.Fill(ds);

            if (ds.Tables.Count != 1)
            {
                CreateTable(a_conn);
                InsertRow(a_conn);
            }
            else if (ds.Tables[0].Rows.Count == 0)
            {
                InsertRow(a_conn);
            }
            else
            {
                DataRow row = ds.Tables[0].Rows[0];

                m_version = row[0].ToString();
                m_validateDb = (bool)row[1];
                m_prepareData = (bool)row[2];
                m_clearCustomTables = (bool)row[3];
            }
        }
        catch (SqlException)
        {
            try
            {
                DropTable(a_conn);
            }
            catch { }

            CreateTable(a_conn);
            InsertRow(a_conn);
        }
    }

    private static void CreateTable(SqlConnection a_conn)
    {
        try
        {
            const string c_newTableCommand = "CREATE TABLE SystemData (Version nvarchar(13), ValidateDB bit, PrepareData bit, ClearCustomTables bit);";
            SqlCommand newTable = new (c_newTableCommand, a_conn) { CommandTimeout = 0 };
            newTable.ExecuteNonQuery();
        }
        catch (Exception err)
        {
            throw new SystemDataTableException("Was unable to create SystemData table", err);
        }
    }

    private static void DropTable(SqlConnection a_conn)
    {
        const string c_dropTableCommand = "DROP TABLE SystemData;";
        SqlCommand dropTable = new (c_dropTableCommand, a_conn) { CommandTimeout = 0 };
        dropTable.ExecuteNonQuery();
    }

    private void InsertRow(SqlConnection a_conn)
    {
        try
        {
            const string c_newDataCommand = "INSERT INTO SystemData VALUES('{0}', 0, 1, 1);";
            string insertstring = string.Format(c_newDataCommand, m_version);
            SqlCommand insertCommand = new (insertstring, a_conn) { CommandTimeout = 0 };
            insertCommand.ExecuteNonQuery();
        }
        catch (Exception err)
        {
            throw new SystemDataTableException("Was unable to Insert row in SystemData table", err);
        }
    }

    internal void UpdateVersion(SqlConnection a_conn, string a_version)
    {
        try
        {
            string newTableCommand = string.Format("UPDATE SystemData SET Version = '{0}';", a_version);
            SqlCommand newTable = new (newTableCommand, a_conn) { CommandTimeout = 0 };
            newTable.ExecuteNonQuery();
            m_version = a_version;
        }
        catch (Exception err)
        {
            throw new SystemDataTableException("Was unable to update SystemData table", err);
        }
    }

    private string m_version = "2010.1.1.1";

    public string Version => m_version;

    private readonly bool m_validateDb;

    public bool ValidateDb => m_validateDb;

    private readonly bool m_prepareData = true;

    public bool PrepareData => m_prepareData;

    private readonly bool m_clearCustomTables = true;

    public bool ClearCustomTables => m_clearCustomTables;
}