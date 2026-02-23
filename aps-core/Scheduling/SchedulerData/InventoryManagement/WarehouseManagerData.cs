using PT.APSCommon;
using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class WarehouseManagerData
{
    #region PT Database
    public static void PtDbPopulate(this WarehouseManager a_warehouseManager, ScenarioDetail a_sd, ref PtDbDataSet dataSet, PtDbDataSet.ItemsDataTable itemsTable, PtDbDataSet.SchedulesRow schedulesRow, HashSet<BaseId> a_itemsToInclude, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_warehouseManager.Count; i++)
        {
            a_warehouseManager.GetByIndex(i).PtDbPopulate(a_sd, ref dataSet, itemsTable, schedulesRow, a_itemsToInclude, a_dbHelper);
        }
    }
    #endregion
}