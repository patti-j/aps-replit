using PT.Database;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData.InventoryManagement;

public static class TransferOrderData
{
    public static void PtDbPopulate(this TransferOrderManager a_manager, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow parentRow, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_manager.Count; i++)
        {
            a_manager.GetByIndex(i).PtDbPopulate(ref dataSet, parentRow, a_dbHelper);
        }
    }

    public static void PtDbPopulate(this TransferOrder a_to, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow parentRow, PTDatabaseHelper a_dbHelper)
    {
        bool shouldPublish = false;
        for (int i = 0; i < a_to.Distributions.Count; i++)
        {
            TransferOrderDistribution dist = a_to.Distributions.GetByIndex(i);
            if (dist.ScheduledShipDateTicks <= a_dbHelper.MaxPublishDate.Ticks || dist.ScheduledReceiveDateTicks <= a_dbHelper.MaxPublishDate.Ticks)
            {
                shouldPublish = true;
                break;
            }
        }

        if (shouldPublish)
        {
            PtDbDataSet.TransferOrdersRow transferOrderRow = dataSet.TransferOrders.AddTransferOrdersRow(
                parentRow,
                parentRow.InstanceId,
                a_to.Id.ToBaseType(),
                a_to.Name,
                a_to.Description,
                a_to.Notes,
                a_to.ExternalId,
                a_to.Firm,
                a_to.Priority,
                a_to.Closed,
                a_to.MaintenanceMethod.ToString()
            );

            for (int i = 0; i < a_to.Distributions.Count; i++)
            {
                TransferOrderDistribution dist = a_to.Distributions.GetByIndex(i);
                dist.PtDbPopulate(ref dataSet, transferOrderRow, a_dbHelper);
            }
        }
    }

    public static void PtDbPopulate(this TransferOrderDistribution a_distribution, ref PtDbDataSet dataSet, PtDbDataSet.TransferOrdersRow parentRow, PTDatabaseHelper a_dbHelper)
    {
        if (a_distribution.ScheduledReceiveDateTicks <= a_dbHelper.MaxPublishDate.Ticks || a_distribution.ScheduledShipDateTicks <= a_dbHelper.MaxPublishDate.Ticks)
        {
            dataSet.TransferOrderDistributions.AddTransferOrderDistributionsRow(
                parentRow.PublishDate,
                parentRow.InstanceId,
                parentRow.TransferOrderId,
                a_distribution.Id.ToBaseType(),
                a_distribution.Item.Id.ToBaseType(),
                a_distribution.FromWarehouse.Id.ToBaseType(),
                a_distribution.ToWarehouse.Id.ToBaseType(),
                a_distribution.FromStorageArea?.ExternalId,
                a_distribution.ToStorageArea?.ExternalId,
                a_distribution.QtyOrdered,
                a_distribution.QtyShipped,
                a_distribution.QtyReceived,
                a_distribution.MinSourceQty,
                a_distribution.MaxSourceQty,
                a_distribution.MaterialSourcing.ToString(),
                a_distribution.MaterialAllocation.ToString(),
                a_dbHelper.AdjustPublishTime(a_distribution.ScheduledShipDate),
                a_dbHelper.AdjustPublishTime(a_distribution.ScheduledReceiveDate),
                a_distribution.Closed,
                a_distribution.PreferEmptyStorageArea,
                a_distribution.OverrideStorageConstraint,
                a_distribution.AllowPartialAllocations
            );
        }
    }
}