using System.Data;
using System.Diagnostics;
using System.Text;

using MassRecordings;

using PT.Common.Sql.SqlServer;
using PT.Scheduler;
using PT.SchedulerDefinitions.MassRecordings;
using PT.Transmissions;

namespace MassRecordingPlayer;

/// <summary>
/// The Player class performs all the actions to run a recording
/// </summary>
public class Player
{
    private static DatabaseConnections s_dbConnector;
    private static PTBroadcaster s_ptBroadcaster;
    private static long s_memoryUsageMB;
    private static readonly TimeSpan s_cpuTime = new (0);
    private static float s_cpuUsage;
    private static readonly string s_recordMemoryStats = "";
    private static readonly string s_memoryLogPath = "";
    private static string s_errorMessage = "";
    private static int s_errorCode;
    private static string s_errorStackTrace = "";

    private static string s_workingDirectory;
    private static readonly object s_dbLock = new ();
    private static string s_prefix;
    private static string s_tempFile;
    private static readonly Dictionary<ulong, long> s_processingSpansDict = new ();
    private static MRConfiguration s_mrConfig;

    public delegate void CleanUpFinishedHandler(int a_errorCode);

    public event CleanUpFinishedHandler FinishedCleanUpEvent;
    private static bool s_isProcess = true;
    private long m_sessionId;
    private string m_recordingPath;
    private int m_playerId;
    private string m_uiPath;

    public static string DebugTempDirectory { get; set; }

