
using Microsoft.AspNetCore.Mvc;
using ReportsWebApp.DB.Models;
using WebAPI.DAL;
using WebAPI.Models;
using WebAPI.RequestsAndResponses;

using UserLoginResponse = WebAPI.RequestsAndResponses.UserLoginResponse;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebAppUsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly PlanningAreaLoginService _paLoginService;
        private readonly CompanyDBService _companyDbService;
        private readonly ApiUserService _userService;

        public WebAppUsersController(IConfiguration configuration, PlanningAreaLoginService paLoginService, 
                                     CompanyDBService a_companyDBService, ApiUserService a_userService)
        {
            _configuration = configuration;
            _paLoginService = paLoginService;
            _companyDbService = a_companyDBService;
            _userService = a_userService;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetWebAppUsers(string planningAreaKey)
        {

            if (string.IsNullOrWhiteSpace(planningAreaKey))
            {
                return BadRequest("A Planning Area Key is required.");
            }

            try
            {
	            var users = await _paLoginService.GetPAUsersForPlanningAreaAsync(planningAreaKey);
                var usersDto = users.Select(user => new UserLoginResponse(user)).ToList();
                return Ok(usersDto);
			}
            catch(Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> AuthenticateWebAppUser(UserLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PlanningAreaKey))
            {
                return BadRequest("A Planning Area Key is required.");

            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                if (request.UserId == null)
                {
                    return BadRequest("A user email or Id is required.");
                }
                else
                {
                    try
                    {
                        var user = await _paLoginService.AuthenticateUserAsync(request.PlanningAreaKey, request.UserId.Value);
                        if (user == null)
                        {
                            return Unauthorized();
                        }
                        
                        return Ok(user);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest();
                    }
                }
            }

            try
            {
                var user = await _paLoginService.AuthenticateUserAsync(request.PlanningAreaKey, request.Email);
                if (user == null)
                {
                    return Unauthorized();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> CreateWebAppUsers([FromBody] CreateWebAppUsersRequest a_request)
        {
            try
            {
                if (!Request.Headers.TryGetValue("CompanyAPIKey", out var companyAPIKey) ||
                    string.IsNullOrWhiteSpace(companyAPIKey) ||
                    !Request.Headers.TryGetValue("CompanyId", out var CompanyId))
                {
                    return BadRequest("Company API Key, CompanyID (in header), are required.");
                }
                //Verify Company API Key
                if (int.TryParse(CompanyId, out int companyId))
                {
                    if (!_companyDbService.VerifyCompanyApiKey(companyId, companyAPIKey))
                        return Unauthorized("Invalid Company API Key.");
                }
                else
                {
                    return BadRequest("Invalid CompanyId in header.");
                }
                var newUsers = new List<WebAPI.Models.User>();
                //Process each user creation request
                foreach (var userCreateRequest in a_request.Users)
                {
                    if (string.IsNullOrWhiteSpace(userCreateRequest.UserType))
                        return BadRequest("UserType is required.");

                    if (userCreateRequest.UserType.Equals("Web", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrWhiteSpace(userCreateRequest.Email) ||
                            string.IsNullOrWhiteSpace(userCreateRequest.FirstName) ||
                            string.IsNullOrWhiteSpace(userCreateRequest.LastName) ||
                            !userCreateRequest.PermissionGroupNamesList.Any() ||
                            string.IsNullOrWhiteSpace(userCreateRequest.DisplayLanguage))
                        {
                            return BadRequest("For Web users,Company API Key, CompanyID (in header), Email, FirstName, LastName, PermissionGroupNamesList, DisplayLanguage, and CompressionType are required.");
                        }

                        // Assuming request.CompressionType is a string like "None", "Fast", "Normal", "High"
                        var compressionType = WebAPI.Models.ECompressionType.Normal; // Default value
                        if (userCreateRequest.CompressionType != null)
                        {
                            if (!Enum.TryParse<WebAPI.Models.ECompressionType>(userCreateRequest.CompressionType, true, out compressionType))
                            {
                                return BadRequest("Invalid CompressionType value.");
                            }
                        }
                        // Lookup permission groups by name
                        var permissionGroups = new List<WebAPI.Models.Role>();
                        foreach (var groupName in userCreateRequest.PermissionGroupNamesList)
                        {
                            permissionGroups.Add(new WebAPI.Models.Role(){Name = groupName});
                        }

                        var newUser = new WebAPI.Models.User()
                        {
                            Email = userCreateRequest.Email,
                            Name = userCreateRequest.FirstName,
                            CompanyId = companyId,
                            LastName = userCreateRequest.LastName,
                            CompressionType = compressionType,
                            DisplayLanguage = userCreateRequest.DisplayLanguage,
                            UserType = EUserType.Web,
                            Groups = permissionGroups,
                            CreationDate = DateTime.UtcNow,
                            UpdatedBy = "Web API"
                        };

                        // Validate new user
                        var userSaved = await _userService.ValidateWebUserAsync(newUser);
                        newUsers.Add(newUser);
                    }
                    else if (userCreateRequest.UserType.Equals("Service", StringComparison.OrdinalIgnoreCase))
                    {
                        return Ok(new { Message = "This feature is not available" });
                    }
                    else
                    {
                        return BadRequest("Invalid UserType. Must be 'Web' or 'Service'.");
                    }
                
                }

                foreach (var user in newUsers)
                {
                    var userSaved = await _userService.AddWebUserAsync(user);
                }
                return Ok(new { Message = "Web users created successfully." });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}
