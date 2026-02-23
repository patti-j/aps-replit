using PT.APSCommon;
using PT.Transmissions;

namespace PT.Transmissions2;

public class ScenarioDetailCustomerT : CustomerT, IScenarioIdBaseT, IPTSerializable
{
    private readonly BaseId m_scenarioId;

    public ScenarioDetailCustomerT() { }

    public ScenarioDetailCustomerT(BaseId a_scenarioId)
    {
        m_scenarioId = a_scenarioId;
    }

    #region IScenarioIdBaseT Members
    public BaseId ScenarioId => m_scenarioId;
    #endregion

    #region IPTSerializable Members
    public ScenarioDetailCustomerT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_scenarioId = new BaseId(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_scenarioId.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 1014;
    public override int UniqueId => UNIQUE_ID;
    #endregion
}