using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using WebAPI.Common;
using WebAPI.DAL;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerManagerActionsController : ControllerBase
    {
        private readonly ILogger<ServerManagerActionsController> m_logger;
        private readonly ServerManagerActionDbService m_serverManagerActionDbService;
        private readonly CompanyDBService m_companyService;
        private readonly WebAppApiService m_webAppService;

        public ServerManagerActionsController(ILogger<ServerManagerActionsController> mLogger, ServerManagerActionDbService dbService, CompanyDBService mCompanyService, WebAppApiService webAppService)
        {
            m_logger = mLogger;
            m_serverManagerActionDbService = dbService;
            m_companyService = mCompanyService;
            m_webAppService = webAppService;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<WebApiAction>> GetNextRequest()
        {
            try
            {
                if (!CommonMethods.ValidateServerApiRequest(Request.Headers, m_companyService, out Company _, out CompanyServer server))
                {
                    return Unauthorized();
                }

                var result = m_serverManagerActionDbService.GetNextRequest(server.AuthToken).Result;
                if (result == null) return Ok(new WebApiAction() { TransactionId = "", Action = "NoRequests", Parameters = ""});
                var res = new WebApiAction()
                {
                    TransactionId = result.TransactionId.ToString(), 
                    Action = result.Action,
                    Parameters = result.ParameterJson
                };
                return res;
            }
            catch (Exception e)
            {
                var errorMessage = $"Error in action {nameof(GetNextRequest)}";
                m_logger.LogError(e, errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> UpdateRequest(WebApiActionFollowup update)
        {
            try
            {
                if (!CommonMethods.ValidateServerApiRequest(Request.Headers, m_companyService, out Company _, out CompanyServer server))
                {
                    return Unauthorized();
                }

                var status = await m_serverManagerActionDbService.UpdateRequest(update, server);
                SignalRMessagePublisher.SendToWebApp("ActionStatusUpdate", status);
                return Ok();
            }
            catch (Exception e)
            {
                var errorMessage = $"Error in action {nameof(UpdateRequest)} with request {JsonConvert.SerializeObject(update)}";
                m_logger.LogError(e, errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> UpdateAgentStatus(WebApiAgentStatusUpdate update)
        {
            try
            {
                if (!CommonMethods.ValidateServerApiRequest(Request.Headers, m_companyService, out Company _, out CompanyServer server))
                {
                    return Unauthorized();
                }

                await m_serverManagerActionDbService.UpdateAgentStatus(update, server);
                return Ok(update.Version.ToString());
            }
            catch (Exception e)
            {
                var errorMessage = $"Error in action {nameof(UpdateAgentStatus)} with request {JsonConvert.SerializeObject(update)}";
                m_logger.LogError(e, errorMessage);
                return BadRequest(errorMessage);
            }
        }

        // Creates a new action from the Server Manager side for the   
        [HttpPost("[action]")]
        public async Task<ActionResult> CreateNewRequest(WebApiActionFromServer newAction)
        {
            try
            {
                if (!CommonMethods.ValidateServerApiRequest(Request.Headers, m_companyService, out Company _, out CompanyServer server))
                {
                    return Unauthorized();
                }

                await m_serverManagerActionDbService.HandleSMSideRequest(newAction, server);
                return Ok();
            }
            catch (Exception e)
            {
                var errorMessage = $"Error in action {nameof(CreateNewRequest)} with request {JsonConvert.SerializeObject(newAction)}";
                m_logger.LogError(e, errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> UpdateStatuses(WebApiStatusUpdate? update)
        {
            try
            {
                var list = await m_serverManagerActionDbService.UpdateStatuses(update);
                //we are also using this status update to keep the web app informed of the status of the server agent so we always wanna send the event.
                //event not sent for 2 minutes means server agent presumed offline (by the ui, DB data not affected)
                await m_webAppService.SendStatusUpdateAsync(list);

                return Ok();
            }
            catch (Exception e)
            {
                var errorMessage = $"Error in action {nameof(UpdateStatuses)} with request {JsonConvert.SerializeObject(update)}";
                m_logger.LogError(e, errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> ServerAgentShutdown()
        {
            try
            {
                if (!CommonMethods.ValidateServerApiRequest(Request.Headers, m_companyService, out Company _, out CompanyServer server))
                {
                    return Unauthorized();
                }

                await m_webAppService.SendServerAgentShutdownAsync(server.AuthToken);

                return Ok();
            }
            catch (Exception e)
            {
                var errorMessage = $"Error in action {nameof(ServerAgentShutdown)}";
                m_logger.LogError(e, errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> UpdateLogs([FromBody]string update)
        {
            try
            {
                await m_webAppService.SendGetLogsUpdateAsync(update);

                return Ok();
            }
            catch (Exception e)
            {
                var errorMessage = $"Error in action {nameof(UpdateLogs)} with request {JsonConvert.SerializeObject(update)}";
                m_logger.LogError(e, errorMessage);
                return BadRequest(errorMessage);
            }
        }
    }
}
