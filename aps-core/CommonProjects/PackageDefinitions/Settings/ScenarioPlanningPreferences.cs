using PT.APSCommon;
using PT.PackageDefinitions;

namespace PT.PackageDefinitions.Settings;

public class ScenarioPlanningPreferences : ISettingData, ICloneable
{
    public const int UNIQUE_ID = 1001;

    public ScenarioPlanningPreferences()
    {
        LoadLastActiveScenario = true;
        LastActiveScenarioId = BaseId.NULL_ID;
    }

    public ScenarioPlanningPreferences(IReader a_reader)
    {
        m_bools = new BoolVector32(a_reader);
        LastActiveScenarioId = new BaseId(a_reader);
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        LastActiveScenarioId.Serialize(a_writer);
    }

    private BoolVector32 m_bools;
    private const int c_loadLastActiveScenarioIdx = 0;

    public bool LoadLastActiveScenario
    {
        get => m_bools[c_loadLastActiveScenarioIdx];
        set => m_bools[c_loadLastActiveScenarioIdx] = value;
    }

    private BaseId m_lastActiveScenarioId;

    public BaseId LastActiveScenarioId
    {
        get => m_lastActiveScenarioId;
        set => m_lastActiveScenarioId = value;
    }

    public int UniqueId => UNIQUE_ID;
    public string SettingKey => "ScenarioPreferences";
    public string Description => "Scenario Preferences";
    public string SettingsGroup => SettingGroupConstants.ScenariosGroup;
    public string SettingsGroupCategory => SettingGroupConstants.ScenarioPlanningPreferences;
    public string SettingCaption => "Planning preferences";

    public object Clone()
    {
        ScenarioPlanningPreferences sppClone = new();
        sppClone.LoadLastActiveScenario = LoadLastActiveScenario;

        return sppClone;
    }
}