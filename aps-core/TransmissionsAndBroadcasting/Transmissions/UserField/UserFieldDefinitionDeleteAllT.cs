using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all UserFieldDefinitions in the specified Scenario (and all of their objects).
/// </summary>
public class UserFieldDefinitionDeleteAllT : UserFieldDefinitionBaseT, IPTSerializable
{
    public override string Description => "All User Field Definitions deleted";

    public const int UNIQUE_ID = 1119;

    #region IPTSerializable Members
    public UserFieldDefinitionDeleteAllT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserFieldDefinitionDeleteAllT() { }
}