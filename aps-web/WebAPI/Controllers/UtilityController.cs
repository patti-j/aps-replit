using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using WebAPI.DAL;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CompanyDBService _dbService;
        private const string AccessTokenHeader = "Access-Token";

        public UtilityController(IConfiguration configuration, CompanyDBService service)
        {
            _configuration = configuration;
            _dbService = service;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> Ping()
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> ValidateInstallCode([FromBody]string code)
        {
            if (code.IsNullOrEmpty())
            {
                return BadRequest("Install Code must be provided");
            }

            try
            {
                var status = _dbService.ValidateInstallCode(code);
                if (status == CompanyDBService.CodeStatus.Valid)
                {
                    return Ok();
                } else if(status == CompanyDBService.CodeStatus.Invalid)
                {
                    return NotFound("The provided code has already been used.");
                }
                else if (status == CompanyDBService.CodeStatus.Expired)
                {
                    return NotFound("The provided code has expired.");
                }
                else
                {
                    return NotFound("The provided code was not found.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        

        [HttpPost("[action]")]
        public async Task<ActionResult> ActivateInstallCode(ActivateCodeMessage code)
        {
            if (code.Code.IsNullOrEmpty())
            {
                return BadRequest("Install Code must be provided");
            }
            try
            {
                if (_dbService.ActivateInstallCode(code))
                {
                    return Ok();
                }
                else
                {
                    return Unauthorized("The code provided is not valid.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
