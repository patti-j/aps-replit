
namespace PT.PackageDefinitions.Settings
{
    public class PTUserScenarioStartupPreferences : ICloneable, ISettingData
    {
        private EScenarioStartupPreferenceType m_scenarioStartupPreferenceType;

        public EScenarioStartupPreferenceType ScenarioStartupPreferenceType
        {
            get => m_scenarioStartupPreferenceType;
            set => m_scenarioStartupPreferenceType = value;
        }

        public PTUserScenarioStartupPreferences()
        {
            m_scenarioStartupPreferenceType = EScenarioStartupPreferenceType.LoadLastSession;
        }

        public PTUserScenarioStartupPreferences(IReader a_reader)
        {
            a_reader.Read(out int temp);
            m_scenarioStartupPreferenceType = (EScenarioStartupPreferenceType)temp;
        }

        public object Clone()
        {
            return new PTUserScenarioStartupPreferences
            {
                ScenarioStartupPreferenceType = this.ScenarioStartupPreferenceType
            };
        }

        public void Serialize(IWriter a_writer)
        {
            a_writer.Write((int)m_scenarioStartupPreferenceType);
        }

        public int UniqueId => 850;
        public string SettingKey => "UserPreferences_PTScenarioPreferences";
        public string SettingCaption => "Scenario Preferences";
        public string Description => "Scenario Preferences Settings";
        public string SettingsGroup => SettingGroupConstants.SystemSettingsGroup;
        public string SettingsGroupCategory => SettingGroupConstants.SystemSettingsUserOptions;

        // Scenario permission supersedes any of these ScenarioStartupPreferenceType.
        // For example, even if a user has their startup preference set to LoadLastActiveScenario,
        // if they've lost permission to their last active Scenario, then it won't be loaded
        public enum EScenarioStartupPreferenceType
        {
            LoadLastSession,
            LoadProductionScenario, 
            LoadLastActiveScenario,
            LoadAllScenarios,
        }
    }
}
