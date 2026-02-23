using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the UserFieldDefinition.
/// </summary>
public class UserFieldDefinitionDeleteT : UserFieldDefinitionIdBaseT
{
    public override string Description => "User Field Definition deleted";

    public const int UNIQUE_ID = 1118;

    #region IPTSerializable Members
    public UserFieldDefinitionDeleteT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserFieldDefinitionDeleteT() { }

    public UserFieldDefinitionDeleteT(IEnumerable<BaseId> a_udfDefinitionIds)
        : base(a_udfDefinitionIds) { }
}