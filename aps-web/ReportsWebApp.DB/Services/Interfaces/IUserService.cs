using Microsoft.AspNetCore.Components.Authorization;
using ReportsWebApp.DB.Models;

public interface IUserService
{
    Task<User> GetCurrentUserAsync(AuthenticationState authState);
    Task<User> GetCurrentUserAsync(AuthenticationStateProvider provider);
    Task<List<User>> GetUsersAsync(List<Role>? userRoles = null, int companyId = 0);
    User GetUserByEmail(string userEmail);
    User GetUserById(int id);
    Task<User> SaveAsync(User user, bool isAdminEdit);
    public Task<User> UpdateUserTimeZone(User a_user);
    Task<bool> RemoveAsync(int userId, int companyId);
    Task<bool> DeleteAsync(int userId);
    Task<List<Company>> GetCompanySwitchList(string userEmail);
    Task<List<Company>> GetAvailableCompaniesForUser(string userEmail);
    Task<Company?> GetUserSelectedCompany(string userEmail);
    Task<bool> SwitchUserCompany(string userEmail, int companyId);
    Task<string> CreateUserInviteLink(User user);
    Task<User?> GetUserByInviteToken(string token);
    Task ClearUserInviteToken(string token);
    Task SaveUserGridLayout(SavedGridLayout newLayout, User user);
}