using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new PTAttribute using default values.
/// </summary>
public class PTAttributeDefaultT : PTAttributeBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 988;

    public PTAttributeDefaultT() { }
    public PTAttributeDefaultT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public override string Description => "New PTAttribute created";
    #region IPTSerializable Members
    public PTAttributeDefaultT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion
}