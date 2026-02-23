using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new User using default values.
/// </summary>
public class CustomerDefaultT : CustomerBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 1015;

    #region IPTSerializable Members
    public CustomerDefaultT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public CustomerDefaultT() { }
    public CustomerDefaultT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public override int UniqueId => UNIQUE_ID;

    public override string Description => "New Customer created";
    #endregion
}