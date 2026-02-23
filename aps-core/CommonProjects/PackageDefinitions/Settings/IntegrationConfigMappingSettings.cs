using PT.APSCommon;
using PT.PackageDefinitions;

namespace PT.PackageDefinitions.Settings;

public class IntegrationConfigMappingSettings : ISettingData, ICloneable
{
    public const int c_NO_MAPPED_CONFIG_ID = -1;
    private int m_integrationConfigId = c_NO_MAPPED_CONFIG_ID;

    public int IntegrationConfigId
    {
        get
        {
            return m_integrationConfigId;
        }
        set
        {
            m_integrationConfigId = value;
        }
    }

    public IntegrationConfigMappingSettings(IReader a_reader)
    {
            a_reader.Read(out int configId);
            m_integrationConfigId = configId;
    }

    public IntegrationConfigMappingSettings()
    {

    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_integrationConfigId);
    }

    public int UniqueId => 1109;
    public string SettingKey => "scenarioSetting_IntegrationConfigPerScenarioMappingSettings";
    public string SettingCaption => "Integration Configuration per scenario mapping";
    public string Description => "Maps Integration Configurations to scenarios";
    public string SettingsGroup => SettingGroupConstants.ScenariosGroup;
    public string SettingsGroupCategory => SettingGroupConstants.ScenarioPlanningSettings;


object ICloneable.Clone()
    {
        return Clone();
    }

    public IntegrationConfigMappingSettings Clone()
    {
        return (IntegrationConfigMappingSettings)MemberwiseClone();
    }
}