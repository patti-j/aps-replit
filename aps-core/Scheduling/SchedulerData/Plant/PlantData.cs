using PT.APSCommon;
using PT.Database;
using PT.ERPTransmissions;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class PlantData
{
    public static void PopulateImportDataSet(this PlantManager a_pm, PtImportDataSet a_ds)
    {
        for (int i = 0; i < a_pm.Count; i++)
        {
            a_pm[i].PopulateImportDataSet(a_ds.Plants);
        }
    }

    public static void PtDbPopulate(this Plant a_plant, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow schedulesRow, bool a_limitToResources, HashSet<BaseId> a_resourceIds, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        //Add Plant row
        PtDbDataSet.PlantsRow plantRow = dataSet.Plants.AddPlantsRow(
            schedulesRow,
            schedulesRow.InstanceId,
            a_plant.Id.ToBaseType(),
            a_plant.Name,
            a_plant.Description,
            a_plant.Notes,
            a_plant.BottleneckThreshold,
            a_plant.HeavyLoadThreshold,
            a_plant.ExternalId,
            a_plant.DepartmentCount,
            a_plant.StableSpan.TotalDays,
            a_plant.DailyOperatingExpense,
            a_plant.InvestedCapital,
            a_plant.AnnualPercentageRate
        );

        a_plant.PtDbPopulateUserFields(plantRow, a_sd, a_dbHelper.UserFieldDefinitions);

        //Add Departments
        a_plant.Departments.PtDbPopulate(ref dataSet, plantRow, a_limitToResources, a_resourceIds, a_sd, a_dbHelper);
    }

    public static void PtDbPopulate(this PlantManager a_plantManager, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow schedulesRow, bool a_limitToResources, HashSet<BaseId> a_resourceIds, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_plantManager.Count; i++)
        {
            a_plantManager[i].PtDbPopulate(ref dataSet, schedulesRow, a_limitToResources, a_resourceIds, a_sd, a_dbHelper);
        }
    }

    /// <summary>
    /// Calculate fields that need to be calculated by Plant, prior to exporting to PT database.
    /// </summary>
    public static void PreProcessWorkForPtDbPopulate(this PlantManager a_manager)
    {
        for (int i = 0; i < a_manager.Count; i++)
        {
            a_manager[i].PreProcessWorkForPtDbPopulate();
        }
    }

    /// <summary>
    /// Calculate fields that need to be calculated by Department, prior to exporting to PT database.
    /// </summary>
    public static void PreProcessWorkForPtDbPopulate(this Plant a_plant)
    {
        for (int i = 0; i < a_plant.DepartmentCount; i++)
        {
            a_plant.Departments[i].PreProcessWorkForPtDbPopulate();
        }
    }
}