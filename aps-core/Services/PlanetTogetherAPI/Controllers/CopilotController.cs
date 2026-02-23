using Microsoft.AspNetCore.Mvc;

using PT.APIDefinitions;
using PT.Common.File;
using PT.Common.Http;
using PT.PlanetTogetherAPI.Server;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.Definitions;

//TODO: Return not found on nulls
namespace PT.PlanetTogetherAPI.Controllers;

/// <summary>
/// This is the web wrapper for the ServerSessionManager.
/// This controller will accept the web request and forward it to the ServerSessionManager and return the result if applicable
/// A client running externally would not use or create this class.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CopilotController : SessionControllerBase
{
    //TODO: Take ServerSessionManager as a parameter from the middlewears
    private ServerSessionManager SessionManager => SystemController.ServerSessionManager;

    [HttpGet]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetCopilotSettings")]
    public ActionResult<CoPilotSettings> GetCopilotSettings()
    {
        return SessionManager.ConstructorValues.CoPilotSettings;
    }

    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("SaveCopilotSettings")]
    public ActionResult<BoolResponse> SaveCopilotSettings(CoPilotSettings a_coPilotSettings)
    {
        SessionManager.ConstructorValues.CoPilotSettings = a_coPilotSettings;

        using (SessionManager.LiveSystem.ScenariosLock.EnterWrite(out ScenarioManager sm))
        {
            sm.UpdateCopilotSettings(a_coPilotSettings);
        }

        return Ok(new BoolResponse { Content = true });
    }

    [HttpGet]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetRuleSeekStatus")]
    public ActionResult<RuleSeekDiagnositcs> GetRuleSeekStatus()
    {
        RuleSeekDiagnositcs status = SessionManager.GetRuleSeekDiagnostics().Result;

        return Ok(status);
    }

    // Copy and pasted from the "GetRuleSeekStatus" route with modifications
    [HttpGet]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetInsertJobsStatus")]
    public ActionResult<InsertJobsDiagnostics> GetInsertJobsStatus()
    {
        try
        {
            InsertJobsDiagnostics status = SessionManager.GetInsertJobDiagnostics().Result;

            return Ok(status);
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException("GetInsertJobsStatus", e);
            return BadRequest(e.Message);
        }
    }
}