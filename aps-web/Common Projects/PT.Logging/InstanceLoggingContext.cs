using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace PT.Logging
{
    public class InstanceLoggingContext
    {
        public string InstanceName { get; set; }
        public string SoftwareVersion { get; set; }
        public string InstanceAuditLogConnectionString { get; set; }

        public InstanceLoggingContext() { }

        public InstanceLoggingContext(string a_paSettings)
        {
            var obj = ((JObject)JsonConvert.DeserializeObject<JObject>(a_paSettings));
            InstanceName = obj["PublicInfo"]["InstanceName"].ToString();
            SoftwareVersion = obj["PublicInfo"]["SoftwareVersion"].ToString();
            InstanceAuditLogConnectionString = obj["Settings"]["SystemServiceSettings"]["LogDbConnectionString"].ToString();
        }
    }
}
