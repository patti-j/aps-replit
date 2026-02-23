using System.Text.RegularExpressions;

using Newtonsoft.Json;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.Utils;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    /// <summary>
    /// APS Instance settings collection that has historically been used as an outbound DTO.
    /// Compare with <seealso cref="APSInstanceEntity">APSInstanceEntity</seealso>, which has been the underlying model and matches the form of existing configuration files.  />
    /// </summary>
    public class InstanceSettingsEntity : Instance
    {
        public InstanceSettingsEntity()
        {
        }

        public InstanceSettingsEntity(APSInstanceEntity a_instance)
        {
            Name = a_instance.PublicInfo.InstanceName;
            Version = a_instance.PublicInfo.SoftwareVersion;
            //Status = a_instance.Status;
            //LicenseStatus = a_instance.LicenseStatus;
            CreationDate = a_instance.Settings.CreationDate;
            EnvironmentType = a_instance.PublicInfo.EnvironmentType;

            if (a_instance.Settings.ErpDatabaseSettings.ConnectionType == ErpDatabaseSettings.EConnectionTypes.SQL7)
            {
                ErpSqlServer = true;
            }
            else if (a_instance.Settings.ErpDatabaseSettings.ConnectionType == ErpDatabaseSettings.EConnectionTypes.ODBC)
            {
                ErpOdbc = true;
            }
            else if (a_instance.Settings.ErpDatabaseSettings.ConnectionType == ErpDatabaseSettings.EConnectionTypes.ORACLE)
            {
                ErpOracle = true;
            }
            else if (a_instance.Settings.ErpDatabaseSettings.ConnectionType == ErpDatabaseSettings.EConnectionTypes.OLEDB)
            {
                ErpOleDB = true;
            }
            RunPreImportSQL = a_instance.Settings.InterfaceServiceSettings.RunPreImportSQL;
            PreImportSQL = a_instance.Settings.InterfaceServiceSettings.PreImportSQL;

            PreImportProgramPath = a_instance.Settings.InterfaceServiceSettings.PreImportProgramPath;//Program to run before import
            PreImportProgramArgs = a_instance.Settings.InterfaceServiceSettings.PreImportProgramArgs;
            ErpDatabaseConnectionString = a_instance.Settings.ErpDatabaseSettings.ConnectionString;

            PublishDbConnectionString = a_instance.Settings.ErpDatabaseSettings.PublishSqlServerConnectionString;
            AnalyticsDbConnectionString = a_instance.Settings.DashboardConnString;
            DashSqlToRun = a_instance.Settings.DashboardSqlToRun;
            DashboardUrl = a_instance.Settings.DashboardReportURL;
            DashTimeout = a_instance.Settings.DashboardTimeout;
            TimeSpan freqSpan = new TimeSpan(0, a_instance.Settings.DashboardPublishFrequency, 0);
            DashFreqHr = freqSpan.Hours;
            DashFreqMin = freqSpan.Minutes;

            PreImportURL = a_instance.Settings.PreImportURL;
            PostImportURL = a_instance.Settings.PostImportURL;
            PreExportURL = a_instance.Settings.PreExportURL;
            PostExportURL = a_instance.Settings.PostExportURL;

            SystemServiceSettingsPort = a_instance.Settings.SystemServiceSettings.Port;
            ExtraServiceWebHostPort = a_instance.Settings.ApiSettings.Port;
            AcceptWebTransmissions = a_instance.Settings.ApiSettings.Enabled;
            AutomaticallyStartServices = a_instance.PublicInfo.AutoStart;
            RecordingsFolder = a_instance.ServicePaths.SystemServiceStartupRecordingsFolder;

            LogonISUserName = a_instance.Settings.InterfaceServiceSettings.LogonUserName;
            string logonPassword = a_instance.Settings.InterfaceServiceSettings.LogonPassword;
            LogonISPwd = Encryption.Decrypt(logonPassword);

            StartServicesTimeoutSeconds = a_instance.Settings.StartServicesTimeoutSeconds;
            SerialCode = Utils.Utils.FormatSerialCodeAddSeparators(a_instance.LicenseInfo.SerialCode);
            SiteId = a_instance.LicenseInfo.SiteId?.ToString() ?? "";
            ClientTimeoutSeconds = a_instance.Settings.SystemServiceSettings.ClientTimeoutSeconds;
            WebApiTimeoutSeconds = a_instance.Settings.SystemServiceSettings.WebApiTimeoutSeconds;
            AllowAcitiveDirectory = a_instance.PublicInfo.ActiveDirectoryAllowed;

            //Clients
            PwdSaving = a_instance.PublicInfo.AllowPasswordSettings;

            // Data/backup
            WorkingData = a_instance.ServicePaths.SystemServiceWorkingDirectory;
            RecordSystem = a_instance.Settings.SystemServiceSettings.RecordSystem;
            SystemSessionsRecord = a_instance.Settings.SystemServiceSettings.MaxNbrSessionRecordingsToStore;
            SystemBackups = a_instance.Settings.SystemServiceSettings.MaxNbrSystemBackupsToStorePerSession;
            MinutesBetweenBackups = a_instance.Settings.SystemServiceSettings.MinutesBetweenSystemBackups;

            SystemStartup = Regex.Replace(Enum.GetName(typeof(EStartType), a_instance.Settings.SystemServiceSettings.SystemEStartType), "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();

            //ADGroups
            //AccessLevels

            EnableDiagnostics = a_instance.Settings.DiagnosticsEnabled;
            ApiDiagnostics = a_instance.Settings.ApiSettings.Diagnostics;
            EnableChecksumDiagnostics = a_instance.Settings.SystemServiceSettings.EnableChecksumDiagnostics;

            SmtpServerAddress = a_instance.Settings.SmtpServerAddress;
            SmtpServerPort = a_instance.Settings.SmtpServerPortNbr;
            SmtpUserName = a_instance.Settings.SmtpUsername;
            // SmtpPassword = String.IsNullOrEmpty(instance.Settings.SmtpEncryptedPassword) ? String.Empty : PT.Common.PtEncryptor.Decrypt(instance.Settings.SmtpEncryptedPassword);
            SmtpFromEmail = a_instance.Settings.SmtpFromEmailAddress;
            SmtpEnableSsl = a_instance.Settings.SmtpUseSsl;

            LogEmailToAddresses = a_instance.Settings.LogEmailToAddresses;
            LogEmailSubjectPrefix = a_instance.Settings.LogEmailSubjectPrefix;
            SupportEmailToAddresses = a_instance.Settings.SupportEmailToAddresses;
            SupportEmailSubjectPrefix = a_instance.Settings.SupportEmailSubjectPrefix;
            StartingScenario = a_instance.Settings.SystemServiceSettings.StartingScenarioNumber.ToString();
            NonSequenced = a_instance.Settings.SystemServiceSettings.NonSequencedTransmissionPlayback;
            SingleThreaded = a_instance.Settings.SystemServiceSettings.SingleThreadedTransmissionProcessing;
            ServerTimeZoneId = a_instance.Settings.ServerTimeZoneId;
            ManualDownload = a_instance.Settings.PackageSettings.ManualDownload;
            if (!a_instance.Settings.PackageSettings.ManualDownload)
            {
                AutoDownload = true;
            }

            NewPackages = a_instance.Settings.PackageSettings.NewPackages;
            UpdatePackages = a_instance.Settings.PackageSettings.UpdatePackages;
            PackageVersionGroup = a_instance.Settings.PackageSettings.PreReleaseChannel ? 1 : 0;

            LogsDbConnectionString = a_instance.Settings.SystemServiceSettings.LogDbConnectionString;
            ServicePaths = a_instance.ServicePaths;
            SsoValidationCertificateThumbprint = a_instance.Settings.SsoValidationCertificateThumbprint;
            //in case we want to show the schena
            ScenarioFile = a_instance.Settings.ScenarioFile;
            AllowSsoLogin = a_instance.Settings.AllowSsoLogin;

            SsoDomain = a_instance.Settings.SystemServiceSettings.SsoDomain;
            SsoClientId = a_instance.Settings.SystemServiceSettings.SsoClientId;
            WebAppClientId = a_instance.Settings.SystemServiceSettings.WebAppClientId;
            ServerWideSettings = a_instance.ServerWideSettings;

            ApiSettings = new ApiSettingsEntity
            {
                Diagnostics = a_instance.Settings.ApiSettings.Diagnostics,
                Enabled = a_instance.Settings.ApiSettings.Enabled,
                Port = a_instance.Settings.ApiSettings.Port
            };

            string systemAlertFolder = a_instance.ServicePaths.SystemServiceAlertsFolder;
            SystemLogs = String.Empty;
            if (Directory.Exists(systemAlertFolder))
            {
                SystemLogs = systemAlertFolder;
            }

            string interfaceServiceWorkingDirectory = a_instance.ServicePaths.InterfaceServiceWorkingDirectory;
            InterfaceLogs = String.Empty;
            if (Directory.Exists(interfaceServiceWorkingDirectory))
            {
                InterfaceLogs = interfaceServiceWorkingDirectory;
            }

            ScenarioLimit = a_instance.Settings.ScenarioLimit;

            CoPilotSettings.Enabled = a_instance.Settings.CoPilotSettings.Enabled;
            CoPilotSettings.AverageCpuUsage = a_instance.Settings.CoPilotSettings.AverageCpuUsage;
            CoPilotSettings.BurstDuration = a_instance.Settings.CoPilotSettings.BurstDuration;
            CoPilotSettings.BoostPercentage = a_instance.Settings.CoPilotSettings.BoostPercentage;

            PublishSettings = a_instance.Settings.PublishSettings.ShallowCopy();

            IntegrationV2Connection = a_instance.Settings.IntegrationV2Connection;
        }

        public bool reregisterSystemService { get; set; } = false;
        public bool ErpSqlServer { get; set; }
        public bool ErpOdbc{ get; set; }
        public bool ErpOracle{ get; set; }
        public bool ErpOleDB{ get; set; }

        public string WorkingData { get; set; }
        public bool RecordSystem { get; set; }
        public int SystemSessionsRecord { get; set; }
        public int SystemBackups { get; set; }
        public int MinutesBetweenBackups { get; set; }

        public string SystemStartup { get; set; }
        public bool RunPreImportSQL{ get; set; }
        public string PreImportSQL{ get; set; }

        public bool RunPostImportSQL{ get; set; }
        public string PostImportSQL{ get; set; }

        public string PreImportProgramPath{ get; set; }
        public string PreImportProgramArgs{ get; set; }

        public string PreImportURL{ get; set; }
        public string PostImportURL{ get; set; }
        public string PreExportURL{ get; set; }
        public string PostExportURL{ get; set; }

        public string ErpDatabaseConnectionString{ get; set; }
        public string PublishDbConnectionString{ get; set; }
        public string AnalyticsDbConnectionString{ get; set; }
        public string DashboardUrl { get; set; }
        public string DashSqlToRun{ get; set; }
        public int DashTimeout{ get; set; }
        public int DashFreqHr{ get; set; }
        public int DashFreqMin { get; set; }
        public int ScenarioLimit { get; set; }
        public EnvironmentType EnvironmentType { get; set; }
        public bool AutomaticallyStartServices { get; set; }
        public string RecordingsFolder { get; set; }
        public int SystemServiceSettingsPort{ get; set; }
        public int InterfaceServiceSettingsPort{ get; set; }
        public bool AcceptWebTransmissions{ get; set; }
        public int ExtraServiceWebHostPort{ get; set; }

        public string SiteId { get; set; }

        public string LogonISUserName{ get; set; }
        public string LogonISPwd{ get; set; }
        public int StartServicesTimeoutSeconds { get; set; }

        public int ClientTimeoutSeconds{ get; set; }
        public int WebApiTimeoutSeconds{ get; set; }
        public bool AllowAcitiveDirectory{ get; set; }

        public bool PwdSaving { get; set; }

        public bool EnableDiagnostics{ get; set; }
        public bool ApiDiagnostics{ get; set; }
        public int EnableChecksumDiagnostics { get; set; } = 0; // currently true/false; may have more values in future

        public string SmtpServerAddress{ get; set; }
        public int SmtpServerPort{ get; set; }
        public string SmtpUserName{ get; set; }
        public string SmtpPassword{ get; set; }
        public string SmtpFromEmail{ get; set; }
        public bool SmtpEnableSsl{ get; set; }

        public string LogEmailToAddresses{ get; set; }
        public string LogEmailSubjectPrefix{ get; set; }
        public string SupportEmailToAddresses{ get; set; }
        public string SupportEmailSubjectPrefix{ get; set; }

        public int SystemStartupSelectedIndex{ get; set; }
        public bool AbsorbCustomization{ get; set; }
        public string StartingScenario{ get; set; }
        public bool NonSequenced{ get; set; }
        public bool SingleThreaded{ get; set; }

        public bool ManualDownload{ get; set; }
        public bool AutoDownload{ get; set; }
        public bool NewPackages{ get; set; }
        public bool UpdatePackages{ get; set; }
        public int PackageVersionGroup{ get; set; }
        public string LogsDbConnectionString{ get; set; }
        public string SsoValidationCertificateThumbprint { get; set; }
        public string ScenarioFile { get; set; }        
        public bool AllowSsoLogin { get; set; }
        public string SsoDomain { get; set; }
        public string SsoClientId { get; set; }
        public string WebAppClientId { get; set; }
        public string IntegrationV2Connection { get; set; }

        public ServerWideInstanceSettings ServerWideSettings { get; set; }

        public ApiSettingsEntity ApiSettings { get; set; }
        public string SystemLogs { get; set; }
        public string InterfaceLogs { get; set; }

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
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(ServerTimeZoneId);
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }

        public CoPilotSettings CoPilotSettings { get; set; } = new CoPilotSettings();
        public PublishSettings PublishSettings { get; set; } = new PublishSettings();

        public bool UseSentry { get; set; } = true;

        /// <summary>
        /// Dsn value for our PT Sentry environment for logging core errors. Provided as a constant by WebApp API.
        /// </summary>
        public string SentryDsn { get; set; }
    }
}
