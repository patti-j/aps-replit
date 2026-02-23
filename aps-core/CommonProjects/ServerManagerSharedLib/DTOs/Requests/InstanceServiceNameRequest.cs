namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class InstanceServiceNameRequest
    {
        public string ServiceName { get; set; }

        public InstanceServiceNameRequest(string a_serviceName)
        {
            ServiceName = a_serviceName;
        }
    }
}
