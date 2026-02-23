using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class Notification : BaseEntity
    {
        [ForeignKey("User")] public int UserId { get; set; }

        [Required] public virtual User User { get; set; }
        
        public string Text { get; set; }
        public bool Read { get; set; }
        public bool Deleted { get; set; }
        public ENotificationType Type { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Notification notif)
            {
                if (Id == notif.Id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
