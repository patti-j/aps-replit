namespace PT.Transmissions;

public class AutoUpdateKeyT : PTTransmission, IPTSerializable
{
    #region IPTSerializable
    public const int UNIQUE_ID = 763;

    public AutoUpdateKeyT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public AutoUpdateKeyT() { }
}