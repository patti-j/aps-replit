//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//using PT.Common.SqlServer;
//using PT.SchedulerDefinitions;
//using PT.Common;
//using PT.Common.Extensions;

//namespace MassRecordings
//{

//    #region Startup class
//    /// <summary>
//    /// </summary>
//    internal class MassRecordingsMaster
//    {
//        [STAThread]
//        private static void Main(String[] a_args)
//        {
//            string configPath = null;// = @"C:\TFSBuilds\APS\ContinuousIntegration(DEBUG)\Binaries\";
//            if(a_args.Length > 0)
//            {
//                configPath = a_args[0];
//            }

//            MassRecordingsTester tester = new MassRecordingsTester(configPath);
//            tester.TestRecordings();
//        }
//    }
//    #endregion

//    internal class MassRecordingsTester
//    {
//        internal MassRecordingsTester(string a_mrPath)
//        {
//            m_badRecordings = new List<BadRecordingEntry>();
//            m_timeToStopMassRecordings = DateTime.Now.AddHours(2.5);
//            m_startTime = DateTime.Now;
//            m_emailLogger = new StringBuilder();
//        }

//        // Subdirectories of the playback directory
//        private string m_playbackWorkingFolderRoot;
//        //private string m_playbackExeFolder;

//        private readonly DateTime m_timeToStopMassRecordings; //2.5 hours after start
//        private readonly DateTime m_startTime;

//        //int m_errorLogCount;
//        private int m_unassignedLogCount;
//        private int m_warningLogCount;
//        private int m_processErrorCount;
//        //int m_unitTestErrorCount;
//        private int m_fileCopyCount;
//        private int m_foldersCreatedCount;
//        private long m_sessionId;
//        private string m_runLocation = "";
//        private PT.Common.SqlServer.DatabaseConnections m_dbConnector;
//        private object m_dbLock = new object();
//        private readonly List<BadRecordingEntry> m_badRecordings;
//        private StringBuilder m_emailLogger;
//        private bool m_saveToEmail;
//        private int m_playerCount;
//        System.Collections.Concurrent.ConcurrentBag<Tuple<Exception, string>> m_taskErrors = new System.Collections.Concurrent.ConcurrentBag<Tuple<Exception, string>>();

//        public void TestRecordings()
//        {
//            try
//            {
//#if DEBUG
//                //Uncomment this to debug when running the executable outside Visual Studio.
//                //Debugger.Break();
//#endif

//                WriteConfigMessages();

//               //Database Initializations. Insert a new instance row. Get the current SessionId
//                DateTime startTime = DateTime.Now;
//                try
//                {
//                    m_dbConnector = new DatabaseConnections(m_currentConfig.DBConnectionString);
//                    string computerName = Environment.MachineName;
//                    string newSession = String.Format(SqlStrings.NewInstanceBase, startTime, Filtering.FilterString(computerName), Filtering.FilterString(m_currentConfig.RunMode.ToString()));
//                    m_dbConnector.SendSQLTransaction(new string[] {newSession});
//                    string selectId = String.Format(SqlStrings.SelectInstanceId, startTime);
//                    DataTable dt = m_dbConnector.SelectSQLTable(selectId);
//                    m_sessionId = Convert.ToInt64(dt.Rows[0][0].ToString());
//                    m_runLocation = dt.Rows[0][1].ToString();
//                }
//                catch (Exception e)
//                {
//                    throw new Exception("Unable to get SessionId from database. " + PT.Common.Extensions.ExceptionExtensions.GetExceptionFullMessage(e));
//                }

//                // Synchronize LocalCopy of recordings with Master copy unless the master copy config value was left blank.
//                PrepareRecordings(m_currentConfig.RecordingsDirectory);

//                //CreatePlaybackDirectory();

//                //Creates and waits on tasks             
//                CreateTasks(m_currentConfig.RecordingsDirectory);
//                TimeSpan runDuration = DateTime.Now - startTime;

//                //Write Messages for cleanup
//                int errorCount = WriteLogsAndCountErrors();

//                //Run unitTest comparisons
//                WriteSeparatorLine(0);
//                if (m_currentConfig.SendKPIEmail)
//                {
//                    m_saveToEmail = true;
//                }
//                WriteMessage("Session Comparison Tests");
//                CompareKpis();

//                WriteMessage("-------");
//                WriteMessage("Run Duration: " + runDuration);
//                WriteEmail();

//                if (m_currentConfig.RunMode == MRConfiguration.ERunModes.Manual)
//                {
//                    WriteMessage("Press <Enter> to exit");
//                    Console.ReadLine();
//                }

//                Environment.Exit(errorCount);
//            }
//            catch (Exception e)
//            {
//                //Global excception handler 
//                ExitWithError(e);
//            }
//        }

