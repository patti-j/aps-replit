using PT.APSCommon;
using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class DepartmentData
{
    #region PT Database
    public static void PtDbPopulate(this Department a_dpt, ref PtDbDataSet dataSet, PtDbDataSet.PlantsRow plantRow, bool a_limitToResources, HashSet<BaseId> a_resourceIds, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        //Add Department row
        PtDbDataSet.DepartmentsRow deptRow = dataSet.Departments.AddDepartmentsRow(
            plantRow.PublishDate,
            plantRow.InstanceId,
            plantRow.PlantId,
            a_dpt.Id.ToBaseType(),
            a_dpt.Name,
            a_dpt.Description,
            a_dpt.Notes,
            a_dpt.ExternalId,
            a_dpt.Plant.Name,
            a_dpt.ResourceCount,
            a_dpt.FrozenSpan.TotalDays
        );

        a_dpt.PtDbPopulateUserFields(deptRow, a_sd, a_dbHelper.UserFieldDefinitions);

        //Add Departments
        a_dpt.Resources.PtDbPopulate(ref dataSet, deptRow, a_limitToResources, a_resourceIds, a_sd, a_dbHelper);
    }

    //DB Export
    public static void PtDbPopulate(this DepartmentManager a_departmentManager, ref PtDbDataSet dataSet, PtDbDataSet.PlantsRow plantRow, bool a_limitToResources, HashSet<BaseId> a_resourceIds, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_departmentManager.Count; i++)
        {
            a_departmentManager[i].PtDbPopulate(ref dataSet, plantRow, a_limitToResources, a_resourceIds, a_sd, a_dbHelper);
        }
    }

    /// <summary>
    /// Calculate fields that need to be calculated by Resource, prior to exporting to PT database.
    /// </summary>
    public static void PreProcessWorkForPtDbPopulate(this Department a_department)
    {
        for (int i = 0; i < a_department.Resources.Count; i++)
        {
            a_department.Resources[i].PreProcessWorkForPtDbPopulate();
        }
    }
    #endregion
}