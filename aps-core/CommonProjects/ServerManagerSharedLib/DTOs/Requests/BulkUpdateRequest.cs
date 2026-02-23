using System.Runtime.Serialization;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class BulkUpdateRequest
    {
        public int ScenarioLimit { get; set; }
        public CopilotRequest CoPilotRequest { get; set; }
    }
}
