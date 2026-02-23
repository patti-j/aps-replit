namespace WebAPI.RequestsAndResponses
{
    public class WebUpgradeInstanceResponseParams
    {
        public string InstanceName { get; set; }
        public string OldVersion { get; set; }
        public string NewVersion { get; set; }
        public string UpdatedSettings { get; set; }
    }
}
