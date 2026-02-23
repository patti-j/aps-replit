using ReportsWebApp.DB.Models;

public interface IRoleService
{
    Task<List<Role>> GetRoles(int CompanyId);
    Task<List<Role>> GetAllRoles();
    Task<Role> DuplicateRoleAsync(Role a_role, User creator);
    Task<Role> SaveAsync(User editor, Role a_role, bool isPtAdmin = false);
    public Task UpdateRolesInCompanyToNewPermissions(int CompanyId);
    Task<bool> DeleteAsync(int groupId);
}