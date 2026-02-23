using System.Runtime.Serialization;

using Microsoft.VisualBasic.FileIO;

using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.Helpers
{
    /// <summary>
    /// Keeps track of logs that are written to all services for the Instance.
    /// </summary>
    [DataContract(Name = "InstanceLogHelper", Namespace = "http://www.planettogether.com")]
    public class InstanceLogHelper : InstanceLogHelperBase
    {
        const string c_logExtensions = "*.log";

        readonly System.IO.FileSystemWatcher m_systemAlertsWatcher = new System.IO.FileSystemWatcher();
        readonly System.IO.FileSystemWatcher m_interfaceAlertsWatcher = new System.IO.FileSystemWatcher();
        readonly System.IO.FileSystemWatcher m_packagesAlertsWatcher = new System.IO.FileSystemWatcher();

        public delegate void LogsWrittenHandler();
        public event LogsWrittenHandler SystemProblemLogWritten;
        public event LogsWrittenHandler InterfaceProblemLogWritten;
        public event LogsWrittenHandler PackagesProblemLogWritten;
        public event LogsWrittenHandler LicensingProblemLogWritten;

        public InstanceLogHelper()
        {
            m_systemAlertsWatcher.Created += new FileSystemEventHandler(SystemAlertsWatcher_Created);
            m_interfaceAlertsWatcher.Created += new FileSystemEventHandler(InterfaceAlertsWatcher_Created);
            m_packagesAlertsWatcher.Created += new FileSystemEventHandler(PackagesAlertsWatcher_Created);
        }

        /// <summary>
        /// Checks whether logs already exist and stores this as a starting point.
        /// Starts to watch all log folders for problem logs.
        /// If any of the Alert folders don't exist yet then the tracking won't start.
        /// </summary>
        /// <param name="a_instance"></param>
        public void CheckForAndStartWatchingForLogs(APSInstanceEntity a_instance)
        {
            SystemHasLogs = LookForLogsAndStartWatchingIfFolderExists(m_systemAlertsWatcher, a_instance.ServicePaths.SystemServiceWorkingDirectory);
            InterfaceHasLogs = LookForLogsAndStartWatchingIfFolderExists(m_interfaceAlertsWatcher, a_instance.ServicePaths.InterfaceServiceWorkingDirectory);
            PackagesHasLogs = LookForLogsAndStartWatchingIfFolderExists(m_packagesAlertsWatcher, a_instance.ServicePaths.PackageWorkingDirectory);
        }

        public bool ClearProblemLogs(APSInstanceEntity a_instance)
        {
            SystemHasLogs = !ClearProblemLogs(a_instance.ServicePaths.SystemServiceWorkingDirectory);
            InterfaceHasLogs = !ClearProblemLogs(a_instance.ServicePaths.InterfaceServiceWorkingDirectory);
            PackagesHasLogs = !ClearProblemLogs(a_instance.ServicePaths.PackageWorkingDirectory);

            return !SystemHasLogs && !InterfaceHasLogs && !PackagesHasLogs;
        }

        public void StopWatchingForLogs(APSInstanceEntity a_instance)
        {
            m_systemAlertsWatcher.EnableRaisingEvents = false;
            m_interfaceAlertsWatcher.EnableRaisingEvents = false;
            m_packagesAlertsWatcher.EnableRaisingEvents = false;
        }

        public LogInfoList GetLogInfos(APSInstanceEntity a_instance)
        {
            return new LogInfoList(a_instance);
        }

        public int GetErrorLogCount(APSInstanceEntity a_instance)
        {
            int count = 0;
            count += GetLogCountForDirectory(a_instance.ServicePaths.SystemServiceWorkingDirectory);
            count += GetLogCountForDirectory(a_instance.ServicePaths.InterfaceServiceWorkingDirectory);
            count += GetLogCountForDirectory(a_instance.ServicePaths.PackageWorkingDirectory);

            return count;
        }

        int GetLogCountForDirectory(string a_folder)
        {
            int count = 0;
            if (Directory.Exists(a_folder))
            {
                string[] allFiles = Directory.GetFiles(a_folder, c_logExtensions, System.IO.SearchOption.AllDirectories);
                for (int i = 0; i < allFiles.Length; i++)
                {
                    string filePath = allFiles.GetValue(i).ToString();
                    if (LogIsProblemLog(filePath))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        void SystemAlertsWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            if (LogIsProblemLog(e.FullPath))
            {
                SystemHasLogs = true;
                SystemProblemLogWritten?.Invoke();
            }
        }

        void InterfaceAlertsWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            if (LogIsProblemLog(e.FullPath))
            {
                InterfaceHasLogs = true;
                InterfaceProblemLogWritten?.Invoke();
            }
        }

        void PackagesAlertsWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            if (LogIsProblemLog(e.FullPath))
            {
                PackagesHasLogs = true;
                PackagesProblemLogWritten?.Invoke();
            }
        }
        
        bool m_systemHasLogs;
        [DataMember]
        internal bool SystemHasLogs
        {
            get { return m_systemHasLogs; }
            private set { m_systemHasLogs = value; }
        }
        
        bool m_interfaceHasLogs;
        [DataMember]
        internal bool InterfaceHasLogs
        {
            get { return m_interfaceHasLogs; }
            private set { m_interfaceHasLogs = value; }
        }
        
        bool m_packagesHasLogs;
        [DataMember]
        internal bool PackagesHasLogs
        {
            get { return m_packagesHasLogs; }
            private set { m_packagesHasLogs = value; }
        }
        
        /// <summary>
        /// Starts watching the folder if it exists and if not already watching.
        /// Returns true if the folder exists and any problem logs are there already.
        /// </summary>
        static bool LookForLogsAndStartWatchingIfFolderExists(System.IO.FileSystemWatcher a_watcher, string a_folder)
        {
            if (Directory.Exists(a_folder))
            {
                if (!a_watcher.EnableRaisingEvents)
                {
                    a_watcher.Path = a_folder;
                    a_watcher.Filter = c_logExtensions;
                    a_watcher.IncludeSubdirectories = true;
                    a_watcher.EnableRaisingEvents = true;
                }

                return ProblemLogsExistInFolderRecursive(a_folder);
            }

            return false;
        }

        /// <summary>
        /// Checks the folder recursively to see if any of the logs are "problem logs".
        /// Returns false if the directory does not exist or no problem logs are found.
        /// </summary>
        static bool ProblemLogsExistInFolderRecursive(string a_directory)
        {
            if (Directory.Exists(a_directory))
            {
                string[] allFiles = Directory.GetFiles(a_directory, c_logExtensions, System.IO.SearchOption.AllDirectories);
                for (int i = 0; i < allFiles.Length; i++)
                {
                    if (LogIsProblemLog(allFiles.GetValue(i).ToString()))
                        return true;
                }

                return false;
            }

            return false;
        }
        
        internal static bool LogIsProblemLog(string a_filePath)
        {
            string logNameUpper = Path.GetFileNameWithoutExtension(a_filePath)?.ToUpper();
            return logNameUpper != null && (logNameUpper.Contains("ERROR") || logNameUpper.Contains("EXCEPTION") || logNameUpper.Contains("PACKAGEINFO"));
        }

        /// <summary>
        /// Clears all "problem" .log files recursively. Sends files to the Recycle Bin.
        /// </summary>
        /// <param name="a_folder"></param>
        /// <returns>True if any problem logs could be cleared.</returns>
        static bool ClearProblemLogs(string a_folder)
        {
            if (Directory.Exists(a_folder))
            {
                string[] allFiles = Directory.GetFiles(a_folder, c_logExtensions, System.IO.SearchOption.AllDirectories);

                bool deletedAllFilesNeeded = true;
                for (int i = 0; i < allFiles.Length; i++)
                {
                    string filePath = allFiles.GetValue(i).ToString();
                    if (LogIsProblemLog(filePath))
                    {
                        try
                        {
                            FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        }
                        catch (Exception)
                        {
                            deletedAllFilesNeeded = false;
                        }
                    }
                }
                return deletedAllFilesNeeded;
            }

            return true; //nothing to clear
        }

        [Serializable]
        public class LogInfoList
        {
            internal LogInfoList(APSInstanceEntity a_instance)
            {
                m_logInfos.Clear();
                CreateLogInfos(a_instance);
            }

            List<LogInfo> m_logInfos = new List<LogInfo>();
            public int Count
            {
                get { return m_logInfos.Count; }
            }

            public LogInfo this[int index]
            {
                get { return m_logInfos[index]; }
            }

            public List<LogInfo> LogList
            {
                get { return m_logInfos; }
            }

            void CreateLogInfos(APSInstanceEntity a_instance)
            {
                CreateLogInfosForFolder(a_instance.ServicePaths.SystemServiceWorkingDirectory, LogInfo.EServiceLogTypes.System);
                CreateLogInfosForFolder(a_instance.ServicePaths.InterfaceServiceWorkingDirectory, LogInfo.EServiceLogTypes.Interface);
                CreateLogInfosForFolder(a_instance.ServicePaths.PackageWorkingDirectory, LogInfo.EServiceLogTypes.Packages);
            }

            void CreateLogInfosForFolder(string a_folder, LogInfo.EServiceLogTypes a_serviceType)
            {
                if (Directory.Exists(a_folder))
                {
                    string[] allFiles = Directory.GetFiles(a_folder, c_logExtensions, System.IO.SearchOption.AllDirectories);
                    for (int i = 0; i < allFiles.Length; i++)
                    {
                        string filePath = allFiles.GetValue(i).ToString();
                        if (LogIsProblemLog(filePath))
                        {
                            LogInfo logInfo = new LogInfo(filePath, a_serviceType);
                            m_logInfos.Add(logInfo);
                        }
                    }
                }
            }
        }
    }
}
