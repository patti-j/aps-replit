using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Scheduler.Sessions;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.SystemServiceDefinitions;

namespace PT.Scheduler;

/// <summary>
/// This class provides functionality to override the static working directory paths. Each directory is set to a default based on the root directory
/// Individual directories can be overriden to a different full file path
/// </summary>
public class WorkingDirectoryPaths
{
    internal static readonly string c_dataFolderPath = "Data";
    internal static readonly string c_systemFolderPath = "System";
    internal static readonly string c_keyFolderPath = "Keys";
    internal static readonly string c_legacyKeyFolderPath = "Key";

    public WorkingDirectoryPaths(string a_rootDirectory, string a_devPackagePath = "", string a_keyFile = "")
    {
        #if DEBUG
        KeyFile = a_keyFile;
        #endif
        DevPackagePath = a_devPackagePath;
        SetWorkingDirectoryPaths(a_rootDirectory);
    }

    public bool RunningMassRecordings;
    private bool UsingLegacyServerManager;

    public WorkingDirectoryPaths(StartupValsAdapter a_constructorValues)
    {
//#if DEBUG
//            KeyFile = a_constructorValues.KeyFile;
//#endif
        DevPackagePath = a_constructorValues.DevPackagePath;
        RunningMassRecordings = a_constructorValues.RunningMassRecordings;

        // I create an ErrorReporter here because the PTSystem may not exist.
        string workingDirectory;

        try
        {
            workingDirectory = a_constructorValues.WorkingDirectory;

            if (!RunningMassRecordings && Directory.Exists(workingDirectory) == false)
            {
                workingDirectory = ".";
            }
        }
        catch
        {
            workingDirectory = ".";
        }

        UsingLegacyServerManager = string.IsNullOrEmpty(a_constructorValues.WebAppEnv);

        SetWorkingDirectoryPaths(workingDirectory);
    }

    public void SetWorkingDirectoryPaths(string a_rootDirectory)
    {
        if (string.IsNullOrEmpty(a_rootDirectory))
        {
            throw new WorkingDirectory.WorkingDirectoryException("2609", new object[] { FileUtils.AppConfigFileName });
        }

        m_rootDirectory = Path.GetFullPath(FileUtils.CleanPath(a_rootDirectory));

        //Defaults
        m_logDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, "Alerts");

