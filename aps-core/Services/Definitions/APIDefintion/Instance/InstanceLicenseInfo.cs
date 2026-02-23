using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace PT.APIDefinitions.Instance;

[DataContract(Name = "InstanceLicenseInfo", Namespace = "http://www.planettogether.com")]
public class InstanceLicenseInfo
{
    public InstanceLicenseInfo()
    {
    }

    public InstanceLicenseInfo(string a_serialCode) : this()
    {
        SerialCode = a_serialCode;
    }

    [DataMember]
    [JsonIgnore]
    public long SessionId { get; set; }

    [DataMember]
    public string SerialCode { get; set; }

    [DataMember]
    [JsonIgnore]
    public bool ReadOnly { get; set; }

    public void Update(string a_newSerialCode)
    {
        SerialCode = a_newSerialCode;
    }
}