using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json.Linq;

using PT.Logging;
using PT.Logging.Interfaces;

using WebAPI.Services.Logging;

using WebAPI.Common;
using WebAPI.DAL;
using WebAPI.Models.Integration;
using WebAPI.Models.V2;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanningAreaPublicApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CompanyDBService _dbService;


        public PlanningAreaPublicApiController(IConfiguration configuration, CompanyDBService service)
        {
            _configuration = configuration;
            _dbService = service;
        }

        public record PublishRequestV2(int PublishType, long ScenarioId);
        [HttpPost("Publish")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> Publish(PublishRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("Publish", request, pa);
                }
                
                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record OptimizeRequestV2(bool MRP, long ScenarioId);
        [HttpPost("Optimize")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> Optimize(OptimizeRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("Optimize", request, pa);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record AdvanceClockRequestV2(DateTime DateTime, long ScenarioId); 
        [HttpPost("AdvanceClock")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> AdvanceClock(AdvanceClockRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("AdvanceClock", request, pa);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record ImportRequestV2(string ScenarioName, bool CreateScenarioIfNew, long ScenarioId);
        [HttpPost("Import")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> Import(ImportRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("Import", request, pa);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record CopyRequestV2(string ScenarioName, bool CreateScenarioIfNew, bool IsBlackBoxScenario, double TimeoutMinutes, long ScenarioId);
        [HttpPost("CopyScenario")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> CopyScenario(CopyRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("CopyScenario", request, pa);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record DeleteRequestV2(string ScenarioName, long ScenarioId);
        [HttpPost("DeleteScenario")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> DeleteScenario(DeleteRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("DeleteScenario", request, pa);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record UndoTransmissionRequestV2(ulong TransmissionNumber, long ScenarioId);
        [HttpPost("UndoTransmission")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> UndoTransmission(UndoTransmissionRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("UndoByTransmissionNbr", request, pa);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record UndoActionsRequestV2(int NumberOfActionsToUndo, long ScenarioId);
        [HttpPost("UndoActions")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> UndoActions(UndoActionsRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("UndoActions", request, pa);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record UndoLastUserActionRequestV2(string InstigatorName, long ScenarioId);
        [HttpPost("UndoLastUserAction")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> UndoLastUserAction(UndoLastUserActionRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("UndoLastUserAction", request, pa);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record GetScenariosRequestV2(string ScenarioType, bool GetBlackBoxScenario, double TimeoutMinutes = 5);
        [HttpPost("GetScenarios")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> GetScenarios(GetScenariosRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("GetScenarios", request, pa);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record GetLastScenarioActionsRequestV2(string ScenarioType, bool GetBlackBoxScenario, double TimeoutMinutes = 5);
        [HttpPost("GetLastScenarioActions")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> GetLastScenarioActions(GetLastScenarioActionsRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("GetLastScenarioActions", request, pa, true);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public record KpiSnapshotOfLiveScenarioRequestV2(string ScenarioType, bool GetBlackBoxScenario, double TimeoutMinutes = 5);
        [HttpPost("KpiSnapshotOfLiveScenario")]
        public async Task<ActionResult<ApsWebServiceResponseBase>> KpiSnapshotOfLiveScenario(KpiSnapshotOfLiveScenarioRequestV2 request)
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    return await SendApiRequest("KpiSnapshotOfLiveScenario", request, pa);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet("GetAuditLogs")]
        public async Task<ActionResult<GetLogsResponse>> GetAuditLogs()
        {
            try
            {
                if (CommonMethods.ValidateInstancePublicApiRequest(Request.Headers, _dbService, out PADetails pa))
                {
                    string planningAreaSettings = _dbService.GetPlanningAreaSettings(pa.PlanningAreaKey);

                    if (planningAreaSettings.IsNullOrEmpty())
                    {
                        return Problem("An error occurred accessing the instance's settings on endpoint /getLogsRequest");
                    }
                    InstanceLoggingContext instanceLoggingContext = new InstanceLoggingContext(planningAreaSettings);

                    if (string.IsNullOrEmpty(instanceLoggingContext.InstanceAuditLogConnectionString))
                    {
                        return BadRequest("Unable to retrieve audit logs: Planning Area has no configured logging database.");
                    }
                    
                    string logContentsAsync = string.Empty;
                    
                    IAuditLogger auditLogger = new SqlErrorLogger(instanceLoggingContext);
                    GetLogsRequest logsRequest = new GetLogsRequest();
                    logsRequest.InstanceId = pa.PlanningAreaKey;
                    logsRequest.ELogKind = ELogKind.Audit;
                    logContentsAsync = await auditLogger.GetLogContentsAsync(logsRequest);
                    
                    return new GetLogsResponse()
                    {
                        LogEntries = logContentsAsync
                    };
                }
                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private async Task<ActionResult<ApsWebServiceResponseBase>> SendApiRequest(string endpoint, object request, PADetails? pa, bool isGet = false)
        {
            if (pa == null) throw new Exception("The Planning Area could not be found."); // Should never happen since the validation will fail
            if (pa.ApiUrl.IsNullOrEmpty()) throw new Exception("The Planning Area specified has no API URL set.");
            if (pa.ApiKey.IsNullOrEmpty()) throw new Exception("The Planning Area specified has no API Key set.");

            //Used the below code in debug because of certificate issue. 
            //var handler = new HttpClientHandler
            //{
            //    ServerCertificateCustomValidationCallback =
            //        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            //};
            //var client = new HttpClient(handler);

            var client = new HttpClient();
            client.BaseAddress = new Uri(pa.ApiUrl);
            var message = new HttpRequestMessage();
            message.Headers.Add("ApiKey", pa.ApiKey);
            message.Method = isGet ? HttpMethod.Get : HttpMethod.Post; 
            var json = JObject.FromObject(request);
            json.Add("TimeoutDuration", "00:00:05");
            var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            message.Content = content;
            message.RequestUri = new Uri(
                $"api/v2/ScenarioActionsV2/{endpoint}",
                UriKind.Relative);
            var response = await client.SendAsync(message);
            var body = await response.Content.ReadAsStringAsync();
            return Ok(body);
        }
    }
}