using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Change scenario type.
/// </summary>
public class ScenarioChangeTypeT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 137;

    #region IPTSerializable Members
    public ScenarioChangeTypeT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 452)
        {
            scenarioId = new BaseId(reader);
            int enumTemp;
            reader.Read(out enumTemp);
            m_scenarioType = (SchedulerDefinitions.ScenarioTypes)enumTemp;
        }

        #region 1
        else if (reader.VersionNumber >= 1)
        {
            scenarioId = new BaseId(reader);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        scenarioId.Serialize(writer);
        writer.Write((int)m_scenarioType);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId scenarioId;

    public SchedulerDefinitions.ScenarioTypes m_scenarioType;

    /// <summary>
    /// Type to change the scenario to.
    /// </summary>
    public SchedulerDefinitions.ScenarioTypes ScenarioType => m_scenarioType;

    public ScenarioChangeTypeT() { }

    public ScenarioChangeTypeT(BaseId scenarioId, SchedulerDefinitions.ScenarioTypes a_scenarioType)
    {
        this.scenarioId = scenarioId;
        m_scenarioType = a_scenarioType;
    }
}