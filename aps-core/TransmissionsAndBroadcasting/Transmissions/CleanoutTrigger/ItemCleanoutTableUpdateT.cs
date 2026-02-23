using PT.APSCommon;
using PT.Transmissions.CleanoutTrigger;

namespace PT.Transmissions;

/// <summary>
/// Update an existing table.
/// </summary>
public class ItemCleanoutTableUpdateT : ItemCleanoutTableIdBaseT
{
    #region IPTSerializable Members
    public ItemCleanoutTableUpdateT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_itemCleanoutTable = new ItemCleanoutTable(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_itemCleanoutTable.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 1130;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ItemCleanoutTableUpdateT() { }

    public ItemCleanoutTableUpdateT(BaseId a_scenarioId, BaseId a_tableId)
        : base(a_scenarioId, a_tableId) { }

    private ItemCleanoutTable m_itemCleanoutTable = new ();

    public ItemCleanoutTable ItemCleanoutTable
    {
        get => m_itemCleanoutTable;
        set => m_itemCleanoutTable = value;
    }
}