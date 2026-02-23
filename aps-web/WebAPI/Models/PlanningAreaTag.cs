using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using WebAPI.Models.Integration;

namespace WebAPI.Models;

public class PlanningAreaTag : BaseEntity
{
    [ForeignKey("Company")] public int CompanyId { get; set; }
    [Required] public virtual Company Company { get; set; }
    public virtual List<PADetails> PlanningAreas { get; set; }
}