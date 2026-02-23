using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class JobTemplate : BaseEntity
    {
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public int TemplateId { get; set; } // Column: TemplateId
        public string TemplateName { get; set; } = string.Empty; // Column: Template Name
        public string ItemExternalId { get; set; } = string.Empty; // Column: Item External ID
        public string WarehouseExternalId { get; set; } = string.Empty; // Column: Warehouse External ID
        public decimal RequiredQty { get; set; } // Column: Required Quantity
        public DateTime NeedDate { get; set; } // Column: Need Date
        public string RequiredPathId { get; set; } = string.Empty; // Column: Required Path ID
        public int Priority { get; set; } // Column: Priority
        public decimal Revenue { get; set; } // Column: Revenue
        public decimal Throughput { get; set; } // Column: Throughput
        public bool IsHot { get; set; } // Column: Is Hot
        public string HotReason { get; set; } = string.Empty; // Column: Hot Reason
        public string Warehouses { get; set; } = string.Empty; // Column: Warehouses
        public string BottleneckConstraints { get; set; } = string.Empty; // Column: Bottleneck Constraints
        public string ItemsWithStockMaterialConstraints { get; set; } = string.Empty; // Column: Items with Stock Material Constraints
        public bool CopyRoutingFromTemplate { get; set; } // Column: CopyRoutingFromTemplate
        public double DBRShippingBufferOverrideDays { get; set; } // Column: DBRShippingBufferOverrideDays
        public string DefaultPathExternalId { get; set; } = string.Empty; // Column: DefaultPathExternalId
    }
}
