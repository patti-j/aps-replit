using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Operation related transmissions.
/// </summary>
public abstract class OperationIdBaseT : ManufacturingOrderIdBaseT
{
    public new const int UNIQUE_ID = 441;

    #region IPTSerializable Members
    public OperationIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            operationId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        operationId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId operationId;

    protected OperationIdBaseT() { }

    protected OperationIdBaseT(BaseId scenarioId, BaseId jobId, BaseId manufacturingOrderId, BaseId operationId)
        : base(scenarioId, jobId, manufacturingOrderId)
    {
        this.operationId = operationId;
    }
}