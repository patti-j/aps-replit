namespace MassRecordings;

/// <summary>
/// Query strings used by MR to interact with the database.
/// </summary>
public class SqlStrings
{
    public static string GetHostConfigTable(string a_machineName)
    {
        TableDefinitions.HostConfigurations hostConfigDt = new ();
        TableDefinitions.HostConfigurationMappings hostConfigMapDt = new ();

        //Select Name from HostConfigurations join HostConfigurationMappings on HostConfigurations.Id = HostConfigurationMappings.ConfigurationId and HostConfigurationMappings.HostName = {a_machineName}
        const string c_configName = "Select * from {0} join {1} on {2}.{3} = {4}.{5} and {6}.{7} = '{8}'";

        string format = string.Format(c_configName, hostConfigDt.TableName, hostConfigMapDt.TableName, hostConfigDt.TableName, hostConfigDt.Id, hostConfigMapDt.TableName, hostConfigMapDt.ConfigurationId, hostConfigMapDt.TableName, hostConfigMapDt.HostName, a_machineName);

        return format;
    }

    public static string GetConfigName(string a_machineName)
    {
        TableDefinitions.HostConfigurations hostConfigDt = new ();
        TableDefinitions.HostConfigurationMappings hostConfigMapDt = new ();

        //Select Name from HostConfigurations join HostConfigurationMappings on HostConfigurations.Id = HostConfigurationMappings.ConfigurationId and HostConfigurationMappings.HostName = {a_machineName}
        const string c_configName = "Select {0} from {1} join {2} on {3}.{4} = {5}.{6} and {7}.{8} = '{9}'";

        string format = string.Format(c_configName, hostConfigDt.Name, hostConfigDt.TableName, hostConfigMapDt.TableName, hostConfigDt.TableName, hostConfigDt.Id, hostConfigMapDt.TableName, hostConfigMapDt.ConfigurationId, hostConfigMapDt.TableName, hostConfigMapDt.HostName, a_machineName);

        return format;
    }

    public static string GetInsertPlayerExceptions(long a_sessionId, int a_recordingPath, string a_Message, string a_exceptionTrace, bool a_error)
    {
        const string c_InsertPlayerExceptions = "Insert into {0} Values({1}, {2}, '{3}','{4}', '{5}')";
        TableDefinitions.PlayerExceptions playerExceptionsDt = new ();
        return string.Format(c_InsertPlayerExceptions, playerExceptionsDt.TableName, a_sessionId, a_recordingPath, a_Message, a_exceptionTrace, a_error);
    }

    public static string GetSelectLastSessionid(string a_runMode, string a_computerName)
    {
        #if DEBUG
        const string c_selectLastSessionid = "Select Max({0}) from {1} where {2} = '{3}' and {4} = '{5}' and {6} = 'DEBUG'";
        #else
            const string c_selectLastSessionid = "Select Max({0}) from {1} where {2} = '{3}' and {4} = '{5}' and {6} = 'RELEASE'";
        #endif

        TableDefinitions.RunInstance runInstanceDt = new ();
        return string.Format(c_selectLastSessionid, runInstanceDt.InstanceId, runInstanceDt.TableName, runInstanceDt.RunMode, a_runMode, runInstanceDt.RunLocation, a_computerName, runInstanceDt.Configuration);
    }

    public static string GetSelectSecondLastSessionid(string a_runMode, string a_computerName)
    {
        #if DEBUG
        const string c_selectSecondLastSessionid = "Select Max({0}) from {1} where {2} = '{3}' and {4} = '{5}' and {6} = 'DEBUG' and {7} NOT IN (SELECT MAX({8}) from {9} where {10} = '{11}' and {12} = '{13}' and {14} = 'DEBUG')";
        #else
            const string c_selectSecondLastSessionid = "Select Max({0}) from {1} where {2} = '{3}' and {4} = '{5}' and {6} = 'RELEASE' aand {7} NOT IN (SELECT MAX({8}) from {9} where {10} = '{11}' and {12} = '{13}' and {14} = 'RELEASE')";
        #endif
        TableDefinitions.RunInstance runInstanceDt = new ();
        return string.Format(c_selectSecondLastSessionid, runInstanceDt.InstanceId, runInstanceDt.TableName, runInstanceDt.RunMode, a_runMode, runInstanceDt.RunLocation, a_computerName, runInstanceDt.Configuration, runInstanceDt.InstanceId, runInstanceDt.InstanceId, runInstanceDt.TableName, runInstanceDt.RunMode, a_runMode, runInstanceDt.RunLocation, a_computerName, runInstanceDt.Configuration);
    }

