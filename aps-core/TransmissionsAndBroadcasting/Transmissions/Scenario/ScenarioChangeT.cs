using PT.APSCommon;
using PT.PackageDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Transmission for changing an existing Scenario.
/// </summary>
public class ScenarioChangeT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 120;

    #region IPTSerializable Members
    public ScenarioChangeT(IReader a_reader) : base(a_reader)
    {
        ScenarioSettings = new List<SettingData>();

        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out m_scenarioName);
            a_reader.Read(out int settingsCount);
            for (int i = 0; i < settingsCount; i++)
            {
                ScenarioSettings.Add(new SettingData(a_reader));
            }
        }
        else
        {
            a_reader.Read(out string propertyName); //Deprecated
            a_reader.Read(out string oldValue); //Deprecated
            a_reader.Read(out string newValue); //Deprecated
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_scenarioName);

        a_writer.Write(ScenarioSettings.Count);
        foreach (SettingData data in ScenarioSettings)
        {
            data.Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioChangeT() { }

    public ScenarioChangeT(BaseId a_scenarioId, string a_scenarioName)
        : base(a_scenarioId)
    {
        m_scenarioName = a_scenarioName;
        ScenarioSettings = new List<SettingData>();
    }

    private readonly string m_scenarioName;

    public string ScenarioName => m_scenarioName;

    public List<SettingData> ScenarioSettings;

    public override string Description => "Scenario Settings Change";
}