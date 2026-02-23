using ReportsWebApp.Common;
using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface IPermissionService
{

    /// <summary>
    /// Retrieves the default admin permissions.
    /// </summary>
    Task<IReadOnlyList<PermissionKey>> GetDefaultAdminPermissionsAsync();

    /// <summary>
    /// Retrieves the default report admin permissions.
    /// </summary>
    Task<IReadOnlyList<PermissionKey>> GetDefaultReportAdminPermissionsAsync();

    /// <summary>
    /// Retrieves the default user permissions.
    /// </summary>
    Task<IReadOnlyList<PermissionKey>> GetDefaultUserPermissionsAsync();

    /// <summary>
    /// Determines if a user has permission to receive a specific notification.
    /// </summary>
    Task<bool> HasNotificationPermissionAsync(User user, ENotificationType type);

    /// <summary>
    /// Filters a list of users to return only those who are allowed to receive a specific notification type.
    /// </summary>
    Task<IReadOnlyList<User>> FilterUsersByNotificationAsync(IReadOnlyList<User> users, ENotificationType type);

    /// <summary>
    /// Filters a list of notification types, returning only those the user has permission for.
    /// </summary>
    public List<NotificationType> FilterNotificationsForUser(User user, IReadOnlyList<ENotificationType> allNotificationTypes);
}
