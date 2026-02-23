using ReportsWebApp.DB.Models;

public interface IDBIntegrationService
{
    Task<List<PADetails>> GetCompanyPlanningAreas(int companyId);

    /// <summary>
    /// Applies a specific integration to the given planning area.
    /// </summary>
    Task ApplyIntegrationToPlanningArea(int planningAreaId, int integrationId);

    /// <summary>
    /// Returns all available integrations for a given company.
    /// </summary>
    Task<List<DBIntegration>> GetAvailableIntegrations(int companyId);
}
