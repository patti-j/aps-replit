using System.Runtime.Serialization;

namespace PT.ServerManagerSharedLib.Definitions
{
    /// <summary>
    /// Information required to access an instance's config data db.
    /// </summary>
    public class InstanceConnectionData
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public string ConnectionString { get; set; }

        public string InstanceId => $"{Name} {Version}";

    }
}
