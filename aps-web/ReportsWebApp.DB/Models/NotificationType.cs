using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    // How the notification is displayed, such as what icon and color to show
    public enum ENotificationType
    {
        Generic = 0,
        XlsImportComplete = 1,
        XlsImportFailed = 2,
        NewUser = 3,
        NewRole = 4,
        UserRolesChanged = 5,
        UserDeleted  = 6
    }

    // Which setting category the notification should be configured under
    public enum ENotificationCategory
    {
        Generic = 1,
        XlsImport = 2,
        NewUser = 3,
        NewGroup = 4,
        UserGroupsChanged = 5,
        DeleteObject = 6
	}

	public class NotificationType
    {
		public string Name { get; set; }
        [NotMapped]
        public bool Checked { get; set; }
        // What Icon/Custom formatting to display
        public ENotificationType Type { get; set; }
        // What Settings Category this notification is determined by
		public ENotificationCategory Category { get; set; }
        
        public static readonly List<NotificationType> NotificationTypes = new List<NotificationType>()
        {
            new NotificationType()
            {
                Name = "General Notifications",
                Type = ENotificationType.Generic, Category = ENotificationCategory.Generic
            },
            new NotificationType()
            {
                Name = "Excel Import Notifications",
                Type = ENotificationType.XlsImportComplete, Category = ENotificationCategory.XlsImport
            },
            new NotificationType()
            {
                Name = "Excel Import Notifications",
                Type = ENotificationType.XlsImportFailed, Category = ENotificationCategory.XlsImport
            },
            new NotificationType()
            {
                Name = "New User Notifications",
                Type = ENotificationType.NewUser, Category = ENotificationCategory.NewUser
            },
            new NotificationType()
            {
                Name = "New Role Notifications",
                Type = ENotificationType.NewRole, Category = ENotificationCategory.NewGroup
            },
            new NotificationType()
            {
                Name = "User Role Changed Notifications",
                Type = ENotificationType.UserRolesChanged, Category = ENotificationCategory.UserGroupsChanged
            },
            new NotificationType()
            {
                Name = "User Deleted Notifications",
                Type = ENotificationType.UserDeleted, Category = ENotificationCategory.DeleteObject
            }
        };

        public static readonly Dictionary<ENotificationType, NotificationType> NotificationTypeTable = NotificationTypes.Select(n => new KeyValuePair<ENotificationType, NotificationType>(n.Type, n)).ToDictionary();
    }
}
