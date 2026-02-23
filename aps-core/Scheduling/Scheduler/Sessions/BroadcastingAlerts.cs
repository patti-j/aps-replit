namespace PT.Scheduler.Sessions;

/// <summary>
/// The location where Broadcasting is allowed to do its work.
/// Broadcasting needs disk access to perform things such as compression.
/// </summary>
public class BroadcastingAlerts
{
    private static bool s_initialized;
    private static string s_broadcastingAlertsAndErrorsWorkingDirectory;

    public static string BroadcastingAlertsAndExceptionsWorkingDirectory
    {
        get
        {
            ThrowIfNotInitialized();
            return s_broadcastingAlertsAndErrorsWorkingDirectory;
        }

        set
        {
            s_broadcastingAlertsAndErrorsWorkingDirectory = value;
            if (!Directory.Exists(s_broadcastingAlertsAndErrorsWorkingDirectory))
            {
                Directory.CreateDirectory(s_broadcastingAlertsAndErrorsWorkingDirectory);
            }

            CombineDirectoryAndLogFileName(ref s_broadcastingLogFileName);
            CombineDirectoryAndLogFileName(ref s_cleanupConnectionsExceptionLogFileName);
            CombineDirectoryAndLogFileName(ref s_createConnectionExceptionLogFileName);
            CombineDirectoryAndLogFileName(ref s_logOnLogOffLogFileName);
            CombineDirectoryAndLogFileName(ref s_miscExceptionLogFileName);
            CombineDirectoryAndLogFileName(ref s_systemNetWebExceptionLogFileName);

            CombineDirectoryAndLogFileName(ref s_biDirectionalKickLogFileName);

            s_initialized = true;
        }
    }

    private static void CombineDirectoryAndLogFileName(ref string r_logFileName)
    {
        r_logFileName = Path.Combine(s_broadcastingAlertsAndErrorsWorkingDirectory, r_logFileName);
    }

    private static void ThrowIfNotInitialized()
    {
        if (!s_initialized)
        {
            throw new Exception("The working broadcasting alerts and working directory hasn't been set.");
        }
    }

    private static string s_broadcastingLogFileName = "BroadcastingException.log";

    public static string BroadcastingLogFilePath
    {
        get
        {
            ThrowIfNotInitialized();
            return s_broadcastingLogFileName;
        }
    }

    private static string s_cleanupConnectionsExceptionLogFileName = "CleanupConnectionsException.log";

    internal static string CleanupConnectionsExceptionLogFilePath
    {
        get
        {
            ThrowIfNotInitialized();
            return s_cleanupConnectionsExceptionLogFileName;
        }
    }

    private static string s_createConnectionExceptionLogFileName = "CreateConnectionException.log";

    public static string CreateConnectionExceptionLogFilePath
    {
        get
        {
            ThrowIfNotInitialized();
            return s_createConnectionExceptionLogFileName;
        }
    }

    private static string s_logOnLogOffLogFileName = "LogOnLogOff.log";

    internal static string LogOnLogOffLogFilePath
    {
        get
        {
            ThrowIfNotInitialized();
            return s_logOnLogOffLogFileName;
        }
    }

    private static string s_miscExceptionLogFileName = "MiscException.log";

    internal static string MiscExceptionLogFilePath
    {
        get
        {
            ThrowIfNotInitialized();
            return s_miscExceptionLogFileName;
        }
    }

    private static string s_systemNetWebExceptionLogFileName = "System.Net.WebException.log";

    internal static string SystemNetWebExceptionLogFilePath
    {
        get
        {
            ThrowIfNotInitialized();
            return s_systemNetWebExceptionLogFileName;
        }
    }

    private static string s_biDirectionalKickLogFileName = "BiDirectionalKick.log";

    internal static string BiDirectionalKickLogFilePath
    {
        get
        {
            ThrowIfNotInitialized();
            return s_biDirectionalKickLogFileName;
        }
    }

    private static readonly string s_unidirectionalKickLogFileName = "UnidirectionalKick.log";

    internal static string UnidirectionalKickLogFilePath
    {
        get
        {
            ThrowIfNotInitialized();
            return s_unidirectionalKickLogFileName;
        }
    }
}