        m_parentDirectory = Directory.GetParent(m_rootDirectory).FullName;
        m_clientUpdaterDirectory = Path.Combine(m_parentDirectory, "ClientUpdaterService");
        m_recordingsDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, "Recordings");
        m_startupRecordingsDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, "StartupRecordings");
        if (UsingLegacyServerManager)
        {
            m_keyDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, c_legacyKeyFolderPath);
        }
        else
        {
            m_keyDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, c_keyFolderPath);
        }
        m_scenarioDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, "Scenario");
        m_prunedDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, "Pruned");
        m_unitTestDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, EStartType.UnitTest.ToString());
        m_unitTestBaseDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, EStartType.UnitTestBase.ToString());
        m_workspacesDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, "Workspaces");

        m_checkpoints = Path.Combine(a_rootDirectory, c_systemFolderPath, "Checkpoints");
        m_compression = Path.Combine(a_rootDirectory, c_systemFolderPath, "Compression");
        m_temp = Path.Combine(a_rootDirectory, c_systemFolderPath, "Temp");
        m_customizationsWorking = m_temp;

        string myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrEmpty(myDocPath)) // This can happen when this is run by a built-in user such as System or LocalService
        {
            myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
        }

        m_export = Path.Combine(myDocPath, "APS Export Files");
        m_images = Path.Combine(m_rootDirectory, "Images");
        m_defaultWorkspaceBackupDirectory = Path.Combine(m_rootDirectory, "Workspace Profile Backups");

        //Packages Directory, the location depends on dev packages and server/client
        if (!string.IsNullOrEmpty(DevPackagePath))
        {
            //Override package path if \DevPackagesPath: parameter is set
            m_packagesDirectory = DevPackagePath;
        }
        else if (PTSystem.Server)
        {
            //Load from the instance's package folder
            m_packagesDirectory = Path.Combine(a_rootDirectory, c_dataFolderPath, "Packages");
        }
        else
        {
            //TODO: Test for external
            //If we are the client, load from where the package files were synced to
            m_packagesDirectory = Path.Combine(ParentDirectory, "Packages");
        }
    }

    private string m_rootDirectory;
    private string m_logDirectory;
    private string m_clientUpdaterDirectory;
    private string m_recordingsDirectory;
    private string m_startupRecordingsDirectory;
    private string m_keyDirectory;
    private string m_scenarioDirectory;
    private string m_prunedDirectory;
    private string m_unitTestDirectory;
    private string m_unitTestBaseDirectory;
    private string m_parentDirectory;
    private string m_workspacesDirectory;
    private string m_packagesDirectory;

    /// <summary>
    /// overrides the standard paths if corresponding settings are set in constructor values.
    /// </summary>
    /// <param name="a_constructorValues"></param>
    internal void OverrideStandardPaths(StartupVals a_constructorValues)
    {
        if (!string.IsNullOrEmpty(a_constructorValues.LogFolder))
        {
            m_logDirectory = a_constructorValues.LogFolder;
        }

        if (!string.IsNullOrEmpty(a_constructorValues.KeyFolder))
        {
            m_keyDirectory = a_constructorValues.KeyFolder;
        }
    }

    private string m_checkpoints;
    private string m_temp;
    private string m_compression;
    private string m_customizationsWorking;

    private string m_export;
    private string m_images;
    private string m_defaultWorkspaceBackupDirectory;

    private string m_devPackagePath = string.Empty;
    #if DEBUG
    private string m_keyFile;
    #endif

    public string ParentDirectory
    {
        get => m_parentDirectory;
        private set => m_parentDirectory = FileUtils.CleanPath(value);
    }

    public string LogDirectory
    {
        get => m_logDirectory;
        private set => m_logDirectory = FileUtils.CleanPath(value);
    }

    public string RootDirectory => m_rootDirectory;

    public string ClientUpdaterDirectory
    {
        get => m_clientUpdaterDirectory;
        private set => m_clientUpdaterDirectory = FileUtils.CleanPath(value);
    }

    public string RecordingsDirectory
    {
        get => m_recordingsDirectory;
        private set => m_recordingsDirectory = FileUtils.CleanPath(value);
    }

    public string StartupRecordingsDirectory
    {
        get => m_startupRecordingsDirectory;
        private set => m_startupRecordingsDirectory = FileUtils.CleanPath(value);
    }

    public string KeyDirectory
    {
        get => m_keyDirectory;
        private set => m_keyDirectory = FileUtils.CleanPath(value);
    }

    public string ScenarioDirectory
    {
        get => m_scenarioDirectory;
        private set => m_scenarioDirectory = FileUtils.CleanPath(value);
    }

    public string PrunedDirectory
    {
        get => m_prunedDirectory;
        private set => m_prunedDirectory = FileUtils.CleanPath(value);
    }

    public string UnitTestDirectory
    {
        get => m_unitTestDirectory;
        private set => m_unitTestDirectory = FileUtils.CleanPath(value);
    }

    public string UnitTestBaseDirectory
    {
        get => m_unitTestBaseDirectory;
        private set => m_unitTestBaseDirectory = FileUtils.CleanPath(value);
    }

    public string CheckpointsDirectory
    {
        set => m_checkpoints = FileUtils.CleanPath(value);
        get => m_checkpoints;
    }

    public string CompressionDirectory
    {
        get => m_compression;
        private set => m_compression = FileUtils.CleanPath(value);
    }

    public string TempDirectory
    {
        private set => m_temp = FileUtils.CleanPath(value);
        get => m_temp;
    }

    public string ExportDirectory
    {
        get => m_export;
        private set => m_export = FileUtils.CleanPath(value);
    }

    public string ImagesDirectory
    {
        set => m_images = FileUtils.CleanPath(value);
        get => m_images;
    }

    public string DefaultWorkspaceBackupDirectory
    {
        get => m_defaultWorkspaceBackupDirectory;
        private set => m_defaultWorkspaceBackupDirectory = FileUtils.CleanPath(value);
    }

    /// <summary>
    /// The instance's data folder for Workspaces
    /// </summary>
    public string WorkspacesDirectory
    {
        set => m_workspacesDirectory = FileUtils.CleanPath(value);
        get => m_workspacesDirectory;
    }

    public string DevPackagePath
    {
        get => m_devPackagePath;
        set => m_devPackagePath = value;
    }

    public string PackagesDirectory
    {
        get => m_packagesDirectory;
        set => m_packagesDirectory = value;
    }
    #if DEBUG
    public string KeyFile
    {
        get => m_keyFile;
        set => m_keyFile = value;
    }
    #endif
}

