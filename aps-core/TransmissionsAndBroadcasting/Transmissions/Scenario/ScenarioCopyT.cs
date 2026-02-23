using PT.APSCommon;
using PT.PackageDefinitions;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Create a new Scenario by copying an old one.
/// </summary>
public class ScenarioCopyT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 122;

    #region IPTSerializable Members
    public ScenarioCopyT(IReader a_reader)
        : base(a_reader)
    {
        InitialSettings = new List<SettingData>();

        if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int temp);
            m_scenarioType = (ScenarioTypes)temp;
            a_reader.Read(out m_customNameOverride);
            OriginalId = new BaseId(a_reader);

            a_reader.Read(out int settingsCount);
            for (int i = 0; i < settingsCount; i++)
            {
                InitialSettings.Add(new SettingData(a_reader));
            }
        }
        else if (a_reader.VersionNumber >= 719)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int temp);
            m_scenarioType = (ScenarioTypes)temp;
            a_reader.Read(out m_customNameOverride);
            OriginalId = new BaseId(a_reader);
        }
        else if (a_reader.VersionNumber >= 452)
        {
            a_reader.Read(out int temp);
            m_scenarioType = (ScenarioTypes)temp;
            a_reader.Read(out m_customNameOverride);
            OriginalId = new BaseId(a_reader);
        }

        #region 1
        else if (a_reader.VersionNumber >= 1)
        {
            bool temp;
            a_reader.Read(out temp);
            if (temp)
            {
                m_scenarioType = ScenarioTypes.Published;
            }
            else
            {
                m_scenarioType = ScenarioTypes.Whatif;
            }

            OriginalId = new BaseId(a_reader);
        }
        #endregion
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        a_writer.Write((int)m_scenarioType);
        a_writer.Write(m_customNameOverride);
        OriginalId.Serialize(a_writer);

        a_writer.Write(InitialSettings.Count);
        foreach (SettingData data in InitialSettings)
        {
            data.Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioCopyT() { }

    public ScenarioCopyT(BaseId a_originalId)
    {
        OriginalId = a_originalId;
        InitialSettings = new List<SettingData>();
    }

    private BoolVector32 m_bools;

    private readonly string m_customNameOverride = "";

    /// <summary>
    /// Name to add after the scenario type.
    /// </summary>
    public string CustomNameOverride => m_customNameOverride;

    public BaseId OriginalId;

    public List<SettingData> InitialSettings;

    private readonly ScenarioTypes m_scenarioType;

    public ScenarioTypes ScenarioType => m_scenarioType;

    private const int c_isBlackBoxScenarioIdx = 0;
    private const int c_isolateFromImportIdx = 1;
    private const int c_isolateFromClockAdvanceIdx = 2;

    public bool IsBlackBoxScenario
    {
        get => m_bools[c_isBlackBoxScenarioIdx];
        set => m_bools[c_isBlackBoxScenarioIdx] = value;
    }

    public bool IsolateFromImport
    {
        get => m_bools[c_isolateFromImportIdx];
        set => m_bools[c_isolateFromImportIdx] = value;
    }

    public bool IsolateFromClockAdvance
    {
        get => m_bools[c_isolateFromClockAdvanceIdx];
        set => m_bools[c_isolateFromClockAdvanceIdx] = value;
    }

    [NonSerialized] public object scenarioCopy;

    public ScenarioCopyT(BaseId originalId, ScenarioTypes a_scenarioType, string a_customNameOverride = "")
    {
        OriginalId = originalId;
        m_scenarioType = a_scenarioType;
        m_customNameOverride = a_customNameOverride;
        InitialSettings = new List<SettingData>();
    }

    public override string Description => "Scenario copy";
}

/// <summary>
/// Create a new Blank Scenario
/// </summary>
public class ScenarioNewT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 455;

    #region IPTSerializable Members
    public ScenarioNewT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioNewT() { }
}