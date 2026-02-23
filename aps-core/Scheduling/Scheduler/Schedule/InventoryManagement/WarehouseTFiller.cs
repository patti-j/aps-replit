using PT.ERPTransmissions;

namespace PT.Scheduler.Schedule.InventoryManagement;

public class WarehouseTFiller
{
    /// <summary>
    /// Fills a PtImportDataSet given a Warehouse
    /// </summary>
    /// <param name="a_ds">The dataset to fill</param>
    /// <param name="a_wh">The source Warehouse</param>
    /// <param name="a_fillOnlyUsedInventories">Boolean to flag if the dataset should be filled with all warehouse data or only if the inventory items are being used/referenced. Default is false.</param>
    /// <param name="a_jobManager">JobManager reference used to determine if inventory items are used in any Material Requirements.</param>
    public static void Fill(PtImportDataSet a_ds, Warehouse a_wh, bool a_fillOnlyUsedInventories = false, JobManager a_jobManager = null)
    {
        if (a_fillOnlyUsedInventories && a_jobManager == null)
        {
            return;
        }

        PtImportDataSet.WarehousesRow whRow = a_ds.Warehouses.NewWarehousesRow();
        whRow.ExternalId = a_wh.ExternalId;
        whRow.Name = a_wh.Name;
        whRow.Description = a_wh.Description;
        whRow.Notes = a_wh.Notes;
        whRow.StorageCapacity = a_wh.StorageCapacity;
        whRow.UserFields = a_wh.UserFields?.GetUserFieldImportString() ?? "";
        whRow.AnnualPercentageRate = a_wh.AnnualPercentageRate;

        bool anyMRUsesWarehouse = a_jobManager.Any(x => x.GetOperations().Any(op => op.MaterialRequirements.Any(mr => mr.Warehouse?.ExternalId == a_wh.ExternalId)));
        if (!a_fillOnlyUsedInventories || (a_fillOnlyUsedInventories && anyMRUsesWarehouse))
        {
            //Add the warehouse to the datasource
            a_ds.Warehouses.AddWarehousesRow(whRow);

            foreach (Inventory itemInv in a_wh.Inventories)
            {
                bool anyMRUsesItem = a_jobManager.Any(x => x.GetOperations().Any(op => op.MaterialRequirements.Any(mr => mr.Item?.ExternalId == itemInv?.Item?.ExternalId)));
                bool itemAdded = false;

                //If the item is not null then add an items row to the datasource
                if ((itemInv.Item != null && !a_fillOnlyUsedInventories) || (a_fillOnlyUsedInventories && anyMRUsesItem))
                {
                    Item item = itemInv.Item;
                    PtImportDataSet.ItemsRow itemRow = a_ds.Items.NewItemsRow();
                    itemRow.ExternalId = item.ExternalId;
                    itemRow.Name = item.Name;
                    itemRow.Description = item.Description;
                    itemRow.Notes = item.Notes;
                    itemRow.Source = item.Source.ToString();
                    itemRow.ItemType = item.ItemType.ToString();
                    itemRow.DefaultLeadTimeDays = item.DefaultLeadTime.TotalDays;
                    itemRow.BatchSize = item.BatchSize;
                    itemRow.BatchWindowHrs = item.BatchWindow.TotalHours;
                    itemRow.MinOrderQty = item.MinOrderQty;
                    itemRow.MaxOrderQty = item.MaxOrderQty;
                    itemRow.MinOrderQtyRoundupLimit = item.MinOrderQtyRoundupLimit;
                    itemRow.JobAutoSplitQty = item.JobAutoSplitQty;
                    itemRow.ShelfLifeHrs = item.ShelfLife.TotalHours;
                    itemRow.TransferQty = item.TransferQty;
                    itemRow.ItemGroup = item.ItemGroup;
                    itemRow.PlanInventory = item.PlanInventory;
                    itemRow.RollupAttributesToParent = item.RollupAttributesToParent;
                    itemRow.Cost = item.Cost;
                    itemRow.UserFields = item.UserFields?.GetUserFieldImportString() ?? "";
                    itemAdded = true;

                    a_ds.Items.AddItemsRow(itemRow);
                }

                //Add a new inventory row after adding the item to fulfill the FK restraint ITEM_INVENTORY
                if (itemAdded)
                {
                    PtImportDataSet.InventoriesRow invRow = a_ds.Inventories.NewInventoriesRow();
                    invRow.ItemExternalId = itemInv?.Item?.ExternalId ?? "";
                    invRow.MrpProcessing = itemInv.MrpProcessing.ToString();
                    invRow.MaterialAllocation = itemInv.MaterialAllocation.ToString();
                    invRow.BufferStock = itemInv.BufferStock;
                    invRow.SafetyStock = itemInv.SafetyStock;
                    invRow.SafetyStockWarningLevel = itemInv.SafetyStockWarningLevel;
                    invRow.LeadTimeDays = itemInv.LeadTime.TotalDays;
                    invRow.StorageCapacity = itemInv.StorageCapacity;
                    invRow.PlannerExternalId = itemInv.PlannerExternalId;
                    invRow.SafetyStockJobPriority = itemInv.SafetyStockJobPriority;
                    invRow.ForecastConsumption = itemInv.ForecastConsumption.ToString();
                    invRow.MaxInventory = itemInv.MaxInventory;
                    invRow.TemplateJobExternalId = itemInv.TemplateManufacturingOrder?.Job?.ExternalId ?? "";
                    invRow.ReceivingBufferDays = itemInv.ReceivingBuffer.TotalDays;
                    invRow.ShippingBufferDays = itemInv.ShippingBuffer.TotalDays;
                    invRow.AutoGenerateForecasts = itemInv.AutoGenerateForecasts;
                    invRow.ForecastInterval = itemInv.ForecastInterval.ToString();
                    invRow.NumberOfIntervalsToForecast = itemInv.NumberOfIntervalsToForecast;
                    invRow.MrpExcessQuantityAllocation = itemInv.MrpExcessQuantityAllocation.ToString();
                    invRow.ForecastConsumptionWindowDays = itemInv.ForecastConsumptionWindowDays;

                    //Add inventory row to datasource
                    a_ds.Inventories.AddInventoriesRow(invRow);
                }


                //Add a lot row for each inventory lot. 
                foreach (Lot lot in itemInv.Lots)
                {
                    PtImportDataSet.LotsRow lotRow = a_ds.Lots.NewLotsRow();
                    lotRow.ExternalId = lot.ExternalId;
                    lotRow.LotProductionDate = lot.ProductionDate;
                    //lotRow.Qty = lot.Qty;
                    lotRow.ItemExternalId = itemInv?.Item?.ExternalId ?? "";
                    lotRow.Code = lot.Code;
                    lotRow.LimitMatlSrcToEligibleLots = lot.LimitMatlSrcToEligibleLots;

                    a_ds.Lots.AddLotsRow(lotRow);
                }
            }
        }
    }
}