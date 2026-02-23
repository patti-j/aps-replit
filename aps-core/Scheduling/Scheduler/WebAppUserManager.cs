using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;
using PT.ServerManagerAPIProxy.APIClients;
using PT.SystemDefinitions.Interfaces;

namespace PT.Scheduler;

/// <summary>
/// Extended User Manager that pulls user data from the Webapp.
/// Builds atop the existing User Manager, taking any users in the scenarios file and adding/updating them from mapped WebApp users.
/// </summary>
class WebAppUserManager : UserManager
{
    private WebAppActionsClient m_webAppClient;

    public WebAppUserManager(WebAppActionsClient a_webAppClient, IReader a_reader, BaseIdGenerator a_idGen) 
        : base(a_reader, a_idGen)
    {
        m_webAppClient = a_webAppClient;
    }

    internal WebAppUserManager(WebAppActionsClient a_webAppClient, string a_instanceId, ISystemLogger erm, BaseIdGenerator idGen, string a_defaultWorkspacesPath) 
        : base(erm, idGen, a_defaultWorkspacesPath)
    {
        m_webAppClient = a_webAppClient;
    }

    internal WebAppUserManager(WebAppActionsClient a_webAppClient, ISystemLogger erm, BaseIdGenerator idGen, string a_defaultWorkspacesPath)
        : base(erm, idGen, a_defaultWorkspacesPath)
    {
        m_webAppClient = a_webAppClient;
    }

    internal override void RestoreReferences(ISystemLogger a_erm, string a_workspacesPath, PackageManager a_packageManager, UserFieldDefinitionManager a_udfManager)
    {
        // Need to set the error reporter before the ImportAllUsersFromWebApp call
        // so that errors can be logged during the function execution.
        // However, the base.RestoreReferences() below will also set m_errorReporter again
        m_errorReporter = a_erm;
        if (PTSystem.Server)
        {
            //ImportAllUsersFromWebApp();
        }
        base.RestoreReferences(a_erm, a_workspacesPath, a_packageManager, a_udfManager);
    }

    // TODO: Currently, the methods below pull all users from the webapp, but they're only going to be called server-side. Clients (other than the new logging in one) won't get updates.
    // TODO: We need to change this structure so that clients get user data by calling instance apis, and don't actually store them.

    /// <summary>
    /// Returns one user from the webapp, adds them to the user manager, and returns the local User object.
    /// This method is called as part of the provided user's SSO login attempt.
    /// </summary>
    /// <param name="a_userEmail"></param>
    /// <param name="dataChanges"></param>
    /// <returns></returns>
    public User ImportUserFromWebApp(string a_userEmail, IScenarioDataChanges dataChanges = null)
    {
        //TODO: Test what this response is if the user is not assigned to this Planning Area
        UserDto userReponse = m_webAppClient.GetUserForLogin(a_userEmail);
        User user = AddUpdateWebAppUser(userReponse, dataChanges);
        return user;
    }

    /// <summary>
    /// Pulls all users mapped to this instance from the webapp and adds them in memory.
    /// This is called at startup to pull all needed data.
    /// TODO: Perhaps we want to call this more frequently than this (aside from on user login, handled in <see cref="ImportUserFromWebApp"/>)?
    /// </summary>
    /// <param name="dataChanges"></param>
    public void ImportAllUsersFromWebApp(IScenarioDataChanges dataChanges = null)
    {
        List<UserDto> userReponse = new List<UserDto>();
        try
        {
            userReponse = m_webAppClient.GetAllUsers();
        }
        catch (PTHandleableException ex)
        {
            m_errorReporter.LogException(ex, null);
            // TODO: How else to handle? We don't want to stop the system from starting up just because the connection to the webapp failed.
        }

        foreach (UserDto userDto in userReponse)
        {
            try
            {
                User user = AddUpdateWebAppUser(userDto, dataChanges);
            }
            catch (Exception ex)
            {
                m_errorReporter.LogException(new PTHandleableException(string.Format("Error importing Webapp User {0}".Localize(), userDto.Email), ex), null);
                // Continue subsequent adds.
            }
        }
        
        //TODO: Delete users not in the response
    }

    private User AddUpdateWebAppUser(UserDto a_userDto, IScenarioDataChanges a_dataChanges)
    {
        // TODO: How best to handle matching existing users? Both the Webapp and Instance enforce unique User Names (ie emails), and this is what is checked against when logging in.
        // TODO: However, it's possible that the scenario could be storing a user with that same name but different external id, at least for as long as we support in-scenario users.
        // TODO: I think replacing the existing one and logging something is probably fine, but worth discussion.

        // TODO: **After talking with Cavan, we will use a unique identifier (but not external id, something we can use as a BaseId), and move toward Webapp-only infrastructure.
        // TODO: ** That means that we should here by ID and not email. However, for now, it's nice to be able to overwrite a scenario user with latest Webapp data.
        if (GetUserByName(a_userDto.Email) is User foundUser)
        {
            if (foundUser.ExternalId != a_userDto.ExternalId) // TODO: use new identifier mentioned above
            {
                m_errorReporter.LogException(new PTHandleableException(string.Format("Error importing Webapp User {0}".Localize(), foundUser.Name)), null);
            }

            // Refresh in case this has changed since last login
            bool changed = foundUser.Update(a_userDto, m_userPermissionSetManager);

            if (changed)
            {
                a_dataChanges?.UserChanges.UpdatedObject(foundUser.Id);
            }

            return foundUser;
        }

        // Not yet in the UM; create it
        User newUser = new(NextID(), m_userPermissionSetManager.GetDefaultUserPermissionSet().Id, m_userPermissionSetManager.GetDefaultPlantPermissionSet().Id);
        newUser.Create(a_userDto, this, m_userPermissionSetManager);
        Add(newUser);
        a_dataChanges?.UserChanges.AddedObject(newUser.Id);

        return newUser;
    }
}
