using System.Text;

using PT.Database;
using PT.Scheduler;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData.InventoryManagement;

public static class SalesOrderData
{
    /// <summary>
    /// Returns a comma separated string of lot codes
    /// </summary>
    public static string GetAllowedLotCodes(this SalesOrderLineDistribution a_dist)
    {
        StringBuilder codeString = new();
        foreach (string lotCode in a_dist.EligibleLotCodesEnumerator)
        {
            codeString.Append(lotCode);
            codeString.Append(',');
        }

        return codeString.ToString().TrimEnd(',');
    }

    public static void PtDbPopulate(this SalesOrderManager a_salesOrderManager, ref PtDbDataSet a_dataSet, PtDbDataSet.SchedulesRow a_parentRow, PTDatabaseHelper a_dbHelper, ScenarioDetail a_sd)
    {
        for (int i = 0; i < a_salesOrderManager.Count; i++)
        {
            a_salesOrderManager[i].PtDbPopulate(ref a_dataSet, a_parentRow, a_dbHelper, a_sd);
        }
    }

    public static void PtDbPopulate(this SalesOrder a_so, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow parentRow, PTDatabaseHelper a_dbHelper, ScenarioDetail a_sd)
    {
        PtDbDataSet.SalesOrdersRow soRow = dataSet.SalesOrders.AddSalesOrdersRow(
            parentRow,
            parentRow.InstanceId,
            a_so.Id.ToBaseType(),
            a_so.ExternalId,
            a_so.Name,
            a_so.Description,
            a_so.Notes,
            a_so.Cancelled,
            a_so.CtpJob == null ? -1 : a_so.CtpJob.Id.ToBaseType(),
            a_so.Customer?.ExternalId,
            a_so.Estimate,
            a_dbHelper.AdjustPublishTime(a_so.ExpirationDate),
            a_so.SalesAmount,
            a_so.SalesOffice,
            a_so.SalesPerson,
            a_so.Planner,
            a_so.Project);

        a_so.PtDbPopulateUserFields(soRow, a_sd, a_dbHelper.UserFieldDefinitions);

        for (int lineI = 0; lineI < a_so.SalesOrderLines.Count; lineI++)
        {
            a_so.SalesOrderLines[lineI].PtDbPopulate(ref dataSet, soRow, a_dbHelper);
        }
    }

    public static void PtDbPopulate(this SalesOrderLine a_line, ref PtDbDataSet dataSet, PtDbDataSet.SalesOrdersRow parentRow, PTDatabaseHelper a_dbHelper)
    {
        PtDbDataSet.SalesOrderLinesRow soLineRow = dataSet.SalesOrderLines.AddSalesOrderLinesRow(
            parentRow.PublishDate,
            parentRow.InstanceId,
            parentRow.SalesOrderId,
            a_line.Id.ToBaseType(),
            a_line.Description,
            a_line.Item.Id.ToBaseType(),
            a_line.LineNumber,
            a_line.UnitPrice);

        for (int distI = 0; distI < a_line.LineDistributions.Count; distI++)
        {
            SalesOrderLineDistribution dist = a_line.LineDistributions[distI];
            if (dist.RequiredAvailableDate.Ticks <= a_dbHelper.MaxPublishDate.Ticks)
            {
                dist.PtDbPopulate(ref dataSet, soLineRow, a_dbHelper);
            }
        }
    }

