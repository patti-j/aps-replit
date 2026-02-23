using System.Globalization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Common.Localization;
using PT.ImportDefintions.RequestsAndResponses;
using PT.PlanetTogetherAPI.APIs;
using PT.PlanetTogetherAPI.Server;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class ScenarioActionsV2Controller : ControllerBase
{
    private ServerSessionManager m_sessionManager => SystemController.ServerSessionManager;
    #region AdvanceClock
    [HttpPost]
    [Route("AdvanceClockStringDate")]
    public ApsWebServiceResponseBase AdvanceClockStringDate(AdvanceClockStringDateRequestV2 a_request)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return errorResponse;

        ApiLogger al = new ("AdvanceClockStringDate", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();
        try
        {
            DateTime parsedDate = DateTime.ParseExact(a_request.DateTime, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
            return al.LogFinishAndReturn(ClockAdvance.SendClockAdvanceT(parsedDate, instigator, al, a_request.ScenarioId));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("DateParse,SendClockAdvanceT", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }

    [HttpPost]
    [Route("AdvanceClock")]
    public ApsWebServiceResponseBase AdvanceClock(AdvanceClockRequestV2 a_request)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return errorResponse;

        ApiLogger al = new ("AdvanceClock", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        try
        {
            return al.LogFinishAndReturn(ClockAdvance.SendClockAdvanceT(a_request.DateTime, instigator, al, a_request.ScenarioId));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendClockAdvanceT", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }

    [HttpPost]
    [Route("AdvanceClockTicks")]
    public ApsWebServiceResponseBase AdvanceClockTicks(AdvanceClockTicksRequestV2 a_request)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return errorResponse;

        ApiLogger al = new ("AdvanceClockTicks", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        try
        {
            return al.LogFinishAndReturn(ClockAdvance.SendClockAdvanceT(new DateTime(a_request.DateTimeTicks), instigator, al, a_request.ScenarioId));
        }
        catch (WebServicesErrorException e)
        {
            return al.LogWebExceptionAndReturn("SendClockAdvanceT", e);
        }
    }
    #endregion

    #region Import
    [HttpPost]
    [Route("Import")]
    public async Task<ApsWebServiceResponseBase> Import(ImportRequestV2 a_request)
    {
        ApiLogger al = new ("Import", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        return await ImportAsync(a_request, al.m_uid);
    }

    [HttpPost]
    [Route("ImportAsync")]
    private async Task<ApsWebServiceResponseBase> ImportAsync(ImportRequestV2 a_request, int a_uid)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return errorResponse;

        ApiLogger al = new (a_uid, "ImportAsync", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        BaseId targetScenario = a_request.ScenarioId == long.MinValue ? BaseId.NULL_ID : new BaseId(a_request.ScenarioId);

        try
        {
            using (TriggerImport importer = new ())
            {
                string importArgs = $"Action: RunERPImport | Target Scenario: {targetScenario} | Instigator: {instigator} | Username = ";
                al.LogDiagnostic(importArgs);

                PerformImportRequest request = new ()
                {
                    TestOnly = false,
                    //UserName = a_request.UserName,
                    Instigator = instigator.Value,
                    SpecificScenarioId = targetScenario.Value
                };

                PerformImportResult importResult = SystemController.ImportingService.RunImport(request);

                if (importResult == PerformImportResult.Busy)
                {
                    string msg = Localizer.GetErrorString("2428", null, true);
                    return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
                }

                if (importResult == PerformImportResult.Failed)
                {
                    string msg = Localizer.GetErrorString("2429", null, true);
                    return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, msg);
                }

                ApsWebServiceResponseBase apsWebServiceResponseBase = new (EApsWebServicesResponseCodes.SuccessWithoutValidation, string.Empty);
                if (apsWebServiceResponseBase.ResponseCode != EApsWebServicesResponseCodes.SuccessWithoutValidation)
                {
                    return al.LogFinishAndReturn(apsWebServiceResponseBase);
                }

                if (a_request.TimeoutDuration == TimeSpan.Zero)
                {
                    return al.LogFinishAndReturn(EApsWebServicesResponseCodes.SuccessWithoutValidation);
                }

                Task awaitTask = importer.AwaitResult(a_request.TimeoutDuration);
                await awaitTask;
                if (!importer.Finished)
                {
                    if (importer.Errors == string.Empty)
                    {
                        return al.LogFinishWithMessageAndReturn(EApsWebServicesResponseCodes.SuccessWithoutValidation, "Import Request timed out");
                    }
                }

                if (importer.Errors == string.Empty)
                {
                    return al.LogFinishAndReturn(EApsWebServicesResponseCodes.Success);
                }

                return al.LogFinishWithMessageAndReturn(EApsWebServicesResponseCodes.Failure, importer.Errors);
            }
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("RunERPImport", e, EApsWebServicesResponseCodes.Failure);
        }
    }
    #endregion

    #region Optimize
    [HttpPost]
    [Route("Optimize")]
    public ApsWebServiceResponseBase Optimize(OptimizeRequestV2 a_request)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return errorResponse;

        ApiLogger al = new ("Optimize", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        try
        {
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 5000))
            {
                if (a_request.ScenarioId == long.MinValue)
                {
                    Scenario liveScenario = sm.GetFirstProductionScenario();
                    ScenarioDetailOptimizeT optimizeT = new (liveScenario.Id, null, a_request.MRP);
                    optimizeT.Instigator = instigator;

                    string broadcastArgs = $"ScenarioId: {optimizeT.scenarioId} | UseScenarioOptimizeOptions: {optimizeT.UseScenarioOptimizeSettings} | RunMRP: {optimizeT.RunMRP} | Instigator: {optimizeT.Instigator} ";
                    al.LogBroadcast("ScenarioDetailOptimizeT", broadcastArgs);
                    SystemController.ClientSession.SendClientAction(optimizeT);
                }
                else
                {
                    for (int i = 0; i < sm.LoadedScenarioCount; i++)
                    {
                        Scenario s = sm.GetByIndex(i);
                        if (s.Id.ToBaseType() == a_request.ScenarioId)
                        {
                            ScenarioDetailOptimizeT optimizeT = new (s.Id, null, a_request.MRP);
                            optimizeT.Instigator = instigator;

                            string broadcastArgs = $"ScenarioId: {optimizeT.scenarioId} | UseScenarioOptimizeOptions: {optimizeT.UseScenarioOptimizeSettings} | RunMRP: {optimizeT.RunMRP} | Instigator: {optimizeT.Instigator} ";
                            al.LogBroadcast("ScenarioDetailOptimizeT", broadcastArgs);
                            SystemController.ClientSession.SendClientAction(optimizeT);
                        }
                    }
                }

                return al.LogFinishAndReturn(EApsWebServicesResponseCodes.SuccessWithoutValidation);
            }
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("ScenarioDetailOptimizeT", e, EApsWebServicesResponseCodes.FailedToBroadcast);
        }
    }
    #endregion

    #region Publish
    [HttpPost]
    [Route("Publish")]
    public async Task<ApsWebServiceResponseBase> Publish(PublishRequestV2 a_request)
    {
        ApiLogger al = new ("Publish", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        return await PublishAsync(a_request, al.m_uid);
    }

    private async Task<ApsWebServiceResponseBase> PublishAsync(PublishRequestV2 a_request, int a_uid)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return errorResponse;

        ApiLogger al = new (a_uid, "PublishAsync", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        try
        {
            using (TriggerExport exporter = new ())
            {
                ApsWebServiceResponseBase apsWebServiceResponseBase = exporter.BeginExport(a_request.ScenarioId, (EExportDestinations)a_request.PublishType, instigator, al);
                if (apsWebServiceResponseBase.ResponseCode != EApsWebServicesResponseCodes.SuccessWithoutValidation)
                {
                    return al.LogFinishAndReturn(apsWebServiceResponseBase);
                }

                if (a_request.TimeoutDuration == TimeSpan.Zero)
                {
                    return al.LogFinishAndReturn(EApsWebServicesResponseCodes.SuccessWithoutValidation);
                }

                Task awaitTask = exporter.AwaitResult(a_request.TimeoutDuration);
                await awaitTask;
                if (!exporter.Finished)
                {
                    return al.LogFinishWithMessageAndReturn(EApsWebServicesResponseCodes.SuccessWithoutValidation, "Export Request timed out");
                }

                if (exporter.Errors == string.Empty)
                {
                    return al.LogFinishAndReturn(EApsWebServicesResponseCodes.Success);
                }

                return al.LogFinishWithMessageAndReturn(EApsWebServicesResponseCodes.Failure, exporter.Errors);
            }
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("BeginExport", e, EApsWebServicesResponseCodes.Failure);
        }
    }
    #endregion

    #region Copy
    [HttpPost]
    [Route("CopyScenario")]
    public CopyScenarioResponse CopyScenario(CopyScenarioRequestV2 a_copyScenarioRequest)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return (CopyScenarioResponse)errorResponse;

        CopyScenarioRequest copyScenarioRequest = new()
        {
            CreateScenarioIfNew = a_copyScenarioRequest.CreateScenarioIfNew,
            IsBlackBoxScenario = a_copyScenarioRequest.IsBlackBoxScenario,
            ScenarioId = a_copyScenarioRequest.ScenarioId,
            ScenarioName = a_copyScenarioRequest.ScenarioName,
            TimeoutMinutes = a_copyScenarioRequest.TimeoutMinutes
        };

        ApiLogger al = new ("CopyScenario", ControllerProperties.ApiDiagnosticsOn, a_copyScenarioRequest.TimeoutDuration);
        al.LogEnter();

        try
        {
            WebServiceProcessors.CopyScenarioProcessor copyScenarioProcessor = new (instigator);
            return al.LogCopyScenarioResponseAndReturn(copyScenarioProcessor.ProcessRequest(copyScenarioRequest));
        }
        catch (WebServicesErrorException e)
        {
            return al.LogCopyScenarioResponseAndReturn(new CopyScenarioResponse
            {
                ResponseCode = e.Code,
                ErrorMessage = $"Error processing CopyScenarioRequest: {e.Code}"
            });
        }
        catch (Exception err)
        {
            return al.LogCopyScenarioResponseAndReturn(new CopyScenarioResponse
            {
                ResponseCode = EApsWebServicesResponseCodes.Failure,
                ErrorMessage = $"Error processing CopyScenarioRequest: {err}"
            });
        }
    }
    #endregion

    #region Delete
    [HttpPost]
    [Route("DeleteScenario")]
    public DeleteScenarioResponse DeleteScenario(DeleteScenarioRequestV2 a_deleteScenarioRequest)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return (DeleteScenarioResponse)errorResponse;

        DeleteScenarioRequest deleteScenarioRequest = new()
        {
            ScenarioId = a_deleteScenarioRequest.ScenarioId,
            TimeoutMinutes = a_deleteScenarioRequest.TimeoutMinutes
        };

        ApiLogger al = new ("DeleteScenario", ControllerProperties.ApiDiagnosticsOn, a_deleteScenarioRequest.TimeoutDuration);
        al.LogEnter();
        
        try
        {
            WebServiceProcessors.DeleteScenarioProcessor deleteScenarioProcessor = new (instigator);
            return al.LogDeleteScenarioResponseAndReturn(deleteScenarioProcessor.ProcessRequest(deleteScenarioRequest));
        }
        catch (WebServicesErrorException e)
        {
            return al.LogDeleteScenarioResponseAndReturn(new DeleteScenarioResponse
            {
                ResponseCode = e.Code,
                ErrorMessage = $"Error processing deleteScenarioRequest: {e.Code}"
            });
        }
        catch (Exception err)
        {
            return al.LogDeleteScenarioResponseAndReturn(new DeleteScenarioResponse
            {
                ResponseCode = EApsWebServicesResponseCodes.Failure,
                ErrorMessage = $"Error processing deleteScenarioRequest: {err}"
            });
        }
    }
    #endregion

    #region Undo
    [HttpPost]
    [Route("UndoByTransmissionNbr")]
    public ApsWebServiceResponseBase UndoByTransmissionNbr(UndoByTransmissionNbrRequestV2 a_request)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return errorResponse;

        ApiLogger al = new ("UndoByTransmissionNbr", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendUndoByTransmissionNbr(a_request.ScenarioId, a_request.TimeoutDuration, a_request.TransmissionNbr, instigator));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendUndoByTransmissionNbr", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }

    [HttpPost]
    [Route("UndoActions")]
    public ApsWebServiceResponseBase UndoActions(UndoActionsRequestV2 a_request)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return errorResponse;

        ApiLogger al = new ("UndoActions", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendUndoActions(a_request.ScenarioId, a_request.TimeoutDuration, a_request.NbrOfActionsToUndo, instigator));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendUndoActions", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }

    [HttpPost]
    [Route("UndoLastUserAction")]
    public ApsWebServiceResponseBase UndoLastUserAction(UndoLastUserActionRequestV2 a_request)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return errorResponse;

        ApiLogger al = new ("UndoLastUserAction", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        try
        {
            return al.LogFinishAndReturn(ActivityUpdates.SendLastUserActionUndo(a_request.ScenarioId, a_request.TimeoutDuration, a_request.InstigatorName, instigator));
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("SendUndoLastUserActions", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }
    #endregion

    #region GetScenarios
    [HttpPost]
    [Route("GetScenarios")]
    public GetScenariosResponse GetScenarios(GetScenariosRequestV2 a_getScenariosRequest)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return (GetScenariosResponse)errorResponse;

        GetScenariosRequest getScenariosRequest = new()
        {
            GetBlackBoxScenario = a_getScenariosRequest.GetBlackBoxScenario,
            ScenarioType = a_getScenariosRequest.ScenarioType,
            TimeoutMinutes = a_getScenariosRequest.TimeoutMinutes
        };

        ApiLogger al = new ("GetScenarios", ControllerProperties.ApiDiagnosticsOn, a_getScenariosRequest.TimeoutDuration);
        al.LogEnter();

        if (!string.IsNullOrEmpty(a_getScenariosRequest.ScenarioType))
        {
            if (!Enum.TryParse(a_getScenariosRequest.ScenarioType, out ScenarioTypes _))
            {
                a_getScenariosRequest.ScenarioType = "";
            }
        }

        try
        {
            WebServiceProcessors.GetScenariosProcessor getScenariosProcessor = new (instigator);
            return al.LogGetScenarioResponseAndReturn(getScenariosProcessor.ProcessRequest(getScenariosRequest));
        }
        catch (WebServicesErrorException e)
        {
            return al.LogGetScenarioResponseAndReturn(new GetScenariosResponse
            {
                ResponseCode = e.Code,
                ErrorMessage = $"Error processing GetScenariosRequest: {e.Code}"
            });
        }
        catch (Exception err)
        {
            return al.LogGetScenarioResponseAndReturn(new GetScenariosResponse
            {
                ResponseCode = EApsWebServicesResponseCodes.Failure,
                ErrorMessage = $"Error processing CopyScenarioRequest: {err}"
            });
        }
    }

    [HttpGet]
    [Route("GetLastScenarioActions")]
    public GetScenarioLastActionInfoResponse GetLastScenarioActions(long a_scenarioId)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return (GetScenarioLastActionInfoResponse)errorResponse;

        ApiLogger al = new("GetLastScenarioActions", ControllerProperties.ApiDiagnosticsOn, TimeSpan.FromTicks(500000));
        al.LogEnter();

        try
        {
            WebServiceProcessors.GetScenarioInformationProcessor getScenariosProcessor = new();
            return al.LogGetScenarioInfoResponseAndReturn(getScenariosProcessor.BuildLastActionInfo(new BaseId(a_scenarioId)));
        }
        catch (WebServicesErrorException e)
        {
            return al.LogGetScenarioInfoResponseAndReturn(new GetScenarioLastActionInfoResponse
            {
                LastActionInfo = "Error retrieving last action performed in this scenario",
                ResponseCode = e.Code,
                HasLastActions = false,
                FullExceptionText = $"Error processing GetLastScenarioActions: {e.Code}"
            });
        }
        catch (Exception err)
        {
            return al.LogGetScenarioInfoResponseAndReturn(new GetScenarioLastActionInfoResponse()
            {
                LastActionInfo = "Error retrieving last action performed in this scenario.",
                ResponseCode = EApsWebServicesResponseCodes.Failure,
                HasLastActions = false,
                FullExceptionText = $"Error retrieving last action performed in this scenario: {err}"
            });
        }
    }
    #endregion

    #region KPI
    [HttpPost]
    [Route("KpiSnapshotOfLiveScenario")]
    public ApsWebServiceResponseBase KpiSnapshotOfLiveScenario(KpiSnapshotOfLiveScenarioRequestV2 a_request)
    {
        var (isValid, instigator, errorResponse) = Helpers.TryGetInstigatorFromHeaders(HttpContext.Request);
        if (!isValid)
            return errorResponse;

        ApiLogger al = new ("KpiSnapshotOfLiveScenario", ControllerProperties.ApiDiagnosticsOn, a_request.TimeoutDuration);
        al.LogEnter();

        try
        {
            KpiSnapshotOfLiveScenarioT kpiSnapshotT = new();
            kpiSnapshotT.Instigator = instigator;

            string broadcastArgs = $"Instigator: {instigator}";
            al.LogBroadcast("KpiSnapshotOfLiveScenarioT", broadcastArgs);
            SystemController.ClientSession.SendClientAction(kpiSnapshotT);

            return al.LogFinishAndReturn(EApsWebServicesResponseCodes.SuccessWithoutValidation);
        }
        catch (Exception e)
        {
            return al.LogExceptionAndReturn("KpiSnapshotOfLiveScenarioT", e, EApsWebServicesResponseCodes.InvalidDateFormat);
        }
    }
    #endregion
}
