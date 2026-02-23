namespace PT.SchedulerDefinitions.UserSettingTemplates;

/// <summary>
/// This class is able to determine whether a permission is enabled. The set of permissions passed
/// to this class comes from UserPermissionSet. If m_autoGrant is true, then the set of permissions
/// passed to this class is a set of denied permissions, and it is assumed that the user has
/// the requested permission if the permission key is not found in the validator's permission set
/// (thus auto-granting them the permission). If m_autoGrant is false, then the requested permission
/// needs to be in the validator's permission set for the user to be granted the permission.
/// </summary>
public class UserPermissionValidator
{
    private readonly HashSet<string> m_permissions;

    /// <summary>
    /// Takes a list of permissions. Based on the auto grant bool they will be treated as granted permissions or denied permissions.
    /// </summary>
    public UserPermissionValidator(HashSet<string> a_permissions, bool a_administerUsers, bool a_maintainSystemSettings, bool a_allowUserOverrideChanges, bool a_canManageScenarios)
    {
        m_permissions = a_permissions;
        AdministerUsers = a_administerUsers;
        CanMaintainSystemSettings = a_maintainSystemSettings;
        AllowActionsThatAffectUsers = a_allowUserOverrideChanges;
        CanManageScenarios = a_canManageScenarios;
    }

    public bool ValidatePermission(string a_permissionsKey)
    {
        return m_permissions.Contains(a_permissionsKey);
    }

    public bool ValidatePermissions(IEnumerable<string> a_permissionsKeys, out List<string> o_missingPermissionKeys)
    {
        o_missingPermissionKeys = new List<string>();

        foreach (string permissionsKey in a_permissionsKeys)
        {
            if (!ValidatePermission(permissionsKey))
            {
                o_missingPermissionKeys.Add(permissionsKey);
            }
        }

        return o_missingPermissionKeys.Count == 0;
    }

    /*
     * I've been calling these 4 bools, User Group Super Permissions (or just super permissions).
     * They're permissions that have a broader scope than the standard user group permissions.
     */
    public bool AdministerUsers { get; }

    public bool CanMaintainSystemSettings { get; }

    public bool AllowActionsThatAffectUsers { get; }
    public bool CanManageScenarios { get; }
}

/// <summary>
/// This class is able to determine whether a permission is enabled.
/// </summary>
public class PlantPermissionValidator
{
    private readonly HashSet<string> m_permissions;
    private readonly bool m_autoGrant;

    /// <summary>
    /// Takes a list of permissions. Based on the auto grant bool they will be treated as granted permissions or denied permissions.
    /// </summary>
    public PlantPermissionValidator(HashSet<string> a_permissions, bool a_autoGrant)
    {
        m_permissions = a_permissions;
        m_autoGrant = a_autoGrant;
    }

    public bool ValidatePermission(string a_permissionsKey)
    {
        if (m_autoGrant)
        {
            return !m_permissions.Contains(a_permissionsKey);
        }

        return m_permissions.Contains(a_permissionsKey);
    }

    public bool AutoGrant => m_autoGrant;
}