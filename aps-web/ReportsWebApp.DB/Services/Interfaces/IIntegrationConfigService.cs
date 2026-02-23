using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services
{
    public interface IIntegrationConfigService
    {
        Task<List<IntegrationConfig>> GetByCompanyIdAsync(int companyId);
        Task<IntegrationConfig> UpdateAsync(IntegrationConfig config);
        Task DeleteAsync(IntegrationConfig config);
    }
}
