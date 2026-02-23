using System.IO;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    /// <summary>
    /// Paths for common PT Instance Service directories.
    /// NOTE: This method depends on initializing the static <see cref="Paths.InstanceDataFolder"/> before use to ensure proper values. 
    /// </summary>
    public class ServicePaths
    {
        private string m_integrationCode;
        private string m_instanceId;

        public ServicePaths(string a_instanceId, string a_integrationCode)
        {
            InstanceId = a_instanceId;
            m_integrationCode = a_integrationCode;
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
        /// NOTE: This method depends on initializing the static <see cref="Paths.InstanceDataFolder"/> before use to ensure proper values. 

        /// </summary>
        public string GetDataFolder()
        {
            return Path.Combine(Paths.ServerManagerPath, InstanceId);
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
            get => Path.Combine(SystemServiceWorkingDirectory, "Data\\Keys");
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
}
