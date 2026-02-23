using PT.Common.Sql.SqlServer;

using static MassRecordings.SqlStrings.TableDefinitions;

namespace MassRecordingsTest;

/// <summary>
/// Query strings used by MR to interact with the database.
/// </summary>
public class SqlStrings
{
    public static string getCurrentPlayerId()
    {
        PlayerInstances playerInstances = new ();
        const string c_currentPlayerId = "Select {1} from {0} where {1} = (select max({1}) from {0}) ";
        return string.Format(c_currentPlayerId, playerInstances.TableName, playerInstances.PlayerId);
    }

    public static string getSelectInstanceId(DateTime a_startTime)
    {
        const string c_selectInstanceId = "Select {0} FROM {1} WHERE {2} =  '{3}'";
        RunInstance runInstanceDt = new ();
        return string.Format(c_selectInstanceId, runInstanceDt.InstanceId, runInstanceDt.TableName, runInstanceDt.StartTime, a_startTime);
    }

    public static string getNewInstanceBase(DateTime a_startTime, string a_compName, string a_runMode, int a_numberOfTests)
    {
        #if DEBUG
        const string c_newInstanceBase = "Insert into {0} with (tablockx) Values('{1}', '{2}', '{3}', 'DEBUG', NULL, {4})";
        #else
            const string c_newInstanceBase = "Insert into {0} with (tablockx) Values('{1}', '{2}', '{3}', 'RELEASE', NULL, {4})";
        #endif
        RunInstance runInstanceDt = new ();
        return string.Format(c_newInstanceBase, runInstanceDt.TableName, a_startTime, a_compName, a_runMode, a_numberOfTests);
    }

    public void UpdateRunInstanceEndTime(string a_sqlConnectionString, DateTime a_endTime, int a_instanceId)
    {
        DatabaseConnections databaseConnection = new (a_sqlConnectionString);
        databaseConnection.SendSQLTransaction(new[] { SetRunInstanceEndTime(a_endTime, a_instanceId) });
    }

    private static string SetRunInstanceEndTime(DateTime m_endTime, int m_instanceId)
    {
        const string c_changeRunInstanceEndTime = "UPDATE {0} SET {1} = '{2}' WHERE {3} = {4}";
        RunInstance runInstance = new ();
        return string.Format(c_changeRunInstanceEndTime, runInstance.TableName, runInstance.EndTime, m_endTime, runInstance.InstanceId, m_instanceId);
    }

    public static string GetInsertInstanceBase(long a_sessionId, string a_recordingPath, DateTime a_startTime)
    {
        const string c_insertInstanceBase = "Insert into {0} with (tablockx) ({1}, {2}, {3}) Values({4}, '{5}', '{6}') ";
        PlayerInstances playerInstancesDt = new ();
        return string.Format(c_insertInstanceBase, playerInstancesDt.TableName, playerInstancesDt.SessionId, playerInstancesDt.StartTime, playerInstancesDt.RecordingPath, a_sessionId, a_startTime, a_recordingPath);
    }

    public static string GetUpdateExitCode(long a_sessionId, int a_playerId, string a_exitCode)
    {
        PlayerInstances playerInstances = new ();

        string insertExitCode = $"Update {playerInstances.TableName} with (tablockx) set {playerInstances.ExitCode} = '{a_exitCode}' " +
                                $"where {playerInstances.SessionId} = {a_sessionId} and {playerInstances.PlayerId} = {a_playerId} ";

        return insertExitCode;
    }
}