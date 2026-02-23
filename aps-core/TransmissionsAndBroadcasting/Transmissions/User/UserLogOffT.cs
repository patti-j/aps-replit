using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Summary description for UserLoggedOffTransmission.
/// </summary>
public class UserLogOffT : UserIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 159;

    #region IPTSerializable Members
    public UserLogOffT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserLogOffT() { }

    public UserLogOffT(BaseId userId)
        : base(userId)
    {
    }

    public override string Description => "User logoff";
}