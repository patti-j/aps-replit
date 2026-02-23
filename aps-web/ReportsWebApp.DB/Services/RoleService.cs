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
using ReportsWebApp.Shared;
using Microsoft.Extensions.Logging;
using ReportsWebApp.DB.Services.Interfaces;

public class RoleService : IRoleService
{
    private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;
    private readonly INotificationService _notificationService;
    private readonly IAppInsightsLogger _logger;
    //private readonly AuditService _audit;


    //public GroupService(IDbContextFactory<DbReportsContext> dbContext, NotificationService notificationService)
    public RoleService(DbReportsContext dbContext, IDbContextFactory<DbReportsContext> dbContextFactory, INotificationService notificationService, IAppInsightsLogger logger/*, AuditService audit*/)
    {
        _dbContextFactory = dbContextFactory;
        _notificationService = notificationService;
        _logger = logger;
        /*_audit = audit;*/
    }

    public async Task<List<Role>> GetRoles(int CompanyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        //await using var dbContext = await _dbContext.CreateDbContextAsync();
        // Use Entity Framework Core or other data access methods to retrieve groups
        var roles = await dbContext.Roles.Where(c => c.CompanyId == CompanyId).Include(x => x.Users)
            .Include(x => x.Categories).Include(x => x.Company)
            .OrderBy(x => x.Readonly).ToListAsync();
        
        return roles;
    }

