namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class InstanceRequest
    {
        public string InstanceName { get; set; }
        public string SoftwareVersion{ get; set; }

        public InstanceRequest()
        {
        }
        public InstanceRequest(string a_instance, string a_softwareVersion)
        {
            InstanceName = a_instance;
            SoftwareVersion = a_softwareVersion;
        }
    }
}