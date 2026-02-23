using PT.APSCommon;
using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

using PT.Scheduler.Schedule.InventoryManagement;
using static PT.Database.PtDbDataSet;

namespace PT.SchedulerData.InventoryManagement
{
    public static class StorageAreaData
    {
        public static void PtDbPopulate(this StorageAreaCollection a_storageAreaCollection, ScenarioDetail a_sd, ref PtDbDataSet r_dataSet, PtDbDataSet.ItemsDataTable a_itemsTable, PtDbDataSet.WarehousesRow a_parentWarehousesRow, HashSet<BaseId> a_itemsToInclude, PTDatabaseHelper a_dbHelper)
        {
            foreach (StorageArea storageArea in a_storageAreaCollection)
            {
                //Now populate Storage Area row
                StorageAreasRow storageAreaRow = r_dataSet.StorageAreas.AddStorageAreasRow(a_parentWarehousesRow.PublishDate,
                    a_parentWarehousesRow.InstanceId,
                    a_parentWarehousesRow.WarehouseId,
                    storageArea.Name,
                    storageArea.ExternalId,
                    storageArea.Description,
                    storageArea.Notes,
                    storageArea.Id.Value,
                    storageArea.SingleItemStorage,
                    storageArea.StorageInFlowLimit,
                    storageArea.StorageOutFlowLimit,
                    storageArea.CounterFlowLimit,
                    storageArea.Resource?.Department.Plant.ExternalId,
                    storageArea.Resource?.Department.ExternalId,
                    storageArea.Resource?.ExternalId);

                foreach (Item item in storageArea.GetItemsStored())
                {
                    ItemStorage itemStorage = storageArea.GetItemStorage(item);
                    r_dataSet.ItemStorage.AddItemStorageRow(item.ExternalId, a_parentWarehousesRow.ExternalId, storageAreaRow,itemStorage.MaxQty,a_parentWarehousesRow.PublishDate, a_parentWarehousesRow.InstanceId, itemStorage.Id.ToString(), itemStorage.DisposalQty, itemStorage.DisposeImmediately);
                }
            }
        }
    }
}