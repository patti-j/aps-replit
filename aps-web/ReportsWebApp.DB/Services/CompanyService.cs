using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Models.DTOs;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Services;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using ReportsWebApp.DB.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;
using ReportsWebApp.Shared;

public class CompanyService : ICompanyService
{
    private readonly IDbContextFactory<DbReportsContext> _factory;
    private readonly IPermissionService _permissionService;
    private readonly IConfiguration _config;
    private readonly ISubscriptionService _subscriptionService;
    private readonly HttpClient _httpClient;
    private readonly IAppInsightsLogger _logger;

    public CompanyService(
        IDbContextFactory<DbReportsContext> factory, 
        IPermissionService permissionService, 
        IConfiguration configuration, 
        ISubscriptionService subscriptionService,
        HttpClient httpClient,
        IAppInsightsLogger logger)
    {
        _factory = factory;
        _permissionService = permissionService;
        _config = configuration;
        _subscriptionService = subscriptionService;
        _httpClient = httpClient;
        _logger = logger;
    }

    private DbReportsContext GetDbContext()
    {
        return _factory.CreateDbContext();
    }
    public async Task<List<Company>> GetCompaniesAsync()
    {
        using var dbContext = GetDbContext();
        // No longer loading Subscriptions since they're retrieved live from external service
        return await dbContext.Companies.Include(c => c.Workspaces).ToListAsync();
    }

    /// <summary>
    /// Gets all the companies that the provided company manages servers for (ie - that have registered servers with their ManagedCompanyId set to this one).
    /// Dev Note: We will need some other method to determine the list of companies *possible* for a company to manage - right now, a PT admin could assign any company for the UsingCompany of a server.
    /// We should probably add a ManagingCompanyId to the Company entity, defaulting this to the PT Company, or something similar.
    /// </summary>
    /// <param name="a_currentCompanyId"></param>
    /// <returns></returns>
    public async Task<List<Company>> GetManagedCompaniesAsync(int a_currentCompanyId)
    {
        using var dbContext = GetDbContext();

        List<CompanyServer> serversForCompany = await GetCompanyServersAsync(a_currentCompanyId);

        List<int> managedCompanyIds = serversForCompany
                                        .SelectMany(server => server.UsingCompanies)
                                        .Select(usingCompany => usingCompany.CompanyId)
                                        .Distinct().ToList();
        managedCompanyIds.Add(a_currentCompanyId); // include self

        return await dbContext.Companies
            .Include(c => c.Workspaces)
            .Where(company => managedCompanyIds.Contains(company.Id))
            .ToListAsync();
    }

