using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace NetsuiteIntegration.ResponseObjects
{
    public class WorkOrders
    {
        public WorkOrder[] workOrders { get; set; }        
    }

    public class WorkOrder
    {
        
        [JsonProperty("Work Order Internal ID")]
        public long? WorkOrderInternalId { get; set; }

        [JsonProperty("Manufacturing Routing Internal ID")]
        public long? ManufacturingRoutingInternalId { get; set; }

        [JsonProperty("Operation Internal ID")]
        public long? OperationInternalId { get; set; }

        [JsonProperty("BOM Revision Id")]
        public long? BOMRevisionId { get; set; }

        [JsonProperty("BOM Id")]
        public long? BOMId { get; set; }
        
        [JsonProperty("Order Status")]
        public string OrderStatus { get; set; }

        [JsonProperty("Order No")]
        public string OrderNo { get; set; }

        [JsonProperty("Part No")]
        public string PartNo { get; set; }

        [JsonProperty("Manufacturing Routing")]
        public string ManufacturingRouting { get; set; }

        [JsonProperty("Operation Name")]
        public string OperationName { get; set; }

        [JsonProperty("Created From")]
        public string CreatedFrom { get; set; }

        [JsonProperty("Quoted Ship Date")]
        public string QuotedShipDateRaw { get; set; }

        [JsonProperty("Expedite Request")]
        public string ExpediteRequest { get; set; }

        [JsonProperty("CPQ Order Type")]
        public string CPQOrderType { get; set; }

        [JsonProperty("BOM")]
        public string BOM { get; set; }

        [JsonProperty("BOM Name")]
        public string BOMName { get; set; }
        
        [JsonProperty("Quantity")]
        public decimal? Quantity { get; set; }

        [JsonProperty("Quantity Built")]
        public decimal? QuantityBuilt { get; set; }

        [JsonProperty("Qty Remaining")]
        public decimal? QtyRemaining { get; set; }

        [JsonProperty("Setup Time (Min)")]
        public decimal? SetupTimeMin { get; set; }

        [JsonProperty("Run Time")]
        public decimal? RunTime { get; set; }

        [JsonProperty("Completed Quantity")]
        public decimal? CompletedQuantity { get; set; }

        [JsonProperty("BOM Width")]
        public decimal? BOMWidth { get; set; }

        [JsonProperty("Op No")]
        public int? OpNo { get; set; }
        
        [JsonProperty("Due Date")]
        public DateTime? DueDate { get; set; }

        [JsonProperty("Expedite Request Date")]
        public DateTime? ExpediteRequestDate { get; set; }

        [JsonProperty("Start Date")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("End Date")]
        public DateTime? EndDate { get; set; }
        
        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
