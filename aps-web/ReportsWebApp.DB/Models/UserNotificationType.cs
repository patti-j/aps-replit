using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class UserNotificationType
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int NotificationId { get; set; }
        public NotificationType Notification { get; set; }
    }
}
