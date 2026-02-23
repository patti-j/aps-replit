using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;

using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Common.File;
using PT.Common.Http;
using PT.PackageDefinitions.Settings;
using PT.PlanetTogetherAPI.APIs;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.ServerManagerSharedLib.DTOs.Requests;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ServerControllerBase
{
    #region Chat
    [HttpPost]
    [Route("Chat")]
    public ApsWebServiceResponseBase Chat(ChatRequest a_request)
    {
        ApiLogger al = new ("Chat", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        BaseId instigator;
        try
        {
            //TODO: V12 permissions
            instigator = Helpers.ValidateUser(a_request.UserName, a_request.Password, new List<UserDefs.EPermissions> { UserDefs.EPermissions.MaintainJobs });
            a_request.ValidateScenarioId();
        }
        catch (WebServicesErrorException e)
        {
            return al.LogWebExceptionAndReturn("UserController.Chat", e);
        }

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendChat(a_request.TimeoutDuration, a_request.ScenarioId, a_request.ChatMessage, a_request.RecipientName, instigator));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("UserController.Chat", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }

    [HttpPost]
    [Route("SendMessage")]
    public ActionResult SendMessage(MessageRequest a_request)
    {
        try
        {
            SystemController.ServerSessionManager.SendMessageToUser(a_request);
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException("UserController.SendMessage", e);
            return BadRequest(e.Message);
        }

        return Ok();
    }
    #endregion

    #region UserInformation
    [HttpGet]
    [Route("GetLoggedInUserData")]
    public ActionResult<ConnectedUserData[]> GetLoggedInUserData()
    {
        ApiLogger al = new ("GetLoggedInUserData", ControllerProperties.ApiDiagnosticsOn, TimeSpan.FromSeconds(30));
        al.LogEnter();

        ConnectedUserData[] activeUsers = SystemController.ServerSessionManager.GetLoggedInUserData();

        al.LogFinishAndReturn(EApsWebServicesResponseCodes.Success);
        return Ok(activeUsers);
    }

    [HttpGet]
    [Route("GetAdminUser")]
    public ApsWebServiceResponseBase GetAdminUser()
    {
        ApiLogger al = new ("GetAdminUser", ControllerProperties.ApiDiagnosticsOn, TimeSpan.FromSeconds(30));
        al.LogEnter();

        using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            foreach (User user in um)
            {
                if (user.Name.ToLower() == "admin")
                {
                    al.LogFinishAndReturn(EApsWebServicesResponseCodes.Success);
                    return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
                }
            }
        }

        al.LogFinishAndReturn(EApsWebServicesResponseCodes.Failure);
        return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure);
    }

    [HttpPost]
    [Route("CreateAdminUser")]
    public ApsWebServiceResponseBase CreateAdminUser(ApsWebServiceRequestBase a_request)
    {
        ApiLogger al = new ("CreateAdminUser", ControllerProperties.ApiDiagnosticsOn, TimeSpan.FromSeconds(30));
        al.LogEnter();

        string userPermissionGroup = "";
        string plantPermissionGroup = "";

        using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            foreach (UserPermissionSet userPermissionSet in um.GetUserPermissionSets())
            {
                if (userPermissionSet.AdministerUsers)
                {
                    userPermissionGroup = userPermissionSet.Name;
                    break;
                }
            }

            foreach (PlantPermissionSet plantPermissionSet in um.GetPlantPermissionSets())
            {
                if (plantPermissionSet.AutoGrantNewPermissions)
                {
                    plantPermissionGroup = plantPermissionSet.Name;
                    break;
                }
            }
        }

        bool didSucceed = new UserHelper().CreateUser(UserHelper.EAddedUserType.AdminUser, null, null, userPermissionGroup, plantPermissionGroup);

        ApsWebServiceResponseBase userAddedResponse = didSucceed ? new ApsWebServiceResponseBase { ResponseCode = EApsWebServicesResponseCodes.Success } : new ApsWebServiceResponseBase { ResponseCode = EApsWebServicesResponseCodes.ProcessingTimeout };

        al.LogFinishAndReturn(userAddedResponse.ResponseCode);
        return userAddedResponse;
    }

    //This needs to be done on the Web APP
    //[HttpPost]
    //[Route("CreateAppUser")]
    //public ActionResult<BoolResponse> CreateAppUser(AddAppUserRequest a_request)
    //{
    //    ApiLogger al = new ("CreateAppUser", ControllerProperties.ApiDiagnosticsOn, TimeSpan.FromSeconds(30));
    //    al.LogEnter();

    //    //Check if this AppUser account already exists
    //    using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
    //    {
    //        foreach (User existingUser in um)
    //        {
    //            if (existingUser.AppUser && existingUser.Name == a_request.AppUserName)
    //            {
    //                al.LogFinishAndReturn(EApsWebServicesResponseCodes.AppUserExists);
    //                return Ok(new BoolResponse { Content = false });
    //            }
    //        }
    //    }

    //    bool didSucceed = new UserHelper().CreateUser(UserHelper.EAddedUserType.AppUser, a_request.AppUserName, a_request.AppUserPassword);

    //    ApsWebServiceResponseBase userAddedResponse = didSucceed ? new ApsWebServiceResponseBase { ResponseCode = EApsWebServicesResponseCodes.Success } : new ApsWebServiceResponseBase { ResponseCode = EApsWebServicesResponseCodes.ProcessingTimeout };

    //    al.LogFinishAndReturn(userAddedResponse.ResponseCode);
    //    if (userAddedResponse.ResponseCode == EApsWebServicesResponseCodes.ProcessingTimeout)
    //    {
    //        return ValidationProblem("System was busy, unable to create the AppUser");
    //    }

    //    return Ok(new BoolResponse { Content = true });
    //}

    [HttpGet]
    [Route("EnableJitLoginForUser")]
    public ActionResult EnableJitLoginForUser(string a_token)
    {
        try
        {
            SystemController.ServerSessionManager.EnableJitLoginForUser(a_token);
            return Ok();
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException("EnableJitLoginForUser", e);
            return BadRequest(e.Message);
        }
    }
    #endregion
}