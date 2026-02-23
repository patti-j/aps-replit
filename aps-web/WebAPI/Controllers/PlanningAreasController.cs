using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

using PT.Common.Http;

using WebAPI.Common;
using WebAPI.DAL;
using WebAPI.Models;
using WebAPI.Models.Integration;
using WebAPI.RequestsAndResponses;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanningAreasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CompanyDBService _dbService;
        private const string AccessTokenHeader = "Access-Token";
        private const int REPLAY_WINDOW_MINUTES = 5; // Time window for request validity

        public PlanningAreasController(IConfiguration configuration, CompanyDBService service)
        {
            _configuration = configuration;
            _dbService = service;
        }

        /// <summary>
        /// Get a planning area by Name/Version. Requires Server-Level auth, as the InstanceIdentifier does not need to be known.
        /// </summary>
        /// <param name="a_request"></param>
        /// <returns></returns>
        [HttpPost("GetSettingsByName")]
        public async Task<ActionResult<GetPlanningAreaResponse>> GetSettingsByName(GetPlanningAreaRequest a_request)
        {
            try
            {
                if (!CommonMethods.ValidateServerApiRequest(Request.Headers, _dbService, out Company _, out CompanyServer server))
                {
                    return Unauthorized();
                }

                var settingsJson = _dbService.GetPlanningAreaSettings(a_request.InstanceName, a_request.InstanceVersion, server.Id);

                if (settingsJson.IsNullOrEmpty())
                {
                    return BadRequest("Planning area not found.");
                }

                var response = new GetPlanningAreaResponse()
                {
                    Settings = settingsJson,
                    ServerWideInstanceSettings = new ServerWideInstanceSettings(server)
                };
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        /// <summary>
        /// Gets all planning area settings for a particular company/server. Requires Server-Level auth, as the InstanceIdentifier does not need to be known.
        /// </summary>
        /// <param name="a_request"></param>
        /// <returns></returns>
        [HttpPost("GetAllForServer")]
        public async Task<ActionResult<GetPlanningAreaResponse>> GetPlanningAreaSettingsForServer()
        {
            try
            {
                if (!CommonMethods.ValidateServerApiRequest(Request.Headers, _dbService, out Company _, out CompanyServer server))
                {
                    return Unauthorized();
                }

                List<PADetails> allSettingsJson = _dbService.GetAllPlanningAreasForServer(server.Id) 
                                               ?? new();

                var serverWideInstanceSettings = new ServerWideInstanceSettings(server);

                var response = new GetAllPlanningAreaSettingsResponse()
                {
                    PlanningAreaSettings = allSettingsJson.Select(pa => new GetPlanningAreaResponse()
                    {
                        Settings = pa.Settings,
                        ServerWideInstanceSettings = serverWideInstanceSettings,
                        IsActive = pa.RegistrationStatus == ERegistrationStatus.Created.ToString() || pa.RegistrationStatus == ERegistrationStatus.Deleting.ToString(),
                        IsBackup = pa.IsBackup
                    }).ToList()
                };
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Gets a planning area based on the InstanceIdentifier authorization used to make the call.
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetSettings")]
        public async Task<ActionResult<GetPlanningAreaResponse>> GetSettings()
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService,
                        out Company integrationCompany, out string planningAreaKey))
                {
                    return Unauthorized();
                }

                string settingsJson = _dbService.GetPlanningAreaSettings(planningAreaKey);

                if (settingsJson.IsNullOrEmpty())
                {
                    return BadRequest("Planning area not found.");
                }

                CompanyServer server = _dbService.GetServerSettings(planningAreaKey);

                GetPlanningAreaResponse response = new GetPlanningAreaResponse()
                {
                    Settings = settingsJson,
                    ServerWideInstanceSettings = new ServerWideInstanceSettings(server)
                };
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost("UpdateLicenseStatus")]
        public async Task<ActionResult<BoolResponse>> UpdateLicenseStatus(ELicenseStatus licenseStatus)
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService,
                        out Company integrationCompany, out string planningAreaKey))
                {
                    return Unauthorized();
                }

                var result = _dbService.UpdateLicenseStatus(planningAreaKey, licenseStatus);
                
                
                return Ok(new BoolResponse() {Content = result});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary> Necessary
        /// Validates license information for a planning area using SHA256 signature verification
        /// </summary>
        /// <param name="request">The license validation request containing planningAreaId, timestamp, nonce, and signature</param>
        /// <returns>License validation response with planning area and company information</returns>
        [HttpPost("ValidateLicense")]
        public async Task<ActionResult<LicenseValidationResponse>> ValidateLicense(
            [FromBody] LicenseValidationRequest request)
        {
            try
            {
                // Validate the request model
                if (!ModelState.IsValid)
                {
                    return Ok(new LicenseValidationResponse
                    {
                        Valid = false,
                        Reason = "Invalid request format"
                    });
                }

                // Validate PlanningAreaId is a valid integer
                if (string.IsNullOrWhiteSpace(request.PlanningAreaId) || !int.TryParse(request.PlanningAreaId, out int planningAreaId))
                {
                    return Ok(new LicenseValidationResponse
                    {
                        Valid = false,
                        Reason = "Invalid Planning Area ID format"
                    });
                }

                // Validate timestamp is within acceptable window
                var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timestampAge = currentTimestamp - request.Timestamp;

                if (timestampAge > (REPLAY_WINDOW_MINUTES * 60) || timestampAge < -(REPLAY_WINDOW_MINUTES * 60))
                {
                    return Ok(new LicenseValidationResponse
                    {
                        Valid = false,
                        Reason = "Request timestamp expired or from future"
                    });
                }

                // Get the planning area by ID
                var planningArea = await _dbService.GetPlanningAreaByIdAsync(planningAreaId);

                if (planningArea == null)
                {
                    return Ok(new LicenseValidationResponse
                    {
                        Valid = false,
                        Reason = "Planning Area not found"
                    });
                }

                // Validate the signature using the PlanningAreaKey
                var expectedSignature = CreateSha256Signature(
                    planningArea.PlanningAreaKey,
                    request.Timestamp,
                    request.Nonce);

                if (expectedSignature != request.Signature)
                {
                    return Ok(new LicenseValidationResponse
                    {
                        Valid = false,
                        Reason = "Invalid signature"
                    });
                }

                // Get company information
                var company = planningArea.Company;
                if (company == null)
                {
                    return Ok(new LicenseValidationResponse
                    {
                        Valid = false,
                        Reason = "Company not found"
                    });
                }

                // Extract the serial code from the planning area
                var serialCode = ExtractSerialCodeFromSettings(planningArea.Settings);

                if (string.IsNullOrEmpty(serialCode))
                {
                    return Ok(new LicenseValidationResponse
                    {
                        Valid = false,
                        Reason = "Serial code not found"
                    });
                }

                var response = new LicenseValidationResponse
                {
                    Valid = true,
                    PlanningAreaId = planningArea.Id,
                    CompanyId = company.Id,
                    CompanyName = company.Name,
                    SerialCode = serialCode,
                    LicenseStatus = (int)planningArea.LicenseStatus
                };

                //_logger.LogEvent($"License validated successfully for Planning Area {planningArea.Id}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Error validating license for Planning Area ID {request?.PlanningAreaId}");
                return Ok(new LicenseValidationResponse
                {
                    Valid = false,
                    Reason = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Creates a SHA256 signature for license validation
        /// </summary>
        /// <param name="planningAreaKey">The planning area key to use as the secret</param>
        /// <param name="timestamp">The timestamp</param>
        /// <param name="nonce">The nonce</param>
        /// <returns>Base64 encoded SHA256 signature</returns>
        private static string CreateSha256Signature(string planningAreaKey, long timestamp, string nonce)
        {
            var combinedString = $"{planningAreaKey}|{timestamp}|{nonce}";
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Extracts the serial code from the planning area settings JSON
        /// </summary>
        /// <param name="settings">The planning area settings as JSON string</param>
        /// <returns>The serial code if found, otherwise null</returns>
        private string ExtractSerialCodeFromSettings(string settings)
        {
            try
            {
                if (string.IsNullOrEmpty(settings))
                    return null;

                var settingsJson = JsonConvert.DeserializeObject<dynamic>(settings);
                return settingsJson?.LicenseInfo?.SerialCode?.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts the Site Id from the planning area settings JSON
        /// </summary>
        /// <param name="settings">The planning area settings as JSON string</param>
        /// <returns>The Site Id if found, otherwise null</returns>
        private string ExtractSiteIdFromSettings(string settings)
        {
            try
            {
                if (string.IsNullOrEmpty(settings))
                    return null;

                var settingsJson = JsonConvert.DeserializeObject<dynamic>(settings);
                return settingsJson?.LicenseInfo?.SiteId?.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the Site ID for a specific planning area
        /// </summary>
        /// <param name="request">The request containing the planning area ID</param>
        /// <returns>Site ID response with planning area information</returns>
        [HttpPost("GetSiteId")]
        public async Task<ActionResult<GetSiteIdResponse>> GetSiteId([FromBody] GetSiteIdRequest request)
        {
            try
            {
                // Validate the request model
                if (!ModelState.IsValid)
                {
                    return Ok(new GetSiteIdResponse
                    {
                        Success = false,
                        Reason = "Invalid request format"
                    });
                }

                // Validate PlanningAreaId is a valid integer
                if (string.IsNullOrWhiteSpace(request.PlanningAreaId) || !int.TryParse(request.PlanningAreaId, out int planningAreaId))
                {
                    return Ok(new GetSiteIdResponse
                    {
                        Success = false,
                        Reason = "Invalid Planning Area ID format"
                    });
                }

                // Get the planning area by ID
                var planningArea = await _dbService.GetPlanningAreaByIdAsync(planningAreaId);

                if (planningArea == null)
                {
                    return Ok(new GetSiteIdResponse
                    {
                        Success = false,
                        Reason = "Planning Area not found"
                    });
                }

                // Extract the site ID from the planning area settings using the existing method
                var siteId = ExtractSiteIdFromSettings(planningArea.Settings);

                if (string.IsNullOrEmpty(siteId))
                {
                    return Ok(new GetSiteIdResponse
                    {
                        Success = false,
                        Reason = "Site ID not found in planning area settings",
                        PlanningAreaId = planningArea.Id
                    });
                }

                var response = new GetSiteIdResponse
                {
                    Success = true,
                    SiteId = siteId,
                    PlanningAreaId = planningArea.Id
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new GetSiteIdResponse
                {
                    Success = false,
                    Reason = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Gets the Serial Code for a specific planning area
        /// </summary>
        /// <param name="request">The request containing the planning area ID</param>
        /// <returns>Serial Code response with planning area information</returns>
        [HttpPost("GetSerialCode")]
        public async Task<ActionResult<GetSerialCodeResponse>> GetSerialCode([FromBody] GetSerialCodeRequest request)
        {
            try
            {
                // Validate the request model
                if (!ModelState.IsValid)
                {
                    return Ok(new GetSerialCodeResponse
                    {
                        Success = false,
                        Reason = "Invalid request format"
                    });
                }

                // Validate PlanningAreaId is a valid integer
                if (string.IsNullOrWhiteSpace(request.PlanningAreaId) || !int.TryParse(request.PlanningAreaId, out int planningAreaId))
                {
                    return Ok(new GetSerialCodeResponse
                    {
                        Success = false,
                        Reason = "Invalid Planning Area ID format"
                    });
                }

                // Get the planning area by ID
                var planningArea = await _dbService.GetPlanningAreaByIdAsync(planningAreaId);

                if (planningArea == null)
                {
                    return Ok(new GetSerialCodeResponse
                    {
                        Success = false,
                        Reason = "Planning Area not found"
                    });
                }

                // Extract the serial code from the planning area settings using the existing method
                var serialCode = ExtractSerialCodeFromSettings(planningArea.Settings);

                if (string.IsNullOrEmpty(serialCode))
                {
                    return Ok(new GetSerialCodeResponse
                    {
                        Success = false,
                        Reason = "Serial code not found in planning area settings",
                        PlanningAreaId = planningArea.Id
                    });
                }

                var response = new GetSerialCodeResponse
                {
                    Success = true,
                    SerialCode = serialCode,
                    PlanningAreaId = planningArea.Id
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new GetSerialCodeResponse
                {
                    Success = false,
                    Reason = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Gets the Serial Code for a specific planning area
        /// </summary>
        /// <param name="request">The request containing the planning area ID</param>
        /// <returns>Serial Code response with planning area information</returns>
        [HttpPost("GetSerialCodeByKey")]
        public async Task<ActionResult<GetSerialCodeByKeyResponse>> GetSerialCodeByKey([FromBody] GetSerialCodeByKeyRequest request)
        {
            try
            {
                // Validate the request model
                if (!ModelState.IsValid)
                {
                    return Ok(new GetSerialCodeByKeyResponse
                    {
                        Success = false,
                        Reason = "Invalid request format"
                    });
                }

                // Validate PlanningAreaKey is provided
                if (string.IsNullOrWhiteSpace(request.PlanningAreaKey))
                {
                    return Ok(new GetSerialCodeByKeyResponse
                    {
                        Success = false,
                        Reason = "Invalid Planning Area Key format"
                    });
                }

                // Get the planning area by key
                var planningArea = await _dbService.GetPlanningAreaByKeyAsync(request.PlanningAreaKey);

                if (planningArea == null)
                {
                    return Ok(new GetSerialCodeByKeyResponse
                    {
                        Success = false,
                        Reason = "Planning Area not found"
                    });
                }

                // Extract the serial code from the planning area settings using the existing method
                var serialCode = ExtractSerialCodeFromSettings(planningArea.Settings);

                if (string.IsNullOrEmpty(serialCode))
                {
                    return Ok(new GetSerialCodeByKeyResponse
                    {
                        Success = false,
                        Reason = "Serial code not found in planning area settings",
                        PlanningAreaId = planningArea.Id
                    });
                }

                var response = new GetSerialCodeByKeyResponse
                {
                    Success = true,
                    SerialCode = serialCode,
                    PlanningAreaId = planningArea.Id
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new GetSerialCodeByKeyResponse
                {
                    Success = false,
                    Reason = "Internal server error"
                });
            }
        }


        /// <summary>
        /// Finds planning areas by the specified Site ID
        /// </summary>
        /// <param name="request">The request containing the Site ID to search for</param>
        /// <returns>Response with list of planning areas that have the specified Site ID</returns>
        [HttpPost("FindBySiteId")]
        public async Task<ActionResult<FindPlanningAreasBySiteIdResponse>> FindBySiteId([FromBody] FindPlanningAreasBySiteIdRequest request)
        {
            try
            {
                // Validate the request model
                if (!ModelState.IsValid)
                {
                    return Ok(new FindPlanningAreasBySiteIdResponse
                    {
                        Success = false,
                        Reason = "Invalid request format"
                    });
                }

                // Get planning areas with the specified Site ID
                var planningAreas = _dbService.GetPlanningAreasBySiteId(request.SiteId);

                var planningAreaInfos = new List<PlanningAreaInfo>();
                
                foreach (var pa in planningAreas)
                {
                    var siteId = ExtractSiteIdFromSettings(pa.Settings);
                    
                    planningAreaInfos.Add(new PlanningAreaInfo
                    {
                        Id = pa.Id,
                        Name = pa.Name ?? string.Empty,
                        Version = pa.Version ?? string.Empty,
                        Environment = pa.Environment ?? string.Empty,
                        CompanyId = pa.CompanyId,
                        CompanyName = pa.Company?.Name,
                        SiteId = siteId ?? string.Empty,
                        LicenseStatus = pa.LicenseStatus.ToString()
                    });
                }

                var response = new FindPlanningAreasBySiteIdResponse
                {
                    Success = true,
                    PlanningAreas = planningAreaInfos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new FindPlanningAreasBySiteIdResponse
                {
                    Success = false,
                    Reason = "Internal server error"
                });
            }
        }
    }

}