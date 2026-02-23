using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace NetsuiteIntegration.ResponseObjects
{
    public class BillOfMaterials
    {
        public BillOfMaterial[] boms { get; set; }
    }

    public class BillOfMaterial
    {
        [JsonProperty("Order No")]
        public string OrderNo { get; set; }

        [JsonProperty("Order Part No")]
        public string OrderPartNo { get; set; }

        [JsonProperty("Required Part No")]
        public string RequiredPartNo { get; set; }

        [JsonProperty("Required Quantity")]
        public decimal? RequiredQuantity { get; set; }
        
        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }

}
