using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public class OperationCountCleanoutTriggerTableUpdateT : OperationCountCleanoutTriggerTableIdBaseT
{
    #region IPTSerializable Members
    public OperationCountCleanoutTriggerTableUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_OperationCountCleanoutTriggerTable = new OperationCountCleanoutTriggerTable(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_OperationCountCleanoutTriggerTable.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1089;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public OperationCountCleanoutTriggerTableUpdateT() { }

    public OperationCountCleanoutTriggerTableUpdateT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    private OperationCountCleanoutTriggerTable m_OperationCountCleanoutTriggerTable = new ();

    public OperationCountCleanoutTriggerTable OperationCountCleanoutTriggerTable
    {
        get => m_OperationCountCleanoutTriggerTable;
        set => m_OperationCountCleanoutTriggerTable = value;
    }
}