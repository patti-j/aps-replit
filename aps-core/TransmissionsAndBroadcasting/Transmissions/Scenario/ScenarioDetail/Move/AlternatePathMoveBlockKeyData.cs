namespace PT.Transmissions;

public class AlternatePathMoveBlockKeyData : MoveBlockKeyData
{
    #region Serialization
    public AlternatePathMoveBlockKeyData(IReader reader)
        : base(reader)
    {
        reader.Read(out m_alternatePathExternalId);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_alternatePathExternalId);
    }

    public new const int UNIQUE_ID = 796;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private string m_alternatePathExternalId;

    public string AlternatePathExternalId
    {
        get => m_alternatePathExternalId;
        set => m_alternatePathExternalId = value;
    }
}