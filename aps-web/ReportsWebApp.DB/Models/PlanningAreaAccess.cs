using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models;

public class PlanningAreaAccess
{
    public int Id { get; set; }
    
    [ForeignKey(nameof(PlanningArea))]
    public int PlanningAreaId { get; set; }
    public virtual PADetails PlanningArea { get; set; }
    
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }
    public virtual User User { get; set; }
    
    [ForeignKey(nameof(PermissionGroup))]
    public int PermissionGroupId { get; set; }
    public virtual PAPermissionGroup PermissionGroup { get; set; }
}