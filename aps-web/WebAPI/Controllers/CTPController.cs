using Microsoft.AspNetCore.Mvc;

using WebAPI.DAL;
using WebAPI.Models.CTP;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CTPController : ControllerBase
    {
        private readonly ILogger<CTPController> _logger;
        private readonly CompanyDBService _companyDbService;
        private const string ApiKey = "YourPredefinedApiKey";  // Replace with actual API key

        public CTPController(ILogger<CTPController> logger, CompanyDBService companyDbService)
        {
            _logger = logger;
            _companyDbService = companyDbService;
        }

        // POST: /api/ctp/getNextRequest
        [HttpPost("getNextRequest")]
        public async Task<ActionResult<CtpResponse>> GetNextRequest([FromBody] CtpRequest ctpRequest)
        {
            if (ctpRequest == null)
            {
                _logger.LogWarning("Invalid request body received.");
                return BadRequest("Request body cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid request body received: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _companyDbService.GetNextCtpRequestAsync(ctpRequest);
                if (result == null)
                {
                    return NotFound("No new requests found.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching the next CTP request. Request data: {@CtpRequest}", ctpRequest);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // PUT: /api/ctp/updateCTPRequest
        [HttpPut("updateCTPRequest")]
        public async Task<IActionResult> UpdateCTPRequest([FromBody] List<CtpUpdateRequest> requests)
        {
            // Check if API key is valid
            if (!Request.Headers.TryGetValue("ApiKey", out var providedApiKey) || providedApiKey != ApiKey)
            {
                _logger.LogWarning("Invalid API key provided.");
                return Unauthorized(new { Message = "Invalid API Key" });
            }

            // Validate request body
            if (requests == null || !requests.Any())
            {
                _logger.LogWarning("No requests provided in the body.");
                return BadRequest("Request body cannot be null or empty.");
            }

            var responseList = new List<CtpUpdateResponse>();

            foreach (var req in requests)
            {
                try
                {
                    var result = await _companyDbService.UpdateCtpRequestAsync(req);
                    responseList.Add(new CtpUpdateResponse
                    {
                        UpdateStatus = result ? "success" : "failure",
                        Detail = result ? $"CTP request {req.RequestID} updated successfully." : $"Failed to update CTP request {req.RequestID}."
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating CTP request {req.RequestID} for CompanyId {req.CompanyId} and PAId {req.PAId}");
                    responseList.Add(new CtpUpdateResponse
                    {
                        UpdateStatus = "failure",
                        Detail = $"Error occurred while updating CTP request {req.RequestID}: {ex.Message}"
                    });
                }
            }

            return Ok(responseList);
        }
    }
}
