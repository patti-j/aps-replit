using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new ManufacturingOrder by copying the specified ManufacturingOrder.
/// </summary>
public class ManufacturingOrderCopyT : ManufacturingOrderBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 82;

    #region IPTSerializable Members
    public ManufacturingOrderCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            originalId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        originalId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId originalId; //Id of the ManufacturingOrder to copy.

    public ManufacturingOrderCopyT() { }

    public ManufacturingOrderCopyT(BaseId scenarioId, BaseId jobId, BaseId originalId)
        : base(scenarioId, jobId)
    {
        this.originalId = originalId;
    }

    public override string Description => "ManufacturingOrder copied";
}