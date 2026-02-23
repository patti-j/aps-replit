using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class PlanningAreaIntegrationConfig
    {
        public int PlanningAreaId { get; set; }
        public PADetails PlanningArea { get; set; }
        public int IntegrationConfigId { get; set; }
        public IntegrationConfig IntegrationConfig { get; set; }
    }
}
