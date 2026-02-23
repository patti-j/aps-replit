using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class SavedGridLayout : BaseEntity
    {
        [ForeignKey("Companies")]
        public int CompanyId { get; set; }
        [ForeignKey("Users")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string? PlanningAreaGridJson { get; set; }

        // For the future, if we wish to share Layouts among users
        public bool Shared = false;
    }
}
