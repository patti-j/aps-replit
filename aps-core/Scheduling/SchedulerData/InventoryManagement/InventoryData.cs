using PT.APSCommon;
using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData.InventoryManagement;

public static class InventoryData
{
    #region PT Database
    public static void PtDbPopulate(this InventoryManager a_manager, ScenarioDetail a_sd, ref PtDbDataSet dataSet, PtDbDataSet.ItemsDataTable itemsTable, PtDbDataSet.WarehousesRow a_warehouseRow, HashSet<BaseId> a_itemsList, PTDatabaseHelper a_dbHelper)
    {
        IEnumerator<Inventory> enumerator = a_manager.GetEnumerator();
        if (a_itemsList == null)
        {
            while (enumerator.MoveNext())
            {
                enumerator.Current.PtDbPopulate(a_sd, ref dataSet, itemsTable, a_warehouseRow, a_dbHelper);
            }
        }
        else
        {
            //Only pupulate items in the items include list
            while (enumerator.MoveNext())
            {
                if (a_itemsList.Contains(enumerator.Current.Item.Id))
                {
                    enumerator.Current.PtDbPopulate(a_sd, ref dataSet, itemsTable, a_warehouseRow, a_dbHelper);
                }
            }
        }
    }

    public static void PtDbPopulate(this Inventory a_inventory, ScenarioDetail a_sd, ref PtDbDataSet dataSet, PtDbDataSet.ItemsDataTable itemsTable, PtDbDataSet.WarehousesRow a_warehouseRow, PTDatabaseHelper a_dbHelper)
    {
        BaseId templateJobId;
        BaseId templateMoId;
        if (a_inventory.TemplateManufacturingOrder != null)
        {
            templateJobId = a_inventory.TemplateManufacturingOrder.Job.Id;
            templateMoId = a_inventory.TemplateManufacturingOrder.Id;
        }
        else
        {
            templateJobId = BaseId.NULL_ID;
            templateMoId = BaseId.NULL_ID;
        }

        //Add Inventory row
        PtDbDataSet.ItemsRow parentItemsRow = itemsTable.FindByPublishDateItemIdInstanceId(a_warehouseRow.PublishDate, a_inventory.Item.Id.ToBaseType(), a_warehouseRow.InstanceId);
        PtDbDataSet.InventoriesRow invRow = dataSet.Inventories.AddInventoriesRow(
            a_warehouseRow.PublishDate,
            a_warehouseRow.InstanceId,
            parentItemsRow.ItemId,
            a_warehouseRow.WarehouseId,
            a_inventory.Id.ToBaseType(),
            a_inventory.LeadTime.TotalDays,
            a_inventory.BufferStock,
            a_inventory.SafetyStock,
            a_inventory.SafetyStockWarningLevel,
            a_inventory.OnHandQty,
            a_inventory.PlannerExternalId,
            a_inventory.StorageCapacity,
            a_inventory.SafetyStockJobPriority,
            a_inventory.ForecastConsumption.ToString(),
            a_inventory.MrpProcessing.ToString(),
            a_inventory.MrpNotes,
            templateJobId.ToBaseType(),
            templateMoId.ToBaseType(),
            a_inventory.HaveTemplateManufacturingOrderId,
            Convert.ToDouble(a_inventory.BufferPenetrationPercent),
            a_inventory.MaterialAllocation.ToString(),
            a_inventory.PreventSharedBatchOverflow
        );

        if (a_inventory.ForecastVersions != null)
        {
            a_inventory.ForecastVersions.PtDbPopulate(ref dataSet, invRow, a_dbHelper);
        }

        AdjustmentArray array = a_inventory.GetRequirementAdjustments(a_sd);
        //To publish the lead time adjustments, they need to be precalculated.
        //AdjustmentArray array = GetAdjustmentArray().AddLeadTimeAdjustments(a_jobs);
        for (int aI = 0; aI < array.Count; aI++)
        {
            Adjustment adj = array[aI];
            adj.PtDbPopulate(ref dataSet, invRow, a_dbHelper);
        }

        a_inventory.Lots.PtDbPopulate(a_sd, ref dataSet, invRow, a_dbHelper, parentItemsRow, a_warehouseRow);
    }

