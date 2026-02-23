using PT.APSCommon;
using PT.Database;
using PT.Scheduler;

namespace PT.SchedulerData.Resources;

public static class ResourceRequirementsData
{
    #region PT Database
    public static void PtDbPopulate(this ResourceRequirementManager a_manager, ref PtDbDataSet dataSet, PtDbDataSet.JobOperationsRow jobOpRow)
    {
        for (int i = 0; i < a_manager.Count; i++)
        {
            ResourceRequirement curReq = a_manager.GetByIndex(i);
            curReq.PtDbPopulate(ref dataSet, curReq == a_manager.PrimaryResourceRequirement, jobOpRow);
        }
    }

    public static void PtDbPopulate(this ResourceRequirement a_rr, ref PtDbDataSet dataSet, bool primary, PtDbDataSet.JobOperationsRow jobOpRow)
    {
        //Values that don't exist in Scheduler.ResourceRequirement

        //Add ResourceRequirement row
        PtDbDataSet.JobResourcesRow rrRow = dataSet.JobResources.AddJobResourcesRow(
            jobOpRow.PublishDate,
            jobOpRow.InstanceId,
            a_rr.Operation.ManufacturingOrder.Job.Id.ToBaseType(),
            a_rr.Operation.ManufacturingOrder.Id.ToBaseType(),
            a_rr.Operation.Id.ToBaseType(),
            a_rr.Id.ToBaseType(),
            a_rr.Description,
            a_rr.ExternalId,
            a_rr.UsageStart.ToString(),
            a_rr.UsageEnd.ToString(),
            a_rr.AttentionPercent,
            primary,
            a_rr.DefaultResource_JITLimit.TotalHours,
            a_rr.DefaultResource_UseJITLimitTicks,
            a_rr.DefaultResource == null ? BaseId.NULL_ID.ToBaseType() : a_rr.DefaultResource.Id.ToBaseType(),
            a_rr.CapacityCode
        );

        //Add ResourceRequirement Capabilities
        for (int i = 0; i < a_rr.CapabilityManager.Count; i++)
        {
            Capability requiredCapability = a_rr.CapabilityManager.GetByIndex(i);
            dataSet.JobResourceCapabilities.AddJobResourceCapabilitiesRow(
                jobOpRow.PublishDate,
                jobOpRow.InstanceId,
                a_rr.Operation.ManufacturingOrder.Job.Id.ToBaseType(),
                a_rr.Operation.ManufacturingOrder.Id.ToBaseType(),
                a_rr.Operation.Id.ToBaseType(),
                a_rr.Id.ToBaseType(),
                requiredCapability.Id.ToBaseType(),
                requiredCapability.ExternalId);
        }
    }
    #endregion
}