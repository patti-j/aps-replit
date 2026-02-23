using PT.APSCommon.Extensions;
using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Defines the possible scheduling result of an activity on this result. The primary resource requirement is used to calculate
/// </summary>
public class SchedulableInfo
{
    /// <summary>
    /// The result of trying to determine whether a primary resource requirement can be scheduled.
    /// </summary>
    /// <param name="zeroLength">Whether the result of scheduling the primary resource requirement will result in a zero length operation.</param>
    /// <param name="scheduledStartDate">The start date of this SchedulableInfo.</param>
    /// <param name="setupFinishDate">The time the setup would finish.</param>
    /// <param name="processingFinishDate">The time the processing would finish.</param>
    /// <param name="finishDate">The time the activity would finish.</param>
    /// <param name="resource">The resource the primary resource requirement would be scheudled on.</param>
    /// <param name="requiredSetupTicks">The amount of setup time necessary.</param>
    /// <param name="requiredProcessingTicks">The amount of processing time necessary.</param>
    /// <param name="requiredPostProcessingTicks">The amount of post processing time necessary.</param>
    internal SchedulableInfo(bool a_zeroLength,
                             long a_scheduledStartDate,
                             long a_setupFinishDate,
                             long a_processingFinishDate,
                             long a_postProcessingFinishDate,
                             long a_storageFinishDate,
                             long a_cleanFinishDate,
                             long a_finishDate,
                             Resource a_resource,
                             RequiredSpanPlusClean a_requiredAdditionalCleanBeforeSpan,
                             RequiredSpanPlusSetup a_requiredSetupSpan,
                             RequiredSpan a_requiredProcessingSpan,
                             RequiredSpan a_requiredPostProcessingSpan,
                             RequiredSpan a_storageSpan,
                             RequiredSpanPlusClean a_requiredCleanAfterAfterSpan,
                             long a_requiredNumberOfCycles,
                             decimal a_requiredQty,
                             OperationCapacityProfile a_oCP)
    {
        // Whether the result of scheduling is zero length
        m_zeroLength = a_zeroLength;

        // The start date of activitiy you're trying to schedule.
        m_scheduledStartDate = a_scheduledStartDate;

        // The dates the various stages will complete.
        m_setupFinishDate = a_setupFinishDate;
        m_productionFinishDate = a_processingFinishDate;
        m_postProcessingFinishDate = a_postProcessingFinishDate;
        m_storageFinishDate = a_storageFinishDate;
        m_cleanFinishDate = a_cleanFinishDate;
        m_finishDate = a_finishDate;

        // A reference back to the resource the primary resource requirement will be scheduled on.
        m_resource = a_resource;

        // The lengths of times the various stages will take.
        m_requiredSetupSpan = a_requiredSetupSpan;
        m_requiredProcessingSpan = a_requiredProcessingSpan;
        m_requiredPostProcessingSpan = a_requiredPostProcessingSpan;
        m_requiredStorageSpan = a_storageSpan;
        m_requiredCleanAfterSpan = a_requiredCleanAfterAfterSpan;

        m_requiredNumberOfCycles = a_requiredNumberOfCycles;
        m_requiredQty = a_requiredQty;

        //The additional clean time that will need to be added to the end of the last operation
        m_requiredAdditionalCleanBeforeSpan = a_requiredAdditionalCleanBeforeSpan;

        m_ocp = a_oCP;
    }

    /// <summary>
    /// Whether the primary resource requirement was calculated to be zero-length and the state of the activities isn't finished.
    /// </summary>
    public readonly bool m_zeroLength;

    /// <summary>
    /// The start time of the primary resource requirement.
    /// </summary>
    public readonly long m_scheduledStartDate;

    /// <summary>
    /// The finish date of the setup phase.
    /// </summary>
    public readonly long m_setupFinishDate;

    /// <summary>
    /// The finish date of production stage.
    /// </summary>
    public readonly long m_productionFinishDate;

    /// <summary>
    /// The finish date of processing stage.
    /// </summary>
    public readonly long m_postProcessingFinishDate;

    /// <summary>
    /// The finish date of storage stage.
    /// </summary>
    public readonly long m_storageFinishDate;

