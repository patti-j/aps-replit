namespace PT.Transmissions;

/// <summary>
/// Summary description for SystemBaseT.
/// </summary>
public class SystemBaseT : PTTransmission, IPTSerializable
{
    public SystemBaseT() { }

    #region IPTSerializable Members
    public SystemBaseT(IReader reader) : base(reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public const int UNIQUE_ID = 458;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}