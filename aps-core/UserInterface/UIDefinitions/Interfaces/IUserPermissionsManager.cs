using PT.APSCommon;
using PT.PackageDefinitions.PackageInterfaces;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.UserSettingTemplates;

namespace PT.UIDefinitions.Interfaces;

public interface IUserPermissionsManager
{
    //TODO: V12 Remove?
    UserPreferences CurrentUserPreferences();

    // A scenario object can have multiple scenario context, and the user might not 
    // have the same access level to each of the scenario context. However, their 
    // user group permission will not vary across the contexts, and the Validator 
    // should be used when we are not interested in the user's scenario permissions. 
    UserPermissionValidator Validator { get; }

    PlantPermissionValidator GetPlantValidator(BaseId a_plantId);

    bool PlantPermissionAutoGrantProperty { get; }

    /// <summary>
    /// Determine the current user's access level based on user permissions and the Scenario's View only/Edit state
    /// </summary>
    /// <param name="a_userPermissions">The required user permissions</param>
    EUserAccess GetCurrentUserEditAccess(params string[] a_userPermissions);

    //event Action<ScenarioUserRights> UserPermissionsChanged;
}