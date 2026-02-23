using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all PTAttribute related transmissions.
/// </summary>
public abstract class PTAttributeBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 985;

    #region IPTSerializable Members
    public PTAttributeBaseT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected PTAttributeBaseT() { }

    protected PTAttributeBaseT(BaseId a_scenarioId)
        : base(a_scenarioId) { }
}