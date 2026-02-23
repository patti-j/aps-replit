namespace ReportsWebApp.DB.Models
{
    public class Dasht_Materials
    {
        public string? PlanningAreaName { get; set; }

        public string? JobName { get; set; }

        public string? Moname { get; set; }

        public string? Opname { get; set; }

        public string? ActivityExternalId { get; set; }

        public string? MaterialName { get; set; }

        public string? SupplyingJobName { get; set; }

        public string? SupplyingMoname { get; set; }

        public string? SupplyingOpname { get; set; }

        public string? SupplyingActivityExternalId { get; set; }

        public decimal? SuppliedQty { get; set; }

        public int? SupplyBomLevel { get; set; }

        public DateTime? PublishDate { get; set; }
        
        public string? ScenarioName { get; set; }

        public string? ScenarioType { get; set; }

        public string? PlanningAreaId { get; set; }
    }
}
