using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetsuiteIntegration.ResponseObjects
{
    public class MaterialConsumptions
    {        
        public MaterialConsumption[] materialConsumptions { get; set; }
    }

    public class MaterialConsumption
    {
        [JsonProperty("Internal ID")]
        public int? InternalId { get; set; }

        [JsonProperty("Document Number")]
        public string DocumentNumber { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Work Order")]
        public string WorkOrder { get; set; }

        [JsonProperty("Work Order ID")]
        public int? WorkOrderId { get; set; }

        [JsonProperty("Used in Build")]
        public decimal? UsedInBuild { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
