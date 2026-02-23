using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Models.DTOs;
using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.Shared;

using System.Text;

namespace ReportsWebApp.DB.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IAppInsightsLogger _logger;

        public SubscriptionService(
            IDbContextFactory<DbReportsContext> dbContextFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IAppInsightsLogger logger)
        {
            _dbContextFactory = dbContextFactory;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<SubscriptionInfo>> GetSubscriptionsBySerialCodeAsync(string serialCode)
        {
            try
            {
                // Get the API endpoint and token from configuration
                var licenseServiceBaseUrl = _configuration["LicenseService:BaseUrl"];
                var subscriptionApiEndpoint = _configuration["SubscriptionApiEndpoint"];
                var apiEndpoint = licenseServiceBaseUrl?.TrimEnd('/') + subscriptionApiEndpoint;
                var apiToken = _configuration["LicenseService:APISecret"];

                if (string.IsNullOrEmpty(apiToken))
                {
                    var error = new InvalidOperationException("License service API secret not configured");
                    _logger.LogError(error, "SubscriptionService");
                    return new List<SubscriptionInfo>();
                }

                if (string.IsNullOrEmpty(licenseServiceBaseUrl) || string.IsNullOrEmpty(subscriptionApiEndpoint))
                {
                    var error = new InvalidOperationException("License service configuration incomplete");
                    _logger.LogError(error, "SubscriptionService");
                    return new List<SubscriptionInfo>();
                }

                // Prepare the request
                var request = new SerialCodeRequest
                {
                    SerialCode = serialCode,
                    Token = apiToken
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Make the API call
                var response = await _httpClient.PostAsync(apiEndpoint, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var licenses = JsonConvert.DeserializeObject<List<LicenseRow>>(responseContent);

                // Convert to SubscriptionInfo objects
                var subscriptions = licenses?.Select(license => new SubscriptionInfo
                {
                    SubscriptionId = license.SubscriptionId,
                    Name = license.Name,
                    Expiration = license.Expiration,
                    SerialCode = license.SerialCode,
                    Edition = license.Edition,
                    Description = license.Description,
                    LicenseServiceUrl = CreateLicenseServiceUrl(_configuration["LicenseService:BaseUrl"], license.CompanyName)
                }).ToList() ?? new List<SubscriptionInfo>();

                return subscriptions;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "SubscriptionService");
                throw new InvalidOperationException($"Failed to retrieve subscriptions: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubscriptionService");
                throw;
            }
        }

        public async Task<List<SubscriptionInfo>> GetLiveSubscriptionsForCompanyAsync(int companyId)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            try
            {
                // Get the company's subscription configuration and company information
                var subscriptionInfo = await dbContext.CompanySubscriptionInfo
                    .Include(s => s.Company)
                    .FirstOrDefaultAsync(s => s.CompanyId == companyId);

                if (subscriptionInfo == null || string.IsNullOrEmpty(subscriptionInfo.SerialCode))
                {
                    _logger.LogEvent("No subscription configuration found for company", "SubscriptionService",
                        new KeyValuePair<string, string>("CompanyId", companyId.ToString()));
                    return new List<SubscriptionInfo>();
                }

                // Get live subscription data from external service
                // License service URLs are now set using company names from the license service response
                var subscriptions = await GetSubscriptionsBySerialCodeAsync(subscriptionInfo.SerialCode);

                _logger.LogEvent("Live subscriptions retrieved for company", "SubscriptionService",
                    new KeyValuePair<string, string>("CompanyId", companyId.ToString()),
                    new KeyValuePair<string, string>("SubscriptionCount", subscriptions.Count.ToString()));

                return subscriptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubscriptionService");
                throw;
            }
        }
        
        public async Task<List<SubscriptionInfo>> GetLiveSubscriptionsForServerAsync(int serverId)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            CompanyServer? server = dbContext.CompanyServers.Include(x => x.UsingCompanies).FirstOrDefault(x => x.Id == serverId);
            List<SubscriptionInfo> subscriptions = new List<SubscriptionInfo>();
            foreach (ServerUsingCompany company in server.UsingCompanies)
            {
                int companyId = company.CompanyId;
                try
                {
                    // Get the company's subscription configuration and company information
                    var subscriptionInfo = await dbContext.CompanySubscriptionInfo
                                                          .Include(s => s.Company)
                                                          .FirstOrDefaultAsync(s => s.CompanyId == companyId);

                    if (subscriptionInfo == null || string.IsNullOrEmpty(subscriptionInfo.SerialCode))
                    {
                        _logger.LogEvent("No subscription configuration found for company", "SubscriptionService",
                            new KeyValuePair<string, string>("CompanyId", companyId.ToString()));
                        return new List<SubscriptionInfo>();
                    }

                    // Get live subscription data from external service
                    // License service URLs are now set using company names from the license service response
                    subscriptions.AddRange(await GetSubscriptionsBySerialCodeAsync(subscriptionInfo.SerialCode));

                    _logger.LogEvent("Live subscriptions retrieved for company", "SubscriptionService",
                        new KeyValuePair<string, string>("CompanyId", companyId.ToString()),
                        new KeyValuePair<string, string>("SubscriptionCount", subscriptions.Count.ToString()));

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SubscriptionService");
                    throw;
                }
            }
            
            return subscriptions;
        }

        /// <summary>
        /// Creates a license service URL with company name parameter in the format:
        /// http://external-license-service-url/subscriptions?companyName=CompanyA
        /// </summary>
        /// <param name="baseUrl">The base license service URL</param>
        /// <param name="companyName">The company name to include in the URL</param>
        /// <returns>The formatted URL</returns>
        private string CreateLicenseServiceUrl(string baseUrl, string companyName)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return string.Empty;

            // Remove trailing slash if present
            baseUrl = baseUrl.TrimEnd('/');
            
            // URL encode the company name to handle special characters and spaces
            var encodedCompanyName = Uri.EscapeDataString(companyName);
            
            return $"{baseUrl}/subscriptions?companyName={encodedCompanyName}";
        }

        public async Task<CompanySubscriptionInfo> UpdateCompanySubscriptionInfoAsync(int companyId, string serialCode)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            try
            {
                // Check if company exists
                var company = await dbContext.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
                if (company == null)
                {
                    throw new ArgumentException($"Company with ID {companyId} not found.");
                }

                // Get existing subscription info or create new one
                var subscriptionInfo = await dbContext.CompanySubscriptionInfo
                    .FirstOrDefaultAsync(s => s.CompanyId == companyId);

                if (subscriptionInfo == null)
                {
                    subscriptionInfo = new CompanySubscriptionInfo
                    {
                        CompanyId = companyId,
                        Name = $"Subscription Configuration for {company.Name}",
                        CreatedBy = "SubscriptionService",
                        CreationDate = DateTime.UtcNow
                    };
                    dbContext.CompanySubscriptionInfo.Add(subscriptionInfo);
                }

                // Update the configuration
                subscriptionInfo.SerialCode = serialCode;

                await dbContext.SaveChangesAsync();

                _logger.LogEvent("Company subscription configuration updated", "SubscriptionService",
                    new KeyValuePair<string, string>("CompanyId", companyId.ToString()),
                    new KeyValuePair<string, string>("SerialCode", serialCode));

                return subscriptionInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubscriptionService");
                throw;
            }
        }

        public async Task<CompanySubscriptionInfo> GetCompanySubscriptionInfoAsync(int companyId)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            return await dbContext.CompanySubscriptionInfo
                .Include(s => s.Company)
                .FirstOrDefaultAsync(s => s.CompanyId == companyId);
        }

        public async Task<bool> RemoveCompanySubscriptionInfoAsync(int companyId)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            try
            {
                var subscriptionInfo = await dbContext.CompanySubscriptionInfo
                    .FirstOrDefaultAsync(s => s.CompanyId == companyId);

                if (subscriptionInfo == null)
                {
                    _logger.LogEvent("Subscription configuration not found for removal", "SubscriptionService",
                        new KeyValuePair<string, string>("CompanyId", companyId.ToString()));
                    return false;
                }

                dbContext.CompanySubscriptionInfo.Remove(subscriptionInfo);
                await dbContext.SaveChangesAsync();

                _logger.LogEvent("Company subscription configuration removed", "SubscriptionService",
                    new KeyValuePair<string, string>("CompanyId", companyId.ToString()));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubscriptionService");
                throw;
            }
        }
    }
}