    public void Start(long a_sessionId, string a_recordingPath, int a_playerId, bool a_isProcess, string UIPath)
    {
        m_sessionId = a_sessionId;
        m_recordingPath = a_recordingPath;
        m_playerId = a_playerId;
        s_isProcess = a_isProcess;
        m_uiPath = UIPath;
        try
        {
            //Add entry into database
            SimpleConfiguration configuration = new ();
            string dbConnectionString = configuration.LoadValue("DBConnectionString");
            s_dbConnector = new DatabaseConnections(dbConnectionString);
            s_mrConfig = new MRConfiguration(s_dbConnector, m_sessionId, m_recordingPath);
            s_prefix = Convert.ToString(m_sessionId) + "MR";
            s_tempFile = Path.GetTempFileName();
            s_workingDirectory = Path.Combine(Path.GetTempPath(), s_prefix + Path.GetFileNameWithoutExtension(s_tempFile));

            DebugTempDirectory = s_workingDirectory;

            //Get files inside the recording folder
            IEnumerable<string> enumerateFiles = Directory.EnumerateFiles(s_mrConfig.RecordingDirectoryToLoadFromAtStartUp);

            //check that the .dat file is larger than 8KB, otherwise it is probably an empty scenario and it does not need to be played back.
            foreach (string file in enumerateFiles)
            {
                if (file.ToUpper().EndsWith(".DAT"))
                {
                    long length = new FileInfo(file).Length;
                    if (length < 8000)
                    {
                        //update database with a warning with error code 13 and end the recording
                        s_errorCode = 13;
                        s_errorMessage = "scenarios.dat file was empty. ";
                        try
                        {
                            lock (s_dbLock)
                            {
                                string updateErrorLog = MassRecordings.SqlStrings.GetInsertPlayerExceptions(m_sessionId, m_playerId, CleanStringForSQL(s_errorMessage), null, false);
                                string updateInstance = MassRecordings.SqlStrings.GetUpdateInstanceBase(DateTime.Now, s_memoryUsageMB, s_cpuTime.Ticks, s_cpuUsage, m_sessionId, m_playerId);
                                s_dbConnector.SendSQLTransaction(new[] { updateInstance, updateErrorLog });
                            }
                        }
                        catch
                        {
                            s_errorCode = 5;
                        }


                        if (s_isProcess)
                        {
                            Environment.Exit(s_errorCode);
                        }

                        FinishedCleanUpEvent?.Invoke(s_errorCode);
                    }
                }
            }

            // Get constructor Values
            BroadcasterConstructorValues broadcasterValues = new BroadcasterConstructorValues();

            // These settings have always been set regardless of whether an instance has been provided or not
            broadcasterValues.WorkingDirectory = s_workingDirectory;
            broadcasterValues.RecordingDirectoryToLoadFromAtStartup = s_mrConfig.RecordingDirectoryToLoadFromAtStartUp;
            broadcasterValues.StartType = StartTypeEnum.RecordingClientDelayed;
            //broadcasterValues.AbsorbCustomizations = true; //TODO: Find a better way to load custom packages
            broadcasterValues.MaxNbrSessionRecordingsToStore = 10;
            broadcasterValues.MaxNbrSystemBackupsToStorePerSession = 100;
            broadcasterValues.MinutesBetweenSystemBackups = 30;
            broadcasterValues.NonSequencedTransmissionPlayback = false;
            broadcasterValues.Record = false;
            broadcasterValues.SingleThreaded = false;
            broadcasterValues.StartingScenarioNumber = 0;
            broadcasterValues.ServiceThreadPriority = ThreadPriority.Normal;
            broadcasterValues.RunningMassRecordings = true;
            broadcasterValues.MassRecordingsSessionId = m_sessionId;
            broadcasterValues.MassRecordingsPlayerPath = Convert.ToString(m_playerId);
            broadcasterValues.KeyFolder = s_mrConfig.KeyFolderPath;
            broadcasterValues.LogDbConnectionString = dbConnectionString;
            broadcasterValues.InstanceName = m_sessionId.ToString();
            broadcasterValues.SoftwareVersion = Convert.ToString(m_playerId);
            broadcasterValues.ClientTimeoutSeconds = Convert.ToInt32(TimeSpan.FromMinutes(20).TotalSeconds);
            broadcasterValues.InterfaceTimeoutSeconds = Convert.ToInt32(TimeSpan.FromMinutes(20).TotalSeconds);
            // Set the rest if no instance have been provided

            // Load customization
            // Customizations are ignored if system StartType = UnitTestBase.

            //2012.12.26: Load customizations for all modes so that the scenario comparisons work (Determined by config parameter)

            if (s_mrConfig.LoadCustomization != null && s_mrConfig.LoadCustomization.ToLower() == "true" && s_mrConfig.RecordingDirectoryToLoadFromAtStartUp.ToUpper().Contains("CUSTOMER"))
            {
                string customerDirectory = s_mrConfig.RecordingDirectoryToLoadFromAtStartUp.Substring(0, s_mrConfig.RecordingDirectoryToLoadFromAtStartUp.LastIndexOf("\\"));
                int temp = customerDirectory.LastIndexOf("\\");
                customerDirectory = customerDirectory.Substring(temp + 1, customerDirectory.Length - temp - 1);
                string templateCustomizationPath = Path.Combine(s_workingDirectory, "Data", "Customizations");
                if (!Directory.Exists(templateCustomizationPath))
                {
                    Directory.CreateDirectory(templateCustomizationPath);
                }

                DirectoryInfo ptSolutionDir = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent;
                List<string> customizationsToLoad = GetCustomizations(ptSolutionDir.FullName, customerDirectory);
                if (customizationsToLoad.Count > 0)
                {
                    //TODO: Find a better way to load custom packages
                    //broadcasterValues.AbsorbCustomizations = true;
                    foreach (string custPath in customizationsToLoad)
                    {
                        File.Copy(custPath, Path.Combine(templateCustomizationPath, Path.GetFileName(custPath)));
                    }
                }
            }

            //Create the file if this is the first player run. 
            //System memory stats will not write to file if it doesn't exist.
            if (s_recordMemoryStats == "true" && !File.Exists(s_memoryLogPath))
            {
                File.Create(s_memoryLogPath);
            }

            SystemController.StartAPSService(broadcasterValues, "Errors.txt");
            SystemController.StartSystemReceiver();
            s_ptBroadcaster = (PTBroadcaster)SystemController.Broadcaster;
            s_ptBroadcaster.PlaybackEndOfTransmissionsEvent += PtBroadcasterOnPlaybackEndOfTransmissionsEvent;

            //if (!string.IsNullOrEmpty(m_uiPath))
            //{
            //    Process p = new Process();
            //    p.StartInfo.FileName = m_uiPath;
            //    p.StartInfo.WorkingDirectory = Path.GetDirectoryName(m_uiPath);
            //    p.StartInfo.UseShellExecute = false;
            //    p.StartInfo.CreateNoWindow = true;
            //    p.StartInfo.Arguments = @"\skipclientupdater: \ServerURI:http://localhost:7991 \InstanceName:Debug \SoftwareVersion:0.0 \UserName:admin \Password:chicago \EndQuickly:  \CreateInstanceOfServer:Internal1 \SkipLoginScreen:";
            //    p.Start();
            //}

            SendPlaybackT();
        }
        catch (Exception e)
        {
            s_errorMessage = PT.Common.Extensions.ExceptionExtensions.GetExceptionFullMessage(e);
            s_errorStackTrace = e.StackTrace;

            if (s_errorMessage.Contains("ERROR 2607"))
            {
                //This exception is due to an invalid scenario (from a newer serialization version)
                //Don't log this exception and exit with a specific code
                s_errorMessage = "";
                s_errorStackTrace = "";
                s_errorCode = 2;
            }

            //all player exceptions will be logged to the database
            if (s_errorCode != 13)
            {
                CleanUpProcessing();
            }
        }
        finally
        {
            try
            {
                PT.Common.File.FileUtils.DeleteFilesWithRetry(Path.GetTempPath(), Path.GetFileName(s_tempFile));
            }
            catch (Exception e)
            {
                string UpdatePlayerExceptions = MassRecordings.SqlStrings.GetInsertPlayerExceptions(m_sessionId, m_playerId, e.Message, e.StackTrace, false);
                lock (s_dbLock)
                {
                    s_dbConnector.SendSQLTransaction(new[] { UpdatePlayerExceptions });
                }
            }
        }
    }

