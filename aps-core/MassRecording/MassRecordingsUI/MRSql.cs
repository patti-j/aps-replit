using System.Data;

using PT.Common.Sql.SqlServer;
using PT.SchedulerDefinitions.MassRecordings;

using static MassRecordings.SqlStrings.TableDefinitions;

namespace MassRecordingsUI;

public class MRSql
{
    private readonly DatabaseConnections m_databaseConnection;
    private int m_nextId;

    /// <summary>
    /// Constructor sets up database connection.
    /// </summary>
    /// <param name="a_sqlConnectionString"></param>
    public MRSql(string a_sqlConnectionString)
    {
        m_databaseConnection = new DatabaseConnections(a_sqlConnectionString);
    }

    /// <summary>
    /// Create and initialize required tables for MassRecordings.
    /// </summary>
    public void CreateDatabase()
    {
        m_databaseConnection.SendSQLTransaction(MRSqlStrings.CreateTables());
        m_nextId = GetNextId();
    }

    /// <summary>
    /// Modify MR database HostConfigurations and HostConfigurationMappings data
    /// </summary>
    public int AddNewHostConfig(string a_userName, string a_masterCopyPath)
    {
        m_nextId = GetNextId();
        m_databaseConnection.SendSQLTransaction(new[] { MRSqlStrings.NewHostConfig(m_nextId, a_userName, a_masterCopyPath) });
        return m_nextId;
    }

    public void AddDefaultHostConfig(string a_name, string a_recordingsDirectory, string a_ptComponentsLocation, string a_masterCopyPath, string a_keyFolderPath, bool a_loadCustomization)
    {
        m_nextId = GetNextId();
        m_databaseConnection.SendSQLTransaction(new[] { MRSqlStrings.NewDefaultHostConfig(m_nextId, a_name, a_recordingsDirectory, a_ptComponentsLocation, a_masterCopyPath, a_keyFolderPath, Convert.ToInt32(a_loadCustomization)) });
    }

    public void AddNewHostConfigMap(string a_hostName, int a_id)
    {
        m_databaseConnection.SendSQLTransaction(new[] { MRSqlStrings.NewHostConfigMap(a_hostName, a_id) });
    }

    public void UpdateHostConfigBaseId(long a_baseSessionId, string a_selectedName, int a_selectedId)
    {
        m_databaseConnection.SendSQLTransaction(new[] { MRSqlStrings.ChangeHostConfigBaseId(a_baseSessionId, a_selectedName, a_selectedId) });
    }

