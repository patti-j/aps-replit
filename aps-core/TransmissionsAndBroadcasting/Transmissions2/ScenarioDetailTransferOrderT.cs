using PT.APSCommon;
using PT.ERPTransmissions;

namespace PT.Transmissions2;

/// <summary>
/// Updates TransferOrders in a particular Scenario.
/// </summary>
public class ScenarioDetailTransferOrderT : TransferOrderT, IScenarioIdBaseT, IPTSerializable
{
    private readonly BaseId scenarioId;

    public ScenarioDetailTransferOrderT() { }

    public ScenarioDetailTransferOrderT(BaseId aScenarioId)
    {
        scenarioId = aScenarioId;
    }

    #region IScenarioIdBaseT Members
    public BaseId ScenarioId => scenarioId;
    #endregion

    #region IPTSerializable Members
    public ScenarioDetailTransferOrderT(IReader reader)
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

    public new const int UNIQUE_ID = 661;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}