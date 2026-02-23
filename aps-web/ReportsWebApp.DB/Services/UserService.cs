using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services;
using ReportsWebApp.Shared;
using System.Transactions;

using Microsoft.PowerBI.Api;

using ReportsWebApp.DB.Services.Interfaces;
using System.ComponentModel.Design;
using System.Data;
using System.Security.Cryptography;

public class UserService : IUserService
{
    private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;
    private readonly INotificationService _notificationService;
    private readonly IRoleService m_roleService;
    private readonly IAppInsightsLogger _logger;


    public UserService(IDbContextFactory<DbReportsContext> dbContextFactory, INotificationService notificationService, IRoleService a_roleService, IAppInsightsLogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _notificationService = notificationService;
        m_roleService = a_roleService;
        _logger = logger;
    }

    public async Task<User> GetCurrentUserAsync(AuthenticationState authState)
    {
        var userEmailClaim = AuthUtils.GetEmailFromClaim(authState.User);
        User user = GetUserByEmail(userEmailClaim);
        user.AuthState = authState;

        return user;
    }

    public async Task<User> GetCurrentUserAsync(AuthenticationStateProvider provider)
    {
        var authState = await provider.GetAuthenticationStateAsync();
        return await GetCurrentUserAsync(authState);
    }

    public async Task<List<User>> GetUsersAsync(List<Role>? userRoles = null, int companyId = 0)
    {
        userRoles ??= await m_roleService.GetRoles(companyId);

        return await GetUsersByRole(userRoles);
    }

