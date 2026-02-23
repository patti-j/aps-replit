using PT.PackageDefinitions;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class SimplifyGanttDaylightSavingsSettings : ISettingData, ICloneable
{
    public const int UNIQUE_ID = 910;

    public static string Key = "SimplifyGanttSettings_DaylightSavings";
    public string SettingKey => Key;
    public string Description => "TODO:"; //TODO: implement this
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SimplifyGanttSettings;
    public string SettingCaption => "Daylight Savings settings";

    #region IPTSerializable Members
    public SimplifyGanttDaylightSavingsSettings(IReader reader)
    {
        m_boolVector32 = new BoolVector32(reader);
    }

    public SimplifyGanttDaylightSavingsSettings()
    {
        ShowDaylightSavingsOnGantt = true;
    }


    public void Serialize(IWriter a_writer)
    {
        m_boolVector32.Serialize(a_writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    private const int c_daylightSavingIdx = 0;
    private BoolVector32 m_boolVector32;

    public bool ShowDaylightSavingsOnGantt
    {
        get => m_boolVector32[c_daylightSavingIdx];
        set => m_boolVector32[c_daylightSavingIdx] = value;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    public SimplifyGanttDaylightSavingsSettings Clone()
    {
        return (SimplifyGanttDaylightSavingsSettings)MemberwiseClone();
    }
}