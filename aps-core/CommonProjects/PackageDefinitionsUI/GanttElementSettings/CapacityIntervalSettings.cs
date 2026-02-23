using PT.PackageDefinitions;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class SimplifyGanttCapacityIntervalSettings : ISettingData, ICloneable
{
    public const int UNIQUE_ID = 909;

    public string SettingKey => "SimplifyGanttSettings_CapacityIntervals";
    public string Description => "TODO:"; //TODO: implement this
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SimplifyGanttSettings;
    public string SettingCaption => "Capacity interval settings";

    #region IPTSerializable Members
    public SimplifyGanttCapacityIntervalSettings(IReader reader)
    {
        m_flags = new BoolVector32(reader);
    }

    public SimplifyGanttCapacityIntervalSettings() { }

    public SimplifyGanttCapacityIntervalSettings(ScheduleViewerSettings.SimplifyGanttSettings a_settings)
    {
        m_flags[0] = a_settings.DragAndResizeOnlineGCIs;
        m_flags[1] = a_settings.DragAndResizeOfflineGCIs;
        m_flags[2] = a_settings.DeleteGCIs;
    }

    public void Serialize(IWriter writer)
    {
        m_flags.Serialize(writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region Bools
    private BoolVector32 m_flags;
    private const int c_canDragAndResizeOnlineIdx = 0;
    private const int c_canDragAndResizeOfflineIdx = 1;
    private const int c_canDeleteIdx = 2;
    #endregion Bools

    #region GCI Move/Delete Properties
    public bool DragAndResizeOnlineGCIs
    {
        get => m_flags[c_canDragAndResizeOnlineIdx];
        set => m_flags[c_canDragAndResizeOnlineIdx] = value;
    }
    public bool DragAndResizeOfflineGCIs
    {
        get => m_flags[c_canDragAndResizeOfflineIdx];
        set => m_flags[c_canDragAndResizeOfflineIdx] = value;
    }
    public bool DeleteGCIs
    {
        get => m_flags[c_canDeleteIdx];
        set => m_flags[c_canDeleteIdx] = value;
    }
    #endregion

    object ICloneable.Clone()
    {
        return Clone();
    }

    public SimplifyGanttCapacityIntervalSettings Clone()
    {
        return (SimplifyGanttCapacityIntervalSettings)MemberwiseClone();
    }
}