    internal DataTable GetAllBaseIdStartLoc()
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.SelectAllRunInstanceIdStartLoc);
    }

    internal DataTable GetRecentBaseIdStartLoc()
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.SelectRecentBaseIds);
    }

    internal DataTable GetAllHostConfigNameIdBaseId()
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetAllHostConfigNameIdBaseId());
    }

    public DataTable GetHostConfigMap(string a_hostName)
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetCurrentHostConfigMap(a_hostName));
    }

    public void UpdateHostConfigMapping(int a_configurationId, string a_hostName)
    {
        m_databaseConnection.SendSQLTransaction(new[] { MRSqlStrings.SetHostConfigMapping(a_configurationId, a_hostName) });
    }

    public void UpdateSelectedConfigData(string a_configName, string a_recordingsDir, string a_masterCopyPath, string a_keyFolderPath, bool a_loadCustomizationOn, double a_playerTimeOutMins, int a_maxRestartCount, int a_id)
    {
        int a_playerTimeOutMS = Convert.ToInt32(TimeSpan.FromMinutes(a_playerTimeOutMins).TotalMilliseconds);
        m_databaseConnection.SendSQLTransaction(new[] { MRSqlStrings.ChangeSelectedConfig(a_configName, a_recordingsDir, a_masterCopyPath, a_keyFolderPath, Convert.ToInt32(a_loadCustomizationOn), a_playerTimeOutMS, a_maxRestartCount, a_id) });
    }

    public void DeleteSelectedConfig(int a_id)
    {
        m_databaseConnection.SendSQLTransaction(new[] { MRSqlStrings.RemoveSelectedConfig(a_id) });
        m_databaseConnection.SendSQLTransaction(new[] { MRSqlStrings.RemoveSelectedHostConfigMap(a_id) });
    }

    /// <summary>
    /// Retrieves Latest Session ID
    /// </summary>
    /// <returns></returns>
    public long GetCurrentSessionId()
    {
        DataTable dt = m_databaseConnection.SelectSQLTable(MRSqlStrings.GetNewestSessionId());
        long instanceId = Convert.ToInt64(dt.Rows[0][0]);
        return instanceId;
    }

    /// <summary>
    /// Returns currently running instance id verifying no endtime
    /// </summary>
    /// <returns>Session Id, or -1 if no end time</returns>
    public long GetCurrentSessionIdUsingEndTime()
    {
        long instanceId = -1;
        RunInstance latestInstance = new ();
        DataTable dt = m_databaseConnection.SelectSQLTable(MRSqlStrings.GetLatestSessionId());
        if (dt != null && dt.Rows.Count > 0)
        {
            if (string.IsNullOrEmpty(dt.Rows[0][latestInstance.EndTime].ToString()))
            {
                instanceId = Convert.ToInt64(dt.Rows[0][latestInstance.InstanceId]);
            }
        }

        return instanceId;
    }

    /// <summary>
    /// Retrieves Number of Players completed from current session
    /// </summary>
    /// <returns></returns>
    public int GetNumberOfPlayersCompleted(long a_sessionId)
    {
        DataTable dt = m_databaseConnection.SelectSQLTable(MRSqlStrings.GetPlayerInstancesCompleted(a_sessionId));
        return dt.Rows.Count;
    }

    /// <summary>
    /// Retrieves total number of players for instance
    /// </summary>
    /// <param name="a_sessionId"></param>
    /// <returns></returns>
    public int GetTotalNumberOfTests(long a_sessionId)
    {
        int totalTests = 0;
        RunInstance latestInstance = new ();
        DataTable dt = m_databaseConnection.SelectSQLTable(MRSqlStrings.GetTotalTests(a_sessionId));
        if (dt != null && dt.Rows.Count > 0)
        {
            if (!string.IsNullOrEmpty(dt.Rows[0][latestInstance.NumberOfRecordings].ToString()))
            {
                totalTests = Convert.ToInt32(dt.Rows[0][latestInstance.NumberOfRecordings].ToString());
            }
        }

        return totalTests;
    }

    /// <summary>
    /// Retrieves number of players currently running
    /// </summary>
    /// <param name="a_sessionId"></param>
    /// <returns></returns>
    public int GetNumberOfPlayersOn(long a_sessionId)
    {
        DataTable dt = m_databaseConnection.SelectSQLTable(MRSqlStrings.GetNumberPlayersOn(a_sessionId));
        return dt.Rows.Count;
    }

    /// <summary>
    /// Retrieves RunInstance EndTime
    /// </summary>
    /// <returns></returns>
    public DataTable GetEndTime(long a_sessionId)
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetRunInstanceEndTime(a_sessionId));
    }

    /// <summary>
    /// Return table with Warnings information from PlayerExceptions and InstanceLogs
    /// </summary>
    /// <param name="a_sessionId"></param>
    /// <returns></returns>
    public DataTable GetMRWarningsTable(long a_sessionId)
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetWarningsTable(a_sessionId));
    }

    /// <summary>
    /// Return number of Players that experienced errors per the PlayerExceptions and InstanceLogs table
    /// </summary>
    /// <param name="a_sessionId"></param>
    /// <returns></returns>
    public int GetErrorRecordings(long a_sessionId)
    {
        DataTable dt = m_databaseConnection.SelectSQLTable(MRSqlStrings.GetErrorNumber(a_sessionId));
        return Convert.ToInt32(dt.Rows[0][0]);
    }

    /// <summary>
    /// Retrieves RunInstance StartTime and RunLocation
    /// </summary>
    /// <param name="a_sessionId"></param>
    /// <returns></returns>
    public DataTable GetInstanceData(long a_sessionId)
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetRunInstanceData(a_sessionId));
    }

    /// <summary>
    /// Retrieves user host configurations data from configuration map
    /// </summary>
    /// <param name="a_hostName"></param>
    /// <returns>All configuration data for user's current configuration mapping</returns>
    public DataTable GetHostConfigInfo(string a_hostName)
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetHostConfigInfo(a_hostName));
    }

    public DataTable GetSelectedConfigData(string a_name, int a_id)
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetSelectedConfig(a_name, a_id));
    }

    /// <summary>
    /// Initialize configuration id
    /// </summary>
    /// <returns>Id value of next HostConfiguration entry</returns>
    private int GetNextId()
    {
        int id = 0;
        HostConfigurations hostConfig = new ();
        string cmdGetNextId = $"Select top 1 * from {hostConfig.TableName} order by id desc";
        DataTable nextId = m_databaseConnection.SelectSQLTable(cmdGetNextId);
        if (nextId != null && nextId.Rows.Count > 0)
        {
            string result = nextId.Rows[0][0].ToString();
            id = Convert.ToInt32(result) + 1;
        }

        return id;
    }

    /// <summary>
    /// Returns  current baseId from database
    /// </summary>
    /// <param name="a_selectedId"></param>
    /// <returns></returns>
    public DataTable GetUpdatedBaseId(int a_selectedId)
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetCurrentSelectedBaseId(a_selectedId));
    }

    /// <summary>
    /// Return selected run instance details
    /// </summary>
    /// <returns></returns>
    public DataTable GetRunInstances()
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetInstances());
    }

    internal DataTable GetTopWarnings(long a_currentSessionId)
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetTopRecordingsWarn(a_currentSessionId));
    }

    internal DataTable GetTopTimes(long a_currentSessionId)
    {
        return m_databaseConnection.SelectSQLTable(MRSqlStrings.GetTopRecordingsTime(a_currentSessionId));
    }
}

