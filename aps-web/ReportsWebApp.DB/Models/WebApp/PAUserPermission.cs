using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReportsWebApp.DB.Models
{
    public class PAUserPermission
    {
        [Key]
        [Required]
        public int Id { get; set; }
        // If company is null, that means it's a default permission that should be visible to all
        [ForeignKey("Company")]
        public int? CompanyId { get; set; } = null;
        public virtual Company? Company { get; set; } = null;
        public List<PAPermissionGroup> Groups { get; set; }
        public string PackageObjectId { get; set; }
        public string GroupKey { get; set; }
        public string PermissionKey { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public bool? Allowed { get; set; }
    }
}
