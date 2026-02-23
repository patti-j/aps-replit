using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetsuiteIntegration.ResponseObjects
{
    public class BillOfMaterialsRevisions
    {
        [JsonProperty("billOfMaterialsRevisions")]
        public BillOfMaterialsRevision[] billOfMaterialsRevisions { get; set; }
    }

    public class BillOfMaterialsRevision
    {
        [JsonProperty("Bill of Materials")]
        public string BillOfMaterials { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Internal ID")]
        public int? InternalId { get; set; }

        [JsonProperty("Width")]
        public decimal? Width { get; set; }

        [JsonProperty("Units")]
        public string Units { get; set; }

        [JsonProperty("Revision Name")]
        public string RevisionName { get; set; }

        [JsonProperty("Component Yield")]
        public decimal? ComponentYield { get; set; }

        [JsonProperty("BoM Quantity")]
        public decimal? BoMQuantity { get; set; }

        [JsonProperty("BOM Id")]
        public int? BomId { get; set; }

        [JsonProperty("Item")]
        public string Item { get; set; }

        [JsonProperty("Component Internal ID")]
        public int? ComponentInternalId { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
