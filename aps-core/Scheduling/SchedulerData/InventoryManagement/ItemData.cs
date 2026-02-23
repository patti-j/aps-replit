using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class ItemData
{
    public static void PopulateImportDataSet(this ItemManager a_im, ERPTransmissions.PtImportDataSet ds)
    {
        for (int i = 0; i < a_im.Count; i++)
        {
            a_im.GetByIndex(i).PopulateImportDataSet(ds.Items);
        }
    }

    #region PT Database
    public static void PtDbPopulate(this Item a_itemData, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow schedulesRow, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        PtDbDataSet.ItemsRow itemRow = dataSet.Items.AddItemsRow(
            schedulesRow,
            schedulesRow.InstanceId,
            a_itemData.Id.ToBaseType(),
            a_itemData.Name,
            a_itemData.Description,
            a_itemData.ExternalId,
            a_itemData.Notes,
            a_itemData.Source.ToString(),
            a_itemData.ItemType.ToString(),
            a_itemData.DefaultLeadTime.TotalDays,
            a_itemData.BatchSize,
            a_itemData.BatchWindow.TotalDays,
            a_itemData.ItemGroup,
            a_itemData.MinOrderQty,
            a_itemData.MaxOrderQty,
            a_itemData.MinOrderQtyRoundupLimit,
            a_itemData.JobAutoSplitQty,
            a_itemData.PlanInventory,
            a_itemData.ShelfLife.TotalDays,
            a_itemData.TransferQty,
            a_itemData.RollupAttributesToParent,
            a_itemData.Cost
        );

        a_itemData.PtDbPopulateUserFields(itemRow, a_sd, a_dbHelper.UserFieldDefinitions);
    }

    public static void PtDbPopulate(this ItemManager a_itemManager, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow schedulesRow, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_itemManager.Count; i++)
        {
            a_itemManager.GetByIndex(i).PtDbPopulate(ref dataSet, schedulesRow, a_sd, a_dbHelper);
        }
    }
    #endregion PT Database
}