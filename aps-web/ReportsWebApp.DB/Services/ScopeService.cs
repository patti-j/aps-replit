using Microsoft.EntityFrameworkCore;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReportsWebApp.DB.Services;
using ReportsWebApp.Common;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Internal;
using System.Data;

using Azure.Messaging.EventHubs.Producer;

using ReportsWebApp.Shared;
using Microsoft.Extensions.Logging;
using ReportsWebApp.DB.Services.Interfaces;

public class ScopeService 
{
    private readonly DbReportsContext _dbContext;
    private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;
    private readonly INotificationService _notificationService;
    private readonly IAppInsightsLogger _logger;
    private readonly AuditService _audit;

    public ScopeService(DbReportsContext dbContext, IDbContextFactory<DbReportsContext> dbContextFactory, INotificationService notificationService, IAppInsightsLogger logger, AuditService audit)
    {
        _dbContext = dbContext;
        _dbContextFactory = dbContextFactory;
        _notificationService = notificationService;
        _logger = logger;
        _audit = audit;
    }

    public async Task<List<PlanningAreaScope>> GetDBScopesForCompany(int CompanyId)
    {
        var scopes = await _dbContext.PlanningAreaScopes.Where(c => c.CompanyId == CompanyId).Include(x => x.PlanningAreas) // TODO: Re-add once entities are mapped
                                     .Include(x => x.Company)
                                     .Include(x => x.PlanningAreas)
                                     .Include(x => x.PlanningAreaScopeAssociationKeys).ToListAsync();
        foreach (var scope in scopes)
        {
            foreach (var key in scope.PlanningAreaScopeAssociationKeys)
            {
                if (key.ScopeAssociationKey.StartsWith("L:"))
                {
                    if (key.ScopeAssociationKey.EndsWith('?'))
                    {
                        // Question mark means all PAs without locations
                        scope.PlanningAreas.AddRange(_dbContext.PlanningAreas.Where(x => x.LocationId == null));
                    }
                    else
                    {
                        var locationId = int.Parse(key.ScopeAssociationKey.Split(':').Last());
                        scope.PlanningAreas.AddRange(_dbContext.PlanningAreas.Where(x => x.LocationId == locationId));
                    }
                }
            }
        }

        return scopes;
    }

    public async Task<List<PlanningAreaScope>> GetAllScopesForCompany(int companyId)
    {
        var calculatedScopes = new List<PlanningAreaScope>();
        var companyPas = _dbContext.PlanningAreas.Where(x => x.CompanyId == companyId).ToList();
        calculatedScopes.Add(new PlanningAreaScope()
        {
            PlanningAreas = companyPas.ToList(),
            Id = -1,
            CompanyId = companyId,
            Readonly = true,
            Name = "All Planning Areas"
        });

        calculatedScopes.Add(new PlanningAreaScope()
        {
            PlanningAreas = companyPas.Where(x => x.Environment.Equals("prod", StringComparison.InvariantCultureIgnoreCase)).ToList(),
            Id = -2,
            CompanyId = companyId,
            Readonly = true,
            Name = "All 'Production' Planning Areas"
        });

        calculatedScopes.Add(new PlanningAreaScope()
        {
            PlanningAreas = companyPas.Where(x => x.Environment.Equals("qa", StringComparison.InvariantCultureIgnoreCase)).ToList(),
            Id = -3,
            CompanyId = companyId,
            Readonly = true,
            Name = "All 'QA' Planning Areas"
        });

        calculatedScopes.Add(new PlanningAreaScope()
        {
            PlanningAreas = companyPas.Where(x => x.Environment.Equals("dev", StringComparison.InvariantCultureIgnoreCase)).ToList(),
            Id = -4,
            CompanyId = companyId,
            Readonly = true,
            Name = "All 'Development' Planning Areas"
        });

        return calculatedScopes.Concat(await GetDBScopesForCompany(companyId)).OrderBy(x => x.Readonly).ToList();
    }

    public async Task<PlanningAreaScope> SaveAsync(User editor, PlanningAreaScope paScope)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var mapSet = dbContext.Set<PAPlanningAreaScope>("PAPlanningAreaScope");

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            if (paScope == null)
            {
                throw new ArgumentNullException(nameof(paScope));
            }

            if (paScope.Readonly)
            {
                throw new ArgumentException($"{paScope.Name} cannot be edited as it is read only.");
            }

            // Prevent EF From creating new elements
            paScope.Company = null;

