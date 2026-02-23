using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class SaveConnectionStringSettingsRequest
    {
        public InstanceKey InstanceKey { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public bool UserWantToRestart { get; set; }
        
    }
}
