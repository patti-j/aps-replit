namespace PT.Transmissions;

/// <summary>
/// Summary description for ScenarioManagerBaseT.
/// </summary>
public abstract class ScenarioManagerBaseT : PTTransmission, IPTSerializable
{
    public ScenarioManagerBaseT() { }

    #region IPTSerializable Members
    public ScenarioManagerBaseT(IReader reader) : base(reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }
    #endregion
}