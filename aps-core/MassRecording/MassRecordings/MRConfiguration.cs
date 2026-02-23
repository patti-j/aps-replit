using System.Data;

using PT.Common.SqlServer;

namespace MassRecordings;

public class MRMissingConfigException : Exception
{
    public MRMissingConfigException(string a_parameter) : base("Missing Configuration: '" + a_parameter + "'. Unable to continue.  Check the value in App.MR.Config in the MassRecordings project for reference.") { }
}

public class MRUnknownValueException : Exception
{
    public MRUnknownValueException(string a_parameter, string a_value)
        : base("Unrecognized value for : " + a_parameter + ". Value: '" + a_value + "'. Unable to continue.") { }
}

public class MRConfigValidationException : Exception
{
    public MRConfigValidationException(string a_parameter) : base(a_parameter) { }
}

/// <summary>
/// This class loads the settings from the App.Mr config files and stores the values.
/// </summary>
public class MRConfiguration
{
    public MRConfiguration(DatabaseConnections a_dbConnector, long a_sessionId, string a_recordingPath)
    {
        LoadConfiguration(a_dbConnector, a_sessionId, a_recordingPath);
    }

    public enum ERunModes { Automatic, Manual }

    #region Values
    private string RecordingsDirectory;
    public ERunModes RunMode = ERunModes.Manual; //set to manual incase there is an error and the console needs to be shown.
    public string MasterCopy;
    public long BaseSessionId;

    //Player Values
    public string LoadCustomization;
    public string RunModeString;
    public string RecordingDirectoryToLoadFromAtStartUp;
    public string KeyFolderPath;

    //Exclusion Lists
    internal List<string> ExclusionList = new ();
    public readonly DateTime StartTime = DateTime.Now;
    #endregion

    /// <summary>
    /// Load data from a data table. Throws missing exception if value doesn't exist.
    /// </summary>
    /// <param name="a_value"></param>
    /// <param name="a_dt"></param>
    /// <returns>DataTable Object</returns>
    public object LoadValue(DataTable a_dt, string a_value)
    {
        try
        {
            return a_dt.Rows[0][a_value];
        }
        catch (KeyNotFoundException)
        {
            throw new MRMissingConfigException(a_value);
        }
    }

    /// <summary>
    /// Sets all of the configuration values.
    /// </summary>
    /// <param name="a_dbConnector"> Connection to the Sql database</param>
    /// <param name="a_sessionId"> Current sessionId</param>
    /// <param name="a_recordingPath"> (customer path + name of recording) to append to local mass recording path</param>
    /// <returns></returns>
    public void LoadConfiguration(DatabaseConnections a_dbConnector, long a_sessionId, string a_recordingPath)
    {
        SqlStrings.TableDefinitions.HostConfigurations hostConfigdt = new ();

        DataTable dt = a_dbConnector.SelectSQLTable(SqlStrings.GetHostConfigTable(Environment.MachineName));
        RunModeString = Convert.ToString(LoadValue(dt, hostConfigdt.RunMode));
        RecordingsDirectory = Convert.ToString(LoadValue(dt, hostConfigdt.RecordingsDirectory));
        BaseSessionId = Convert.ToInt32(LoadValue(dt, hostConfigdt.BaseSessionId));
        KeyFolderPath = Convert.ToString(LoadValue(dt, hostConfigdt.KeyFolderPath));
        MasterCopy = Convert.ToString(LoadValue(dt, hostConfigdt.MasterCopyPath));
        RecordingDirectoryToLoadFromAtStartUp = ProcessDirectory(Path.Combine(RecordingsDirectory, a_recordingPath));

        if (string.IsNullOrEmpty(RecordingDirectoryToLoadFromAtStartUp))
        {
            throw new Exception("No scenario file found.  " + Path.Combine(RecordingsDirectory, a_recordingPath));
        }

        //Player Values
        LoadCustomization = Convert.ToString(LoadValue(dt, hostConfigdt.LoadCustomization));

        //Additional processing
        VerifyDirectory(RecordingsDirectory, hostConfigdt.RecordingsDirectory);
        ValidateAndSetRunMode(RunModeString);
    }

    public static string ProcessDirectory(string targetDirectory)
    {
        // Process the list of files found in the directory.
        string[] fileEntries = Directory.GetFiles(targetDirectory);
        foreach (string fileName in fileEntries)
        {
            if (fileName.ToUpper().EndsWith(".DAT"))
            {
                return targetDirectory;
            }
        }

        // Recurse into subdirectories of this directory.
        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach (string subdirectory in subdirectoryEntries)
        {
            string dir = ProcessDirectory(subdirectory);
            if (!string.IsNullOrEmpty(dir))
            {
                return dir;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Validates RunMode value and converts it to enum
    /// </summary>
    /// <param name="a_runModeString"></param>
    public void ValidateAndSetRunMode(string a_runModeString)
    {
        if (a_runModeString.ToLower() == "automatic")
        {
            RunMode = ERunModes.Automatic;
        }
        else if (a_runModeString.ToLower() == "manual")
        {
            RunMode = ERunModes.Manual;
        }
        else
        {
            throw new MRUnknownValueException("Runmode", a_runModeString);
        }
    }

    /// <summary>
    /// Set a bool to the value specified by the string read from the file. Throws unknown value exception if the string can't be parsed
    /// </summary>
    public void ValidateAndSetBoolean(ref bool a_valueToSet, string a_configString)
    {
        try
        {
            a_valueToSet = bool.Parse(a_configString);
        }
        catch
        {
            throw new MRUnknownValueException("Bool", a_configString);
        }
    }

    /// <summary>
    /// Verifies that a directory exists. Throws validation exception if it doesn't
    /// </summary>
    public static void VerifyDirectory(string a_directory, string a_parameter)
    {
        if (!File.Exists(a_directory))
        {
            if (!Directory.Exists(a_directory))
            {
                throw new MRConfigValidationException(a_parameter);
            }
        }
    }
}