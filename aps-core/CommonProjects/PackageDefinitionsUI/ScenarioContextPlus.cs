using System.Drawing;

using PT.APSCommon;
using PT.PackageDefinitions;
using PT.PackageDefinitions.Settings;
using PT.PackageDefinitionsUI.Interfaces;
using PT.Scheduler;
using PT.SchedulerData;
using PT.Transmissions.Interfaces;

namespace PT.PackageDefinitionsUI;

public class ScenarioContextPlus : ScenarioContext
{
    private readonly IDynamicSkin m_theme;
    public event Action<BaseId, bool> ComparableScenariosListModified;
    public ScenarioPlanningSettings PlanningSettings;
    public IntegrationConfigMappingSettings IntegrationConfigMappingSettings;

    public ScenarioContextPlus(IUsersInfo a_usersInfo, IClientSession a_clientSession, Scenario a_s, bool a_isReadOnly, string a_scenarioName, IDynamicSkin a_theme, IImpactAnalyzer a_impactAnalyzer) : base(a_usersInfo, a_clientSession, a_s, a_isReadOnly, a_scenarioName, a_impactAnalyzer)
    {
        m_theme = a_theme;
        PlanningSettings = new ScenarioPlanningSettings();
        IntegrationConfigMappingSettings = new IntegrationConfigMappingSettings();
    }

    public override void InitializeSettingsListeners(ISettingsManager a_settingsManager)
    {
        PlanningSettings = a_settingsManager.LoadSetting(PlanningSettings);
        if (PlanningSettings.ScenarioColor == Color.Empty)
        {
            PlanningSettings.ScenarioColor = m_theme.PTBlue;
        }

        IntegrationConfigMappingSettings = a_settingsManager.LoadSetting(IntegrationConfigMappingSettings);

        base.InitializeSettingsListeners(a_settingsManager);
    }

    /// <summary>
    /// Handle settings changes that need to be tracked on the context. The base method will be called at the end, propagating the event down.
    /// This allows this context object to be updated before any UI listeners receive the event, in case they need to access updated context data.
    /// </summary>
    /// <param name="a_settingsManager"></param>
    /// <param name="a_settingKey"></param>
    protected override async void ScenarioSettingsOnSettingSavedEvent(ISettingsManager a_settingsManager, string a_settingKey)
    {
        if (a_settingKey == PlanningSettings.SettingKey)
        {
            bool wasCompareScenario = PlanningSettings.CompareScenario;
            ScenarioPlanningSettings cachedPlanningSettings = PlanningSettings;
            using (BackgroundLock asyncLock = new(m_scenarioId))
            {
                await asyncLock.RunLockCodeBackground(ReloadScenarioPlanningSettingsPermissions);
                if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled)
                {
                    return; //PlanningSettings possibly not loaded
                }
            }

            if (wasCompareScenario && !cachedPlanningSettings.Equals(PlanningSettings))
            {
                // Also check to make sure the scenario was a compare scenario instead
                // of just relying on the setting's Equals function because we don't want to 
                // fire this event when the ScenarioPlanningSettings of a non-compare 
                // scenario is changed
                ComparableScenariosListModified?.Invoke(ScenarioId, PlanningSettings.CompareScenario);
            }
            else if (!wasCompareScenario && PlanningSettings.CompareScenario) /* Conditions are split into 2 if blocks for readability */
            {
                //A previously non-compare scenario became a compare scenario
                ComparableScenariosListModified?.Invoke(ScenarioId, PlanningSettings.CompareScenario);
            }
        }

        if (a_settingKey == IntegrationConfigMappingSettings.SettingKey)
        {
            using (BackgroundLock asyncLock = new(m_scenarioId))
            {
                await asyncLock.RunLockCodeBackground(ReloadScenarioIntegrationConfigSettingsPermissions);
                if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled)
                {
                    return; //PlanningSettings possibly not loaded
                }
            }
        }

        // Event is propagated in base class.
        base.ScenarioSettingsOnSettingSavedEvent(a_settingsManager, a_settingKey);
    }

    private void ReloadScenarioPlanningSettingsPermissions(ScenarioSummary a_ss, params object[] a_params)
    {
        PlanningSettings = a_ss.ScenarioSettings.LoadSetting(PlanningSettings);
    }

    private void ReloadScenarioIntegrationConfigSettingsPermissions(ScenarioSummary a_ss, params object[] a_params)
    {
        IntegrationConfigMappingSettings = a_ss.ScenarioSettings.LoadSetting(IntegrationConfigMappingSettings);
    }

    public Color ScenarioColor => PlanningSettings.ScenarioColor;
}