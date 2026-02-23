using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.DB.Services;

public class ExternalIntegrationService : IExternalIntegrationService
{
    private readonly DbReportsContext _dbContext;
    private readonly IDbContextFactory<DbReportsContext> _factory;
    private readonly IPlanningAreaAssignmentPropagationService _propagationService;

    public ExternalIntegrationService(
        DbReportsContext dbContext,
        IDbContextFactory<DbReportsContext> factory,
        IPlanningAreaAssignmentPropagationService propagationService)
    {
        _dbContext = dbContext;
        _factory = factory;
        _propagationService = propagationService;
    }

    public ExternalIntegration? GetExternalIntegration(int ExternalIntegrationId)
    {
        ExternalIntegration? integration = _dbContext.ExternalIntegrations.FirstOrDefault(i => i.Id == ExternalIntegrationId);
        return integration;
    }

    public List<ExternalIntegration> GetExternalIntegrationsForCompany(int CompanyId)
    {
        if (CompanyId == -1)
        {
            return _dbContext.ExternalIntegrations.AsNoTracking().Include(i => i.Company).ToList();
        }
        return _dbContext.ExternalIntegrations.AsNoTracking().Include(i => i.Company).Where(i => i.CompanyId == CompanyId).ToList();
    }

    public bool SaveExternalIntegration(ExternalIntegration externalIntegration)
    {
        using var db = _factory.CreateDbContext();

        if (externalIntegration.Id == 0)
        {
            if (string.IsNullOrEmpty(externalIntegration.SettingsJson))
            {
                externalIntegration.SettingsJson = "{}";
            }

            db.ExternalIntegrations.Add(externalIntegration);
            db.SaveChanges();

            _propagationService.ReapplyExternalIntegrationAsync(externalIntegration.CompanyId, externalIntegration.Id).GetAwaiter().GetResult();
            return true;
        }

        ExternalIntegration? integration = db.ExternalIntegrations
            .FirstOrDefault(i => i.Id == externalIntegration.Id);

        if (integration == null)
        {
            return false;
        }

        integration.SettingsJson = externalIntegration.SettingsJson;
        integration.Type = externalIntegration.Type;
        integration.CompanyId = externalIntegration.CompanyId;
        integration.Name = externalIntegration.Name;
        integration.ImportDataConnector = externalIntegration.ImportDataConnector;
        integration.PublishDataConnector = externalIntegration.PublishDataConnector;

        integration.PreImportProgramArgs = externalIntegration.PreImportProgramArgs;
        integration.PreImportProgramPath = externalIntegration.PreImportProgramPath;
        integration.PreImportURL = externalIntegration.PreImportURL;
        integration.PostImportURL = externalIntegration.PostImportURL;
        integration.IsIntegrationV2Enabled = externalIntegration.IsIntegrationV2Enabled;
        integration.RunPreImportSQL = externalIntegration.RunPreImportSQL;

        integration.PreExportURL = externalIntegration.PreExportURL;
        integration.PostExportURL = externalIntegration.PostExportURL;
        integration.AnalyticsURL = externalIntegration.AnalyticsURL;

        db.SaveChanges();

        _propagationService.ReapplyExternalIntegrationAsync(integration.CompanyId, integration.Id).GetAwaiter().GetResult();
        return true;
    }

    public async Task<bool> SaveExternalIntegrationAsync(ExternalIntegration externalIntegration)
    {
        await using var db = await _factory.CreateDbContextAsync();

        if (externalIntegration.Id == 0)
        {
            if (string.IsNullOrEmpty(externalIntegration.SettingsJson))
            {
                externalIntegration.SettingsJson = "{}";
            }

            db.ExternalIntegrations.Add(externalIntegration);
            await db.SaveChangesAsync();

            await _propagationService.ReapplyExternalIntegrationAsync(externalIntegration.CompanyId, externalIntegration.Id);
            return true;
        }

        ExternalIntegration? integration = await db.ExternalIntegrations
            .FirstOrDefaultAsync(i => i.Id == externalIntegration.Id);

        if (integration == null)
        {
            return false;
        }

        integration.SettingsJson = externalIntegration.SettingsJson;
        integration.Type = externalIntegration.Type;
        integration.CompanyId = externalIntegration.CompanyId;
        integration.Name = externalIntegration.Name;
        integration.ImportDataConnector = externalIntegration.ImportDataConnector;
        integration.PublishDataConnector = externalIntegration.PublishDataConnector;

        integration.PreImportProgramArgs = externalIntegration.PreImportProgramArgs;
        integration.PreImportProgramPath = externalIntegration.PreImportProgramPath;
        integration.PreImportURL = externalIntegration.PreImportURL;
        integration.PostImportURL = externalIntegration.PostImportURL;
        integration.IsIntegrationV2Enabled = externalIntegration.IsIntegrationV2Enabled;
        integration.RunPreImportSQL = externalIntegration.RunPreImportSQL;

        integration.PreExportURL = externalIntegration.PreExportURL;
        integration.PostExportURL = externalIntegration.PostExportURL;
        integration.AnalyticsURL = externalIntegration.AnalyticsURL;

        await db.SaveChangesAsync();

        await _propagationService.ReapplyExternalIntegrationAsync(integration.CompanyId, integration.Id);
        return true;
    }

    public bool DeleteExternalIntegration(int ExternalIntegrationId)
    {
        using var db = _factory.CreateDbContext();

        ExternalIntegration externalIntegration = db.ExternalIntegrations.FirstOrDefault(i => i.Id == ExternalIntegrationId);
        if (externalIntegration == null)
        {
            return false;
        }

        List<PADetails> pasUsingThis = db.PlanningAreas.Where(pa => pa.ExternalIntegrationId == externalIntegration.Id).ToList();
        foreach (PADetails pa in pasUsingThis)
        {
            pa.ExternalIntegrationId = null;
            db.PlanningAreas.Update(pa);
        }

        db.ExternalIntegrations.Remove(externalIntegration);
        db.SaveChanges();
        return true;
    }
}
