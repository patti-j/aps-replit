namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class InstanceKey
    {
        public string InstanceName { get; set; }
        public string SoftwareVersion { get; set; }

        public InstanceKey() { }

        public InstanceKey(string a_instanceName, string a_softwareVersion)
        {
            InstanceName = a_instanceName;
            SoftwareVersion = a_softwareVersion;
        }

        public override string ToString()
        {
            return $"{InstanceName} {SoftwareVersion}";
        }
    }
}
