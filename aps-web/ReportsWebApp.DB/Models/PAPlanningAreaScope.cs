using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class PAPlanningAreaScope
    {
        public int PlanningAreaId { get; set; }
        public PADetails PlanningArea { get; set; }
        public int PlanningAreaScopeId { get; set; }
        public PlanningAreaScope PlanningAreaScope { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is PAPlanningAreaScope scope)
            {
                return PlanningAreaScopeId == scope.PlanningAreaScopeId && PlanningAreaId == scope.PlanningAreaId;
            }

            return false;
        }
    }
}
