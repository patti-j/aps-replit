using System.Data;
using System.Globalization;

using Microsoft.Data.SqlClient;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.File;
using PT.Common.Sql.SqlServer;
using PT.ServerManagerSharedLib.Definitions;
using PT.SystemDefinitions.Interfaces;

namespace PT.Scheduler;

internal class PackageLogger
{
    private ISystemLogger m_systemLogger;
    private string m_logDBConnectionString = "";

    public string LogDBConnectionString
    {
        get => m_logDBConnectionString;
        set => m_logDBConnectionString = value;
    }
    public bool IsValidConnection { get; private set; }

    private BaseId m_instanceId;

    private string InstanceId
    {
        get => m_logDBConnectionString;
        set => m_logDBConnectionString = value;
    }

    private string m_instanceName;
    private string m_softwareVersion;

    public PackageLogger(ISystemLogger a_systemLogger)
    {
        m_systemLogger = a_systemLogger;
    }

    public void SetPackageLoggingVals(string a_connectionString, string a_instanceName, string a_softwareVersion)
    {
        LogDBConnectionString = a_connectionString;
        m_instanceName = a_instanceName.CleanString();
        m_softwareVersion = a_softwareVersion.CleanString();

        CheckAndCreatePackageLogTable();
    }

    private void CheckAndCreatePackageLogTable()
    {
        if (string.IsNullOrWhiteSpace(LogDBConnectionString) ||
            !new DatabaseConnections(LogDBConnectionString).IsValid())
        {
            // AuditDb not in use; calls to LogTransmissionToSQL will not run.
            IsValidConnection = false;
            return;
        }

        using (SqlConnection conn = new (LogDBConnectionString))
        {
            DataTable logTable = CreatePackageLoggerDataTable();
            try
            {
                conn.Open();
                SyncLogTableSchema(logTable, conn);

                IsValidConnection = true;
                return;
            }
            catch (SqlException e)
            {
                PTException ptException = new PTException("4074", new object[] { e.Server, "PackageLogger", e.Message });
                m_systemLogger.LogException(ptException.GenerateDescriptionInfo(), ELogClassification.PtSystem, true);
            }
            catch (Exception e)
            {
                PTException ptException = new PTException("4074", new object[] { "", "PackageLogger", e.Message });
                m_systemLogger.LogException(ptException.GenerateDescriptionInfo(), ELogClassification.PtSystem, true);
            }

            IsValidConnection = false;

        }
    }

    private static DataTable CreatePackageLoggerDataTable()
    {
        DataTable logTable = new() { TableName = "PackageLog" };
        logTable.Columns.Add("InstanceName", typeof(string));
        logTable.Columns.Add("SoftwareVersion", typeof(string));
        logTable.Columns.Add("PackageName", typeof(string));
        logTable.Columns.Add("PackageVersion", typeof(string));
        logTable.Columns.Add("Timestamp", typeof(DateTime));
        return logTable;
    }

    private string PreparePackageInsertCmd(PackageLog a_log)
    {
        string insertCommandSql = $@"INSERT INTO PackageLog (InstanceName, SoftwareVersion, PackageName, PackageVersion, Timestamp) VALUES ('{m_instanceName}', '{m_softwareVersion}', '{a_log.m_packageName}', '{a_log.m_packageVersion}', '{a_log.m_timestamp.ToString("s", CultureInfo.InvariantCulture)}' )";
        return insertCommandSql;
    }

    public void LogPackageToSQL(AssemblyPackageInfo[] a_packageInfos)
    {

        if (!IsValidConnection)
        {
            return;
        }

        DatabaseConnections dbConnector = new(LogDBConnectionString);
        List<string> insertCommands = new();
        DateTime timestamp = DateTime.UtcNow; // set once for entire batch

        foreach (AssemblyPackageInfo packageInfo in a_packageInfos)
        {
            PackageLog logEntry = new (packageInfo.AssemblyTitleName, packageInfo.Version, timestamp);
            insertCommands.Add(PreparePackageInsertCmd(logEntry));
        }

        if (insertCommands.Count > 0)
        {
            // Submitting these as a single command so the default timestamp matches
            dbConnector.SendSQLTransaction(insertCommands.ToArray());
        }
    }

    private static void SyncLogTableSchema(DataTable logTable, SqlConnection conn)
    {
        DataSet dataSet = new DataSet();
        dataSet.Tables.Add(logTable);
        DatabaseSynchronizer.AlterDbStructureToMatchDataSet(conn, dataSet, false);
    }
}

internal class PackageLog
{
    internal string m_packageName;
    internal string m_packageVersion;
    internal DateTime m_timestamp;

    internal PackageLog(string a_packageName, string a_packageVersion, DateTime a_timestamp)
    {
        m_packageName = Filtering.FilterString(a_packageName);
        m_packageVersion = Filtering.FilterString(a_packageVersion);
        m_timestamp = a_timestamp;
    }
}