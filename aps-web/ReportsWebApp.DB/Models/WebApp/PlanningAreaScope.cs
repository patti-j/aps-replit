using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReportsWebApp.DB.Models
{
    public class PlanningAreaScope : BaseEntity
    {
        [ConcurrencyCheck]
        [Timestamp]
        public byte[] Version { get; set; }
        public string Description { get; set; }
        public string UpdatedBy { get; set; }
        [ForeignKey("Company")] public int CompanyId { get; set; }

        [Required] public virtual Company Company { get; set; }
        [NotMapped] // TODO: Add mapping entity
        public List<PADetails> PlanningAreas { get; set; } = new();
        public List<PlanningAreaScopeAssociationKey> PlanningAreaScopeAssociationKeys { get; set; } = new();
        public override string DetailDisplayValue => $"{Company?.Name} - {Name}";

        /// <summary>
        /// Used to mark that a Scope is a calculated Scope, and is not stored in the DB.
        /// </summary>
        [NotMapped]
        public bool Readonly { get; set; } = false;
    }
}