    public async Task<Company> SaveAsync(Company company)
    {
        using var dbContext = GetDbContext();


        if (company == null)
        {
            throw new ArgumentNullException(nameof(company));
        }

        if (company.Id == 0)
        {
            dbContext.Companies.Add(company);

            // Update DB to set the new company's id
            dbContext.SaveChanges();
            
            var CompanyId = company.Id;
            
            Role CompanyAdmin = new ()
            {
                CompanyId = CompanyId,
                Name = "Company Admin",
                Description = "Company Administrator, has rights to all Web and Desktop permissions.",
                Readonly = true,
                Permissions = Permission.CompanyAdminPermissions.Select(x => x.Key).ToList(),
                DesktopPermissions = PermissionsGroupConstants.DefaultPermissions.Select(x => x.PermissionKey).ToList(),
                UpdatedBy = "",
                LastModified = DateTime.UtcNow
            };
            
            dbContext.Roles.Add(CompanyAdmin);
            
            Role WebAdmin = new () 
            {
                CompanyId = CompanyId,
                Name = "Web Admin",
                Description = "Web Administrator, has rights to all Web app permissions.",
                Readonly = true,
                Permissions = Permission.WebAdminPermissions.Select(x => x.Key).ToList(),
                UpdatedBy = "",
                LastModified = DateTime.UtcNow
            };
            
            dbContext.Roles.Add(WebAdmin);
            
            Role WebServerAdmin = new ()
            {
                CompanyId = CompanyId,
                Name = "Web Server Admin",
                Description = "Web Server Administrator, View-only rights to all web and desktop systems and environments. Create, Edit, and Delete rights for Servers, Planning Areas, and Integrations.",
                Readonly = true,
                Permissions = Permission.WebServerAdminPermissions.Select(x => x.Key).ToList(),
                UpdatedBy = "",
                LastModified = DateTime.UtcNow
            };
            
            dbContext.Roles.Add(WebServerAdmin);
            
            Role ReportsAdmin = new ()
            {
                CompanyId = CompanyId,
                Name = "Reports Admin",
                Description = "Reports Admin, View-only rights to all web and desktop systems and environments. Create, Edit and Delete rights for Web App Reports and Report Categories.",
                Readonly = true,
                Permissions = Permission.ReportAdminPermissions.Select(x => x.Key).ToList(),
                UpdatedBy = "",
                LastModified = DateTime.UtcNow
            };
            
            dbContext.Roles.Add(ReportsAdmin);
            
            Role DesktopGlobalAdmin = new ()
            {
                CompanyId = CompanyId,
                Name = "Desktop Global Admin",
                Description = "Desktop Global Admin, View-only rights to all web systems. Admin rights within desktop planning areas.",
                Readonly = true,
                Permissions = Permission.DesktopGlobalAdminPermissions.Select(x => x.Key).ToList(),
                DesktopPermissions = PermissionsGroupConstants.DefaultPermissions.Select(x => x.PermissionKey).ToList(),
                UpdatedBy = "",
                LastModified = DateTime.UtcNow
            };
            
            dbContext.Roles.Add(DesktopGlobalAdmin);
            
            Role DesktopMasterScheduler = new ()
            {
                CompanyId = CompanyId,
                Name = "Desktop Master Scheduler",
                Description = "Desktop Master Scheduler, View-only rights to Web Reports and Gantts. Admin rights within accessible desktop planning areas.",
                Readonly = false,
                Permissions = Permission.DesktopMasterSchedulerPermissions.Select(x => x.Key).ToList(),
                DesktopPermissions = PermissionsGroupConstants.DefaultPermissions.Select(x => x.PermissionKey).ToList(),
                UpdatedBy = "",
                LastModified = DateTime.UtcNow
            };
            
            dbContext.Roles.Add(DesktopMasterScheduler);
            
            Role WebViewOnly = new ()
            {
                CompanyId = CompanyId,
                Name = "Web View Only",
                Description = "View-only rights to all web systems. ",
                Readonly = false,
                Permissions = Permission.WebViewOnlyPermissions.Select(x => x.Key).ToList(),
                UpdatedBy = "",
                LastModified = DateTime.UtcNow
            };
            
            dbContext.Roles.Add(WebViewOnly);
            
            Role DesktopViewOnly = new ()
            {
                CompanyId = CompanyId,
                Name = "Desktop View Only",
                Description = "View-only rights to allowed Planning Areas. ",
                Readonly = false,
                UpdatedBy = "",
                LastModified = DateTime.UtcNow
            };
            
            dbContext.Roles.Add(DesktopViewOnly);
            
            Role WebDesktopViewOnly = new ()
            {
                CompanyId = CompanyId,
                Name = "Web and Desktop View Only",
                Description = "View-only rights to all Web systems and allowed Planning Areas. ",
                Readonly = false,
                Permissions = Permission.WebViewOnlyPermissions.Select(x => x.Key).ToList(),
                UpdatedBy = "",
                LastModified = DateTime.UtcNow
            };
            
            dbContext.Roles.Add(WebDesktopViewOnly);
            
            
            // Update DB to set the new groups' ids
            dbContext.SaveChanges();
        }
        else
        {
            var c = dbContext.Companies.Where(c => c.Id == company.Id).Include((c => c.Workspaces)).FirstOrDefault();

            var diff = c.Workspaces.Synchronize(company.Workspaces);

            foreach (var workspace in diff.removed)
            {
                var reports = dbContext.Reports.Where(r => r.PBIWorkspace.Id == workspace.Id).ToList();
                foreach (var report in reports)
                {
                    var categories = dbContext.Set<ReportCategory>("CategoryReport");
                    var rc = categories.Where(x => x.ReportsId == report.Id);
                    dbContext.RemoveRange(rc);
                    dbContext.Remove(report);
                }
            }
            c.Email = company.Email;
            c.AllowedDomains = company.AllowedDomains; // TODO: validate that this is well-formed
            c.Name = company.Name;
            c.Active = company.Active;
            c.UseSSOLogin = company.UseSSOLogin;
            c.CompanyType = company.CompanyType;
            c.LicenseServiceCompanyName = company.LicenseServiceCompanyName;
            c.LicenseServiceCompanyUrl = company.LicenseServiceCompanyUrl;
            c.Version = company.Version;
            dbContext.Companies.Update(c);
        }

        await dbContext.SaveChangesAsync();
        return company;
    }

