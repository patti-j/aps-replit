using PT.PackageDefinitions;

namespace PT.UIDefinitions.ControlSettings;

public class ScheduleReportSettings : ISettingData
{
    #region IPTSerializable Members
    public ScheduleReportSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_reportDuration);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(m_reportDuration);
    }

    public int UniqueId => 0; //TODO
    #endregion

    private BoolVector32 m_bools;
    private const short c_refreshAutomaticallyIdx = 0;
    private const short c_showMaterialRequirementsIdx = 1;

    public ScheduleReportSettings()
    {
        ShowMaterialRequirements = false;
        RefreshAutomatically = true;
        ReportDuration = TimeSpan.FromDays(1);
    }

    public bool RefreshAutomatically
    {
        get => m_bools[c_refreshAutomaticallyIdx];
        set => m_bools[c_refreshAutomaticallyIdx] = value;
    }

    public bool ShowMaterialRequirements
    {
        get => m_bools[c_showMaterialRequirementsIdx];
        set => m_bools[c_showMaterialRequirementsIdx] = value;
    }

    private long m_reportDuration;

    public TimeSpan ReportDuration
    {
        get => new (m_reportDuration);
        set => m_reportDuration = value.Ticks;
    }

    public string SettingKey => "ScheduleReportSettings";
    public string SettingCaption => "Schedule Report Settings";
    public string Description => ""; //TODO: Description
    public string SettingsGroup => "Reports";
    public string SettingsGroupCategory => "Reports";
}