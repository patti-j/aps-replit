using System;
using System.IO;
using Newtonsoft.Json;

namespace PT.ServerManagerSharedLib.Definitions
{
    public class LicenseKeyJson
    {
        public string SerialCode { get; set; }
        public bool Expirable { get; set; }
        public DateTime ExpirationDate { get; set; }
        public double LicenseGraceDays { get; set; }
        public DateTime LastKeyFieldModificationUTC { get; set; }
        public string SystemIdType { get; set; }

        public static LicenseKeyJson DeserializeToObject(string a_jsonFilePath)
        {
            return JsonConvert.DeserializeObject<LicenseKeyJson>(File.ReadAllText(a_jsonFilePath));
        }
    }
}