//        /// <summary>
//        /// Sends KPI Comparison email to Development.  
//        /// Program waits until confirmation of delivery or failure.
//        /// </summary>
//        private void WriteEmail()
//        {
//            if(m_currentConfig.SendKPIEmail)
//            {
//                List<string> targets = new List<string>();
//                targets.Add("Development@planettogether.com");
//                string emailSubject = "APS Build KPI Comparison Results";
//                using (Emailer email = Emailer.CreateWithPTSmtpSettings())
//                {
//                    email.NewEmail(emailSubject, m_emailLogger.ToString(), targets, "System@Planettogether.com");
//                    email.Send();
//                }
//            }
//        }

//        private void CompareKpis()
//        {
//            try
//            {
//                if (!m_currentConfig.CompareKPIsAgainstLastRun && !m_currentConfig.CompareKPIsAgainstBase)
//                {
//                    WriteMessage("Not comparing KPIs");
//                    return;
//                }

//                if (m_currentConfig.CompareKPIsAgainstBase)
//                {
//                    WriteMessage("Comparing KPIs against the base session");
//                    RunKPIComparison(m_sessionId, m_currentConfig.BaseSessionId);
//                }

//                if (m_currentConfig.CompareKPIsAgainstLastRun)
//                {
//                    //DataTable t1;
//                    long session1 = m_sessionId;
//                    long session2 = 0;
//                    try
//                    {
//                        lock (m_dbLock)
//                        {
//                            DataTable t2 = m_dbConnector.SelectSQLTable(SqlStrings.SelectSecondLastSessionid(m_currentConfig.RunMode.ToString(), m_runLocation));
//                            session2 = Convert.ToInt64(t2.Rows[0][0]);
//                        }
//                    }
//                    catch
//                    {
//                        WriteMessage("Unable to retrieve session info for " + m_currentConfig.RunMode + " run from " + m_runLocation);
//                        return;
//                    }

//                    if (m_currentConfig.CompareKPIsAgainstBase)
//                    {
//                        //Add some spacing for readability
//                        WriteMessage(Environment.NewLine + Environment.NewLine);
//                    }
//                    WriteMessage("Comparing KPIs against the last run");
//                    RunKPIComparison(session1, session2);               
//                }
//            }
//            catch (Exception e)
//            {
//                WriteMessage("Unknown Error: " + e.Message);
//            }
//        }

//        private void RunKPIComparison(long a_sessionId1, long a_sessionId2)
//        {
//            try
//            {
//                List<string> comparisonMessages = new List<string>();

//                KpiRetriever comparisonBuilder = new KpiRetriever(m_currentConfig.DBConnectionString);
//                List<KpiComparison> kpiSets;
//                try
//                {
//                    kpiSets = comparisonBuilder.RetrieveKpi(a_sessionId1, a_sessionId2);
//                }
//                catch (Exception e)
//                {
//                    throw new Exception("Failed to retrieve KPI data: " + e.Message);
//                }

//                for (int i = 0; i < kpiSets.Count; i++)
//                {
//                    if (kpiSets[i].Existence == KpiComparison.ExistenceStatus.BothExist)
//                    {
//                        try
//                        {
//                            kpiSets[i].RunComparison();

//                            if (kpiSets[i].ComparisonStatus != KpiComparison.DifferenceStatus.NoDifference)
//                            {
//                                string msg = kpiSets[i].GetSimpleErrorMessage();
//                                comparisonMessages.Add(msg);
//                            }
//                        }
//                        catch (Exception e)
//                        {
//                            comparisonMessages.Add("Unable to calculate Kpis for path: " + kpiSets[i].GetPathName() + ". Error: " + e.Message);
//                        }
//                    }
//                    else
//                    {
//                        comparisonMessages.Add(kpiSets[i].GetPathName() + " recording didn't exist for both run instances. Can't compare KPIs");
//                    }
//                }

//                WriteMessage("KPI Comparison messages. Session " + a_sessionId1 + " , " + a_sessionId2 + ": " + comparisonMessages.Count);
//                if (comparisonMessages.Count == 0)
//                {
//                    WriteMessage("All Comparisons succeeded without messages");
//                }
//                for (int i = 0; i < comparisonMessages.Count; i++)
//                {
//                    WriteMessage(comparisonMessages[i]);
//                }
//            }
//            catch (Exception e)
//            {
//                WriteMessage("Unknown Error: " + e.Message);
//            }
//        }

//        private void WriteConfigMessages()
//        {
////#if DEBUG
////            WriteMessage("DEBUG");
////#else 
////            WriteMessage("RELEASE");
////#endif
////            WriteMessage(m_currentConfig.RunMode.ToString());
////            if (m_currentConfig.InstanceName == "")
////            {
////                WriteMessage("No Instance was provided. Default constructor values will be used.");
////            }
////            else
////            {
////                WriteMessage(string.Format("Instance Name = {0}, Instance Version = {1}", m_currentConfig.InstanceName, m_currentConfig.Version));
////            }

