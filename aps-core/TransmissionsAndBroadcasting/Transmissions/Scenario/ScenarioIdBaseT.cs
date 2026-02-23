using PT.APSCommon;
using PT.Common.Debugging;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Base class for all transmissions that must be passed to the Scenario object.
/// </summary>
public abstract class ScenarioIdBaseT : ScenarioBaseT, IHistoryScenarioId
{
    #region IPTSerializable Members
    protected ScenarioIdBaseT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 678)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int val);
            m_destination = (EDestinations)val;

            m_scenarioId = new BaseId(a_reader);
        }
        else if (a_reader.VersionNumber >= 652)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int val);
            m_destination = (EDestinations)val;

            m_scenarioId = new BaseId(a_reader);
            a_reader.Read(out bool checksumData);
            if (checksumData)
            {
                new ChecksumValues(a_reader);
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int val);
            m_destination = (EDestinations)val;

            m_scenarioId = new BaseId(a_reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        m_bools.Serialize(writer);

        writer.Write((int)m_destination);
        m_scenarioId.Serialize(writer);
    }
    #endregion

    protected ScenarioIdBaseT() { }
    protected ScenarioIdBaseT(BaseId a_scenarioId)
    {
        m_scenarioId = new BaseId(a_scenarioId);
    }

    protected ScenarioIdBaseT(ScenarioIdBaseT a_t)
        : base(a_t)
    {
        m_destination = a_t.m_destination;
        m_scenarioId = new BaseId(a_t.scenarioId);
    }

    private BoolVector32 m_bools;

    private BaseId m_scenarioId; // The id of the scenario this message is destined for.
    public BaseId scenarioId => m_scenarioId;

    public void ChangeScenarioTarget(BaseId a_scenarioId)
    {
        m_scenarioId = a_scenarioId;
    }

    public enum EDestinations { BasedOnScenarioId, ToLiveScenario }

    private EDestinations m_destination = EDestinations.BasedOnScenarioId;

    public EDestinations Destination
    {
        get => m_destination;
        set => m_destination = value;
    }

    public override string Description
    {
        get
        {
            DebugException.ThrowInTest($"{GetType()} transmission did not override Description");
            return "Scenario object updated";
        }
    }

    #region IHistoryScenarioId Members
    public BaseId ScenarioId => scenarioId;
    #endregion
}