            if (paScope.Id == 0)
            {
                if (dbContext.PlanningAreaScopes.Any(g => g.Name == paScope.Name && g.CompanyId == paScope.CompanyId))
                {
                    throw new ArgumentException($"Scope with name {paScope.Name} already exists.");
                }

                // Prevent EF from creating PA entities
                var pasToLink = paScope.PlanningAreas;
                paScope.PlanningAreas = null;

                // This is a new scope; add it to the context
                dbContext.PlanningAreaScopes.Add(paScope);
                await dbContext.SaveChangesAsync();

                mapSet.AddRange(pasToLink.Select(x => new PAPlanningAreaScope() { PlanningAreaId = x.Id, PlanningAreaScopeId = paScope.Id }));
                await dbContext.SaveChangesAsync();
            }
            else
            {
                if (dbContext.PlanningAreaScopes.Any(g => g.Name == paScope.Name && g.CompanyId == paScope.CompanyId && g.Id != paScope.Id))
                {
                    throw new ArgumentException($"Scope with name {paScope.Name} already exists.");
                }

                // Find the existing scope
                var existingScope = dbContext.PlanningAreaScopes.Include(x => x.PlanningAreas).Include(x => x.PlanningAreaScopeAssociationKeys).FirstOrDefault(x => x.Id == paScope.Id);

                var audit = _audit.CreateEntityUpdateAuditRecord(editor, existingScope, paScope);

                var existingRelations = mapSet.Where(x => x.PlanningAreaScopeId == existingScope.Id)
                                              .ToList();

                var oldName = existingScope.Name;
                existingScope.Name = paScope.Name;
                existingScope.UpdatedBy = paScope.UpdatedBy;
                existingScope.UpdatedBy = editor.Email;
                existingScope.Description = paScope.Description;
                existingScope.PlanningAreaScopeAssociationKeys.Synchronize(paScope.PlanningAreaScopeAssociationKeys);

                foreach (var key in existingScope.PlanningAreaScopeAssociationKeys)
                {
                    if (key.ScopeAssociationKey.StartsWith("L:"))
                    {
                        int? locationId;
                        if (key.ScopeAssociationKey.EndsWith("?"))
                        {
                            locationId = null;
                        }
                        else
                        {
                            locationId = int.Parse(key.ScopeAssociationKey.Split(':').Last());

                        }
                        // Association keys are stored separately, so don't redundantly save associated PAs
                        paScope.PlanningAreas.RemoveAll(x => x.LocationId == locationId);
                        existingScope.PlanningAreas.RemoveAll(x => x.LocationId == locationId);
                    }
                }

                var paDiff = existingRelations.Synchronize(paScope.PlanningAreas.Select(x => new PAPlanningAreaScope() { PlanningAreaScopeId = existingScope.Id, PlanningAreaId = x.Id }));


                foreach (PADetails pa in existingScope.PlanningAreas)
                {
                    pa.Company = null;
                    pa.Location = null;
                    pa.Server = null;
                }

                mapSet.AddRange(paDiff.added);
                mapSet.RemoveRange(paDiff.removed);
                dbContext.PlanningAreaScopes.Update(existingScope);

                await dbContext.SaveChangesAsync();

                await _audit.SaveAuditRecord(audit);
                
                _logger.LogEvent("Scope Changed", paScope.UpdatedBy,
                    KeyValuePair.Create("ScopeName", paScope.Name),
                    KeyValuePair.Create("OldName", oldName)
                );
            }
            paScope = dbContext.PlanningAreaScopes
                .Include(x => x.PlanningAreas)
                .FirstOrDefault(x => x.Id == paScope.Id) ?? throw new Exception($"An unknown Error ocurred when saving scope {paScope.Name}");
            scope.Complete();
        }

        return paScope;
    }

    public async Task<bool> DeleteAsync(User editor, int scopeId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var mapSet = dbContext.Set<PAPlanningAreaScope>("PAPlanningAreaScope");

        if (scopeId <= 0)
        {
            return false; // Scope is not in DB
        }

        var scope = dbContext.PlanningAreaScopes.FirstOrDefault(g => g.Id == scopeId);

        if (scope == null)
        {
            return false; // Scope not found
        }
        if (scope.Readonly)
        {
            return false; // Scope is readonly
        }

        var scopeRelations = mapSet.Where(x => x.PlanningAreaScopeId == scopeId);

        mapSet.RemoveRange(scopeRelations);
        dbContext.PlanningAreaScopes.Remove(scope);
        await _audit.SaveAuditRecord(new AuditService.AuditLogRecord(editor.Id, AuditService.AuditType.Delete, new ()));
        await dbContext.SaveChangesAsync();
        return true; // Scope successfully deleted
    }
}