    public async Task UpdateRolesInCompanyToNewPermissions(int CompanyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var roles = await dbContext.Roles.Where(c => c.CompanyId == CompanyId).Include(x => x.Users)
                                    .Include(x => x.Categories).Include(x => x.Company)
                                    .OrderBy(x => x.Readonly).ToListAsync();

        foreach (Role role in roles)
        {
            bool modified = false;
            List<string> toRemove = new List<string>();
            foreach (string permission in role.Permissions)
            {
                if (!Permission.AllPermissions.Any(x => x.Key == permission))
                {
                    modified = true;
                    toRemove.Add(permission);
                }
            }
            
            

            role.Permissions.RemoveAll(p => toRemove.Contains(p));

            if (modified)
            {
                await dbContext.SaveChangesAsync();
            }
        }

        if (roles.Any(x => x.Name == "Default Administrator"))
        {
            var role = roles.First(x => x.Name == "Default Administrator");
            role.Name = "Company Admin";
            role.Description = "Company Administrator, has rights to all Web and Desktop permissions.";
            role.Readonly = true;
            role.Permissions = Permission.CompanyAdminPermissions.Select(x => x.Key).ToList();
            role.DesktopPermissions = PermissionsGroupConstants.DefaultPermissions.Select(x => x.PermissionKey).ToList();

        }
        else
        {
            Role CompanyAdmin = new ()
            {
                CompanyId = CompanyId,
                Name = "Company Admin",
                Description = "Company Administrator, has rights to all Web and Desktop permissions.",
                Readonly = true,
                Permissions = Permission.CompanyAdminPermissions.Select(x => x.Key).ToList(),
                DesktopPermissions = PermissionsGroupConstants.DefaultPermissions.Select(x => x.PermissionKey).ToList(),
                UpdatedBy = "",
                LastModified = DateTime.UtcNow
            };
            
            dbContext.Roles.Add(CompanyAdmin);
        }
        
        Role WebAdmin = new ()
        {
            CompanyId = CompanyId,
            Name = "Web Admin",
            Description = "Web Administrator, has rights to all Web app permissions.",
            Readonly = true,
            Permissions = Permission.WebAdminPermissions.Select(x => x.Key).ToList(),
            UpdatedBy = "",
            LastModified = DateTime.UtcNow
        };
        
        dbContext.Roles.Add(WebAdmin);
        
        Role WebServerAdmin = new ()
        {
            CompanyId = CompanyId,
            Name = "Web Server Admin",
            Description = "Web Server Administrator, View-only rights to all web and desktop systems and environments. Create, Edit, and Delete rights for Servers, Planning Areas, and Integrations.",
            Readonly = true,
            Permissions = Permission.WebServerAdminPermissions.Select(x => x.Key).ToList(),
            UpdatedBy = "",
            LastModified = DateTime.UtcNow
        };
        
        dbContext.Roles.Add(WebServerAdmin);
        
        Role ReportsAdmin = new ()
        {
            CompanyId = CompanyId,
            Name = "Reports Admin",
            Description = "Reports Admin, View-only rights to all web and desktop systems and environments. Create, Edit and Delete rights for Web App Reports and Report Categories.",
            Readonly = true,
            Permissions = Permission.ReportAdminPermissions.Select(x => x.Key).ToList(),
            UpdatedBy = "",
            LastModified = DateTime.UtcNow
        };
        
        dbContext.Roles.Add(ReportsAdmin);
        
        Role DesktopGlobalAdmin = new ()
        {
            CompanyId = CompanyId,
            Name = "Desktop Global Admin",
            Description = "Desktop Global Admin, View-only rights to all web systems. Admin rights within desktop planning areas.",
            Readonly = true,
            Permissions = Permission.DesktopGlobalAdminPermissions.Select(x => x.Key).ToList(),
            DesktopPermissions = PermissionsGroupConstants.DefaultPermissions.Select(x => x.PermissionKey).ToList(),
            UpdatedBy = "",
            LastModified = DateTime.UtcNow
        };
        
        dbContext.Roles.Add(DesktopGlobalAdmin);
        
        Role DesktopMasterScheduler = new ()
        {
            CompanyId = CompanyId,
            Name = "Desktop Master Scheduler",
            Description = "Desktop Master Scheduler, View-only rights to Web Reports and Gantts. Admin rights within accessible desktop planning areas.",
            Readonly = false,
            Permissions = Permission.DesktopMasterSchedulerPermissions.Select(x => x.Key).ToList(),
            DesktopPermissions = PermissionsGroupConstants.DefaultPermissions.Select(x => x.PermissionKey).ToList(),
            UpdatedBy = "",
            LastModified = DateTime.UtcNow
        };
        
        dbContext.Roles.Add(DesktopMasterScheduler);
        
        Role WebViewOnly = new ()
        {
            CompanyId = CompanyId,
            Name = "Web View Only",
            Description = "View-only rights to all web systems. ",
            Readonly = false,
            Permissions = Permission.WebViewOnlyPermissions.Select(x => x.Key).ToList(),
            UpdatedBy = "",
            LastModified = DateTime.UtcNow
        };
        
        dbContext.Roles.Add(WebViewOnly);
        
        Role DesktopViewOnly = new ()
        {
            CompanyId = CompanyId,
            Name = "Desktop View Only",
            Description = "View-only rights to allowed Planning Areas. ",
            Readonly = false,
            UpdatedBy = "",
            LastModified = DateTime.UtcNow
        };
        
        dbContext.Roles.Add(DesktopViewOnly);
        
        Role WebDesktopViewOnly = new ()
        {
            CompanyId = CompanyId,
            Name = "Web and Desktop View Only",
            Description = "View-only rights to all Web systems and allowed Planning Areas. ",
            Readonly = false,
            Permissions = Permission.WebViewOnlyPermissions.Select(x => x.Key).ToList(),
            UpdatedBy = "",
            LastModified = DateTime.UtcNow
        };
        
        dbContext.Roles.Add(WebDesktopViewOnly);
        
        Company company = dbContext.Companies.First(c => c.Id == CompanyId);
        if (company.SoftMigrationStatus == 0)
        {
            company.SoftMigrationStatus = 1;
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<Role>> GetAllRoles()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        // Use Entity Framework Core or other data access methods to retrieve groups
        return await dbContext.Roles.Include(x => x.Users).Include(x => x.Categories)
            .Include(x => x.Company)
            .OrderBy(x => x.CompanyId).ThenBy(x => x.Readonly).ToListAsync();
    }

    public async Task<Role> DuplicateRoleAsync(Role a_role, User creator)
    {
        //await using var dbContext = await _dbContext.CreateDbContextAsync();
        var newGroup = new Role()
        {
            Categories = new List<Category>(a_role.Categories),
            CompanyId = a_role.CompanyId,
            CreationDate = DateTime.UtcNow,
            CreatedBy = creator.Email,
            UpdatedBy = creator.Email,
            GanttFavorites = new List<SchedulerFavorite>(a_role.GanttFavorites),
            Name = await GetUniqueGroupName(a_role.Name, a_role.CompanyId),
            Permissions = a_role.Permissions,
            Readonly = false
        };

        await SaveAsync(creator, newGroup, creator.IsPTAdmin());
        return newGroup;
    }

    private async Task<string> GetUniqueGroupName(string roleName, int companyId)
    {
        var roles = await GetRoles(companyId);
        string baseCopyName = roleName + " Copy";
        var copyRoles = roles.Where(g => g.Name.StartsWith(baseCopyName)).ToList();
        if (copyRoles.Count() > 0)
        {
            if (copyRoles.Any(g => g.Name.EndsWith(')')))
            {
                var nums = copyRoles.Select(g =>
                {
                    try
                    {
                        return g.Name.TrimEnd(')').Split('(').Last();
                    }
                    catch
                    {
                        return null;
                    }
                }).Where(s => s is not null).Distinct().Select(s =>
                {
                    try
                    {
                        return new int?(int.Parse(s!));
                    }
                    catch
                    {
                        return null;
                    }
                }).Where(i => i.HasValue).Select(s => s!.Value).ToList();
                if (nums.Count() == 0)
                {
                    return roleName + " Copy (2)";
                }
                var largestNumber = nums.Max();

                return roleName + " Copy " + $"({++largestNumber})";
            }
            else
            {
                return roleName + " Copy (2)";
            }
        }
        else
        {
            return roleName + " Copy";
        }
    }

    public async Task<Role> SaveAsync(User editor, Role a_role, bool isPtAdmin = false)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var notificationType = ENotificationType.Generic;
        var categorySet = dbContext.Set<RoleCategory>("RoleCategory");

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            if (a_role == null)
            {
                throw new ArgumentNullException(nameof(a_role));
            }

            if (a_role.Readonly && !isPtAdmin)
            {
                throw new ArgumentException($"{a_role.Name} cannot be edited as it is read only.");
            }

            if (a_role.Permissions.Any(x => x == Permission.PTAdmin.Key) && a_role.CompanyId != 1)
            {
                throw new ArgumentException($"Cannot assign to PTAdmin privileges to {a_role.Name} as it is not a part of the Planettogether Company.");
            }

            // Get permission records assoicated with the new list of permissions
            var newCategories = a_role.Categories.Select(x => new RoleCategory() { RolesId = a_role.Id, CategoriesId = x.Id }).ToList();

            // Prevent EF From creating new elements
            a_role.Categories = null;
            a_role.Company = null;

            if (a_role.Id == 0)
            {
                if (dbContext.Roles.Any(g => g.Name == a_role.Name && g.CompanyId == a_role.CompanyId))
                {
                    throw new ArgumentException($"Role with name {a_role.Name} already exists.");
                }
                
                a_role.LastModified = DateTime.UtcNow;
                
                // This is a new group; add it to the context
                dbContext.Roles.Add(a_role);
                await dbContext.SaveChangesAsync();

                // Update relations with new id
                newCategories.ForEach(x => x.RolesId = a_role.Id);
                // Add references to the permissions added
                categorySet.AddRange(newCategories);

                _logger.LogEvent("Role Added", a_role.UpdatedBy,
                    KeyValuePair.Create("RoleName", a_role.Name),
                    KeyValuePair.Create("PermissionsAdded", string.Join(',', a_role.Permissions))
                );
                //TODO AUDIT LOG

                notificationType = ENotificationType.NewRole;
            }
            else
            {
                if (dbContext.Roles.Any(g => g.Name == a_role.Name && g.CompanyId == a_role.CompanyId && g.Id != a_role.Id))
                {
                    throw new ArgumentException($"Role with name {a_role.Name} already exists.");
                }
                
                // This is an existing group; update it in the context
                // Find existing relations
                var oldCategories = categorySet.AsNoTracking().Where(x => x.RolesId == a_role.Id).ToList();

                // Find the existing group
                var existingRole = dbContext.Roles.FirstOrDefault(x => x.Id == a_role.Id);

                //var audit = _audit.CreateEntityUpdateAuditRecord(editor, existingGroup, group);

                var oldName = existingRole.Name;
                var oldPermissions = existingRole.Permissions;
                existingRole.Name = a_role.Name;
                existingRole.Description = a_role.Description;
                existingRole.UpdatedBy = a_role.UpdatedBy;

                var permissionDiff = existingRole.Permissions.Synchronize(a_role.Permissions);
                var papermissionDiff = existingRole.DesktopPermissions.Synchronize(a_role.DesktopPermissions);
                
                existingRole.LastModified = DateTime.UtcNow;

                // Update categories
                var categoryDiff = oldCategories.Synchronize(newCategories);
                categorySet.AddRange(categoryDiff.added);
                categorySet.RemoveRange(categoryDiff.removed);

                dbContext.Update(existingRole);

                //await _audit.SaveAuditRecord(audit);

                _logger.LogEvent("Role Changed", a_role.UpdatedBy,
                    KeyValuePair.Create("RoleName", a_role.Name),
                    KeyValuePair.Create("OldName", oldName),
                    KeyValuePair.Create("PermissionsAdded", string.Join(',', permissionDiff.added.Count)),
                    KeyValuePair.Create("PermissionsRemoved", string.Join(',', permissionDiff.removed.Count))
                );
                //TODO AUDIT LOG

                // Add or remove references to the permissions respectively
                //permissionSet.AddRange(permissions.Select(x => new GroupPermission { GroupId = group.Id, PermissionId = x.Id }));
            }
            await dbContext.SaveChangesAsync();
            a_role = dbContext.Roles
                .Include(x => x.Categories)
                .FirstOrDefault(x => x.Id == a_role.Id) ?? throw new Exception($"An unknown Error ocurred when saving Role {a_role.Name}");
            scope.Complete();
            //_dbContext.Entry(group).State = EntityState.Detached;
        }

