using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.Utils;

namespace PT.ServerManagerSharedLib.Definitions
{
    /// <summary>
    /// Used for accessing Registry entries for the System Service.
    /// </summary>
    public class SystemServiceSettings
    {
        public SystemServiceSettings()
        {
        }

        public SystemServiceSettings(SystemServiceSettings a_copyFromSettings)
        {
            ClientTimeoutSeconds = a_copyFromSettings.ClientTimeoutSeconds;
            WebApiTimeoutSeconds = a_copyFromSettings.WebApiTimeoutSeconds;
            MaxNbrSessionRecordingsToStore = a_copyFromSettings.MaxNbrSessionRecordingsToStore;
            MaxNbrSystemBackupsToStorePerSession = a_copyFromSettings.MaxNbrSystemBackupsToStorePerSession;
            MinutesBetweenSystemBackups = a_copyFromSettings.MinutesBetweenSystemBackups;
            NonSequencedTransmissionPlayback = a_copyFromSettings.NonSequencedTransmissionPlayback;
            Priority = a_copyFromSettings.Priority;
            RecordSystem = a_copyFromSettings.RecordSystem;
            SingleThreadedTransmissionProcessing = a_copyFromSettings.SingleThreadedTransmissionProcessing;
            StartingScenarioNumber = a_copyFromSettings.StartingScenarioNumber;
            SystemEStartType = a_copyFromSettings.SystemEStartType;
            KeyFolder = a_copyFromSettings.KeyFolder;
            LogFolder = a_copyFromSettings.LogFolder;
            m_logDbConnectionString = a_copyFromSettings.m_logDbConnectionString;
            SsoDomain = a_copyFromSettings.SsoDomain;
            SsoClientId = a_copyFromSettings.SsoClientId;
            WebAppClientId = a_copyFromSettings.WebAppClientId;
            EnableChecksumDiagnostics = a_copyFromSettings.EnableChecksumDiagnostics;
        }

        public static string GetSystemProgramFilesPathForVersion(string a_version)
        {
            return Path.Combine(Paths.GetProgramFilesPathForVersion(a_version), "System");
        }

        public static string GetExePath(string a_aVersion)
        {
            return Path.Combine(GetSystemProgramFilesPathForVersion(a_aVersion), "PlanetTogether System.exe");
        }

        public int Port { get; set; } = 4001;

        public enum EPriorityEnum { Lowest, BelowNormal, Normal, AboveNormal, Highest };

        public EPriorityEnum Priority { get; set; } = EPriorityEnum.AboveNormal;

        public bool RecordSystem { get; set; } = true;

        public int MaxNbrSessionRecordingsToStore { get; set; } = 10;

        public int MaxNbrSystemBackupsToStorePerSession { get; set; } = 100;

        public string DashSqlToRun { get; set; } = "";

        public int MinutesBetweenSystemBackups { get; set; } = 15;

        //TODO: Rename to SystemStartType
        public EStartType SystemEStartType { get; set; } = EStartType.Normal;

        /// <summary>
        /// Where Instance stores its logs
        /// </summary>
        public string LogFolder { get; set; } = String.Empty;

        /// <summary>
        /// Where Instance key files reside
        /// </summary>
        public string KeyFolder { get; set; } = String.Empty;

        string m_logDbConnectionString = String.Empty;

        /// <summary>
        /// Connection string to db storing alerts
        /// </summary>
        public string LogDbConnectionString
        {
            get { return string.IsNullOrEmpty(m_logDbConnectionString) ? string.Empty : Encryption.Decrypt(m_logDbConnectionString); }
            set { m_logDbConnectionString = Encryption.Encrypt(value); }
        }

        public int StartingScenarioNumber { get; set; }

        public bool NonSequencedTransmissionPlayback { get; set; }

        public bool SingleThreadedTransmissionProcessing { get; set; }


