using PT.APSCommon;
using PT.ERPTransmissions;

namespace PT.Transmissions2;

/// <summary>
/// Updates PurchaseOrders in a particular Scenario.
/// </summary>
public class ScenarioDetailPurchaseOrderT : PurchaseToStockT, IScenarioIdBaseT, IPTSerializable
{
    private readonly BaseId scenarioId;

    public ScenarioDetailPurchaseOrderT() { }

    public ScenarioDetailPurchaseOrderT(BaseId aScenarioId)
    {
        scenarioId = aScenarioId;
    }

    public override string Description => "Purchase Order Changes saved";

    #region IScenarioIdBaseT Members
    public BaseId ScenarioId => scenarioId;
    #endregion

    #region IPTSerializable Members
    public ScenarioDetailPurchaseOrderT(IReader reader)
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

    public new const int UNIQUE_ID = 659;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}