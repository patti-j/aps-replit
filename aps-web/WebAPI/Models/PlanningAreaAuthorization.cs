using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    /// <summary>
    /// This entity authorizes a PlanningAreaLogin to log in to a specific Planning Area
    /// Name and Version are included so that the PA can still be shown even if the server is offline
    /// </summary>
    [PrimaryKey(nameof(PlanningAreaLoginId), nameof(PlanningAreaKey))]
    public class PlanningAreaAuthorization
    {
        // This is the value refers to 'InstanceIdentifier' on the internal 'Instance' object
        public string PlanningAreaKey { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool Connected = true;
        public int PlanningAreaLoginId { get; set; }
        public PlanningAreaLogin PlanningAreaLogin { get; set; }
    }
}