    public static void PtDbPopulate(this Adjustment a_adjustment, ref PtDbDataSet dataSet, PtDbDataSet.InventoriesRow parentInvRow, PTDatabaseHelper a_dbHelper)
    {
        if (a_adjustment.AdjDate.Ticks <= a_dbHelper.MaxPublishDate.Ticks)
        {
            BaseId storageAreaId = BaseId.NULL_ID;
            BaseId lotId = BaseId.NULL_ID;
            if (a_adjustment.HasLotStorage)
            {
                storageAreaId = a_adjustment.Storage.StorageArea.Id;
                lotId = a_adjustment.Storage.Lot.Id;
            }

            if (a_adjustment is ActivityAdjustment) //this is for Products and Materials.
            {
                dataSet.JobActivityInventoryAdjustments.AddJobActivityInventoryAdjustmentsRow(
                    parentInvRow.PublishDate,
                    parentInvRow.InstanceId,
                    parentInvRow.InventoryId,
                    a_adjustment.GetReason().Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(a_adjustment.AdjDate),
                    a_adjustment.ChangeQty,
                    a_adjustment.ReasonDescription,
                    storageAreaId.Value,
                    lotId.Value
                );
            }
            else if (a_adjustment is SalesOrderAdjustment)
            {
                dataSet.SalesOrderDistributionInventoryAdjustments.AddSalesOrderDistributionInventoryAdjustmentsRow(
                    parentInvRow.PublishDate,
                    parentInvRow.InstanceId,
                    parentInvRow.InventoryId,
                    a_adjustment.GetReason().Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(a_adjustment.AdjDate),
                    a_adjustment.ChangeQty,
                    a_adjustment.ReasonDescription,
                    storageAreaId.Value,
                    lotId.Value
                );
            }
            else if (a_adjustment is ForecastAdjustment)
            {
                dataSet.ForecastShipmentInventoryAdjustments.AddForecastShipmentInventoryAdjustmentsRow(
                    parentInvRow.PublishDate,
                    parentInvRow.InstanceId,
                    parentInvRow.InventoryId,
                    a_adjustment.GetReason().Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(a_adjustment.AdjDate),
                    a_adjustment.ChangeQty,
                    a_adjustment.ReasonDescription,
                    storageAreaId.Value,
                    lotId.Value
                );
            }
            else if (a_adjustment is TransferOrderAdjustment)
            {
                dataSet.TransferOrderDistributionInventoryAdjustments.AddTransferOrderDistributionInventoryAdjustmentsRow(
                    parentInvRow.PublishDate,
                    parentInvRow.InstanceId,
                    parentInvRow.InventoryId,
                    a_adjustment.GetReason().Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(a_adjustment.AdjDate),
                    a_adjustment.ChangeQty,
                    a_adjustment.ReasonDescription,
                    storageAreaId.Value,
                    lotId.Value
                );
            }
            else if (a_adjustment is PurchaseOrderAdjustment)
            {
                dataSet.PurchaseToStockInventoryAdjustments.AddPurchaseToStockInventoryAdjustmentsRow(
                    parentInvRow.PublishDate,
                    parentInvRow.InstanceId,
                    parentInvRow.InventoryId,
                    a_adjustment.Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(a_adjustment.AdjDate),
                    a_adjustment.ChangeQty,
                    a_adjustment.ReasonDescription,
                    storageAreaId.Value,
                    lotId.Value
                );
            }
        }
    }

    public static void PtDbPopulate(this LotManager a_lotManager, ScenarioDetail a_sd, ref PtDbDataSet dataSet, PtDbDataSet.InventoriesRow a_invRow, PTDatabaseHelper a_dbHelper, PtDbDataSet.ItemsRow a_parentItemsRow, PtDbDataSet.WarehousesRow a_warehouseRow)
    {
        foreach (Lot lot in a_lotManager)
        {
            lot.PtDbPopulate(a_sd, ref dataSet, a_invRow, a_dbHelper, a_parentItemsRow, a_warehouseRow);
        }
    }

    private static void PtDbPopulate(this Lot a_lot, ScenarioDetail a_sd, ref PtDbDataSet dataSet, PtDbDataSet.InventoriesRow a_invRow, PTDatabaseHelper a_dbHelper, PtDbDataSet.ItemsRow a_parentItemsRow, PtDbDataSet.WarehousesRow a_warehouseRow)
    {
        PtDbDataSet.LotsRow LotRow = dataSet.Lots.AddLotsRow(
            a_invRow.PublishDate,
            a_invRow.InstanceId,
            a_lot.Id.ToBaseType(),
            a_lot.ExternalId,
            a_invRow.InventoryId,
            a_lot.LotSource.ToString(),
            a_lot.Qty,
            a_dbHelper.AdjustPublishTime(a_lot.ProductionDate),
            a_lot.Code,
            a_lot.LimitMatlSrcToEligibleLots);
        
        Item item = a_sd.ItemManager.GetByExternalId(a_invRow.ItemsRowParent.ExternalId);
        foreach (LotStorage lotStorage in a_lot.GetLotStorages())
        {
            dataSet.ItemStorageLots.AddItemStorageLotsRow(
                a_parentItemsRow.ExternalId,
                a_warehouseRow.ExternalId,
                lotStorage.StorageArea.ExternalId,
                a_invRow.PublishDate,
                a_invRow.InstanceId,
                lotStorage.StorageArea.GetItemStorage(item).Id.Value.ToString(),
                a_lot.Id.Value,
                a_lot.ExternalId,
                lotStorage.Qty
            );
        }
    }
    #endregion
}