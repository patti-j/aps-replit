using PT.APSCommon;
using PT.ERPTransmissions;

namespace PT.Transmissions2;

public class ScenarioDetailProductRulesT : ProductRulesT, IScenarioIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public ScenarioDetailProductRulesT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            scenarioId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        scenarioId.Serialize(writer);
    }

    public new const int UNIQUE_ID = 622;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly BaseId scenarioId;

    public ScenarioDetailProductRulesT() { }

    public ScenarioDetailProductRulesT(BaseId aScenarioId)
    {
        scenarioId = aScenarioId;
    }

    public override string Description => "Product Rules saved";

    #region IScenarioIdBaseT Members
    public BaseId ScenarioId => scenarioId;
    #endregion
}