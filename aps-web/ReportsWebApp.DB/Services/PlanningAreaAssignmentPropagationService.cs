using Microsoft.EntityFrameworkCore;

using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.DB.Services;

public class PlanningAreaAssignmentPropagationService : IPlanningAreaAssignmentPropagationService
{
    private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;
    private readonly IPlanningAreaDataService _planningAreaDataService;

    public PlanningAreaAssignmentPropagationService(
        IDbContextFactory<DbReportsContext> dbContextFactory,
        IPlanningAreaDataService planningAreaDataService)
    {
        _dbContextFactory = dbContextFactory;
        _planningAreaDataService = planningAreaDataService;
    }

    public async Task<int> ReapplyDataConnectorAsync(int companyId, int dataConnectorId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var dataConnector = await dbContext.DataConnectors
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == dataConnectorId && c.CompanyId == companyId);

        if (dataConnector == null)
        {
            return 0;
        }

        var affectedPaIds = await dbContext.PlanningAreas
            .AsNoTracking()
            .Where(pa => pa.CompanyId == companyId && pa.DataConnectorId == dataConnectorId)
            .Select(pa => pa.Id)
            .ToListAsync();

        if (affectedPaIds.Count == 0)
        {
            return 0;
        }

        foreach (var paId in affectedPaIds)
        {
            var pa = await _planningAreaDataService.GetPlanningAreaByIdAsync(paId);
            if (pa == null)
            {
                continue;
            }

            ApplyDataConnectorSettings(pa, dataConnector);
            await _planningAreaDataService.SaveAsync(pa);
        }

        return affectedPaIds.Count;
    }

    public async Task<int> ReapplyExternalIntegrationAsync(int companyId, int externalIntegrationId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var integration = await dbContext.ExternalIntegrations
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == externalIntegrationId && i.CompanyId == companyId);

        if (integration == null)
        {
            return 0;
        }

        var affectedPaIds = await dbContext.PlanningAreas
            .AsNoTracking()
            .Where(pa => pa.CompanyId == companyId && pa.ExternalIntegrationId == externalIntegrationId)
            .Select(pa => pa.Id)
            .ToListAsync();

        if (affectedPaIds.Count == 0)
        {
            return 0;
        }

        foreach (var paId in affectedPaIds)
        {
            var pa = await _planningAreaDataService.GetPlanningAreaByIdAsync(paId);
            if (pa == null)
            {
                continue;
            }

            ApplyExternalIntegrationSettings(pa, integration);
            await _planningAreaDataService.SaveAsync(pa);
        }

        return affectedPaIds.Count;
    }

    private static void ApplyDataConnectorSettings(PADetails pa, DataConnector dataConnector)
    {
        var hasIntegrationAssigned = pa.ExternalIntegrationId.HasValue;

        pa.PlanningArea ??= new PlanningArea();
        var settings = pa.PlanningArea.Settings;

        settings.ErpDatabaseSettings.ConnectionString = dataConnector.ImportConnectionString ?? string.Empty;
        settings.ErpDatabaseSettings.PublishSqlServerConnectionString = dataConnector.PublishConnectionString ?? string.Empty;
        settings.ErpDatabaseSettings.IntegrationUserCreds = dataConnector.ImportIntegrationUserAndPass ?? string.Empty;

        settings.InterfaceServiceSettings.PreImportSQL = dataConnector.PreImportSQL ?? string.Empty;
        settings.InterfaceServiceSettings.PreImportProgramPath = dataConnector.PreImportProgramPath ?? string.Empty;
        settings.InterfaceServiceSettings.PreImportProgramArgs = dataConnector.PreImportProgramArgs ?? string.Empty;
        settings.InterfaceServiceSettings.RunPreImportSQL = dataConnector.RunPreImportSQL ?? false;

        settings.IntegrationV2Connection = (dataConnector.IsIntegrationV2Enabled ?? false) ? "true" : "";

        settings.PostImportURL = dataConnector.PostImportURL ?? string.Empty;
        settings.PreExportURL = dataConnector.PreExportURL ?? string.Empty;

        // Keep current behavior: do not override integration URLs when integration is assigned.
        if (!hasIntegrationAssigned)
        {
            settings.PreImportURL = dataConnector.PreImportURL ?? string.Empty;
            settings.PostExportURL = dataConnector.PostExportURL ?? string.Empty;
        }
    }

    private static void ApplyExternalIntegrationSettings(PADetails pa, ExternalIntegration integration)
    {
        pa.PlanningArea ??= new PlanningArea();
        var settings = pa.PlanningArea.Settings;

        settings.PreImportURL = integration.PreImportURL ?? string.Empty;
        settings.PostExportURL = integration.PostExportURL ?? string.Empty;

        var basePre = integration.PreImportURL ?? string.Empty;
        var basePost = integration.PostExportURL ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(basePre))
        {
            settings.PreImportURL = $"{basePre.TrimEnd('/')}/{pa.CompanyId}/{pa.Id}/{(int)integration.Type}";
        }

        if (!string.IsNullOrWhiteSpace(basePost))
        {
            settings.PostExportURL = $"{basePost.TrimEnd('/')}/{pa.CompanyId}/{pa.Id}/{(int)integration.Type}";
        }
    }
}