    private void PtBroadcasterOnPlaybackEndOfTransmissionsEvent()
    {
        StartCleanUp();
    }

    private void CleanUpProcessing()
    {
        try
        {
            if (s_errorMessage != "")
            {
                string updateErrorLog = MassRecordings.SqlStrings.GetInsertPlayerExceptions(m_sessionId, m_playerId, CleanStringForSQL(s_errorMessage), CleanStringForSQL(s_errorStackTrace), true);
                s_dbConnector.SendSQLTransaction(new[] { updateErrorLog });
                s_errorCode = 4;
            }
            else
            {
                //CompareCheckSumValues();
                CheckForAdverseScheduleEffects();
            }
            //CalculateStatistics();

            //Log results to the database

            lock (s_dbLock)
            {
                string updateInstance = MassRecordings.SqlStrings.GetUpdateInstanceBase(DateTime.Now, s_memoryUsageMB, s_cpuTime.Ticks, s_cpuUsage, m_sessionId, m_playerId);
                s_dbConnector.SendSQLTransaction(new[] { updateInstance });
            }

            // Destroys the system and causes it to be written to disk.
            if (s_ptBroadcaster != null)
            {
                s_ptBroadcaster.Dispose();
            }

            if (s_isProcess)
            {
                Environment.Exit(s_errorCode);
            }

            FinishedCleanUpEvent?.Invoke(s_errorCode);
        }
        catch (DataException e)
        {
            string updateErrorLog = MassRecordings.SqlStrings.GetInsertPlayerExceptions(m_sessionId, m_playerId, CleanStringForSQL(e.Message), CleanStringForSQL(e.StackTrace), true);
            s_dbConnector.SendSQLTransaction(new[] { updateErrorLog });

            if (s_isProcess)
            {
                Environment.Exit(15);
            }

            FinishedCleanUpEvent?.Invoke(15);
        }
        catch (Exception e)
        {
            try
            {
                string updateErrorLog = MassRecordings.SqlStrings.GetInsertPlayerExceptions(m_sessionId, m_playerId, CleanStringForSQL(e.Message), CleanStringForSQL(e.StackTrace), true);
                s_dbConnector.SendSQLTransaction(new[] { updateErrorLog });
                if (s_isProcess)
                {
                    Environment.Exit(3);
                }

                FinishedCleanUpEvent?.Invoke(3);
            }
            catch
            {
                if (s_isProcess)
                {
                    Environment.Exit(5);
                }

                FinishedCleanUpEvent?.Invoke(5);
            }
        }
    }

