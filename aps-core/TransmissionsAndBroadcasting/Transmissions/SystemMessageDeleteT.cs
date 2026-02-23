namespace PT.Transmissions;

/// <summary>
/// Deletes SystemMessages.
/// </summary>
public class SystemMessageDeleteT : UserBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 149;

    #region IPTSerializable Members
    public SystemMessageDeleteT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out deleteAllMessages);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(deleteAllMessages);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public bool deleteAllMessages; //If true, delete all.  Otherwise only the user's own.

    public SystemMessageDeleteT() { }

    public SystemMessageDeleteT(bool deleteAll)
    {
        deleteAllMessages = deleteAll;
    }
}