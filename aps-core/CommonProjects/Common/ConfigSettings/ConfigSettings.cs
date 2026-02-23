using System.Collections;

namespace PT.Common.ConfigSettings;

/// <summary>
/// ConfigSettings related utility functions.
/// </summary>
public class ConfigSettings
{
    /// <summary>
    /// Retrieve an int configuration setting from a Hashtable obtained by ConfigurationSettings.GetConfig. This function doesn't throw any exceptions.
    /// </summary>
    /// <param name="defaultValue">The default value of the setting. This value is used in the case an exception occurs.</param>
    /// <param name="valueName">The name of the configuration setting.</param>
    /// <param name="debugConfiguration">The Hashtable created by ConfigurationSettings.GetConfig.</param>
    public static int GetIntConfigurationSetting(int defaultValue, string valueName, Hashtable debugConfiguration)
    {
        int outValue = defaultValue;

        try
        {
            string configurationValue = (string)debugConfiguration[valueName];

            if (configurationValue != null)
            {
                int temp = int.Parse(configurationValue);
                outValue = temp;
            }
        }
        catch
        {
            // This function doesn't throw any exceptions.
        }

        return outValue;
    }

    public static int GetIntConfigurationSetting(string valueName, Hashtable configuration)
    {
        string configurationValue = (string)configuration[valueName];
        int v = int.Parse(configurationValue);
        return v;
    }

    /// <summary>
    /// Retrieve an int configuration setting from a Hashtable obtained by ConfigurationSettings.GetConfig. This function doesn't throw any exceptions.
    /// </summary>
    /// <param name="minValue">The default value of the setting. This value is used in the case an exception occurs. Or the minimum value if the configuration setting is too small.</param>
    /// <param name="valueName">The name of the configuration setting.</param>
    /// <param name="debugConfiguration">The Hashtable created by ConfigurationSettings.GetConfig.</param>
    public static int GetMinIntConfigurationSetting(int minValue, string valueName, Hashtable debugConfiguration)
    {
        int outValue = GetIntConfigurationSetting(minValue, valueName, debugConfiguration);

        if (outValue < minValue)
        {
            outValue = minValue;
        }

        return outValue;
    }

    /// <summary>
    /// Retrieve an bool configuration setting from a Hashtable obtained by ConfigurationSettings.GetConfig. This function doesn't throw any exceptions.
    /// </summary>
    /// <param name="defaultValue">The default value of the setting. This value is used in the case an exception occurs.</param>
    /// <param name="valueName">The name of the configuration setting.</param>
    /// <param name="debugConfiguration">The Hashtable created by ConfigurationSettings.GetConfig.</param>
    public static bool GetBoolConfigurationSetting(bool defaultValue, string valueName, Hashtable debugConfiguration)
    {
        bool outValue = defaultValue;

        try
        {
            string configurationValue = (string)debugConfiguration[valueName];

            if (configurationValue != null)
            {
                bool temp = bool.Parse(configurationValue);
                outValue = temp;
            }
        }
        catch
        {
            // This function doesn't throw any exceptions.
        }

        return outValue;
    }
}