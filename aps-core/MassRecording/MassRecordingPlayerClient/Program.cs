using PT.Common.Sql.SqlServer;

namespace MassRecordingPlayerClient;

public class MassRecordingPlayerClient
{
    private static DatabaseConnections m_dbConnector;
    private static long p_sessionId;
    private static string p_DBConnectionString;
    private static string p_recordingDirectoryToLoadFromAtStartup;
    private static string m_uiPath;

    public static void Main(string[] args)
    {
        p_sessionId = int.Parse(args[0]);
        p_DBConnectionString = args[1];
        p_recordingDirectoryToLoadFromAtStartup = args[2];

        m_dbConnector = new DatabaseConnections(p_DBConnectionString);
        m_uiPath = args[3];
        SoftwareVersion serverProductVersion;
        string usersLanguage;
        //TODO: Service proxy login
        //SystemController.Login("admin", "chicago", SystemController.LoginType.User, true, out serverProductVersion, out usersLanguage, 500, "", out byte[] o_symmetricKey);
    }

    public const string InsertPlayerExceptions = "Insert into PlayerExceptions Values({0}, '{1}', '{2}','{3}')";

    private static void LogError()
    {
        string updateErrorLog = string.Format(InsertPlayerExceptions, p_sessionId, p_recordingDirectoryToLoadFromAtStartup, "Recoring Encountered Desynch.", "none");
        m_dbConnector.SendSQLTransaction(new[] { updateErrorLog });
    }
}