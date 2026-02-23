using PT.PackageDefinitions;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class SimplifyGanttSpeedUpSettings : ISettingData, ICloneable
{
    public const int UNIQUE_ID = 907;
    public string SettingKey => "SimplifyGanttSettings_SpeedUps";
    public string Description => "TODO:"; //TODO: implement this
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SimplifyGanttSettings;
    public string SettingCaption => "Speed-up settings";

    #region IPTSerializable Members
    public SimplifyGanttSpeedUpSettings(IReader a_reader)
    {
        a_reader.Read(out m_hideActivities);
        a_reader.Read(out m_hideActivitiesAfter);
        a_reader.Read(out m_hideCapacityIntervals);
        a_reader.Read(out m_hideCapacityIntervalsAfter);
    }

    public SimplifyGanttSpeedUpSettings() { }

    public SimplifyGanttSpeedUpSettings(ScheduleViewerSettings.SimplifyGanttSettings a_settings)
    {
        m_hideActivities = a_settings.HideActivities;
        m_hideActivitiesAfter = a_settings.HideActivitiesAfter;
        m_hideCapacityIntervals = a_settings.HideCapacityIntervals;
        m_hideCapacityIntervalsAfter = a_settings.HideCapacityIntervalsAfter;
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_hideActivities);
        a_writer.Write(m_hideActivitiesAfter);
        a_writer.Write(m_hideCapacityIntervals);
        a_writer.Write(m_hideCapacityIntervalsAfter);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region Speed-up Properties
    private bool m_hideCapacityIntervals;

    public bool HideCapacityIntervals
    {
        get => m_hideCapacityIntervals;
        set => m_hideCapacityIntervals = value;
    }

    private TimeSpan m_hideCapacityIntervalsAfter = new (0);

    public TimeSpan HideCapacityIntervalsAfter
    {
        get => m_hideCapacityIntervalsAfter;
        set => m_hideCapacityIntervalsAfter = value;
    }

    private bool m_hideActivities;

    public bool HideActivities
    {
        get => m_hideActivities;
        set => m_hideActivities = value;
    }

    private TimeSpan m_hideActivitiesAfter = new (28, 0, 0, 0, 0);

    public TimeSpan HideActivitiesAfter
    {
        get => m_hideActivitiesAfter;
        set => m_hideActivitiesAfter = value;
    }
    #endregion

    object ICloneable.Clone()
    {
        return Clone();
    }

    public SimplifyGanttSpeedUpSettings Clone()
    {
        return (SimplifyGanttSpeedUpSettings)MemberwiseClone();
    }
}