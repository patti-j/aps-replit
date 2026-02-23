using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Copy a specific Table.
/// </summary>
public class ItemCleanoutTableCopyT : ItemCleanoutTableIdBaseT
{
    #region IPTSerializable Members
    public ItemCleanoutTableCopyT(IReader a_reader)
        : base(a_reader)
    {
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 1126;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ItemCleanoutTableCopyT() { }

    public ItemCleanoutTableCopyT(BaseId a_scenarioId, BaseId a_tableId)
        : base(a_scenarioId, a_tableId) { }
}