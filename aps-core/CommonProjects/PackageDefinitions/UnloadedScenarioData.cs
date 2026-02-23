using PT.APSCommon;
using PT.PackageDefinitions.Settings;

namespace PT.PackageDefinitions;

public class UnloadedScenarioData : IPTSerializable
{
    private readonly BaseId m_scenarioId;
    private string m_scenarioName;
    private readonly string m_lastAction;
    private readonly DateTimeOffset m_lastActionDate;
    private readonly DateTime m_clockDate;
    private  ScenarioPlanningSettings m_planningSettings;
    private  ScenarioPermissionSettings m_scenarioPermissions;

    #region IPTSerializable Members
    public UnloadedScenarioData(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12507)
        {
            m_scenarioId = new BaseId(a_reader);
            a_reader.Read(out m_scenarioName);
            a_reader.Read(out m_lastAction);
            a_reader.Read(out m_lastActionDate);
            a_reader.Read(out m_clockDate);
            m_planningSettings = new ScenarioPlanningSettings(a_reader);
            m_scenarioPermissions = new ScenarioPermissionSettings(a_reader);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_scenarioId.Serialize(a_writer);
        a_writer.Write(m_scenarioName);
        a_writer.Write(m_lastAction);
        a_writer.Write(m_lastActionDate);
        a_writer.Write(m_clockDate);
        m_planningSettings.Serialize(a_writer);
        m_scenarioPermissions.Serialize(a_writer);
    }

    public int UniqueId => 1119;
    #endregion

    public UnloadedScenarioData(BaseId a_scenarioId, 
                              string a_scenarioName, 
                              string a_lastAction, 
                              DateTimeOffset a_lastActionDate, 
                              DateTime a_clockDate, 
                              ScenarioPlanningSettings a_planningSettings, 
                              ScenarioPermissionSettings a_scenarioPermissions)
    {
        m_scenarioId = a_scenarioId;
        m_scenarioName = a_scenarioName;
        m_lastAction = a_lastAction;
        m_lastActionDate = a_lastActionDate;
        m_clockDate = a_clockDate;
        m_planningSettings = a_planningSettings;
        m_scenarioPermissions = a_scenarioPermissions;
    }

    public BaseId ScenarioId => m_scenarioId;
    public string ScenarioName => m_scenarioName;
    public string LastAction => m_lastAction;
    public DateTimeOffset LastActionDate => m_lastActionDate;
    public DateTime ClockDate => m_clockDate;
    public ScenarioPlanningSettings PlanningSettings => m_planningSettings;
    public ScenarioPermissionSettings ScenarioPermissions => m_scenarioPermissions;

    public void SetName(string a_scenarioName)
    {
        m_scenarioName = a_scenarioName;
    }
    public void SetPlanningSettings(ScenarioPlanningSettings a_planningSettings)
    {
        m_planningSettings = a_planningSettings;
    }
    public void SetScenarioPermissions(ScenarioPermissionSettings a_permissionSettings)
    {
        m_scenarioPermissions = a_permissionSettings;
    }
}