    private async Task<List<User>> GetUsersByCompanyId(int companyId, List<Role> roles)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Users.Include(u => u.Company)
            .Include(r => r.Roles)
            .ThenInclude(g => g.Company)
            .Where(u => u.CompanyId == companyId && u.Roles.Any(g => roles.Select(x => x.Id).Any(l => l == g.Id)))
            .ToListAsync();
    }

    private async Task<List<User>> GetUsersByRole(List<Role> roles)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var roleIds = roles.Select(g => g.Id).ToList();

        return await dbContext.Users.Include(u => u.Company)
            .Include(r => r.Roles)
            .ThenInclude(g => g.Company)
            .Where(u => u.Roles.Select(g => g.Id).Any(id => roleIds.Contains(id)))
            .ToListAsync();
    }

    public User GetUserByEmail(string userEmail)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var user = (dbContext.Users
            .Include(u => u.SavedGridLayouts)
            .Include(u => u.Company)
                .ThenInclude(c => c.Workspaces)
            .Include(g => g.Roles)// Assuming Workspaces is the navigation property in the User entity
            .Include(g => g.Roles)
            .ThenInclude(g => g.Categories)
            .Include(g => g.Roles)
            .ThenInclude(g => g.Company)
            //.AsNoTracking()
            .FirstOrDefault(u => u.Email == userEmail)) ?? new User { Email = userEmail };
        return user;
    }

    public User GetUserById(int id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var user = (dbContext.Users
            .Include(u => u.SavedGridLayouts)
            .Include(u => u.Company)
                .ThenInclude(c => c.Workspaces)
            .Include(g => g.Roles)// Assuming Workspaces is the navigation property in the User entity
            .Include(g => g.Roles)
            .ThenInclude(g => g.Categories)
            .FirstOrDefault(u => u.Id == id)) ?? new User();
        return user;
    }

    public async Task<User> UpdateUserTimeZone(User a_user)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        User? existingUser = dbContext.Users.FirstOrDefault( u => u.Id == a_user.Id);
        if (existingUser == null) throw new InvalidOperationException("Attempted to update TimeZone for user that does not exist.");
        existingUser.TimeZone = a_user.TimeZone;
        dbContext.Update(existingUser);
        await dbContext.SaveChangesAsync();
        return existingUser;
    }

    public async Task<User> SaveAsync(User a_userModel, bool a_isElevatedEdit)
    {
        bool isNew;
        bool hasRoleChanges;
        (ICollection<Role> added, ICollection<Role> removed) rolesDiff;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (a_userModel == null)
        {
            throw new ArgumentNullException(nameof(a_userModel));
        }

        if (string.IsNullOrEmpty(a_userModel.Email))
        {
            throw new ArgumentException("User email cannot be empty.", nameof(a_userModel.Email));
        }

        if (a_userModel.IsPTAdmin() && !a_userModel.Email.EndsWith("@planettogether.com"))
        {
            throw new ArgumentException("Cannot assign the role PT Admin to a user outside of the Planet Together Domain.");
        }

        if (!a_userModel.Roles.Any())
        {
            throw new ArgumentException("Users must have at least one role.");
        }

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var existingUser = dbContext.Users
                .Include(u => u.Roles)
                .AsNoTracking()
                .FirstOrDefault(u => u.Email.Equals(a_userModel.Email));

            if (existingUser == null && a_userModel.Id != 0)
            {
                // User email was not found but id exists, email must have changed
                existingUser = dbContext.Users
                    .Include(u => u.Roles)
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Id == a_userModel.Id);
                isNew = false;
            }
            else if (existingUser != null)
            {
                // Existing user found, determine whether to update or import below
                isNew = false;
            }
            else
            {
                // User is new, add new entity
                isNew = true;
            }

            // Collect role changes before main user update, so it can be cleared from the model
            var modelRoles = a_userModel.Roles.ToList();
            // Ensure role data is loaded
            foreach (var role in modelRoles)
            {
                try
                {
                    role.Company = dbContext.Companies.AsNoTracking().First(x => x.Id == role.CompanyId);
                }
                catch (Exception e)
                {
                    throw new InvalidDataException("Failed to load company data.");
                }
            }

            var existingRoles = existingUser?.Roles.ToList() ?? new List<Role>();
            rolesDiff = existingRoles.Synchronize(modelRoles);
            // Validate new roles
            ValidateRoles(a_userModel.Email, rolesDiff.added);

            // Handle updating base entity
            if (isNew)
            {
                a_userModel.Roles = null; // Skip handling this in main entity update
                await AddNewUserAsync(a_userModel, dbContext); // Saves context, which assigns an Id for below
            }
            else
            {
                if (ShouldUserEntityBeUpdated(a_isElevatedEdit, existingUser, rolesDiff.added))
                {

                    if (!existingUser.Email.Equals(a_userModel.Email, StringComparison.InvariantCultureIgnoreCase) && a_userModel.AuthorizedCompanyIds.Count() > 1)
                    {
                        if (a_isElevatedEdit)
                        {
                            ValidateRoles(a_userModel.Email, a_userModel.Roles);
                        }
                        else
                        {
                            throw new InvalidDataException(
                                $"Cannot update {existingUser.Email}'s email as their account is registered in more than one company. You will need to contact a PlanetTogether administrator to request an email change if it's necessary.");
                        }
                    }
                    a_userModel.Roles = null; // Skip handling this in main entity update
                    await UpdateExistingUserAsync(a_userModel, dbContext);
                }
            }

            // Handle updating Roles
            await UpdateAssignedPermissionRoles(modelRoles, existingRoles, rolesDiff, a_userModel.Id, dbContext);

            hasRoleChanges = rolesDiff.added.Any() || rolesDiff.removed.Any();

            scope.Complete();
        }

        // Notify results
        if (isNew)
        {
            await NotifyUserCreated(a_userModel);
        }

        if (hasRoleChanges)
        {
            await NotifyRoleChange(a_userModel, rolesDiff);
        }

        return GetUserByEmail(a_userModel.Email);
    }

    /// <summary>
    /// Validates that a given email is valid for all provided roles
    /// </summary>
    /// <param name="a_email">The email to check</param>
    /// <param name="a_permissionsRoles">The roles to validate against</param>
    /// <exception cref="InvalidDataException">Throws an InvalidDataException if the email is not valid</exception>
    private static void ValidateRoles(string a_email, ICollection<Role> a_permissionsRoles)
    {
        string[] userEmailParts = a_email.Split("@");
        if (userEmailParts.Length != 2) throw new InvalidDataException("Invalid User Email."); //technically emails can contain more than one '@' but it's exceedingly rare for people to actually do this
        string userDomain = userEmailParts[1].Trim().ToLower();

        //service users are created with .invalid tld's to ensure that they can never receive email
        //if you tried to create a normal user with a .invalid tld they wont be able to sign in either due to no identity provider
        //authenticating a .invalid domain or the user will never be able to receive the verification email.
        if (userDomain.EndsWith(".invalid"))
        {
            return;
        }
        
        IEnumerable<Company> companies = a_permissionsRoles.Select(role => role.Company).Distinct();
        foreach (Company company in companies)
        {
            if (userDomain != "planettogether.com" && 
                company.AllowedDomains != null && 
                !company.AllowedDomains.Split(",").Select(s => s.Trim().ToLower()).Contains(userDomain))
            {
                // Generally, we'd want to avoid showing any company's name - but since this was a newly added permission, it must be for a company the user can see.
                throw new InvalidDataException($"The domain {userDomain} is not allowed for {company.Name}. Please contact a PlanetTogether Administrator if you believe this is incorrect.");
            }
        }
    }

    /// <summary>
    /// The <see cref="SaveAsync"/> endpoint is used when updating a user, which includes the use case of adding an existing user to a different company's roles.
    /// In that last situation, the person making the edit doesn't actually know the user exists, so we want to ignore their values for the base User entity (and skip to Role assignment)
    /// </summary>
    /// <param name="a_isElevatedEdit"></param>
    /// <param name="a_userModel"></param>
    /// <param name="a_existingUser"></param>
    /// <returns></returns>
    private bool ShouldUserEntityBeUpdated(bool a_isElevatedEdit, User a_existingUser, ICollection<Role> a_addedPermissionsRoles)
    {
        // Always allow updates from admin "All Users" page - they'll always know the user exists
        if (a_isElevatedEdit) 
        {
            return true;
        }

        // Assert no new company's roles were assigned to the user
        return a_addedPermissionsRoles.All(permissionRole => a_existingUser.AuthorizedCompanyIds.Contains(permissionRole.CompanyId));
    }

    private static async Task AddNewUserAsync(User a_userModel, DbReportsContext dbContext)
    {
        if (a_userModel.Version != null)
        {
            //user got deleted while editing
            throw new DbUpdateConcurrencyException("User was deleted while you were editing. Please retry");
        }

        a_userModel.ExternalId = Guid.NewGuid().ToString();
        a_userModel.CreationDate = DateTime.UtcNow;

        // New users should default to having all notification types enabled
        // (but may not get all types based on current permissions)
        NotificationType.NotificationTypes.ForEach(nt => a_userModel.SubscribedNotifications.Add(nt.Type));
        dbContext.Users.Add(a_userModel);

        await dbContext.SaveChangesAsync();
    }

    private static async Task UpdateExistingUserAsync(User a_userModel, DbReportsContext dbContext)
    {
        dbContext.Users.Attach(a_userModel);
        dbContext.Users.Update(a_userModel);
    }

    private static async Task UpdateAssignedPermissionRoles(List<Role> a_userModelRoles,
        List<Role> a_userExistingRoles, (ICollection<Role> added, ICollection<Role> removed) a_rolesDiff,
        int a_userId, DbReportsContext a_dbContext)
    {
        var removedRoleIds = a_rolesDiff.removed.Select(x => new UserRole()
        {
            UsersId = a_userId,
            RoleId = x.Id
        });
        List<User> ptUsers = a_dbContext.Users.Where(u => u.Roles.Any(g => g.Name.ToLower() == "ptadmin")).ToList();
        var ptRoleId = a_dbContext.Roles.FirstOrDefault(g => g.Name.ToLower() == "ptadmin").Id;
        if (ptUsers.Count() == 1 && ptUsers.Any(u => u.Id == a_userId) && removedRoleIds.Any(g => g.RoleId == ptRoleId))
        {
            throw new InvalidDataException("Cannot remove the last PTAdmin from the system");
        }
        var userRoleDbSet = a_dbContext.Set<UserRole>(nameof(UserRole));
        var newRoleIds = a_rolesDiff.added.Select(x => new UserRole()
        {
            UsersId = a_userId,
            RoleId = x.Id
        });
        userRoleDbSet.AddRange(newRoleIds);
        

        if (removedRoleIds.Any())
        {

        }

        userRoleDbSet.RemoveRange(removedRoleIds);

        await a_dbContext.SaveChangesAsync();
    }

    private async Task NotifyRoleChange(User a_userModel, (ICollection<Role> added, ICollection<Role> removed) diff)
    {
        var addedRoles = diff.added.ToList();
        var removedRoles = diff.removed.ToList();
        string notificationText = $"User {a_userModel.Email} Roles updated" +
                           (addedRoles.Any()
                               ? $"\nAdded: {string.Join(", ", addedRoles.Select(x => x.Name))}"
                               : "") +
                           (removedRoles.Any()
                               ? $"\nAdded: {string.Join(", ", removedRoles.Select(x => x.Name))}"
                               : "");

        await _notificationService.AddNotificationForCompany(a_userModel.CompanyId, ENotificationType.UserRolesChanged,
            notificationText);
    }

    private async Task NotifyUserCreated(User a_userModel)
    {
        await _notificationService.AddNotificationForCompany(a_userModel.CompanyId, ENotificationType.NewUser,
            $"New User created: {a_userModel.Email}");
    }
    private async Task NotifyUserDeleted(int companyId, string message)
    {
        await _notificationService.AddNotificationForCompany(companyId, ENotificationType.UserDeleted,
            message);
    }

    public async Task<bool> RemoveAsync(int userId, int companyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var user = await dbContext.Users.Where(u => u.Id == userId).Include(g => g.Roles).FirstOrDefaultAsync();
        if (user == null) return false;

        string userEmail = user.Email;
        List<User> ptUsers = dbContext.Users.Where(u => u.Roles.Any(g => g.Name.ToLower() == "ptadmin")).ToList();
        if (ptUsers.Contains(user) && ptUsers.Count() == 1)
        {
            NotifyUserDeleted(companyId, $"Cannot delete last PTAdmin  {userEmail} from system");
            return false;
        }

        // Remove roles for this company
        user.Roles.RemoveAll(g => g.CompanyId == companyId);
        
        if (user.Roles.Count == 0)
        {
            dbContext.Users.Remove(user);
            user = null;
        } 
        else if (user.CompanyId == companyId) // If the user was set to this company, switch them to a different one.
        {
            user.CompanyId = user.Roles.First().CompanyId;
            user.Company = null;
        }

        await dbContext.SaveChangesAsync();
        NotifyUserDeleted(companyId, $"User {userEmail} Deleted");
        return true;
    }

    public async Task<bool> DeleteAsync(int userId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var user = await dbContext.Users
            .Where(u => u.Id == userId)
            .Include(g => g.Roles)
            .FirstOrDefaultAsync();
        if (user == null) return false;
        int companyId = user.CompanyId;
        string userEmail = user.Email;
        List<User> ptUsers = dbContext.Users.Where(u => u.Roles.Any(g => g.Name.ToLower() == "ptadmin")).ToList();
        if (ptUsers.Contains(user) && ptUsers.Count() == 1)
        {
            NotifyUserDeleted(companyId, $"Cannot delete last PTAdmin  {userEmail} from system");
            return false;
        }

        // Remove roles for this company
        user.Roles.Clear();
        
        dbContext.Users.Remove(user);

        await dbContext.SaveChangesAsync();
        NotifyUserDeleted(companyId, $"User {userEmail} Deleted");
        return true;
    }

    public async Task<List<Company>> GetCompanySwitchList(string userEmail)
    {
		await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

		User? user = dbContext.Users.Include(u => u.Company).Include(u => u.Roles).ThenInclude(g => g.Company)
            .Include(u => u.Roles).FirstOrDefault(x => x.Email == userEmail);

        if (user == null) return new List<Company>();

        // PT admins get access to all companies
        if (user.IsPTAdmin())
        {
            return await dbContext.Companies.Where(x => x.Id != user.Company.Id).ToListAsync();
        }

        return user.AuthorizedCompanies.Where(x => x.Id != user.Company.Id).ToList();
    }

    public async Task<List<Company>> GetAvailableCompaniesForUser(string userEmail)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        User? user = dbContext.Users.AsNoTracking().Include(u => u.Company).Include(u => u.Roles).ThenInclude(g => g.Company)
                              .Include(u => u.Roles).FirstOrDefault(x => x.Email == userEmail);

        if (user == null) return new List<Company>();

        // PT admins get access to all companies
        if (user.IsPTAdmin())
        {
            return await dbContext.Companies.AsNoTracking().ToListAsync();
        }

        return user.AuthorizedCompanies.ToList();
    }

	public async Task<Company?> GetUserSelectedCompany(string userEmail)
    {
		await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

		User? user = dbContext.Users.Include(u => u.Company).FirstOrDefault(x => x.Email == userEmail);

        if (user == null) return null;

        return user.Company;
	}


	public async Task<bool> SwitchUserCompany(string userEmail, int companyId)
    {
		await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
		User? user = dbContext.Users.Include(u => u.Company).Include(u => u.Roles).ThenInclude(g => g.Company)
            .Include(u => u.Roles).FirstOrDefault(u => u.Email == userEmail);

        // If user is null or user is not authorized to switch to this company, don't switch. PTAdmins can switch to any company
        if ((user == null || user.AuthorizedCompanies.All(x => x.Id != companyId)) && !(user?.IsPTAdmin() ?? false)) return false;

		user.CompanyId = companyId;
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<string> CreateUserInviteLink(User user)
    {
		await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Invalidate existing links for this user
        var existingLinks = dbContext.UserInviteLinks.Where(x => x.UserId == user.Id);
        dbContext.RemoveRange(existingLinks);

        var token = RandomNumberGenerator.GetHexString(64);

        dbContext.UserInviteLinks.Add(new UserInviteLink()
        {
            UserId = user.Id,
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(24)
        });

        await dbContext.SaveChangesAsync();

        return token;
    }

    public async Task<User?> GetUserByInviteToken(string token)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var link = dbContext.UserInviteLinks.FirstOrDefault(x => x.Token == token);

        if (link == null) return null;
        if (link.Expiration <= DateTime.UtcNow)
        {
            await ClearUserInviteToken(token);
            return null;
        }

        var user = dbContext.Users.FirstOrDefault(x => x.Id == link.UserId);

        return user;
    }

    public async Task ClearUserInviteToken(string token)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var link = dbContext.UserInviteLinks.FirstOrDefault(x => x.Token == token);

        if (link != null)
        {
            dbContext.UserInviteLinks.Remove(link);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task SaveUserGridLayout(SavedGridLayout newLayout, User user)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (newLayout.Id == 0)
        {
            dbContext.SavedGridLayouts.Add(newLayout);
            await dbContext.SaveChangesAsync();
        }

        var dbLayout = dbContext.SavedGridLayouts.FirstOrDefault(x => x.Id == newLayout.Id);

        if (dbLayout != null)
        {
            if (!string.IsNullOrEmpty(newLayout.PlanningAreaGridJson))
            {
                dbLayout.PlanningAreaGridJson = newLayout.PlanningAreaGridJson;
            }
            dbContext.SavedGridLayouts.Update(dbLayout);
        }

        await dbContext.SaveChangesAsync();
    }

}

