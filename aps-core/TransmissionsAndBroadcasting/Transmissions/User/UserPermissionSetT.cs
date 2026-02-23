using PT.APSCommon;
using PT.SchedulerDefinitions.PermissionTemplates;

namespace PT.Transmissions;

/// <summary>
/// Create a new User by copying the specified User.
/// </summary>
public class UserPermissionSetT : UserBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 935;

    #region IPTSerializable Members
    public UserPermissionSetT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_permissionSet = new UserPermissionSet(a_reader);
            m_replacementId = new BaseId(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_permissionSet.Serialize(a_writer);
        m_replacementId.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly UserPermissionSet m_permissionSet;
    private BoolVector32 m_bools;
    private const short c_deleteIdx = 0;
    private const short c_defaultIdx = 1;
    private BaseId m_replacementId;

    public UserPermissionSet PermissionSet => m_permissionSet;

    public bool Delete
    {
        get => m_bools[c_deleteIdx];
        set => m_bools[c_deleteIdx] = value;
    }

    public bool Default
    {
        get => m_bools[c_defaultIdx];
        set => m_bools[c_defaultIdx] = value;
    }

    public BaseId DeleteReplacementId
    {
        get => m_replacementId;
        set => m_replacementId = value;
    }

    public UserPermissionSetT() { }

    public UserPermissionSetT(UserPermissionSet a_permissionSet)
    {
        m_permissionSet = a_permissionSet;
    }
    public override string Description => "A user permission set is created, modified, or deleted";
}