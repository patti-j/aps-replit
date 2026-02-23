using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base class for all AttributeCodeTable related Transmissions.
/// </summary>
public abstract class ItemCleanoutTableIdBaseT : ItemCleanoutTableBaseT
{
    #region IPTSerializable Members
    protected ItemCleanoutTableIdBaseT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_id = new BaseId(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_id.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 1128;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected ItemCleanoutTableIdBaseT() { }

    protected ItemCleanoutTableIdBaseT(BaseId a_scenarioId, BaseId a_tableId)
        : base(a_scenarioId)
    {
        m_id = a_tableId;
    }

    private readonly BaseId m_id;

    public BaseId Id => m_id;
}