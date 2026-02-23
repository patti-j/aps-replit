using PT.APIDefinitions;
using PT.APSCommon.Interfaces;
using PT.Scheduler;

namespace PT.PlanetTogetherAPI.Server;

/// <summary>
/// This is an annoying workaround class to get the hosted asp.net middlewear to validate sessions.
/// </summary>
public class StaticAuthorizer : IAuthorizer
{
    public bool ValidateAuthorization(string a_sessionToken)
    {
        return SystemController.ServerSessionManager.ValidateAuthorization(a_sessionToken);
    }

    public IUserPermissionSet GetUserPermissions(string a_sessionToken)
    {
        return SystemController.ServerSessionManager.GetUserPermissions(a_sessionToken);
    }

    public bool IsAppConnection(string a_sessionToken)
    {
        return SystemController.ServerSessionManager.IsAppConnection(a_sessionToken);
    }

    public bool ValidateServerAuthorization(string a_serverToken)
    {
        return SystemController.ServerSessionManager.ValidateServerAuthorization(a_serverToken);
    }
}