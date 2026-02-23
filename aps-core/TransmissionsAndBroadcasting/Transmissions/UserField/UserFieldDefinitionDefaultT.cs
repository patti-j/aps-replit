using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new UserFieldDefinition using default values.
/// </summary>
public class UserFieldDefinitionDefaultT : UserFieldDefinitionBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 1120;

    #region IPTSerializable Members
    public UserFieldDefinitionDefaultT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserFieldDefinitionDefaultT() { }

    public override string Description => "New User Field Definition created";
}