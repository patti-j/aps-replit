using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class SchedulerFavorite : BaseEntity
    {
        public Guid CosmosDbRecordId { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual List<Role> Roles { get; set; } = new();

        //public virtual List<Group> Groups { get; } = new();
        public string PlanningArea { get; set; }
        public string ScenarioName { get; set; }
    }

}