public class MRSqlStrings
{
    /// <summary>
    /// Query string to create database tables used by ScenarioDetailReceive.
    /// </summary>
    private static readonly MassRecordingsTableDefinitions.ActionChecksums checksum;

    private static readonly string ActionChecksums = $"IF OBJECT_ID('{checksum.TableName}', 'U') IS NULL CREATE TABLE {checksum.TableName} ({checksum.StartAndEndSums} decimal(38, 0),  {checksum.ResourceJobOperationCombos} decimal(38, 0), {checksum.BlockCount} int, {checksum.ScheduleDescription} nvarchar(MAX), {checksum.NbrOfSimulations} bigint, {checksum.ScenarioId} int not null, {checksum.SessionId} bigint not null, {checksum.PlayerPath} int not null, {checksum.TransmissionType} nvarchar(MAX), {checksum.TransmissionNbr} bigint not null, constraint PK_{checksum.TableName} primary key clustered ({checksum.ScenarioId} asc, {checksum.PlayerPath} asc, {checksum.SessionId} asc, {checksum.TransmissionNbr} asc) with (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

    private static readonly MassRecordingsTableDefinitions.ScheduleIssues s_scheduleIssues;
    private static readonly string s_createScheduleIssuesTable = $"IF OBJECT_ID('{s_scheduleIssues.TableName}', 'U') IS NULL CREATE TABLE {s_scheduleIssues.TableName} ({s_scheduleIssues.ScenarioId} int not null,  {s_scheduleIssues.SessionId} bigint not null, {s_scheduleIssues.PlayerId} int not null, {s_scheduleIssues.TransmissionType} nvarchar(max) null, {s_scheduleIssues.TransmissionNbr} bigint not null, {s_scheduleIssues.LateObjectType} nvarchar(max) not null, {s_scheduleIssues.LateObjectName} nvarchar(max) not null, {s_scheduleIssues.LateObjectId} int not null) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

    /// <summary>
    /// Query string to create database table used by ErrorReporter.
    /// </summary>
    private static MassRecordingsTableDefinitions.InstanceLogs instanceLogs;

    private static readonly string InstanceLogs = $"IF OBJECT_ID('{instanceLogs.TableName}', 'U') IS NULL CREATE TABLE {instanceLogs.TableName} ({instanceLogs.InstanceName} nvarchar(15),  {instanceLogs.SoftwareVersion} nvarchar(25) not null, {instanceLogs.TypeName} nvarchar(MAX), {instanceLogs.Message} nvarchar(MAX), {instanceLogs.StackTrace} nvarchar(MAX), {instanceLogs.Source} nvarchar(MAX), {instanceLogs.InnerExceptionMessage} nvarchar(MAX), {instanceLogs.InnerExceptionStackTrace} nvarchar(MAX), {instanceLogs.LogType} nvarchar(50), {instanceLogs.HeaderMessage} nvarchar(MAX), Index IX_{instanceLogs.TableName} clustered ({instanceLogs.InstanceName} asc, {instanceLogs.SoftwareVersion} asc, {instanceLogs.LogType} asc) with (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

    /// <summary>
    /// Query strings to create database tables used by MassRecordings.
    /// </summary>
    private static HostConfigurationMappings hostConfigMap;

