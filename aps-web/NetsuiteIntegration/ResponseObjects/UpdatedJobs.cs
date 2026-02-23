using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetsuiteIntegration.ResponseObjects
{
    public class UpdatedJobs
    {
        public int PT_WorkOrderID { get; set; }
        public string PT_WorkOrderName { get; set; }
        public DateTime PT_WorkOrderStartDate { get; set; }
        public DateTime PT_WorkOrderEndDate { get; set; }
        public int PT_WorkOrderMFGRouteID { get; set; }
        public string PT_WorkOrderMFGRouteName { get; set; }
        public int PT_WorkOrderBOMID { get; set; }
        public string PT_WorkOrderBOMName { get; set; }
        //public int PT_OpID { get; set; }
        //public string PT_OpName { get; set; }
        //public DateTime PT_OpStartDate { get; set; }
        //public DateTime PT_OpEndDate { get; set; }
        //public string PT_ResourcesUsed { get; set; }
        //public string PT_Status { get; set; }        
    }
}
