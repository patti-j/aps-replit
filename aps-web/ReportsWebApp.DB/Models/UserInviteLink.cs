using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class UserInviteLink
    {
        [Key]
        [Required]
        public int Id { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public DateTime Expiration { get; set; }
    }
}
