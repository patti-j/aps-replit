using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface  IPlanningAreaLoginService
{
    Task<List<PAPermissionGroup>> GetPermissionGroupsAsync(Company company);
    Task CreatePermissionGroupRecords(List<PAUserPermission> newPermissions);
    public Task<List<PlanningAreaAccess>> GetPlanningAreaAccessesForUserAsync(int userId);
    public Task<List<PlanningAreaAccess>> GetPlanningAreaAccessesForCompanyAsync(int companyId);
    public Task<PlanningAreaAccess> AddOrUpdatePlanningAreaAccessAsync(PlanningAreaAccess planningAreaAccess);
    public Task DeletePlanningAreaAccessAsync(PlanningAreaAccess planningAreaAccess);
    public Task DeletePlanningAreaAccessesForUserAsync(int UserId);
    Task<List<PAUserPermission>> GetPAUserPermissionsForCompanyAsync(Company company);
    Task<PAPermissionGroup> AddOrUpdatePAPermissionGroupAsync(PAPermissionGroup group);
    Task RemovePAPermissionGroupAsync(int groupId);
}