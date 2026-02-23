using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the ManufacturingOrder (and all of its Resources).
/// </summary>
public class ManufacturingOrderDeleteT : ManufacturingOrderIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 85;

    #region IPTSerializable Members
    public ManufacturingOrderDeleteT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ManufacturingOrderDeleteT() { }

    public ManufacturingOrderDeleteT(BaseId scenarioId, BaseId jobId, BaseId manufacturingOrderId)
        : base(scenarioId, jobId, manufacturingOrderId) { }

    public override string Description => "ManufacturingOrder deleted";
}