    private static readonly string HostConfigurationMappings = $"IF OBJECT_ID('{hostConfigMap.TableName}', 'U') IS NULL CREATE TABLE {hostConfigMap.TableName} ({hostConfigMap.HostName} nvarchar(50),  {hostConfigMap.ConfigurationId} int)";

    private static HostConfigurations hostConfig;

    private static PlayerExceptions playerException;
    private static readonly string PlayerExceptions = $"IF OBJECT_ID('{playerException.TableName}', 'U') IS NULL CREATE TABLE {playerException.TableName} ({playerException.SessionId} bigint, {playerException.RecordingPath} int not null, {playerException.ExceptionMessage} nvarchar(MAX) default null, {playerException.ExceptionTrace} nvarchar(MAX) default null, {playerException.Error} bit default null, index IX_{playerException.TableName} clustered ({playerException.SessionId} asc, {playerException.RecordingPath} asc, {playerException.Error} asc)with (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

    private static PlayerInstances playerInstance;
    private static readonly string PlayerInstances = $"IF OBJECT_ID('{playerInstance.TableName}', 'U') IS NULL CREATE TABLE {playerInstance.TableName} ({playerInstance.SessionId} bigint not null, {playerInstance.PlayerId} int IDENTITY(1,1) not null, {playerInstance.RecordingPath} nvarchar(MAX), {playerInstance.StartTime} datetime default null, {playerInstance.EndTime} datetime default null, {playerInstance.PeakMemoryUsage} bigint default null, {playerInstance.CpuTime} bigint default null, {playerInstance.CpuUsage} float default null, {playerInstance.ExitCode} int, constraint PK_{playerInstance.TableName} primary key clustered ({playerInstance.SessionId} asc, {playerInstance.PlayerId} asc) with (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

    private static RunInstance runInstance;
    private static readonly string RunInstance = $"IF OBJECT_ID('{runInstance.TableName}', 'U') IS NULL CREATE TABLE {runInstance.TableName} ({runInstance.InstanceId} bigint primary key IDENTITY(1,1) NOT NULL, {runInstance.StartTime} datetime NOT NULL, {runInstance.RunLocation} nvarchar(MAX) NOT NULL, {runInstance.RunMode} nvarchar(MAX) NOT NULL, {runInstance.Configuration} nvarchar(MAX) NOT NULL, {runInstance.EndTime} datetime, {runInstance.NumberOfRecordings} int)";

    /// <summary>
    /// Query strings to modify tables used by MassRecordings.
    /// </summary>
    private const string InsertHostConfig = "Insert into {0} ({1}, {2}, {3}) Values({4}, '{5}', '{6}')";

    private const string InsertDefaultHostConfig = "Insert into {0} ({1}, {2}, {3}, {4}, {5}, {6}) VALUES ({7}, '{8}', '{9}', '{10}', {11}, '{12}', '{13}')";
    private const string InsertHostConfigMap = "Insert into {2} Values('{0}', {1})";
    private const string UpdateHostConfigBaseId = "Update {0} SET {1} = {2} WHERE  {3} = '{4}' AND {5} = {6}";
    private const string CreateHostConfigMapping = "IF EXISTS(SELECT {0} FROM {1} WHERE {2} = '{3}') BEGIN Update {4} set {5} = {6} WHERE HostName = '{7}' END ELSE BEGIN INSERT INTO {8} VALUES('{9}', {10}) END";
    private const string DeleteSelectedHostConfig = "Delete from {0} WHERE {1} = {2}";
    private const string DeleteSelectedHostConfigMap = "Delete from {0} WHERE {1} = {2}";

    /// <summary>
    /// Query strings to get all configuration data
    /// </summary>
    private const string SelectHostConfigInfo = "SELECT * FROM {1} WHERE {2} = (SELECT {3} FROM {4} WHERE {5} = '{0}')";

    private const string SelectAllHostConfigNameIdBaseId = "SELECT {0}, {1}, {2} FROM {3}";
    private const string SelectedHostConfig = "SELECT {0},{1},{2},{3},{4}, {5}, {6}, {7} FROM {8} WHERE {9} = '{10}' AND {11} = {12}";