////            WriteMessage(String.Format("Playback Directory: {0}", m_currentConfig.PlaybackDirectory));
////            if (m_currentConfig.MasterCopy != null)
////            {
////                WriteMessage(String.Format("MasterCopy: {0}", m_currentConfig.MasterCopy));
////            }
////            else
////            {
////                WriteMessage("MasterCopy: No Copy");
////            }
////            WriteMessage(String.Format("SourceFilesLocation: {0}", m_currentConfig.PTComponentsLocation));
////            WriteMessage(String.Format("ConcurrencyType: {0}", m_currentConfig.ConcurrencyType));
////            if (m_currentConfig.Concurrencymultiplyer != -1)
////            {
////                WriteMessage(String.Format("ConcurrencyValue: {0}", m_currentConfig.Concurrencymultiplyer));
////            }
////            else
////            {
////                m_currentConfig.Concurrencymultiplyer = 1;
////                WriteMessage("Missing Configuration: ConcurrencyMultiplier. Using default value: 1");
////            }

////            // ReSharper disable once CompareOfFloatsByEqualityOperator -1 is set specifically, and will not be affected by presission loss
////            if (m_currentConfig.RequiredFreeMemory != -1)
////            {
////                WriteMessage(String.Format("MinMemory: {0} GB", m_currentConfig.RequiredFreeMemory / Math.Pow(1024, 3)));
////            }
////            else
////            {
////                m_currentConfig.RequiredFreeMemory = 3;
////                WriteMessage("Missing Configuration: RequiredMinimumMemory. Using default value: 3");
////            }

////            if (m_currentConfig.ExclusionList.Count > 0)
////            {
////                WriteMessage("Excluding the following recordings: ");
////                foreach (string exclusion in m_currentConfig.ExclusionList)
////                {
////                    WriteMessage(exclusion);
////                }
////            }
//        }

//        private int WriteLogsAndCountErrors()
//        {
//            DataTable dt;
//            lock (m_dbLock)
//            {
//                dt = m_dbConnector.SelectSQLTable(String.Format(SqlStrings.SelectPlayerEntries, m_sessionId));
//            }
//            int numberOfPlayersStarted = dt.Rows.Count;

//            WriteMessage(string.Format("{0} tests run", numberOfPlayersStarted));
//            //WriteMessage(string.Format("There were {0} estimated scenarios.", m_plannedRecordingsToRun.Count));

//            //Select Database entries
//            List<PlayerInstanceRow> playerRows = new List<PlayerInstanceRow>();
//            foreach (DataRow row in dt.Rows)
//            {
//                playerRows.Add(new PlayerInstanceRow(row));
//            }

//            WriteMessage("");
//            List<Tuple<long, string>> completedMessages = new List<Tuple<long, string>>();
//            List<string> incompleteMessages = new List<string>();
//            List<string> mrMessages = new List<string>();
//            List<string> alertsMessages = new List<string>();
//            List<string> warningMessages = new List<string>();
//            foreach (PlayerInstanceRow instance in playerRows)
//            {
//                if (instance.ExitCode == int.MinValue)
//                {
//                    incompleteMessages.Add(instance.RecordingPath);                    
//                }

//                if (instance.EndTime != DateTime.MinValue)
//                {
//                    completedMessages.Add(new Tuple<long, string>((instance.EndTime - instance.StartTime).Ticks, instance.RecordingPath));
//                }
//                else if(instance.ExitCode != int.MinValue)
//                {
//                    mrMessages.Add(instance.RecordingPath + " | Completed but the player did not log completion");
//                }

//                if (instance.ErrorFiles)
//                {
//                    alertsMessages.Add(instance.RecordingPath);
//                }
//                else if (instance.WarningFiles)
//                {
//                    warningMessages.Add(instance.RecordingPath);
//                }
//            }

//            WriteMessage("Completed Recordings: " + completedMessages.Count);
//            completedMessages = completedMessages.OrderBy(x => x.Item1).ToList();
//            foreach (Tuple<long, string> message in completedMessages)
//            {
//                WriteMessage("Run: " + new TimeSpan(message.Item1) + " | " + message.Item2);
//            }
//            WriteMessage("");
//            WriteMessage("Incomplete Recordings: " + incompleteMessages.Count);
//            foreach (string message in incompleteMessages)
//            {
//                WriteMessage(message);
//            }

//            WriteMessage("");
//            WriteMessage("Recordings with Error Alerts: " + alertsMessages.Count);
//            foreach (string message in alertsMessages)
//            {
//                WriteMessage(message);
//            }

