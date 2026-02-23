using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Create a new table.
/// </summary>
public class CompatibilityCodeTableNewT : CompatibilityCodeTableBaseT
{
    #region IPTSerializable Members
    public CompatibilityCodeTableNewT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_compatibilityCodeTable = new CompatibilityCodeTable(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_compatibilityCodeTable.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 5055;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CompatibilityCodeTableNewT() { }

    public CompatibilityCodeTableNewT(BaseId a_scenarioId)
        : base(a_scenarioId) { }

    private CompatibilityCodeTable m_compatibilityCodeTable = new ();

    public CompatibilityCodeTable CompatibilityCodeTable
    {
        get => m_compatibilityCodeTable;
        set => m_compatibilityCodeTable = value;
    }

    public override string Description => "Compatibility Code Table created";
}