    /// <summary>
    /// The finish date of clean stage.
    /// </summary>
    public readonly long m_cleanFinishDate;    

    /// <summary>
    /// The finish date of storage and the operation.
    /// </summary>
    public readonly long m_finishDate;

    /// <summary>
    /// The resource the primary resource requirement was scheduled on.
    /// </summary>
    internal Resource m_resource;

    /// <summary>
    /// The required setup capacity.
    /// </summary>
    public readonly RequiredSpanPlusSetup m_requiredSetupSpan;

    /// <summary>
    /// The required processing capacity.
    public readonly RequiredSpan m_requiredProcessingSpan;

    /// <summary>
    /// The required post-processing capacity.
    /// </summary>
    public readonly RequiredSpan m_requiredPostProcessingSpan;

    /// <summary>
    /// The required storage capacity.
    /// </summary>
    public readonly RequiredSpan m_requiredStorageSpan;

    /// <summary>
    /// The required sequence dependant clean capacity that needs to be added to the previous Clean
    /// Only clean capacity that is calculated while this activity is being scheduled will be represented here.
    /// </summary>
    public readonly RequiredSpanPlusClean m_requiredAdditionalCleanBeforeSpan;

    /// <summary>
    /// The required non-sequence dependant clean capacity.
    /// Only clean capacity that is calculated while this activity is being scheduled will be represented here.
    /// </summary>
    public readonly RequiredSpanPlusClean m_requiredCleanAfterSpan;

    /// <summary>
    /// The number of cycles that need to be run.
    /// </summary>
    public readonly long m_requiredNumberOfCycles;

    /// <summary>
    /// The amount of material that is to be made.
    /// </summary>
    public readonly decimal m_requiredQty;
    
    /// <summary>
    /// In the event overlap is possible this specifies the set of capacity profiles used by the operation.
    /// </summary>
    internal OperationCapacityProfile m_ocp;

    /// <summary>
    /// The time when procesing starts.
    /// </summary>
    internal long ProcStartDate => m_setupFinishDate;

    internal long GetTotalRequiredNumberOfTicks()
    {
        long ticks = m_requiredSetupSpan.TimeSpanTicks + m_requiredProcessingSpan.TimeSpanTicks + m_requiredPostProcessingSpan.TimeSpanTicks;
        return ticks;
    }

    internal long GetTotalRequiredNumberOfSetupAndProcessingTicks()
    {
        long ticks = m_requiredSetupSpan.TimeSpanTicks + m_requiredProcessingSpan.TimeSpanTicks;
        return ticks;
    }

    public override string ToString()
    {
        return string.Format("su {0}[{1}] -> proc {2}[{3}] -> pp {4}[{5}] -> fin {6}".Localize(),
            DateTimeHelper.ToLocalTimeFromUTCTicks(m_scheduledStartDate),
            PrintTimeSpan(m_scheduledStartDate, ProcStartDate),
            DateTimeHelper.ToLocalTimeFromUTCTicks(ProcStartDate),
            PrintTimeSpan(m_requiredProcessingSpan.TimeSpanTicks),
            DateTimeHelper.ToLocalTimeFromUTCTicks(m_productionFinishDate),
            PrintTimeSpan(m_productionFinishDate, m_finishDate),
            DateTimeHelper.ToLocalTimeFromUTCTicks(m_finishDate));
    }

    internal string PrintTimeSpan(long a_start, long a_end)
    {
        long diff = a_end - a_start;
        return PrintTimeSpan(diff);
    }

    internal string PrintTimeSpan(long a_ts)
    {
        if (a_ts == 0)
        {
            return "0";
        }

        return DateTimeHelper.PrintTimeSpan(a_ts);
    }

