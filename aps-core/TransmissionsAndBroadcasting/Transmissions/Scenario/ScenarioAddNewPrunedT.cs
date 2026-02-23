using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Stores the serialized scenario bytes of the new scenario to add.
/// Serialized scenario is compressed on construction.
/// </summary>
public class ScenarioAddNewPrunedT : ScenarioAddNewT, IPTSerializable
{
    public new const int UNIQUE_ID = 816;

    #region IPTSerializable Members
    public ScenarioAddNewPrunedT(IReader reader)
        : base(reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioAddNewPrunedT() { }

    public ScenarioAddNewPrunedT(byte[] a_scenarioBytes, BaseId a_scenarioId)
        : base(a_scenarioBytes, a_scenarioId, null) { }
}