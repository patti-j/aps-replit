using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetsuiteIntegration.ResponseObjects
{
    public class Mfgroutings
    {        
        public Mfgrouting[] mfgroutings { get; set; }
    }

    public class Mfgrouting
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Bill of Materials")]
        public string BillOfMaterials { get; set; }

        [JsonProperty("Operation Name")]
        public string OperationName { get; set; }

        [JsonProperty("Operation Sequence")]
        public int? OperationSequence { get; set; }

        [JsonProperty("Setup Time")]
        public decimal? SetupTime { get; set; }

        [JsonProperty("Run Rate")]
        public decimal? RunRate { get; set; }

        [JsonProperty("Internal ID")]
        public int? InternalId { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
