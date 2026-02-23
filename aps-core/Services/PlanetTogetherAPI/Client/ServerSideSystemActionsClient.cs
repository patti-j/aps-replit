using PT.APIDefinitions.RequestsAndResponses;
using PT.Common.Http;
using PT.ServerManagerSharedLib.DTOs.Requests;

namespace PT.PlanetTogetherAPI.Client;

public class ServerSideSystemActionsClient : PTHttpClient
{
    public ServerSideSystemActionsClient(string a_instanceName, string a_instanceVersion, string a_systemServiceUri) : base(
        "api/SystemService/",
        new InstanceKey(a_instanceName, a_instanceVersion),
        a_systemServiceUri) { }

    public ValidatePasswordResponse ValidatePassword(string a_password)
    {
        ValidatePasswordRequest request = new ("", a_password);
        return MakePostRequest<ValidatePasswordResponse>("ValidatePassword", request);
    }

    public ValidateCredentialsResponse ValidateCredentials(string a_username, string a_password)
    {
        ValidateCredentialsRequest request = new (a_username, a_password);
        return MakePostRequest<ValidateCredentialsResponse>("ValidateCredentials", request);
    }

    public ApsWebServiceResponseBase ResetCredentials(long a_userIdVal, string a_currentPassword, string a_newPassword, bool a_resetPwdOnNextLogin, int a_port)
    {
        ResetCredentialsRequest request = new (a_userIdVal, a_currentPassword, a_newPassword, a_resetPwdOnNextLogin, "");
        return MakePostRequest<ApsWebServiceResponseBase>("ResetCredentials", request);
    }

    public ApsWebServiceResponseBase GetAdminUser()
    {
        return MakeGetRequest<ApsWebServiceResponseBase>("GetAdminUser");
    }

    public ApsWebServiceResponseBase CreateAdminUser(long a_userIdVal, string a_currentPassword, string a_newPassword, bool a_resetPwdOnNextLogin, int a_port)
    {
        ApsWebServiceRequestBase request = new ()
        {
            UserName = "",
            Password = ""
        };
        return MakePostRequest<ApsWebServiceResponseBase>("CreateAdminUser", request);
    }
}