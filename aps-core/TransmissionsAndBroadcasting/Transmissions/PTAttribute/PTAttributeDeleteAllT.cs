using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all PTAttributes in the specified Scenario (and all of their Resources).
/// </summary>
public class PTAttributeDeleteAllT : PTAttributeBaseT, IPTSerializable
{
    public override string Description => "All PTAttributes deleted";

    public new const int UNIQUE_ID = 989;

    #region IPTSerializable Members
    public PTAttributeDeleteAllT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public PTAttributeDeleteAllT() { }

    public PTAttributeDeleteAllT(BaseId a_scenarioId)
        : base(a_scenarioId) { }
}