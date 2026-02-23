using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Delete a specific table.
/// </summary>
public class ItemCleanoutTableDeleteT : ItemCleanoutTableIdBaseT
{
    #region IPTSerializable Members
    public ItemCleanoutTableDeleteT(IReader a_reader)
        : base(a_reader)
    {
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 1127;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ItemCleanoutTableDeleteT() { }

    public ItemCleanoutTableDeleteT(BaseId a_scenarioId, BaseId a_tableId)
        : base(a_scenarioId, a_tableId) { }
}