    public async Task<List<CompanyServer>> GetCompanyServersAsync(int companyId)
    {
        using var dbContext = GetDbContext();

        var servers = await dbContext.CompanyServers.Where(x => x.ManagingCompanyId == companyId).Include(x => x.OwningUser).ToListAsync();

        return servers;
    }

    public async Task<int> GetCompanyIdUsingAPIKey(string apiKey)
    {
        using var dbContext = GetDbContext();
        var company = await dbContext.Companies.Where(c => c.ApiKey == apiKey).FirstOrDefaultAsync();
        if (company != null)
        {
            return company.Id;
        }
        else
        {
            return 0; // or throw an exception if preferred
        }
    }

    public async Task<CompanyServer> AddOrUpdateServerAsync(CompanyServer server)
    {
        using var dbContext = GetDbContext();

        if (await dbContext.CompanyServers.Where(x => x.Id == server.Id).AnyAsync())
        {
            dbContext.CompanyServers.Update(server);
        }
        else
        {
            dbContext.CompanyServers.Add(server);
        }

        await dbContext.SaveChangesAsync();

        return server;
    }

    public async Task DeleteServerAsync(int serverId)
    {
        await DeleteServerAsync(new CompanyServer() { Id = serverId });
    }

    public async Task DeleteServerAsync(CompanyServer server)
    {
        using (var dbContext = GetDbContext())
        {
            dbContext.CompanyServers.Remove(server);

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<CompanyDb>> GetCompanyDbsAsync(int CompanyId)
    {
        using var dbContext = GetDbContext();
        var x = dbContext.CompanyDbs.Where(c => c.CompanyId == CompanyId).ToList();
        return x;
    }

    public async Task SaveCompanyDb(CompanyDb editModel)
    {
        using var dbContext = GetDbContext();
        var companyDb = dbContext.CompanyDbs.SingleOrDefault(d => d.Id == editModel.Id);
        if (companyDb != null)
        {
            companyDb.Name = editModel.Name;
            companyDb.Environment = editModel.Environment;
            companyDb.ServerManagerUrl = editModel.ServerManagerUrl;
            companyDb.ImportUserName = editModel.ImportUserName;
            companyDb.ImportUserPasswordKey = editModel.ImportUserPasswordKey;
            companyDb.DBServerName = editModel.DBServerName;
            companyDb.DBName = editModel.DBName;
            companyDb.DBUserName = editModel.DBUserName;
            companyDb.DBPasswordKey = editModel.DBPasswordKey;
            companyDb.DbType = editModel.DbType;
            companyDb.CreatedBy = editModel.CreatedBy;
            companyDb.CreationDate = DateTime.UtcNow;
            dbContext.SaveChanges();
        }
        else
        {
            dbContext.Add(editModel);
            dbContext.SaveChanges();
        }
    }

    public async Task DeleteCompanyDb(CompanyDb editModel)
    {
        using var dbContext = GetDbContext();
        var companyDb = dbContext.CompanyDbs.SingleOrDefault(d => d.Id == editModel.Id);
        if (companyDb != null)
        {
            dbContext.CompanyDbs.Remove(companyDb);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<string> GetCurrentInstallCode(User requestingUser)
    {
        using var dbContext = GetDbContext();

        // Try to find an existing Install Code for this company that hasn't been used and is less than 10 minutes old.
        var existingCode = dbContext.InstallCodes.FirstOrDefault(x => 
            x.CompanyId == requestingUser.CompanyId 
            && x.CreatedBy == requestingUser.Email
            && x.Used == false 
            && x.CreationDate > DateTime.UtcNow.AddMinutes(-10)); 

        // If an valid code is found, return it. Otherwise, generate a new one.
        if (existingCode != null)
        {
            return existingCode.Code;
        }

        string env = _config["Environment"] ?? "local";
        var code = new InstallCode(requestingUser, env);

        dbContext.Add(code);

        await dbContext.SaveChangesAsync();

        return code.Code;
    }

    /// <summary>
    /// Retrieves and refreshes subscriptions for a company based on serial code.
    /// Updates the company's subscription configuration and returns live data.
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <param name="serialCode">The serial code to lookup subscriptions</param>
    /// <returns>Live list of subscriptions from external service</returns>
    public async Task<List<SubscriptionInfo>> RefreshCompanySubscriptionsAsync(int companyId, string serialCode)
    {
        // Update or create the subscription configuration
        await _subscriptionService.UpdateCompanySubscriptionInfoAsync(companyId, serialCode);

        // Return live subscription data
        return await _subscriptionService.GetLiveSubscriptionsForCompanyAsync(companyId);
    }

    /// <summary>
    /// Gets live subscriptions for a company from external service
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <returns>List of live subscription information</returns>
    public async Task<List<SubscriptionInfo>> GetCompanySubscriptionsAsync(int companyId)
    {
        return await _subscriptionService.GetLiveSubscriptionsForCompanyAsync(companyId);
    }

    /// <summary>
    /// Gets the company's subscription configuration (serial code, URLs, etc.)
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <returns>The subscription configuration</returns>
    public async Task<CompanySubscriptionInfo> GetCompanySubscriptionInfoAsync(int companyId)
    {
        return await _subscriptionService.GetCompanySubscriptionInfoAsync(companyId);
    }

    /// <summary>
    /// Removes the company's subscription configuration
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <returns>True if successful</returns>
    public async Task<bool> RemoveCompanySubscriptionConfigurationAsync(int companyId)
    {
        return await _subscriptionService.RemoveCompanySubscriptionInfoAsync(companyId);
    }

    /// <summary>
    /// Gets all companies from the external license service
    /// </summary>
    /// <returns>List of companies from the license service</returns>
    public async Task<List<LicenseServiceCompanyDto>> GetLicenseServiceCompaniesAsync()
    {
        try
        {
            // Get the API endpoint and token from configuration
            var licenseServiceBaseUrl = _config["LicenseService:BaseUrl"];
            var apiEndpoint = licenseServiceBaseUrl?.TrimEnd('/') + "/api/companies";
            var apiToken = _config["LicenseService:APISecret"];

            if (string.IsNullOrEmpty(apiToken))
            {
                var error = new InvalidOperationException("License service API secret not configured");
                _logger.LogError(error, "CompanyService");
                return new List<LicenseServiceCompanyDto>();
            }

            if (string.IsNullOrEmpty(licenseServiceBaseUrl))
            {
                var error = new InvalidOperationException("License service base URL not configured");
                _logger.LogError(error, "CompanyService");
                return new List<LicenseServiceCompanyDto>();
            }

            // Prepare the request
            var request = new GetCompaniesRequest
            {
                Token = apiToken
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Make the API call
            var response = await _httpClient.PostAsync(apiEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var companies = JsonConvert.DeserializeObject<List<LicenseServiceCompanyDto>>(responseContent);

            _logger.LogEvent("License service companies retrieved", "CompanyService",
                new KeyValuePair<string, string>("CompanyCount", companies?.Count.ToString() ?? "0"));

            return companies ?? new List<LicenseServiceCompanyDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "CompanyService");
            throw new InvalidOperationException($"Failed to.retrieve companies from license service: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CompanyService");
            throw;
        }
    }
}
