using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Common.File;
using PT.Common.Http;
using PT.Common.Sql;
using PT.Scheduler;
using PT.ServerManagerAPIProxy.APIClients;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.DTOs.Requests;
using PT.ServerManagerSharedLib.DTOs.Responses;
using PT.ServerManagerSharedLib.Exceptions;
using PT.ServerManagerSharedLib.Helpers;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.Controllers;

/// <summary>
/// A collection endpoints intended for consumption by the Server Manager.
/// All non-public endpoints it consumes should be here (and are implemented with Server Token Authorization).
/// </summary>
[ApiController]
[Route("api/SystemServerActions")]
public class ServerActionsController : ControllerBase
{
    private ServerSessionManager m_sessionManager => SystemController.ServerSessionManager;
    private InstanceActionsClient m_systemActionsClient;

    #region Messaging
    [HttpPost]
    [Route("SendMessage")]
    [Authorize(AuthenticationSchemes = PTServerAuthSchemeOptions.SchemeName)]
    public ActionResult SendMessage(MessageRequest a_request)
    {
        SystemController.ServerSessionManager.SendMessageToUser(a_request);
        return Ok();
    }
    #endregion

    [HttpPost]
    [Route("CreateAppUser")]
    [Authorize(AuthenticationSchemes = PTServerAuthSchemeOptions.SchemeName)]
    public ActionResult<BoolResponse> CreateAppUser(AddAppUserRequest a_request)
    {
        ApiLogger al = new ("CreateAppUser", ControllerProperties.ApiDiagnosticsOn, TimeSpan.FromSeconds(30));
        al.LogEnter();

        //Check if this AppUser account already exists
        using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            foreach (User existingUser in um)
            {
                if (existingUser.AppUser && existingUser.Name == a_request.AppUserName)
                {
                    al.LogFinishAndReturn(EApsWebServicesResponseCodes.AppUserExists);
                    return Ok(new BoolResponse { Content = false });
                }
            }
        }

        bool didSucceed = new UserHelper().CreateUser(UserHelper.EAddedUserType.AppUser, a_request.AppUserName, a_request.AppUserPassword);

        ApsWebServiceResponseBase userAddedResponse = didSucceed ? new ApsWebServiceResponseBase { ResponseCode = EApsWebServicesResponseCodes.Success } : new ApsWebServiceResponseBase { ResponseCode = EApsWebServicesResponseCodes.ProcessingTimeout };

        al.LogFinishAndReturn(userAddedResponse.ResponseCode);
        if (userAddedResponse.ResponseCode == EApsWebServicesResponseCodes.ProcessingTimeout)
        {
            return ValidationProblem("System was busy, unable to create the AppUser");
        }

