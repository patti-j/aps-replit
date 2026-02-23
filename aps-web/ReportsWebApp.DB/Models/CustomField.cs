using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class CustomField : BaseEntity
    {
        [ForeignKey("Company")] public int CompanyId { get; set; }
        [Required] 
        public virtual Company Company { get; set; }
        [Required]
        public string ExternalId { get; set; }
        public CustomFieldType Type { get; set; }
        public CustomFieldObject Object { get; set; }
        public bool ShowInGantt { get; set; }
        public bool ShowInGrids { get; set; }
        public bool CanPublish { get; set; }
        public virtual List<CFGroup> Groups { get; set; }

        public override bool Equals(object obj)
        {
            var cf = obj as CustomField;
            if (cf != null)
            {
                return cf.Id == Id;
            }
            return base.Equals(obj);
        }
    }

    public enum CustomFieldType
    {
        String,
        Int,
        Long,
        Double,
        Decimal,
        Datetime,
        Bool,
        Timespan
    }

    public enum CustomFieldObject
    {
        Capabilities,
        CapacityIntervals,
        Cells,
        Customers,
        Departments,
        Forecasts,
        Items,
        Jobs,
        JobOperations,
        ManufacturingOrders,
        Plants,
        ProductRules,
        PurchaseToStock,
        ResourceConnectors,
        Resources,
        SalesOrders,
        TransferOrders,
        Users,
        Warehouses
    }
}