    /// <summary>
    /// Get the start time of a resource requirements UsageStart. For instance
    /// if UsageStart==Setup, this object's m_scheduledStartDate will be returned.
    /// If UsageStart==Run, this object's m_setupFinishDate would be returned.
    /// </summary>
    /// <param name="a_rr">The resource requirement whose UsageStart you need.</param>
    /// <returns></returns>
    internal long GetUsageStart(ResourceRequirement a_rr)
    {
        long start;

        // Note some of the usages end at the same time.
        switch (a_rr.UsageStart)
        {
            case MainResourceDefs.usageEnum.Setup:
                start = m_scheduledStartDate;
                break;

            case MainResourceDefs.usageEnum.Run:
                start = m_setupFinishDate;
                break;

            case MainResourceDefs.usageEnum.PostProcessing:
                start = m_productionFinishDate;
                break;

            case MainResourceDefs.usageEnum.Storage:
                start = m_postProcessingFinishDate;
                break;

            case MainResourceDefs.usageEnum.Clean:
                start = m_storageFinishDate;
                break;

            default:
                throw new Exception("An unhandled UsageEnum was encountered.".Localize());
        }

        return start;
    }

    /// <summary>
    /// Get the time of a resource requirements UsageEnd. For instance
    /// if UsageStart==StoragePostProcessing, this object's m_finishDate will be returned.
    /// </summary>
    /// <param name="a_rr">The resource requirement whose UsageStart you need.</param>
    /// <returns></returns>
    internal long GetUsageEnd(ResourceRequirement a_rr)
    {
        long end;

        // Note some of the usages end at the same time.
        switch (a_rr.UsageEnd)
        {
            case MainResourceDefs.usageEnum.Setup:
                end = m_setupFinishDate;
                break;

            case MainResourceDefs.usageEnum.Run:
                end = m_productionFinishDate;
                break;

            case MainResourceDefs.usageEnum.PostProcessing:
                end = m_postProcessingFinishDate;
                break;
            
            case MainResourceDefs.usageEnum.Storage:
                end = m_storageFinishDate;
                break;

            case MainResourceDefs.usageEnum.Clean:
                end = m_cleanFinishDate;
                break;

            default:
                end = m_finishDate;
                break;
        }

        return end;
    }

    /// <summary>
    /// Whether a resource requirement requires 0 capacity. This SchedulableInfo must have been calculated for the activity
    /// the resource requirement is for.
    /// </summary>
    /// <param name="a_rr">The ResourceRequirement to test.</param>
    /// <returns>true if the ResourceRequirement requires 0 capacity.</returns>
    internal bool ZeroCapacityRequired(ResourceRequirement a_rr)
    {
        //TODO: This is a workaround for storage usages since there is no known capacity yet.
        // Ideally this would have been set to the duration of max storage, and reduced later after the storage has been calculated
        // Without this, storage only helpers would never schedule
        if (a_rr.UsageStart == MainResourceDefs.usageEnum.Storage)
        {
            return false;
        }

        long start = GetUsageStart(a_rr);
        long end = GetUsageEnd(a_rr);

        long capacityRequired = end - start;

        return capacityRequired == 0;
    }

    /// <summary>
    /// Get non primary Setup end date based on its usage
    /// </summary>
    /// <param name="a_rr"></param>
    /// <returns></returns>
    internal long GetSetupEnd(ResourceRequirement a_rr, out RequiredSpanPlusSetup o_setupSpan)
    {
        if (!a_rr.ContainsUsage(MainResourceDefs.usageEnum.Setup))
        {
            o_setupSpan = RequiredSpanPlusSetup.s_notInit;
            return GetUsageStart(a_rr);
        }

        o_setupSpan = m_requiredSetupSpan;
        return m_setupFinishDate;
    }

    /// <summary>
    /// Get non primary Processing end date based on its usage
    /// </summary>
    /// <param name="a_rr"></param>
    /// <returns></returns>
    internal long GetProcessingEnd(ResourceRequirement a_rr, out RequiredSpan o_processingSpan)
    {
        if (!a_rr.ContainsUsage(MainResourceDefs.usageEnum.Run))
        {
            o_processingSpan = new RequiredSpan(0, false);
            return GetUsageStart(a_rr);
        }

        o_processingSpan = m_requiredProcessingSpan;
        return m_productionFinishDate;
    }