        return Ok(new BoolResponse { Content = true });
    }

    /// <summary>
    /// Returns simple display and ID data for all administrator users on the instance.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetAdminUsers")]
    [Authorize(AuthenticationSchemes = PTServerAuthSchemeOptions.SchemeName)]
    public ActionResult<List<UserDataResponse>> GetAdminUsers()
    {
        ApiLogger al = new ("GetAdminUsers", ControllerProperties.ApiDiagnosticsOn, TimeSpan.FromSeconds(30));
        al.LogEnter();

        List<User> admins;

        using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            admins = um.GetAdministrators();
        }

        if (!admins.Any())
        {
            return NotFound();
        }

        return admins.Select(a => new UserDataResponse
                     {
                         Id = a.Id.Value,
                         Name = a.Name
                     })
                     .ToList();
    }
    [HttpPost]
    [Route("ResetAdminPassword")]
    [Authorize(AuthenticationSchemes = PTServerAuthSchemeOptions.SchemeName)]
    public ActionResult<BoolResponse> ResetAdminPassword(SystemServiceResetAdminRequest a_request)
    {
        ApiLogger al = new("ResetAdminPassword", ControllerProperties.ApiDiagnosticsOn, TimeSpan.FromSeconds(30));
        al.LogEnter();

        BaseId adminId = BaseId.NULL_ID;
        using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            if (a_request.UserId == null)
            {
                // Legacy behavior: get first active admin
                User admin = um.GetAdministrator();
                adminId = admin.Id;
            }
            else
            {
                User admin = um.GetAdministrators()
                               .FirstOrDefault(a => a.Id.Value == a_request.UserId);
                adminId = admin?.Id ?? BaseId.NULL_ID;
            }
        }

        if (adminId == BaseId.NULL_ID)
        {
            return BadRequest("Could not reset password for provided admin Id.");
        }

        UserResetMyPasswordT t = new(adminId, a_request.NewPassword);
        SystemController.ClientSession.SendClientAction(t, true);

        al.LogFinishAndReturn(EApsWebServicesResponseCodes.SuccessWithoutValidation);
        BoolResponse bools = new();
        bools.Content = true;
        return Ok(bools);
    }

    #region Copilot
    [HttpPost]
    [Route("SaveCopilotSettings")]
    [Authorize(AuthenticationSchemes = PTServerAuthSchemeOptions.SchemeName)]
    public ActionResult<BoolResponse> SaveCopilotSettings(CoPilotSettings a_coPilotSettings)
    {
        m_sessionManager.ConstructorValues.CoPilotSettings = a_coPilotSettings;

        using (m_sessionManager.LiveSystem.ScenariosLock.EnterWrite(out ScenarioManager sm))
        {
            sm.UpdateCopilotSettings(a_coPilotSettings);
        }

        return Ok(new BoolResponse { Content = true });
    }
    #endregion

    #region Login
    [HttpGet]
    [Route("Handshake")]
    public ActionResult<byte[]> Handshake(string a_publicKey)
    {
        ServerSessionManager ptBroadcaster = SystemController.ServerSessionManager;
        byte[] encryptedKey = ptBroadcaster.Handshake(a_publicKey);
        return Ok(encryptedKey);
    }

    /// <summary>
    /// Use this to login to SystemService. Creates a connection on the server on which transmissions will
    /// be broadcasted.
    /// </summary>
    /// <param name="a_request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("ValidateAsUser")]
    public ActionResult<BoolResponse> ValidateAsUser(BasicLoginRequest a_request)
    {
        try
        {
            bool validateLogin = m_sessionManager.ValidateLogin(a_request.UserName, a_request.PasswordHash);
            return Ok(new BoolResponse { Content = validateLogin });
        }
        catch (Exception e)
        {
            string logFilePath = Path.Combine($"{PTHttpExceptionHandlingFilter.c_apiLogSubpath}/SystemServerActions/ValidateAsUser.log");
            SimpleExceptionLogger.LogException(logFilePath, e.Message);

            return NoContent(); // do not provide validation error details to user
        }
    }

    /// <summary>
    /// Use this to login to SystemService. Creates a connection on the server on which transmissions will
    /// be broadcasted.
    /// </summary>
    /// <param name="a_request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("ValidateWithToken")]
    public ActionResult<BoolResponse> ValidateWithToken(TokenLoginRequest a_request)
    {
        try
        {
            bool validateLogin = m_sessionManager.ValidateLogin(a_request.Token);
            return Ok(new BoolResponse { Content = validateLogin });
        }
        catch (Exception e)
        {
            string logFilePath = Path.Combine($"{PTHttpExceptionHandlingFilter.c_apiLogSubpath}/SystemServerActions/ValidateAsUser.log");
            SimpleExceptionLogger.LogException(logFilePath, e.Message);

            return NoContent(); // do not provide validation error details to user
        }
    }
    #endregion

    #region Instances
    [HttpGet]
    [Route("GetInstanceStatus")]
    public ActionResult<ServiceStatus> GetInstanceStatus()
    {
        ServiceStatus status = m_sessionManager.GetServiceStatus();
        return Ok(status);
    }

    [HttpGet]
    [Route("GetThumbprint")]
    public ActionResult<string> GetThumbprint()
    {
        IInstanceSettingsManager instanceSettingsManager = InstanceSettingManagerFactory.CreateInstanceSettingsManagerForInstance(m_sessionManager.ConstructorValues.InstanceDatabaseConnectionString,
            Environment.MachineName, m_sessionManager.ConstructorValues.InstanceId, m_sessionManager.ConstructorValues.ApiKey, m_sessionManager.ConstructorValues.WebAppEnv);

        string thumbprint = instanceSettingsManager.GetCertificateThumbprint(m_sessionManager.ConstructorValues.InstanceName, m_sessionManager.ConstructorValues.SoftwareVersion);

        return Ok(thumbprint);
    }

    [HttpPost]
    [Route("TestConnectionString")]
    public ActionResult<BoolResponse> TestConnectionString(TestConnectionStringRequest a_connectionStringRequest)
    {
        if (!ConnectionValidation.ValidateConnectionString(a_connectionStringRequest.ConnectionString))
        {
            return BadRequest("Invalid connection string format");
        }

        if (!ConnectionValidation.TestConnectionString(a_connectionStringRequest.ConnectionString))
        {
            return BadRequest("Invalid connection parameters.");
        }

        return Ok(new BoolResponse { Content = true });
    }

    [HttpGet]
    [Route("GetClientZipStreaming")]
    public ActionResult<byte[]> GetClientZipStreaming(string a_version)
    {
        string versionPath = Paths.GetClientPathForVersion(a_version);
        if (!System.IO.File.Exists(versionPath))
        {
            throw new ServerManagerException(string.Format("Client file for version '{0}' is missing", new object[] { a_version }));
        }

        return System.IO.File.ReadAllBytes(versionPath);
    }
    #endregion
}