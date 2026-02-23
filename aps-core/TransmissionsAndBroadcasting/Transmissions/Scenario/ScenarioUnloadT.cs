using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;
public class ScenarioUnloadT : ScenarioBaseT
{
    public const int UNIQUE_ID = 1124;
    public override int UniqueId => UNIQUE_ID;
    private readonly BaseId m_scenarioToUnloadId; 
    private readonly BaseId m_userRequestingUnloadId; 

    public ScenarioUnloadT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_scenarioToUnloadId = new BaseId(a_reader);
            m_userRequestingUnloadId = new BaseId(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_scenarioToUnloadId.Serialize(a_writer);
        m_userRequestingUnloadId.Serialize(a_writer);
    }

    public ScenarioUnloadT() { }

    public ScenarioUnloadT(BaseId a_scenarioId, BaseId a_userId)
    {
        m_scenarioToUnloadId = a_scenarioId;
        m_userRequestingUnloadId = a_userId;
    }

    public BaseId ScenarioToUnloadId => m_scenarioToUnloadId;
    public BaseId UserRequestingUnloadId => m_userRequestingUnloadId;

    public override string Description => "Scenario Unloaded".Localize();
}
