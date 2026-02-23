using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.DB.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;

        public PermissionService(IDbContextFactory<DbReportsContext> factory)
        {
            _dbContextFactory = factory;
        }

        /// <summary>
        /// Creates a new scoped instance of <see cref="DbReportsContext"/>.
        /// </summary>
        private DbReportsContext CreateDbContext() =>
            _dbContextFactory.CreateDbContext();

        /// <summary>
        /// Retrieves permission keys for a given role.
        /// </summary>
        public Task<IReadOnlyList<PermissionKey>> GetDefaultAdminPermissionsAsync() =>
            GetPermissionKeysAsync(Permission.DefaultAdminPermissions);

        public Task<IReadOnlyList<PermissionKey>> GetDefaultReportAdminPermissionsAsync() =>
            GetPermissionKeysAsync(Permission.DefaultReportAdminPermissions);

        public Task<IReadOnlyList<PermissionKey>> GetDefaultUserPermissionsAsync() =>
            GetPermissionKeysAsync(Permission.DefaultUserPermissions);

        /// <summary>
        /// Filters a list of users to return only those who can receive a specific notification.
        /// </summary>
        public async Task<IReadOnlyList<User>> FilterUsersByNotificationAsync(IReadOnlyList<User> users, ENotificationType type)
        {
            var permissionChecks = users.Select(user => CheckUserNotificationPermissionAsync(user, type));
            var results = await Task.WhenAll(permissionChecks);
            return results.Where(x => x.HasPermission).Select(x => x.User).ToList();
        }

        /// <summary>
        /// Filters a list of notification types, returning only those the user has permission for.
        /// </summary>
        public List<NotificationType> FilterNotificationsForUser(User user, IReadOnlyList<ENotificationType> allNotificationTypes)
        {
            var permissionChecks = allNotificationTypes
                .Select(async type => new { Notification = type, Allowed = await HasNotificationPermissionAsync(user, type) });

            var results = Task.WhenAll(permissionChecks).Result;
            return results.Where(x => x.Allowed).Select(x => NotificationType.NotificationTypeTable[x.Notification]).ToList();
        }

        /// <summary>
        /// Determines whether a user is allowed to receive a specific notification type.
        /// </summary>
        public Task<bool> HasNotificationPermissionAsync(User user, ENotificationType type) =>
            Task.FromResult(type switch
            {
                ENotificationType.XlsImportComplete or ENotificationType.XlsImportFailed => user.IsAuthorizedFor(Permission.ManageIntegration),
                ENotificationType.NewUser or ENotificationType.UserRolesChanged => user.IsAuthorizedFor(Permission.EditUsers),
                ENotificationType.NewRole => user.IsAuthorizedFor(Permission.EditUsers),
                _ => true // General notifications are available to all users
            });

        /// <summary>
        /// Retrieves permission keys from the database for a given set of permissions.
        /// </summary>
        private async Task<IReadOnlyList<PermissionKey>> GetPermissionKeysAsync(IReadOnlyList<Permission> permissions)
        {
            return AuthUtils.AllPermissions.Where(p => permissions.Any(perm => perm.Key == p.Key)).Select(p => new PermissionKey(p.Key)).ToList();
        }

        /// <summary>
        /// Checks if a user has permission for a notification and returns both user and permission status.
        /// </summary>
        private async Task<(User User, bool HasPermission)> CheckUserNotificationPermissionAsync(User user, ENotificationType type) =>
            (user, await HasNotificationPermissionAsync(user, type));
    }
}
