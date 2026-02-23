namespace PT.Transmissions;

public class SystemStartupOptionsT : SystemBaseT, IPTSerializable
{
    public SystemStartupOptionsT() { }

    #region IPTSerializable Members
    public SystemStartupOptionsT(IReader reader) : base(reader)
    {
        reader.Read(out biDirectionalConnectionTimeout);
        reader.Read(out uniDirectionalConnectionTimeout);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(BiDirectionalConnectionTimeout);
        writer.Write(UniDirectionalConnectionTimeout);
    }

    public new const int UNIQUE_ID = 457;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private int biDirectionalConnectionTimeout = 120;

    public int BiDirectionalConnectionTimeout
    {
        get => biDirectionalConnectionTimeout;

        set => biDirectionalConnectionTimeout = value;
    }

    private int uniDirectionalConnectionTimeout = 120;

    public int UniDirectionalConnectionTimeout
    {
        get => uniDirectionalConnectionTimeout;

        set => uniDirectionalConnectionTimeout = value;
    }
}