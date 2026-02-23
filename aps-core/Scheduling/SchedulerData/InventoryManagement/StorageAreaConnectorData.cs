using PT.APSCommon;
using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

using static PT.Database.PtDbDataSet;

namespace PT.SchedulerData.InventoryManagement
{
    public static class StorageAreaConnector
    {
        public static void PtDbPopulate(this StorageAreaConnectorCollection a_storageAreaConnectorCollection, ScenarioDetail a_sd, ref PtDbDataSet r_dataSet, WarehousesRow a_parentWarehousesRow, PTDatabaseHelper a_dbHelper)
        {
            foreach (Scheduler.StorageAreaConnector storageAreaConnector in a_storageAreaConnectorCollection)
            {
                StorageAreaConnectorRow addStorageAreaConnectorRow = r_dataSet.StorageAreaConnector.AddStorageAreaConnectorRow(a_parentWarehousesRow.PublishDate, a_parentWarehousesRow.InstanceId,storageAreaConnector.Id.Value, storageAreaConnector.ExternalId, storageAreaConnector.Name, storageAreaConnector.Notes, storageAreaConnector.Description, a_parentWarehousesRow.WarehouseId, storageAreaConnector.CounterFlow, storageAreaConnector.StorageInFlowLimit, storageAreaConnector.StorageOutFlowLimit, storageAreaConnector.CounterflowLimit);

                foreach (BaseId resourceId in storageAreaConnector.ResourceInList)
                {
                    InternalResource resource = a_sd.PlantManager.GetResource(resourceId);
                    r_dataSet.ResourceStorageAreaConnectorIn.AddResourceStorageAreaConnectorInRow(a_parentWarehousesRow.PublishDate, a_parentWarehousesRow.InstanceId, resourceId.Value, storageAreaConnector.Id.Value, storageAreaConnector.ExternalId, resource.ExternalId, resource.Department.ExternalId, resource.Department.Plant.ExternalId);
                }

                foreach (BaseId resourceId in storageAreaConnector.ResourceOutList)
                {
                    InternalResource resource = a_sd.PlantManager.GetResource(resourceId);
                    r_dataSet.ResourceStorageAreaConnectorOut.AddResourceStorageAreaConnectorOutRow(a_parentWarehousesRow.PublishDate, a_parentWarehousesRow.InstanceId, resourceId.Value, storageAreaConnector.Id.Value, storageAreaConnector.ExternalId, resource.ExternalId, resource.Department.ExternalId, resource.Department.Plant.ExternalId);
                }

                foreach (BaseId storageAreaId in storageAreaConnector.StorageAreaInList)
                {
                    Warehouse warehouse = a_sd.WarehouseManager.GetByExternalId(a_parentWarehousesRow.ExternalId);
                    StorageArea storageArea = warehouse.StorageAreas.Find(storageAreaId);
                    r_dataSet.StorageAreaConnectorIn.AddStorageAreaConnectorInRow(a_parentWarehousesRow.PublishDate, a_parentWarehousesRow.InstanceId, storageAreaConnector.Id.Value, storageAreaConnector.ExternalId, storageArea.ExternalId, storageAreaId.Value);
                }

                foreach (BaseId storageAreaId in storageAreaConnector.StorageAreaOutList)
                {
                    Warehouse warehouse = a_sd.WarehouseManager.GetByExternalId(a_parentWarehousesRow.ExternalId);
                    StorageArea storageArea = warehouse.StorageAreas.Find(storageAreaId);
                    r_dataSet.StorageAreaConnectorOut.AddStorageAreaConnectorOutRow(a_parentWarehousesRow.PublishDate, a_parentWarehousesRow.InstanceId, storageAreaConnector.Id.Value, storageAreaConnector.ExternalId, storageArea.ExternalId, storageAreaId.Value);
                }
            }
        }
    }
}