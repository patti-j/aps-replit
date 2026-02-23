using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class PlanningAreaLocation : BaseEntity
    {
        [ForeignKey("CompanyServer")] public int ServerId { get; set; }
        [Required] public virtual CompanyServer Server { get; set; }
        public string Environment { get; set; }
        [ForeignKey("PlanningAreaLocation")] public int? ParentId { get; set; }
        public virtual PlanningAreaLocation? Parent { get; set; }
        public virtual List<PlanningAreaLocation> Children { get; set; }

        public virtual List<PADetails> PlanningAreas { get; set; }
    }
}
