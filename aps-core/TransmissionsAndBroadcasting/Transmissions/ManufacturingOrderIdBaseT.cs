using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all ManufacturingOrder related transmissions.
/// </summary>
public abstract class ManufacturingOrderIdBaseT : ManufacturingOrderBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 86;

    #region IPTSerializable Members
    public ManufacturingOrderIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            manufacturingOrderId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        manufacturingOrderId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId manufacturingOrderId;

    protected ManufacturingOrderIdBaseT() { }

    protected ManufacturingOrderIdBaseT(BaseId scenarioId, BaseId jobId, BaseId manufacturingOrderId)
        : base(scenarioId, jobId)
    {
        this.manufacturingOrderId = manufacturingOrderId;
    }
}