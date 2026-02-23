namespace ReportsWebApp.DB.Services.Interfaces;

public interface IPlanningAreaAssignmentPropagationService
{
    Task<int> ReapplyDataConnectorAsync(int companyId, int dataConnectorId);
    Task<int> ReapplyExternalIntegrationAsync(int companyId, int externalIntegrationId);
}
