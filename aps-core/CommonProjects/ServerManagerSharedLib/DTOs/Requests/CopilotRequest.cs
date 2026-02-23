namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class CopilotRequest
    {
        public bool Enabled { get; set; }
        public decimal AverageCpuUsage { get; set; }
        public decimal BoostPercentage { get; set; } 
        public decimal BurstDuration { get; set; }
    }
}
