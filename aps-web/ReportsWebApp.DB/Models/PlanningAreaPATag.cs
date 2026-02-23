using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class PlanningAreaPATag
    {
        public int PlanningAreaId { get; set; }
        public PADetails PlanningArea { get; set; }
        public int PAGroupId { get; set; }
        public PlanningAreaTag PAGroup { get; set; }
    }
}