        // Attribute ensures that missing values don't overwrite desired default (120) with type default (0)
        [DefaultValue(120)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int ClientTimeoutSeconds { get; set; } = 120;



        [DefaultValue(120)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int WebApiTimeoutSeconds { get; set; } = 120;

        public string SsoDomain { get; set; } = string.Empty;
        public string SsoClientId { get; set; } = string.Empty;

        /// <summary>
        /// SSO Client Id for the Webapp. We are currently designing the Webapp login as an alternative means of connecting to an instance.
        /// A login token from this id should be accepted in addition to the instance-configured one in <see cref="SsoClientId"/>.
        /// </summary>
        public string WebAppClientId { get; set; } = string.Empty;

        /// <summary>
        /// Controls whether additional checksum file logging (writes of server/client scenario data for comparison) is performed on the instance.
        /// When disabled, previous checksum logging is deleted on startup.
        /// </summary>
        public int EnableChecksumDiagnostics { get; set; } = 0;


        public bool Update(InstanceSettingsEntity a_updateSettings)
        {
            bool needToRestartSystemService = false;
            if (Port != a_updateSettings.SystemServiceSettingsPort)
            {
                Port = a_updateSettings.SystemServiceSettingsPort;
            }
            if (RecordSystem != a_updateSettings.RecordSystem)
            {
                RecordSystem = a_updateSettings.RecordSystem;
                needToRestartSystemService = true;
            }
            if (MaxNbrSessionRecordingsToStore != Convert.ToInt32(a_updateSettings.SystemSessionsRecord))
            {
                MaxNbrSessionRecordingsToStore = Convert.ToInt32(a_updateSettings.SystemSessionsRecord);
                needToRestartSystemService = true;
            }
            if (MaxNbrSystemBackupsToStorePerSession != Convert.ToInt32(a_updateSettings.SystemBackups))
            {
                MaxNbrSystemBackupsToStorePerSession = Convert.ToInt32(a_updateSettings.SystemBackups);
                needToRestartSystemService = true;
            }
            if (MinutesBetweenSystemBackups != Convert.ToInt32(a_updateSettings.MinutesBetweenBackups))
            {
                MinutesBetweenSystemBackups = Convert.ToInt32(a_updateSettings.MinutesBetweenBackups);
                needToRestartSystemService = true;
            }

            if (DashSqlToRun != a_updateSettings.DashSqlToRun)
            {
                DashSqlToRun = a_updateSettings.DashSqlToRun;
                needToRestartSystemService = true;
            }

            EStartType eStartType = (EStartType)Enum.Parse(typeof(EStartType), Regex.Replace(a_updateSettings.SystemStartup, @"\s+", ""));
            if (SystemEStartType != eStartType)
            {
                SystemEStartType = eStartType;
                needToRestartSystemService = true;
            }
            if (StartingScenarioNumber != Convert.ToInt32(a_updateSettings.StartingScenario))
            {
                StartingScenarioNumber = Convert.ToInt32(a_updateSettings.StartingScenario);
                needToRestartSystemService = true;
            }
            if (NonSequencedTransmissionPlayback != a_updateSettings.NonSequenced)
            {
                NonSequencedTransmissionPlayback = a_updateSettings.NonSequenced;
                needToRestartSystemService = true;
            }
            if (SingleThreadedTransmissionProcessing != a_updateSettings.SingleThreaded)
            {
                SingleThreadedTransmissionProcessing = a_updateSettings.SingleThreaded;
                needToRestartSystemService = true;
            }
            if (ClientTimeoutSeconds != Convert.ToInt32(a_updateSettings.ClientTimeoutSeconds))
            {
                ClientTimeoutSeconds = Convert.ToInt32(a_updateSettings.ClientTimeoutSeconds);
                needToRestartSystemService = true;
            }
            if (WebApiTimeoutSeconds != Convert.ToInt32(a_updateSettings.WebApiTimeoutSeconds))
            {
                WebApiTimeoutSeconds = Convert.ToInt32(a_updateSettings.WebApiTimeoutSeconds);
                needToRestartSystemService = true;
            }
            if (SsoDomain != a_updateSettings.SsoDomain)
            {
                SsoDomain = a_updateSettings.SsoDomain;
                needToRestartSystemService = true;
            }
            if (SsoClientId != a_updateSettings.SsoClientId)
            {
                SsoClientId = a_updateSettings.SsoClientId;
                needToRestartSystemService = true;
            }
            if (WebAppClientId != a_updateSettings.WebAppClientId)
            {
                WebAppClientId = a_updateSettings.WebAppClientId;
                needToRestartSystemService = true;
            }
            if (EnableChecksumDiagnostics != a_updateSettings.EnableChecksumDiagnostics)
            {
                EnableChecksumDiagnostics = a_updateSettings.EnableChecksumDiagnostics;
                needToRestartSystemService = true;
            }

            return needToRestartSystemService;
        }
    }

    /// <summary>
    /// Used for accessing Registry entries for the InterfaceService Service.
    /// </summary>
    public class InterfaceServiceSettings
    {
        public InterfaceServiceSettings() { }

        public InterfaceServiceSettings(InterfaceServiceSettings a_copyFromSettings)
        {
            RunPreImportSQL = a_copyFromSettings.RunPreImportSQL;
            PreImportSQL = a_copyFromSettings.PreImportSQL;
            PreProcessingArg1 = a_copyFromSettings.PreProcessingArg1;
            PreProcessingArg2 = a_copyFromSettings.PreProcessingArg2;
            PreProcessingObjectURL = a_copyFromSettings.PreProcessingObjectURL;
        }

        public static string GetServiceName(string a_aInstanceName, string a_aVersion)
        {
            return string.Format("PlanetTogether {0} {1} Interface", a_aInstanceName, a_aVersion);
        }

        public static string GetInterfaceProgramFilesPathForVersion(string a_aVersion)
        {
            return Path.Combine(Paths.GetProgramFilesPathForVersion(a_aVersion), "Interface");
        }
        public static string GetExePath(string a_aVersion)
        {
            return Path.Combine(GetInterfaceProgramFilesPathForVersion(a_aVersion), "PlanetTogether Interface.exe");
        }

        [Obsolete("Remove This")]
        public int Port { get; set; } = 4002;

        [Obsolete("Remove This")]
        public bool UseLogonAccount { get; set; }

        public bool RunPreImportSQL { get; set; }

        public string PreImportSQL { get; set; } = "";

        public string PreImportProgramPath { get; set; } = "";

        public string PreImportProgramArgs { get; set; } = "";


        #region Login Credentials

        /// <summary>
        /// Whether to Logon as a specified User as opposed to logging on as the Local System Account.
        /// </summary>
        public bool LogonAsUser { get; set; }

        public string LogonUserName { get; set; } = "";

        public string LogonPassword { get; set; } = "";

        #endregion

        #region Preprocessing

        public string PreProcessingObjectURL { get; set; } = "";

        public string PreProcessingArg1 { get; set; } = "";

        public string PreProcessingArg2 { get; set; } = "";

        #endregion

        internal void Update(InterfaceServiceSettings a_updateSettings)
        {
            
        }

        #region Obsolete

        public long TimeSpanMultiplier { get; set; } = 36000000000;

        public string ImportConnectionInitializationSQL { get; set; } = "";

        #endregion
    }

    /// <summary>
    /// Used for accessing Registry entries for integrations with an ERP Database.
    /// </summary>
    public class ErpDatabaseSettings
    {
        public ErpDatabaseSettings() { }

        public ErpDatabaseSettings(ErpDatabaseSettings a_aCopyFromSettings)
        {
            ConnectionString = a_aCopyFromSettings.ConnectionString;
            ConnectionType = a_aCopyFromSettings.ConnectionType;
            DatabaseName = a_aCopyFromSettings.DatabaseName;
            m_erpDatabaseName = a_aCopyFromSettings.m_erpDatabaseName;
            m_erpPassword = a_aCopyFromSettings.m_erpPassword;
            m_erpServerName = a_aCopyFromSettings.m_erpServerName;
            m_erpUserName = a_aCopyFromSettings.m_erpUserName;
            Password = a_aCopyFromSettings.Password;
            ServerName = a_aCopyFromSettings.ServerName;
            UserName = a_aCopyFromSettings.UserName;
            ConnectionStringType = a_aCopyFromSettings.ConnectionStringType;
        }

        public string ServerName { get; set; } = "";

        public string DatabaseName { get; set; } = "";

        public string UserName { get; set; } = "";

        public string Password { get; set; } = "";

        public string ConnectionString { get; set; } = "";

        public enum EConnectionTypes
        {
            SQL7,
            ODBC,
            ORACLE,
            OLEDB
        };

        public EConnectionTypes ConnectionType { get; set; } = EConnectionTypes.SQL7;

        public enum EConnectionStringTypes
        {
            SIMPLE,
            COMPLEX
        };

        public EConnectionStringTypes ConnectionStringType { get; set; } = EConnectionStringTypes.SIMPLE;

        string m_erpUserName = "";

        /// <summary>
        /// This is for logging into the ERP sytem if needed for custom SQL Exports.
        /// </summary>
        public string ErpUserName
        {
            get { return string.IsNullOrEmpty(m_erpUserName) ? string.Empty : Encryption.Decrypt(m_erpUserName);}
            set { m_erpUserName = Encryption.Encrypt(value); }
        }

        string m_erpPassword = "";

        /// <summary>
        /// This is for logging into the ERP sytem if needed for custom SQL Exports.
        /// </summary>
        public string ErpPassword
        {
            get { return string.IsNullOrEmpty(m_erpPassword) ? string.Empty :  Encryption.Decrypt(m_erpPassword); }
            set { m_erpPassword = Encryption.Encrypt(value); }
        }

        string m_erpServerName = "";

        /// <summary>
        /// This is for logging into the ERP sytem if needed for custom SQL Exports.
        /// </summary>
        public string ErpServerName
        {
            get { return string.IsNullOrEmpty(m_erpServerName) ? string.Empty :  Encryption.Decrypt(m_erpServerName); }
            set { m_erpServerName = Encryption.Encrypt(value); }
        }

        string m_erpDatabaseName = "";

        /// <summary>
        /// This is for logging into the ERP sytem if needed for custom SQL Exports.
        /// </summary>
        public string ErpDatabaseName
        {
            get { return string.IsNullOrEmpty(m_erpDatabaseName) ? string.Empty : Encryption.Decrypt(m_erpDatabaseName); }
            set { m_erpDatabaseName = Encryption.Encrypt(value); }
        }

        string m_publishSqlServerConnectionString = "";

        /// <summary>
        /// This is for logging into the ERP sytem if needed for custom SQL Exports.
        /// </summary>
        public string PublishSqlServerConnectionString
        {
            get { return string.IsNullOrEmpty(m_publishSqlServerConnectionString) ? string.Empty :  Encryption.Decrypt(m_publishSqlServerConnectionString); }
            set { m_publishSqlServerConnectionString = Encryption.Encrypt(value); }
        }
    }

    /// <summary>
    /// Used for accessing Registry entries for the InterfaceService Service.
    /// </summary>
    public class ApiSettings
    {
        public ApiSettings() { }

        public ApiSettings(ApiSettings a_aCopyFromSettings)
        {
            Enabled = a_aCopyFromSettings.Enabled;
            Port = a_aCopyFromSettings.Port;
            Diagnostics = a_aCopyFromSettings.Diagnostics;
        }

        public int Port { get; set; } = 8080;

        public bool Enabled { get; set; }

        public bool Diagnostics { get; set; }
    }

    /// <summary>
    /// Used for accessing Registry entries for the InterfaceService Service.
    /// </summary>
    public class PackageSettings
    {
        public PackageSettings() { }

        public PackageSettings(PackageSettings a_aCopyFromSettings)
        {
            ManualDownload = a_aCopyFromSettings.ManualDownload;
            UpdatePackages = a_aCopyFromSettings.UpdatePackages;
            NewPackages = a_aCopyFromSettings.NewPackages;
        }

        public bool ManualDownload { get; set; }

        public bool UpdatePackages { get; set; }

        public bool NewPackages { get; set; }

        public bool PreReleaseChannel { get; set; }

        internal void Update(PackageSettings a_aUpdateSettings)
        {
            //These values are grabbed when needed so no need to restart the service when changed.
            if (ManualDownload != a_aUpdateSettings.ManualDownload)
            {
                ManualDownload = a_aUpdateSettings.ManualDownload;
            }

            if (UpdatePackages != a_aUpdateSettings.UpdatePackages)
            {
                UpdatePackages = a_aUpdateSettings.UpdatePackages;
            }

            if (NewPackages != a_aUpdateSettings.NewPackages)
            {
                NewPackages = a_aUpdateSettings.NewPackages;
            }
            
            if (PreReleaseChannel != a_aUpdateSettings.PreReleaseChannel)
            {
                PreReleaseChannel = a_aUpdateSettings.PreReleaseChannel;
            }
        }
    }

    public class InstanceLicenseInfoBase
    {
        public InstanceLicenseInfoBase() { }

        public string SerialCode { get; set; }

        /// <summary>
        /// The Company's Site this instance is associated with, as identified in the v12 LicenseManager DB
        /// </summary>
        public string SiteId { get; set; }

    }

    public class InstanceLogHelperBase
    {
        bool m_systemHasLogs;
        public bool SystemHasLogs
        {
            get { return m_systemHasLogs; }
            protected set { m_systemHasLogs = value; }
        }

        bool m_interfaceHasLogs;
        public bool InterfaceHasLogs
        {
            get { return m_interfaceHasLogs; }
            protected set { m_interfaceHasLogs = value; }
        }

        bool m_packagesHasLogs;
        public bool PackagesHasLogs
        {
            get { return m_packagesHasLogs; }
            protected set { m_packagesHasLogs = value; }
        }

    }
}