    public static string GetUpdateInstanceBase(DateTime a_endTime, long a_peakMemoryUsage, long a_CpuTime, float a_CpuUsage, long a_sessionId, int a_playerId)
    {
        const string c_updateInstanceBase = "Update {0} SET {1} = '{2}', {3} = {4}, {5} = {6}, {7} = {8} WHERE {9} = {10} and {11} = {12}";
        TableDefinitions.PlayerInstances playerInstancesDt = new ();
        return string.Format(c_updateInstanceBase, playerInstancesDt.TableName, playerInstancesDt.EndTime, a_endTime, playerInstancesDt.PeakMemoryUsage, a_peakMemoryUsage, playerInstancesDt.CpuTime, a_CpuTime, playerInstancesDt.CpuUsage, a_CpuUsage, playerInstancesDt.SessionId, a_sessionId, playerInstancesDt.PlayerId, a_playerId);
    }

    public static string GetHostConfigTables()
    {
        return "SELECT name FROM sys.Tables where not name = 'sysdiagrams'";
    }

    //public static string getInsertRunInstance(long a_sessionId, string a_recordingPath, bool a_started)
    //{
    //    string InsertRunInstance;
    //    if (a_started)
    //    {
    //        InsertRunInstance = "Insert into {0} Values({1}, '{2}', 'TRUE')";
    //    }
    //    else
    //    {
    //        InsertRunInstance = "Insert into {0} Values({1}, '{2}', 'FALSE')";
    //    }
    //    TableDefinitions.RecordingInstances runInstancesDt = new TableDefinitions.RecordingInstances();
    //    return string.Format(InsertRunInstance, runInstancesDt.TableName, a_sessionId, a_recordingPath);
    //}
    public class TableDefinitions
    {
        public struct HostConfigurations
        {
            public string TableName => "HostConfigurations";
            public string Name => "Name";
            public string Id => "Id";
            public string RecordingsDirectory => "RecordingsDirectory";
            public string RunMode => "RunMode";
            public string BaseSessionId => "BaseSessionId";
            public string PlayerTimeOutMS => "PlayerTimeOutMS";
            public string MaxRestartCount => "MaxRestartCount";
            public string LoadCustomization => "LoadCustomization";
            public string MasterCopyPath => "MasterCopyPath";
            public string RequiredFreeMemory => "RequiredFreeMemory";
            public string KeyFolderPath => "KeyFolderPath";
        }

        public struct HostConfigurationMappings
        {
            public string TableName => "HostConfigurationMappings";
            public string HostName => "HostName";
            public string ConfigurationId => "ConfigurationId";
        }

        public struct PlayerExceptions
        {
            public string TableName => "PlayerExceptions";
            public string SessionId => "SessionId";
            public string RecordingPath => "RecordingPath";
            public string ExceptionMessage => "ExceptionMessage";
            public string ExceptionTrace => "ExceptionTrace";
            public string Error => "Error";
        }

        //public struct RecordingInstances
        //{
        //    public string TableName => "RecordingInstances";
        //    public string SessionId => "SessionId";
        //    public string RecordingPath => "RecordingPath";
        //    public string Started => "Started";
        //}

        public struct RunInstance
        {
            public string TableName => "RunInstance";
            public string InstanceId => "InstanceId";
            public string StartTime => "StartTime";
            public string RunLocation => "RunLocation";
            public string RunMode => "RunMode";
            public string Configuration => "Configuration";
            public string EndTime => "EndTime";
            public string NumberOfRecordings => "NumberOfRecordings";
        }

        public struct PlayerInstances
        {
            public string TableName => "PlayerInstances";
            public string SessionId => "SessionId";
            public string PlayerId => "PlayerId";
            public string RecordingPath => "RecordingPath";
            public string StartTime => "StartTime";
            public string EndTime => "EndTime";
            public string PeakMemoryUsage => "PeakMemoryUsage";
            public string CpuTime => "CpuTime";
            public string CpuUsage => "CpuUsage";
            public string ExitCode => "ExitCode";
        }
    }
}