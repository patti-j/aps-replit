using PT.APSCommon;
using PT.PackageDefinitions.Settings;

namespace PT.PackageDefinitions;

public interface IUserManager
{
    bool ValidateScenarioEditPermissions(BaseId a_userId, ScenarioPermissionSettings a_scenarioPermissionSettings);

    BaseId GetDefaultAdministrationGroupId();
    BaseId GetDefaultViewOnlyGroupId();

    /// <summary>
    /// Retrieve User by their ID.
    /// </summary>
    /// <param name="a_userId"></param>
    /// <returns></returns>
    object GetUserById(BaseId a_userId);

    /// <summary>
    /// This function is meant to find the BaseId of the UserPermissionSet that the user belongs to
    /// </summary>
    /// <param name="a_userId"></param>
    /// <returns>The BaseId of the UserPermissionSet that the user belongs to</returns>
    BaseId FindUserPermissionSetIdUsingUserId(BaseId a_userId);

    /// <summary>
    /// Gets the user's ScenarioStartupPreferenceType (which determines which scenarios are loaded when they login) and
    /// the user's last active Scenario Id.
    /// </summary>
    /// <param name="a_userId"></param>
    /// <returns></returns>
    (PTUserScenarioStartupPreferences.EScenarioStartupPreferenceType, BaseId) GetUserScenarioStartupInformation(BaseId a_userId);


    string GetUserName(BaseId a_userId);

    bool IsUserAdmin(BaseId a_userId);

    /// <summary>
    /// Checks if the user's UserPermissionSet has CanManageScenarios set to true, which
    /// means that users assigned to said UserPermissionSet has access to all scenarios
    /// </summary>
    /// <param name="a_userId"></param>
    /// <returns></returns>
    bool CanUserManageAllScenarios(BaseId a_userId);
}