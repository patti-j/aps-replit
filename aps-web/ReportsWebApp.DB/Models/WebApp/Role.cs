using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReportsWebApp.DB.Models
{
    public class Role : BaseEntity
    {
        [ConcurrencyCheck]
        [Timestamp]
        public byte[] Version { get; set; }
        public string UpdatedBy { get; set; }
        [ForeignKey("Company")] public int CompanyId { get; set; }

        [Required] public virtual Company Company { get; set; }
        public List<User> Users { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<SchedulerFavorite> GanttFavorites { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<string> DesktopPermissions { get; set; } = new();
        public DateTime LastModified { get; set; }
        public string Description { get; set; }
        public bool Readonly { get; set; }
        public override string DetailDisplayValue => $"{Company?.Name} - {Name}";
    }
}
