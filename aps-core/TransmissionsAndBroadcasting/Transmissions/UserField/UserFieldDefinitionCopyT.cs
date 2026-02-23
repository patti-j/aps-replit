using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new UserFieldDefinition by copying the specified UserFieldDefinition.
/// </summary>
public class UserFieldDefinitionCopyT : UserFieldDefinitionBaseT
{
    public override string Description => "User Field Definition copied";

    public const int UNIQUE_ID = 1121;

    #region IPTSerializable Members
    public UserFieldDefinitionCopyT(IReader a_reader)
        : base(a_reader)
    {
        OriginalId = new BaseId(a_reader);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        OriginalId.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId OriginalId; //Id of the UserFieldDefinition to copy.

    public UserFieldDefinitionCopyT() { }

    public UserFieldDefinitionCopyT(BaseId a_originalId)
    {
        OriginalId = a_originalId;
    }
}