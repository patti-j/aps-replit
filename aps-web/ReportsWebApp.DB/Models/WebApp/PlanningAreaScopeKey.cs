using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReportsWebApp.DB.Models
{
    public class PlanningAreaScopeAssociationKey
    {
        public int Id { get; set; }
        [ForeignKey(nameof(PlanningAreaScope))]
        public int ScopeId { get; set; }
        public virtual PlanningAreaScope Scope { get; set; }
        /// <summary>
        /// A key that determines which scopes should be automatically included with the scope
        /// </summary>
        public string ScopeAssociationKey { get; set; }

        public override bool Equals(object other)
        {
            if (other is PlanningAreaScopeAssociationKey scopeKey)
            {
                return scopeKey.ScopeAssociationKey == ScopeAssociationKey;
            }

            return false;
        }
    }
}
