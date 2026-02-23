using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class Category : BaseEntity
    {
        [ConcurrencyCheck]
        [Timestamp]
        public byte[] Version { get; set; }
        public string UpdatedBy { get; set; }
        [ForeignKey("Company")] public int CompanyId { get; set; }

        [Required] public virtual Company Company { get; set; }
        // Navigation property for relationships
        public List<Report> Reports { get; } = new();
        public List<Role> Roles { get; set; } = new();

        [NotMapped]
        public override string TypeDisplayName => "Report Category";
    }
}