    public static void PtDbPopulate(this SalesOrderLineDistribution a_dist, ref PtDbDataSet dataSet, PtDbDataSet.SalesOrderLinesRow parentRow, PTDatabaseHelper a_dbHelper)
    {
        dataSet.SalesOrderLineDistributions.AddSalesOrderLineDistributionsRow(
            parentRow.PublishDate,
            parentRow.InstanceId,
            parentRow.SalesOrderLineId,
            a_dist.Id.ToBaseType(),
            a_dist.AllowPartialAllocations,
            a_dist.BacklogQty,
            a_dist.Hold,
            a_dist.HoldReason,
            a_dist.MaximumLateness.TotalDays,
            a_dist.MaterialAllocation.ToString(),
            a_dist.MinAllocationQty,
            a_dist.MinSourceQty,
            a_dist.MaxSourceQty,
            a_dist.MaterialSourcing.ToString(),
            a_dist.MissedSaleQty,
            a_dist.MustSupplyFromWarehouse == null ? -1 : a_dist.MustSupplyFromWarehouse.Id.ToBaseType(),
            a_dist.Priority,
            a_dist.QtyAllocatedFromOnHandInventory,
            a_dist.QtyAllocatedFromProjectedInventory,
            a_dist.QtyNotAllocated,
            a_dist.QtyShipped,
            a_dist.QtyOrdered,
            a_dbHelper.AdjustPublishTime(a_dist.RequiredAvailableDate),
            a_dist.SalesRegion,
            a_dist.ShipToZone,
            a_dist.StockShortageRule.ToString(),
            a_dist.SupplyingWarehouse?.Id.ToBaseType() ?? -1,
            a_dbHelper.AdjustPublishTime(a_dist.ActualAvailableTicks == 0 ? PTDateTime.MinDateTime : new DateTime(a_dist.ActualAvailableTicks)),
            a_dist.GetAllowedLotCodes());
    }

    public static void PopulateSalesOrderTDataSet(this SalesOrder a_so, ERPTransmissions.SalesOrderTDataSet a_dataset)
    {
        ERPTransmissions.SalesOrderTDataSet.SalesOrderRow soRow = a_dataset.SalesOrder.AddSalesOrderRow(
            a_so.ExternalId,
            a_so.Name,
            a_so.Description,
            a_so.Notes,
            a_so.Customer?.ExternalId,
            a_so.Estimate,
            a_so.SalesAmount,
            a_so.SalesOffice,
            a_so.SalesPerson,
            a_so.Planner,
            a_so.Project,
            a_so.Cancelled,
            a_so.CancelAtExpirationDate,
            a_so.ExpirationDate.ToDisplayTime().ToDateTime(),
            a_so.UserFields == null ? "" : a_so.UserFields.GetUserFieldImportString());

        for (int solI = 0; solI < a_so.SalesOrderLines.Count; solI++)
        {
            SalesOrderLine sol = a_so.SalesOrderLines[solI];
            ERPTransmissions.SalesOrderTDataSet.SalesOrderLineRow solRow = a_dataset.SalesOrderLine.AddSalesOrderLineRow(
                soRow,
                sol.LineNumber,
                sol.Description,
                sol.UnitPrice,
                sol.Item.ExternalId);

            for (int distI = 0; distI < sol.LineDistributions.Count; distI++)
            {
                SalesOrderLineDistribution dist = sol.LineDistributions[distI];
                string mustSupplyFromWarehouseExternalId;
                if (dist.MustSupplyFromWarehouse != null)
                {
                    mustSupplyFromWarehouseExternalId = dist.MustSupplyFromWarehouse.ExternalId;
                }
                else
                {
                    mustSupplyFromWarehouseExternalId = "";
                }

                a_dataset.SalesOrderLineDist.AddSalesOrderLineDistRow(
                    solRow.SalesOrderExternalId,
                    solRow.LineNumber,
                    mustSupplyFromWarehouseExternalId,
                    dist.UseMustSupplyFromWarehouse,
                    dist.QtyOrdered,
                    dist.QtyShipped,
                    dist.MinSourceQty,
                    dist.MaxSourceQty,
                    dist.MaterialSourcing.ToString(),
                    dist.RequiredAvailableDate.ToDisplayTime().ToDateTime(),
                    dist.ShipToZone,
                    dist.SalesRegion,
                    dist.Closed,
                    dist.Hold,
                    dist.HoldReason,
                    dist.Priority,
                    dist.MaximumLateness.TotalDays,
                    dist.AllowPartialAllocations,
                    dist.MaterialAllocation.ToString(),
                    dist.MinAllocationQty,
                    dist.StockShortageRule.ToString(),
                    dist.GetAllowedLotCodes());
            }
        }
    }
}