        if (notificationType == ENotificationType.NewRole)
        {
            await _notificationService.AddNotificationForCompany(a_role.CompanyId, ENotificationType.NewRole, $"New Role created: {a_role.Name}", "NewRole");
        }

        return a_role;
    }

    public async Task<bool> DeleteAsync(int groupId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        //var group = await _dbContext.Groups.FindAsync(groupId);
        var role = dbContext.Roles.Where(g => g.Id == groupId).Include(u => u.Users).ThenInclude(x => x.Roles).Include(c => c.Categories).FirstOrDefault();

        if (role == null)
        {
            return false; // Group not found
        }
        if (role.Readonly)
        {
            return false; // Group is readonly
        }

        // Check for users that only have this group
        var solitaryUsers = role.Users.Where(x => x.Roles.Where(y => y.CompanyId == role.CompanyId).Count() == 1).ToList();
        if (solitaryUsers.Count > 0)
        {
            throw new DataDependencyException($"There are {solitaryUsers.Count} user(s) that are only assigned to the Role {role.Name}. Before you can delete this Role, you must either assign those users to a different Role, or delete them:\n{string.Join(", ", solitaryUsers.Select(x => x.Email))}");
        }

        role.Categories.Clear();
        role.Permissions.Clear();

        // Clear users so that EFCore doesn't try to delete them
        role.Users.Clear();
        await dbContext.SaveChangesAsync();


        dbContext.Roles.Remove(role);
        await dbContext.SaveChangesAsync();
        return true; // Group successfully deleted
    }
}