    /// <summary>
    /// Get non primary Post-Processing End date based on its usage
    /// </summary>
    /// <param name="a_rr"></param>
    /// <returns></returns>
    internal long GetPostProcessingEnd(ResourceRequirement a_rr, out RequiredSpan o_postProcessingSpan)
    {
        if (!a_rr.ContainsUsage(MainResourceDefs.usageEnum.PostProcessing))
        {
            o_postProcessingSpan = new RequiredSpan(0, false);
            return GetUsageStart(a_rr);
        }

        o_postProcessingSpan = m_requiredPostProcessingSpan;
        return m_productionFinishDate;
    }

    /// <summary>
    /// Get non primary Post-Processing End date based on its usage
    /// </summary>
    /// <param name="a_rr"></param>
    /// <returns></returns>
    internal long GetCleanEnd(ResourceRequirement a_rr, out RequiredSpanPlusClean o_cleanSpan)
    {
        if (!a_rr.ContainsUsage(MainResourceDefs.usageEnum.Clean))
        {
            o_cleanSpan = RequiredSpanPlusClean.s_notInit;
            return GetUsageStart(a_rr);
        }

        o_cleanSpan = m_requiredCleanAfterSpan;
        return m_cleanFinishDate;
    }

    /// <summary>
    /// Get non primary Post-Processing End date based on its usage
    /// </summary>
    /// <param name="a_rr"></param>
    /// <returns></returns>
    internal long GetStorageEnd(ResourceRequirement a_rr, out RequiredSpan o_storageSpan)
    {
        if (!a_rr.ContainsUsage(MainResourceDefs.usageEnum.Storage))
        {
            o_storageSpan = RequiredSpan.NotInit;
            return GetUsageStart(a_rr);
        }

        o_storageSpan = m_requiredStorageSpan;
        return m_storageFinishDate;
    }

    /// <summary>
    /// Returns a new SI based on the primaries SI. This SI will only include the times required by the RR's Usages
    /// This can be used to calculate capacity and reservations for helper resources.
    /// This is important when determining when a helper will start in the future if it doesn't share the same usages as the primary
    /// </summary>
    /// <param name="a_rr">The Resource Requirement to create the SI for</param>
    /// <param name="a_nonPrimaryResource">The RR's Resource to set in the SI</param>
    /// <param name="a_storagePostProcessingTime">Storage post processing time to use if this RR needs StoragePostProcessing</param>
    /// <returns></returns>
    internal SchedulableInfo GetNonPrimarySchedulableInfo(ResourceRequirement a_rr, Resource a_nonPrimaryResource)
    {
        long setupEnd = GetSetupEnd(a_rr, out RequiredSpanPlusSetup setupSpan);
        long processingEnd = GetProcessingEnd(a_rr, out RequiredSpan processingSpan);
        long postProcessingEnd = GetPostProcessingEnd(a_rr, out RequiredSpan postProcessingSpan);
        long storageEnd = GetStorageEnd(a_rr, out RequiredSpan storageSpan);
        long cleanEnd = GetCleanEnd(a_rr, out RequiredSpanPlusClean cleanAfterSpan);
        bool zeroLength = setupSpan.GrossSetupTicks.TimeSpanTicks == 0 && processingSpan.TimeSpanTicks == 0 && postProcessingSpan.TimeSpanTicks == 0 && cleanAfterSpan.TimeSpanTicks == 0 && storageSpan.TimeSpanTicks == 0 && !a_rr.ContainsUsage(MainResourceDefs.usageEnum.Storage);

        // Note some of the usages end at the same time.
        return new SchedulableInfo(zeroLength,
            GetUsageStart(a_rr),
            setupEnd,
            processingEnd,
            postProcessingEnd,
            storageEnd,
            cleanEnd,
            GetUsageEnd(a_rr),
            a_nonPrimaryResource,
            RequiredSpanPlusClean.s_notInit, //TODO: How to handle this for helpers?
            setupSpan,
            processingSpan,
            postProcessingSpan,
            storageSpan,
            cleanAfterSpan,
            m_requiredNumberOfCycles,
            m_requiredQty,
            m_ocp);
    }

    public RequiredCapacity CreateRequiredCapacity()
    {
        return new RequiredCapacity(m_requiredAdditionalCleanBeforeSpan, m_requiredSetupSpan, m_requiredProcessingSpan, m_requiredPostProcessingSpan, m_requiredStorageSpan, m_requiredCleanAfterSpan);
    }
}