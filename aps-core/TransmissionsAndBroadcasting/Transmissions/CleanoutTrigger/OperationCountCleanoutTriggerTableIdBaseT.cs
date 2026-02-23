using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public abstract class OperationCountCleanoutTriggerTableIdBaseT : OperationCountCleanoutTriggerTableBaseT
{
    #region IPTSerializable Members
    public OperationCountCleanoutTriggerTableIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            id = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        id.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1085;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected OperationCountCleanoutTriggerTableIdBaseT() { }

    protected OperationCountCleanoutTriggerTableIdBaseT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId)
    {
        id = tableId;
    }

    private readonly BaseId id;

    public BaseId Id => id;
}