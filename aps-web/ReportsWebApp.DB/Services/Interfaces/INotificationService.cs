using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services.Interfaces
{
    /// <summary>
    /// Defines methods for managing notifications and user subscriptions.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Seeds initial notification types into the database if they do not exist.
        /// </summary>
        Task SeedInitialData();

        /// <summary>
        /// Retrieves a list of notifications for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of notifications for the user.</returns>
        Task<List<Notification>> GetNotificationsAsync(int userId);

        /// <summary>
        /// Marks a specific notification as read.
        /// </summary>
        /// <param name="notification">The notification to mark as read.</param>
        Task MarkAsReadAsync(Notification notification);

        /// <summary>
        /// Marks multiple notifications as read.
        /// </summary>
        /// <param name="notifications">The list of notifications to mark as read.</param>
        Task MarkAllAsReadAsync(List<Notification> notifications);

        /// <summary>
        /// Adds a new notification to the database.
        /// </summary>
        /// <param name="notification">The notification to add.</param>
        Task AddAsync(Notification notification);

        /// <summary>
        /// Adds a notification for all eligible users in a company.
        /// </summary>
        /// <param name="companyId">The ID of the company.</param>
        /// <param name="type">The type of notification.</param>
        /// <param name="text">The notification message.</param>
        /// <param name="name">The name of the notification (default is "New Notification").</param>
        Task AddNotificationForCompany(int companyId, ENotificationType type, string text, string name = "New Notification");

        /// <summary>
        /// Marks a notification as deleted.
        /// </summary>
        /// <param name="notification">The notification to delete.</param>
        Task DeleteAsync(Notification notification);

        /// <summary>
        /// Retrieves all available notification types.
        /// </summary>
        /// <returns>A list of all notification types.</returns>
        List<NotificationType> GetAllNotificationTypes();

        /// <summary>
        /// Retrieves the list of notification types a user is subscribed to, filtered by permissions.
        /// </summary>
        /// <param name="userEmail">The email of the user.</param>
        /// <returns>A list of subscribed notification types.</returns>
        Task<List<NotificationType>> GetSubscribedNotificationTypesAsync(string userEmail);

        /// <summary>
        /// Updates a user's notification subscriptions.
        /// </summary>
        /// <param name="user">The user whose subscriptions are being updated.</param>
        /// <param name="types">The list of notification types to subscribe to.</param>
        Task SetSubscriptionsAsync(User user, List<NotificationType> types);

    }
}
