using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebAPI.DAL;
using WebAPI.Models;
using WebAPI.Models.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private const string c_API_KEY = "Pl@n3t10geth3r";
        
        private readonly CompanyDBContext _context;
        private readonly ILogger<CompaniesController> _logger;
        private readonly IConfiguration _configuration;

        public CompaniesController(
            CompanyDBContext context, 
            ILogger<CompaniesController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets all companies from this application for the external license service
        /// </summary>
        /// <param name="request">Request containing authentication token</param>
        /// <returns>List of companies with basic information</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> GetCompanies()
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

            try
            {

                // Get all active companies from the database
                var companies = await _context.Companies
                    .Where(c => c.Active)
                    .Select(c => new CompanyNameDto
                    {
                        Name = c.Name ?? string.Empty,
                        Email = c.Email ?? string.Empty,
                        ContactName = string.Empty, // Company entity doesn't have ContactName field
                        IsActive = c.Active
                    })
                    .ToListAsync();

                _logger.LogInformation("Companies retrieved successfully. Count: {Count}, ClientIP: {ClientIp}", 
                    companies.Count, clientIp);

                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving companies. ClientIP: {ClientIp}, UserAgent: {UserAgent}", 
                    clientIp, userAgent);
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
    }

    /// DTO for company name information returned to license service
    /// </summary>
    public class CompanyNameDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string ContactName { get; set; }
        public bool IsActive { get; set; }
    }
}