/// <summary>
/// Contains static properties of paths such as the Scenario path, Export path, etc...
/// It also creates directories when the directory property is set and calls the function to initialize the
/// WorkingPaths class.
/// </summary>
public class WorkingDirectory
{
    #region Working Directory. When set all other paths are set.
    /// <summary>
    /// The working directory. This is where PT is free to write whatever it want to.
    /// </summary>
    private static WorkingDirectoryPaths s_pathStructure;

    public static bool IsSetup => s_pathStructure == null;

    public WorkingDirectory(WorkingDirectoryPaths a_structure)
    {
        InitializeWorkingDirectory(a_structure);
    }

    private void InitializeWorkingDirectory(WorkingDirectoryPaths a_structure)
    {
        try
        {
            s_pathStructure = a_structure;

            if (!Directory.Exists(s_pathStructure.RootDirectory))
            {
                Directory.CreateDirectory(s_pathStructure.RootDirectory);
            }

            Directory.SetCurrentDirectory(s_pathStructure.RootDirectory);

            CreateDataFolders();
            CreateSystemFolders();
            CreateUserFolders();
        }
        catch (Exception e)
        {
            throw new WorkingDirectoryException("2608", e, new object[] { s_pathStructure.RootDirectory, FileUtils.AppConfigFileName, e.Message });
        }
    }
    #endregion

    #region Data Folders
    /// <summary>
    /// The base directory for ClientUpdater Update Files Folder
    /// </summary>
    public string ClientUpdaterUpdateFiles => Path.Combine(s_pathStructure.ClientUpdaterDirectory, "UpdateFiles");

    /// <summary>
    /// The base folder for UserSettings used by ClientUpdaterService
    /// </summary>
    public string ClientUpdaterUserSettings => Path.Combine(s_pathStructure.ClientUpdaterDirectory, "UpdateFiles", "UserSettings");

    public static void CreateDataFolders()
    {
        string dataFolder = Path.Combine(s_pathStructure.RootDirectory, WorkingDirectoryPaths.c_dataFolderPath);
        Directory.CreateDirectory(dataFolder);
        Directory.SetCurrentDirectory(dataFolder);


        BroadcastingAlerts.BroadcastingAlertsAndExceptionsWorkingDirectory = s_pathStructure.LogDirectory;

        Directory.CreateDirectory(s_pathStructure.LogDirectory);
        Directory.CreateDirectory(s_pathStructure.RecordingsDirectory);
        Directory.CreateDirectory(s_pathStructure.StartupRecordingsDirectory);
        Directory.CreateDirectory(s_pathStructure.ScenarioDirectory);
        Directory.CreateDirectory(s_pathStructure.PrunedDirectory);
        Directory.CreateDirectory(s_pathStructure.KeyDirectory);
        
        //TODO: Server Manager. Remove when only using SA
        Directory.CreateDirectory(s_pathStructure.KeyDirectory.TrimEnd('s'));
        
        Directory.CreateDirectory(s_pathStructure.UnitTestDirectory);
        Directory.CreateDirectory(s_pathStructure.UnitTestBaseDirectory);
        Directory.CreateDirectory(s_pathStructure.WorkspacesDirectory);
    }

