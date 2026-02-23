using Microsoft.AspNetCore.Mvc;

using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.PlanetTogetherAPI.APIs;
using PT.SchedulerDefinitions;

namespace PT.PlanetTogetherAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MESController : ControllerBase
{
    #region CTP
    [HttpPost]
    [Route("CTP")]
    public CtpResponse CTP(CtpRequest a_ctpRequest)
    {
        ApiLogger al = new ("CTP", ControllerProperties.ApiDiagnosticsOn, a_ctpRequest.TimeoutDuration);
        al.LogEnter();

        BaseId instigator;
        try
        {
            List<UserDefs.EPermissions> permissions = new ();
            permissions.Add(a_ctpRequest.ReserveMaterialsAndCapacity ? UserDefs.EPermissions.CTPReserve : UserDefs.EPermissions.CTPWhatIf);
            instigator = Helpers.ValidateUser(a_ctpRequest.UserName, a_ctpRequest.Password, permissions);
        }
        catch (WebServicesErrorException wsErr)
        {
            al.LogWebExceptionAndReturn("Add,Helpers.ValidateUser", wsErr);
            return new CtpResponse
            {
                ReturnCode = wsErr.Code,
                ErrorMessage = $"Error validating username and password: {wsErr.Code}"
            };
        }

        try
        {
            WebServiceProcessors.CtpProcessor ctpProcessor = new (instigator);
            return al.LogCtpResponseAndReturn(ctpProcessor.ProcessRequest(a_ctpRequest));
        }
        catch (WebServicesErrorException wsErr)
        {
            return al.LogCtpResponseAndReturn(new CtpResponse
            {
                ReturnCode = wsErr.Code,
                ErrorMessage = $"Error processing CtpRequest: {wsErr.Code}"
            });
        }
        catch (Exception err)
        {
            return al.LogCtpResponseAndReturn(new CtpResponse
            {
                ReturnCode = EApsWebServicesResponseCodes.Failure,
                ErrorMessage = $"Error processing CtpRequest: {err}"
            });
        }
    }
    #endregion

