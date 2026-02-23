using Microsoft.AspNetCore.Mvc;

using PT.APIDefinitions;
using PT.PlanetTogetherAPI.Server;
using PT.Scheduler;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.Controllers;

/// <summary>
/// This is the web wrapper for the ServerSessionManager.
/// This controller will accept the web request and forward it to the ServerSessionManager and return the result if applicable
/// A client running externally would not use or create this class.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PublishController : SessionControllerBase
{
    [HttpGet]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetPublishStatus")]
    public ActionResult<PublishStatusMessage> GetPublishStatus(long scenarioId)
    {
        PublishStatusMessage status = SystemController.ServerSessionManager.GetCurrentPublishStatus(scenarioId);

        if (status == null)
        {
            return NoContent();
        }

        return Ok(status);
    }
}