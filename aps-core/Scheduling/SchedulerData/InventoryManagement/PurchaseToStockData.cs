using PT.APSCommon;
using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class PurchaseToStockData
{
    public static void PtDbPopulate(this PurchaseToStock a_purchaseToStock, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow schedulesRow, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        //Add PurchaseToStock row
        //Note below that we add 1 to ScheduledDockNbr since it is base zero internally but displayed as base 1.
        PtDbDataSet.PurchasesToStockRow purchaseRow = dataSet.PurchasesToStock.AddPurchasesToStockRow(
            schedulesRow,
            schedulesRow.InstanceId,
            a_purchaseToStock.Id.ToBaseType(),
            a_purchaseToStock.Inventory.Id.ToBaseType(),
            a_purchaseToStock.Name,
            a_purchaseToStock.Description,
            a_purchaseToStock.Inventory.Item.Id.ToBaseType(),
            a_purchaseToStock.QtyOrdered,
            a_purchaseToStock.ExternalId,
            a_dbHelper.AdjustPublishTime(a_purchaseToStock.ScheduledReceiptDate),
            a_purchaseToStock.UnloadSpan.TotalHours,
            a_purchaseToStock.TransferSpan.TotalHours,
            a_dbHelper.AdjustPublishTime(a_purchaseToStock.AvailableDate),
            a_purchaseToStock.VendorExternalId,
            a_purchaseToStock.BuyerExternalId,
            a_purchaseToStock.Warehouse.Id.ToBaseType(),
            a_dbHelper.AdjustPublishTime(a_purchaseToStock.UnloadEndDate),
            a_purchaseToStock.Notes,
            a_purchaseToStock.Firm,
            a_purchaseToStock.Closed,
            a_purchaseToStock.ReceivingBuffer.TotalHours,
            a_dbHelper.AdjustPublishTime(a_purchaseToStock.ReceiptDate),
            Convert.ToDouble(a_purchaseToStock.GetCurrentBufferPenetrationPercent(a_sd)),
            a_purchaseToStock.QtyReceived,
            a_purchaseToStock.MaintenanceMethod.ToString(),
            a_dbHelper.AdjustPublishTime(a_purchaseToStock.ScheduledReceiptDate),
            a_purchaseToStock.OverrideStorageConstraint,
            a_purchaseToStock.RequireEmptyStorageArea,
            a_purchaseToStock.StorageArea?.Id.ToBaseType() ?? BaseId.NULL_ID.ToBaseType()
        );

        a_purchaseToStock.PtDbPopulateUserFields(purchaseRow, a_sd, a_dbHelper.UserFieldDefinitions);

        if (a_purchaseToStock.Demands != null)
        {
            a_purchaseToStock.Demands.PtDbPopulate(ref dataSet, purchaseRow, a_dbHelper);
        }
    }

    #region PT Database
    public static void PtDbPopulate(this PurchaseToStockManager a_purchaseToStock, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow schedulesRow, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_purchaseToStock.Count; i++)
        {
            PurchaseToStock po = a_purchaseToStock.GetByIndex(i);
            if (po.ScheduledReceiptDate.Ticks <= a_dbHelper.MaxPublishDate.Ticks)
            {
                po.PtDbPopulate(ref dataSet, schedulesRow, a_sd, a_dbHelper);
            }
        }
    }
    #endregion
}