    public static readonly string SelectAllRunInstanceIdStartLoc = $"SELECT a.*, b.IdCount FROM (SELECT  ri.{runInstance.InstanceId}, ri.{runInstance.StartTime}, ri.{runInstance.RunLocation}, ri.{runInstance.RunMode}, ri.{runInstance.Configuration}, ri.{runInstance.NumberOfRecordings}\n" +
                                                                   $"FROM            (SELECT {playerInstance.SessionId} AS ID\n" +
                                                                   $"                        FROM dbo.{playerInstance.TableName}\n" +
                                                                   $"                        WHERE ({playerInstance.EndTime} IS NULL)\n" +
                                                                   $"                        UNION\n" +
                                                                   $"                        SELECT {playerException.SessionId} AS ID\n" +
                                                                   $"                        FROM dbo.{playerException.TableName}\n" +
                                                                   $"                        WHERE ({playerException.Error} = 1)\n" +
                                                                   $"                        UNION\n" +
                                                                   $"                        SELECT {instanceLogs.InstanceName} AS ID\n" +
                                                                   $"                        FROM dbo.{instanceLogs.TableName}) AS x RIGHT OUTER JOIN\n" +
                                                                   $"                        dbo.{runInstance.TableName} AS ri ON x.ID = ri.{runInstance.InstanceId}\n" +
                                                                   $"WHERE        (x.ID IS NULL)) as a INNER JOIN\n" +
                                                                   $"(SELECT {playerInstance.SessionId} AS ID, count({playerInstance.SessionId}) as IdCount\n" +
                                                                   $"                        FROM dbo.{playerInstance.TableName}\n" +
                                                                   $"						 GROUP BY {playerInstance.SessionId}) as b on a.{runInstance.InstanceId} = b.ID\n" +
                                                                   $"						 WHERE a.{runInstance.NumberOfRecordings} = b.IdCount";

    public static readonly string SelectRecentBaseIds = $"SELECT TOP 10 {runInstance.InstanceId}, {runInstance.StartTime}, {runInstance.RunLocation}, {runInstance.RunMode}, {runInstance.Configuration} FROM {runInstance.TableName} " +
                                                        $"ORDER BY {runInstance.StartTime} DESC";

    private const string SelectCurrentBaseId = "SELECT {0} FROM {1} WHERE {2} = {3}";
    private const string SelectCurrentHostMapping = "SELECT {0} FROM {1} WHERE {2} = '{3}'";
    private const string SelectInstanceData = "SELECT {0}, {1} FROM {2} WHERE {3} = {4}";
    private const string SelectCurrentSessionId = "Select MAX({0}) from {1}";
    private const string SelectCurrentSessionIdEndTime = "SELECT {0}, {1} from {2} where {0} = (Select MAX({0}) from {2})";
    private const string GetPlayerCountCompleted = "SELECT * FROM {0} WHERE {1} = {2} AND {3} IS NOT NULL";
    private const string GetPlayerCountOn = "SELECT * FROM {0} WHERE {1} = {2} AND {3} IS NULL";
    private const string GetTotalTestCount = "SELECT {0} FROM {1} WHERE {2} = {3}";
    private const string GetEndTime = "SELECT {0} FROM {1} WHERE {2} = {3}";

    /// <summary>
    /// Query strings to get analysis data
    /// </summary>
    private static readonly string SelectRunInstances = $"SELECT {runInstance.InstanceId}, {runInstance.RunLocation}, {runInstance.StartTime} FROM {runInstance.TableName} ORDER BY {runInstance.InstanceId} DESC";

    private static readonly string SelectErrorRecordings = "SELECT COUNT(*) FROM (SELECT {0} AS RecordingPath FROM {1} WHERE {2} = {3} AND {4} = 'Exceptions' UNION  SELECT {5} FROM {6} WHERE {7} = {8} AND {9} = 1) AS NUMBER";
    private static string SelectWarningRecordings = "SELECT {0} AS RecordingPath, {1} AS WarningMessage, {2} as WarningType FROM {3} WHERE {4} = {5} AND {6} != 'Exceptions' UNION  SELECT {7}, {8}, 'MRPlayer' as WarningType FROM {9} WHERE {10} = {11} AND {12} = 0";

    public static string GetErrorNumber(long a_sessionId)
    {
        return string.Format(SelectErrorRecordings, instanceLogs.SoftwareVersion, instanceLogs.TableName, instanceLogs.InstanceName, a_sessionId, instanceLogs.LogType, playerException.RecordingPath, playerException.TableName, playerException.SessionId, a_sessionId, playerException.Error);
    }

