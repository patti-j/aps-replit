using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Create a new User by copying the specified User.
/// </summary>
public class UserCopyT : UserBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 286;

    #region IPTSerializable Members
    public UserCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            originalId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        originalId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId originalId;

    public UserCopyT() { }

    public UserCopyT(BaseId originalId)
    {
        this.originalId = originalId;
    }
}