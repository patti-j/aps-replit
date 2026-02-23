using PT.Common.Debugging;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerAPIProxy;

/// <summary>
/// Tools for getting the instance settings database connections string at startup.
/// Because that database is the source for other identifying instance data, we need to get the connection string elsewise.
/// </summary>
public static class StartupInstanceSettingsHelper
{
    /// <summary>
    /// Find the config file's location in the aps-core repo.
    /// </summary>
    /// <param name="a_instanceFullName"></param>
    /// <returns></returns>
    public static string GetPathToConfigFile(string a_instanceFullName)
    {
        string configDirPath = string.Empty;
        string configFolder = "SolutionMiscFiles";

#if DEBUG

        // This should likely be the standard path for any dev debug instance.
        // (We can't use standard ServicePath methods here, because this method is called before we would get the server directory from instance settings).
        string c_defaultConfigPath = "C:\\ProgramData\\PlanetTogether\\Debug 0.0\\System\\configuration.json";

        if (!File.Exists(c_defaultConfigPath))
        {
            DebugException.ThrowInDebug("Default config path is not valid. " +
                                        "Add a custom path to your Debug instance's config.json file (in System folder) to your debug args with the InstanceConfigPath key");
        }

        return c_defaultConfigPath;

#else

        // Find from current directory (PlanetTogether\Software\{Version}\ProgramFiles\System), moving relatively up and into the Instance's directory
        string instanceExePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        string PTRootFolder = Path.GetFullPath(Path.Combine(instanceExePath, "..\\..\\..\\..\\"));

        // Scaffold the DataFolder structure to get the path to the connection string file.
        Paths.InstanceDataFolder = PTRootFolder;
        ServicePaths servicePaths = new (a_instanceFullName, "_");
        configDirPath = servicePaths.SystemInstanceConnectionFilePath;
#endif

        return configDirPath;
    }

    private class InstanceConfigFile
    {
        public string InstanceDataConnectionString { get; set; }
    }
}