    private readonly Dictionary<string, string> m_lateOpsDictionary = new ();
    private readonly Dictionary<string, string> m_lateJobsDictionary = new ();

    /// <summary>
    /// Search through database for published late objects. Compare to a base session to determine whether the objects have become late due to a code change
    /// </summary>
    private void CheckForAdverseScheduleEffects()
    {
        try
        {
            MassRecordingsTableDefinitions.ScheduleIssues schedIssues = new ();

            //Get current MR session late objects data table
            DataTable currentScheduleIssuesDt = s_dbConnector.SelectSQLTable($"select * from {schedIssues.TableName} where ({schedIssues.SessionId} = '{m_sessionId}' and (select cast ({schedIssues.PlayerId} as int)) = {m_playerId}) order by {schedIssues.TransmissionNbr}");
            int basePlayerId = GetBasePlayerId();
            bool someObjectsAreAffected = false;
            int firstProblemTransmission = -1;

            //If there are late objects, determine at which transmission the current session has late objects that were not late at the same point in the base session
            if (currentScheduleIssuesDt.Rows.Count > 0)
            {
                //-1 is returned if no differences were found
                firstProblemTransmission = GetFirstProblemTransmission(currentScheduleIssuesDt, basePlayerId);
            }

            if (firstProblemTransmission != -1)
            {
                //Get table with all objects that were late in the first problem transmission
                DataTable lateObjectsDt = s_dbConnector.SelectSQLTable($"select * from {schedIssues.TableName} where ({schedIssues.SessionId} = '{m_sessionId}' and (select cast ({schedIssues.PlayerId} as int)) = {m_playerId}) and {schedIssues.TransmissionNbr} = {firstProblemTransmission}");

                //Iterate through the table and check if the base session contains the same late objects at the same point in time (the same transmission)
                for (int i = 0; i < lateObjectsDt.Rows.Count; i++)
                {
                    int objectId = Convert.ToInt32(lateObjectsDt.Rows[i][schedIssues.LateObjectId]);
                    string objectType = Convert.ToString(lateObjectsDt.Rows[i][schedIssues.LateObjectType]);
                    string objectName = Convert.ToString(lateObjectsDt.Rows[i][schedIssues.LateObjectName]);
                    string transmissionType = Convert.ToString(lateObjectsDt.Rows[i][schedIssues.TransmissionType]);
                    int transmissionNbr = Convert.ToInt32(lateObjectsDt.Rows[i][schedIssues.TransmissionNbr]);

                    DataTable dt = s_dbConnector.SelectSQLTable($"select * from {schedIssues.TableName} where ({schedIssues.SessionId} = '{s_mrConfig.BaseSessionId}' and {schedIssues.PlayerId} = {basePlayerId}) and {schedIssues.LateObjectId} " +
                                                                $"= {objectId} and {schedIssues.TransmissionNbr} = {transmissionNbr}");

                    //If data table has no rows, it means that the object has become late due to changes that were late to the code
                    if (dt.Rows.Count == 0)
                    {
                        someObjectsAreAffected = true;
                        if (objectType == "Operation")
                        {
                            if (!m_lateOpsDictionary.TryGetValue(objectId.ToString(), out string opName))
                            {
                                m_lateOpsDictionary.Add(objectId.ToString(), objectName + "," + transmissionType);
                            }
                        }
                        else
                        {
                            if (!m_lateJobsDictionary.TryGetValue(objectId.ToString(), out string jobName))
                            {
                                m_lateJobsDictionary.Add(objectId.ToString(), objectName + "," + transmissionType);
                            }
                        }
                    }
                }
            }

            //if some objects were affected by the code change, construct an error string and throw an exception to fail the test recording and display the message
            if (someObjectsAreAffected)
            {
                StringBuilder sb = new ();
                sb = ConstructLateObjectsErrorString();

                throw new DataException(sb.ToString());
            }
        }
        catch (DataException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            string UpdatePlayerExceptions = MassRecordings.SqlStrings.GetInsertPlayerExceptions(m_sessionId, m_playerId, e.Message, e.StackTrace, false);
            lock (s_dbLock)
            {
                s_dbConnector.SendSQLTransaction(new[] { UpdatePlayerExceptions });
            }
        }
    }

