using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetsuiteIntegration.ResponseObjects
{
    public class RawMaterials
    {        
        public RawMaterial[] rawMaterials { get; set; }
    }

    public class RawMaterial
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Display Name")]
        public string DisplayName { get; set; }

        [JsonProperty("Purchase Price")]
        public decimal? PurchasePrice { get; set; }

        [JsonProperty("Last Purchase Price")]
        public decimal? LastPurchasePrice { get; set; }

        [JsonProperty("Classification")]
        public string Classification { get; set; }

        [JsonProperty("Cost_per_MSI/Lb")]
        public decimal? CostPerMsiPerLb { get; set; }        

        [JsonProperty("adhesive")]
        public string Adhesive { get; set; }

        [JsonProperty("liner")]
        public string Liner { get; set; }

        [JsonProperty("msiPerLb")]
        public decimal? MsiPerLb { get; set; }

        [JsonProperty("pricePerLb/click")]
        public decimal? PricePerLbOrClick { get; set; }

        [JsonProperty("Ink Type (Product)")]
        public string InkTypeProduct { get; set; }

        [JsonProperty("Machine")]
        public string Machine { get; set; }

        [JsonProperty("Internal ID")]
        public int? InternalId { get; set; }

        [JsonProperty("colorCode")]
        public string ColorCode { get; set; }

        [JsonProperty("Ink/Varnish Appearance")]
        public string InkVarnishAppearance { get; set; }

        [JsonProperty("Ink Type")]
        public string InkType { get; set; }

        [JsonProperty("Inactive")]
        public string Inactive { get; set; }

        [JsonProperty("Display in Webcenter")]
        public string DisplayInWebcenter { get; set; }

        [JsonProperty("Is Available?")]
        public string IsAvailable { get; set; }

        [JsonProperty("stock")]
        public string Stock { get; set; }

        [JsonProperty("Available")]
        public decimal? Available { get; set; }

        [JsonProperty("On Hand")]
        public decimal? OnHand { get; set; }

        [JsonProperty("On Order")]
        public decimal? OnOrder { get; set; }

        [JsonProperty("Committed")]
        public decimal? Committed { get; set; }

        [JsonProperty("Stock Type")]
        public string StockType { get; set; }

        [JsonProperty("Total Caliper (in)")]
        public string? TotalCaliperIn { get; set; }

        [JsonProperty("Stock Color")]
        public string StockColor { get; set; }

        [JsonProperty("Liner Caliper (in)")]
        public decimal? LinerCaliperIn { get; set; }

        [JsonProperty("Ink Cure Type")]
        public string InkCureType { get; set; }

        [JsonProperty("Recommended Anilox Line Screen (LPI)")]
        public decimal? RecommendedAniloxLineScreenLpi { get; set; }

        [JsonProperty("Recommended Anilox BCM")]
        public decimal? RecommendedAniloxBcm { get; set; }

        [JsonProperty("Facestock Caliper (in)")]
        public decimal? FacestockCaliperIn { get; set; }

        [JsonProperty("External ID")]
        public string ExternalId { get; set; }

        [JsonProperty("Thickness (microns)")]
        public decimal? ThicknessMicrons { get; set; }

        [JsonProperty("Shrink Force")]
        public string ShrinkForce { get; set; }

        [JsonProperty("Face Stock")]
        public string FaceStock { get; set; }

        [JsonProperty("Stock Appearance")]
        public string StockAppearance { get; set; }

        [JsonProperty("RUNOUT")]
        public string Runout { get; set; }

        [JsonProperty("Primary Item Substitute")]
        public string PrimaryItemSubstitute { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
