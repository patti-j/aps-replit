using PT.Transmissions.User;

namespace PT.Transmissions;

/// <summary>
/// Base object for all User related transmissions.
/// </summary>
public abstract class UserBaseT : PTTransmission, IPTSerializable, IDataChangesDependentT<UserBaseT>
{
    #region IPTSerializable Members
    public UserBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }
    #endregion

    protected UserBaseT() { }
}