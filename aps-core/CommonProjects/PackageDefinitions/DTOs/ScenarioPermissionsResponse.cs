using PT.PackageDefinitions.PackageInterfaces;

namespace PT.PackageDefinitions.DTOs;

/// <summary>
/// Provides, per scenario, a <see cref="ScenarioPermissionSet" /> object containing scenario permissions for all set groups and users.
/// </summary>
public class ScenarioPermissionsResponse
{
    /// <summary>
    /// A collection which, for each scenario (by ScenarioId), contains a list of <see cref="ScenarioPermissionSets" />
    /// describing scenario permissions for that group and its users.
    /// </summary>
    public Dictionary<long, ScenarioPermissionSet> ScenarioPermissionSets { get; set; } = new ();
}

/// <summary>
/// Provides grouped scenario permission data for one scenario.
/// </summary>
public class ScenarioPermissionSet
{
    /// <summary>
    /// The scenario this permission set is for.
    /// </summary>
    public long ScenarioId { get; set; }

    public string ScenarioName { get; set; }

    public long ScenarioOwnerId { get; set; }

    public string ScenarioOwnerName { get; set; }

    /// <summary>
    /// Whether the scenario has been shared. Full permission data is only populated for scenarios once shared; before that, only the owner is guaranteed to be set in the
    /// <see cref="GroupScenarioPermissions" />.
    /// Other users and groups should have no access.
    /// </summary>
    public bool Shared { get; set; }

    /// <summary>
    /// A nested collection of scenario permissions for groups and their constituent users.
    /// </summary>
    public List<GroupedScenarioPermissions> GroupScenarioPermissions { get; set; }
}

/// <summary>
/// Represents a tree structure of scenario permissions for one group (by GroupId) and its constituent users (which may differ from their parent).
/// </summary>
public class GroupedScenarioPermissions
{
    /// <summary>
    /// The group this permission set is for.
    /// </summary>
    public long GroupId { get; set; }

    public string GroupName { get; set; }

    /// <summary>
    /// The default permission for all users of this group.
    /// </summary>
    public EUserAccess GroupPermission { get; set; }

    /// <summary>
    /// The individual permissions for users of this group, if they have been set as different from the default <see cref="GroupPermission" />.
    /// </summary>
    public Dictionary<long, UserPermissionDto> UserPermissions { get; set; }
}

public class UserPermissionDto
{
    public long UserId { get; set; }

    public string UserName { get; set; }

    /// <summary>
    /// The scenario access of the permission group this user is a part of. <see cref="UserAccess"/> will take precedent over this unless set to <see cref="EUserAccess.UseGroupAccess"/>
    /// </summary>
    public EUserAccess GroupAccess { get; set; }

    /// <summary>
    /// The user's explicitly set user access. May defer to <see cref="GroupAccess"/> if set to <see cref="EUserAccess.UseGroupAccess"/>.
    /// This is desirable if showing the source of the user's permissions; if you want the actual one that would be used, use <see cref="ActualAccess"/>
    /// </summary>
    public EUserAccess UserAccess { get; set; }

    /// <summary>
    /// The user's individual <see cref="UserAccess"/>, unless set to <see cref="EUserAccess.UseGroupAccess"/>, in which case its <see cref="GroupAccess"/>.
    /// </summary>
    public EUserAccess ActualAccess => UserAccess == EUserAccess.UseGroupAccess ? GroupAccess : UserAccess;
}