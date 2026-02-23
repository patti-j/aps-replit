using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Models.DTOs;

public interface ICompanyService
{
    Task<List<Company>> GetCompaniesAsync();
    Task<List<Company>> GetManagedCompaniesAsync(int a_currentCompanyId);
    Task<Company> SaveAsync(Company company);
    Task<List<CompanyServer>> GetCompanyServersAsync(int companyId);
    Task<CompanyServer> AddOrUpdateServerAsync(CompanyServer server);
    Task DeleteServerAsync(int serverId);
    Task DeleteServerAsync(CompanyServer server);
    Task<List<CompanyDb>> GetCompanyDbsAsync(int CompanyId);
    Task SaveCompanyDb(CompanyDb editModel);
    Task<string> GetCurrentInstallCode(User requestingUser);
    Task DeleteCompanyDb(CompanyDb editModel);
    
    // Updated subscription-related methods - now work with live data only
    Task<List<SubscriptionInfo>> RefreshCompanySubscriptionsAsync(int companyId, string serialCode);
    Task<List<SubscriptionInfo>> GetCompanySubscriptionsAsync(int companyId);
    Task<CompanySubscriptionInfo> GetCompanySubscriptionInfoAsync(int companyId);
    Task<bool> RemoveCompanySubscriptionConfigurationAsync(int companyId);
    Task<List<LicenseServiceCompanyDto>> GetLicenseServiceCompaniesAsync();
}