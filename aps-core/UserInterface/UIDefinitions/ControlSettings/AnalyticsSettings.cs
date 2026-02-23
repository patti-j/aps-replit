using PT.PackageDefinitions;

namespace PT.UIDefinitions.ControlSettings;

public class AnalyticsSettings : ISettingData
{
    #region IPTSerializable Members
    public AnalyticsSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 607)
        {
            a_reader.Read(out m_currentUrl);
            a_reader.Read(out m_cookieString);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_currentUrl);
        a_writer.Write(m_cookieString);
    }

    public int UniqueId => 0; //TODO
    #endregion

    private string m_currentUrl;
    private string m_cookieString;

    public AnalyticsSettings() { }

    public string CurrentUrl
    {
        get => m_currentUrl;
        set => m_currentUrl = value;
    }

    public string CookieString
    {
        get => m_cookieString;
        set => m_cookieString = value;
    }

    public string SettingKey => "AnalyticsSettings";
    public string SettingCaption => "Analytics Settings";
    public string Description => "Saves analytic view settings";
    public string SettingsGroup => "Dashboard";
    public string SettingsGroupCategory => "Analytics";
}