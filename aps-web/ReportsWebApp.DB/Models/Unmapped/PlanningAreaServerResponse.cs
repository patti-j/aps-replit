using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class PlanningAreaServerResponse
    {
        public List<PlanningAreaLiteModel> PlanningAreas { get; set; } = new();
        public bool IsSuccess { get; set; } = false;
        public string ErrorMessage { get; set; } = "";

        public PlanningAreaServerResponse(List<PlanningAreaLiteModel> planningAreas) {
            IsSuccess = true;
            PlanningAreas = planningAreas;
        }

        public PlanningAreaServerResponse(string errorMessage)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage;
        }
    }
}
