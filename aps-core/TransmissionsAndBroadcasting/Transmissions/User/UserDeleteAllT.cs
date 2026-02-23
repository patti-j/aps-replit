namespace PT.Transmissions;

/// <summary>
/// UserDeleteAllT is the transmission sent to attempt a delete on all users.
/// The system must have at least one user that can administer users, and deleting
/// the active user will cause some odd behavior so these are the two situations
/// where the server will not delete the user. If a user is the owner of a scenario,
/// the server will re-assign the owner of the scenario to the server/Copilot.
/// </summary>
public class UserDeleteAllT : UserBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 156;

    #region IPTSerializable Members
    public UserDeleteAllT(IReader reader)
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

    public UserDeleteAllT()
    {
    }

    public override string Description => "All users were deleted";
}