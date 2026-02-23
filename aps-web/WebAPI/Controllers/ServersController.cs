using Microsoft.AspNetCore.Mvc;

using WebAPI.Common;
using WebAPI.DAL;
using WebAPI.Models;
using WebAPI.RequestsAndResponses;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CompanyDBService _dbService;

        public ServersController(IConfiguration configuration, CompanyDBService service)
        {
            _configuration = configuration;
            _dbService = service;
        }


        [HttpPost("GetSettings")]
        public async Task<ActionResult<GetServerSettingsResponse>> GetSettings()
        {
            try
            {
                if (CommonMethods.ValidateServerApiRequest(Request.Headers, _dbService, out Company _, out CompanyServer server))
                {
                    var response = new GetServerSettingsResponse(server);
                    return Ok(response);
                }
                else if (CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company _, out string o_instanceId))
                {
                    var instanceServer = _dbService.GetServerSettings(o_instanceId);
                    var response = new GetServerSettingsResponse(instanceServer);
                    return Ok(response);
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}