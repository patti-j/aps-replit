using System.Data;

using PT.Common.SqlServer;

namespace MassRecordings;

public class MRdbInstallation
{
    public bool InstallVerification;

    public PT.SchedulerDefinitions.MassRecordings.MassRecordingsTableDefinitions.ActionChecksums ActCheckSums;
    public PT.SchedulerDefinitions.MassRecordings.MassRecordingsTableDefinitions.ScheduleIssues ScheduleIssues;

    public SqlStrings.TableDefinitions.HostConfigurationMappings HostConfigMaps;
    public SqlStrings.TableDefinitions.HostConfigurations HostConfig;
    public PT.SchedulerDefinitions.MassRecordings.MassRecordingsTableDefinitions.InstanceLogs InstLogs;
    public SqlStrings.TableDefinitions.PlayerExceptions PlayerExceptions;
    public SqlStrings.TableDefinitions.PlayerInstances PlayerInstances;
    public SqlStrings.TableDefinitions.RunInstance RunInstance;

    private static DatabaseConnections m_dbConnector;
    private static string m_UIPath;

    public MRdbInstallation()
    {
        SimpleConfiguration configuration = new ();
        string DBConnectionString = configuration.LoadValue("DBConnectionString");
        m_UIPath = configuration.LoadValue("UIPath");
        m_dbConnector = new DatabaseConnections(DBConnectionString);
        InstallVerification = ConfirmInstall();
    }

    private bool ConfirmInstall()
    {
        List<string> tableNames = new () { ActCheckSums.TableName, ScheduleIssues.TableName, HostConfigMaps.TableName, HostConfig.TableName, InstLogs.TableName, PlayerExceptions.TableName, PlayerInstances.TableName, RunInstance.TableName };
        DataTable tables = VerifyInstall();
        foreach (string name in tableNames)
        {
            if (!TableExists(tables, name))
            {
                return false;
            }
        }

        return true;
    }

    private DataTable VerifyInstall()
    {
        return m_dbConnector.SelectSQLTable(SqlStrings.GetHostConfigTables());
    }

    /// <summary>
    /// Verifies if required database table installed.
    /// </summary>
    /// <param name="a_dbTables"></param>
    /// <param name="a_name"></param>
    /// <returns>true if table exists in database, false if does not</returns>
    private bool TableExists(DataTable a_dbTables, string a_name)
    {
        if (a_dbTables == null)
        {
            return false;
        }

        foreach (DataRow row in a_dbTables.Rows)
        {
            if (row[0].ToString() == a_name)
            {
                return true;
            }
        }

        return false;
    }
}