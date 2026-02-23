using PT.SchedulerDefinitions.PermissionTemplates;

namespace PT.Transmissions;

/// <summary>
/// Create a new User by copying the specified User.
/// </summary>
public class UserPermissionSetsT : UserBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 1;

    #region IPTSerializable Members
    public UserPermissionSetsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 666)
        {
            m_permissionSets = new UserPermissionSetManager(reader, null);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_permissionSets.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly UserPermissionSetManager m_permissionSets;

    public UserPermissionSetManager PermissionSets => m_permissionSets;

    public UserPermissionSetsT() { }

    public UserPermissionSetsT(UserPermissionSetManager a_permissionSets)
    {
        m_permissionSets = a_permissionSets;
    }
}