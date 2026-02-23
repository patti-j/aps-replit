using PT.APSCommon;
using PT.ERPTransmissions;

namespace PT.Transmissions2;

/// <summary>
/// Updates Sales Orders in a particular Scenario.
/// </summary>
public class ScenarioDetailSalesOrderT : SalesOrderT, IScenarioIdBaseT, IPTSerializable
{
    private readonly BaseId scenarioId;

    public ScenarioDetailSalesOrderT() { }

    public ScenarioDetailSalesOrderT(BaseId aScenarioId)
    {
        scenarioId = aScenarioId;
    }

    public override string Description => "Sales Order Changes saved";

    #region IScenarioIdBaseT Members
    public BaseId ScenarioId => scenarioId;
    #endregion

    #region IPTSerializable Members
    public ScenarioDetailSalesOrderT(IReader reader)
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

    public new const int UNIQUE_ID = 658;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}