//            WriteMessage("");
//            List<string> failedToStartList = new List<string>();
//            //foreach (string path in m_plannedRecordingsToRun)
//            //{
//            //    bool found = false;
//            //    foreach (PlayerInstanceRow instance in playerRows)
//            //    {
//            //        if (instance.RecordingPath == path)
//            //        {
//            //            found = true;
//            //            break;
//            //        }
//            //    }
//            //    if (!found)
//            //    {
//            //        failedToStartList.Add(path);
//            //    }
//            //}
//            WriteMessage("Recordings failed to start: " + failedToStartList.Count);
//            foreach (string message in failedToStartList)
//            {
//                WriteMessage(message);
//            }

//            WriteMessage("");
//            WriteMessage("Unknown Issues: " + mrMessages.Count);
//            foreach (string message in mrMessages)
//            {
//                WriteMessage(message);
//            }

//            WriteMessage("");
//            WriteMessage("Recordings with only non error Alerts: " + warningMessages.Count);
//            foreach (string message in warningMessages)
//            {
//                WriteMessage(message);
//            }

//            WriteMessage("");
//            //WriteMessage(string.Format("Error Log File Count={0}", m_errorLogCount));
//            WriteMessage(string.Format("Unassigned Log File Count={0}", m_unassignedLogCount));
//            //WriteMessage(string.Format("Warning Log File Count={0}", m_warningLogCount));
//            //WriteMessage(string.Format("Completed {0} of estimated {1} tests", m_completedRecordings, m_plannedRecordingsToRun.Count));
//            //WriteMessage(string.Format("Unit Test Error Count={0}", m_unitTestErrorCount));
//            WriteMessage(string.Format("Process Bad Return Value Count={0}", m_processErrorCount));

//            List<string> playerErrorsList = new List<string>();
//            try
//            {
//                lock (m_dbLock)
//                {
//                    dt = m_dbConnector.SelectSQLTable(String.Format(SqlStrings.SelectPlayerExceptions, m_sessionId));
//                }
//                foreach (DataRow row in dt.Rows)
//                {
//                    playerErrorsList.Add(row["RecordingPath"].ToString());
//                    playerErrorsList.Add(row["ExceptionMessage"].ToString());
//                    playerErrorsList.Add(row["ExceptionTrace"].ToString());
//                }
//                WriteMessage("Player Errors: " + playerErrorsList.Count / 3);
//                foreach (string message in playerErrorsList)
//                {
//                    WriteMessage(message);
//                }
//            }
//            catch (Exception e)
//            {
//                //ExitWithError(e);
//            }

//            //Note: If these recording messages are not longer needed (the database format is working), then the code used to 
//            //  generate these badRecordings can be removed. Possibly along with the TaskNumber all together.

//            //string lastBadRecordingFolder = "";
//            //for (int badRecordingI = 0; badRecordingI < m_badRecordings.Count; ++badRecordingI)
//            //{
//            //    BadRecordingEntry bre = (BadRecordingEntry)m_badRecordings[badRecordingI];
//            //    if (lastBadRecordingFolder != bre.RecordingDirectory)
//            //    {
//            //        WriteBadSeparatorLine();
//            //    }
//            //    lastBadRecordingFolder = bre.RecordingDirectory;
//            //    WriteMessage(bre.ToString());
//            //}

//            WriteMessage("");
//            WriteMessage("Debug Info");
//            WriteMessage("alertsMessages: " + alertsMessages.Count);
//            WriteMessage("incompleteMessages: " + incompleteMessages.Count);
//            WriteMessage("m_processErrorCount: " + m_processErrorCount);
//            WriteMessage("failedToStartList: " + failedToStartList.Count);
//            WriteMessage("playerErrorsList: " + playerErrorsList.Count / 3);

//            int returnCode = alertsMessages.Count + incompleteMessages.Count + m_processErrorCount + failedToStartList.Count + (playerErrorsList.Count / 3);
//            WriteMessage("returnCode: " + returnCode);
//            WriteMessage("");

//            return returnCode;
//        }

//        /// <summary>
//        /// Calls FindAndTestRecordings to create tasks.
//        /// Waits on the last tasks created and then returns
//        /// </summary>
//        private void CreateTasks(string a_recordingsDirectory)
//        {
//            try
//            {
//                FindAndTestRecordings(a_recordingsDirectory);
//            }
//            catch (Exception e)
//            {
//                WriteMessage("An Exception happened in FindAndTestRecordings.");
//                throw new Exception("An Exception happened in FindAndTestRecordings.", e);
//            }

//            //List<Task> waitTasks = new List<Task>();
//            //for (int taskI = 0; taskI < m_tasks.Length; ++taskI)
//            //{
//            //    if (m_tasks[taskI] != null)
//            //    {
//            //        waitTasks.Add(m_tasks[taskI]);
//            //    }
//            //}
//            //Task[] waitTaskArray = waitTasks.ToArray();

