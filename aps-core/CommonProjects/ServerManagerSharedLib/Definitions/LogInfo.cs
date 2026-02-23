using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.ServerManagerSharedLib.Utils;

namespace PT.ServerManagerSharedLib.Definitions
{
    public class LogInfo
    {
        public enum EServiceLogTypes { System, Interface, ExtraServices, SchedulingAgent, ClientUpdater, Packages, Licensing };

        public LogInfo(string a_filePath, EServiceLogTypes a_serviceType)
        {
            bool read = false;
            int maxTries = 5;
            int tryCount = 0;
            while (!read && tryCount < maxTries)
            {
                try
                {
                    FileUtils.TextFile txtFile = new FileUtils.TextFile(a_filePath, System.IO.FileShare.Read);
                    read = true;

                    Text = txtFile.Text;
                }
                catch (IOException)
                {
                    //locked
                    System.Threading.Thread.Sleep(500);
                }
                finally
                {
                    tryCount++;
                }
            }

            FilePath = a_filePath;
            LastWriteDateTime = File.GetLastWriteTime(a_filePath);
            LogFileName = Path.GetFileName(a_filePath);
            ServiceType = a_serviceType;

            if (!read)
            {
                Text = "[FILE LOCKED, COULDN'T READ CONTENTS]";
            }
        }

        string m_text;
        /// <summary>
        /// The text contained in the log.
        /// </summary>
        public string Text
        {
            get { return m_text; }
            internal set { m_text = value; }
        }

        string m_logFileName;
        /// <summary>
        /// The full name of the log file without a path.
        /// </summary>
        public string LogFileName
        {
            get { return m_logFileName; }
            internal set { m_logFileName = value; }
        }

        string m_filePath;
        public string FilePath
        {
            get { return m_filePath; }
            internal set { m_filePath = value; }
        }

        EServiceLogTypes m_serviceType;
        /// <summary>
        /// The service that the log was created for.
        /// </summary>
        public EServiceLogTypes ServiceType
        {
            get { return m_serviceType; }
            internal set { m_serviceType = value; }
        }

        DateTime m_lastWriteDateTime;
        public DateTime LastWriteDateTime
        {
            get { return m_lastWriteDateTime; }
            internal set { m_lastWriteDateTime = value; }
        }
    }
}
