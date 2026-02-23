using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Toggles scenario isolation parameters. For example enable or disable whether the scenario should process import transmissions.
/// </summary>
public class ScenarioIsolateT : ScenarioIdBaseT
{
    public const int UNIQUE_ID = 823;
    public ScenarioIsolateT() { }

    #region IPTSerializable Members
    public ScenarioIsolateT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 641)
        {
            m_bools = new BoolVector32(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BoolVector32 m_bools;
    private const short c_isolateImportIdx = 0;
    private const short c_isolateImportSetIdx = 1;
    private const short c_isolateClockAdvanceIdx = 2;
    private const short c_isolateClockAdvanceSetIdx = 3;

    public bool IsolateImport
    {
        get => m_bools[c_isolateImportIdx];
        set
        {
            m_bools[c_isolateImportIdx] = value;
            m_bools[c_isolateImportSetIdx] = true;
        }
    }

    public bool IsolateImportSet => m_bools[c_isolateImportSetIdx];

    public bool IsolateClockAdvance
    {
        get => m_bools[c_isolateClockAdvanceIdx];
        set
        {
            m_bools[c_isolateClockAdvanceIdx] = value;
            m_bools[c_isolateClockAdvanceSetIdx] = true;
        }
    }

    public bool IsolateClockAdvanceSet => m_bools[c_isolateClockAdvanceSetIdx];

    public ScenarioIsolateT(BaseId a_scenarioId) : base(a_scenarioId)
    {
    }

    public override string Description => "Scenario isolated".Localize();
}