namespace MassRecordingPlayer;

/// <summary>
/// Query strings used by MR to interact with the database.
/// </summary>
public class SqlStrings
{
    public static string getBaseSessionPlayerId(long a_baseSessionId, string a_playerPath)
    {
        MassRecordings.SqlStrings.TableDefinitions.PlayerInstances playerInstances = new ();
        const string c_baseSessionPlayerId = "select {0} from {1} where {2} = {3} and {4} = '{5}'";
        return string.Format(c_baseSessionPlayerId, playerInstances.PlayerId, playerInstances.TableName, playerInstances.SessionId, a_baseSessionId, playerInstances.RecordingPath, a_playerPath);
    }

    public static string getBaseSessionPlayerPath(int a_playerId)
    {
        MassRecordings.SqlStrings.TableDefinitions.PlayerInstances playerInstances = new ();
        const string c_baseSessionPlayerPath = "select {0} from {1} where {2} = {3}";
        return string.Format(c_baseSessionPlayerPath, playerInstances.RecordingPath, playerInstances.TableName, playerInstances.PlayerId, a_playerId);
    }

    public static string getUpdateExitCode(int a_exitCode, long a_sessionId, int a_palyerId)
    {
        const string c_updateExitCode = "Update {0} SET {1} = {2} WHERE {3} = {4} and {5} = {6}";
        MassRecordings.SqlStrings.TableDefinitions.PlayerInstances playerInstancesDt = new ();
        return string.Format(c_updateExitCode, playerInstancesDt.TableName, playerInstancesDt.ExitCode, a_exitCode, playerInstancesDt.SessionId, a_sessionId, playerInstancesDt.PlayerId, a_palyerId);
    }
}