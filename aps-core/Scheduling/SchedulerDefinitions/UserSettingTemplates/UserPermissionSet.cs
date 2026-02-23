using PT.APSCommon;
using PT.APSCommon.Interfaces;
using PT.Common.Extensions;
using PT.SchedulerDefinitions.UserSettingTemplates;

namespace PT.SchedulerDefinitions.PermissionTemplates;

/// <summary>
/// Users in PT must belong to a User Group, and each UserPermissionSet is linked to
/// one User Group. The information in a UserPermissionSet can be viewed and changed
/// on the User Permission Management tile of the Users Board.
/// Within the UserPermissionSet, there are a set of administrative permissions
/// that are relevant across the system, and two levels of permission that pertain
/// to specific functionalities within the PT software. The higher level functionality
/// permissions are stored in m_allowedPermissionGroups and m_deniedPermissionGroups,
/// while the lower level permissions are in m_allowedPermissions and m_deniedPermissions.
/// Permission Group in this case refers to a group of permissions (not the user group).
/// The group functionality permissions typically refers to a specific board while the
/// individual functionality permissions refers to specific functionalities within a
/// a board.
/// The UserPermissionSet is used to create a PermissionSetValidator, which is then used
/// by the PT client to check whether or not the user has permissions to perform
/// certain actions in the PlanetTogether software. I believe that this is done so
/// that each user can have their own permission validator instead of having the potential of
/// many users trying to concurrently access the UserPermissionSet associated with
/// their User Group. If AutoGrantNewPermissions is true, then m_deniedPermissions is passed
/// to the UserPermissionValidator, and if false, then m_allowedPermissionGroups is passed to it.
/// </summary>
public class UserPermissionSet : IUserPermissionSet, IPTSerializable, ICloneable
{
    #region IPTSerializable
    public UserPermissionSet(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12018)
        {
            m_bools = new BoolVector32(a_reader);

            m_id = new BaseId(a_reader);
            a_reader.Read(out m_name);

            a_reader.Read(out int allowedCount);
            for (int i = 0; i < allowedCount; i++)
            {
                a_reader.Read(out string permissionKey);
                m_allowedPermissions.Add(permissionKey);
            }

            a_reader.Read(out int allowedGroupCount);
            for (int i = 0; i < allowedGroupCount; i++)
            {
                a_reader.Read(out string groupKey);
                m_allowedPermissionGroups.Add(groupKey);
            }

            a_reader.Read(out int deniedCount);
            for (int i = 0; i < deniedCount; i++)
            {
                a_reader.Read(out string permissionKey);
                m_deniedPermissions.Add(permissionKey);
            }

            a_reader.Read(out int deniedGroupCount);
            for (int i = 0; i < deniedGroupCount; i++)
            {
                a_reader.Read(out string groupKey);
                m_deniedPermissionGroups.Add(groupKey);
            }
        }

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);

            m_id = new BaseId(a_reader);
            a_reader.Read(out m_name);

            a_reader.Read(out int allowedCount);
            for (int i = 0; i < allowedCount; i++)
            {
                a_reader.Read(out string permissionKey);
                m_allowedPermissions.Add(permissionKey);
            }

            a_reader.Read(out int deniedCount);
            for (int i = 0; i < deniedCount; i++)
            {
                a_reader.Read(out string permissionKey);
                m_deniedPermissions.Add(permissionKey);
            }
        }
        #endregion
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);

        m_id.Serialize(a_writer);
        a_writer.Write(m_name);
        a_writer.Write(m_allowedPermissions.Count);
        foreach (string p in m_allowedPermissions)
        {
            a_writer.Write(p);
        }

        a_writer.Write(m_allowedPermissionGroups.Count);
        foreach (string p in m_allowedPermissionGroups)
        {
            a_writer.Write(p);
        }

        a_writer.Write(m_deniedPermissions.Count);
        foreach (string p in m_deniedPermissions)
        {
            a_writer.Write(p);
        }

        a_writer.Write(m_deniedPermissionGroups.Count);
        foreach (string p in m_deniedPermissionGroups)
        {
            a_writer.Write(p);
        }
    }

    public int UniqueId => 929;
    #endregion

    private readonly HashSet<string> m_allowedPermissions = new ();
    private readonly HashSet<string> m_deniedPermissions = new ();

    //TODO: Kept these here in case we want to restore the functionality at some point. These can be used to store permission groups that are allowed/denied
    // This can be used to auto-grant permissions only in allowed groups.
    private readonly HashSet<string> m_allowedPermissionGroups = new ();
    private readonly HashSet<string> m_deniedPermissionGroups = new ();

    /// <summary>
    /// Construct a new Permission for use in updating settings
    /// </summary>
    /// <param name="a_name"></param>
    public UserPermissionSet(BaseId a_id, string a_name)
    {
        m_id = a_id;
        m_name = a_name;
    }

    /// <summary>
    /// Construct a new default permissions set
    /// </summary>
    public UserPermissionSet(BaseId a_id)
    {
        m_id = a_id;
        m_name = "Administration".Localize();
        //Start with Admin permissions
        AdministerUsers = true;
        AutoGrantNewPermissions = true;
        MaintainSystemSettings = true;
        AllowChangesThatOverrideOtherUserActions = true;
        CanManageScenarios = true;
    }

    /// <summary>
    /// Construct a new default permissions set
    /// </summary>
    public UserPermissionSet(UserPermissionSet a_originalSet, BaseId a_id)
    {
        m_id = a_id;
        Update(a_originalSet);
    }

    /// <summary>
    /// Sync the permissions loaded from packages into this group
    /// They will be auto-granted based on the AutoGrantNewPermissions setting
    /// This also removes any permissions no longer loaded by a package
    /// </summary>
    internal void ValidateLoadedPackagePermissions(IEnumerable<(string Group, string Key)> a_permissions)
    {
        HashSet<string> allPermissionKeys = new ();

        if (!ValidatePermissionsWithLegacyGroupsDefaults(a_permissions))
        {
            foreach ((string Group, string Key) in a_permissions)
            {
                allPermissionKeys.Add(Key);
                if (!m_deniedPermissions.Contains(Key))
                {
                    if (AutoGrantNewPermissions)
                    {
                        m_allowedPermissions.AddIfNew(Key);
                    }
                    else if (!m_allowedPermissions.Contains(Key))
                    {
                        //Auto denied
                        m_deniedPermissions.Add(Key);
                    }
                }
            }

            //Clear out any permissions no longer loaded by a package
            List<string> permissions = m_allowedPermissions.ToList();
            foreach (string allowedPermission in permissions)
            {
                if (!allPermissionKeys.Contains(allowedPermission))
                {
                    m_allowedPermissions.Remove(allowedPermission);
                }
            }

            permissions = m_deniedPermissions.ToList();
            foreach (string deniedPermission in permissions)
            {
                if (!allPermissionKeys.Contains(deniedPermission))
                {
                    m_deniedPermissions.Remove(deniedPermission);
                }
            }
        }

        // Remove legacy group lists, now that they've been processed.
        m_allowedPermissionGroups.Clear();
        m_deniedPermissionGroups.Clear();
    }

    /// <summary>
    /// Earlier serialization versions would not set any permissions until they were explicitly modified,
    /// and instead rely on broad, grouped permissions for each UserPermissionSet.
    /// </summary>
    /// <param name="a_permissions"></param>
    private bool ValidatePermissionsWithLegacyGroupsDefaults(IEnumerable<(string Group, string Key)> a_permissions)
    {
        // If any permissionGroups are set, this UserPermissionSet data has not yet been processed by the ValidateLoadedPackagePermissions method which was implemented when these were made obsolete.
        // If any explicit permissions are set, it means the old scenario was saved at least once, and thus no longer needs default values set.
        bool usesLegacyGroupsWithoutSavedPermissions = !m_allowedPermissions.Any() &&
                                                       !m_deniedPermissions.Any() &&
                                                       (m_allowedPermissionGroups.Any() || m_deniedPermissionGroups.Any());

        if (usesLegacyGroupsWithoutSavedPermissions)
        {
            // Populate individual permissions based on their groups (as they previously would have in the UI before first update)
            foreach ((string Group, string Key) in a_permissions)
            {
                if (m_allowedPermissionGroups.Contains(Group))
                {
                    m_allowedPermissions.AddIfNew(Key);
                }
                else
                {
                    //Auto denied
                    m_deniedPermissions.Add(Key);
                }
            }
        }

        return usesLegacyGroupsWithoutSavedPermissions;
    }

    /// <summary>
    /// Returns a validator class that is capable of determining whether the user has a specified permission
    /// </summary>
    public UserPermissionValidator GetPermissionsValidator()
    {
        return new UserPermissionValidator(m_allowedPermissions, AdministerUsers, MaintainSystemSettings, AllowChangesThatOverrideOtherUserActions, CanManageScenarios);
    }

    /// <summary>
    /// Returns the permission keys in this group
    /// </summary>
    /// <param name="a_allowed">Whether to return the list of allowed permissions or the denied permissions</param>
    /// <returns></returns>
    public HashSet<string> GetPermissions(bool a_allowed)
    {
        return a_allowed ? m_allowedPermissions : m_deniedPermissions;
    }

    /// <summary>
    /// Returns whether the permission set contains an unconfigured permission
    /// </summary>
    /// <param name="a_permissionKey"></param>
    /// <returns></returns>
    public bool ContainsUnConfiguredPermission(string a_permissionKey)
    {
        return !m_allowedPermissions.Contains(a_permissionKey) && !m_deniedPermissions.Contains(a_permissionKey);
    }

    /// <summary>
    /// Adds a new permission to one of the permissions lists, for the specified scenario
    /// </summary>
    public void AddPermission(string a_permission, bool a_allowed)
    {
        HashSet<string> dict = a_allowed ? m_allowedPermissions : m_deniedPermissions;
        dict.AddIfNew(a_permission);
    }

    /// <summary>
    /// Adds a new permission to one of the permissions lists, for the specified scenario
    /// </summary>
    public void AddPermissions(IEnumerable<string> a_permission, bool a_allowed)
    {
        foreach (string permissionKey in a_permission)
        {
            AddPermission(permissionKey, a_allowed);
        }
    }

    public void Update(UserPermissionSet a_permissionSet)
    {
        m_name = a_permissionSet.Name;
        m_bools = a_permissionSet.m_bools;

        //Update dictionaries
        m_allowedPermissions.Clear();
        m_deniedPermissions.Clear();
        foreach (string permission in a_permissionSet.m_allowedPermissions)
        {
            m_allowedPermissions.Add(permission);
        }

        foreach (string permission in a_permissionSet.m_deniedPermissions)
        {
            m_deniedPermissions.Add(permission);
        }

        m_allowedPermissionGroups.Clear();
        m_deniedPermissionGroups.Clear();
        foreach (string permission in a_permissionSet.m_allowedPermissionGroups)
        {
            m_allowedPermissionGroups.Add(permission);
        }

        foreach (string permission in a_permissionSet.m_deniedPermissionGroups)
        {
            m_deniedPermissionGroups.Add(permission);
        }
    }

    private BoolVector32 m_bools;
    private const short c_administerUsersIdx = 0;
    private const short c_autoGrantNewPermissionsIdx = 1;
    private const short c_maintainSystemSettingsIdx = 2;
    private const short c_allowChangesThatOverrideOtherUsersIdx = 3;
    private const short c_canManageScenarios = 4;

    private BaseId m_id;

    public BaseId Id => m_id;

    public void GenerateId(BaseId a_id)
    {
        m_id = a_id;
    }

    private string m_name;

    public string Name
    {
        get => m_name;
        set => m_name = value;
    }

    /// <summary>
    /// User Group super permission for whether users in this group are allowed to modify user data
    /// </summary>
    public bool AdministerUsers
    {
        get => m_bools[c_administerUsersIdx];
        set => m_bools[c_administerUsersIdx] = value;
    }

    /// <summary>
    /// User Group super permission for whether users in this group are automatically granted new, unconfigured permissions
    /// </summary>
    public bool AutoGrantNewPermissions
    {
        get => m_bools[c_autoGrantNewPermissionsIdx];
        set => m_bools[c_autoGrantNewPermissionsIdx] = value;
    }

    /// <summary>
    /// User Group super permission for whether users in this group are allowed to modify system settings
    /// </summary>
    public bool MaintainSystemSettings
    {
        get => m_bools[c_maintainSystemSettingsIdx];
        set => m_bools[c_maintainSystemSettingsIdx] = value;
    }

    /// <summary>
    /// User Group super permission for whether users in this group are allowed to make changes to other user's data. Such as delete their scenarios or undo their actions
    /// </summary>
    public bool AllowChangesThatOverrideOtherUserActions
    {
        get => m_bools[c_allowChangesThatOverrideOtherUsersIdx];
        set => m_bools[c_allowChangesThatOverrideOtherUsersIdx] = value;
    }

    /// <summary>
    /// User Group super permission for whether users in this group can manage scenarios, which
    /// is basically all the actions that modify scenario data. This supersedes
    /// whatever the scenario's shared permissions is set to, and it'll effectively make the user
    /// an owner of all the scenarios. It does NOT supersede user group permissions though,
    /// so if a user lacks certain user group permissions such as jobs, then they'll still need
    /// the jobs user group permission to interact with jobs.
    /// </summary>
    public bool CanManageScenarios
    {
        get => m_bools[c_canManageScenarios];
        set => m_bools[c_canManageScenarios] = value;
    }

    public object Clone()
    {
        return new UserPermissionSet(this, Id);
    }
}