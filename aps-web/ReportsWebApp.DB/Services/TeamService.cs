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

public class TeamService 
{
    private readonly DbReportsContext _dbContext;
    private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;
    private readonly INotificationService _notificationService;
    private readonly IAppInsightsLogger _logger;
    private readonly AuditService _audit;

    public TeamService(DbReportsContext dbContext, IDbContextFactory<DbReportsContext> dbContextFactory, INotificationService notificationService, IAppInsightsLogger logger, AuditService audit)
    {
        _dbContext = dbContext;
        _dbContextFactory = dbContextFactory;
        _notificationService = notificationService;
        _logger = logger;
        _audit = audit;
    }

    public async Task<List<Team>> GetDBTeamsForCompany(int CompanyId)
    {
        return await _dbContext.Teams.Where(c => c.CompanyId == CompanyId)
                               .Include(x => x.DBRolesAndScopes)
                               .ThenInclude(x => x.Role)
                               .Include(x => x.DBRolesAndScopes)
                               .ThenInclude(x => x.Scope)
                               .Include(x => x.Users)
            .Include(x => x.Company).ToListAsync();
    }

    public async Task<List<Team>> GetAllTeamsForCompany(int companyId)
    {
        //await using var dbContext = await _dbContext.CreateDbContextAsync();

        var calculatedTeams = new List<Team>();
        var companyTeams = await GetDBTeamsForCompany(companyId);
        calculatedTeams.Add(new Team()
        {
            Id = -1,
            CompanyId = companyId,
            Readonly = true,
            Name = "Company Admins",
            Description = "Company-level administrators"
            // TODO: Link with corresponding roles/scopes
        });
        calculatedTeams.Add(new Team()
        {
            Id = -1,
            CompanyId = companyId,
            Readonly = true,
            Name = "Web Admins",
            Description = "Web app administrators"
            // TODO: Link with corresponding roles/scopes
        });
        calculatedTeams.Add(new Team()
        {
            Id = -1,
            CompanyId = companyId,
            Readonly = true,
            Name = "Desktop Global Admins",
            Description = "Desktop app planning area administrators",
            // TODO: Link with corresponding roles/scopes
        });

        return calculatedTeams.Concat(companyTeams).OrderBy(x => x.Readonly).ToList();
    }

    public async Task<Team> SaveAsync(User editor, Team team)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            if (team.Readonly)
            {
                throw new ArgumentException($"{team.Name} cannot be edited as it is read only.");
            }

            // Prevent EF From creating new elements
            team.Company = null;


            if (team.Id == 0)
            {
                if (dbContext.Teams.Any(g => g.Name == team.Name && g.CompanyId == team.CompanyId))
                {
                    throw new ArgumentException($"Team with name {team.Name} already exists.");
                }

                var rolesScopes = team.DBRolesAndScopes;
                team.DBRolesAndScopes = null;
                var users = await dbContext.Users.Where(x => x.CompanyId == team.CompanyId && team.Users.Select(e => e.Id).ToArray().Any(e => e == x.Id)).ToListAsync();
                team.Users = users;
                // This is a new team; add it to the context
                dbContext.Teams.Add(team);
                await dbContext.SaveChangesAsync();
                foreach (ScopedRole rolesScope in rolesScopes)
                {
                    rolesScope.TeamId = team.Id;
                }
                team.DBRolesAndScopes = rolesScopes;
                await dbContext.SaveChangesAsync();
            }
            else
            {
                if (dbContext.Teams.Any(g => g.Name == team.Name && g.CompanyId == team.CompanyId && g.Id != team.Id))
                {
                    throw new ArgumentException($"Team with name {team.Name} already exists.");
                }

                // Find the existing team
                var existingTeam = dbContext.Teams.Include(x => x.Users).FirstOrDefault(x => x.Id == team.Id);

                var audit = _audit.CreateEntityUpdateAuditRecord(editor, existingTeam, team);

                var oldName = existingTeam.Name;
                existingTeam.Name = team.Name;
                existingTeam.Description = team.Description;
                existingTeam.UpdatedBy = team.UpdatedBy;
                existingTeam.DBRolesAndScopes = team.DBRolesAndScopes;
                existingTeam.ComputedRolesAndScopes = team.ComputedRolesAndScopes;
                var users = await dbContext.Users.Where(x => x.CompanyId == team.CompanyId && team.Users.Select(e => e.Id).ToArray().Any(e => e == x.Id)).ToListAsync();
                existingTeam.Users = users;
                
                foreach (ScopedRole roleScope in existingTeam.DBRolesAndScopes)
                {
                    roleScope.TeamId = existingTeam.Id;
                }
                foreach (ComputedScopedRole roleScope in existingTeam.ComputedRolesAndScopes)
                {
                    roleScope.TeamId = existingTeam.Id;
                }

                // TODO: manage relationships
                
                await _audit.SaveAuditRecord(audit);

                _logger.LogEvent("Scope Changed", team.UpdatedBy,
                    KeyValuePair.Create("TeamName", team.Name),
                    KeyValuePair.Create("OldName", oldName)
                );
            }
            await dbContext.SaveChangesAsync();
            team = dbContext.Teams
                .FirstOrDefault(x => x.Id == team.Id) ?? throw new Exception($"An unknown Error ocurred when saving team {team.Name}");
            scope.Complete();
        }

        return team;
    }

    public async Task<bool> DeleteAsync(User editor, int scopeId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (scopeId <= 0)
        {
            return false; // Team is not in DB
        }

        var scope = dbContext.Teams.Where(g => g.Id == scopeId).FirstOrDefault();

        if (scope == null)
        {
            return false; // Team not found
        }
        if (scope.Readonly)
        {
            return false; // Team is readonly
        }

        dbContext.Teams.Remove(scope);
        _audit.SaveAuditRecord(new AuditService.AuditLogRecord(editor.Id, AuditService.AuditType.Delete, new ()));
        await dbContext.SaveChangesAsync();
        return true; // Team successfully deleted
    }
}
