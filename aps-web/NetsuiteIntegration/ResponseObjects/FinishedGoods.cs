using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetsuiteIntegration.ResponseObjects
{
    public class FinishedGoods
    {        
        public FinishedGood[] finishedGoods { get; set; }
    }

    public class FinishedGood
    {
        [JsonProperty("Internal ID")]
        public int? InternalId { get; set; }

        [JsonProperty("Part No")]
        public string PartNo { get; set; }

        [JsonProperty("ColorGamut")]
        public string ColorGamut { get; set; }

        [JsonProperty("Material")]
        public string Material { get; set; }

        [JsonProperty("LayFlat")]
        public string LayFlat { get; set; }

        [JsonProperty("SlitWidth")]
        public decimal? SlitWidth { get; set; }

        [JsonProperty("SizeAcross")]
        public decimal? SizeAcross { get; set; }

        [JsonProperty("CoreSize")]
        public decimal? CoreSize { get; set; }

        [JsonProperty("CuttingDie")]
        public string CuttingDie { get; set; }

        [JsonProperty("EmbossDie")]
        public string EmbossDie { get; set; }

        [JsonProperty("Screen")]
        public string Screen { get; set; }

        [JsonProperty("Lamination Material")]
        public string LaminationMaterial { get; set; }

        [JsonProperty("Flexo Ink 1")]
        public string FlexoInk1 { get; set; }

        [JsonProperty("Flexo Ink 2")]
        public string FlexoInk2 { get; set; }

        [JsonProperty("ScreenInk")]
        public string ScreenInk { get; set; }

        [JsonProperty("UUID")]
        public string UUID { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
