namespace MassRecordingUnitTestGenerator;

internal class SqlStrings
{
    public static string GetAverageRunTimeQuery(string a_configName)
    {
        MassRecordings.SqlStrings.TableDefinitions.PlayerInstances playerInstancesDt = new ();
        MassRecordings.SqlStrings.TableDefinitions.RunInstance runInstanceDt = new ();


        //Select RecordingPath, AVG(DateDiff(ss, PlayerInstances.StartTime, PlayerInstances.EndTime)) as Seconds from PlayerInstances join RunInstance 
        //on PlayerInstances.SessionId = RunInstances.InstanceId 
        //where RunInstance.RunMode = '{0}' and PlayerInstances.EndTime is not NULL group by RecordingPath
        const string c_baseString = "Select {0}, AVG(DateDiff(ss, {1}.{2}, {3}.{4})) as Seconds from {5} join {6} on {7}.{8} = {9}.{10} where {11}.{12} = '{13}' and {14}.{15} is not NULL group by {16}";

        string format = string.Format(c_baseString, playerInstancesDt.RecordingPath, playerInstancesDt.TableName, playerInstancesDt.StartTime, playerInstancesDt.TableName, playerInstancesDt.EndTime, playerInstancesDt.TableName, runInstanceDt.TableName, playerInstancesDt.TableName, playerInstancesDt.SessionId, runInstanceDt.TableName, runInstanceDt.InstanceId, runInstanceDt.TableName, runInstanceDt.RunLocation, a_configName, playerInstancesDt.TableName, playerInstancesDt.EndTime, playerInstancesDt.RecordingPath);

        return format;
    }

    public static string GetConfigNames()
    {
        MassRecordings.SqlStrings.TableDefinitions.RunInstance runInstanceDt = new ();

        const string c_configNames = "Select distinct {0} from {1} order by {2} asc";
        string format = string.Format(c_configNames, runInstanceDt.RunLocation, runInstanceDt.TableName, runInstanceDt.RunLocation);

        return format;
    }
}