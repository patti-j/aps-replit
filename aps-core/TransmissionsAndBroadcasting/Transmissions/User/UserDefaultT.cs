namespace PT.Transmissions;

/// <summary>
/// Creates a new User using default values.
/// </summary>
public class UserDefaultT : UserBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 155;

    #region IPTSerializable Members
    public UserDefaultT(IReader reader)
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

    public UserDefaultT()
    {
    }

    public override string Description => "A new user was created";
}