using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class PlanningAreaLogin
    {
        [Key]
        [Required]
        public int Id { get; set; }
        public virtual List<PlanningAreaAuthorization> PlanningAreas { get; set; } = new();
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int PAPermissionGroupId { get; set; }
        public virtual PAPermissionGroup PAPermissionGroup { get; set; }
        //public int PlantPermissionsId { get; set; } = NULL_ID;
    }
}
