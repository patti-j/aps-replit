using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base class for all ItemCleanoutTable related Transmissions.
/// </summary>
public abstract class ItemCleanoutTableBaseT : ScenarioIdBaseT
{
    #region IPTSerializable Members
    protected ItemCleanoutTableBaseT(IReader a_reader)
        : base(a_reader)
    {
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public const int UNIQUE_ID = 1112;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected ItemCleanoutTableBaseT() { }

    protected ItemCleanoutTableBaseT(BaseId a_scenarioId)
        : base(a_scenarioId) { }

    public override string Description => "Item Cleanout Table updated";
}