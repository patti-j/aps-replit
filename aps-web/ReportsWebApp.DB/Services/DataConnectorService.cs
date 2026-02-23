using Microsoft.EntityFrameworkCore;

using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.DB.Services;

public class DataConnectorService : IDataConnectorService
{
    private readonly DbReportsContext _dbContext;
    private readonly IDbContextFactory<DbReportsContext> _factory;
    private readonly IPlanningAreaAssignmentPropagationService _propagationService;

    public DataConnectorService(
        DbReportsContext dbContext,
        IDbContextFactory<DbReportsContext> factory,
        IPlanningAreaAssignmentPropagationService propagationService)
    {
        _dbContext = dbContext;
        _factory = factory;
        _propagationService = propagationService;
    }

    public DataConnector? GetDataConnector(int a_dataConnectorId)
    {
        DataConnector? connector = _dbContext.DataConnectors.FirstOrDefault(i => i.Id == a_dataConnectorId);
        return connector;
    }

    public List<DataConnector> GetDataConnectorsForCompany(int CompanyId)
    {
        if (CompanyId == -1)
        {
            return _dbContext.DataConnectors.AsNoTracking().Include(i => i.Company).ToList();
        }
        return _dbContext.DataConnectors.AsNoTracking().Include(i => i.Company).Where(i => i.CompanyId == CompanyId).ToList();
    }

    public bool SaveDataConnector(DataConnector a_dataConnector)
    {
        using var db = _factory.CreateDbContext();

        if (a_dataConnector.Id == 0)
        {
            db.DataConnectors.Add(a_dataConnector);
            db.SaveChanges();

            // Sync settings for any PAs using this connector.
            _propagationService.ReapplyDataConnectorAsync(a_dataConnector.CompanyId, a_dataConnector.Id).GetAwaiter().GetResult();
            return true;
        }

        var existing = db.DataConnectors.FirstOrDefault(x => x.Id == a_dataConnector.Id);
        if (existing == null)
        {
            return false;
        }

        // Update fields to avoid EF tracking conflicts.
        existing.Name = a_dataConnector.Name;
        existing.CompanyId = a_dataConnector.CompanyId;

        existing.ConnectionString = a_dataConnector.ConnectionString;

        existing.ImportConnectionString = a_dataConnector.ImportConnectionString;
        existing.ImportIntegrationUserAndPass = a_dataConnector.ImportIntegrationUserAndPass;

        existing.PublishConnectionString = a_dataConnector.PublishConnectionString;

        existing.PreImportSQL = a_dataConnector.PreImportSQL;
        existing.PostExportSQL = a_dataConnector.PostExportSQL;

        existing.PreImportProgramArgs = a_dataConnector.PreImportProgramArgs;
        existing.PreImportProgramPath = a_dataConnector.PreImportProgramPath;

        existing.PreImportURL = a_dataConnector.PreImportURL;
        existing.PostImportURL = a_dataConnector.PostImportURL;

        existing.IsIntegrationV2Enabled = a_dataConnector.IsIntegrationV2Enabled;
        existing.RunPreImportSQL = a_dataConnector.RunPreImportSQL;

        existing.PreExportURL = a_dataConnector.PreExportURL;
        existing.PostExportURL = a_dataConnector.PostExportURL;

        existing.UseSeparateDatabases = a_dataConnector.UseSeparateDatabases;

        db.SaveChanges();

        _propagationService.ReapplyDataConnectorAsync(existing.CompanyId, existing.Id).GetAwaiter().GetResult();
        return true;
    }

    public async Task<bool> SaveDataConnectorAsync(DataConnector a_dataConnector)
    {
        await using var db = await _factory.CreateDbContextAsync();

        if (a_dataConnector.Id == 0)
        {
            db.DataConnectors.Add(a_dataConnector);
            await db.SaveChangesAsync();

            await _propagationService.ReapplyDataConnectorAsync(a_dataConnector.CompanyId, a_dataConnector.Id);
            return true;
        }

        var existing = await db.DataConnectors.FirstOrDefaultAsync(x => x.Id == a_dataConnector.Id);
        if (existing == null)
        {
            return false;
        }

        // Update fields to avoid EF tracking conflicts.
        existing.Name = a_dataConnector.Name;
        existing.CompanyId = a_dataConnector.CompanyId;

        existing.ConnectionString = a_dataConnector.ConnectionString;

        existing.ImportConnectionString = a_dataConnector.ImportConnectionString;
        existing.ImportIntegrationUserAndPass = a_dataConnector.ImportIntegrationUserAndPass;

        existing.PublishConnectionString = a_dataConnector.PublishConnectionString;

        existing.PreImportSQL = a_dataConnector.PreImportSQL;
        existing.PostExportSQL = a_dataConnector.PostExportSQL;

        existing.PreImportProgramArgs = a_dataConnector.PreImportProgramArgs;
        existing.PreImportProgramPath = a_dataConnector.PreImportProgramPath;

        existing.PreImportURL = a_dataConnector.PreImportURL;
        existing.PostImportURL = a_dataConnector.PostImportURL;

        existing.IsIntegrationV2Enabled = a_dataConnector.IsIntegrationV2Enabled;
        existing.RunPreImportSQL = a_dataConnector.RunPreImportSQL;

        existing.PreExportURL = a_dataConnector.PreExportURL;
        existing.PostExportURL = a_dataConnector.PostExportURL;

        existing.UseSeparateDatabases = a_dataConnector.UseSeparateDatabases;

        await db.SaveChangesAsync();

        await _propagationService.ReapplyDataConnectorAsync(existing.CompanyId, existing.Id);
        return true;
    }

    public bool DeleteDataConnector(int a_dataConnectorId)
    {
        using var db = _factory.CreateDbContext();

        DataConnector dataConnector = db.DataConnectors.FirstOrDefault(i => i.Id == a_dataConnectorId);
        if (dataConnector == null)
        {
            return false;
        }
        db.DataConnectors.Remove(dataConnector);
        db.SaveChanges();
        return true;
    }
}