    /// <summary>
    /// Iterates through the late ops and late jobs dictionaries to construct an error string
    /// </summary>
    /// <returns></returns>
    private StringBuilder ConstructLateObjectsErrorString()
    {
        StringBuilder sb = new ();
        if (m_lateOpsDictionary.Count > 0)
        {
            sb.AppendLine("=============================================================");
            sb.AppendLine();
            sb.AppendLine("The following Operations have become late as a result of changes to the code:");
        }

        foreach (KeyValuePair<string, string> pair in m_lateOpsDictionary)
        {
            sb.AppendFormat("Op Name: {1} | Op Id: {0} on the {2}{3} transmission", pair.Key, pair.Value.Split(',')[0], pair.Value.Split(',')[1], Environment.NewLine);
            sb.AppendLine();
        }

        if (m_lateJobsDictionary.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("The following Jobs have become late as a result of changes to the code:");
        }

        foreach (KeyValuePair<string, string> pair in m_lateJobsDictionary)
        {
            sb.AppendFormat("Job Name: {1} | Job Id: {0} on the {2}{3} transmission", pair.Key, pair.Value.Split(',')[0], pair.Value.Split(',')[1], Environment.NewLine);
            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine("=============================================================");
        sb.AppendLine();

        return sb;
    }

    /// <summary>
    /// Iterates through the current session late objects data table and finds the first transmission at which we have at least one late object in the current session that is not
    /// also late in the base session
    /// </summary>
    /// <param name="a_currentScheduleIssuesDt"></param>
    /// <param name="a_basePlayerId"></param>
    /// <returns></returns>
    private static int GetFirstProblemTransmission(DataTable a_currentScheduleIssuesDt, int a_basePlayerId)
    {
        MassRecordingsTableDefinitions.ScheduleIssues schedIssues = new ();

        for (int i = 0; i < a_currentScheduleIssuesDt.Rows.Count; i++)
        {
            int objectId = Convert.ToInt32(a_currentScheduleIssuesDt.Rows[i][schedIssues.LateObjectId]);
            int transmissionNbr = Convert.ToInt32(a_currentScheduleIssuesDt.Rows[i][schedIssues.TransmissionNbr]);

            DataTable dt = s_dbConnector.SelectSQLTable($"select * from {schedIssues.TableName} where ({schedIssues.SessionId} = '{s_mrConfig.BaseSessionId}' and {schedIssues.PlayerId} = {a_basePlayerId}) and {schedIssues.LateObjectId} " +
                                                        $"= {objectId} and {schedIssues.TransmissionNbr} = {transmissionNbr}");

            if (dt.Rows.Count == 0)
            {
                return transmissionNbr;
            }
        }

        return -1;
    }

    /// <summary>
    /// Use a SQL CheckSums data table to compare values between current and base recordings
    /// </summary>
    private void CompareCheckSumValues()
    {
        MassRecordingsTableDefinitions.ActionChecksums actCheckSumsDt = new ();

        DataTable currentIdCheckSums = s_dbConnector.SelectSQLTable($"select * from {actCheckSumsDt.TableName} where ({actCheckSumsDt.SessionId} = '{m_sessionId}' and (select cast ({actCheckSumsDt.PlayerPath} as int)) = {m_playerId}) order by {actCheckSumsDt.TransmissionNbr}");
        int basePlayerId = 0;

        try
        {
            basePlayerId = GetBasePlayerId();
        }
        catch (Exception e)
        {
            string UpdatePlayerExceptions = MassRecordings.SqlStrings.GetInsertPlayerExceptions(m_sessionId, m_playerId, e.Message, e.StackTrace, false);
            lock (s_dbLock)
            {
                s_dbConnector.SendSQLTransaction(new[] { UpdatePlayerExceptions });
            }
        }

        DataTable baseIdCheckSums = s_dbConnector.SelectSQLTable($"select * from {actCheckSumsDt.TableName} where ({actCheckSumsDt.SessionId} = '{s_mrConfig.BaseSessionId}' and (select cast ({actCheckSumsDt.PlayerPath} as int)) = {basePlayerId}) order by {actCheckSumsDt.TransmissionNbr}");

        try
        {
            try
            {
                //Check if base session is outdated, i.e.  missing a recording
                if (baseIdCheckSums.Rows.Count == 0)
                {
                    throw new Exception("Base Session is outdated.");
                }
            }
            catch (Exception e)
            {
                string UpdatePlayerExceptions = MassRecordings.SqlStrings.GetInsertPlayerExceptions(m_sessionId, m_playerId, e.Message, e.StackTrace, false);
                lock (s_dbLock)
                {
                    s_dbConnector.SendSQLTransaction(new[] { UpdatePlayerExceptions });
                }

                return;
            }

            //Check if base and current recordings have the same number of actions.
            if (currentIdCheckSums.Rows.Count != baseIdCheckSums.Rows.Count)
            {
                throw new Exception("Base Session and Current Session do not have the same number of actions.");
            }

            //Compare values
            for (int i = 0; i < currentIdCheckSums.Rows.Count; i++)
            {
                if (Convert.ToDecimal(currentIdCheckSums.Rows[i][actCheckSumsDt.StartAndEndSums]) != Convert.ToDecimal(baseIdCheckSums.Rows[i][actCheckSumsDt.StartAndEndSums]))
                {
                    throw new Exception("Base Session and Current Session have differing start and end checksum values.");
                }

                if (Convert.ToDecimal(currentIdCheckSums.Rows[i][actCheckSumsDt.ResourceJobOperationCombos]) != Convert.ToDecimal(baseIdCheckSums.Rows[i][actCheckSumsDt.ResourceJobOperationCombos]))
                {
                    throw new Exception("Base Session and Current Session have differing Resourse-Job-Operation combos.");
                }

                if (Convert.ToInt64(currentIdCheckSums.Rows[i][actCheckSumsDt.BlockCount]) != Convert.ToInt64(baseIdCheckSums.Rows[i][actCheckSumsDt.BlockCount]))
                {
                    throw new Exception("Base Session and Current Session have a different block count.");
                }

                if (Convert.ToInt64(currentIdCheckSums.Rows[i][actCheckSumsDt.NbrOfSimulations]) != Convert.ToInt64(baseIdCheckSums.Rows[i][actCheckSumsDt.NbrOfSimulations]))
                {
                    throw new Exception("Base Session and Current Session have a different number of simulations.");
                }
            }
        }
        catch (Exception e)
        {
            string UpdatePlayerExceptions = MassRecordings.SqlStrings.GetInsertPlayerExceptions(m_sessionId, m_playerId, e.Message, e.StackTrace, true);
            lock (s_dbLock)
            {
                s_dbConnector.SendSQLTransaction(new[] { UpdatePlayerExceptions });
            }
        }
    }

    private int GetBasePlayerId()
    {
        int playerId = 0;
        try
        {
            DataTable dt = s_dbConnector.SelectSQLTable(SqlStrings.getBaseSessionPlayerPath(m_playerId));

            if (dt.Rows.Count == 0)
            {
                throw new Exception("There are no recordings in the database. It is not possible to compare CheckSums.");
            }

            string playerPath = Convert.ToString(dt.Rows[0][0]);
            dt = s_dbConnector.SelectSQLTable(SqlStrings.getBaseSessionPlayerId(s_mrConfig.BaseSessionId, playerPath.CleanString()));

            if (dt.Rows.Count == 0)
            {
                throw new Exception("Could not locate the playerId in the database. It is not possible to compare CheckSums.");
            }

            playerId = Convert.ToInt32(dt.Rows[0][0]);
        }
        catch (Exception e)
        {
            throw e;
        }

        return playerId;
    }

    /// <summary>
    /// Calculate process usage statistics
    /// </summary>
    //private static void CalculateStatistics()
    //{
    //    if (s_recordMemoryStats == "true")
    //    {
    //        RecordMemoryStatistics(s_memoryLogPath, s_mrConfig.RecordingDirectoryToLoadFromAtStartUp);
    //    }

    //    //Get process usages. 
    //    Process currentProcess = Process.GetCurrentProcess();
    //    s_memoryUsageMB = currentProcess.PrivateMemorySize64 / (2 * 1024);
    //    s_cpuTime = currentProcess.TotalProcessorTime;
    //    //Player is multithreaded and can return more than 100%.
    //    using (PerformanceCounter counter = new PerformanceCounter("Process", "% Processor Time", currentProcess.ProcessName, true))
    //    {
    //        counter.NextValue();
    //        System.Threading.Thread.Sleep(2000);
    //        s_cpuUsage = counter.NextValue();
    //    }
    //}

    /// <summary>
    /// Send a transmission to playback until the next simulation
    /// </summary>
    private static void SendPlaybackT()
    {
        TriggerRecordingPlaybackT t = new ();
        t.PlayBackMode = TriggerRecordingPlaybackT.EPlayBackModes.Full;
        s_ptBroadcaster.Broadcast(Transmission.Compress(t), -1);
    }

    private static string CleanStringForSQL(string a_string)
    {
        return a_string == null ? "null" : a_string.Replace("'", "''");
    }

    /// <summary>
    /// Will look into APS folder for a customization projects matching this directory.
    /// </summary>
    /// <param name="a_apsSolutionpath">Customization folder location</param>
    /// <param name="a_customization">Customer name</param>
    /// <returns>returns a list containing paths to customization projects if any found, an array of length zero otherwise</returns>
    private static List<string> GetCustomizations(string a_apsSolutionpath, string a_customization)
    {
        try
        {
            List<string> customizationPaths = new ();
            string pattern = string.Format("schedCust.{0}.*.dll", a_customization);
            string[] paths = Directory.GetFileSystemEntries(a_apsSolutionpath, pattern, SearchOption.AllDirectories);
            foreach (string path in paths)
            {
                //Add customization if it is in the TFS build Binaries folder or the local Bin\[Mode] folder.
                #if DEBUG
                if (path.ToUpper().Contains("BINARIES") || (path.ToUpper().Contains("BIN\\DEBUG") && !path.ToUpper().Contains("UNUSED")))
                {
                    customizationPaths.Add(path);
                }
                #else
                    if (path.ToUpper().Contains("BINARIES") || (path.ToUpper().Contains("BIN\\RELEASE") && !path.ToUpper().Contains("UNUSED")))
                        customizationPaths.Add(path);
                #endif
            }

            return customizationPaths;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    //TODO: Replace with logging to DB
    private void RecordMemoryStatistics(string a_memoryLogPath, string a_recordingPath)
    {
        File.AppendAllText(a_memoryLogPath, a_recordingPath + Environment.NewLine + "Peak Memory Usage: " + Process.GetCurrentProcess().PeakWorkingSet64 / 1024 / 1024 + "MB" + Environment.NewLine + Environment.NewLine);
    }

    private void StoreTiming(ulong a_transmissionNbr, TimeSpan a_processingTime)
    {
        if (s_processingSpansDict.ContainsKey(a_transmissionNbr))
        {
            //duplicate. Not sure why this happens yet. Possibly undo/redo related
            //Remove both.
            s_processingSpansDict.Remove(a_transmissionNbr);
            return;
        }

        s_processingSpansDict.Add(a_transmissionNbr, a_processingTime.Ticks);
    }

    private void StartCleanUp()
    {
        //This should be in a new thread because the event caller is comming from broadcaster which will be disposed in the following function.
        if (s_errorCode != 13)
        {
            Task.Factory.StartNew(CleanUpProcessing);
        }
    }
}

/// <summary>
/// This class contains the the MassRecordingsPlayer executable main entry point.
/// </summary>
public class MassRecordingsPlayer
{
    private static long p_sessionId;
    private static int p_playerId;
    private static string p_recordingPath;
    private static bool s_isProcess;
    private static string m_uiPath;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        #if DEBUG
        //System.Threading.Thread.Sleep(10000);
        #endif
        p_sessionId = Convert.ToInt64(args[0]);
        p_recordingPath = args[1];
        p_playerId = Convert.ToInt32(args[2]);
        s_isProcess = true;
        m_uiPath = args[3];

        Player newPlayer = new ();
        newPlayer.Start(p_sessionId, p_recordingPath, p_playerId, s_isProcess, m_uiPath);
    }
}