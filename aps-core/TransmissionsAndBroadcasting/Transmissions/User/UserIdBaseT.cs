using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all User related transmissions.
/// </summary>
public abstract class UserIdBaseT : UserBaseT, IPTSerializable
{
    #region PT Serialization
    public UserIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            userId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        userId.Serialize(writer);
    }
    #endregion

    public BaseId userId;

    protected UserIdBaseT() { }

    protected UserIdBaseT(BaseId userId)
    {
        this.userId = userId;
    }

    protected UserIdBaseT(ulong lastTransmissionNbr) { }
}