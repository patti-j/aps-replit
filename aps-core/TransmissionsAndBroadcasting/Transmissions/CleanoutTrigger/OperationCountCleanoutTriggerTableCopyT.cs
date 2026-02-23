using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public class OperationCountCleanoutTriggerTableCopyT : OperationCountCleanoutTriggerTableIdBaseT
{
    #region IPTSerializable Members
    public OperationCountCleanoutTriggerTableCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1086;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public OperationCountCleanoutTriggerTableCopyT() { }

    public OperationCountCleanoutTriggerTableCopyT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    public override string Description => "Operation Count Cleanout Trigger Table copied";
}