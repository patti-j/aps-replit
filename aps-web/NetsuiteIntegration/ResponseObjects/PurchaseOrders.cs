using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetsuiteIntegration.ResponseObjects
{
    public class PurchaseOrders
    {        
        public PurchaseOrder[] purchaseOrders { get; set; }
    }

    public class PurchaseOrder
    {
        [JsonProperty("Order No")]
        public string OrderNo { get; set; }

        [JsonProperty("Order Type")]
        public string OrderType { get; set; }

        [JsonProperty("Part No")]
        public string PartNo { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Supply Date")]
        public DateTime? SupplyDate { get; set; }

        [JsonProperty("Quantity")]
        public decimal? Quantity { get; set; }

        [JsonProperty("Width")]
        public decimal? Width { get; set; }

        [JsonProperty("Length")]
        public decimal? Length { get; set; }

        [JsonProperty("PO Line Item ID")]
        public int? POLineItemId { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
