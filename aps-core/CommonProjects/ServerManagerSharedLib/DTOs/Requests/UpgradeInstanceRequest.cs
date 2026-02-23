using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class UpgradeInstanceRequest
    {
        public string NewVersion  { get; set; }
        public string SerialCode  { get; set; }
        public InstanceKey OriginalInstance { get; set; }
        public bool StartWhenUpgraded { get; set; }
    }
}