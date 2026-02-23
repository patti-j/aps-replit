using System.Globalization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.Http;
using PT.Scheduler;
using PT.Scheduler.Sessions;
using PT.SchedulerDefinitions.Session;
using PT.ServerManagerSharedLib.DTOs.Requests;
using PT.SystemServiceDefinitions.Headers;

namespace PT.PlanetTogetherAPI.Controllers;

/// <summary>
/// This is a web wrapper for the ServerSessionManager.
/// This controller will accept the web request and forward it to the ServerSessionManager and return the result if applicable
/// A client running externally would not use or create this class.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private ServerSessionManager SessionManager => SystemController.ServerSessionManager;
    /// <summary>
    /// Use this to login to SystemService. Creates a connection on the server on which transmissions will
    /// be broadcasted.
    /// </summary>
    /// <param name="a_request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("LoginAsUser")]
    public ActionResult<UserLoginResponse> LoginAsUser(BasicLoginRequest a_request)
    {
        try
        {
            UserSession session = SessionManager.LoginNewUserSession(a_request.UserName, a_request.PasswordHash);
            HashSet<long> loadedScenarioIdsAsLongs = new();
            foreach (BaseId scenarioId in session.LoadedScenarioIds)
            {
                loadedScenarioIdsAsLongs.Add(scenarioId.Value);
            }
            UserLoginResponse response = new()
            {
                SessionToken = session.SessionToken,
                UserId = session.UserId.ToBaseType(),
                LoadedScenarioIds = loadedScenarioIdsAsLongs
            };
            return Ok(response);
        }
        catch (FailedLogonException e)
        {
            return BadRequest(e.GetExceptionFullMessage()); //Include the message for the user.
        }
        catch (NoMoreUserLicensesException e)
        {
            return new ObjectResult(e.GetExceptionFullMessage()) { StatusCode = 403 }; //Valid credentials, but stopped by license restrictions
        }
        catch
        {
            FailedLogonException failedLogonException = new FailedLogonException("4447");
            return BadRequest(failedLogonException.GetExceptionFullMessage()); //Don't include any details, this user is not logged in.
        }
    }

    /// <summary>
    /// Use this to login to SystemService. Creates a connection on the server on which transmissions will
    /// be broadcasted.
    /// </summary>
    /// <param name="a_request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("LoginWithToken")]
    public ActionResult<UserLoginResponse> LoginWithToken(TokenLoginRequest a_request)
    {
        try
        {
            ELoginMethod tokenLoginType = string.IsNullOrEmpty(SystemController.ServerSessionManager.ConstructorValues.WebAppEnv) ? 
                ELoginMethod.Token : 
                ELoginMethod.TokenFromWebApp;

            UserSession session = SystemController.ServerSessionManager.LoginNewUserSession(a_request.Token, tokenLoginType);

            UserLoginResponse response = new ()
            {
                SessionToken = session.SessionToken,
                UserId = session.UserId.ToBaseType()
            };
            return Ok(response);
        }
        catch (FailedLogonException e)
        {
            return BadRequest(e.GetExceptionFullMessage()); //Include the message for the user.
        }
        catch (NoMoreUserLicensesException e)
        {
            return new ObjectResult(e.GetExceptionFullMessage()) { StatusCode = 403 }; //Valid credentials, but stopped by license restrictions
        }
        catch
        {
            FailedLogonException failedLogonException = new FailedLogonException("4447");
            return BadRequest(failedLogonException.GetExceptionFullMessage()); //Don't include any details, this user is not logged in.
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
            bool validateLogin = SessionManager.ValidateLogin(a_request.Token);
            return Ok(new BoolResponse { Content = validateLogin });
        }
        catch (NoMoreUserLicensesException e)
        {
            return new ObjectResult(e.GetExceptionFullMessage()) { StatusCode = 403 }; //Valid credentials, but stopped by license restrictions
        }
        catch
        {
            //TODO: Log any other reasonable, secure responses 
            return NoContent(); //Don't include any details, this user is not logged in.
        }
    }
    
    static MemoryCache UserCache = new MemoryCache(new MemoryCacheOptions());
    
    [HttpGet]
    [Route("GetWebAppUserForPlanningArea")]
    [Authorize(AuthenticationSchemes = PTSessionAuthSchemeOptions.SchemeName)]
    public ActionResult<UserDto> GetUserForPlanningArea(int a_webAppUserId)
    {
        try
        {
            UserDto dto;
            if ( (dto = (UserDto)UserCache.Get(a_webAppUserId.ToString(NumberFormatInfo.InvariantInfo))) != null)
            {
                return Ok(dto);
            }
            else
            {
                UserDto userDto = SystemController.WebAppActionsClient.GetUserForLogin(a_webAppUserId);
                UserCache.GetOrCreate<UserDto>(a_webAppUserId.ToString(NumberFormatInfo.InvariantInfo), (cacheEntry) =>
                {
                    cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                    return userDto;
                });
                return Ok(userDto);
            }
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DBIntegration".Localize(), "System".Localize(), apie));
        }
    }

    [HttpGet]
    [Route("Handshake")]
    public ActionResult<byte[]> Handshake(string a_publicKey)
    {
        ServerSessionManager ptBroadcaster = SystemController.ServerSessionManager;
        byte[] encryptedKey = ptBroadcaster.Handshake(a_publicKey);
        return Ok(encryptedKey);
    }
}