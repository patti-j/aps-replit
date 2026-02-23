using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI.UserSettings;

public class ActivityReportingSettings : ISettingData, ICloneable
{
    #region IPTSerializable Members
    public ActivityReportingSettings(IReader a_reader)
    {
        m_bools = new BoolVector32(a_reader);
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
    }
    #endregion

    private BoolVector32 m_bools;
    private const short c_allocateMaterialFromOnHandIdx = 0;
    private const short c_releaseProductToWarehouseIdx = 1;

    public ActivityReportingSettings()
    {
        AllocateMaterialFromOnHand = true;
        ReleaseProductToWarehouse = true;
    }

    public bool AllocateMaterialFromOnHand
    {
        get => m_bools[c_allocateMaterialFromOnHandIdx];
        set => m_bools[c_allocateMaterialFromOnHandIdx] = value;
    }

    public bool ReleaseProductToWarehouse
    {
        get => m_bools[c_releaseProductToWarehouseIdx];
        set => m_bools[c_releaseProductToWarehouseIdx] = value;
    }

    public int UniqueId => 1037;
    public string SettingKey => "reportingSettings_Activity";
    public string Description => "Whether to auto issue material or produce to inventory when reporting quantities.";
    public string SettingsGroup => SettingGroupConstants.ReportingSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.ActivityReportingSettings;
    public string SettingCaption => "Activity reporting settings";

    object ICloneable.Clone()
    {
        return Clone();
    }

    public ActivityReportingSettings Clone()
    {
        return (ActivityReportingSettings)MemberwiseClone();
    }
}