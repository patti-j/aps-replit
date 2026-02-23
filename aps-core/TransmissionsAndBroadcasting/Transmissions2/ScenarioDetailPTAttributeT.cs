using PT.APSCommon;
using PT.Transmissions;

namespace PT.Transmissions2;

public class ScenarioDetailPTAttributeT : PTAttributeT, IScenarioIdBaseT, IPTSerializable
{
    private readonly BaseId m_scenarioId;
    public BaseId ScenarioId => m_scenarioId;

    public override string Description => "PTAttributes updated.";

    public ScenarioDetailPTAttributeT() { }

    public ScenarioDetailPTAttributeT(BaseId a_scenarioId)
    {
        m_scenarioId = a_scenarioId;
    }

    #region IPTSerializable Members
    public ScenarioDetailPTAttributeT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12303)
        {
            m_scenarioId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        m_scenarioId.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1016;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}