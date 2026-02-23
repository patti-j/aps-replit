using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetsuiteIntegration.ResponseObjects
{
    public class StockOnHands
    {        
        public StockOnHand[] stockOnHand { get; set; }
    }

    public class StockOnHand
    {
        [JsonProperty("Lot Internal ID")]
        public int? LotInternalId { get; set; }

        [JsonProperty("Item Internal ID")]
        public int? ItemInternalId { get; set; }

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

        [JsonProperty("Lot Number")]
        public string LotNumber { get; set; }

        [JsonProperty("Width")]
        public decimal? Width { get; set; }

        [JsonProperty("Length")]
        public decimal? Length { get; set; }

        [JsonProperty("adhesive")]
        public string Adhesive { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("coldFoil")]
        public string ColdFoil { get; set; }

        [JsonProperty("colorCode")]
        public string ColorCode { get; set; }        

        [JsonProperty("hotstamp")]
        public string Hotstamp { get; set; }

        [JsonProperty("hotstamp2")]
        public string Hotstamp2 { get; set; }

        [JsonProperty("lam1")]
        public string Lam1 { get; set; }

        [JsonProperty("lam2")]
        public string Lam2 { get; set; }

        [JsonProperty("liner")]
        public string Liner { get; set; }

        [JsonProperty("msiPerLb")]
        public decimal? MsiPerLb { get; set; }

        [JsonProperty("pricePerLb/click")]
        public decimal? PricePerLbOrClick { get; set; }

        [JsonProperty("shrink")]
        public string Shrink { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("stock")]
        public string Stock { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