    public static string GetWarningsTable(long a_sessionId)
    {
        return $"SELECT b.{playerInstance.PlayerId}, c.{playerInstance.RecordingPath}, b.WarningMessage, b.WarningType from " +
               $"(SELECT {instanceLogs.SoftwareVersion} AS PlayerId, {instanceLogs.Message} AS WarningMessage, {instanceLogs.LogType} as WarningType FROM {instanceLogs.TableName} WHERE {instanceLogs.InstanceName} = {a_sessionId} AND {instanceLogs.LogType} != 'Exceptions' " +
               $"UNION  SELECT {playerException.RecordingPath} AS PlayerId, {playerException.ExceptionMessage}, 'MRPlayer' as WarningType FROM {playerException.TableName} WHERE {playerException.SessionId} = {a_sessionId} AND {playerException.Error} = 0) as b " +
               $"INNER JOIN {playerInstance.TableName} as c on c.{playerInstance.PlayerId} = b.{playerInstance.PlayerId} ";
    }

    /// <summary>
    /// Prepare query string for database creation
    /// </summary>
    /// <returns></returns>
    public static string[] CreateTables()
    {
        string[] tablesToAdd = { ActionChecksums, HostConfigurationMappings, GetHostConfigurations(), InstanceLogs, PlayerExceptions, PlayerInstances, RunInstance, s_createScheduleIssuesTable };

        return tablesToAdd;
    }

    /// <summary>
    /// Get create HostConfigurations data table SQL statement
    /// </summary>
    /// <returns></returns>
    public static string GetHostConfigurations()
    {
        string defaultPath = "C:\\MassRecordings";
        string recDir = Path.Combine(defaultPath, "TestRecordings");
        string PTCompLoc = "D:\\TFS\\APS\\APS\\APS12";
        string KeyFolderPath = Path.Combine(defaultPath, "Key");
        int timeOut = Convert.ToInt32(TimeSpan.FromMinutes(20).TotalMilliseconds);
        int maxRestartCount = 3;

        HostConfigurations hostConfig = new ();
        string HostConfigurations = $"IF OBJECT_ID('{hostConfig.TableName}', 'U') IS NULL CREATE TABLE {hostConfig.TableName} ({hostConfig.Id} int, {hostConfig.Name} nvarchar(MAX) " +
                                    $", {hostConfig.RecordingsDirectory} nvarchar(MAX) default '{recDir}', {hostConfig.RunMode} nvarchar(MAX) default 'MANUAL'" +
                                    $", {hostConfig.BaseSessionId} bigint default 0, {hostConfig.PlayerTimeOutMS} int default {timeOut}, {hostConfig.MaxRestartCount} int default {maxRestartCount}, {hostConfig.LoadCustomization} bit default 1, {hostConfig.MasterCopyPath} nvarchar(MAX) " +
                                    $", {hostConfig.RequiredFreeMemory} float default 0, {hostConfig.KeyFolderPath} nvarchar(MAX) default '{KeyFolderPath}')";
        return HostConfigurations;
        //const string HostConfigurations = "IF OBJECT_ID('{0}', 'U') IS NULL CREATE TABLE {1} ({2} int, {3} nvarchar(MAX) default {4}, {5} nvarchar(MAX) default {6}, {7} nvarchar(MAX) default {8}, {9} nvarchar(MAX) default {10}, {hostConfig.BaseSessionId} bigint, {hostConfig.LoadCustomization} bit, {hostConfig.MasterCopyPath} nvarchar(MAX), {hostConfig.RequiredFreeMemory} float, {hostConfig.KeyFolderPath} nvarchar(MAX))";
        //return string.Format(HostConfigurations, hostConfig.TableName, hostConfig.TableName, hostConfig.Id, hostConfig.Name, a_userName, hostConfig.RecordingsDirectory, a_recordingsDirectory, hostConfig.PTComponentsLocation, a_PTComponentsLocation, hostConfig.RunMode, "MANUAL", );
    }

    /// <summary>
    /// Prepare MR Database Modification Strings with values
    /// </summary>
    internal static string NewDefaultHostConfig(int m_nextId, string a_name, string a_recordingsDirectory, string a_ptComponentsLocation, string a_masterCopyPath, string a_keyFolderPath, int a_loadCustomization)
    {
        return string.Format(InsertDefaultHostConfig, hostConfig.TableName.CleanString(), hostConfig.Id, hostConfig.Name.CleanString(), hostConfig.RecordingsDirectory.CleanString(), hostConfig.LoadCustomization.CleanString(), hostConfig.MasterCopyPath.CleanString(), hostConfig.KeyFolderPath.CleanString(), m_nextId, a_name.CleanString(), a_recordingsDirectory.CleanString(), a_ptComponentsLocation.CleanString(), a_loadCustomization, a_masterCopyPath.CleanString(), a_keyFolderPath.CleanString());
    }

