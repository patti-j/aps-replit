using System.Text;

using PT.APSCommon.Extensions;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerExtensions.Activity;

public static class ActivityExtensions
{
    /// <summary>
    /// The amount of time scheduled for tank storage processing.
    /// </summary>
    public static TimeSpan GetScheduledStorageSpan(this InternalActivity a_act)
    {
        return a_act.Batch.StorageSpan.TimeSpan;
    }
    
    /// <summary>
    /// Returns a readable name for the activity that includes job name, operation name, and potentially MO and Activity values if more than one exists in the job.
    /// </summary>
    /// <param name="a_act"></param>
    public static string GetReadableActivityName(this InternalActivity a_act)
    {
        if (a_act == null)
        {
            return string.Empty;
        }

        InternalOperation op = a_act.Operation;
        ManufacturingOrder mo = op.ManufacturingOrder;
        Scheduler.Job j = mo.Job;
        StringBuilder sb = new ("Job".Localize());
        sb.Append(" ");

        sb.Append(j.Name.QuotationSingle());
        sb.Append(" ");
        if (j.ManufacturingOrderCount != 1)
        {
            sb.Append(mo.Name.QuotationSingle());
            sb.Append(" ");
        }

        if (mo.OperationCount != 1)
        {
            sb.Append(op.Name.QuotationSingle());
            sb.Append(" ");
        }

        if (op.Activities.Count != 1)
        {
            sb.Append(!string.IsNullOrEmpty(a_act.ExternalId) ? a_act.ExternalId.QuotationSingle() : a_act.Id.ToString().QuotationSingle());
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// The amount of capacity required for the setup stage.
    /// </summary>
    public static TimeSpan StandardSetupSpan(this InternalActivity a_act)
    {
        return a_act.Batch != null ? TimeSpan.FromTicks(a_act.Batch.SetupCapacitySpan.TimeSpanTicks) : TimeSpan.Zero;
    }

    /// <summary>
    /// The amount of capacity required for the processing stage.
    /// </summary>
    public static TimeSpan StandardRunSpan(this InternalActivity a_act)
    {
        return a_act.Batch != null ? TimeSpan.FromTicks(a_act.Batch.ProcessingCapacitySpan.TimeSpanTicks) : TimeSpan.Zero;
    }

    public static ActivityKey GetActivityKey(this InternalActivity a_act)
    {
        return new ActivityKey(a_act.Operation.Job.Id, a_act.Operation.ManufacturingOrder.Id, a_act.Operation.Id, a_act.Id);
    }
}