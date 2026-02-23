using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.DB.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly IPermissionService _permissionService;
        private readonly IConfiguration _configuration;

        public NotificationService(IConfiguration configuration, IDbContextFactory<DbReportsContext> dbContext, HttpClient httpClient, NavigationManager navigationManager, IPermissionService permissionService)
        {
            _dbContextFactory = dbContext;
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _configuration = configuration;
            _permissionService = permissionService;
        }

        /// <summary>
        /// Seeds initial notification data into the database.
        /// </summary>
        public async Task SeedInitialData()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            
        }

        /// <summary>
        /// Retrieves all notifications for a user, filtered by their subscriptions.
        /// </summary>
        public async Task<List<Notification>> GetNotificationsAsync(int userId)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var user = await dbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null) return new List<Notification>();

            var subscribedTypes = user.SubscribedNotifications.Select(s => (int)s).ToList();

            return await dbContext.Notifications
                .Where(n => n.UserId == userId && !n.Deleted && subscribedTypes.Contains((int)n.Type)) 
                .OrderByDescending(n => n.CreationDate)
                .ToListAsync();
        }

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        public async Task MarkAsReadAsync(Notification notification)
        {
            await UpdateNotificationStatusAsync(notification, n => n.Read = true);
        }

        /// <summary>
        /// Marks multiple notifications as read.
        /// </summary>
        public async Task MarkAllAsReadAsync(List<Notification> notifications)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var notificationIds = notifications.Where(n => !n.Read).Select(n => n.Id).ToList();

            if (!notificationIds.Any()) return;

            await dbContext.Notifications
                .Where(n => notificationIds.Contains(n.Id))
                .ExecuteUpdateAsync(n => n.SetProperty(x => x.Read, true));

            await dbContext.SaveChangesAsync();
        }


        /// <summary>
        /// Adds a new notification.
        /// </summary>
        public async Task AddAsync(Notification notification)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.Notifications.AddAsync(notification);
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a notification for all eligible users in a company.
        /// </summary>
        public async Task AddNotificationForCompany(int companyId, ENotificationType type, string text, string name = "New Notification")
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var users = await GetEligibleUsersForNotificationAsync(dbContext, companyId, type);

            var notifications = users.Select(user => new Notification
            {
                UserId = user.Id,
                Type = type,
                Text = text,
                Read = false,
                Deleted = false,
                Name = name
            }).ToList();

            await dbContext.Notifications.AddRangeAsync(notifications);
            await dbContext.SaveChangesAsync();

            await NotifyUsersAsync(users);
        }

        /// <summary>
        /// Updates notification status.
        /// </summary>
        private async Task UpdateNotificationStatusAsync(Notification notification, Action<Notification> updateAction)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.Notifications.Attach(notification);
            updateAction(notification);
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves users eligible to receive a specific notification type.
        /// </summary>
        private async Task<IReadOnlyList<User>> GetEligibleUsersForNotificationAsync(DbReportsContext dbContext, int companyId, ENotificationType type)
        {
            var users = await dbContext.Users
                .Include(u => u.Roles)
                .Where(u => u.CompanyId == companyId && u.SubscribedNotifications.Any(nt => nt == type))
                .ToListAsync();

            return await _permissionService.FilterUsersByNotificationAsync(users, type);
        }

        /// <summary>
        /// Sends notification to eligible users via HTTP requests.
        /// </summary>
        private async Task NotifyUsersAsync(IEnumerable<User> users)
        {
            var accessToken = _configuration["NotificationEndpointAccessToken"];

            foreach (var user in users)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, _navigationManager.ToAbsoluteUri($"api/Notification/notify?userEmail={user.Email}"));
                request.Headers.Add("Access-Token", accessToken);
                await _httpClient.SendAsync(request);
            }
        }
        /// <summary>
        /// Deletes a notification by marking it as deleted.
        /// </summary>
        public async Task DeleteAsync(Notification notification)
        {
            await UpdateNotificationStatusAsync(notification, n => n.Deleted = true);
        }

        /// <summary>
        /// Retrieves all notification types.
        /// </summary>
        public List<NotificationType> GetAllNotificationTypes()
        {
            return NotificationType.NotificationTypes;
        }

        /// <summary>
        /// Retrieves the notification types a user is subscribed to, filtered by permissions.
        /// </summary>
        public async Task<List<NotificationType>> GetSubscribedNotificationTypesAsync(string userEmail)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            return user == null
                ? new List<NotificationType>()
                : (_permissionService.FilterNotificationsForUser(user, user.SubscribedNotifications)).ToList();
        }

        /// <summary>
        /// Sets notification subscriptions for a user.
        /// </summary>
        public async Task SetSubscriptionsAsync(User user, List<NotificationType> editedTypesFromUI)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var existingUser = await LoadUserWithSubscriptionsAsync(dbContext, user.Id);
            var allNotificationTypes = NotificationType.NotificationTypes;

            foreach (var editedType in editedTypesFromUI)
            {
                var typesInCategory = GetTypesInCategory(allNotificationTypes, editedType.Category);
                var isSubscribed = IsUserSubscribedToCategory(existingUser, editedType.Category);
                var shouldBeSubscribed = editedType.Checked;

                if (shouldBeSubscribed && !isSubscribed)
                    SubscribeUserToCategory(existingUser, typesInCategory.Select(t => t.Type).ToList());

                else if (!shouldBeSubscribed && isSubscribed)
                    UnsubscribeUserFromCategory(existingUser, typesInCategory.Select(t => t.Type).ToList());
            }

            dbContext.Users.Update(existingUser);
            await dbContext.SaveChangesAsync();
        }
        private async Task<User> LoadUserWithSubscriptionsAsync(DbReportsContext dbContext, int userId)
        {
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user ?? throw new Exception("User not found.");
        }

        private List<NotificationType> GetTypesInCategory(IEnumerable<NotificationType> allTypes, ENotificationCategory category)
        {
            return allTypes.Where(t => t.Category == category).ToList();
        }

        private bool IsUserSubscribedToCategory(User user, ENotificationCategory category)
        {
            return user.SubscribedNotifications.Any(t => NotificationType.NotificationTypeTable[t].Category == category);
        }

        private void SubscribeUserToCategory(User user, List<ENotificationType> typesInCategory)
        {
            if (!user.SubscribedNotifications.Any(n => typesInCategory.Contains(n)))
            {
                user.SubscribedNotifications.Add(typesInCategory.First());
            }
        }

        private void UnsubscribeUserFromCategory(User user, List<ENotificationType> typesInCategory)
        {
            var toRemove = user.SubscribedNotifications
                .Where(nt => typesInCategory.Any(tc => tc == nt))
                .ToList();

            foreach (var unsub in toRemove)
            {
                user.SubscribedNotifications.Remove(unsub);
            }
        }

    }
}