    internal static string NewHostConfig(int a_id, string a_userName, string a_masterCopyPath)
    {
        return string.Format(InsertHostConfig, hostConfig.TableName, hostConfig.Id, hostConfig.Name, hostConfig.MasterCopyPath, a_id, a_userName.CleanString(), a_masterCopyPath.CleanString());
    }

    internal static string NewHostConfigMap(string a_hostName, int a_configurationId)
    {
        return string.Format(InsertHostConfigMap, a_hostName.CleanString(), a_configurationId, hostConfigMap.TableName.CleanString());
    }

    internal static string ChangeHostConfigBaseId(long a_baseSessionId, string a_selectedName, int a_selectedId)
    {
        return string.Format(UpdateHostConfigBaseId, hostConfig.TableName.CleanString(), hostConfig.BaseSessionId.CleanString(), a_baseSessionId, hostConfig.Name.CleanString(), a_selectedName, hostConfig.Id.CleanString(), a_selectedId);
    }

    internal static string SetHostConfigMapping(int a_configurationId, string a_hostName)
    {
        return string.Format(CreateHostConfigMapping, hostConfigMap.ConfigurationId, hostConfigMap.TableName.CleanString(), hostConfigMap.HostName.CleanString(), a_hostName.CleanString(), hostConfigMap.TableName.CleanString(), hostConfigMap.ConfigurationId.CleanString(), a_configurationId, a_hostName.CleanString(), hostConfigMap.TableName.CleanString(), a_hostName.CleanString(), a_configurationId);
    }

    internal static string ChangeSelectedConfig(string a_configName, string a_recordingsDir, string a_masterCopyPath, string a_keyFolderPath, int a_loadCustomization, int a_playerTimeOutMS, int a_maxRestartCount, int a_id)
    {
        return $"Update {hostConfig.TableName} SET {hostConfig.Name} = '{a_configName}', {hostConfig.RecordingsDirectory} = '{a_recordingsDir}', {hostConfig.MasterCopyPath} = '{a_masterCopyPath}', {hostConfig.KeyFolderPath} = '{a_keyFolderPath}', {hostConfig.LoadCustomization} = {a_loadCustomization}, {hostConfig.PlayerTimeOutMS} = {a_playerTimeOutMS}, {hostConfig.MaxRestartCount} = {a_maxRestartCount} WHERE {hostConfig.Id} = {a_id}";
    }

    internal static string RemoveSelectedConfig(int a_id)
    {
        return string.Format(DeleteSelectedHostConfig, hostConfig.TableName.CleanString(), hostConfig.Id.CleanString(), a_id);
    }

    internal static string RemoveSelectedHostConfigMap(int a_id)
    {
        return string.Format(DeleteSelectedHostConfigMap, hostConfigMap.TableName.CleanString(), hostConfigMap.ConfigurationId, a_id);
    }

    /// <summary>
    /// Get String to Select newest SessionId
    /// </summary>
    internal static string GetNewestSessionId()
    {
        return string.Format(SelectCurrentSessionId, runInstance.InstanceId, runInstance.TableName);
    }

    /// <summary>
    /// String format to select current running session
    /// </summary>
    /// <returns></returns>
    internal static string GetLatestSessionId()
    {
        return string.Format(SelectCurrentSessionIdEndTime, runInstance.InstanceId, runInstance.EndTime, runInstance.TableName);
    }

    /// <summary>
    /// Get String to Select Player Count
    /// </summary>
    internal static string GetPlayerInstancesCompleted(long a_sessionId)
    {
        return string.Format(GetPlayerCountCompleted, playerInstance.TableName, playerInstance.SessionId, a_sessionId, playerInstance.EndTime);
    }

    /// <summary>
    /// Select string for total test methods for session instance
    /// </summary>
    /// <param name="a_sessionId"></param>
    /// <returns></returns>
    internal static string GetTotalTests(long a_sessionId)
    {
        return string.Format(GetTotalTestCount, runInstance.NumberOfRecordings, runInstance.TableName, runInstance.InstanceId, a_sessionId);
    }

    /// <summary>
    /// Select total players instances running for session
    /// </summary>
    /// <param name="a_sessionId"></param>
    /// <returns></returns>
    internal static string GetNumberPlayersOn(long a_sessionId)
    {
        return string.Format(GetPlayerCountOn, playerInstance.TableName, playerInstance.SessionId, a_sessionId, playerInstance.EndTime);
    }