    /// <summary>
    /// Store all logs here.
    /// </summary>
    public string LogFolder => s_pathStructure.LogDirectory;

    /// <summary>
    /// Used to store the results of simulations. This special folder holds the results from which later Unit Tests are compared to.
    /// </summary>
    public string UnitTestBase => s_pathStructure.UnitTestBaseDirectory;

    /// <summary>
    /// Used during unit testing to store the results of simulations. The files in this folder can be compared to the files in the UnitTestBase folder
    /// to help identify unwanted simulation results.
    /// </summary>
    public string UnitTest => s_pathStructure.UnitTestDirectory;
    #endregion

    #region System Folders
    private void CreateSystemFolders()
    {
        string systemFolder = Path.Combine(s_pathStructure.RootDirectory, WorkingDirectoryPaths.c_systemFolderPath);
        Directory.CreateDirectory(systemFolder);

        string broadcasting = Path.Combine(systemFolder, "Broadcasting");
        Directory.CreateDirectory(broadcasting);
        BroadcastingFolder = broadcasting;

        CleanAndCreate(s_pathStructure.CheckpointsDirectory);
        CleanAndCreate(s_pathStructure.CompressionDirectory);
        CleanAndCreate(s_pathStructure.TempDirectory);
    }

    public string BroadcastingFolder;

    /// <summary>
    /// Where scenario checkpoints are stored.
    /// </summary>
    public string Checkpoints => s_pathStructure.CheckpointsDirectory;

    /// <summary>
    /// Used for compression.
    /// </summary>
    public string Compression => s_pathStructure.CompressionDirectory;

    /// <summary>
    /// Temporary system files.
    /// </summary>
    public string Temp => s_pathStructure.TempDirectory;
    #endregion

    #region User Folders
    private void CreateUserFolders()
    {
        Directory.CreateDirectory(s_pathStructure.ExportDirectory);

        string oldExport = Path.Combine(s_pathStructure.RootDirectory, "Export");
        Directory.CreateDirectory(oldExport);

        Directory.CreateDirectory(s_pathStructure.ImagesDirectory);

        Common.Directory.DirectoryUtils.ValidateDirectory(PackagesPath);
    }

    public string PackagesPath
    {
        get => s_pathStructure.PackagesDirectory;
        set => s_pathStructure.PackagesDirectory = value;
    }

    public string LicensingPath { get; set; }

    /// <summary>
    /// Used for exporting xml, etc.
    /// </summary>
    public string Export => s_pathStructure.ExportDirectory;

    /// <summary>
    /// Used for storing icons, etc.
    /// </summary>
    public string Images => s_pathStructure.ImagesDirectory;

    public string Pruned => s_pathStructure.PrunedDirectory;

    /// <summary>
    /// Where the system and all related scenario data are stored.
    /// </summary>
    public string Scenario => s_pathStructure.ScenarioDirectory;

    /// <summary>
    /// Used for recording transmissions.
    /// </summary>
    public string RecordingRootFolder => s_pathStructure.RecordingsDirectory;

    /// <summary>
    /// Used for recording transmissions.
    /// </summary>
    public string StartupRecordingsPath => s_pathStructure.StartupRecordingsDirectory;

    /// <summary>
    /// Where instance default workspaces are stored
    /// </summary>
    public string WorkspacesPath => s_pathStructure.WorkspacesDirectory;

    public string DefaultWorkspaceBackup => s_pathStructure.DefaultWorkspaceBackupDirectory + "workspaceProfileBackup.ptwst";

    /// <summary>
    /// If running in DEBUG mode this is where the Scenario schedule file and client schedule file are stored when a checksum failure occurs.
    /// You can compare these two files to see what the differences are between the client and server.
    /// These are stored in a subdirectory structure: one subdirectory per UserId experiencing a desync, then with one subdirectory each for Client and Server.
    /// Each of those directories should have a file with a matching name based on a particularly TransmissionId (the one catching the desync), with the corresponding scenario description.
    /// </summary>
    public string CheckSumDifferences => Path.Combine(s_pathStructure.LogDirectory, "DesyncDiagnostics");