//            try
//            {
//                //Task.WaitAll(waitTaskArray, -1);
//            }
//            catch (Exception e)
//            {
//                string message = "Unhandled exception from Player " + e.GetExceptionFullMessage();
//                WriteMessage(message);
//            }

//            foreach (Tuple<Exception, string> taskError in m_taskErrors)
//            {
//                string message = "Exception from Player running " + taskError.Item2 + taskError.Item1.GetExceptionFullMessage();
//                WriteMessage(message);
//            }
//        }

//        private void PrepareRecordings(string a_recordingsDirectory)
//        {
//            //if (m_currentConfig.MasterCopy != "")
//            //{
//            //    if (MapMastercopy())
//            //    {
//            //        //Mapping succeeded. Write messages.
//            //        WriteMessage("Synching local recordings folder with Master Copy");
//            //        WriteMessage(string.Format("Master Copy location = {0}", m_currentConfig.MasterCopy));
//            //        WriteMessage(string.Format("Local Copy location = {0}", a_recordingsDirectory));

//            //        UpdateLocalCopy(m_currentConfig.MasterCopy, a_recordingsDirectory, false);

//            //        WriteMessage(string.Format("Number of new folders created = {0}", m_foldersCreatedCount));
//            //        WriteMessage(string.Format("Number of files copied = {0}", m_fileCopyCount));
//            //    }
//            //}
//            //else
//            {
//                WriteMessage("Recording Sync skipped. Dissabled in configuration file");
//            }

//            WriteMessage(string.Format("RecordingsDirectory: {0}", a_recordingsDirectory));
//            //WriteMessage(string.Format("Estimated Number of Scenarios to Play: {0}", m_plannedRecordingsToRun.Count));
//        }

//        private bool MapMastercopy()
//        {
//            //New method. The files should be readable to all users.
//            try
//            {
//                //int directories = Directory.GetDirectories(m_currentConfig.MasterCopy).Length;
//                //if (directories > 0)
//                //{
//                //    //Has access to read files
//                //    return true;
//                //}
//                //else
//                //{
//                //    WriteMessage("Recoding Sync skipped: No master copy directories exist.");
//                //    return false;
//                //}
//            }
//            catch (Exception e)
//            {
//                WriteMessage("Recoding Sync skipped: An error occurred while trying to map the Master Copy folder.");
//                WriteMessage(e.Message);
//                return false;
//            }

//            //string driveLetter = "r:\\";
//            //if (!Directory.Exists(driveLetter))
//            //{
//            //    // Map network drive for MasterCopy
//            //    Process p = new Process();
//            //    p.StartInfo.FileName = "net.exe";
//            //    string args = string.Format("use r: {0} /user:PT\\MR OptimizeREC101# /persistent:YES", m_currentConfig.MasterCopy);
//            //    p.StartInfo.Arguments = args;
//            //    p.StartInfo.CreateNoWindow = true;
//            //    p.StartInfo.RedirectStandardOutput = true;
//            //    p.StartInfo.UseShellExecute = false; 
//            //    if (m_currentConfig.RunMode != MRConfiguration.RunModes.Manual)
//            //    {
//            //        try
//            //        {
//            //            p.Start();
//            //            string output = p.StandardOutput.ReadToEnd();
//            //            p.WaitForExit();
//            //            if (p.ExitCode != 0)
//            //            {
//            //                WriteMessage("Recoding Sync skipped: An error occurred while trying to map the Master Copy folder.");
//            //                WriteMessage(output);
//            //                return false;
//            //            }
//            //        }
//            //        catch (Exception e)
//            //        {
//            //            WriteMessage("Recoding Sync skipped: " + e.Message);
//            //            return false;
//            //        }
//            //    }
//            //}
//            //return true;
//        }

//        /// <summary>
//        /// Method to recursively copy any new or updated recordings files from MasterCopy direcotry to Local Working directory. It will also unzip any zipped files.
//        /// </summary>
//        /// <param name="a_sourcePath">Path of the directory from which to copy</param>
//        /// <param name="a_destPath">Path of the directory to copy to</param>
//        /// <param name="a_skipFlag">Whether to check file/folder modified dates for any updates. This will be true when a directory has just been copied during update operation.</param>
//        private void UpdateLocalCopy(string a_sourcePath, string a_destPath, bool a_skipFlag)
//        {
//            try
//            {
//                bool skipExistanceCheck = a_skipFlag;
//                if (!Directory.Exists(a_destPath))
//                {
//                    Directory.CreateDirectory(a_destPath);

//                    skipExistanceCheck = true;
//                    m_foldersCreatedCount++;
//                }

