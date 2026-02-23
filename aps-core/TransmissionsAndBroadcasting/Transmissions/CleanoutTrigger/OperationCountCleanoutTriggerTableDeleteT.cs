using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public class OperationCountCleanoutTriggerTableDeleteT : OperationCountCleanoutTriggerTableIdBaseT
{
    #region IPTSerializable Members
    public OperationCountCleanoutTriggerTableDeleteT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1088;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public OperationCountCleanoutTriggerTableDeleteT() { }

    public OperationCountCleanoutTriggerTableDeleteT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    public override string Description => "Operation Count Cleanout Trigger Table deleted";
}