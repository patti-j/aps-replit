namespace PT.Common.Http;

//TODO synchronize with SM's equivalent class
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

    /// <summary>
    /// Constructor expecting the standard service name format "PlanetTogether {InstanceName} {SoftwareVersion} System".
    /// This service name is typically passed in as a startup argument for the instance and client.
    /// </summary>
    /// <param name="a_ptFormattedServiceName"></param>
    public InstanceKey(string a_ptFormattedServiceName)
    {
        string[] s = a_ptFormattedServiceName.Split(' ');

        //Get range of strings making up the instance name
        string[] instanceStrings = s[1..^2];
        string instanceName = string.Join(' ', instanceStrings);
        string softwareVersion = s[^2];

        InstanceName = instanceName;
        SoftwareVersion = softwareVersion;
    }

    public string GetServiceName()
    {
        return $"PlanetTogether {InstanceName} {SoftwareVersion} System";
    }
}