//                // Create flag used for cleaning up recordings that were removed.
//                string flagFileName = "MasterCopy"; //string.Format("MasterCopy", DateTime.Today.Month, DateTime.Today.Day, DateTime.Today.Year);
//                string flagFilePath = Path.Combine(a_destPath, flagFileName);
//                try
//                {
//                    File.Create(flagFilePath);
//                }
//                catch (IOException ioe)
//                {
//                    //ExitWithError("Could not create flag file: " + ioe.Message);
//                }
//                catch (Exception ee)
//                {
//                    throw new Exception("Could not create flag file", ee);
//                }

//                string[] files = Directory.GetFiles(a_sourcePath);

//                foreach (string sourcefile in files)
//                {
//                    try
//                    {
//                        string fileName = Path.GetFileName(sourcefile);
//                        string destFile = Path.Combine(a_destPath, fileName);
//                        if (!skipExistanceCheck && File.Exists(destFile) && File.GetLastWriteTime(sourcefile) == File.GetLastWriteTime(destFile))
//                        {
//                            continue;
//                        }
//                        File.Copy(sourcefile, destFile, true);
//                        m_fileCopyCount++;
//                        if (destFile.ToUpper().EndsWith(".ZIP"))
//                        {
//                            string newDir = Path.Combine(a_destPath, Path.GetFileNameWithoutExtension(destFile));
//                            PT.Common.Compression.Zip.Extract(destFile, newDir);
//                        }
//                    }
//                    catch (Exception e)
//                    {
//                        WriteMessage("Unable to load file: " + sourcefile + Environment.NewLine + e.Message);
//                        throw;
//                    }
//                }

//                string[] folders = Directory.GetDirectories(a_sourcePath);
//                if (!skipExistanceCheck)
//                {
//                    List<string> foldersList = new List<string>(folders);
//                    string[] existingFolders = Directory.GetDirectories(a_destPath);
//                    if (existingFolders.Length > foldersList.Count)
//                    {
//                        foreach (string existingfolder in existingFolders)
//                        {
//                            bool mayNeedToDelete = true;
//                            for (int i = 0; i < foldersList.Count; i++)
//                            {
//                                string existingDirName = new DirectoryInfo(existingfolder).Name;
//                                string folderDirName = new DirectoryInfo(foldersList[i]).Name;
//                                if (existingDirName.Equals(folderDirName))
//                                {
//                                    foldersList.RemoveAt(i);
//                                    mayNeedToDelete = false;
//                                    break;
//                                }
//                            }
//                            if (mayNeedToDelete)
//                            {
//                                string parentPath = new DirectoryInfo(existingfolder).Parent.FullName;
//                                string[] filesInParentFolder = Directory.GetFiles(parentPath);
//                                bool del = true;
//                                foreach (string parentFile in filesInParentFolder)
//                                {
//                                    if (Path.GetFileNameWithoutExtension(parentFile).Equals(existingfolder))
//                                    {
//                                        del = false;
//                                        break;
//                                    }
//                                }
//                                if (del)
//                                {
//                                    bool delete = false;
//                                    string[] filesNotInMasterCopy = Directory.GetFiles(existingfolder);
//                                    foreach (string fileNotInMC in filesNotInMasterCopy)
//                                    {
//                                        string fileName = new DirectoryInfo(fileNotInMC).Name;
//                                        if (fileName.Equals("MasterCopy"))
//                                        {
//                                            delete = true;
//                                            break;
//                                        }
//                                    }
//                                    if (delete)
//                                    {
//                                        try
//                                        {
//                                            Directory.Delete(existingfolder, true);
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            throw new Exception("Couldn't remove folder", ex);
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }

//                foreach (string sourceFolder in folders)
//                {
//                    string folderName = Path.GetFileName(sourceFolder);
//                    string destFolder = Path.Combine(a_destPath, folderName);
//                    UpdateLocalCopy(sourceFolder, destFolder, skipExistanceCheck);
//                }
//            }
//            catch (IOException ioe)
//            {
//                WriteMessage("Sync Recordings skipped: Unable to access or process mastercopy files: " + ioe.Message);
//            }
//            catch (UnauthorizedAccessException uae)
//            {
//                WriteMessage("Sync Recordings skipped: Insufficient credentials to access or process mastercopy files: " + uae.Message);
//            }
//            catch (Exception ex)
//            {
//                WriteMessage("Sync Recordings skipped: Unable to process MasterCopy " + ex.Message);
//            }
//        }

//        /// <summary>
//        /// Creates a task for each of the recordings folders
//        /// </summary>
//        /// <param name="a_directory">The base directory to search for recordings</param>
//        private void FindAndTestRecordings(string a_directory)
//        {

//            //LaunchPlayer(a_directory, taskNumber, m_playerCount);

