using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Summary description for Class1.
/// </summary>
public class UserLogOnT : UserIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 287;

    #region PT Serialization
    public UserLogOnT(IReader reader)
        : base(reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserLogOnT() { }

    public UserLogOnT(BaseId userId)
        : base(userId)
    {
    }

    public override string Description => "User logon";
}