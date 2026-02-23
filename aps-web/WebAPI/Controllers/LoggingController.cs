using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using PT.Common.Debugging;
using PT.Logging;
using PT.Logging.Interfaces;

using WebAPI.Services.Logging;

using WebAPI.Common;
using WebAPI.DAL;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    /*
     * This controller is meant to serve as the connection between an instance and the Webapp for logging purposes.
     * The flow of logging in the PT software goes:
     *   1. Client sends logging information to instance.
     *   2. Instance will log errors to Sentry and the Webapp.
     *     - For logging errors to the Webapp, the instance will make post requests and hit the APIs on this controller.
     *     - The instance will handle logging to Sentry (for now). 
     *   3. This controller should then route the logs to the appropriate location (likely an audit database that its connected to).
     *
     * There is some intention to eventually remove the instance, and just have each client communicate with the Webapp
     * so it's possible that in the future, the instance will not be part of the equation. For now though, the only connections requirements
     * we have for PT customers is that the clients need to be able to connect to the instance, and the instance needs to be able to
     * connect to the Webapp (or ServerManager for older versions). 
     */
    [Route("api/[controller]")]
    [ApiController]
    public class LoggingController : ControllerBase
    {
        /*  Some design notes/thoughts:
         * I'm currently including the UserId in the requests, but if we remove the instance component in the flow described above,
         * then I feel like we should be able to get the user credentials from the http connection in some way.
         *
         * Intellisense gives me an option to add the parameter to the method's route template.
         *   What does this mean? What are the benefits, and should we do this?
         *
         * I made these functions async since I don't think logging should be ran synchronously.
         * These functions also return an IActionResult since that was the structure I saw in the
         * other controllers, but I think all we need on the client (which is the instance in the current design)
         * is to know if the logging was successful or not. The instance should just log something
         * to the machine's event viewer if logging was unsuccessful. 
         */
        private readonly IConfiguration _configuration;
        private readonly CompanyDBService _dbService;
        private readonly IServiceScopeFactory _scopeFactory;

        public LoggingController(IConfiguration configuration, CompanyDBService service, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _dbService = service;
            _scopeFactory = scopeFactory;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> LogError(LogErrorRequest logErrorRequest)
        {
            
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company integrationCompany, out string instanceId))
                {
                    return Unauthorized();
                }

                // Do logging work in the background so controller can immediately return
                // TODO: Queueing to a background service would be better, so requests aren't lost if the api goes down. Going to wait on Byju's Kafka research before solidifying implementation
                Task.Run(() =>
                {
                    try
                    {
                        // Can't use injected services in the task, as they will be disposed after the method returns. 
                        // TODO: replace with BackgroundService queue if we do end up receiving all logging traffic here (vs eg kafka)
                        using var scope = _scopeFactory.CreateScope();
                        var dbService = scope.ServiceProvider.GetRequiredService<CompanyDBService>();
                        string planningAreaSettings = dbService.GetPlanningAreaSettings(instanceId);

                        if (planningAreaSettings.IsNullOrEmpty())
                        {
                            // TODO: Shouldn't happen due to authorization check, but log to some internal logging (sentry?)
                        }

                        InstanceLoggingContext instanceLoggingContext = new InstanceLoggingContext(planningAreaSettings);

                        // TODO: Manage a collection of loggers so we can group db calls rather than making 1/error
                        IErrorLogger logger = new SqlErrorLogger(instanceLoggingContext);
                        logger.LogExceptionAsync(logErrorRequest.ErrorLog);
                    }
                    catch (Exception e)
                    {
                        // TODO: log to sentry;
                        DebugException.ThrowInDebug(e.Message);
                    }
                });

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> EnsureErrorLoggerConfigured()
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company integrationCompany, out string instanceId))
                {
                    return Unauthorized();
                }
                string planningAreaSettings = _dbService.GetPlanningAreaSettings(instanceId);

                if (planningAreaSettings.IsNullOrEmpty())
                {
                    return Problem("An error occurred accessing the instance's settings /ensureErrorLoggerConfigured");
                }

                InstanceLoggingContext instanceLoggingContext = new InstanceLoggingContext(planningAreaSettings);
                IErrorLogger logger = new SqlErrorLogger(instanceLoggingContext);
                logger.EnsureErrorLogConfigured();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> LogUserAction(LogUserActionRequest logUserActionRequest)
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company integrationCompany, out string instanceId))
                {
                    return Unauthorized();
                }

                // Do logging work in the background so controller can immediately return
                // TODO: Queueing to a background service would be better, so requests aren't lost if the api goes down. Going to wait on Byju's Kafka research before solidifying implementation
                Task.Run(() =>
                {
                    try
                    {
                        // Can't use injected services in the task, as they will be disposed after the method returns. 
                        // TODO: replace with BackgroundService queue if we do end up receiving all logging traffic here (vs eg kafka)
                        using var scope = _scopeFactory.CreateScope();
                        var dbService = scope.ServiceProvider.GetRequiredService<CompanyDBService>();
                        string planningAreaSettings = dbService.GetPlanningAreaSettings(instanceId);

                        if (planningAreaSettings.IsNullOrEmpty())
                        {
                            // TODO: Shouldn't happen due to authorization check, but log to some internal logging (sentry?)
                        }

                        InstanceLoggingContext instanceLoggingContext = new InstanceLoggingContext(planningAreaSettings);

                        // TODO: Manage a collection of loggers so we can group db calls rather than making 1/error
                        IAuditLogger logger = new SqlErrorLogger(instanceLoggingContext);
                        logger.LogAuditAsync(logUserActionRequest.AuditLog);
                    }
                    catch (Exception e)
                    {
                        // TODO: log to sentry
                        DebugException.ThrowInDebug(e.Message);
                    }
                });

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> EnsureAuditLoggerConfigured()
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company integrationCompany, out string instanceId))
                {
                    return Unauthorized();
                }
                string planningAreaSettings = _dbService.GetPlanningAreaSettings(instanceId);

                if (planningAreaSettings.IsNullOrEmpty())
                {
                    return Problem("An error occurred accessing the instance's settings on endpoint /ensureAuditLoggerConfigured");
                }

                InstanceLoggingContext instanceLoggingContext = new InstanceLoggingContext(planningAreaSettings);
                IAuditLogger logger = new SqlErrorLogger(instanceLoggingContext);
                logger.EnsureAuditLogConfigured();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("[action]")]
        public async Task<ActionResult<GetLogsResponse>> GetLogs([FromQuery]GetLogsRequest getLogsRequest)
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company integrationCompany, out string instanceId))
                {
                    return Unauthorized();
                }

                string planningAreaSettings = _dbService.GetPlanningAreaSettings(instanceId);

                if (planningAreaSettings.IsNullOrEmpty())
                {
                    return Problem("An error occurred accessing the instance's settings on endpoint /getLogsRequest");
                }

                InstanceLoggingContext instanceLoggingContext = new InstanceLoggingContext(planningAreaSettings);


                string logContentsAsync = string.Empty;
                switch (getLogsRequest.ELogKind)
                {
                    case ELogKind.Error:
                        IErrorLogger errorLogger = new SqlErrorLogger(instanceLoggingContext);
                        logContentsAsync = await errorLogger.GetLogContentsAsync(getLogsRequest);
                        break;
                    case ELogKind.Audit:
                        IAuditLogger auditLogger = new SqlErrorLogger(instanceLoggingContext);
                        logContentsAsync = await auditLogger.GetLogContentsAsync(getLogsRequest);
                        break;
                }

                return new GetLogsResponse()
                {
                    LogEntries = logContentsAsync
                };
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