    //TODO:  Too many key file configs.  Combine with workingPath constant
    private const string c_configFileName = "pt.json";

    public string KeyFilePath
    {
        get
        {
            #if DEBUG
            if (!string.IsNullOrEmpty(KeyFile))
            {
                return KeyFile;
            }
            #endif
            return Path.Combine(s_pathStructure.KeyDirectory, c_configFileName);
        }
    }

    /// <summary>
    /// This is where key and feature files are stored.
    /// </summary>
    public string Key
    {
        get
        {
            #if DEBUG
            if (!string.IsNullOrEmpty(KeyFile))
            {
                return Path.GetDirectoryName(KeyFile);
            }
            #endif
            return s_pathStructure.KeyDirectory;
        }
    }
    #if DEBUG
    /// <summary>
    /// This is where keyFile is stored.
    /// </summary>
    public string KeyFile => s_pathStructure.KeyFile;
    #endif
    #endregion

    #region Validation functions and Exception definitions
    [Serializable]
    public class WorkingDirectoryException : PTException
    {
        internal WorkingDirectoryException(string message, Exception innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
            : base(message, innerException, a_stringParameters, a_appendHelpUrl) { }

        internal WorkingDirectoryException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
            : base(message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region helpers
    /// <summary>
    /// Deletes the directory if it already exists.
    /// </summary>
    /// <param name="a_path">The path to the directory you want to create.</param>
    private static void CleanAndCreate(string a_path)
    {
        if (Directory.Exists(a_path))
        {
            Common.Directory.DirectoryUtils.Delete(a_path);
        }

        Directory.CreateDirectory(a_path);
    }
    #endregion

    private string m_recordingDirectory;
    private bool m_recordingDirectorySet;

    internal string RecordingDirectory
    {
        get
        {
            if (!m_recordingDirectorySet)
            {
                throw new Exception("RecordingDirectory was called by SetRecordingDirectoryPath() had not been called yet.");
            }

            return m_recordingDirectory;
        }
    }

    internal void SetRecordingDirectoryPath(DateTime a_startTime)
    {
        m_recordingDirectory = MakeRecordingDirectoryPathString(a_startTime);
        m_recordingDirectorySet = true;
    }

    internal string MakeRecordingDirectoryPathString(DateTime a_startTime)
    {
        string temp = $"{a_startTime.Year}.{a_startTime.Month:D2}.{a_startTime.Day:D2}T{a_startTime.Hour:D2}.{a_startTime.Minute:D2}.{a_startTime.Second:D2}.{a_startTime.Millisecond:D3}";
        temp = Path.Combine(RecordingRootFolder, temp);
        return temp;
    }

    public string GetRecordingFilePath(string a_path, long a_recordNumber, string a_className)
    {
        string fileName = GetRecordingFileName(a_recordNumber, a_className);
        return Path.Combine(a_path, fileName);
    }

    public static readonly string TransmissionNumberFormatStringMakePrivateOrInternal = "D10";

    public string GetRecordingFileName(long a_recordNumber, string a_className)
    {
        return $"{a_recordNumber.ToString(TransmissionNumberFormatStringMakePrivateOrInternal)}.{a_className}.bin";
    }

    internal string GetRecordingFilePath(long a_recordNumber, string a_className)
    {
        return GetRecordingFilePath(RecordingDirectory, a_recordNumber, a_className);
    }

    /// <summary>
    /// This is the name of the service log. It is written into the folder where the service is run from.
    /// </summary>
    internal string APSServiceLog => Path.Combine(LogFolder, "Service.log");

    /// <summary>
    /// This is the name of the service error log. It is written to the place where the service is run from.
    /// </summary>
    internal string APSServiceErrorLog => Path.Combine(LogFolder, "ServiceError.log");
}