    /// <summary>
    /// Get String to Select Player EndTime
    /// </summary>
    internal static string GetRunInstanceEndTime(long a_sessionId)
    {
        return string.Format(GetEndTime, runInstance.EndTime, runInstance.TableName, runInstance.InstanceId, a_sessionId);
    }

    /// <summary>
    /// Get Sql String for Select Instance data
    /// </summary>
    /// <param name="a_sessionId"></param>
    /// <returns></returns>
    internal static string GetRunInstanceData(long a_sessionId)
    {
        return string.Format(SelectInstanceData, runInstance.StartTime, runInstance.RunLocation, runInstance.TableName, runInstance.InstanceId, a_sessionId);
    }

    /// <summary>
    /// Prepare MR Database Confirmation String with values
    /// </summary>
    internal static string GetHostConfigInfo(string a_hostName)
    {
        return string.Format(SelectHostConfigInfo, a_hostName.CleanString(), hostConfig.TableName.CleanString(), hostConfig.Id.CleanString(), hostConfigMap.ConfigurationId.CleanString(), hostConfigMap.TableName.CleanString(), hostConfigMap.HostName.CleanString());
    }

    internal static string GetAllHostConfigNameIdBaseId()
    {
        return string.Format(SelectAllHostConfigNameIdBaseId, hostConfig.Name.CleanString(), hostConfig.Id.CleanString(), hostConfig.BaseSessionId, hostConfig.TableName.CleanString());
    }

    internal static string GetSelectedConfig(string a_name, int a_id)
    {
        return string.Format(SelectedHostConfig, hostConfig.Name, hostConfig.RecordingsDirectory, hostConfig.MasterCopyPath, hostConfig.KeyFolderPath, hostConfig.LoadCustomization, hostConfig.BaseSessionId, hostConfig.PlayerTimeOutMS, hostConfig.MaxRestartCount, hostConfig.TableName, hostConfig.Name, a_name, hostConfig.Id, a_id);
    }

    internal static string GetCurrentSelectedBaseId(int a_selectedId)
    {
        return string.Format(SelectCurrentBaseId, hostConfig.BaseSessionId, hostConfig.TableName, hostConfig.Id, a_selectedId);
    }

    internal static string GetCurrentHostConfigMap(string a_hostName)
    {
        return string.Format(SelectCurrentHostMapping, hostConfigMap.ConfigurationId, hostConfigMap.TableName, hostConfigMap.HostName, a_hostName);
    }

    internal static string GetInstances()
    {
        return SelectRunInstances;
    }

    internal static string GetTopRecordingsWarn(long a_sessionId)
    {
        return $"SELECT top 5 c.{playerInstance.RecordingPath}, b.WarningCount from " +
               $"(Select a.{playerInstance.PlayerId}, Count(a.{playerInstance.PlayerId}) as WarningCount from " +
               $"(SELECT {instanceLogs.SoftwareVersion} AS {playerInstance.PlayerId}, {instanceLogs.Message} AS WarningMessage, {instanceLogs.LogType} as WarningType FROM {instanceLogs.TableName} " +
               $"WHERE {instanceLogs.InstanceName} = {a_sessionId} AND {instanceLogs.LogType} != 'Exceptions' UNION " +
               $"SELECT {playerException.RecordingPath} as PlayerId, {playerException.ExceptionMessage}, 'MRPlayer' as WarningType FROM {playerException.TableName} WHERE {playerException.SessionId} = {a_sessionId} AND {playerException.Error} = 0) as a group by {playerInstance.PlayerId} ) as b " +
               $"join {playerInstance.TableName} as c on c.{playerInstance.PlayerId} = b.{playerInstance.PlayerId} order by WarningCount desc";
    }

    internal static string GetTopRecordingsTime(long a_sessionId)
    {
        return $"Select top 5 {playerInstance.RecordingPath}, DateDiff(ss, {playerInstance.TableName}.{playerInstance.StartTime}, {playerInstance.TableName}.{playerInstance.EndTime}) as Seconds from {playerInstance.TableName} join {runInstance.TableName} on {playerInstance.TableName}.{playerInstance.SessionId} = {runInstance.TableName}.{runInstance.InstanceId} where {runInstance.TableName}.{runInstance.InstanceId} = {a_sessionId} order by Seconds desc";
    }
}