//            string[] subdirectories = Directory.GetDirectories(a_directory);
//            foreach (string folder in subdirectories)
//            {
//                FindAndTestRecordings(folder);
//            }
//        }

//        /// <summary>
//        /// Entry point for each new task. Waits on available cpu cores and memory, then tests a recording.
//        /// </summary>
//        /// <param name="a_directory">Working directory for the task</param>
//        /// <param name="a_taskNumber">Task index and identifier</param>
//        private void LaunchPlayer(string a_directory, int a_taskNumber, int a_playerCount)
//        {
//            int maxWaitLoops = 10;

//            try
//            {
//                //2012.09.28: Removed random wait
//                //Random randomWait = new Random();

//                //Staggar tasks so that several tasks don't start at the same which could bypass memory check as memory wouldn't be allocated yet.
//                //Thread.Sleep(randomWait.Next(18000));
//                //Thread.Sleep(9000);

//                while (true)
//                {
//                    long currentAvailableMemory;

//                    //Get Available memory.
//                    using (PerformanceCounter pc = new PerformanceCounter("Memory", "Available Bytes"))
//                    {
//                        currentAvailableMemory = Convert.ToInt64(pc.NextValue());
//                    }

//                    //Check for available memory
//                   // if (currentAvailableMemory > (long)m_currentConfig.RequiredFreeMemory)
//                    {
//                        break;
//                    }

//                    //Wait for another task to end and free memory.
//                    //Thread.Sleep(randomWait.Next(maxWaitTimeMS));
//                    Thread.Sleep(16000);
//                    maxWaitLoops--;

//                    //Run anyways if memory hasn't been available for too long.
//                    if (maxWaitLoops == 0)
//                    {
//                        break;
//                    }
//                }
//            }
//            finally
//            {
//                //Check memory failed or memory is available
//                TestRecording(a_directory, a_taskNumber, a_playerCount);
//            }
//        }

//        private void TestRecording(string a_recordingPath, int a_taskNumber, int a_playerCount)
//        {
//            //RunRecording(a_recordingPath, playbackWorkingFolder, a_playerCount);

//            //MoveErrorsFolder(a_recordingPath, playbackWorkingFolder);
//            //MoveUnitTestFolders(a_recordingPath, a_taskNumber);
//        }

//        private void RunRecording(string a_recordingPath, string a_playbackWorkingFolder, int a_playerCount)
//        {
//            string errorMsg = "";
//            //StoreMessages(c_separatorLine, a_recordingNumber);
//            //StoreMessages(string.Format("Test number {0} out of {1} scenarios. [{2}]", a_testNumber, m_initialScenariosCount, a_recordingNumber), a_recordingNumber);
//            //StoreMessages(string.Format("Starting test number {0} from {1} at {2}.", a_testNumber, a_recordingPath, DateTime.Now.ToString()), a_recordingNumber);
//            Process p = new Process();
//            //string exePath = m_currentConfig.GetMRPlayerPath();
//            //p.StartInfo.FileName = exePath;
//            //p.StartInfo.WorkingDirectory = m_currentConfig.PlayerFolderPath;
//            //p.StartInfo.UseShellExecute = false;
//            //p.StartInfo.Arguments = GetPlayerParameters(m_sessionId, m_currentConfig.DBConnectionString, a_recordingPath, a_playbackWorkingFolder, m_currentConfig.InstanceName, m_currentConfig.Version,
//            //    m_currentConfig.PTComponentsLocation, m_currentConfig.SMComputerNameOrIP, m_currentConfig.SMPort, m_currentConfig.ClientTimeout, m_currentConfig.InterfaceTimeout, m_currentConfig.LoadCustomization,
//            //    m_currentConfig.InterfaceURL, m_currentConfig.Port + a_playerCount);

//            try
//            {
//                if (DateTime.Now > m_timeToStopMassRecordings)
//                {
//                    TimeSpan runDuration = DateTime.Now - m_startTime;

//                    //Write Messages for cleanup
//                    WriteLogsAndCountErrors();

//                    WriteMessage("-------");
//                    WriteMessage("Run Duration: " + runDuration);
//                    //ExitWithError("MR Ran out of time to run. Exiting.");
//                }

