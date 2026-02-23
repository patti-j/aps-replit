using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

using WebAPI.DAL;

namespace WebAPI.Controllers;

[Route("api/powerbi/")]
[ApiController]
public class PowerBIRequestsController : ControllerBase
{
    private readonly ILogger<ImportDBController> _logger;
    private CompanyDBService m_companyDBService;
    public PowerBIRequestsController(ILogger<ImportDBController> logger, CompanyDBService companyDbService)
    {
        logger = _logger;
        m_companyDBService = companyDbService;
    }

    [HttpGet("GetCompanyBIReportNotificationGroup/v1")]
    public IActionResult GetCompanyBIReportNotificationGroupV1()
    {
        StringValues DbName;
        StringValues DbServerName;
        try
        {
            Request.Headers.TryGetValue("DbName", out DbName);
            Request.Headers.TryGetValue("DbServerName", out DbServerName);
            var res = m_companyDBService.GetBINotificationGroupUsers(DbName.ToString(), DbServerName.ToString());
            return Ok(JsonConvert.SerializeObject(res));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}

