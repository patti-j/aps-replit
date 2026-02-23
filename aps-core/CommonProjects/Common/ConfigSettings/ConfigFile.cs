namespace PT.Common.File;

/// <summary>
/// Helper functionality for configuration files.
/// </summary>
public class ConfigFile
{
    private static string configFileFullPath;

    /// <summary>
    /// Get the full path of the configuration file.
    /// </summary>
    public static string ConfigFileFullPath
    {
        get
        {
            if (configFileFullPath == null)
            {
                string[] args = Environment.GetCommandLineArgs();
                configFileFullPath = string.Format("{0}.config", args[0]);
            }

            return configFileFullPath;
        }
    }

    private static string configFileName;

    /// <summary>
    /// Get the name of the configuration file without any path information.
    /// </summary>
    public static string ConfigFileName
    {
        get
        {
            if (configFileName == null)
            {
                string configFileFullPath = ConfigFileFullPath;
                int lastPathSeparatorIndex = configFileFullPath.LastIndexOf(Path.DirectorySeparatorChar);
                configFileName = configFileFullPath.Substring(lastPathSeparatorIndex);
            }

            return configFileName;
        }
    }

    /// <summary>
    /// Change the key and values of a configuration file. The algorith works by searching for a line with "key" and "value" on it and replaces what's in between
    /// the double quotes for with newValue.
    /// </summary>
    /// <param name="configFile">The configuration file loaded into a TextFile.</param>
    /// <param name="key">The key whose value you want to change.</param>
    /// <param name="newValue">The new value.</param>
    public static void SetConfigFileValue(TextFile configFile, string key, string newValue)
    {
        for (int lineI = 0; lineI < configFile.Count; ++lineI)
        {
            string line = configFile[lineI];
            if (Text.TextUtil.IndexOfCaseInsensitive(line, "key") >= 0)
            {
                int keyI = Text.TextUtil.IndexOfCaseInsensitive(line, key);

                if (keyI >= 0)
                {
                    int valueI = Text.TextUtil.IndexOfCaseInsensitive(line, "value");

                    if (valueI >= 0)
                    {
                        int startIndex = line.IndexOf("\"", valueI);
                        if (startIndex >= 0)
                        {
                            int endIndex = line.IndexOf("\"", startIndex + 1);
                            if (endIndex >= 0)
                            {
                                string sub1 = line.Substring(0, startIndex + 1);
                                string sub2 = line.Substring(endIndex);
                                System.Text.StringBuilder sb = new ();
                                sb.Append(sub1);
                                sb.Append(newValue);
                                sb.Append(sub2);
                                line = sb.ToString();
                                configFile[lineI] = line;
                            }
                        }
                    }
                }
            }
        }
    }
}