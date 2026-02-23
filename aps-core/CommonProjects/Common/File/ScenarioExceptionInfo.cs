namespace PT.Common.File;

public class ScenarioExceptionInfo
{
    private string m_scenarioName;
    private string m_scenarioType;
    private DateTimeOffset m_scenarioCreationDate;

    public void Initialize(string a_scenarioName, string a_scenarioType, DateTimeOffset a_creationDateTime)
    {
        m_scenarioName = a_scenarioName;
        m_scenarioType = a_scenarioType;
        m_scenarioCreationDate = a_creationDateTime;
    }

    public bool Initialized => !string.IsNullOrEmpty(m_scenarioName) && !string.IsNullOrEmpty(m_scenarioType);

    public string ScenarioName => m_scenarioName;

    public string ScenarioType => m_scenarioType;

    public DateTimeOffset ScenarioCreationDate => m_scenarioCreationDate;
}