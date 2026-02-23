using PT.PackageDefinitions;

namespace PT.UIDefinitions.ControlSettings;

public class VisualFactorySettings : ISettingData
{
    #region IPTSerializable Members
    private BoolVector32 m_bools;

    public VisualFactorySettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 636)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_duration);
        }
    }

    public VisualFactorySettings()
    {
        m_duration = MinDuration.Ticks;
    }

    private const int c_useInVisualFactoryIdx = 0;

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(m_duration);
    }

    public int UniqueId => 1026;
    #endregion

    private long m_duration;

    public TimeSpan Duration
    {
        get => new (m_duration);
        set => m_duration = Math.Max(value.Ticks, MinDuration.Ticks);
    }

    public bool UseInVisualFactory
    {
        get => m_bools[c_useInVisualFactoryIdx];
        set => m_bools[c_useInVisualFactoryIdx] = value;
    }

    public static readonly TimeSpan MinDuration = TimeSpan.FromSeconds(10);
    public string SettingKey => "VisualFactorySettings";
    public string SettingCaption => "TODO";
    public string Description => "TODO";
    public string SettingsGroup => "TODO";
    public string SettingsGroupCategory => "TODO";
}