using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the PTAttribute.
/// </summary>
public class PTAttributeDeleteT : PTAttributeIdBaseT
{
    public override string Description => "PTAttribute deleted";

    public new const int UNIQUE_ID = 990;

    #region IPTSerializable Members
    public PTAttributeDeleteT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public PTAttributeDeleteT() { }

    public PTAttributeDeleteT(BaseId a_scenarioId, IEnumerable<BaseId> a_udfDefinitionIds)
        : base(a_scenarioId, a_udfDefinitionIds) { }
}