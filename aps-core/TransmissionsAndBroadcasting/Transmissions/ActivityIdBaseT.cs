using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Activity related transmissions.
/// </summary>
public abstract class ActivityIdBaseT : OperationIdBaseT
{
    public new const int UNIQUE_ID = 442;

    #region IPTSerializable Members
    public ActivityIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            activityId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        activityId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId activityId;

    protected ActivityIdBaseT() { }

    protected ActivityIdBaseT(BaseId scenarioId, BaseId jobId, BaseId manufacturingOrderId, BaseId operationId, BaseId activityId)
        : base(scenarioId, jobId, manufacturingOrderId, operationId)
    {
        this.activityId = activityId;
    }
}