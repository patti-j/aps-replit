using ReportsWebApp.Common;
using ReportsWebApp.DB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

namespace ReportsWebApp.DB.Models
{
    [Index(nameof(PlanningAreaKey))]
    [Index(nameof(ApiKey))]
    public class 
        PADetails : BaseEntity
    {
        public override string TypeDisplayName
        {
            get => "Planning Area";
        }
        public virtual List<PlanningAreaTag> Tags { get; set; } = new();
        [ForeignKey("PlanningAreaLocation")] public int? LocationId { get; set; }
        public virtual PlanningAreaLocation Location { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public int? UsedByCompanyId { get; set; }
        public virtual Company? UsedByCompany { get; set; }
        public string Version { get; set; } = "";

        /// <summary>
        /// The ID of the server.
        /// </summary>
        [ForeignKey("Server")]
        public int ServerId { get; set; }

        /// <summary>
        /// The company that manages this server.
        /// </summary>
        public virtual CompanyServer Server { get; set; }
        public string Environment { get; set; } = "";
        // This correlates to the InstanceIdentifier property
        public string PlanningAreaKey { get; set; } = "";
        public string Settings { get; set; } = "";
        public DateTime? UpdateDate { get; set; } = DateTime.UtcNow;

        public virtual ExternalIntegration? ExternalIntegration { get; set; }
        
        [ForeignKey("ExternalIntegration")]
        public int? ExternalIntegrationId { get; set; }
        public int? DataConnectorId { get; set; }
        public virtual DBIntegration? CurrentIntegration {get; set;}

        [ForeignKey("CurrentIntegration")]
        public int? DBIntegrationId { get; set; }

        public DateTime? DBIntegrationLastAppliedTime { get; set; }

        public virtual List<PlanningAreaScope> Scopes { get; set; }
        public virtual List<IntegrationConfig> IntegrationConfigs { get; set; }
        [ForeignKey("CFGroup")]
        public int? CFGroupId { get; set; }
        public virtual CFGroup? CFGroup { get; set; }
        public string FavoriteSettingScenarioIds { get; set; } = string.Empty;
        public string RegistrationStatus { get; set; } = ERegistrationStatus.Pending.ToString();
        private PlanningArea? m_planningArea = null;
        public ELicenseStatus LicenseStatus { get; set; } = ELicenseStatus.Unknown;
        public EServiceState ServiceState { get; set; }
        public DateTime ServiceStateUpdated { get; set; }
        public int? BackupOf { get; set; }
        public bool IsBackup => BackupOf is > 0;

        [NotMapped]
        public PlanningArea? PlanningArea
        {
            get => m_planningArea ?? DeserializeSettingsJSON();
            set => m_planningArea = value;
        }

        [NotMapped]
        public ServiceStatus? Status { get; set; }

        [NotMapped] public ConnectionDetails ConnectionDetails { get; set; } = new();

        [NotMapped]
        public bool IsStarted => ServiceState != null && 
                                 ServiceState is EServiceState.Started or EServiceState.Starting or EServiceState.Active;

        public bool? DBIntegrationChangeInProgress { get; internal set; }
        
        public string? ApiKey { get; set; }
        public string? ApiUrl { get; set; }

        public record InstanceStatusRequest(string SystemServiceUrl, string SystemServiceName);
        public record InstanceStatusResponse(ServiceStatus Status);

        /// <summary>
        /// Populates the PlanningArea property by deserializing its settings.
        /// </summary>
        /// <returns>The updated planning area details.</returns>
        private PlanningArea? DeserializeSettingsJSON()
        {
            if (Settings.IsNullOrEmpty())
            {
                return null;
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            m_planningArea = System.Text.Json.JsonSerializer.Deserialize<PlanningArea>(Settings, options);

            InitValsFromServer(m_planningArea);
            return m_planningArea;
        }

        /// <summary>
        /// Some objects need initializing using data outside of settings.
        /// TODO: Can we store this info without needing the Server ref? (eg Call and hard code on Server settings changes)
        /// TODO: May be other things to auto-populate on instance creation, consult SM.
        /// </summary>
        /// <param name="a_planningArea">Probably should always be this.PlanningArea, but the timing in DeserializeSettingsJSON and nature of the PlanningArea Getter require a reference</param>
        public void InitValsFromServer(PlanningArea a_planningArea)
        {
            a_planningArea.ServicePaths.Init(new InstanceKey(Name, Version).ToString(), Server);
        }
    }
    public class BulkEditorValues
    {
        public int ScenarioLimit { get; set; } = 5;
        public bool CopilotEnabled { get; set; } = false;
        public float AverageCpuUsage { get; set; } = 1;
        public float BoostPercentage { get; set; } = 200;
        public string BurstDuration { get; set; } = "1";
    }

    public class ColumnOption
    {
        public string ColumnWidth { get; set; } = "150";
        public string ColumnName { get; set; }
        public string FieldName { get; set; }
        public bool IsChecked { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool Unbound { get; set; } = false;
    }

    public class PlanningArea
    {
        public Settings Settings { get; set; } = new();
        public InstancePublicInfo PublicInfo { get; set; } = new();
        public Licenseinfo LicenseInfo { get; set; } = new();
        public Loghelper LogHelper { get; set; } = new();
        public Servicepaths ServicePaths { get; set; } = new();
    }

    public class Settings
    {
        public int m_scenarioLimit { get; set; }
        public DateTime CreationDate { get; set; }
        public string ScenarioFile { get; set; }
        public int StartServicesTimeoutSeconds { get; set; } = 120;
        public string DashboardConnString { get; set; } = string.Empty;
        public string DashboardSqlToRun { get; set; } = string.Empty;
        public int DashboardTimeout { get; set; } = 120;
        public int ScenarioLimit { get; set; } = 5;
        public int DashboardPublishFrequency { get; set; } = 0;
        public string DashboardReportURL { get; set; } = "http://dashboard.planettogether.com";
        public string AnalyticsURL { get; set; } = string.Empty;
        public string PreImportURL { get; set; } = string.Empty;
        public string PostImportURL { get; set; } = string.Empty;
        public string PreExportURL { get; set; } = string.Empty;
        public string PostExportURL { get; set; } = string.Empty;
        public bool DiagnosticsEnabled { get; set; }
        public string InstanceId { get; set; }
        public string ServerTimeZoneId { get; set; }
        public string SecurityToken { get; set; }
        public string? IntegrationV2Connection { get; set; } = "true";
        public SystemServiceSettings SystemServiceSettings { get; set; } = new();
        public InterfaceServiceSettings InterfaceServiceSettings { get; set; } = new();
        public ErpDatabaseSettings ErpDatabaseSettings { get; set; } = new();
        public ApiSettings ApiSettings { get; set; } = new();
        public PackageSettings PackageSettings { get; set; } = new();
        public object SsoValidationCertificateThumbprint { get; set; }
        public bool AllowSsoLogin { get; set; } = true;
        public CopilotSettings CoPilotSettings { get; set; } = new();
        public PublishSettings PublishSettings { get; set; } = new();
        
        /// <summary>
        /// Enables additional logging for the instance to Sentry.
        /// The credentials to that sentry environment are injected in the api layer.
        /// </summary>
        public bool UseSentry { get; set; }
        public bool DisableHttps { get; set; }

        /// <summary>
        /// Checks if any data has been changed that would require an SM setting update. 
        /// </summary>
        /// <param name="pa2">The new SystemServiceSettings</param>
        /// <returns>True if any setting has changed that requires an update, false otherwise.</returns>
        /// <exception cref="InvalidDataException">If the settings are null</exception>
        public bool CheckIfSettingUpdateNeeded(Settings? pa2)
        {
            if (pa2 == null)
            {
                throw new InvalidDataException("Settings was null");
            }
            if (pa2.SystemServiceSettings.Port != SystemServiceSettings.Port)
            {
                return true;
            }

            if (pa2.InterfaceServiceSettings.LogonAsUser != InterfaceServiceSettings.LogonAsUser)
            {
                return true;
            }
            if (pa2.InterfaceServiceSettings.LogonPassword != InterfaceServiceSettings.LogonPassword)
            {
                return true;
            }
            if (pa2.InterfaceServiceSettings.LogonUserName != InterfaceServiceSettings.LogonUserName)
            {
                return true;
            }
            if (pa2.InterfaceServiceSettings.UseLogonAccount != InterfaceServiceSettings.UseLogonAccount)
            {
                return true;
            }

            return false;
        }
    }

    public class SystemServiceSettings
    {
        public int Port { get; set; }
        public int Priority { get; set; }
        public bool RecordSystem { get; set; } = true;
        public int MaxNbrSessionRecordingsToStore { get; set; } = 10;
        public int MaxNbrSystemBackupsToStorePerSession { get; set; } = 100;
        public string DashSqlToRun { get; set; } = string.Empty;
        public int MinutesBetweenSystemBackups { get; set; } = 15;
        public EStartType SystemEStartType { get; set; } = EStartType.Normal;
        public string LogFolder { get; set; } = string.Empty;
        public string KeyFolder { get; set; } = string.Empty;
        public string LogDbConnectionString { get; set; }
        public int StartingScenarioNumber { get; set; }
        public bool NonSequencedTransmissionPlayback { get; set; }
        public bool SingleThreadedTransmissionProcessing { get; set; }
        public int ClientTimeoutSeconds { get; set; } = 120;
        public int WebApiTimeoutSeconds { get; set; } = 120;
        public string SsoDomain { get; set; }
        public string SsoClientId { get; set; }
        public int EnableChecksumDiagnostics { get; set; } = 0;

        

        /// <summary>
        /// Checks if any data has been changed that would require a service restart. 
        /// </summary>
        /// <param name="pa2">The new SystemServiceSettings</param>
        /// <returns>True if any setting has changed that requires a restart, false otherwise.</returns>
        /// <exception cref="InvalidDataException">If the settings are null</exception>
        public bool CheckIfRestartNeeded(SystemServiceSettings? pa2)
        {
            if (pa2 == null)
            {
                throw new InvalidDataException($"Tried to compare invalid Planning Area settings.");
            }
            bool needToRestartSystemService = false;

            if (pa2.RecordSystem != this.RecordSystem)
            {
                needToRestartSystemService = true;
            }
            if (pa2.MaxNbrSessionRecordingsToStore != this.MaxNbrSessionRecordingsToStore)
            {
                needToRestartSystemService = true;
            }
            if (pa2.MaxNbrSystemBackupsToStorePerSession != this.MaxNbrSystemBackupsToStorePerSession)
            {
                needToRestartSystemService = true;
            }
            if (pa2.MinutesBetweenSystemBackups != this.MinutesBetweenSystemBackups)
            {
                needToRestartSystemService = true;
            }
            if (pa2.DashSqlToRun != this.DashSqlToRun)
            {
                needToRestartSystemService = true;
            }
            if (pa2.SystemEStartType != this.SystemEStartType)
            {
                needToRestartSystemService = true;
            }
            if (pa2.StartingScenarioNumber != this.StartingScenarioNumber)
            {
                needToRestartSystemService = true;
            }
            if (pa2.NonSequencedTransmissionPlayback != this.NonSequencedTransmissionPlayback)
            {
                needToRestartSystemService = true;
            }
            if (pa2.SingleThreadedTransmissionProcessing != this.SingleThreadedTransmissionProcessing)
            {
                needToRestartSystemService = true;
            }
            if (pa2.ClientTimeoutSeconds != this.ClientTimeoutSeconds)
            {
                needToRestartSystemService = true;
            }
            if (pa2.WebApiTimeoutSeconds != this.WebApiTimeoutSeconds)
            {
                needToRestartSystemService = true;
            }
            if (pa2.SsoDomain != this.SsoDomain)
            {
                needToRestartSystemService = true;
            }
            if (pa2.SsoClientId != this.SsoClientId)
            {
                needToRestartSystemService = true;
            }
            if (pa2.EnableChecksumDiagnostics != this.EnableChecksumDiagnostics)
            {
                needToRestartSystemService = true;
            }

            return needToRestartSystemService;
        }
    }

    public class InterfaceServiceSettings
    {
        public int Port { get; set; }
        public bool UseLogonAccount { get; set; }
        public bool RunPreImportSQL { get; set; }
        public string PreImportSQL { get; set; }
        public string PreImportProgramPath { get; set; }
        public string PreImportProgramArgs { get; set; }
        public bool LogonAsUser { get; set; }
        public string LogonUserName { get; set; }
        public string LogonPassword { get; set; }
        public string PreProcessingObjectURL { get; set; }
        public string PreProcessingArg1 { get; set; }
        public string PreProcessingArg2 { get; set; }
        public long TimeSpanMultiplier { get; set; }
        public string ImportConnectionInitializationSQL { get; set; }
    }

    public class ErpDatabaseSettings
    {
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConnectionString { get; set; }
        public int ConnectionType { get; set; }
        public int ConnectionStringType { get; set; }
        public string ErpUserName { get; set; }
        public string ErpPassword { get; set; }
        public string IntegrationUserCreds { get; set; }
        public string ErpServerName { get; set; }
        public string ErpDatabaseName { get; set; }
        public string PublishSqlServerConnectionString { get; set; }
    }

    public class ApiSettings
    {
        public int Port { get; set; }
        public bool Enabled { get; set; }
        public bool Diagnostics { get; set; }
    }

    public class PackageSettings
    {
        public bool ManualDownload { get; set; }
        public bool UpdatePackages { get; set; }
        public bool NewPackages { get; set; }
        public bool PreReleaseChannel { get; set; }
    }

    public class CopilotSettings
    {
        public CopilotSettings()
        {
            Enabled = false;
            AverageCpuUsage = 1;
            BoostPercentage = 200;
            BurstDuration = TimeSpan.Zero.ToString();
        }
        public bool Enabled { get; set; }
        public float AverageCpuUsage { get; set; }
        public float BoostPercentage { get; set; }
        public string BurstDuration { get; set; }
    }

    public class PublishSettings
    {
        public bool PublishToSQL { get; set; }
        public bool PublishToCustomDll { get; set; }
        public bool PublishToXML { get; set; }
        public bool PublishAllActivitiesForMO { get; set; }
        public bool RunStoredProcedureAfterPublish { get; set; }
        public string PostPublishStoredProcedureName { get; set; }
        public bool RunProgramAfterPublish { get; set; }
        public string RunProgramPath { get; set; }
        public string RunProgramCommandLine { get; set; }
        public bool PublishInLocalTime { get; set; }
        public bool NetChangePublishingEnabled { get; set; }
        public bool RunStoredProcedureAfterNetChangePublish { get; set; }
        public string NetChangeStoredProcedureName { get; set; }
        public bool PublishInventory { get; set; }
        public bool PublishJobs { get; set; }
        public bool PublishManufacturingOrders { get; set; }
        public bool PublishOperations { get; set; }
        public bool PublishActivities { get; set; }
        public bool PublishBlocks { get; set; }
        public bool PublishBlockIntervals { get; set; }
        public bool PublishProductRules { get; set; }
        public bool PublishCapacityIntervals { get; set; }
        public bool PublishTemplates { get; set; }
    }

    public class InstancePublicInfo
    {
        public string SystemServiceUrl { get; set; }
        public string InstanceName { get; set; }
        public string SoftwareVersion { get; set; }
        public string SystemServiceName => $"PlanetTogether {InstanceId} System";
        public object AdminMessage { get; set; }
        public DateTime CreationDate { get; set; }
        public EnvironmentType EnvironmentType { get; set; } = EnvironmentType.Dev;
        public bool ActiveDirectoryAllowed { get; set; }
        public bool SsoAllowed { get; set; }
        public bool AllowPasswordSettings { get; set; }
        public bool AutoStart { get; set; }
        /// <summary>
        /// Readable identifier for an instance. The server manager enforces uniqueness among name/version combos, as do conventions on our cloud.
        /// However, anything needing a unique, unspoofable identifier should use <see cref="InstanceSettings.InstanceId"/>
        /// </summary>
        public string InstanceId => $"{InstanceName} {SoftwareVersion}";
        public Currentintegration CurrentIntegration { get; set; } = new();
    }

    public class Currentintegration
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string ERPDatabaseName { get; set; }
        public string PreImportSQL { get; set; }
        public string SQLServerConnectionString { get; set; }
        public bool RunPreImportSQL { get; set; }
        public string UserName { get; set; }
        public string ERPServerName { get; set; }
        public string ERPUserName { get; set; }
        public string ERPPassword { get; set; }
    }

    public class Licenseinfo
    {
        public int SessionId { get; set; }
        public object CpuId { get; set; }
        public string SerialCode { get; set; }
        public string SiteId { get; set; }
        public bool ReadOnly { get; set; }
        public string Subscription { get; set; } = string.Empty;
        public int SubscriptionId { get; set; }
    }

    public class Loghelper
    {
        public bool SystemHasLogs { get; set; }
        public bool InterfaceHasLogs { get; set; }
        public bool PackagesHasLogs { get; set; }
    }

    public class Servicepaths
    {
        private string m_integrationCode;
        private string m_instanceId;
        private ServerDirectoryPaths m_paths;
        private bool IsInitiated { get; set; }
        public void Init(string a_planningAreaFullName, CompanyServer? a_server)
        {
            if (!IsInitiated)
            {
                InstanceId = a_planningAreaFullName;
                m_paths = new ServerDirectoryPaths(a_server?.ServerManagerPath ?? "");
                IsInitiated = true;
            }
        }

        public string IntegrationCode
        {
            get { return m_integrationCode; }
            set { m_integrationCode = value; }
        }

        public string InstanceId
        {
            get { return m_instanceId; }
            set { m_instanceId = value; }
        }

        /// <summary>
        /// The path to the folder containing all data for one Instance.
        /// </summary>
        public string GetDataFolder()
        {
            return Path.Combine(m_paths.ServerManagerPath, InstanceId);
        }

        /// <summary>
        /// The path to the folder containing all data for one Instance.
        /// </summary>
        public string GetWorkspacesFolder()
        {
            return Path.Combine(SystemServiceWorkingDirectory, "Data", "Workspaces");
        }

        /// <summary>
        /// The full path to the specific integration folder based on the Instances IntegrationCode.
        /// </summary>
        public string GetIntegrationFilesSubFolder()
        {
            return Path.Combine(GetIntegrationFilesFolder(), IntegrationCode);
        }

        /// <summary>
        /// The full path to the Special Versions folder in the data folder.
        /// </summary>
        public string GetIntegrationFilesFolder()
        {
            return System.IO.Path.Combine(this.GetDataFolder(), "IntegrationFiles");
        }

        /// <summary>
        /// The full path to the Packages folder in the data folder.
        /// </summary>
        public string GetPackagesFilesFolder()
        {
            return SystemServicePackagesFolder;
        }

        public string SystemServiceWorkingDirectory
        {
            get => System.IO.Path.Combine(GetDataFolder(), "System");
            set {; }
        }

        public string SystemServiceRecordingsFolder
        {
            get => Path.Combine(SystemServiceWorkingDirectory, "Data\\Recordings");
            set {; }
        }
        public string SystemServicePackagesFolder
        {
            get => Path.Combine(SystemServiceWorkingDirectory, "Data\\Packages");
            set {; }
        }

        public string SystemServiceStartupRecordingsFolder
        {
            get => Path.Combine(SystemServiceWorkingDirectory, "Data\\StartupRecordings");
            set {; }
        }

        public string SystemServiceAlertsFolder
        {
            get => Path.Combine(SystemServiceWorkingDirectory, "Data\\Alerts");
            set {; }
        }

        public string SystemServiceScenariosFolder
        {
            get => Path.Combine(SystemServiceWorkingDirectory, "Data\\Scenario");
            set {; }
        }

        public string SystemServiceKeyFolder
        {
            get => Path.Combine(SystemServiceWorkingDirectory, "Data\\Key");
            set {; }
        }

        public string SystemServiceKeyJsonFilePath
        {
            get => Path.Combine(SystemServiceKeyFolder, PTKeyJsonFileName);
            set {; }
        }

        public string SystemInstanceSettingsDirectory
        {
            get => SystemServiceWorkingDirectory;
            set {; }
        }
        public string SystemInstanceConnectionFilePath
        {
            get => Path.Combine(SystemInstanceSettingsDirectory, "configuration.json");
            set {; }
        }

        public string ScenariosFilePath
        {
            get => Path.Combine(SystemServiceScenariosFolder, "Scenarios.dat");
            set {; }
        }

        public string InterfaceServiceWorkingDirectory
        {
            get => System.IO.Path.Combine(GetDataFolder(), "Interface");
            set {; }
        }

        public string InterfaceFileNameWithFullPath
        {
            get => Path.Combine(GetIntegrationFilesFolder(), "APSInterfaceSettings.xml");
            set {; }
        }


        public string PackageWorkingDirectory
        {
            get => System.IO.Path.Combine(GetDataFolder(), "Packages");
            set {; }
        }

        public string ClientUpdaterServiceWorkingDirectory
        {
            get => System.IO.Path.Combine(GetDataFolder(), "ClientUpdaterService");
            set {; }
        }

        public string ClientUpdaterServiceBaseFilesFolder
        {
            get => System.IO.Path.Combine(ClientUpdaterServiceWorkingDirectory, "UpdateFiles");
            set {; }
        }

        public string ClientUpdaterServiceReportsFilesFolder
        {
            get => System.IO.Path.Combine(ClientUpdaterServiceBaseFilesFolder, "Reports");
            set {; }
        }

        public string PTKeyJsonFileName { get => "pt.json"; }

    }

    /// <summary>
    /// Paths.cs class from SM, made non-static for better use contextually. Because the original class was static, we can't access this one quite the same.
    /// TODO: Split up classes in this monolith file. This could probably be related to other CompanyServer helper classes
    /// </summary>
    public class ServerDirectoryPaths
    {

        private string m_instanceDataPath;

        public ServerDirectoryPaths(string a_rootDirectoryPath)
        {
            m_instanceDataPath = a_rootDirectoryPath;
        }

        /// <summary>
        /// The top level path for the Server Manager where data and program files, etc. are contained.
        /// </summary>
        public string ServerManagerPath => m_instanceDataPath;

        /// <summary>
        /// Folder where software versions are stored.
        /// </summary>
        public string SoftwarePath => Path.Combine(ServerManagerPath, "Software");

        //public List<string> GetInstalledVersions()
        //{
        //    List<string> listOfFolderNames = new List<string>();
        //    if (Directory.Exists(SoftwarePath))
        //    {
        //        string[] versionFolderNames = Directory.GetDirectories(SoftwarePath);
        //        for (int i = 0; i < versionFolderNames.Length; i++)
        //        {
        //            DirectoryInfo dirInfo = new DirectoryInfo(versionFolderNames.GetValue(i).ToString());
        //            if (SoftwareVersion.IsValidVersion(dirInfo.Name))
        //            {
        //                SoftwareVersion version = new SoftwareVersion(dirInfo.Name);
        //                if (version.MeetsMinimum(new SoftwareVersion(12, 0, 0)))
        //                {
        //                    if (dirInfo.GetDirectories("ProgramFiles").Length > 0)
        //                    {
        //                        listOfFolderNames.Add(dirInfo.Name);
        //                    }
        //                    else
        //                    {
        //                        //This was an invalid software folder, there are no program files
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Directory.CreateDirectory(SoftwarePath);
        //    }

        //    return listOfFolderNames;
        //}

        const string c_serverSettingsJsonFileName = "PlanetTogether_ServerSettings.json";
        /// <summary>
        /// The full path and file name for the xml file that contains Server settings.
        /// </summary>
        public string ServerSettingsJsonFilePath
        {
            get
            {
                return Path.Combine(ServerManagerPath, c_serverSettingsJsonFileName);
            }
        }

        const string c_serverSettingsJsonFileBackupName = "PlanetTogether_ServerSettings_Backup.json";
        public string ServerSettingsJsonFileBackupPath
        {
            get
            {
                return Path.Combine(ServerManagerPath, c_serverSettingsJsonFileBackupName);
            }
        }

        public string ServerSettingsJsonFileAutomaticBackupsPath
        {
            get
            {
                return Path.Combine(ServerManagerPath, "AutomaticBackups");
            }
        }

        /// <summary>
        /// Gets the parent path for all the software versions.  This is NOT the ProgramFiles path, but its parent.
        /// </summary>
        public string GetPathForVersion(string a_version)
        {
            return Path.Combine(SoftwarePath, a_version);
        }

        public bool VersionInstalled(string aVersion)
        {
            return Directory.Exists(GetPathForVersion(aVersion));
        }

        #region Special Versions Folders

        /// <summary>
        /// Path to the folder that contains the program file subfolders like System, ClientUpdater, etc.
        /// </summary>
        public string GetProgramFilesPathForVersion(string a_version)
        {
            return Path.Combine(GetPathForVersion(a_version), "ProgramFiles");
        }

        /// <summary>
        /// Path to the folder that contains the zipped client files
        /// </summary>
        public string GetClientPathForVersion(string a_version)
        {
            return Path.Combine(GetPathForVersion(a_version), "Client.zip");
        }

        // {Version}\IntegrationFiles
        public string GetIntegrationFilesFolder()
        {
            return Path.Combine(ServerManagerPath, "IntegrationFiles");
        }

        // {Version}\PackagesFiles
        public string GetPackagesFilesFolderForVersion(string aVersion)
        {
            return Path.Combine(GetPathForVersion(aVersion), "Packages");
        }

        // {Version}\UpdateFiles
        public string GetUpdateFilesPathForVersion(string aVersion)
        {
            return Path.Combine(GetPathForVersion(aVersion), "UpdateFiles");
        }

        // {Version}\UpdateFiles\UserSettings
        public string GetUserSettingsPathForVersion(string aVersion)
        {
            return Path.Combine(GetUpdateFilesPathForVersion(aVersion), "UserSettings");
        }

        public const string UI_CUSTOMIZATIONS_SUBFOLDERNAME = "";

        // {Version}\UpdateFiles\UICustomization
        public string GetUICustomizationPathForVersion(string aVersion)
        {
            return Path.Combine(GetUpdateFilesPathForVersion(aVersion), UI_CUSTOMIZATIONS_SUBFOLDERNAME);
        }

        public const string UPDATEFILES_CONFIG_FILENAME = "UpdateFiles.config";



        // // {Version}/UpdateFiles/ConfigFile.Config
        public string GetUpdateFilesConfigFilePathForVersion(string aVersion)
        {
            return Path.Combine(GetUpdateFilesPathForVersion(aVersion), UPDATEFILES_CONFIG_FILENAME);
        }

        #endregion Special Version folders
    }


    public class Credentials
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AdminUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public enum EStartType
    {
        NotSet = 0,
        //
        // Summary:
        //     Load the scenario file in the scenario folder. If the scenario file doesn't
        //     exist then a new blank one is created.
        Normal = 1,
        //
        // Summary:
        //     Create a new system at startup.
        Fresh = 2,
        //
        // Summary:
        //     Loads the scenario and play the transmissions in a specified folder.
        Recording = 4,
        //
        // Summary:
        //     This gives clients the opportunity to start before the recording begins.
        //     The service starts but playback is delayed until the BeginReplay message
        //     is sent by from the sample data generator.
        RecordingClientDelayed = 5,
        //
        // Summary:
        //     Run a recording. After each optimization has completed store the schedule
        //     and inventory allocations in a file in the unit test base folder.  The ToString()
        //     value of this enumeration element is used as the directory name for this
        //     type of unit test.
        UnitTestBase = 6,
        //
        // Summary:
        //     Run a recording. After each optimization has completed store the schedule
        //     and inventory allocations in a file in the unit test folder. You can compare
        //     these files against the files in the unit test base folder to determine whether
        //     any differences exist (suggesting a bug).  The ToString() value of this enumeration
        //     element is used as the directory name for this type of unit test.
        UnitTest = 7,
        //
        // Summary:
        //     Attempt to reproduce an exception in scenario detail. If an exception does occur, attempt to remove data that is 
        //     not related to the error to simplify the scenario.
        Prune = 8,
        Scenario = 9,
    }

    public enum ERegistrationStatus
    {
        Pending,
        Created,
        Error,
        Unknown
    }

}
