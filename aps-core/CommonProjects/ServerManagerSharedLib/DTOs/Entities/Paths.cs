
namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    /// <summary>
    /// Provides path data for common PT locations.
    /// Note: The static <see cref="InstanceDataFolder"/> should be set with the main directory of the Server Manager (e.g. C:\ProgramData) before using this class,
    /// otherwise dev defaults are used that are likely not desired.
    /// </summary>
    // Changing this weird behavior would break existing nuget usages of this class, so best to let it lie.
    public class Paths
    {

        private static string s_instanceDataPath;
        public static string InstanceDataFolder
        {
            set
            {
                s_instanceDataPath = value;
            }
        }

        public static string ServerManagerPath
        {
            get
            {
                if (string.IsNullOrEmpty(s_instanceDataPath))
                {
                    string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    DirectoryInfo dirInfo = new DirectoryInfo(exePath);
                    //development version, keep same drive letter but use specific path.
                    string newPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.DoNotVerify), "PlanetTogetherServerManagerDEV");
                    return newPath.Replace("C:\\", dirInfo.Root.Name);
                }

                return s_instanceDataPath;
            }
        }

        /// <summary>
        /// Folder where software versions are stored.
        /// </summary>
        public static string SoftwarePath
        {
            get { return Path.Combine(ServerManagerPath, "Software"); }
        }

        public static List<string> GetInstalledVersions()
        {
            List<string> listOfFolderNames = new List<string>();
            if (Directory.Exists(SoftwarePath))
            {
                string[] versionFolderNames = Directory.GetDirectories(SoftwarePath);
                for (int i = 0; i < versionFolderNames.Length; i++)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(versionFolderNames.GetValue(i).ToString());
                    if (SoftwareVersion.IsValidVersion(dirInfo.Name))
                    {
                        SoftwareVersion version = new SoftwareVersion(dirInfo.Name);
                        if (version.MeetsMinimum(new SoftwareVersion(12, 0, 0)))
                        {
                            if (dirInfo.GetDirectories("ProgramFiles").Length > 0)
                            {
                                listOfFolderNames.Add(dirInfo.Name);
                            }
                            else
                            {
                                //This was an invalid software folder, there are no program files
                            }
                        }
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(SoftwarePath);
            }

            return listOfFolderNames;
        }

        const string c_serverSettingsJsonFileName = "PlanetTogether_ServerSettings.json";
        /// <summary>
        /// The full path and file name for the xml file that contains Server settings.
        /// </summary>
        public static string ServerSettingsJsonFilePath
        {
            get
            {
                return Path.Combine(ServerManagerPath, c_serverSettingsJsonFileName);
            }
        }

        const string c_serverSettingsJsonFileBackupName = "PlanetTogether_ServerSettings_Backup.json";
        public static string ServerSettingsJsonFileBackupPath
        {
            get
            {
                return Path.Combine(ServerManagerPath, c_serverSettingsJsonFileBackupName);
            }
        }

        public static string ServerSettingsJsonFileAutomaticBackupsPath
        {
            get
            {
                return Path.Combine(ServerManagerPath, "AutomaticBackups");
            }
        }

        /// <summary>
        /// Gets the parent path for all the software versions.  This is NOT the ProgramFiles path, but its parent.
        /// </summary>
        public static string GetPathForVersion(string a_version)
        {
            return Path.Combine(SoftwarePath, a_version);
        }

        public static bool VersionInstalled(string aVersion)
        {
            return Directory.Exists(GetPathForVersion(aVersion));
        }

        #region Special Versions Folders

        /// <summary>
        /// Path to the folder that contains the program file subfolders like System, ClientUpdater, etc.
        /// </summary>
        public static string GetProgramFilesPathForVersion(string a_version)
        {
            return Path.Combine(GetPathForVersion(a_version), "ProgramFiles");
        }

        /// <summary>
        /// Path to the folder that contains the zipped client files
        /// </summary>
        public static string GetClientPathForVersion(string a_version)
        {
            return Path.Combine(GetPathForVersion(a_version), "Client.zip");
        }

        // {Version}\IntegrationFiles
        public static string GetIntegrationFilesFolder()
        {
            return Path.Combine(ServerManagerPath, "IntegrationFiles");
        }

        // {Version}\PackagesFiles
        public static string GetPackagesFilesFolderForVersion(string aVersion)
        {
            return Path.Combine(GetPathForVersion(aVersion), "Packages");
        }

        // {Version}\UpdateFiles
        public static string GetUpdateFilesPathForVersion(string aVersion)
        {
            return Path.Combine(GetPathForVersion(aVersion), "UpdateFiles");
        }

        // {Version}\UpdateFiles\UserSettings
        public static string GetUserSettingsPathForVersion(string aVersion)
        {
            return Path.Combine(GetUpdateFilesPathForVersion(aVersion), "UserSettings");
        }

        public const string UI_CUSTOMIZATIONS_SUBFOLDERNAME = "";

        // {Version}\UpdateFiles\UICustomization
        public static string GetUICustomizationPathForVersion(string aVersion)
        {
            return Path.Combine(GetUpdateFilesPathForVersion(aVersion), UI_CUSTOMIZATIONS_SUBFOLDERNAME);
        }

        public const string UPDATEFILES_CONFIG_FILENAME = "UpdateFiles.config";

        

        // // {Version}/UpdateFiles/ConfigFile.Config
        public static string GetUpdateFilesConfigFilePathForVersion(string aVersion)
        {
            return Path.Combine(GetUpdateFilesPathForVersion(aVersion), UPDATEFILES_CONFIG_FILENAME);
        }

        /// <summary>
        /// Where we store files just for partners.
        /// </summary>
        public static string SpecialVersionsPartnersFolder(string aVersion)
        {
            return Path.Combine(GetIntegrationFilesFolder(), "Partners");
        }

        public static string SpecialVersionsAccpacFolder(string aVersion)
        {
            return Path.Combine(GetIntegrationFilesFolder(), "Accpac");
        }

        public static string SpecialVersionsO2Folder(string aVersion)
        {
            return Path.Combine(GetIntegrationFilesFolder(), "O2");
        }

        public static string SpecialVersionsZemeterFolder(string aVersion)
        {
            return Path.Combine(GetIntegrationFilesFolder(), "Zemeter");
        }

        public static string SpecialVersionsDemandSolutionsFolder(string aVersion)
        {
            return Path.Combine(GetIntegrationFilesFolder(), "DemandSolutions");
        }

        public static string SpecialVersionsSpokaneFolder(string aVersion)
        {
            return Path.Combine(GetIntegrationFilesFolder(), "Spokane");
        }

        public static string SpecialVersionsNAVFolder(string aVersion)
        {
            return Path.Combine(GetIntegrationFilesFolder(), "NAV");
        }

        public static string SpecialVersionsGPFolder(string aVersion)
        {
            return Path.Combine(GetIntegrationFilesFolder(), "GP");
        }

        public static string SpecialVersionsProductionOneFolder(string aVersion)
        {
            return Path.Combine(GetIntegrationFilesFolder(), "ProductionOne");
        }

        #endregion Special Version folders
    }
}
