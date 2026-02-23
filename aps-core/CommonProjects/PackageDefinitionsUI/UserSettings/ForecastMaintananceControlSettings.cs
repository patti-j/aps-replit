using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI.UserSettings;

public class ForecastMaintananceControlSettings : ISettingData
{
    #region IPTSerializable Members
    public ForecastMaintananceControlSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_bools = new BoolVector32(a_reader);
        }
    }

    public string SettingKey => "ForecastSettings";
    public string Description => "Forecast Settings";
    public string SettingsGroup => SettingGroupConstants.BoardsSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.InventoryPlanSettings;
    public string SettingCaption => "Forecast settings";
    public int UniqueId => 927;

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
    }
    #endregion

    private BoolVector32 m_bools;
    private const short c_includeForecastIdx = 0;

    public ForecastMaintananceControlSettings()
    {
        m_bools = new BoolVector32();
    }

    //TODO: implement and save/load setting when option added to 
    public bool IncludeNetInventory
    {
        get => m_bools[c_includeForecastIdx];
        set => m_bools[c_includeForecastIdx] = value;
    }
}