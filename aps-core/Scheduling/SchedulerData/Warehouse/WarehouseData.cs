using PT.APSCommon;
using PT.Database;
using PT.Scheduler;
using PT.SchedulerData.InventoryManagement;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class WarehouseData
{
    public static void PtDbPopulate(this Warehouse a_wrhs, ScenarioDetail a_sd, ref PtDbDataSet r_dataSet, PtDbDataSet.ItemsDataTable a_itemsTable, PtDbDataSet.SchedulesRow a_schedulesRow, HashSet<BaseId> a_itemsToInclude, PTDatabaseHelper a_dbHelper)
    {
        PtDbDataSet.WarehousesRow warehouseRow = r_dataSet.Warehouses.AddWarehousesRow(
            a_schedulesRow,
            a_schedulesRow.InstanceId,
            a_wrhs.Id.ToBaseType(),
            a_wrhs.Name,
            a_wrhs.Description,
            a_wrhs.ExternalId,
            a_wrhs.Notes,
            a_wrhs.StorageCapacity,
            a_wrhs.AnnualPercentageRate
        );

        //Populate Storage Areas
        a_wrhs.StorageAreas.PtDbPopulate(a_sd, ref r_dataSet, a_itemsTable, warehouseRow, a_itemsToInclude, a_dbHelper);

        //Populate Inventories
        a_wrhs.Inventories.PtDbPopulate(a_sd, ref r_dataSet, a_itemsTable, warehouseRow, a_itemsToInclude, a_dbHelper);

        a_wrhs.PtDbPopulateUserFields(warehouseRow, a_sd, a_dbHelper.UserFieldDefinitions);


        //Populate Storage Area Connectors
        a_wrhs.StorageAreaConnectors.PtDbPopulate(a_sd, ref r_dataSet,warehouseRow,a_dbHelper);
    }
}