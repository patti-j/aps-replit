using PT.APSCommon;

namespace PT.Scheduler;

public partial class ResourceBlock
{
    #region Construction
    /// <summary>
    /// </summary>
    /// <param name="a_id">The id to use for the block.</param>
    /// <param name="a_batch">If it's part of a batch pass that in.</param>
    /// <param name="a_rr">Resource Requirement scheduled</param>
    /// <param name="a_res">The resource being scheduled.</param>
    /// <param name="a_si"></param>
    /// <param name="a_op">Activity scheduled</param>
    /// <param name="a_scheduledStartTicks"></param>
    /// <param name="a_scheduledFinishTicks"></param>
    internal ResourceBlock(BaseId a_id, Batch a_batch, ResourceRequirement a_rr, Resource a_res, OperationCapacityProfile a_capacityProfile)
        : base(a_id, a_rr)
    {
        // [BATCH_CODE]
        SetBatch(a_batch);

        ScheduledResource = a_res;
        m_capacityProfile = a_capacityProfile;
    }
    #endregion

    /// <summary>
    /// Depending on how the parameter is setup, whether the block is in the resources frozen or stable span.
    /// </summary>
    /// <param name="a_spanCalc">Must be configured for either OptimizeSettings.startTimes.EndOfFrozenZone or OptimizeSettings.startTimes.EndOfStableZone</param>
    /// <returns></returns>
    internal bool InSpan(SimulationTimePoint a_spanCalc)
    {
        long endOfSpan = a_spanCalc.GetTimeForResource(ScheduledResource);
        return StartTicks < endOfSpan;
    }
}