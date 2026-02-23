using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using ReportsWebApp.Controllers.Models;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services;
using Microsoft.AspNetCore.SignalR;
using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.Hubs;
using ReportsWebApp.Shared;

using Newtonsoft.Json;

namespace ReportsWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly AzureBlobService _blobService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IHubContext<PAStatusHub> _paHubContext;
        private readonly IAppInsightsLogger _logger;
        private const string AccessTokenHeader = "Access-Token";

		public NotificationController(IConfiguration configuration, INotificationService notificationService, IUserService userService, IHubContext<NotificationHub> hubContext, IHubContext<PAStatusHub> paHubContext, IAppInsightsLogger logger, AzureBlobService blobService)
        {
            _configuration = configuration;
            _notificationService = notificationService;
            _userService = userService;
            _hubContext = hubContext;
            _paHubContext = paHubContext;
            _logger = logger;
            _blobService = blobService;
        }
        [HttpPost("[action]")]
        public async Task<ActionResult> Add(string userEmail, string text, string type)
        {
            try
            {

	            if (!Request.Headers.TryGetValue(AccessTokenHeader, out var token) || token != _configuration["NotificationEndpointAccessToken"])
	            {
		            return Unauthorized();
	            }
	            var user = _userService.GetUserByEmail(userEmail);
	            if (!user.Exists())
	            {
		            return BadRequest("User Email not found.");
	            }

				await _notificationService.AddAsync(new Notification()
	            {
                    UserId = user.Id,
                    Text = text,
                    Type = (ENotificationType)int.Parse(type),
                    CreationDate = DateTime.UtcNow,
                    Name = "",
                    Read = false,
                    Deleted = false
	            });

				await _hubContext.Clients.Group(userEmail).SendAsync("NewNotification");
			}
            catch(Exception ex)
            {
                _logger.LogError(ex, userEmail);
                return BadRequest();
            }
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Notify(string userEmail)
        {
	        if (!Request.Headers.TryGetValue(AccessTokenHeader, out var token) || token != _configuration["NotificationEndpointAccessToken"])
	        {
		        return Unauthorized();
	        }
			try
	        {
		        var user = _userService.GetUserByEmail(userEmail);
		        if (!user.Exists())
		        {
			        return BadRequest("User with that Email not found.");
		        }

		        await _hubContext.Clients.Group(userEmail).SendAsync("NewNotification");
	        }
	        catch (Exception ex)
	        {
                _logger.LogError(ex, userEmail);
                return BadRequest();
	        }
	        return Ok();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> NotifyClientLaunchStatus(string id, ELaunchStatus status, string userEmail)
        {
            try
            {
                await _hubContext.Clients.Group(userEmail).SendAsync("LaunchStatusUpdate", arg1: id, arg2: status);
                return Ok();
            } catch (Exception ex)
            {
                _logger.LogError(ex, userEmail);
                return BadRequest();
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> GetClientAgent(string version)
        {
            if (!Version.TryParse(version, out _))
            {
                return BadRequest("Please provide a 4 part version in the form 'x.x.x.x'");
            }

            try
            {
                Stream agentZip = await _blobService.GetClientAgentZip(version);

                if (agentZip == null)
                {
                    return BadRequest($"ClientAgent version {version} does not exist.");
                }

                return File(agentZip, "application/octet-stream", "PlanetTogetherClientAgent.zip"); // Return stream directly, with suggested filename
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured on endpoint '{nameof(GetClientAgent)}'");
                return Problem("An error occured during the request.");
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> StatusUpdate(string id, string userEmail, double progress, string message)
        {
            if (!Request.Headers.TryGetValue(AccessTokenHeader, out var token) || token != _configuration["NotificationEndpointAccessToken"])
            {
                return Unauthorized();
            }
            try
            {
                await _hubContext.Clients.Group(userEmail).SendAsync("UploadProgressUpdate", arg1: id, arg2: progress, arg3: message);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, userEmail);
                return BadRequest();
            }
        }
        public record EventLogEntryAdapter(string Category, short CategoryNumber, string Message, string Source, DateTime TimeGenerated);
        public record PlanningAreaGetLogsUpdateRequest(string TransactionId, List<EventLogEntryAdapter> Events);
        [HttpPost("[action]")]
        public async Task<ActionResult> LogsUpdate([FromBody] string data)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var token) || token != _configuration["NotificationEndpointAccessToken"])
            {
                return Unauthorized();
            }
            try
            {
                var statuses = JsonConvert.DeserializeObject<PlanningAreaGetLogsUpdateRequest>(data);
                await _hubContext.Clients.Group(statuses.TransactionId).SendAsync("Events", arg1: statuses.Events);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest();
            }
        }
    }
}
