using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class PlanningAreaTag : BaseEntity
    {
        [ForeignKey("Company")] public int CompanyId { get; set; }
        [Required] public virtual Company Company { get; set; }
        public virtual List<PADetails> PlanningAreas { get; set; }
    }
}
