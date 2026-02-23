using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetsuiteIntegration.ResponseObjects
{
    public class SalesOrders
    {        
        public SalesOrder[] salesOrders { get; set; }
    }

    public class SalesOrder
    {
        [JsonProperty("Order No")]
        public string OrderNo { get; set; }

        [JsonProperty("Order Type")]
        public string OrderType { get; set; }

        [JsonProperty("Order Line No")]
        public int? OrderLineNo { get; set; }

        [JsonProperty("Part No")]
        public string PartNo { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Demand Date")]
        public DateTime? DemandDate { get; set; }

        [JsonProperty("Quantity")]
        public decimal? Quantity { get; set; }

        [JsonProperty("Customer")]
        public string Customer { get; set; }

        [JsonProperty("Quantity FulfilledReceived")]
        public decimal? QuantityFulfilledReceived { get; set; }

        [JsonProperty("Amount")]
        public decimal? Amount { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
