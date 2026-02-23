using PT.APSCommon;
using PT.ERPTransmissions;

namespace PT.Transmissions2;

/// <summary>
/// Updates Warehouses and Inventories in a particular Scenario.
/// </summary>
public class ScenarioDetailWarehouseT : WarehouseT, IScenarioIdBaseT, IPTSerializable
{
    private readonly BaseId scenarioId;

    public ScenarioDetailWarehouseT() { }

    public ScenarioDetailWarehouseT(BaseId aScenarioId)
    {
        scenarioId = aScenarioId;
    }

    public ScenarioDetailWarehouseT(BaseId aScenarioId, PtImportDataSet ds)
        : base(ds)
    {
        scenarioId = aScenarioId;
    }

    #region IScenarioIdBaseT Members
    public BaseId ScenarioId => scenarioId;
    #endregion

    #region IPTSerializable Members
    public ScenarioDetailWarehouseT(IReader reader)
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

    public new const int UNIQUE_ID = 616;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}