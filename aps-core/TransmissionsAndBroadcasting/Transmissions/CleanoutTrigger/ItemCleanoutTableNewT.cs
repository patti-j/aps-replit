using PT.APSCommon;
using PT.Transmissions.CleanoutTrigger;

namespace PT.Transmissions;

/// <summary>
/// Create a new table.
/// </summary>
public class ItemCleanoutTableNewT : ItemCleanoutTableBaseT
{
    #region IPTSerializable Members
    public ItemCleanoutTableNewT(IReader a_reader)
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

    public new const int UNIQUE_ID = 1129;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ItemCleanoutTableNewT() { }

    public ItemCleanoutTableNewT(BaseId a_scenarioId)
        : base(a_scenarioId) { }

    private ItemCleanoutTable m_itemCleanoutTable = new ();

    public ItemCleanoutTable ItemCleanoutTable
    {
        get => m_itemCleanoutTable;
        set => m_itemCleanoutTable = value;
    }
}