namespace WebAPI.Models.Integration
{
    public class PlanningAreaIntegrationConfig
    {
        public int PlanningAreaId { get; set; }
        public PADetails PlanningArea { get; set; }
        public int IntegrationConfigId { get; set; }
        public IntegrationConfig IntegrationConfig { get; set; }
    }
}
