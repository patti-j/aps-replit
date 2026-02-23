using PT.APSCommon;
using PT.ERPTransmissions;

namespace PT.Transmissions2;

/// <summary>
/// This class is now only used to support backwards compatibility with recordings and undo checkpoints.
///  It is not otherwise constructed or serialized. Items are now sent with the WarehouseT.
/// Updates Items in a particular Scenario.
/// </summary>
public class ScenarioDetailItemT : ItemT, IScenarioIdBaseT, IPTSerializable
{
    private readonly BaseId scenarioId;

    public ScenarioDetailItemT() { }

    public ScenarioDetailItemT(BaseId aScenarioId)
    {
        scenarioId = aScenarioId;
    }

    //public ScenarioDetailItemT(BaseId aScenarioId, PtImportDataSet ds)
    //    : base(ds)
    //{
    //    this.scenarioId = aScenarioId;
    //}

    #region IScenarioIdBaseT Members
    public BaseId ScenarioId => scenarioId;
    #endregion

    #region IPTSerializable Members
    public ScenarioDetailItemT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            scenarioId = new BaseId(reader);
        }
    }

    //public override void Serialize(IWriter writer)
    //{
    //    base.Serialize(writer);
    //    scenarioId.Serialize(writer);
    //}

    public new const int UNIQUE_ID = 615;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}