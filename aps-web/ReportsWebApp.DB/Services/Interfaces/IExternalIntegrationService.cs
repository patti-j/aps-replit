using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface IExternalIntegrationService
{
    public ExternalIntegration? GetExternalIntegration(int ExternalIntegrationId);
    public bool SaveExternalIntegration(ExternalIntegration ExternalIntegration);
    public List<ExternalIntegration> GetExternalIntegrationsForCompany(int CompanyId);
    public bool DeleteExternalIntegration(int ExternalIntegrationId);
    Task<bool> SaveExternalIntegrationAsync(ExternalIntegration externalIntegration);

}