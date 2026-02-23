using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models;

public class ScopedRole
{
    [NotMapped]
    public int Id { get; set; }
    [ForeignKey(nameof(Team))]
    public int TeamId { get; set; }
    public Team Team { get; set; }
    
    public virtual PlanningAreaScope Scope { get; set; }
    [ForeignKey(nameof(Scope))]
    public int ScopeId { get; set; }
    public virtual Role Role { get; set; }
    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }
}

public class ComputedScopedRole
{
    public int TeamId { get; set; }
    public int ScopeId { get; set; }
    public int RoleId { get; set; }
}