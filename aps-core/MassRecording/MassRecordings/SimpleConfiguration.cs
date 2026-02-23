using System.Xml;

namespace MassRecordings;

public class SimpleConfiguration
{
    /// <summary>
    /// Stores a dictionary of the MR config file. Load values by key.
    /// </summary>
    public SimpleConfiguration()
    {
        string appMR = Resources.App_MR;
        LoadConfiguration(appMR);
        m_configName = "";
    }

    private readonly Dictionary<string, string> m_configDictionary = new ();
    private string m_configName;

    /// <summary>
    /// Sets all of the configuration values for user.
    /// </summary>
    private void LoadConfiguration(string a_xml)
    {
        XmlDocument document = new ();
        document.LoadXml(a_xml);

        //Load in the settings
        XmlNodeList nodes = document.SelectNodes("configuration/configSections");
        foreach (XmlNode node in nodes)
        {
            SetConfigName(node);
        }

        if (!string.IsNullOrEmpty(m_configName))
        {
            string configFileStructure = $"configuration/{m_configName}/add";
            XmlNodeList configNodes = document.SelectNodes(configFileStructure);
            foreach (XmlNode node in configNodes)
            {
                m_configDictionary.Add(node.Attributes["key"].Value, node.Attributes["value"].Value);
            }
        }
    }

    /// <summary>
    /// Sets configuration name to user entry in App.MR.config file when matches machine name.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool SetConfigName(XmlNode node)
    {
        node = node.NextSibling;

        if (node == null)
        {
            return false;
        }

        while (node != null)

        {
            string test = node.Name;

            if (Environment.MachineName.Equals(node.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                m_configName = node.Name;
                return true;
            }

            node = node.NextSibling;
        }

        return false;
    }

    /// <summary>
    /// Load configuration value by key
    /// </summary>
    public string LoadValue(string a_key)
    {
        if (m_configDictionary.TryGetValue(a_key, out string keyValue))
        {
            return keyValue;
        }

        return string.Empty;
    }
}