//                int maxDurationMS = (int)(m_timeToStopMassRecordings - DateTime.Now).TotalMilliseconds;
//                WriteMessage("Starting: " + a_recordingPath + ". MaxRunTime: " + TimeSpan.FromMilliseconds(maxDurationMS));
//                p.Start();
//                if (!p.WaitForExit(maxDurationMS))
//                {
//                    long memoryUsageMB = 0;
//                    TimeSpan cpuTime = new TimeSpan(0);
//                    float cpuUsage = 0;
//                    try
//                    {
//                        //Get process usages. 
//                        memoryUsageMB = p.PrivateMemorySize64 / (2 * 1024);
//                        cpuTime = p.TotalProcessorTime;
//                        //Player is multithreaded and can return more than 100%.
//                        using (PerformanceCounter counter = new PerformanceCounter("Process", "% Processor Time", p.ProcessName, true))
//                        {
//                            counter.NextValue();
//                            Thread.Sleep(2000);
//                            cpuUsage = counter.NextValue();
//                        }
//                    }
//                    finally
//                    {
//                        p.Kill();
//                        string updateInstance = String.Format(SqlStrings.UpdateInstanceBase, memoryUsageMB, cpuTime.Ticks, cpuUsage, m_sessionId, Filtering.FilterString(a_recordingPath));
//                        string updateErrorLog = String.Format(SqlStrings.InsertPlayerExceptions, m_sessionId, Filtering.FilterString(a_recordingPath), "Recoring Timed Out.", "none");
//                        lock (m_dbLock)
//                        {
//                            m_dbConnector.SendSQLTransaction(new string[] { updateInstance, updateErrorLog });
//                        }
//                    }
//                    //WriteMessage("Timed Out: " + a_recordingPath);
//                    return;
//                }
//                else
//                {
//                    switch (p.ExitCode)
//                    {
//                        case 0:
//                            //normal exit
//                            WriteMessage("Finished with code " + p.ExitCode + " : " + a_recordingPath);
//                            break;
//                        case 1:
//                            break;
//                        case 2:
//                            //invalid scenario (newer serialization version)
//                            WriteMessage("Finished with code " + p.ExitCode + " : Invalid Scenario. Recording skipped : " + a_recordingPath);
//                            break;
//                        case 3:
//                            //Error closing player
//                            WriteMessage("Finished with code " + p.ExitCode + " : Error closing player : " + a_recordingPath);
//                            break;
//                        case 4:
//                            //Service exited with error
//                            WriteMessage("Finished with code " + p.ExitCode + " : System crashed : " + a_recordingPath);
//                            break;
//                    }
//                    string updateErrorCodeMsg = String.Format(SqlStrings.UpdateExitCode, p.ExitCode, m_sessionId, Filtering.FilterString(a_recordingPath));
//                    lock (m_dbLock)
//                    {
//                        m_dbConnector.SendSQLTransaction(new string[] { updateErrorCodeMsg });
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                WriteMessage(string.Format("NON ZERO EXIT CODE with error {0}. {1}", errorMsg, a_recordingPath));
//                errorMsg = PT.Common.Extensions.ExceptionExtensions.GetExceptionFullMessage(e);
//            }

//            if (errorMsg == "")
//            {
//                //StoreMessages(string.Format("Test {0} Completed at {1}.", a_testNumber, DateTime.Now.ToString()),a_recordingNumber);
//                return;
//            }

//            ++m_processErrorCount;
//            string errMsg = string.Format("NON ZERO EXIT CODE: Completed at {0} with error {1}.", DateTime.Now, errorMsg);
//            //m_badRecordings.Add(new BadRecordingEntry(a_recordingPath, errMsg));
//            //StoreMessages(errMsg, a_taskNumber);
//            try
//            {
//                string updateErrorMsg = String.Format(SqlStrings.InsertPlayerExceptions, m_sessionId, Filtering.FilterString(a_recordingPath), errMsg, "none");
//                lock (m_dbLock)
//                {
//                    m_dbConnector.SendSQLTransaction(new string[] { updateErrorMsg });
//                }
//            }
//            catch (Exception e)
//            {
//                //WriteMessage(string.Format("NON ZERO EXIT CODE: Completed at {0} with oringal error {1}. {2}Logging error: {3} " , DateTime.Now, errorMsg, Environment.NewLine, e.GetExceptionFullMessage()));
//            }
//        }

//        private void WriteMessage(string a_comparingKpisAgainstTheBaseSession)
//        {
//            throw new NotImplementedException();
//        }

//        private string GetPlayerParameters(long a_sessionid, string a_dbConnectionString, string a_recordingDirToLoadFrom, string a_workingDir, string a_instanceName, string a_version, string a_baseDirectory,
//                                           string a_smComputerIp, string a_smPort, int a_clientTimeout, int a_interfaceTimeout, string a_loadCusts, string a_interfaceUrl, int a_port)
//        {
//            return string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\" \"{6}\" \"{7}\" \"{8}\" \"{9}\" \"{10}\" \"{11}\" \"{12}\" \"{13}\" \"{14}\""
//                , a_sessionid, a_dbConnectionString, a_recordingDirToLoadFrom, a_workingDir, a_instanceName, a_version, a_baseDirectory,
//                a_smComputerIp, a_smPort, a_clientTimeout, a_interfaceTimeout, a_loadCusts, a_interfaceUrl, a_port, false);
//        }
//    }
//}

