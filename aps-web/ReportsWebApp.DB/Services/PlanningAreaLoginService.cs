using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Services
{
    public enum ELaunchStatus
    {
        Initial = 0, // No communication has been received from the Launcher
        LauncherStarting = 1, // Launcher has been called successfully
        LauncherUpdating = 11, // Launcher has been called successfully
        ClientUpdating = 12, // Launcher has been called successfully

        Success = 20, // The client has launched

        LauncherNotFound = 50, // The Launcher did not respond within the expected window
        VersionNotFound = 51, // The Launcher could not find the required version
        VersionNotSupported = 52, // The version selected does not support web login
        FailedToLaunch = 53, // The Launcher failed to start the client
    }

    public class PlanningAreaLoginService : IPlanningAreaLoginService
    {
        private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;
        private readonly INotificationService _notificationService;

        public PlanningAreaLoginService(IDbContextFactory<DbReportsContext> dbContextFactory, INotificationService notificationService)
        {
            _dbContextFactory = dbContextFactory;
            _notificationService = notificationService;
        }

        public async Task<List<PAPermissionGroup>> GetPermissionGroupsAsync(Company company)
        {
            var dbContext = _dbContextFactory.CreateDbContext();

            return await dbContext.PlanningAreaPermissionGroups.Where(x => x.CompanyId == company.Id).Include(x => x.Permissions).ToListAsync();
        }

        public async Task CreatePermissionGroupRecords(List<PAUserPermission> newPermissions)
        {
            var dbContext = _dbContextFactory.CreateDbContext();

            var existingPermissions = dbContext.PAUserPermissions.Select(x => x.PermissionKey).ToList();
            dbContext.PAUserPermissions.AddRange(newPermissions.Where(x => !existingPermissions.Contains(x.PermissionKey)));

            await dbContext.SaveChangesAsync();
        }

        public async Task<List<PAUserPermission>> GetPAUserPermissionsForCompanyAsync(Company company)
        {
            var dbContext = _dbContextFactory.CreateDbContext();

            return await dbContext.PAUserPermissions.Where(x => x.CompanyId == null || x.CompanyId == company.Id).ToListAsync();
        }

        public async Task<List<PlanningAreaAccess>> GetPlanningAreaAccessesForUserAsync(int userId)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            return await dbContext.PlanningAreaAccesses
                     .Include(x => x.PlanningArea)
                     .Include(x => x.User)
                     .Include(x => x.PermissionGroup)
                     .Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<List<PlanningAreaAccess>> GetPlanningAreaAccessesForCompanyAsync(int companyId)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            
            var users = await dbContext.Users.Where(x => x.CompanyId == companyId).Select(x => x.Id).ToListAsync();
            var accesses = await dbContext.PlanningAreaAccesses
                                          .Include(x => x.PlanningArea)
                                          .Include(x => x.User)
                                          .Include(x => x.PermissionGroup)
                                          .Where(x => users.Any(u => u == x.UserId)).ToListAsync();
            return accesses;
        }

        public async Task<PlanningAreaAccess> AddOrUpdatePlanningAreaAccessAsync(PlanningAreaAccess planningAreaAccess)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            if (planningAreaAccess.Id != 0)
            {
                //exists
                PlanningAreaAccess? existing = dbContext.PlanningAreaAccesses.FirstOrDefault(x => x.Id == planningAreaAccess.Id);
                if (existing == null)
                {
                    throw new InvalidOperationException("PlanningAreaAccess not found");
                }

                List<PlanningAreaAccess> targetObjects = await dbContext.PlanningAreaAccesses.Where(x => x.PlanningAreaId == planningAreaAccess.PlanningAreaId && x.UserId == planningAreaAccess.UserId && x.Id != planningAreaAccess.Id).ToListAsync();
                //check if access objects already exist for the desired user and PA, if so this call is invalid and those objects should be updated instead.
                if (targetObjects.Any())
                {
                    throw new InvalidOperationException("Attempted to create PA Access for a User and PA combo which already has a PA Access object defined for it.");
                }
                
                existing.PlanningAreaId = planningAreaAccess.PlanningAreaId;
                existing.UserId = planningAreaAccess.UserId;
                existing.PermissionGroupId = planningAreaAccess.PermissionGroupId;
            }
            else
            {
                List<PlanningAreaAccess> targetObjects = await dbContext.PlanningAreaAccesses.Where(x => x.PlanningAreaId == planningAreaAccess.PlanningAreaId && x.UserId == planningAreaAccess.UserId).ToListAsync();
                //check if access objects already exist for the desired user and PA, if so this call is invalid and those objects should be updated instead.
                if (targetObjects.Any())
                {
                    throw new InvalidOperationException("Attempted to create PA Access for a User and PA combo which already has a PA Access object defined for it.");
                }
                
                dbContext.PlanningAreaAccesses.Add(planningAreaAccess);
            }
            
            await dbContext.SaveChangesAsync();
            return dbContext.PlanningAreaAccesses.First(x => x.PlanningAreaId == planningAreaAccess.PlanningAreaId);
        }

        public async Task DeletePlanningAreaAccessAsync(PlanningAreaAccess planningAreaAccess)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            PlanningAreaAccess? existing = dbContext.PlanningAreaAccesses.FirstOrDefault(x => x.PlanningAreaId == planningAreaAccess.PlanningAreaId);
            if (existing == null)
            {
                throw new InvalidOperationException("PlanningAreaAccess not found");
            }
            
            dbContext.PlanningAreaAccesses.Remove(existing);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeletePlanningAreaAccessesForUserAsync(int UserId)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            var accesses = await dbContext.PlanningAreaAccesses.Where(x => x.UserId == UserId).ToListAsync();
            dbContext.PlanningAreaAccesses.RemoveRange(accesses);
            await dbContext.SaveChangesAsync();
        }

        public async Task<PAPermissionGroup> AddOrUpdatePAPermissionGroupAsync(PAPermissionGroup group)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            var groupSet = dbContext.Set<PAPermissionGroupPAUserPermission>("PAPermissionGroupPAUserPermission");

            var newPermissions = group.Permissions ?? new();
            group.Permissions = null;

            var existingGroup = await dbContext.PlanningAreaPermissionGroups.Where(x => x.Id == group.Id).FirstOrDefaultAsync();

            if (existingGroup != null)
            {
                dbContext.Attach(existingGroup);

                var existingPermissions = groupSet.Where(x => x.PAPermissionGroupId == group.Id).ToList();

                var diff = existingPermissions.Synchronize(newPermissions.Select(x => new PAPermissionGroupPAUserPermission() { PAPermissionGroupId = group.Id, PAUserPermissionId = x.Id}),
                    (x,y) => x.PAPermissionGroupId == y.PAPermissionGroupId && x.PAUserPermissionId == y.PAUserPermissionId);

                groupSet.AddRange(diff.added);
                groupSet.RemoveRange(diff.removed);

            }
            else
            {
                dbContext.Add(group);

            await dbContext.SaveChangesAsync();

                var added = newPermissions.Select(x => new PAPermissionGroupPAUserPermission() { PAPermissionGroupId = group.Id, PAUserPermissionId = x.Id });
                groupSet.AddRange(added);
                await dbContext.SaveChangesAsync();

            }

            await dbContext.SaveChangesAsync();

            return await dbContext.PlanningAreaPermissionGroups.Where(x => x.Id == group.Id).Include(x => x.Permissions).FirstOrDefaultAsync() ?? throw new Exception("Saved object was not retrievable from the database.");
        }

        public async Task RemovePAPermissionGroupAsync(int groupId)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            var groupSet = dbContext.Set<PAPermissionGroupPAUserPermission>("PAPermissionGroupPAUserPermission");

            var existingGroup = await dbContext.PlanningAreaPermissionGroups.Where(x => x.Id == groupId).FirstOrDefaultAsync();

            if (existingGroup != null)
            {
                dbContext.Attach(existingGroup);

                var existingPermissions = groupSet.Where(x => x.PAPermissionGroupId == groupId).ToList();

                groupSet.RemoveRange(existingPermissions);
                dbContext.Remove(existingGroup);
            }
            else
            {
                throw new ArgumentException("Group was not found in the database.");
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
