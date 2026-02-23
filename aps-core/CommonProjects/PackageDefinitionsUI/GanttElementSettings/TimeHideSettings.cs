using PT.PackageDefinitions;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class SimplifyGanttTimeHidingSettings : ISettingData, ICloneable
{
    public const int UNIQUE_ID = 908;
    public string SettingKey => "SimplifyGanttSettings_TimeHiding";
    public string Description => "TODO:"; //TODO: implement this
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SimplifyGanttSettings;
    public string SettingCaption => "Time hiding settings";

    private BoolVector32 m_flags;

    #region IPTSerializable Members
    public SimplifyGanttTimeHidingSettings(IReader reader)
    {
        m_flags = new BoolVector32(reader);
    }

    public SimplifyGanttTimeHidingSettings() { }

    public SimplifyGanttTimeHidingSettings(ScheduleViewerSettings.SimplifyGanttSettings a_settings)
    {
        m_flags[0] = a_settings.HideMondays;
        m_flags[1] = a_settings.HideTuesdays;
        m_flags[2] = a_settings.HideWednesdays;
        m_flags[3] = a_settings.HideThursdays;
        m_flags[4] = a_settings.HideFridays;
        m_flags[5] = a_settings.HideSaturdays;
        m_flags[6] = a_settings.HideSundays;
        m_flags[7] = a_settings.HideHour0;
        m_flags[8] = a_settings.HideHour1;
        m_flags[9] = a_settings.HideHour2;
        m_flags[10] = a_settings.HideHour3;
        m_flags[11] = a_settings.HideHour4;
        m_flags[12] = a_settings.HideHour5;
        m_flags[13] = a_settings.HideHour6;
        m_flags[14] = a_settings.HideHour7;
        m_flags[15] = a_settings.HideHour8;
        m_flags[16] = a_settings.HideHour9;
        m_flags[17] = a_settings.HideHour10;
        m_flags[18] = a_settings.HideHour11;
        m_flags[19] = a_settings.HideHour12;
        m_flags[20] = a_settings.HideHour13;
        m_flags[21] = a_settings.HideHour14;
        m_flags[22] = a_settings.HideHour15;
        m_flags[23] = a_settings.HideHour16;
        m_flags[24] = a_settings.HideHour17;
        m_flags[25] = a_settings.HideHour18;
        m_flags[26] = a_settings.HideHour19;
        m_flags[27] = a_settings.HideHour20;
        m_flags[28] = a_settings.HideHour21;
        m_flags[29] = a_settings.HideHour22;
        m_flags[30] = a_settings.HideHour23;
    }

    public void Serialize(IWriter writer)
    {
        m_flags.Serialize(writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region Time Hiding Properties
    private const int HideMondaysIdx = 0;

    public bool HideMondays
    {
        get => m_flags[HideMondaysIdx];
        set => m_flags[HideMondaysIdx] = value;
    }

    private const int HideTuesdaysIdx = 1;

    public bool HideTuesdays
    {
        get => m_flags[HideTuesdaysIdx];
        set => m_flags[HideTuesdaysIdx] = value;
    }

    private const int HideWednesdaysIdx = 2;

    public bool HideWednesdays
    {
        get => m_flags[HideWednesdaysIdx];
        set => m_flags[HideWednesdaysIdx] = value;
    }

    private const int HideThursdaysIdx = 3;

    public bool HideThursdays
    {
        get => m_flags[HideThursdaysIdx];
        set => m_flags[HideThursdaysIdx] = value;
    }

    private const int HideFridaysIdx = 4;

    public bool HideFridays
    {
        get => m_flags[HideFridaysIdx];
        set => m_flags[HideFridaysIdx] = value;
    }

    private const int HideSaturdaysIdx = 5;

    public bool HideSaturdays
    {
        get => m_flags[HideSaturdaysIdx];
        set => m_flags[HideSaturdaysIdx] = value;
    }

    private const int HideSundaysIdx = 6;

    public bool HideSundays
    {
        get => m_flags[HideSundaysIdx];
        set => m_flags[HideSundaysIdx] = value;
    }

    private const int HideHourIdx0 = 7;

    public bool HideHour0
    {
        get => m_flags[HideHourIdx0];
        set => m_flags[HideHourIdx0] = value;
    }

    private const int HideHour1Idx = 8;

    public bool HideHour1
    {
        get => m_flags[HideHour1Idx];
        set => m_flags[HideHour1Idx] = value;
    }

    private const int HideHour2Idx = 9;

    public bool HideHour2
    {
        get => m_flags[HideHour2Idx];
        set => m_flags[HideHour2Idx] = value;
    }

    private const int HideHour3Idx = 10;

    public bool HideHour3
    {
        get => m_flags[HideHour3Idx];
        set => m_flags[HideHour3Idx] = value;
    }

    private const int HideHour4Idx = 11;

    public bool HideHour4
    {
        get => m_flags[HideHour4Idx];
        set => m_flags[HideHour4Idx] = value;
    }

    private const int HideHour5Idx = 12;

    public bool HideHour5
    {
        get => m_flags[HideHour5Idx];
        set => m_flags[HideHour5Idx] = value;
    }

    private const int HideHour6Idx = 13;

    public bool HideHour6
    {
        get => m_flags[HideHour6Idx];
        set => m_flags[HideHour6Idx] = value;
    }

    private const int HideHour7Idx = 14;

    public bool HideHour7
    {
        get => m_flags[HideHour7Idx];
        set => m_flags[HideHour7Idx] = value;
    }

    private const int HideHour8Idx = 15;

    public bool HideHour8
    {
        get => m_flags[HideHour8Idx];
        set => m_flags[HideHour8Idx] = value;
    }

    private const int HideHour9Idx = 16;

    public bool HideHour9
    {
        get => m_flags[HideHour9Idx];
        set => m_flags[HideHour9Idx] = value;
    }

    private const int HideHour10Idx = 17;

    public bool HideHour10
    {
        get => m_flags[HideHour10Idx];
        set => m_flags[HideHour10Idx] = value;
    }

    private const int HideHour11Idx = 18;

    public bool HideHour11
    {
        get => m_flags[HideHour11Idx];
        set => m_flags[HideHour11Idx] = value;
    }

    private const int HideHour12Idx = 19;

    public bool HideHour12
    {
        get => m_flags[HideHour12Idx];
        set => m_flags[HideHour12Idx] = value;
    }

    private const int HideHour13Idx = 20;

    public bool HideHour13
    {
        get => m_flags[HideHour13Idx];
        set => m_flags[HideHour13Idx] = value;
    }

    private const int HideHour14Idx = 21;

    public bool HideHour14
    {
        get => m_flags[HideHour14Idx];
        set => m_flags[HideHour14Idx] = value;
    }

    private const int HideHour15Idx = 22;

    public bool HideHour15
    {
        get => m_flags[HideHour15Idx];
        set => m_flags[HideHour15Idx] = value;
    }

    private const int HideHour16Idx = 23;

    public bool HideHour16
    {
        get => m_flags[HideHour16Idx];
        set => m_flags[HideHour16Idx] = value;
    }

    private const int HideHour17Idx = 24;

    public bool HideHour17
    {
        get => m_flags[HideHour17Idx];
        set => m_flags[HideHour17Idx] = value;
    }

    private const int HideHour18Idx = 25;

    public bool HideHour18
    {
        get => m_flags[HideHour18Idx];
        set => m_flags[HideHour18Idx] = value;
    }

    private const int HideHour19Idx = 26;

    public bool HideHour19
    {
        get => m_flags[HideHour19Idx];
        set => m_flags[HideHour19Idx] = value;
    }

    private const int HideHour20Idx = 27;

    public bool HideHour20
    {
        get => m_flags[HideHour20Idx];
        set => m_flags[HideHour20Idx] = value;
    }

    private const int HideHour21Idx = 28;

    public bool HideHour21
    {
        get => m_flags[HideHour21Idx];
        set => m_flags[HideHour21Idx] = value;
    }

    private const int HideHour22Idx = 29;

    public bool HideHour22
    {
        get => m_flags[HideHour22Idx];
        set => m_flags[HideHour22Idx] = value;
    }

    private const int HideHour23Idx = 30;

    public bool HideHour23
    {
        get => m_flags[HideHour23Idx];
        set => m_flags[HideHour23Idx] = value;
    }
    #endregion

    object ICloneable.Clone()
    {
        return Clone();
    }

    public SimplifyGanttTimeHidingSettings Clone()
    {
        return (SimplifyGanttTimeHidingSettings)MemberwiseClone();
    }
}