    #region Hold
    [HttpPost]
    [Route("Hold")]
    public ApsWebServiceResponseBase Hold(HoldRequest a_request)
    {
        ApiLogger al = new ("Hold", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        if (a_request.PtObjects == null || a_request.PtObjects.Length == 0)
        {
            return al.LogFinishWithMessageAndReturn(EApsWebServicesResponseCodes.Failure, "At least one object must be specified");
        }

        BaseId instigator;
        try
        {
            instigator = Helpers.ValidateUser(a_request.UserName, a_request.Password, new List<UserDefs.EPermissions> { UserDefs.EPermissions.MaintainJobs });
            a_request.ValidateScenarioId();
        }
        catch (WebServicesErrorException e)
        {
            //Validation Failed
            return al.LogWebExceptionAndReturn("Helpers.ValidateUser", e);
        }

        try
        {
            if (a_request.Hold)
            {
                return al.LogFinishAndReturn(ActivityUpdates.SendHold(a_request.ScenarioId, a_request.TimeoutDuration, a_request.PtObjects, a_request.HoldDate, a_request.Reason, instigator, al));
            }

            //TODO:
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendHold", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }
    #endregion

    #region Lock
    [HttpPost]
    [Route("Lock")]
    public ApsWebServiceResponseBase Lock(LockRequest a_request)
    {
        ApiLogger al = new ("Lock", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        if (a_request.PtObjects == null || a_request.PtObjects.Length == 0)
        {
            return al.LogFinishWithMessageAndReturn(EApsWebServicesResponseCodes.Failure, "At least one object must be specified");
        }

        BaseId instigator;
        try
        {
            instigator = Helpers.ValidateUser(a_request.UserName, a_request.Password, new List<UserDefs.EPermissions> { UserDefs.EPermissions.MaintainJobs });
            a_request.ValidateScenarioId();
        }
        catch (WebServicesErrorException e)
        {
            return al.LogWebExceptionAndReturn("Helpers.ValidateUser", e);
        }

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendLock(a_request.ScenarioId, a_request.TimeoutDuration, a_request.PtObjects, a_request.Lock, instigator, al));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendLock", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }
    #endregion

    #region Anchor
    [HttpPost]
    [Route("Anchor")]
    public ApsWebServiceResponseBase Anchor(AnchorRequest a_request)
    {
        ApiLogger al = new ("Anchor", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        if (a_request.PtObjects == null || a_request.PtObjects.Length == 0)
        {
            return al.LogFinishWithMessageAndReturn(EApsWebServicesResponseCodes.Failure, "At least one object must be specified");
        }

        BaseId instigator;
        try
        {
            instigator = Helpers.ValidateUser(a_request.UserName, a_request.Password, new List<UserDefs.EPermissions> { UserDefs.EPermissions.MaintainJobs });
            a_request.ValidateScenarioId();
        }
        catch (WebServicesErrorException e)
        {
            return al.LogWebExceptionAndReturn("Helpers.ValidateUser", e);
        }

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendAnchor(a_request.ScenarioId, a_request.TimeoutDuration, a_request.PtObjects, a_request.Anchor, a_request.Lock, a_request.AnchorDate, instigator, al));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendAnchor", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }
    #endregion

    #region Unschedule
    [HttpPost]
    [Route("Unschedule")]
    public ApsWebServiceResponseBase Unschedule(UnscheduleRequest a_request)
    {
        ApiLogger al = new ("Unschedule", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        if (a_request.PtObjects == null || a_request.PtObjects.Length == 0)
        {
            return al.LogFinishWithMessageAndReturn(EApsWebServicesResponseCodes.Failure, "At least one object must be specified");
        }

        BaseId instigator;
        try
        {
            instigator = Helpers.ValidateUser(a_request.UserName, a_request.Password, new List<UserDefs.EPermissions> { UserDefs.EPermissions.MaintainJobs });
            a_request.ValidateScenarioId();
        }
        catch (WebServicesErrorException e)
        {
            return al.LogWebExceptionAndReturn("Helpers.ValidateUser", e);
        }

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendUnschedule(a_request.ScenarioId, a_request.TimeoutDuration, a_request.PtObjects, instigator));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendUnschedule", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }
    #endregion

    #region Move
    [HttpPost]
    [Route("Move")]
    public ApsWebServiceResponseBase Move(MoveRequest a_request)
    {
        ApiLogger al = new ("Move", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        if (a_request.PtObjects == null || a_request.PtObjects.Length == 0)
        {
            return al.LogFinishWithMessageAndReturn(EApsWebServicesResponseCodes.Failure, "At least one object must be specified");
        }

        BaseId instigator;
        try
        {
            instigator = Helpers.ValidateUser(a_request.UserName, a_request.Password, new List<UserDefs.EPermissions> { UserDefs.EPermissions.MaintainJobs });
            a_request.ValidateScenarioId();
        }
        catch (WebServicesErrorException e)
        {
            return al.LogWebExceptionAndReturn("Helpers.ValidateUser", e);
        }

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendMove(a_request.ScenarioId,
                a_request.TimeoutDuration,
                a_request.PtObjects,
                a_request.ResourceExternalId,
                a_request.MoveDateTime,
                a_request.LockMove,
                a_request.AnchorMove,
                a_request.ExpediteSuccessors,
                a_request.ExactMove,
                instigator));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendMove", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }
    #endregion

    #region Activity
    [HttpPost]
    [Route("ActivityStatusUpdate")]
    public ApsWebServiceResponseBase ActivityStatusUpdate(ActivityStatusUpdateRequest a_request)
    {
        ApiLogger al = new ("ActivityStatusUpdate", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        BaseId instigator;
        try
        {
            instigator = Helpers.ValidateUser(a_request.UserName, a_request.Password, new List<UserDefs.EPermissions> { UserDefs.EPermissions.MaintainJobs });
            a_request.ValidateScenarioId();
        }
        catch (WebServicesErrorException e)
        {
            return al.LogWebExceptionAndReturn("Helpers.ValidateUser", e);
        }

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendActivityStatusUpdate(a_request.ScenarioId, a_request.TimeoutDuration, a_request.ActivityStatusUpdates, instigator));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendActivityStatusUpdate", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }

    [HttpPost]
    [Route("ActivityQuantitiesUpdate")]
    public ApsWebServiceResponseBase ActivityQuantitiesUpdate(ActivityQuantitiesUpdateRequest a_request)
    {
        ApiLogger al = new ("ActivityQuantitiesUpdate", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        BaseId instigator;
        try
        {
            instigator = Helpers.ValidateUser(a_request.UserName, a_request.Password, new List<UserDefs.EPermissions> { UserDefs.EPermissions.MaintainJobs });
            a_request.ValidateScenarioId();
        }
        catch (WebServicesErrorException e)
        {
            return al.LogWebExceptionAndReturn("Helpers.ValidateUser", e);
        }

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendActivityQuantitiesUpdate(a_request.ScenarioId, a_request.TimeoutDuration, a_request.ActivityQuantitiesUpdates, instigator));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendActivityQuantitiesUpdate", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }

    [HttpPost]
    [Route("ActivityDatesUpdate")]
    public ApsWebServiceResponseBase ActivityDatesUpdate(ActivityDatesUpdateRequest a_request)
    {
        ApiLogger al = new ("ActivityDatesUpdate", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        BaseId instigator;
        try
        {
            instigator = Helpers.ValidateUser(a_request.UserName, a_request.Password, new List<UserDefs.EPermissions> { UserDefs.EPermissions.MaintainJobs });
            a_request.ValidateScenarioId();
        }
        catch (WebServicesErrorException e)
        {
            return al.LogWebExceptionAndReturn("Helpers.ValidateUser", e);
        }

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendActivityDatesUpdate(a_request.ScenarioId, a_request.TimeoutDuration, a_request.ActivityDatesUpdates, instigator));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendActivityDatesUpdate", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }
    #endregion
}