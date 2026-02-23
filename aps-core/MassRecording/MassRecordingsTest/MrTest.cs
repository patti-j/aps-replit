using System.Data;

using MassRecordingPlayer;

using MassRecordings;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PT.Common.Sql.SqlServer;

namespace MassRecordingsTest;

public partial class MrTest
{
    public DatabaseConnections DBConnector;
    private static StartNewInstance s_instanceConfig; //starts new player instance and get session Id and database connector
    private static readonly object s_lock = new ();
    public static bool CanStart { get; set; }
    public string DebugTempDirectory { get; set; }

    /// <summary>
    /// Allows UnitTest to start a Player Class to run a recording rather than starting a process to make debugging easier.
    /// This allows to debug a single recording without having to have to attach to a process.
    /// </summary>
    /// <returns></returns>
    internal static bool CanStartInternally()
    {
        lock (s_lock)
        {
            if (!CanStart)
            {
                CanStart = true;
                return true;
            }

            return false;
        }
    }

    ///<summary>Start new session before running Test Methods</summary>
    [ClassInitialize]
    public static void DataBaseInit(TestContext a_context)
    {
        s_instanceConfig = new StartNewInstance();
        s_instanceConfig.GetInstanceInfo();
        string tempPath = Path.GetTempPath();

        DirectoryInfo tempDir = new (tempPath);
        DirectoryInfo[] filesInTempDir = tempDir.GetDirectories("*" + "MRtmp" + "*");

        foreach (DirectoryInfo file in filesInTempDir)
        {
            try
            {
                PT.Common.File.FileUtils.DeleteDirectoryRecursivelyWithRetry(file.FullName);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    /// <summary>
    /// When all tests have run, delete all temp folders created by the player to use a working directory.
    /// </summary>
    [ClassCleanup]
    public static void DeleteTempFolders()
    {
        DeleteBadFolders();
        if (!string.IsNullOrEmpty(s_instanceConfig.DBConnectionString))
        {
            SqlStrings updateEndTime = new ();
            updateEndTime.UpdateRunInstanceEndTime(s_instanceConfig.DBConnectionString, DateTime.Now, s_instanceConfig.SessionId);
        }

        string partialName = Convert.ToString(s_instanceConfig.SessionId);
        DirectoryInfo tempDir = new (Path.GetTempPath());
        DirectoryInfo[] filesInTempDir = tempDir.GetDirectories(partialName + "*");

        foreach (DirectoryInfo file in filesInTempDir)
        {
            try
            {
                if (file.FullName != Player.DebugTempDirectory)
                {
                    PT.Common.File.FileUtils.DeleteDirectoryRecursivelyWithRetry(file.FullName);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        MRUtilities mrKill = new ();
        mrKill.KillMRProcesses();
    }

    public static void DeleteBadFolders()
    {
        MassRecordings.SqlStrings.TableDefinitions.HostConfigurations hostConfigDt = new ();
        DatabaseConnections dbConnector = new (s_instanceConfig.DBConnectionString);

        DataTable dt = dbConnector.SelectSQLTable(MassRecordings.SqlStrings.GetHostConfigTable(Environment.MachineName));
        string destPath = Convert.ToString(dt.Rows[0][hostConfigDt.RecordingsDirectory]);
        FileInfo fileInfo = new (destPath);

        DirectoryInfo[] directory = fileInfo.Directory.GetDirectories("BAD_FOLDER_*", SearchOption.AllDirectories);

        foreach (DirectoryInfo file in directory)
        {
            try
            {
                if (file.FullName.Contains("BAD_FOLDER_"))
                {
                    PT.Common.File.FileUtils.DeleteDirectoryRecursivelyWithRetry(file.FullName);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}