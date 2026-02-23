using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReportsWebApp.DB.Models
{
    public class Team : BaseEntity
    {
        [ConcurrencyCheck]
        [Timestamp]
        public byte[] Version { get; set; }
        public string Description { get; set; }
        public string UpdatedBy { get; set; }
        [ForeignKey("Company")] public int CompanyId { get; set; }

        [Required] public virtual Company Company { get; set; }
        public List<ScopedRole> DBRolesAndScopes { get; set; }

        [NotMapped]
        public List<ScopedRole> RolesAndScopes
        {
            get
            {
                return DBRolesAndScopes.Concat(ComputedRolesAndScopes.Select(x => new ScopedRole() { RoleId = x.RoleId, ScopeId = x.ScopeId, TeamId = x.TeamId })).ToList();
            }
        }

        public List<ComputedScopedRole> ComputedRolesAndScopes { get; set; }
        public virtual List<User> Users { get; set; } = new();
        public override string DetailDisplayValue => $"{Company?.Name} - {Name}";

        /// <summary>
        /// Used to mark that a Scope is a calculated Scope, and is not stored in the DB.
        /// </summary>
        [NotMapped]
        public bool Readonly { get; set; } = false;
    }
}
