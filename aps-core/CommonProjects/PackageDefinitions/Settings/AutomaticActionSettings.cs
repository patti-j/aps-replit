namespace PT.PackageDefinitions.Settings;

public class BaseAutomaticActionSettings
{
    public BaseAutomaticActionSettings(bool a_enabled, string a_cronSchedule1, string a_cronSchedule2)
    {
        Enabled = a_enabled;
        m_cronSchedule1 = a_cronSchedule1;
        m_cronSchedule2 = a_cronSchedule2;
    }

    public BaseAutomaticActionSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12203)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_cronSchedule1);
            a_reader.Read(out m_cronSchedule2);
        }
        else
        {
            //Read for data only, no backwards compatibility is preserved
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out TimeSpan m_frequency);
            a_reader.Read(out string timesList);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(m_cronSchedule1);
        a_writer.Write(m_cronSchedule2);
    }

    private BoolVector32 m_bools;
    private const int c_enabledIdx = 0;

    public bool Enabled
    {
        get => m_bools[c_enabledIdx];
        set => m_bools[c_enabledIdx] = value;
    }

    private string m_cronSchedule1;
    private string m_cronSchedule2;

    public string CronSchedule
    {
        get => m_cronSchedule1;
        set => m_cronSchedule1 = value;
    }

    public string CronSchedule2
    {
        get => m_cronSchedule2;
        set => m_cronSchedule2 = value;
    }

    public static string SettingKeyPrefix => "settings_AutomaticActions";
}