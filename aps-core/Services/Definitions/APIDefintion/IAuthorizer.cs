using PT.APSCommon.Interfaces;

namespace PT.APIDefinitions;

/// <summary>
/// This interface provides access to functions required to Authorize a client on the server API
/// </summary>
public interface IAuthorizer
{
    bool ValidateAuthorization(string a_sessionToken);
    IUserPermissionSet GetUserPermissions(string a_sessionToken);
    bool IsAppConnection(string a_sessionToken);
    bool ValidateServerAuthorization(string a_serverToken);
}