using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

/// <summary>
/// Create a new table.
/// </summary>
public class OperationCountCleanoutTriggerTableNewT : OperationCountCleanoutTriggerTableBaseT
{
    #region IPTSerializable Members
    public OperationCountCleanoutTriggerTableNewT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12305)
        {
            cleanoutTriggerTable = new OperationCountCleanoutTriggerTable(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        cleanoutTriggerTable.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1087;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public OperationCountCleanoutTriggerTableNewT() { }

    public OperationCountCleanoutTriggerTableNewT(BaseId scenarioId)
        : base(scenarioId) { }

    private OperationCountCleanoutTriggerTable cleanoutTriggerTable = new ();

    public OperationCountCleanoutTriggerTable CleanoutTriggerTable
    {
        get => cleanoutTriggerTable;
        set => cleanoutTriggerTable = value;
    }

    public override string Description => "Operation Count Cleanout Trigger Table created";
}