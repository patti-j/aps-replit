using System;
using System.Threading;

using Newtonsoft.Json;

using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class StartupVals
    {
        public StartupVals()
        {
        }

        protected string m_devPackagePath = String.Empty;
        public string DevPackagePath
        {
            get
            {
                return m_devPackagePath;
            }

            set
            {
                m_devPackagePath = value;
            }
        }

        protected string m_workingDirectory;
        public string WorkingDirectory
        {
            get
            {
                return m_workingDirectory;
            }

            set
            {
                m_workingDirectory = value;
            }
        }

        protected string m_serviceName;
        public string ServiceName
        {
            get
            {
                return m_serviceName;
            }

            set
            {
                m_serviceName = value;
            }
        }

        protected int m_port;
        public int Port
        {
            get
            {
                return m_port;
            }

            set
            {
                m_port = value;
            }
        }


        protected EStartType m_startType;
        public EStartType StartType
        {
            get
            {
                return m_startType;
            }

            set
            {
                m_startType = value;
            }
        }

        protected ThreadPriority m_threadPriority;
        public ThreadPriority ServiceThreadPriority
        {
            get { return m_threadPriority; }
            set { m_threadPriority = value; }
        }

        protected bool m_record;
        public bool Record
        {
            get { return m_record; }
            set { m_record = value; }
        }

        protected int m_maxNbrSessionRecordingsToStore;
        public int MaxNbrSessionRecordingsToStore
        {
            get { return m_maxNbrSessionRecordingsToStore; }
            set { m_maxNbrSessionRecordingsToStore = value; }
        }
        protected TimeZoneInfo m_publishTimeZone;
        public TimeZoneInfo PublishTimeZone
        {
            get {
                if (m_publishTimeZone == null)
                    return m_publishTimeZone;
                else
                    return TimeZoneInfo.Utc;
            }
            set { m_publishTimeZone = value; }
        }

        protected int m_maxNbrSystemBackupsToStorePerSession;
        
        public int MaxNbrSystemBackupsToStorePerSession
        {
            get { return m_maxNbrSystemBackupsToStorePerSession; }
            set { m_maxNbrSystemBackupsToStorePerSession = value; }
        }
        protected int m_minutesBetweenSystemBackups;
        public int MinutesBetweenSystemBackups
        {
            get { return m_minutesBetweenSystemBackups; }
            set { m_minutesBetweenSystemBackups = value; }
        }

        protected bool m_nonSequenced;
        public bool NonSequencedTransmissionPlayback
        {
            get { return m_nonSequenced; }
            set { m_nonSequenced = value; }
        }

        protected int m_startingScenarioNbr;
        public int StartingScenarioNumber
        {
            get { return m_startingScenarioNbr; }
            set { m_startingScenarioNbr = value; }
        }

        protected bool m_singleThreaded;
        public bool SingleThreaded
        {
            get { return m_singleThreaded; }
            set { m_singleThreaded = value; }
        }

        protected string m_systemServiceUrl;
        public string SystemServiceUrl
        {
            get
            {
                return m_systemServiceUrl;
            }
            set { m_systemServiceUrl = value; }
        }

        protected string m_interfaceServiceUrl;
        public string InterfaceServiceUrl
        {
            get { return m_interfaceServiceUrl; }
            set { m_interfaceServiceUrl = value; }
        }

        /// <summary>
        /// The timeout interval used on clients. Any connection that doesn't
        /// contact the broadcaster within each interval will be dropped.
        /// </summary>
        protected int m_clientTimeoutSeconds;
        public int ClientTimeoutSeconds
        {
            get { return m_clientTimeoutSeconds; }
            set { m_clientTimeoutSeconds = value; }
        }

        /// <summary>
        /// Seconds after which the connection to interface service times out.
        /// </summary>
        protected int m_webApiTimeoutSeconds;
        public int WebApiTimeoutSeconds
        {
            get { return m_webApiTimeoutSeconds; }
            set { m_webApiTimeoutSeconds = value; }
        }

        protected bool m_runningMassRecordings;
        public bool RunningMassRecordings
        {
            get { return m_runningMassRecordings; }
            set { m_runningMassRecordings = value; }
        }

        //Used as a Primary Key in the MR database
        protected long m_massRecordingsSessionId = 0;
        public long MassRecordingsSessionId
        {
            get { return m_massRecordingsSessionId; }
            set { m_massRecordingsSessionId = value; }
        }

        //Used as a Primary Key in the MR database to distinguish recordings.
        protected string m_massRecordingsPlayerPath = "";
        public string MassRecordingsPlayerPath
        {
            get { return m_massRecordingsPlayerPath; }
            set { m_massRecordingsPlayerPath = value; }
        }

        public CoPilotSettings CoPilotSettings { get; set; } = new();
        public PublishSettings PublishSettings { get; set; } = new();

        protected string m_instanceName;
        public string InstanceName { get => m_instanceName; set => m_instanceName = value; }

        protected EnvironmentType m_environmentType;
        public EnvironmentType EnvironmentType { get => m_environmentType; set => m_environmentType = value; }

        protected string m_SoftwareVersion;
        public string SoftwareVersion { get => m_SoftwareVersion; set => m_SoftwareVersion = value; }

        protected string m_logDbConnectionString;
        public string LogDbConnectionString { get => m_logDbConnectionString; set => m_logDbConnectionString = value; }

        protected string m_logFolder;
        public string LogFolder { get => m_logFolder; set => m_logFolder = value; }

        protected string m_keyFolder;
        public string KeyFolder { get => m_keyFolder; set => m_keyFolder = value; }
        
        public bool AllowSsoLogin;
        
        public string SsoValidationCertificateThumbprint { get; set; }
        
        public string SsoDomain { get; set; }
        
        public string SsoClientId { get; set; }
        
        public bool AllowAdLogin;

        public string SecurityToken { get; set; }

        [JsonIgnore]
        [Obsolete("Use GetServerTimeZone")]
        public string TimeZone => ServerTimeZoneId;

        /// <summary>
        /// Stores TimeZoneInfo.Id, for easy translation to JS frontend and the db.
        /// Used to construct <see cref="ServerTimeZone"/>
        /// </summary>
        public string ServerTimeZoneId { get; set; } // has to be public for serialization

        /// <summary>
        /// Returns the TimeZoneInfo data associated with the set TimeZone Id string in ServerTimeZoneId.
        /// </summary>
        /// <returns></returns>
        public TimeZoneInfo GetServerTimeZone()
        {
            TimeZoneInfo tzi = TimeZoneInfo.Utc;
            try
            {
                if (!string.IsNullOrEmpty(ServerTimeZoneId))
                {
                    tzi = TimeZoneInfo.FindSystemTimeZoneById(ServerTimeZoneId);
                }
            }
            catch
            {
            }
            
            return tzi;
        }

        public int ScenarioLimit { get; set; }

        public string SerialCode { get; set; }
        
        
        public string SiteId { get; set; }

        public string InstanceId { get; set; }
        public string ApiKey { get; set; }

        public bool EnableDiagnostics { get; set; }

        public int EnableChecksumDiagnostics { get; set; } = 0;

        public string WebAppUrl { get; set; }

        public string WebAppClientId { get; set; }

        /// <summary>
        /// Connection string to access the database of Integration Configurations under the new model.
        /// </summary
        public string IntegrationV2Connection { get; set; }

        public bool UseSentry { get; set; } = true;
        public string SentryDsn { get; set; }

        public ServicePaths ServicePaths { get; set; }
        
        public long AutoLogoffTimeout { get; set; }

        public bool DisableHttps { get; set; }
    }
}
