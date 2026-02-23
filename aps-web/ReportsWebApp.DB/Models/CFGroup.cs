using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class CFGroup : BaseEntity
    {
        [ForeignKey("Company")] public int CompanyId { get; set; }
        [Required] public virtual Company Company { get; set; }
        public virtual List<CustomField> CustomFields { get; set; }
    }
}
