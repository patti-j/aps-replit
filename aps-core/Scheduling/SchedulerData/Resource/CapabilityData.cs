using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class CapabilityData
{
    #region PT Database
    public static void PtDbPopulate(this Capability a_cpblty, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow schedulesRow, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        //Add Capability row
        PtDbDataSet.CapabilitiesRow capabilityRow = dataSet.Capabilities.AddCapabilitiesRow(
            schedulesRow,
            schedulesRow.InstanceId,
            a_cpblty.Id.ToBaseType(),
            a_cpblty.Name,
            a_cpblty.Description,
            a_cpblty.Notes,
            a_cpblty.ExternalId);

        a_cpblty.PtDbPopulateUserFields(capabilityRow, a_sd, a_dbHelper.UserFieldDefinitions);

        for (int resI = 0; resI < a_cpblty.Resources.Count; resI++)
        {
            InternalResource resource = a_cpblty.Resources.GetByIndex(resI);
            dataSet.ResourceCapabilities.AddResourceCapabilitiesRow(
                capabilityRow.PublishDate,
                capabilityRow.InstanceId,
                capabilityRow.CapabilityId,
                resource.Department.Plant.Id.ToBaseType(),
                resource.Department.Id.ToBaseType(),
                resource.Id.ToBaseType());
        }
    }

    public static void PtDbPopulate(this CapabilityManager a_capabilityManager, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow schedulesRow, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_capabilityManager.Count; i++)
        {
            a_capabilityManager.GetByIndex(i).PtDbPopulate(ref dataSet, schedulesRow, a_sd, a_dbHelper);
        }
    }
    #endregion
}