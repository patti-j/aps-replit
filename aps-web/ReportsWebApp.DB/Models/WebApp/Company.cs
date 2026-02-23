using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ReportsWebApp.Common;

namespace ReportsWebApp.DB.Models
{
    public class Company : BaseEntity
    {
        [ConcurrencyCheck]
        [Timestamp]
        public byte[] Version { get; set; }
        public string UpdatedBy { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        public string? ContactName { get; set; }

        public virtual List<SchedulerFavorite> GanttFavorites { get; } = new();
        public bool Active { get; set; }
        public string? ApiKey { get; set; }
        [NotMapped]
        public string FavoriteSettingPlanningAreaIds { get; set; }
        public string? AllowedDomains { get; set; }

        public ECompanyType? CompanyType { get; set; }
        public bool? UseSSOLogin { get; set; }
        public int SoftMigrationStatus { get; set; }

        public virtual List<DBIntegration> Integrations { get; set; } = new();

        // Navigation property for the relationship
        public ICollection<PBIWorkspace> Workspaces { get; set; } = new List<PBIWorkspace>();

        /// <summary>
        /// Name of the associated company in the external license service
        /// </summary>
        public string? LicenseServiceCompanyName { get; set; }

        /// <summary>
        /// URL to view/manage this company in the license service application
        /// </summary>
        public string? LicenseServiceCompanyUrl { get; set; }

        public override int GetHashCode()
        {
            int hc = 0;
            if (Workspaces != null)
                foreach (PBIWorkspace s in Workspaces)
                    hc ^= s.GetHashCode();
            return (hc ^= base.GetHashCode()) + Workspaces.Count;
        }
    }
}
