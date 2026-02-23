using PT.ERPTransmissions;
using PT.Scheduler.Demand;

namespace PT.Scheduler.Schedule.Demand.SalesOrder;

public class SalesOrderDataSetFiller
{
    public void FillDataSet(SalesOrderTDataSet a_ds, Scheduler.Demand.SalesOrder a_so)
    {
        SalesOrderTDataSet.SalesOrderRow salesOrderRow = a_ds.SalesOrder.NewSalesOrderRow();

        salesOrderRow.Name = a_so.Name;
        salesOrderRow.ExternalId = a_so.ExternalId;
        salesOrderRow.CancelAtExpirationDate = a_so.CancelAtExpirationDate;
        salesOrderRow.Cancelled = a_so.Cancelled;
        salesOrderRow.CustomerExternalId = a_so.Customer?.ExternalId ?? "";
        salesOrderRow.Description = a_so.Description;
        salesOrderRow.Estimate = a_so.Estimate;
        salesOrderRow.ExpirationDate = a_so.ExpirationDate;
        salesOrderRow.Notes = a_so.Notes;
        salesOrderRow.Planner = a_so.Planner;
        salesOrderRow.Project = a_so.Project;
        salesOrderRow.SalesAmount = a_so.SalesAmount;
        salesOrderRow.SalesOffice = a_so.SalesOffice;
        salesOrderRow.SalesPerson = a_so.SalesPerson;
        salesOrderRow.UserFields = a_so.UserFields?.GetUserFieldImportString() ?? "";

        a_ds.SalesOrder.AddSalesOrderRow(salesOrderRow);

        foreach (SalesOrderLine soLine in a_so.SalesOrderLines)
        {
            SalesOrderTDataSet.SalesOrderLineRow soLineRow = a_ds.SalesOrderLine.NewSalesOrderLineRow();
            soLineRow.SalesOrderExternalId = soLine.SalesOrder?.ExternalId ?? "";
            soLineRow.ItemExternalId = soLine.Item?.ExternalId ?? "";
            soLineRow.Description = soLine.Description;
            soLineRow.UnitPrice = soLine.UnitPrice;
            soLineRow.LineNumber = soLine.LineNumber;

            a_ds.SalesOrderLine.AddSalesOrderLineRow(soLineRow);

            foreach (SalesOrderLineDistribution distribution in soLine.LineDistributions)
            {
                SalesOrderTDataSet.SalesOrderLineDistRow distributionRow = a_ds.SalesOrderLineDist.NewSalesOrderLineDistRow();
                distributionRow.LineNumber = distribution.SalesOrderLine?.LineNumber ?? "";
                distributionRow.AllowPartialAllocations = distribution.AllowPartialAllocations;
                distributionRow.AllowedLotCodes = distribution.EligibleLots.ToString();
                distributionRow.Closed = distribution.Closed;
                distributionRow.Hold = distribution.Hold;
                distributionRow.HoldReason = distribution.HoldReason;
                distributionRow.MaterialAllocation = distribution.MaterialAllocation.ToString();
                distributionRow.MaterialSourcing = distribution.MaterialSourcing.ToString();
                distributionRow.MaximumLatenessDays = distribution.MaximumLateness.TotalDays;
                distributionRow.MinAllocationQty = distribution.MinAllocationQty;
                distributionRow.MaxSourceQty = distribution.MaxSourceQty;
                distributionRow.MinSourceQty = distribution.MinSourceQty;
                distributionRow.MustSupplyFromWarehouseExternalId = distribution.MustSupplyFromWarehouse?.ExternalId ?? "";
                distributionRow.UseMustSupplyFromWarehouseExternalId = distribution.UseMustSupplyFromWarehouse;
                distributionRow.Priority = distribution.Priority;
                distributionRow.QtyOrdered = distribution.QtyOrdered;
                distributionRow.QtyShipped = distribution.QtyShipped;
                distributionRow.SalesRegion = distribution.SalesRegion;
                distributionRow.ShipToZone = distribution.ShipToZone;
                distributionRow.StockShortageRule = distribution.StockShortageRule.ToString();
                distributionRow.SalesOrderExternalId = distribution.SalesOrderLine?.SalesOrder?.ExternalId ?? "";
                distributionRow.RequiredAvailableDate = distribution.RequiredAvailableDate;

                a_ds.SalesOrderLineDist.AddSalesOrderLineDistRow(distributionRow);
            }
        }
    }
}