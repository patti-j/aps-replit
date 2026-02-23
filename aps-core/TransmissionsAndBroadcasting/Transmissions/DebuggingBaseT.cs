namespace PT.Transmissions;

/// <summary>
/// Base for transmissions created for debugging purposes.
/// </summary>
public abstract class DebuggingBaseT : PTTransmission, IPTSerializable
{
    public DebuggingBaseT() { }

    #region IPTSerializable Members
    public DebuggingBaseT(IReader reader)
        : base(reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }
    #endregion
}