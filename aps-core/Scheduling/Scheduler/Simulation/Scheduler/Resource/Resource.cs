using System.Collections;
using System.Text;

using PT.APSCommon;
using PT.Common.Collections;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.PTMath;
using PT.Scheduler.Schedule.Operation;
using PT.Scheduler.Schedule.Resource;
using PT.Scheduler.Schedule.Resource.LookupTables;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Customizations;
using PT.Scheduler.Simulation.Events;
using PT.Scheduler.Simulation.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public partial class Resource
{
    /// <summary>
    /// This function should only be called by the scheduled block associated with this resource.
    /// </summary>
    /// <param name="node"></param>
    internal void Unschedule(ResourceBlockList.Node a_node)
    {
        m_blocks.Remove(a_node);
        if (LastScheduledBlockListNode == a_node)
        {
            // The last scheduled block last node was removed. The new last scheduled block list node is the
            // one before the one just removed.
            LastScheduledBlockListNode = LastScheduledBlockListNode.Previous;
        }
    }

    private bool TimeInBlock(long a_time, ResourceBlockList.Node a_node)
    {
        return a_time >= a_node.Data.StartTicks && a_time < a_node.Data.EndTicks;
    }

    private bool TimePastBlock(long a_time, ResourceBlockList.Node a_node)
    {
        return a_time >= a_node.Data.EndTicks;
    }

    private ResourceBlockList.Node m_lastScheduledBlockListNode;

    internal ResourceBlockList.Node LastScheduledBlockListNode
    {
        get => m_lastScheduledBlockListNode;
        set => m_lastScheduledBlockListNode = value;
    }

    private SchedulableResult PrimarySchedulableHelper(ScenarioDetail a_sd,
                                                       long a_simClock,
                                                       InternalActivity a_act,
                                                       bool a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable,
                                                       ResourceRequirement a_rr,
                                                       long a_startDate,
                                                       LeftNeighborSequenceValues a_leftNeighborVals,
                                                       RequiredCapacityPlus a_requiredCapacity, //Overwritten if left neighbor node is set
                                                       LinkedListNode<ResourceCapacityInterval> a_startingInterval,
                                                       bool a_allowSpanCantSpanRci,
                                                       bool a_late,
                                                       out bool o_willSpanLateOnlyRci)
    {
        o_willSpanLateOnlyRci = false;
        bool zeroLength = a_requiredCapacity.TotalRequiredCapacity() == 0;
        string capacityGroupCode = a_act.Operation.ResourceRequirements.PrimaryResourceRequirement.CapacityCode;

        OperationCapacityProfile ocp = new();

        if (a_requiredCapacity.SetupSpan.Overrun)
        {
            a_requiredCapacity.SetupSpan = RequiredSpanPlusSetup.s_overrun;
        }

        long setupFinishDate;
        if (a_requiredCapacity.SetupSpan.TimeSpanTicks > 0)
        {
            FindCapacityResult setupResult = FindCapacity(a_startDate, a_requiredCapacity.SetupSpan.TimeSpanTicks, a_act.Operation.CanPause, a_startingInterval, MainResourceDefs.usageEnum.Setup, true, a_late, capacityGroupCode, a_allowSpanCantSpanRci, a_act.PeopleUsage, a_act.NbrOfPeople, out o_willSpanLateOnlyRci);

            if (setupResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
            {
                return new SchedulableResult(setupResult, ocp);
            }

            ocp.Add(setupResult.CapacityUsageProfile, MainResourceDefs.usageEnum.Setup);

            if (!a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable)
            {
                setupResult = ValidateCapacityForReservationsAndAttention(a_sd, a_simClock, a_startDate, setupResult, a_startingInterval, a_act, a_rr);
                if (setupResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
                {
                    return new SchedulableResult(setupResult, ocp);
                }
            }

            setupFinishDate = setupResult.FinishDate;
        }
        else
        {
            setupFinishDate = a_startDate;
        }

        long processingFinishDate;

        if (a_requiredCapacity.ProcessingSpan.Overrun)
        {
            a_requiredCapacity.ProcessingSpan = new RequiredSpan(RequiredSpan.OverrunRequiredSpan);
        }

        if (a_requiredCapacity.ProcessingSpan.TimeSpanTicks > 0)
        {
            FindCapacityResult processingResult = FindCapacity(setupFinishDate, a_requiredCapacity.ProcessingSpan.TimeSpanTicks, a_act.Operation.CanPause, a_startingInterval, MainResourceDefs.usageEnum.Run, a_requiredCapacity.SetupSpan.TimeSpanTicks == 0, a_late, capacityGroupCode, a_allowSpanCantSpanRci, a_act.PeopleUsage, a_act.NbrOfPeople, out o_willSpanLateOnlyRci);

            ocp.Add(processingResult.CapacityUsageProfile, MainResourceDefs.usageEnum.Run);

            if (processingResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
            {
                return new SchedulableResult(processingResult, ocp);
            }

            if (!a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable)
            {
                processingResult = ValidateCapacityForReservationsAndAttention(a_sd, a_simClock, setupFinishDate, processingResult, a_startingInterval, a_act, a_rr);
                if (processingResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
                {
                    return new SchedulableResult(processingResult, ocp);
                }
            }

            processingFinishDate = processingResult.FinishDate;
        }
        else
        {
            processingFinishDate = setupFinishDate;
        }

        long postProcessingFinishDate;
        if (a_requiredCapacity.PostProcessingSpan.Overrun)
        {
            a_requiredCapacity.PostProcessingSpan = new RequiredSpan(RequiredSpan.OverrunRequiredSpan);
        }

        if (a_requiredCapacity.PostProcessingSpan.TimeSpanTicks > 0)
        {
            FindCapacityResult postProcessingResult = FindCapacity(processingFinishDate, a_requiredCapacity.PostProcessingSpan.TimeSpanTicks, a_act.Operation.CanPause, a_startingInterval, MainResourceDefs.usageEnum.PostProcessing, a_requiredCapacity.ProcessingSpan.TimeSpanTicks == 0, a_late, capacityGroupCode, a_allowSpanCantSpanRci, a_act.PeopleUsage, a_act.NbrOfPeople, out o_willSpanLateOnlyRci);

            ocp.Add(postProcessingResult.CapacityUsageProfile, MainResourceDefs.usageEnum.PostProcessing);

            if (postProcessingResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
            {
                return new SchedulableResult(postProcessingResult, ocp);
            }

            if (!a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable)
            {
                postProcessingResult = ValidateCapacityForReservationsAndAttention(a_sd, a_simClock, processingFinishDate, postProcessingResult, a_startingInterval, a_act, a_rr);
                if (postProcessingResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
                {
                    return new SchedulableResult(postProcessingResult, ocp);
                }
            }

            postProcessingFinishDate = postProcessingResult.FinishDate;
        }
        else
        {
            postProcessingFinishDate = processingFinishDate;
        }

        long storageFinishDate;
        if (a_requiredCapacity.StorageSpan.Overrun)
        {
            a_requiredCapacity.StorageSpan = new RequiredSpan(RequiredSpan.OverrunRequiredSpan);
        }

        if (a_requiredCapacity.StorageSpan.TimeSpanTicks > 0)
        {
            FindCapacityResult storageResult = FindCapacity(postProcessingFinishDate, a_requiredCapacity.StorageSpan.TimeSpanTicks, a_act.Operation.CanPause, a_startingInterval, MainResourceDefs.usageEnum.Storage, a_requiredCapacity.StorageSpan.TimeSpanTicks == 0, a_late, capacityGroupCode, a_allowSpanCantSpanRci, a_act.PeopleUsage, a_act.NbrOfPeople, out o_willSpanLateOnlyRci);

            ocp.Add(storageResult.CapacityUsageProfile, MainResourceDefs.usageEnum.Storage);

            if (storageResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
            {
                return new SchedulableResult(storageResult, ocp);
            }

            if (!a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable)
            {
                storageResult = ValidateCapacityForReservationsAndAttention(a_sd, a_simClock, postProcessingFinishDate, storageResult, a_startingInterval, a_act, a_rr);
                if (storageResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
                {
                    return new SchedulableResult(storageResult, ocp);
                }
            }

            storageFinishDate = storageResult.FinishDate;
        }
        else
        {
            storageFinishDate = postProcessingFinishDate;
        }

        long cleanFinishDate;
        if (a_requiredCapacity.CleanAfterSpan.Overrun)
        {
            a_requiredCapacity.CleanAfterSpan = RequiredSpanPlusClean.s_overrun;
        }

        if (a_requiredCapacity.CleanAfterSpan.TimeSpanTicks > 0)
        {
            FindCapacityResult cleanResult = FindCapacity(storageFinishDate, a_requiredCapacity.CleanAfterSpan.TimeSpanTicks, a_act.Operation.CanPause, a_startingInterval, MainResourceDefs.usageEnum.Clean, false, a_late, capacityGroupCode, false, a_act.PeopleUsage, a_act.NbrOfPeople, out o_willSpanLateOnlyRci);

            ocp.Add(cleanResult.CapacityUsageProfile, MainResourceDefs.usageEnum.Clean);

            if (cleanResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
            {
                if (cleanResult.ResultStatus == SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan)
                {
                    return new SchedulableResult(cleanResult.NextStartTime);
                }

                return new SchedulableResult(cleanResult, ocp);
            }

            if (!a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable)
            {
                cleanResult = ValidateCapacityForReservationsAndAttention(a_sd, a_simClock, storageFinishDate, cleanResult, a_startingInterval, a_act, a_rr, false);
                if (cleanResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
                {
                    return new SchedulableResult(cleanResult, ocp);
                }
            }
        }

        //TODO: This sets the block end to the previous block's end. Why?
        if (CapacityType == InternalResourceDefs.capacityTypes.Infinite && Sequential)
        {
            if (Blocks.Last != null)
            {
                ResourceBlock block = Blocks.Last.Data;
                if (block.EndTicks > storageFinishDate)
                {
                    storageFinishDate = block.EndTicks;
                }
            }
        }

        if (IntersectsReservations(a_simClock, a_startDate, storageFinishDate, a_act, out long resEndDateTicks))
        {
            SchedulableResult sr = new(SchedulableSuccessFailureEnum.IntersectsReservedMoveDate, ocp, resEndDateTicks);
            return sr;
        }

        SchedulableInfo si = new(zeroLength,
            a_startDate,
            setupFinishDate,
            processingFinishDate,
            postProcessingFinishDate,
            storageFinishDate,
            storageFinishDate,
            storageFinishDate,
            this,
            a_requiredCapacity.CleanBeforeSpan,
            a_requiredCapacity.SetupSpan,
            a_requiredCapacity.ProcessingSpan,
            a_requiredCapacity.PostProcessingSpan,
            a_requiredCapacity.StorageSpan,
            RequiredSpanPlusClean.s_notInit, //We don't schedule the after span. We just use the required capacity to verify it could schedule. The actual clean will be scheduled by the following block or planning horizon
            a_requiredCapacity.RequiredNumberOfCycles,
            a_requiredCapacity.RequiredQty,
            ocp);

        SchedulableResult cleanoutResult = ValidateCleanBeforeSpan(a_sd, a_simClock, a_act, a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable, a_rr, a_startDate, a_leftNeighborVals, a_requiredCapacity.CleanBeforeSpan, a_startingInterval, a_allowSpanCantSpanRci, a_late, capacityGroupCode, ocp);
        if (cleanoutResult.m_result != SchedulableSuccessFailureEnum.Success)
        {
            if (cleanoutResult.m_result == SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan)
            {
                //Returning what we need for a success result, but tracking that it actually failed due to clean before span capacity
                cleanoutResult.m_si = si;
            }

            return cleanoutResult;
        }

        return new SchedulableResult(si, ocp);
    }

    /// <summary>
    /// Determine whether the activity's Primary Resource Requirement can be scheduled. If so the return value will
    /// specify the end of setup time, the end of processing time, etc.
    /// </summary>
    internal SchedulableResult PrimarySchedulable(ScenarioDetail a_sd,
                                                  long a_simClock,
                                                  InternalActivity a_act,
                                                  bool a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable,
                                                  ResourceRequirement a_rr,
                                                  long a_startDate,
                                                  LeftNeighborSequenceValues a_leftNeighborVals,
                                                  RequiredCapacityPlus a_requiredCapacity, //Overwritten if left neighbor node is set
                                                  LinkedListNode<ResourceCapacityInterval> a_startingInterval,
                                                  bool a_allowSpanCantSpanRci)
    {
        ActivityResourceBufferInfo bufferInfo = a_act.GetBufferResourceInfo(Id);
        SchedulableResult schedulableResult = PrimarySchedulableHelper(a_sd, a_simClock, a_act, a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable, a_rr, a_startDate, a_leftNeighborVals, a_requiredCapacity, a_startingInterval, a_allowSpanCantSpanRci, false, out bool willSpanLateOnlyRci);
        if (schedulableResult.m_result == SchedulableSuccessFailureEnum.Success)
        {
            //Now that we have an end date, we need to verify that the capacity found is valid if the activity will be late and the capacity found spans a UseOnlyWhenLate interval
            if (willSpanLateOnlyRci && schedulableResult.m_si.m_storageFinishDate > bufferInfo.BufferEndDate)
            {
                schedulableResult = PrimarySchedulableHelper(a_sd, a_simClock, a_act, a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable, a_rr, a_startDate, a_leftNeighborVals, a_requiredCapacity, a_startingInterval, a_allowSpanCantSpanRci, true, out willSpanLateOnlyRci);
            }
        }

        return schedulableResult;
    }

    internal SchedulableResult ValidateCleanBeforeSpan(ScenarioDetail a_sd,
                                                       long a_simClock,
                                                       InternalActivity a_act,
                                                       bool a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable,
                                                       ResourceRequirement a_rr,
                                                       long a_startDate,
                                                       LeftNeighborSequenceValues a_leftNeighbor,
                                                       RequiredSpanPlusClean a_requiredCapacity,
                                                       LinkedListNode<ResourceCapacityInterval> a_startingInterval,
                                                       bool a_allowSpanCantSpanRci,
                                                       bool a_late,
                                                       string a_capacityGroupCode,
                                                       OperationCapacityProfile a_ocp)
    {
        SchedulableResult result = new SchedulableResult(SchedulableSuccessFailureEnum.Success, a_ocp);

        if (!a_leftNeighbor.Initialized || a_requiredCapacity.TimeSpanTicks <= 0)
        {
            return result;
        }

        //We need to verify there is enough capacity to run this CIP after the previous block
        long previousBatchCleanStartDate = a_sd.Clock;
        bool previousScheduledBatchCanPause = false;
        if (a_leftNeighbor.Activity.Batch != null)
        {
            //Scheduled block
            previousBatchCleanStartDate = a_leftNeighbor.Activity.Batch.EndOfStorageTicks;
        }
        else
        {
            //Actual, use the date closest to the clean start
            if (a_leftNeighbor.Activity.ReportedEndOfStorageSet)
            {
                previousBatchCleanStartDate = a_leftNeighbor.Activity.ReportedEndOfStorageTicks;
            }
            else if (a_leftNeighbor.Activity.ReportedEndOfPostProcessingSet)
            {
                previousBatchCleanStartDate = a_leftNeighbor.Activity.ReportedEndOfPostProcessingTicks;
            }
            else if (a_leftNeighbor.Activity.ReportedFinishDateSet)
            {
                previousBatchCleanStartDate = a_leftNeighbor.Activity.ReportedFinishDateTicks;
            }
        }

        previousScheduledBatchCanPause = a_leftNeighbor.Activity.Operation.CanPause;

        //We need to find the interval that the last block is scheduled on, it may not be the same one we are scheduling on now
        LinkedListNode<ResourceCapacityInterval> previousBatchNode = ResourceCapacityIntervalList.FindFirstOnlineBackwardsContainingPoint(a_startingInterval, previousBatchCleanStartDate, true);
        
        if (previousBatchNode != null)
        {
            //Start at end of post-processing, the duration of capacity needed is the merged clean span
            FindCapacityResult capacityResult = FindCapacity(previousBatchCleanStartDate,
                a_requiredCapacity.TimeSpanTicks,
                previousScheduledBatchCanPause,
                previousBatchNode,
                MainResourceDefs.usageEnum.Clean,
                false,
                a_late,
                a_capacityGroupCode,
                a_allowSpanCantSpanRci,
                a_act.PeopleUsage,
                a_act.NbrOfPeople,
                out bool _);

            if (capacityResult.ResultStatus == SchedulableSuccessFailureEnum.Success)
            {
                if (!a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable)
                {
                    capacityResult = ValidateCapacityForReservationsAndAttention(a_sd, a_simClock, previousBatchCleanStartDate, capacityResult, previousBatchNode, a_leftNeighbor.Activity, a_rr);
                    if (capacityResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
                    {
                        long retryTime = capacityResult.CapacityUsageProfile[^1].EndTicks;
                        if (retryTime > a_simClock)
                        {
                            //A time was found in the future, so return the retry time
                            return new SchedulableResult(SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan, new OperationCapacityProfile(), retryTime);
                        }

                        //It's possible to fail due to something other than a lack of capacity. So return fail without retry
                        return new SchedulableResult(capacityResult.ResultStatus, a_ocp);
                    }
                }

                //Found Capacity in the future, possibly due to a cleanout that was added.
                if (capacityResult.CapacityUsageProfile.Count > 0 && capacityResult.CapacityUsageProfile[^1].EndTicks > a_startDate)
                {
                    return new SchedulableResult(SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan, new OperationCapacityProfile(), capacityResult.CapacityUsageProfile[^1].EndTicks);
                }


                return new SchedulableResult(capacityResult, a_ocp);
            }
            else if (a_simClock < a_sd.EndOfPlanningHorizon)
            {
                return new SchedulableResult(capacityResult, a_ocp);
            }
            //Else, the cleanout doesn't fit, but let everything schedule past the planning horizon
        }
        else
        {
            #if DEBUG
            throw new DebugException("Error calculating merged Clean span. Previous capacity could not be found");
            #endif
        }

        return result;
     }

    /// <summary>
    /// Whether a timespan where you would like to schedule an activity intersects a ResourceReservation or scheduled block.
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_start">The start of the timespan needed.</param>
    /// <param name="a_end">The end of the timespan needed. </param>
    /// <param name="a_act">The activity the teimspan is for. </param>
    /// <param name="a_l"></param>
    /// <returns></returns>
    private bool IntersectsReservations(long a_simClock, long a_start, long a_end, InternalActivity a_act, out long o_resEndTicks)
    {
        o_resEndTicks = 0;

        if (a_act.BeingMoved || a_act.MoveIntoBatch)
        {
            return false;
        }

        //TODO: Possible check capacity here for multi-tasking? Otherwise maybe it can be checked during normal capacity calcs
        if (EnforceMaxDelay && CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            AVLTree<ResourceSpan, ResourceSpan>.Enumerator rrEtr = m_resourceSpans.GetEnumerator();
            while (rrEtr.MoveNext())
            {
                if (Interval.Intersection(a_start, a_end, rrEtr.Current.Value.Start, rrEtr.Current.Value.End))
                {
                    if (rrEtr.Current.Value.Reason is InternalActivity act)
                    {
                        o_resEndTicks = rrEtr.Current.Value.End;
                        return act != a_act;
                    }

                    if (rrEtr.Current.Value.Reason is ResourceBlock block && block.Batched)
                    {
                        o_resEndTicks = rrEtr.Current.Value.End;
                        return block.Batch.FirstActivity != a_act;
                        //TODO: Batching. What if there are multiple activities here?
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Calculate the SchedulableResult of a block that's already scheduled.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_startTicks"></param>
    /// <param name="a_rb"></param>
    /// <param name="a_timeCustomizer"></param>
    /// <param name="a_allowSpanCantSpanRci"></param>
    /// <returns></returns>
    internal SchedulableResult PrimarySchedulable(ScenarioDetail a_sd, long a_simClock, long a_startTicks, ResourceBlock a_rb, ExtensionController a_timeCustomizer, bool a_allowSpanCantSpanRci)
    {
        // [USAGE_CODE]
        Batch batch = a_rb.Batch;
        InternalActivity act = batch.GetFirstActivity();
        InternalOperation op = act.Operation;
        ResourceRequirement primaryRR = op.ResourceRequirements.PrimaryResourceRequirement;

        ResourceBlockList.Node node = Blocks.First; //Assume performance is better to start at the front
        ResourceBlockList.Node leftNode = null;

        //Check if the block is being moved to the front of the schedule
        if (node != null && node.Data.StartTicks > a_startTicks)
        {
            node = null;
        }

        if (a_rb.StartTicks < a_startTicks && a_rb.ScheduledResource == this)
        {
            //moving later we can start from here
            node = a_rb.MachineBlockListNode;
        }

        while (node != null)
        {
            if (node.Data.Batch.Id == a_rb.Batch.Id)
            {
                node = node.Next;
                continue;
            }

            //TODO: Take into account move settings like exact move
            if (node.Data.Batch.EndOfStorageTicks <= a_startTicks)
            {
                leftNode = node;
                node = node.Next;
            }
            else
            {
                break;
            }
        }

        LeftNeighborSequenceValues leftNeighborSequenceVals = CreateDefaultLeftNeighborSequenceValues(a_startTicks, leftNode);

        
        SchedulableResult sr = PrimarySchedulableHelper(a_sd, a_startTicks, a_startTicks, act, primaryRR, leftNeighborSequenceVals, a_timeCustomizer, a_allowSpanCantSpanRci);
        return sr;
    }

    /// <summary>
    /// Calculate the SchedulableResult of an activity that's already scheduled.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_startTicks"></param>
    /// <param name="a_act"></param>
    /// <returns></returns>
    internal SchedulableResult PrimarySchedulable(ScenarioDetail a_sd, long a_simClock, long a_startTicks, InternalActivity a_act, ExtensionController a_timeCustomizer, bool a_allowSpanCantSpanRci)
    {
        // [USAGE_CODE]
        ResourceRequirement primaryRR = a_act.Operation.ResourceRequirements.PrimaryResourceRequirement;
        ResourceBlock block = FindBlockBefore(a_startTicks, a_act.Batch);
        ResourceBlockList.Node blockNode = null;
        if (block != null)
        {
            blockNode = block.MachineBlockListNode;
        }

        LeftNeighborSequenceValues leftNeighborSequenceValues = CreateDefaultLeftNeighborSequenceValues(a_startTicks, blockNode);
        SchedulableResult sr = PrimarySchedulableHelper(a_sd, a_simClock, a_startTicks, a_act, primaryRR, leftNeighborSequenceValues, a_timeCustomizer, a_allowSpanCantSpanRci);
        return sr;
    }

    /// <summary>
    /// A helper to calculate the SchedulableResult of something that's already scheduled.
    /// A helper for the versions of PrimarySchedulable that calculate the SchedulableResource of an activitiy that's already scheduled.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_startTicks"></param>
    /// <param name="a_act"></param>
    /// <param name="a_primaryRR"></param>
    /// <param name="a_leftSequenceValues"></param>
    /// <param name="blockNode"></param>
    /// <param name="a_timeCustomizer"></param>
    /// <param name="a_allowSpanCantSpanRci"></param>
    private SchedulableResult PrimarySchedulableHelper(ScenarioDetail a_sd,
                                                       long a_simClock,
                                                       long a_startTicks,
                                                       InternalActivity a_act,
                                                       ResourceRequirement a_primaryRR,
                                                       LeftNeighborSequenceValues a_leftSequenceValues,
                                                       ExtensionController a_timeCustomizer,
                                                       bool a_allowSpanCantSpanRci)
    {
        LinkedListNode<ResourceCapacityInterval> intervalNode = FindContainingCapacityIntervalNode(a_startTicks, null);

        RequiredCapacityPlus rc = CalculateTotalRequiredCapacity(a_simClock, a_act, a_leftSequenceValues, true, a_startTicks, a_timeCustomizer);
        SchedulableResult sr = PrimarySchedulable(a_sd, a_simClock, a_act, true, a_primaryRR, a_startTicks, a_leftSequenceValues, rc, intervalNode, a_allowSpanCantSpanRci);
        return sr;
    }

    public (bool, SchedulableInfo) PrimarySchedulableExternal(ScenarioDetail a_sd, long a_startTicks, InternalActivity a_act, RequiredCapacityPlus a_requiredCapacity, bool a_allowSpanCantSpanRci)
    {
        LinkedListNode<ResourceCapacityInterval> intervalNode = FindContainingCapacityIntervalNode(a_startTicks, null);

        ResourceBlock block = FindBlockBefore(a_startTicks, a_act.Batch);
        LeftNeighborSequenceValues leftNeighbor = CreateDefaultLeftNeighborSequenceValues(a_startTicks, block?.MachineBlockListNode);
        
        SchedulableResult sr = PrimarySchedulable(a_sd, a_sd.SimClock, a_act, true, a_act.Operation.ResourceRequirements.PrimaryResourceRequirement, a_startTicks, leftNeighbor, a_requiredCapacity, intervalNode, a_allowSpanCantSpanRci);
        return (sr.m_result == SchedulableSuccessFailureEnum.Success, sr.m_si);
    }

    public long CalculateLastCycleStartTime(InternalActivity a_activity, SchedulableInfo a_si)
    {
        if (a_si.m_requiredNumberOfCycles > 1)
        {
            //The amount of capacity needed for the next cycle.
            ProductionInfo resourceProductionInfo = a_activity.GetResourceProductionInfo(this);
            long neededCapacity = resourceProductionInfo.CycleSpanTicks;
            return Math.Max(a_si.m_setupFinishDate, a_si.m_productionFinishDate - neededCapacity);
        }

        return a_si.m_setupFinishDate;
    }
    
    /// <summary>
    /// Determine the CycleCompltion of each cycle.
    /// </summary>
    /// <param name="a_activity">The activity to calculate the CycleCompletionProfile for.</param>
    /// <param name="a_si">The SchedulableInfo for the activity.</param>
    /// <param name="o_ccp">A set that will contain all the CycleCompletions.</param>
    public void CalculateCycleCompletionTimes(InternalActivity a_activity, SchedulableInfo a_si, out CycleAdjustmentProfile o_ccp)
    {
        if (a_si.m_requiredNumberOfCycles > 0 && a_si.m_ocp != null)
        {
            o_ccp = new CycleAdjustmentProfile();
            ResourceOperation operation = (ResourceOperation)a_activity.Operation;

            // The number of cycles that have been found so far.
            int cycles = 0;

            //The amount of capacity needed for the next cycle.
            ProductionInfo resourceProductionInfo = a_activity.GetResourceProductionInfo(this);
            long neededCapacity = resourceProductionInfo.CycleSpanTicks;

            //Check if the mod of the required capacity by number of cycles needed  and the calculated required processing span gives a remainder
            //this would happen if auto-reporting caused a partially finished cycle. If so adjust the needed capacity for the first cycle using the
            //remainder so that the cycle completion profile can be created accurately
            long leftoverCapacity = (neededCapacity * a_si.m_requiredNumberOfCycles) % a_si.m_requiredProcessingSpan.TimeSpanTicks;
            if (leftoverCapacity > 0)
            {
                //we have a partial cycle due to TimeBasedReporting
                neededCapacity = leftoverCapacity;
            }

            //Add a cycle completion object for the Reported good qty at the start date to allow transfer qty to 
            //still work and not push consumers out.
            //if (a_activity.ReportedGoodQty > 0)
            //{
            //    o_ccp.Add(new CycleAdjustment(cycles, a_si.m_scheduledStartDate, a_activity.ReportedGoodQty));
            //}

            // The amount of capacity that is left is translated to ticks so the end date of this cycle
            // can be determined. 
            decimal timespanToCapacityRatio = 0;

            for (int ocpI = 0; ocpI < a_si.m_ocp.ProductionProfile.Count; ++ocpI)
            {
                OperationCapacity oc = a_si.m_ocp.ProductionProfile[ocpI];

                // The time span of this OperationCapacity.
                long ocTimeSpan = oc.EndTicks - oc.StartTicks;

                if (ocTimeSpan != oc.TotalCapacityTicks)
                {
                    // Even though this variable is only used at one point in the code below,
                    // it is calculated here to prevent having to needlessly recalculate its value.
                    timespanToCapacityRatio = ocTimeSpan / (decimal)oc.TotalCapacityTicks;
                }

                // The amount of capacity still available within the current interval.
                // Initally set to the amount of capacity in the OperationCapacity.
                decimal availableCapacity = oc.TotalCapacityTicks;

                // Consume all of the capacity to cycles. The cycles occur in sequence. Unless there are precision errors, all the capacity
                // from all the OperationCapacities will be perfectly consumed by the right number of cycles. You'll notice that "decimal" is 
                // being used below. It was picked over "decimal" because some simple "decimal" calculations were resulting in slightly
                // incorrect values in some situations.

                while (availableCapacity > 0)
                {
                    decimal neededFractionOfCapacity = neededCapacity / availableCapacity;

                    if (neededFractionOfCapacity < 1)
                    {
                        // There's more capacity available than what's needed.
                        availableCapacity -= neededCapacity;
                        ++cycles;

                        long endDate;
                        if (ocTimeSpan == oc.TotalCapacityTicks)
                        {
                            // The "decimal" (availableCapacity) below needed to be converted to a long before performing the 
                            // subtraction or else it is possible for a "decimal" precision error to get into the result.
                            endDate = oc.EndTicks - (long)availableCapacity;
                        }
                        else
                        {
                            endDate = (long)(oc.EndTicks - availableCapacity * timespanToCapacityRatio);
                        }

                        o_ccp.Add(new CycleAdjustment(cycles, endDate, resourceProductionInfo.QtyPerCycle));
                        neededCapacity = resourceProductionInfo.CycleSpanTicks;
                    }
                    else if (neededFractionOfCapacity == 1)
                    {
                        // The capacity available is exactly what's needed.
                        ++cycles;
                        o_ccp.Add(new CycleAdjustment(cycles, oc.EndTicks, resourceProductionInfo.QtyPerCycle));
                        neededCapacity = resourceProductionInfo.CycleSpanTicks;
                        break;
                    }
                    else
                    {
                        // There's less capacity available than what's needed.
                        // Here all we end up doing is reducing the needed capacity.
                        // The next capacity further up in the loop will provide additional capacity. 
                        neededCapacity = (long)(neededCapacity - availableCapacity);

                        // This is workaround code for any "decimal" precision errors.
                        // I've seen these occur when calculating the capacity of an interval, resulting in operations not scheduling.
                        if (ocpI == a_si.m_ocp.ProductionProfile.Count - 1 && neededFractionOfCapacity < 1.05m)
                        {
                            ++cycles;
                            o_ccp.Add(new CycleAdjustment(cycles, oc.EndTicks, resourceProductionInfo.QtyPerCycle));
                        }

                        break;
                    }
                }
            }

            decimal totalCCQty = o_ccp.Count * resourceProductionInfo.QtyPerCycle;
            decimal excess = 0;
            if (totalCCQty > 0 && a_si.m_requiredQty > 0)
            {
                excess = totalCCQty % a_si.m_requiredQty;
            }

            if (excess > 0)
            {
                if (excess < resourceProductionInfo.QtyPerCycle)
                {
                    CycleAdjustment cc = o_ccp[^1];
                    cc.Qty -= excess;
                }
            }
        }
        else
        {
            o_ccp = null;
        }
    }

    /// <summary>
    /// Determine the CycleCompltion of each cycle.
    /// </summary>
    /// <param name="a_activity">The activity to calculate the CycleCompletionProfile for.</param>
    /// <param name="a_si">The SchedulableInfo for the activity.</param>
    /// <param name="o_ccp">A set that will contain all the CycleCompletions.</param>
    public void CalculateCycleStartTimes(InternalActivity a_activity, SchedulableInfo a_si, out CycleAdjustmentProfile o_ccp)
    {
        if (a_si.m_requiredNumberOfCycles > 0 && a_si.m_ocp != null)
        {
            o_ccp = new CycleAdjustmentProfile();

            // The number of cycles that have been found so far.
            int cycles = 0;

            //The amount of capacity needed for the next cycle.
            ProductionInfo resourceProductionInfo = a_activity.GetResourceProductionInfo(this);
            long neededCapacity = resourceProductionInfo.CycleSpanTicks;

            //Check if the mod of the required capacity by number of cycles needed  and the calculated required processing span gives a remainder
            //this would happen if auto-reporting caused a partially finished cycle. If so adjust the needed capacity for the first cycle using the
            //remainder so that the cycle completion profile can be created accurately
            long leftoverCapacity = (neededCapacity * a_si.m_requiredNumberOfCycles) % a_si.m_requiredProcessingSpan.TimeSpanTicks;
            if (leftoverCapacity > 0)
            {
                //we have a partial cycle due to TimeBasedReporting
                neededCapacity = leftoverCapacity;
            }

            //Add an initial start cycle
            o_ccp.Add(new CycleAdjustment(cycles, a_si.m_setupFinishDate, 0m));

            // The amount of capacity that is left is translated to ticks so the end date of this cycle
            // can be determined. 
            decimal timespanToCapacityRatio = 0;

            for (int ocpI = 0; ocpI < a_si.m_ocp.ProductionProfile.Count; ++ocpI)
            {
                OperationCapacity oc = a_si.m_ocp.ProductionProfile[ocpI];

                // The time span of this OperationCapacity.
                long ocTimeSpan = oc.EndTicks - oc.StartTicks;

                if (ocTimeSpan != oc.TotalCapacityTicks)
                {
                    // Even though this variable is only used at one point in the code below,
                    // it is calculated here to prevent having to needlessly recalculate its value.
                    timespanToCapacityRatio = ocTimeSpan / (decimal)oc.TotalCapacityTicks;
                }

                // The amount of capacity still available within the current interval.
                // Initally set to the amount of capacity in the OperationCapacity.
                decimal availableCapacity = oc.TotalCapacityTicks;

                // Consume all of the capacity to cycles. The cycles occur in sequence. Unless there are precision errors, all the capacity
                // from all the OperationCapacities will be perfectly consumed by the right number of cycles. You'll notice that "decimal" is 
                // being used below. It was picked over "decimal" because some simple "decimal" calculations were resulting in slightly
                // incorrect values in some situations.

                while (availableCapacity > 0)
                {
                    decimal neededFractionOfCapacity = neededCapacity / availableCapacity;

                    if (neededFractionOfCapacity < 1)
                    {
                        // There's more capacity available than what's needed.
                        availableCapacity -= neededCapacity;
                        ++cycles;

                        long endDate;
                        if (ocTimeSpan == oc.TotalCapacityTicks)
                        {
                            // The "decimal" (availableCapacity) below needed to be converted to a long before performing the 
                            // subtraction or else it is possible for a "decimal" precision error to get into the result.
                            endDate = oc.EndTicks - (long)availableCapacity;
                        }
                        else
                        {
                            endDate = (long)(oc.EndTicks - availableCapacity * timespanToCapacityRatio);
                        }

                        o_ccp.Add(new CycleAdjustment(cycles, endDate, 0m));
                        neededCapacity = resourceProductionInfo.CycleSpanTicks;
                    }
                    else if (neededFractionOfCapacity == 1)
                    {
                        // The capacity available is exactly what's needed.
                        ++cycles;
                        o_ccp.Add(new CycleAdjustment(cycles, oc.EndTicks, 0m));
                        neededCapacity = resourceProductionInfo.CycleSpanTicks;
                        break;
                    }
                    else
                    {
                        // There's less capacity available than what's needed.
                        // Here all we end up doing is reducing the needed capacity.
                        // The next capacity further up in the loop will provide additional capacity. 
                        neededCapacity = (long)(neededCapacity - availableCapacity);

                        // This is workaround code for any "decimal" precision errors.
                        // I've seen these occur when calculating the capacity of an interval, resulting in operations not scheduling.
                        if (ocpI == a_si.m_ocp.ProductionProfile.Count - 1 && neededFractionOfCapacity < 1.05m)
                        {
                            ++cycles;
                            o_ccp.Add(new CycleAdjustment(cycles, oc.EndTicks, resourceProductionInfo.QtyPerCycle));
                        }

                        break;
                    }
                }
            }

            if (o_ccp.Count > 1)
            {
                //Remove the final cycle end, since we only care about cycle starts.
                o_ccp.RemoveAt(o_ccp.Count - 1);
            }
        }
        else
        {
            o_ccp = null;
        }
    }

    private FindCapacityResult ValidateCapacityForReservationsAndAttention(ScenarioDetail a_sd, long a_simClock, long a_startTicks, FindCapacityResult a_capacityResult, LinkedListNode<ResourceCapacityInterval> a_after, InternalActivity a_act, ResourceRequirement a_rr, bool a_checkMoveReservations = true)
    {
        LeftNeighborSequenceValues sequenceValues;
        if (a_checkMoveReservations && IntersectsReservedMoveTime(a_sd, a_act, a_startTicks, a_capacityResult.FinishDate, out sequenceValues))
        {
            a_capacityResult.SetLeftSequenceValues(sequenceValues);
            a_capacityResult.ResultStatus = SchedulableSuccessFailureEnum.IntersectsReservedMoveDate;
            return a_capacityResult;
        }

        //Check for reserved blocks. Any conflicts on multi-tasking will be handled in the attention percent calcs
        if (CapacityType == InternalResourceDefs.capacityTypes.SingleTasking && IntersectsBlockReservation(a_act, a_startTicks, a_capacityResult.FinishDate, out sequenceValues))
        {
            a_capacityResult.SetLeftSequenceValues(sequenceValues);
            a_capacityResult.ResultStatus = SchedulableSuccessFailureEnum.IntersectsBlockReservation;
            return a_capacityResult;
        }

        if (CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
        {
            LinkedListNode<ResourceCapacityInterval> startNode = a_after;
            if (AvailableInSimulation != null)
            {
                //when validating incurring clean on previous blocks, we want to be able to use the rci node that was passed in as a parameter which 
                //can be earlier than the available in simulation node. 
                if (AvailableInSimulation.Node.Value.StartDate < a_after.Value.StartDate)
                {
                    startNode = AvailableInSimulation.Node;
                }
            }
         
            ResourceCapacityInterval.AttnAvailResult attnAvail = AttentionAvailable(a_act, a_startTicks, a_capacityResult.FinishDate, a_rr, null, null, startNode);
            if (attnAvail.SuccessResult != SchedulableSuccessFailureEnum.Success)
            {
                a_capacityResult.ResultStatus = attnAvail.SuccessResult;
                a_capacityResult.NextStartTime = attnAvail.AvailableTicks;
                return a_capacityResult;
            }
        }

        return a_capacityResult;
    }

    /// <summary>
    /// Specifies whether a helper resource can be schedule to match the primary.
    /// </summary>
    internal struct NonPrimarySchedulableResult
    {
        internal NonPrimarySchedulableResult(ResourceRequirement a_rr, SchedulableSuccessFailureEnum a_avail, long a_nextAvailTicks)
        {
            RR = a_rr;
            Availability = a_avail;
            m_tryAgainTicks = a_nextAvailTicks;
            Constructed = true;
        }

        internal NonPrimarySchedulableResult(ResourceRequirement a_rr, SchedulableSuccessFailureEnum a_avail)
        {
            RR = a_rr;
            Availability = a_avail;
            Constructed = true;
        }

        internal NonPrimarySchedulableResult(SchedulableSuccessFailureEnum a_avail)
        {
            RR = null;
            Availability = a_avail;
            Constructed = true;
        }

        internal bool Constructed { get; private set; }

        /// <summary>
        /// Indicates either success (the helper can schedule) or a failure reason.
        /// </summary>
        internal readonly SchedulableSuccessFailureEnum Availability;

        /// <summary>
        /// If the helper can't be scheduled, this indicates whether a good time to retry scheduling on the
        /// helper was found.
        /// </summary>
        internal bool HasTryAgainTicks => m_tryAgainTicks != 0;

        private long m_tryAgainTicks;

        /// <summary>
        /// A good time to retry scheduling on the helper.
        /// </summary>
        internal long TryAgainTicks
        {
            get => m_tryAgainTicks;
            set => m_tryAgainTicks = value;
        }

        private long m_helperEndTicks;
        internal long EndTicks
        {
            get => m_helperEndTicks;
            set => m_helperEndTicks = value;
        }

        internal readonly ResourceRequirement RR;

        public NonPrimarySchedulableResult(long a_endTicks, SchedulableSuccessFailureEnum a_success)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compare based on Availability, if necessary sub-compared by TryAgainTicks.
        /// Compare based on the TryAgainTicks. Objects without TryAgainTicks have a large
        /// TryAgainTicks==long.MaxValue and so should always compare larger than real dates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal static int Compare(NonPrimarySchedulableResult x, NonPrimarySchedulableResult y)
        {
            int comp = x.Availability.Compare(y.Availability);
            if (comp == 0)
            {
                comp = Comparer.Default.Compare(x.TryAgainTicks, y.TryAgainTicks);
            }

            return comp;
        }

        public override string ToString()
        {
            StringBuilder sb = new ();
            sb.AppendFormat("{0}", Availability);
            if (HasTryAgainTicks)
            {
                sb.AppendFormat("; {0}", DateTimeHelper.ToLongLocalTimeFromUTCTicks(TryAgainTicks));
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Determine whether the activity's Helper Resource Requirement can be scheduled to match the Primary.
    /// </summary>
    internal NonPrimarySchedulableResult NonPrimarySchedulable(ScenarioDetail a_sd, InternalActivity a_activity, ResourceRequirement a_rr, ResourceSatiator[] a_resourceRequirementSatiaters, SchedulableInfo a_primarySi, LinkedListNode<ResourceCapacityInterval> a_helperAvailableNode)
    {
        NonPrimarySchedulableResult result = new (SchedulableSuccessFailureEnum.Success);

        OperationCapacityProfile helperOcp = a_primarySi.m_ocp.ReduceToRequirementUsage(a_rr);

        long helperStart = helperOcp.GetUsageStart();
        long helperEnd = helperOcp.GetUsageEnd();

        #if DEBUG
        if (helperEnd <= helperStart && a_rr.UsageStart != MainResourceDefs.usageEnum.Storage)
        {
            throw new PTException("The finish date must be greater than the start date.");
        }
        #endif

        //TODO: Should these be helper start and helper end?
        if (IntersectsReservedMoveTime(a_sd, a_activity, helperStart, helperEnd, out LeftNeighborSequenceValues sequenceValues))
        {
            result = new NonPrimarySchedulableResult(a_rr, SchedulableSuccessFailureEnum.IntersectsReservedMoveDate);

            return result;
        }

        // [USAGE_CODE] NonPrimarySchedulable(): Check for intersection with a block reservation.
        if (CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            //Check for existing primary blocks that did not create a reservation. These primaries might be scheduled on this op's helper.
            if (!a_activity.BeingMoved && LastScheduledBlockListNode?.Data is ResourceBlock resourceBlock)
            {
                if (resourceBlock.StartTicks <= helperEnd && resourceBlock.EndTicks > helperStart)
                {
                    //Try again at the end of the block in the way
                    result = new NonPrimarySchedulableResult(a_rr, SchedulableSuccessFailureEnum.Occupied, resourceBlock.EndTicks);
                    return result;
                }
            }

            // Check for intersection with a block reservation.
            if (IntersectsBlockReservation(a_activity, helperStart, helperEnd, out sequenceValues))
            {
                result = new NonPrimarySchedulableResult(a_rr, SchedulableSuccessFailureEnum.IntersectsBlockReservation, sequenceValues.EndDate);
                return result;
            }
        }

        LinkedListNode<ResourceCapacityInterval> thisHelperCapacityNode;
        if (a_helperAvailableNode != null)
        {
            thisHelperCapacityNode = a_helperAvailableNode;
        }
        else if (AvailableInSimulation != null)
        {
            thisHelperCapacityNode = AvailableInSimulation.Node;
        }
        else
        {
            //The helper doesn't currently have capacity, but it could be available in the future.
            thisHelperCapacityNode = LastResultantCapacityNode;
            if (helperStart < LastResultantCapacityNode.Value.StartDate)
            {
                //We are searching before capacity is available. 
                return new NonPrimarySchedulableResult(a_rr, SchedulableSuccessFailureEnum.LackCapacity, LastResultantCapacityNode.Value.StartDate);
            }

            thisHelperCapacityNode = ResultantCapacity.FindForwards(helperStart, thisHelperCapacityNode);
        }

        if (CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
        {
            // Multi-tasking resources can intersect block reservations as long as there's
            // still enough attention available.
            ResourceCapacityInterval.AttnAvailResult attnAvail = AttentionAvailable(a_activity, helperStart, helperEnd, a_rr, a_resourceRequirementSatiaters, a_primarySi, thisHelperCapacityNode);
            if (attnAvail.SuccessResult != SchedulableSuccessFailureEnum.Success)
            {
                result = new NonPrimarySchedulableResult(a_rr, attnAvail.SuccessResult, attnAvail.AvailableTicks);
                return result;
            }
        }

        ActivityResourceBufferInfo bufferInfo = a_activity.GetBufferResourceInfo(a_primarySi.m_resource.Id);
        bool primaryActIsLate = !bufferInfo.DbrJitDateCalculated && a_primarySi.m_ocp.StorageProfile.CapacityEndTicks > bufferInfo.BufferEndDate;

        //Loop through each primary capacity usage and make sure there is enough capacity on the helper to match.
        bool start = true;
        foreach ((MainResourceDefs.usageEnum capacityType, OperationCapacity ocp) in helperOcp.CapacityEnumerator())
        {
            thisHelperCapacityNode = ResultantCapacity.FindForwards(ocp.StartTicks, thisHelperCapacityNode);
            if (thisHelperCapacityNode.Value.StartDate > ocp.StartTicks)
            {
                //No capacity at the start time
                return new NonPrimarySchedulableResult(a_rr, SchedulableSuccessFailureEnum.LackCapacity, thisHelperCapacityNode.Value.StartDate);
            }

            FindCapacityResult helperCapacityResult = FindCapacity(ocp.StartTicks,
                ocp.TotalCapacityTicks,
                a_activity.Operation.CanPause,
                thisHelperCapacityNode,
                capacityType,
                start,
                primaryActIsLate,
                a_rr.CapacityCode,
                false,
                a_activity.PeopleUsage,
                a_activity.NbrOfPeople,
                out bool _);
            if (helperCapacityResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
            {
                return new NonPrimarySchedulableResult(a_rr, helperCapacityResult.ResultStatus, helperCapacityResult.NextStartTime);
            }

            start = false;
        }

        return result;
    }

    private NonPrimarySchedulableResult FindHelperCapacityInternal(OperationCapacityProfile a_helperOcp, LinkedListNode<ResourceCapacityInterval> a_thisHelperCapacityNode, ResourceRequirement a_rr, InternalActivity a_activity, ActivityResourceBufferInfo a_bufferInfo, bool a_late, out bool o_willSpanLateOnlyRci)
    {
        o_willSpanLateOnlyRci = false;
        //Loop through each primary capacity usage and make sure there is enough capacity on the helper to match.
        bool start = true;
        FindCapacityResult helperCapacityResult = null;
        foreach ((MainResourceDefs.usageEnum capacityType, OperationCapacity ocp) in a_helperOcp.CapacityEnumerator())
        {
            a_thisHelperCapacityNode = ResultantCapacity.FindForwards(ocp.StartTicks, a_thisHelperCapacityNode);
            if (a_thisHelperCapacityNode.Value.StartDate > ocp.StartTicks)
            {
                //No capacity at the start time
                return new NonPrimarySchedulableResult(a_rr, SchedulableSuccessFailureEnum.LackCapacity, a_thisHelperCapacityNode.Value.StartDate);
            }

            helperCapacityResult = FindCapacity(ocp.StartTicks,
                ocp.TotalCapacityTicks,
                a_activity.Operation.CanPause,
                a_thisHelperCapacityNode,
                capacityType,
                start,
                a_late,
                a_rr.CapacityCode,
                false,
                a_activity.PeopleUsage,
                a_activity.NbrOfPeople,
                out o_willSpanLateOnlyRci);
            if (helperCapacityResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
            {
                return new NonPrimarySchedulableResult(a_rr, helperCapacityResult.ResultStatus, helperCapacityResult.NextStartTime);
            }

            start = false;
        }

        if (helperCapacityResult != null)
        {
            return new NonPrimarySchedulableResult(helperCapacityResult.FinishDate, SchedulableSuccessFailureEnum.Success);
        }

        return new NonPrimarySchedulableResult(SchedulableSuccessFailureEnum.SuccessResNotRequired); 
    }

    #region Overlap
    internal class AllocationResult
    {
        internal readonly long m_matlReleaseTicks;

        internal readonly decimal m_qtyAllocatedFromOnHand;
        internal readonly decimal m_qtyAllocatedFromExpected;
        internal readonly decimal m_qtyFromOverlap;
        internal readonly decimal m_nonAllocatedQty;

        /// <summary>
        /// Quantity coming from the non-allocated quantity source such as lead-time or the planning horizon.
        /// </summary>
        internal readonly decimal m_nonallocatedQtySourced;

        internal readonly long m_nonAllocatedQtyReleaseDate;
        internal readonly decimal m_unaccountedQty;
        internal readonly decimal m_qtyFromLeadTime;

        /// <summary>
        /// If the quantity is available, this is set to the first node in the QtyProfile that has enough material.
        /// </summary>
        internal readonly QtyNode m_earliestAvailableQtyNode;

        internal AllocationResult(
            long a_matlReleaseDate,
            decimal a_qtyAllocatedFromOnHand,
            decimal a_qtyAllocatedFromExpected,
            decimal a_qtyFromOverlap,
            decimal a_nonAllocatedQty,
            decimal a_nonallocatedQtySourced,
            long a_nonAllocatedQtyReleaseDate,
            decimal a_unaccountedQty,
            decimal a_qtyFromLeadTime,
            QtyNode a_earliestAvailableQtyNode)
        {
            m_matlReleaseTicks = a_matlReleaseDate;

            m_qtyAllocatedFromOnHand = a_qtyAllocatedFromOnHand;
            m_qtyAllocatedFromExpected = a_qtyAllocatedFromExpected;
            m_qtyFromOverlap = a_qtyFromOverlap;
            m_nonAllocatedQty = a_nonAllocatedQty;
            m_nonallocatedQtySourced = a_nonallocatedQtySourced;
            m_nonAllocatedQtyReleaseDate = a_nonAllocatedQtyReleaseDate;
            m_unaccountedQty = a_unaccountedQty;
            m_qtyFromLeadTime = a_qtyFromLeadTime;
            m_earliestAvailableQtyNode = a_earliestAvailableQtyNode;
        }

        internal AllocationResult(AllocationResult a_x, AllocationResult a_y)
        {
            m_matlReleaseTicks = Math.Max(a_x.m_matlReleaseTicks, a_y.m_matlReleaseTicks);
            m_qtyAllocatedFromOnHand = a_x.m_qtyAllocatedFromOnHand + a_y.m_qtyAllocatedFromOnHand;
            m_qtyAllocatedFromExpected = a_x.m_qtyAllocatedFromExpected + a_y.m_qtyAllocatedFromExpected;
            
            m_qtyFromOverlap = a_x.m_qtyFromOverlap + a_y.m_qtyFromOverlap;
            m_nonAllocatedQty = a_x.m_nonAllocatedQty + a_y.m_nonAllocatedQty;
            m_nonallocatedQtySourced = a_x.m_nonallocatedQtySourced + a_y.m_nonallocatedQtySourced;
            m_nonAllocatedQtyReleaseDate = Math.Max(a_x.m_nonAllocatedQtyReleaseDate, a_y.m_nonAllocatedQtyReleaseDate);
            m_unaccountedQty = a_x.m_unaccountedQty + a_y.m_unaccountedQty;
            m_qtyFromLeadTime = a_x.m_qtyFromLeadTime + a_y.m_qtyFromLeadTime;
        }

        public override string ToString()
        {
            StringBuilder sb = new ();
            string releaseDate = DateTimeHelper.ToLocalTimeFromUTCTicks(m_matlReleaseTicks).ToString();
            sb.AppendFormat("Supply: {0}; date={1}", string.Empty, releaseDate);
            if (m_qtyAllocatedFromOnHand > 0)
            {
                sb.AppendFormat("; QtyAllocatedFromOneHand={0}", m_qtyAllocatedFromOnHand);
            }

            if (m_qtyAllocatedFromExpected > 0)
            {
                sb.AppendFormat("; QtyAllocatedFromExpected={0}", m_qtyAllocatedFromExpected);
            }

            if (m_qtyFromOverlap > 0)
            {
                sb.AppendFormat("; QtyFromOverlap={0}", m_qtyFromOverlap);
            }

            if (m_nonAllocatedQty > 0)
            {
                sb.AppendFormat("; NonAllocatedQty={0}", m_nonAllocatedQty);
            }

            if (m_nonallocatedQtySourced > 0)
            {
                sb.AppendFormat("; NonAllocatedQtySourced={0}; NonAllocatedQtyReleasedDate={1}", m_nonallocatedQtySourced, DateTimeHelper.ToLocalTimeFromUTCTicks(m_nonAllocatedQtyReleaseDate));
            }

            return sb.ToString();
        }

        private static readonly AllocationResult s_noMatlSource;

        /// <summary>
        /// The AllocationResult for use when there is no material source. For instance
        /// there's no inventory and/or no expected. The Source will be set to None.
        /// </summary>
        internal static AllocationResult NoMatlSource => s_noMatlSource;
    }

    /// <summary>
    /// Allocates material if it's available at the simulation clock. Otherwise, returns the earliest the activity could start based on projected inventory.
    /// </summary>
    internal AllocationResult AttemptToAllocateMaterial(MaterialRequirement a_mr, bool a_overlap, decimal a_neededQty, Inventory a_inv, SchedulableInfo a_si, long a_simClock, InternalActivity a_act, long a_clock, ScenarioDetail a_sd, long a_endOfPlanningHorizon)
    {
        decimal qtyAllocatedFromOnHand = 0;
        decimal qtyAllocatedFromExpected = 0;
        decimal qtyFromOverlap = 0;
        decimal nonAllocatedQty = a_neededQty;
        // The material date that will be used if material can't be allocated. This could be the inventory lead time or end of planning horizon.
        long nonAllocatedQtyDefaultReleaseDate;
        decimal unaccountedQty = 0;
        decimal qtyFromLeadTime = 0;
        decimal nonallocatedQtySourced = 0;

        {
            long invLeadTimeReleaseTicks = a_sd.CalcLeadTimeTicks(a_act.Operation, a_mr, a_inv);

            // Initialize variable to default release dates and supply types.
            switch (a_mr.ConstraintType)
            {
                case MaterialRequirementDefs.constraintTypes.NonConstraint:
                case MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByAvailableDate:
                case MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByEarlierOfLeadTimeOrAvailableDate:
                    nonAllocatedQtyDefaultReleaseDate = a_clock;
                    if (a_simClock >= invLeadTimeReleaseTicks)
                    {
                    }
                    else
                    {
                    }

                    break;

                case MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate:
                    if (invLeadTimeReleaseTicks <= a_endOfPlanningHorizon)
                    {
                        nonAllocatedQtyDefaultReleaseDate = invLeadTimeReleaseTicks;
                    }
                    else
                    {
                        //If leadtime is past planning horizon, constrain by planning horizon.
                        nonAllocatedQtyDefaultReleaseDate = a_endOfPlanningHorizon;
                    }

                    break;

                case MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate:
                    nonAllocatedQtyDefaultReleaseDate = a_endOfPlanningHorizon;
                    break;

                default:
                    throw new Exception("Unexpected or new material requirement id.");
            }
        }

        QtyNode earliestAvailableQtyNode;

        MatlAvailResult earlistProcStartResult = EarliestActivityProcessingCanStartBasedOnInvAndMaterialOverlap(a_simClock, a_mr, a_overlap, a_sd, a_act, a_inv, a_neededQty, a_si, nonAllocatedQtyDefaultReleaseDate, out earliestAvailableQtyNode);

        long matlAvailableDate;

        if (earlistProcStartResult.EarliestStartDate <= a_si.m_scheduledStartDate)
        {
            // The activity can start now. The material is either available now or the inventory.LeadTime, Item.LeadTime, or PlanningHorizon has been reached.
            matlAvailableDate = a_si.m_scheduledStartDate;

            //TODO: Storage
            //a_inv.Allocate(a_sd, this, a_simClock, a_si, a_mr, a_mr, a_mr.UsabilityRequirement, this, a_matlOverlapType, a_act, a_neededQty, false, out qtyAllocatedFromOnHand, out qtyAllocatedFromExpected, out qtyFromOverlap, out nonAllocatedQty, out unaccountedQty, nonAllocatedQtyDefaultReleaseDate, out nonallocatedQtySourced, out qtyFromLeadTime, nonAllocatedQtySupplySource, out supplyType);
        }
        else
        {
            if (earlistProcStartResult.AvailableType == MatlAvailResult.AvailType.NotAllocatableReleaseTicks && nonAllocatedQtyDefaultReleaseDate > a_simClock)
            {
                nonAllocatedQty = a_neededQty;
            }

            /*TODO-MutlTank-UseOverlap*/
            if (earlistProcStartResult.AvailableType == MatlAvailResult.AvailType.Inventory && a_si.m_requiredSetupSpan.TimeSpanTicks > 0 && a_overlap)
            {
                // Using overlap, there is a time when the material will be available.
                // From the ealiest overlap start time, back off by the amount of setup time to determine when 
                // the activity could be scheduled.
                long startDate;
                LinkedListNode<ResourceCapacityInterval> startNode = ResultantCapacity.First;

                // The entire analysis below now seems like junk. Try to determine the cause of the problem.
                // But analyze what happens after the function call to make sure that also makes sense.
                // And reverify the changes made to EarliestActivityProcessingCanStartBasedOnInvAndMaterialOverlap() make sense
                // and it's named appropriately
                //***********************************************************************************************
                // ********** I might be wrong about my assessment below, take a look at the overlap conditional above. This might be right. **********
                // ********** Try to fix the error that's occuring.
                // ********** 1. get the actual earliest material available time
                // ********** 2. then check overlap to see if it can be used.
                // ********** 3. Calculate the time considering setup time.
                // The answer is closer to the original than you thought. The problem was before it wasn't checking overlap at this level.
                /*
                 *
                 get available time
                 check for no overlap and whether the time is available now
                 check for overlap and whether the quantities line up.
                 *
                 * */

                // See if it's possible to overlap the setup before the material is available.
                // In this conditional a check is being performed to see if the earliest start time based on material availability minus the setup time equals the simulation clock.
                // That is whether setup ends and processing starts exactly when the material requirement can be satisfied.
                if (FindStartTimeFromEndDateForOverlap(a_simClock, a_act, a_si.m_requiredSetupSpan.TimeSpanTicks, false, earlistProcStartResult.EarliestStartDate, ref startNode, out startDate))
                {
                    // An attempt to estimate whether the activity can start earlier if there's setup time.
                    // This should be earlier than the else part of these conditional.
                    matlAvailableDate = startDate;
                    if (matlAvailableDate <= a_si.m_scheduledStartDate)
                    {
                        //a_inv.Allocate(
                        //    a_sd,
                        //    this,
                        //    a_simClock,
                        //    a_si,
                        //    a_mr,
                        //    a_mr,
                        //    a_mr,
                        //    this,
                        //    a_matlOverlapType,
                        //    a_act,
                        //    a_neededQty,
                        //    true,
                        //    out qtyAllocatedFromOnHand,
                        //    out qtyAllocatedFromExpected,
                        //    out qtyFromOverlap,
                        //    out nonAllocatedQty,
                        //    out unaccountedQty,
                        //    nonAllocatedQtyDefaultReleaseDate,
                        //    out nonallocatedQtySourced,
                        //    out qtyFromLeadTime,
                        //    nonAllocatedQtySupplySource,
                        //    out supplyType);
                    }
                }
                else
                {
                    matlAvailableDate = earlistProcStartResult.EarliestStartDate;
                }
            }
            else
            {
                matlAvailableDate = earlistProcStartResult.EarliestStartDate;
            }
        }

        AllocationResult result = new (
            matlAvailableDate,
            qtyAllocatedFromOnHand,
            qtyAllocatedFromExpected,
            qtyFromOverlap,
            nonAllocatedQty,
            nonallocatedQtySourced,
            nonAllocatedQtyDefaultReleaseDate,
            unaccountedQty,
            qtyFromLeadTime,
            earliestAvailableQtyNode);

        return result;
    }

    internal struct MatlAvailResult
    {
        internal long EarliestStartDate { get; set; }

        internal enum AvailType
        {
            /// <summary>
            /// The quantity is available as inventory.
            /// </summary>
            Inventory,

            /// <summary>
            /// The quantity is from the non-allocatable release time, such as inventory lead-time, part lead-time, or the planning horizon.
            /// </summary>
            NotAllocatableReleaseTicks,

            /// <summary>
            /// Some inventory is available, but not enough. The remaining quantity is satisfiable through the non-allocatalbe lead-time.
            /// </summary>
            InventoryAndNotAllocatableReleaseTicks
        }

        internal AvailType AvailableType { get; set; }

        internal string EarliestStartDateString => DateTimeHelper.ToLocalTimeFromUTCTicks(EarliestStartDate).ToString();

        public override string ToString()
        {
            string s = DateTimeHelper.ToLocalTimeFromUTCTicks(EarliestStartDate).ToString();
            s += ";" + AvailableType;
            return s;
        }
    }

    /// <summary>
    /// Returns the earliest the activity processing can start. This function presumes the QtyProfiles are up-to-date and the quantities are all eligible.
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_si"></param>
    /// <param name="a_mr"></param>
    /// <param name="a_act"></param>
    /// <param name="a_needQty"></param>
    /// <param name="a_inv"></param>
    /// <param name="a_nonAllocatedQtyDefaultReleaseDate">
    /// The release date to use for material that is not expected to be ready. This may correspond to an inventory lead time, the end of the planning
    /// horizon, or in the case of a non-constraining material requirement the current APS clock time.
    /// </param>
    /// <returns></returns>
    private MatlAvailResult EarliestActivityProcessingCanStartBasedOnInvAndMaterialOverlap(long a_simClock, MaterialRequirement a_mr, bool a_overlap, ScenarioDetail a_sd, InternalActivity a_act, Inventory a_inv, decimal a_needQty, SchedulableInfo a_si, long a_nonAllocatedQtyDefaultReleaseDate, out QtyNode o_earliestAvailableQtyNode)
    {
        MatlAvailResult result = new ();
        o_earliestAvailableQtyNode = null;
        //long matlAvailableDate = a_inv.FindDateUnAllocatedQtyWillBeAvailable(a_simClock,
        //    a_sd,
        //    a_act,
        //    a_matlOverlapType,
        //    a_mr,
        //    a_mr,
        //    a_mr.UsabilityRequirement,
        //    this,
        //    a_needQty,
        //    out o_earliestAvailableQtyNode,
        //    out QtyNode earliestUnavailableQtyNode,
        //    out decimal availQty);

        //if (matlAvailableDate != -1 && matlAvailableDate <= a_si.m_scheduledStartDate)
        //{
        //    // The material is available at or before the required time. There's no need to try to find a better time.
        //    result.EarliestStartDate = matlAvailableDate;
        //    result.AvailableType = MatlAvailResult.AvailType.Inventory;

        //    return result;
        //}

        //long bestStartDate;

        //if (matlAvailableDate == -1)
        //{
        //    bestStartDate = a_nonAllocatedQtyDefaultReleaseDate;
        //    result.EarliestStartDate = bestStartDate;
        //    if (bestStartDate <= a_si.m_scheduledStartDate && availQty > 0)
        //    {
        //        result.AvailableType = MatlAvailResult.AvailType.InventoryAndNotAllocatableReleaseTicks;
        //    }
        //    else
        //    {
        //        result.AvailableType = MatlAvailResult.AvailType.NotAllocatableReleaseTicks;
        //    }

        //    if (a_inv.AvailableQtyProfile.HasNodes)
        //    {
        //        // This function presumes the node's quantities are up-to-date and eligible.
        //        earliestUnavailableQtyNode = a_inv.AvailableQtyProfile.Last.LastQtyNode;
        //    }
        //}
        //else
        //{
        //    bestStartDate = Math.Min(a_nonAllocatedQtyDefaultReleaseDate, matlAvailableDate);
        //}

        //if ((a_matlOverlapType.UseOverlapActivities || a_matlOverlapType.UseOverlapPOs) // Can use overlap
        //    &&
        //    bestStartDate > a_si.ProcStartDate) // The material isn't available right now but it's available in the future. Try to use overlap to find the date when it's available.
        //{
        //    // Attempt to find an earlier start time using overlap.
        //    ResourceOperation ro = (ResourceOperation)a_act.Operation;

        //    // Calculate the number of capacity intervals that need to be run and the amount of material required at the interval.
        //    a_act.CalculateProcessingTimeSpanOnResource(this, out RequiredSpan _, out long requiredNumberOfCycles, out decimal _);
        //    long maxNbrOfCyclesToTest = requiredNumberOfCycles - 1;
        //    long nbrOfCyclesTested = 0;

        //    LinkedListNode<ResourceCapacityInterval> currentRCINode = ResultantCapacity.First;

        //    long cycleTime = a_act.GetResourceProductionInfo(this).CycleSpanTicks;

        //    decimal useageQtyPerCycle = a_mr.DetermineUsageQtyPerCycle(ro, a_si.m_requiredNumberOfCycles);
        //    // Calculate whether it is possible to start earlier overlapping either on POs or activities that haven't arrived.

        //    bool fullyAllocatedInventoryQtyProfile = earliestUnavailableQtyNode == null;

        //    a_inv.ResetAllocationsForOverlap(a_simClock, a_sd, a_act, a_inv, a_matlOverlapType, a_mr, a_mr, a_mr, this);

        //    //TODO: This is wrong. It uses this Ops cycle time to determine earliest transfer, but this doesn't work if this act's cycle time is 
        //    //different than the producing activities cycle time. For example if this act cycle time is 10 minutes, even if transfer is 1 minute from 
        //    //a source, it will think it needs material 10 minutes earlier. This results in always missing the first transfer
        //    while (nbrOfCyclesTested < maxNbrOfCyclesToTest && currentRCINode != null && FindStartTimeFromEndDateForOverlap(a_simClock, a_act, cycleTime, true, bestStartDate, ref currentRCINode, out long potentialBestStartDate))
        //    {
        //        ++nbrOfCyclesTested;
        //        decimal remainingNeededUsageQty = useageQtyPerCycle;

        //        FindMaterial: ;
        //        if (fullyAllocatedInventoryQtyProfile)
        //        {
        //            if (availQty > remainingNeededUsageQty)
        //            {
        //                availQty -= remainingNeededUsageQty;
        //                bestStartDate = potentialBestStartDate;
        //            }
        //            else if (availQty == remainingNeededUsageQty)
        //            {
        //                bestStartDate = potentialBestStartDate;
        //                goto Exit;
        //            }
        //            else
        //            {
        //                goto Exit;
        //            }
        //        }
        //        else
        //        {
        //            //Find the first timepoint after the potentialBestStartDate.
        //            QtyProfile.QtyProfileTimePointRange futureRange = new (a_inv.FutureQtyProfile, earliestUnavailableQtyNode.Date);
        //            using IEnumerator<QtyProfileTimePoint> timePointEnumerator = futureRange.GetReverseEnumerator();
        //            QtyProfileTimePoint firstTimePointAfterPotentialBestStartDate = null;
        //            while (timePointEnumerator.MoveNext())
        //            {
        //                QtyProfileTimePoint timePoint = timePointEnumerator.Current;

        //                if (timePoint.Date <= potentialBestStartDate)
        //                {
        //                    firstTimePointAfterPotentialBestStartDate = timePoint;
        //                    break;
        //                }
        //            }

        //            //If no timepoint is after the potentialBestStartDate, then search forward using the first timepoint.
        //            if (firstTimePointAfterPotentialBestStartDate == null)
        //            {
        //                long searchDate = futureRange.FirstForwardNode()?.Date ?? 0;
        //                SearchForwardsForUnusedMaterial(a_inv.FutureQtyProfile, searchDate, ref remainingNeededUsageQty, ref bestStartDate, ref potentialBestStartDate);
        //                fullyAllocatedInventoryQtyProfile = true;
        //                goto FindMaterial;
        //            }

        //            //Continue forward through all of the future nodes
        //            QtyProfile.QtyProfileTimePointRange range = new (a_inv.FutureQtyProfile, firstTimePointAfterPotentialBestStartDate.Date);
        //            using IEnumerator<QtyProfileTimePoint> searchTimePointEnumerator = range.GetEnumerator();
        //            while (searchTimePointEnumerator.MoveNext())
        //            {
        //                QtyProfileTimePoint timePoint = searchTimePointEnumerator.Current;
        //                IEnumerator<QtyNode> nodeEnumerator = timePoint.GetEnumerator();
        //                bool missingMaterial = false;
        //                while (nodeEnumerator.MoveNext())
        //                {
        //                    QtyNode node = nodeEnumerator.Current;

        //                    if (node.UnallocatedQty >= remainingNeededUsageQty)
        //                    {
        //                        node.TestAllocate(remainingNeededUsageQty);
        //                        remainingNeededUsageQty = 0;
        //                        bestStartDate = potentialBestStartDate;
        //                        break;
        //                    }

        //                    remainingNeededUsageQty -= node.UnallocatedQty;
        //                    missingMaterial = true;
        //                    node.TestAllocateAll();
        //                }

        //                if (missingMaterial)
        //                {
        //                    //We reached the end of the timePoint without finding enough material.
        //                    SearchForwardsForUnusedMaterial(a_inv.FutureQtyProfile, timePoint.Date, ref remainingNeededUsageQty, ref bestStartDate, ref potentialBestStartDate);
        //                    fullyAllocatedInventoryQtyProfile = true;
        //                    goto FindMaterial;
        //                }
        //            }
        //        }

        //        if (bestStartDate <= a_si.ProcStartDate)
        //        {
        //            goto Exit;
        //        }
        //    }
        //}

        //Exit:

        //long ret = Math.Min(a_nonAllocatedQtyDefaultReleaseDate, bestStartDate);
        //result.EarliestStartDate = Math.Max(a_simClock, ret);
        return result;
    }

    internal long[] FindCycleStartTimes(long a_date, InternalActivity a_ia, long a_numberOfCycles, long a_procFinishDate)
    {
        long[] cycleTimes = new long[a_numberOfCycles];
        long cycleSpan = a_ia.GetResourceProductionInfo(this).CycleSpanTicks;
        long startTime = a_procFinishDate;
        LinkedListNode<ResourceCapacityInterval> currentNode = ResultantCapacity.First;

        for (long cycleI = a_numberOfCycles - 1; cycleI >= 0; --cycleI)
        {
            if (FindStartTimeFromEndDateForOverlap(a_date, a_ia, cycleSpan, true, startTime, ref currentNode, out startTime))
            {
                cycleTimes[cycleI] = startTime;
            }
        }

        return cycleTimes;
    }

    //10	[PAPER DOC ID: OverlapProblemMinor] Under special circumstances the earliest possible start time calculation for operation overlap can be late by less than 1 cycle.
    //    Image two operations supplying inventory. 
    //    Each scheduled on a different resources. 
    //    Each producing 2 units of material. 
    //    1 task on a different resource consuming all 4 units.
    //    The cycle time of producing operations and consuming operations are equal. Say one hour.
    //    When optimized the consumer schedules at the end of the first hour.
    //    But when you move one of the producers to start at about at about 66% through its original start time the release of material lines up so that the algorithm is off
    //    and the consumer is no longer able to start at the finish time of the first piece of material. This is due to the way the material ends up being allocated to the different cycles.
    //    The first product is allocated to the first consumption cycle.
    //    The second to nothing.
    //    The third to the third.
    //    The fourth to the fourth.
    //    To solve this problem I think the following needs to be done:
    //    From the start time found work forwards in time for unallocated material up to but not including the next cycle. This is an effort to adjust the start time, free up material, and try to squeeze
    //    one more earlier cycle out of the schedule. But you need to validate this is the correct approach. You'll need to unallocate material previously allocated to the operation.
    internal long DetermineEarliestOverlapTime(long a_clock, InternalOperation a_io, InternalOperation a_predOp)
    {
        TransferQtyCompletionProfile tqp = a_predOp.TransferQtyProfile;
        if (tqp == null || tqp.Count == 0)
        {
            return 0;
        }

        long lastTQCPDate = tqp[^1].m_completionDate;
        LinkedListNode<ResourceCapacityInterval> currentRCINode = ResultantCapacity.First;

        if (a_io.Activities.Count > 1)
        {
            return lastTQCPDate;
        }

        long endDate = lastTQCPDate;

        // Calculate the number of capacity internvals that need to be run and the amount of material required at the interval.
        RequiredSpan processingSpan;
        long requiredNumberOfCycles;
        decimal requiredFinishQty;
        a_io.Activities.GetByIndex(0).CalculateProcessingTimeSpanOnResource(this, out processingSpan, out requiredNumberOfCycles, out requiredFinishQty);

        InternalActivity ia = a_io.Activities.GetByIndex(0);
        long startDate = lastTQCPDate;
        long cyclesToCheck = requiredNumberOfCycles - 1;
        long cycleTime = ia.GetResourceProductionInfo(this).CycleSpanTicks;
        int tqpIdx = tqp.Count - 1;
        decimal usageQtyPerCycle = a_predOp.Successors[0].UsageQtyPerCycle;

        // Assume the last cycle starts at the end of the predecessor operation.
        // Work backwards calculating when each cycle could start and end.
        for (int cycleI = (int)cyclesToCheck; cycleI >= 1; --cycleI)
        {
            if (FindStartTimeFromEndDateForOverlap(a_clock, ia, cycleTime, true, endDate, ref currentRCINode, out startDate))
            {
                decimal usage = usageQtyPerCycle * cycleI;
                long usageSatisfactionDate = FindUsageSatisfactionDate(usage, tqp, ref tqpIdx);

                if (usageSatisfactionDate > startDate)
                {
                    // Adjust the earliest start date to when the predecessor material becomes available.
                    startDate = usageSatisfactionDate;
                }

                endDate = startDate;
            }
            else
            {
                return endDate; //tqp[tqp.Count-1].completionDate;
            }
        }

        return startDate;
    }

    /// <summary>
    /// Determine when there will be enough material available to satisfy a usage quantity requirement.
    /// </summary>
    /// <param name="usageQty">The quantity of material that's required.</param>
    /// <param name="tqcp">When predecessor material becomes available.</param>
    /// <param name="successfulIdx">The current searh index within the TransferQtyCompletionProfile</param>
    /// <returns>The latest time the material becomes available.</returns>
    private long FindUsageSatisfactionDate(decimal a_usageQty, TransferQtyCompletionProfile a_tqcp, ref int r_successfulIdx)
    {
        //			int maxIdx=tqcp.Count-1;
        //
        //			if(maxIdx==currentIdx)
        //			{
        //				if(tqcp[maxIdx].totalQtyAvailable<usageQty)
        //				{
        //					// There's not enough material at the end of completion of all the containers.
        //					// Assume the last container has enough material to satisfy the requirements.
        //					return tqcp[maxIdx].completionDate;
        //				}
        //			}

        int currentIdx = r_successfulIdx;
        r_successfulIdx = a_tqcp.Count - 1;

        while (currentIdx >= 0)
        {
            if (a_tqcp[currentIdx].m_totalQtyAvailable >= a_usageQty)
            {
                r_successfulIdx = currentIdx;
            }

            --currentIdx;
        }

        return a_tqcp[r_successfulIdx].m_completionDate;
    }

    public struct FindStartFromEndResult
    {
        public bool Success;
        public long StartTicks;
        internal decimal Capacity;

        internal FindStartFromEndResult(bool a_success, long a_startTicks, decimal a_capacity)
        {
            Success = a_success;
            StartTicks = a_startTicks;
            Capacity = a_capacity;
        }

        public override string ToString()
        {
            StringBuilder sb = new ();
            sb.AppendFormat("StartDate={0}; Capacity={1}", DateTimeHelper.ToLocalTimeFromUTCTicks(StartTicks), Capacity);
            return sb.ToString();
        }
    }

    [Obsolete("Use FindCapacityReverse instead")]
    /// <summary>
    /// Search backwards for capcity on this resource. The time that processing would need to start to obtain the capcity is returned in  a FindStartFromEndResult struct.
    /// </summary>
    /// <param name="a_clock">The current APS clock.</param>
    /// <param name="a_ia">The activity the capacity  is being searached for.</param>
    /// <param name="a_rc">The capacity being sought.</param>
    /// <param name="a_endDate">The date to start the backwards search from.</param>
    /// <returns></returns>
    public FindStartFromEndResult FindStartTimeFromEndDate(long a_clock, InternalActivity a_ia, RequiredCapacity a_rc, long a_endDate)
    {
        long endDate = a_endDate;

        FindStartFromEndResult res;
        res.Success = true;
        res.Capacity = 0;
        res.StartTicks = a_endDate;

        bool resNotSet = true;

        if (a_rc.PostProcessingSpan.TimeSpanTicks > 0)
        {
            res = FindStartTimeFromEndDate(a_clock, a_ia, a_rc.PostProcessingSpan.TimeSpanTicks, false, endDate);
            resNotSet = false;

            if (!res.Success)
            {
                return res;
            }
        }

        if (a_rc.ProcessingSpan.TimeSpanTicks > 0)
        {
            endDate = res.StartTicks;
            res = FindStartTimeFromEndDate(a_clock, a_ia, a_rc.ProcessingSpan.TimeSpanTicks, true, endDate);
            resNotSet = false;

            if (!res.Success)
            {
                return res;
            }
        }

        if (a_rc.SetupSpan.TimeSpanTicks > 0 || resNotSet)
        {
            endDate = res.StartTicks;
            res = FindStartTimeFromEndDate(a_clock, a_ia, a_rc.SetupSpan.TimeSpanTicks, false, endDate);
        }

        return res;
    }

    private FindStartFromEndResult FindStartTimeFromEndDate(long a_clock, InternalActivity a_ia, long a_requiredCapacity, bool a_capacityForProcessingTime, long a_endDate)
    {
        FindStartFromEndResult result = new ();

        result.StartTicks = -1;

        if (a_endDate <= a_clock)
        {
            return result;
        }

        LinkedListNode<ResourceCapacityInterval> currentNode = FindEndDateNode(a_endDate, ResultantCapacity.Last);
        currentNode = ResourceCapacityIntervalList.FindFirstOnlineBackwards(currentNode);

        if (currentNode == null)
        {
            return result;
        }

        ResourceCapacityInterval rci = currentNode.Value;

        // Work backwards through capacity intervals until you have obtained enough online time to satisfy the capacity requirement.
        long adjustedStartDate;
        if (rci.StartDate < a_clock)
        {
            adjustedStartDate = a_clock;
        }
        else
        {
            adjustedStartDate = rci.StartDate;
        }

        long intervalEndDate = currentNode.Value.EndDate < a_endDate ? currentNode.Value.EndDate : a_endDate;

        decimal currentNodeCapacity;
        if (a_capacityForProcessingTime)
        {
            currentNodeCapacity = CalculateCapacity(adjustedStartDate, intervalEndDate, a_ia.PeopleUsage, a_ia.NbrOfPeople, currentNode.Value, out bool capacityAdjusted, out decimal _);
        }
        else
        {
            currentNodeCapacity = intervalEndDate - adjustedStartDate;
        }

        result.Capacity = currentNodeCapacity;

        if (adjustedStartDate == a_clock && currentNodeCapacity < a_requiredCapacity)
        {
            return result;
        }

        while (result.Capacity < a_requiredCapacity)
        {
            currentNode = currentNode.Previous;

            if (currentNode == null)
            {
                return result;
            }

            if (!currentNode.Value.Active)
            {
                //No online time here
                continue;
            }

            rci = currentNode.Value;
            intervalEndDate = rci.EndDate;

            if (a_endDate <= a_clock)
            {
                return result;
            }

            if (rci.StartDate < a_clock)
            {
                adjustedStartDate = a_clock;
            }
            else
            {
                adjustedStartDate = rci.StartDate;
            }

            if (a_capacityForProcessingTime)
            {
                currentNodeCapacity = CalculateCapacity(adjustedStartDate, intervalEndDate, a_ia.PeopleUsage, a_ia.NbrOfPeople, currentNode.Value, out bool capacityAdjusted, out decimal _);
            }
            else
            {
                currentNodeCapacity = intervalEndDate - adjustedStartDate;
            }

            result.Capacity += currentNodeCapacity;
            if (adjustedStartDate == a_clock && result.Capacity < a_requiredCapacity)
            {
                // There's not enough space.
                return result;
            }
        }

        if (result.Capacity > a_requiredCapacity)
        {
            result.StartTicks = DetermineStartDate(a_ia, result.Capacity, a_requiredCapacity, currentNodeCapacity, adjustedStartDate, intervalEndDate);
        }
        else
        {
            result.StartTicks = adjustedStartDate;
        }

        result.Capacity = a_requiredCapacity;
        result.Success = true;
        return result;
    }

    [Obsolete("Use FindCapacityReverse to simplify")]
    private bool FindStartTimeFromEndDateForOverlap(long a_simClock, InternalActivity a_ia, long a_requiredCapacity, bool a_capacityForProcessingTime, long a_endDate, ref LinkedListNode<ResourceCapacityInterval> r_startNode, out long o_startDate)
    {
        LinkedListNode<ResourceCapacityInterval> currentNode = FindEndDateNode(a_endDate, r_startNode);

        if (currentNode == null)
        {
            o_startDate = 0;
            r_startNode = null;
            return false;
        }

        ResourceCapacityInterval rci = currentNode.Value;

        // Work backwards through capacity intervals until you have obtained enough online time to satisfy the capacity requirement.
        if (a_endDate <= a_simClock)
        {
            #if DEBUG
            throw new Exception("Debug exception in overlap. End date before clock.");
            #else
                o_startDate = 0;
                r_startNode = null;
                return false;
            #endif
        }

        long adjustedStartDate;
        if (rci.StartDate < a_simClock)
        {
            adjustedStartDate = a_simClock;
        }
        else
        {
            adjustedStartDate = rci.StartDate;
        }

        long intervalEndDate = a_endDate;

        decimal currentNodeCapacity;
        if (a_capacityForProcessingTime)
        {
            currentNodeCapacity = CalculateCapacity(adjustedStartDate, intervalEndDate, a_ia.PeopleUsage, a_ia.NbrOfPeople, currentNode.Value, out bool capacityAdjusted, out decimal _);
        }
        else
        {
            currentNodeCapacity = intervalEndDate - adjustedStartDate;
        }

        if (adjustedStartDate == a_simClock && currentNodeCapacity < a_requiredCapacity)
        {
            // There's not enough space left. Stop at the clock.
            o_startDate = a_simClock;
            r_startNode = null;
            return true;
        }

        decimal capacity = currentNodeCapacity;
        while (capacity < a_requiredCapacity)
        {
            currentNode = currentNode.Previous;

            if (currentNode == null)
            {
                o_startDate = 0;
                r_startNode = null;
                return false;
            }

            if (!currentNode.Value.Active)
            {
                //No online time here
                continue;
            }

            rci = currentNode.Value;
            intervalEndDate = rci.EndDate;

            if (intervalEndDate <= a_simClock)
            {
                o_startDate = 0;
                r_startNode = null;
                return false;
            }

            if (a_endDate <= a_simClock)
            {
                o_startDate = 0;
                r_startNode = null;
                return false;
            }

            if (rci.StartDate < a_simClock)
            {
                adjustedStartDate = a_simClock;
            }
            else
            {
                adjustedStartDate = rci.StartDate;
            }

            if (a_capacityForProcessingTime)
            {
                currentNodeCapacity = CalculateCapacity(adjustedStartDate, intervalEndDate, a_ia.PeopleUsage, a_ia.NbrOfPeople, currentNode.Value, out bool capacityAdjusted, out decimal _);
            }
            else
            {
                currentNodeCapacity = intervalEndDate - adjustedStartDate;
            }

            capacity += currentNodeCapacity;
            if (adjustedStartDate == a_simClock && capacity < a_requiredCapacity)
            {
                // There's not enough space left. Stop at the clock.
                o_startDate = a_simClock;
                r_startNode = null;
                return true;
            }
        }

        if (capacity > a_requiredCapacity)
        {
            o_startDate = DetermineStartDate(a_ia, capacity, a_requiredCapacity, currentNodeCapacity, adjustedStartDate, intervalEndDate);
        }
        else
        {
            o_startDate = adjustedStartDate;
        }

        // Set the start node to the latest node which is passed back to the caller. This makes it easier to continue searching from this point.
        r_startNode = currentNode;
        return true;
    }

    /// <summary>
    /// When you've found excess capacity call this function to determine where to start; some capacity at the start of the current interval
    /// won't be necessary.
    /// </summary>
    /// <param name="capacity">The amount of capacity that has been found.</param>
    /// <param name="requiredCapacity">The amount of capacity that is required.</param>
    /// <param name="currentNodeCapacity">The amount of capacity in the current online node.</param>
    /// <param name="adjustedStartDate">The adjusted start date of the current interval. The date may be adjusted if some of the interval is in the past.</param>
    /// <param name="intervalEndDate">When the current interval ends.</param>
    /// <returns></returns>
    private long DetermineStartDate(InternalActivity a_test, decimal a_capacity, long a_requiredCapacity, decimal a_currentNodeCapacity, long a_adjustedStartDate, long a_intervalEndDate)
    {
        decimal capacityOverage = a_capacity - a_requiredCapacity;
        decimal unneededFraction = capacityOverage / a_currentNodeCapacity;
        long uneededSpan = (long)Math.Round(unneededFraction * (a_intervalEndDate - a_adjustedStartDate), MidpointRounding.AwayFromZero);
        return a_adjustedStartDate + uneededSpan;
    }

    /// <summary>
    /// returns null if searching backwards doesn't turn up a capacity interval.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="startNode"></param>
    /// <returns></returns>
    private LinkedListNode<ResourceCapacityInterval> FindEndDateNode(long a_date, LinkedListNode<ResourceCapacityInterval> a_startNode)
    {
        LinkedListNode<ResourceCapacityInterval> current = a_startNode;

        if (current == null)
        {
            return null;
        }

        ResourceCapacityInterval rci = current.Value;
        ResourceCapacityInterval.ContainmentType ct = rci.ContainsEndPoint(a_date);

        if (ct == ResourceCapacityInterval.ContainmentType.LessThan)
        {
            while (rci.ContainsEndPoint(a_date) != ResourceCapacityInterval.ContainmentType.Contains)
            {
                current = current.Previous;
                if (current == null)
                {
                    break;
                }

                rci = current.Value;
            }
        }
        else if (ct == ResourceCapacityInterval.ContainmentType.GreaterThan)
        {
            while (rci.ContainsEndPoint(a_date) != ResourceCapacityInterval.ContainmentType.Contains)
            {
                current = current.Next;
                rci = current.Value;
            }
        }

        return current;
    }
    #endregion

    internal ResourceBlockList.Node Schedule(ScenarioDetail a_sd,
                                             BaseId a_blockId,
                                             Batch a_batch,
                                             InternalActivity a_activity,
                                             ResourceRequirement a_rr,
                                             long a_startTicks,
                                             long a_endTicks,
                                             SchedulableInfo a_si,
                                             bool a_ignoreRciProfile)
    {
        ResourceBlock block = new (a_blockId, a_batch, a_rr, this, a_si.m_ocp);
        int simultaneousSequenceIdx = 0;
        if (LastScheduledBlockListNode != null)
        {
            if (LastScheduledBlockListNode.Data.StartTicks == a_batch.StartTicks)
            {
                //We are scheduling a batch at the same time as an existing batch. We need to track scheduling order for non-optimize sequencing.
                simultaneousSequenceIdx = LastScheduledBlockListNode.Data.Batch.FirstActivity.SimultaneousSequenceIdx + 1;
            }
        }
        else
        {
            //Scheduling the first block on the resource, we need to create a cleanout retry event for each time based cleanout trigger.
            //the last cleanout run time would be set during the attempt to schedule if actuals are tracked.
            foreach ((int cleanoutGrade, TimeBasedCleanoutTriggerInfo triggerInfo) in m_sortedTimeBasedCleanoutTriggers)
            {
                long triggerStartTicks = triggerInfo.LastCleanoutRunTime == 0 ? a_sd.Clock : triggerInfo.LastCleanoutRunTime;

                if (triggerStartTicks > a_sd.SimClock)
                {
                    a_sd.AddEvent(new RetryForCleanoutEvent(triggerStartTicks + triggerInfo.CleanoutDurationTicks));
                }
            }
        }

        //We still need to set this value because it is not cleared on simulation initialization
        a_batch.SetSimultaneousSequenceIdx(simultaneousSequenceIdx);

        LastScheduledBlockListNode = m_blocks.Add(block, LastScheduledBlockListNode);

        if (EnforceMaxDelay && a_endTicks < a_sd.EndOfPlanningHorizon) //No need to enforce MaxDelay past the planning horizon. That would impact performance.
        {
            BlockResourceSpan brr = new (block, a_startTicks, a_endTicks);
            AddToResourceUsedAndReserveSpansSet(brr);
        }

        block.MachineBlockListNode = LastScheduledBlockListNode;
        ResourceBlockList.Node scheduledNode = LastScheduledBlockListNode;

        switch (CapacityType)
        {
            case InternalResourceDefs.capacityTypes.SingleTasking:
                IncrementScheduledCountSinceLastClearCodes();
                break;

            case InternalResourceDefs.capacityTypes.Infinite:
                break;

            case InternalResourceDefs.capacityTypes.MultiTasking:
                break;

            default:
                throw new PTException("This type of resource is not handled yet. The types the system can handle are SingleTasking, MultiTasking, and Infinite.");
        }

        //Schedule CIP on the previous block, but only if this is the primary resource (a_blockId is the rr index)
        ResourceBlockList.Node leftNeighborBlock = Blocks.Last.Previous;
        if (a_blockId.Value == 0 && leftNeighborBlock != null && a_si.m_requiredAdditionalCleanBeforeSpan.TimeSpanTicks > 0)
        {
            if (a_si.m_requiredAdditionalCleanBeforeSpan.ActivityToIncurClean?.Batch is { } previousBatch && previousBatch.Id == leftNeighborBlock.Data.Batch.Id)
            {
                long cleanStartDate = previousBatch.EndOfStorageTicks;
                //Find the starting node
                LinkedListNode<ResourceCapacityInterval> startPosition = ResultantCapacity.Find(cleanStartDate, LastResultantCapacityNode);

                //Calculate capacity for Clean to see when it will complete. //TODO: allow for capacity on cleanouts when other capacity types can't
                FindCapacityResult capacityResult = FindCapacity(cleanStartDate,
                    a_si.m_requiredAdditionalCleanBeforeSpan.TimeSpanTicks,
                    true,
                    startPosition,
                    MainResourceDefs.usageEnum.Clean,
                    false,
                    previousBatch.FirstActivity.ScheduledStartTicks() > previousBatch.FirstActivity.DbrJitStartTicks,
                    a_rr.CapacityCode,
                    a_ignoreRciProfile,
                    previousBatch.FirstActivity.PeopleUsage,
                    previousBatch.FirstActivity.NbrOfPeople, 
                    out bool _);

                if (capacityResult.ResultStatus == SchedulableSuccessFailureEnum.Success)
                {
                    previousBatch.MergeCleanout(a_si.m_requiredAdditionalCleanBeforeSpan, capacityResult.CapacityUsageProfile);
                    
                    //Look up new list for time based events. Create an event to attempt based on the highest grade less than this scheduled one.
                    if (m_sortedTimeBasedCleanoutTriggers.TryGetLowerEntry(a_si.m_requiredAdditionalCleanBeforeSpan.CleanoutGrade, true, out KeyValuePair<int, TimeBasedCleanoutTriggerInfo> timeBasedCleanoutInfo))
                    {
                        a_sd.AddEvent(new RetryForCleanoutEvent(a_sd.SimClock + timeBasedCleanoutInfo.Value.CleanoutDurationTicks));
                    }
                }
                else
                {
                    //DebugException.ThrowInDebug("Failed to add clean before span due to invalid capacity (1)");
                }
            }
        }

        ScheduleCompatibilityCode(a_sd, a_batch, a_activity, a_rr, a_startTicks, a_endTicks, a_si);

        return scheduledNode;
    }

    /// <summary>
    /// Consume attention on an interval of a multi-tasking resource. The interval can be in the future.
    /// </summary>
    /// <param name="a_activity">The activity the attention is being committed for.</param>
    /// <param name="a_rr">The requirement the attention is being committed for.</param>
    /// <param name="a_startTicks">The start of the interval.</param>
    /// <param name="a_endTicks">The end of the interval.</param>
    internal void CommitAttention(long a_simClock, InternalActivity a_activity, ResourceRequirement a_rr, long a_startTicks, long a_endTicks)
    {
        if (CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
        {
            // Find the first interval attention will be consumed from.
            LinkedListNode<ResourceCapacityInterval> curIntNode;
            if (AvailableInSimulation != null)
            {
                curIntNode = AvailableInSimulation.Node;
            }
            else
            {
                // [USAGE_CODE]
                curIntNode = ResultantCapacity.FindFirstOnline(a_startTicks, null);
                #if DEBUG
                if (!Interval.Contains(curIntNode.Value.StartDate, curIntNode.Value.EndDate, a_startTicks))
                {
                    throw new Exception("This was written for usage code. The interval found should have contained the RCI.");
                }
                #endif
            }

            // Commit attention from the intervals.
            while (curIntNode != null)
            {
                ResourceCapacityInterval curInt = curIntNode.Value;

                if (curInt.StartDate >= a_endTicks)
                {
                    // All the intervals attention is required from have been committed.
                    break;
                }

                if (curInt.Active)
                {
                    // Commit the intersection of time needed and the current interval being processed.
                    if (Interval.Intersection(curInt.StartDate, curInt.EndDate, a_startTicks, a_endTicks))
                    {
                        decimal percent = a_activity.GetAdjustedAttentionPercent(a_rr, curInt);
                        long startTime = Math.Max(curInt.StartDate, a_startTicks);
                        long releaseTime = Math.Min(curInt.EndDate, a_endTicks);
                        curInt.ScheduleAttention(a_activity, a_simClock, startTime, releaseTime, percent);
                    }
                }

                curIntNode = curIntNode.Next;
            }
        }
    }

    private void JumpToBlockBeforeDate(long a_startDate, ref ResourceBlockList.Node r_startNode)
    {
        ResourceBlockList.Node nextNode = r_startNode.Next;

        while (nextNode != null && nextNode.Data.StartTicks > a_startDate)
        {
            r_startNode = nextNode;
            nextNode = r_startNode.Next;
        }
    }

    internal void SimulationInitialization(long a_clock, ScenarioDetail a_sd, long a_planningHorizonEndTicks, ScenarioOptions a_so)
    {
        SimulationInitialization(a_clock, a_sd, a_planningHorizonEndTicks);
        InitResourceReservationsSet(a_so);
    }

    internal override void SimulationInitialization(long a_clock, ScenarioDetail a_sd, long a_planningHorizonEndTicks)
    {
        base.SimulationInitialization(a_clock, a_sd, a_planningHorizonEndTicks);

        CopyToResultantCapacity(a_clock);

        if (CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            MergeFrozenBlocksOntoResultantCapacity();
        }

        LastScheduledBlockListNode = null;

        LinkedListNode<ResourceCapacityInterval> node = ResultantCapacity.First;
        while (node != null)
        {
            node.Value.SimulationInitialization(this);
            node = node.Next;
        }

        m_blockReservations.Clear();
        if (CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
        {
            if (m_neededPctMgr == null)
            {
                m_neededPctMgr = new RCINeededPercentManager();
            }
            else
            {
                m_neededPctMgr.Clear();
            }
        }

        SimulationInitializationExtra();
        if (EnforceMaxDelay)
        {
            m_resourceSpans.Clear();
            Reservations.Clear();
        }
    }

    /// <summary>
    /// A set of RCINeededPercent that would be needed to schedule the activity being tested by the AttentionAvailable function.
    /// </summary>
    private RCINeededPercentManager m_neededPctMgr;

    private void MergeFrozenBlocksOntoResultantCapacity()
    {
        LinkedListNode<ResourceCapacityInterval> lastRCINode = ResultantCapacity.First;

        if (lastRCINode == null)
        {
            throw new PTException("The resource doesn't have any capacity.");
        }

        ResourceBlockList.Node current = m_blocks.First;

        while (current != null)
        {
            ResourceBlock b = current.Data;

            // Find the first intersecting interval.
            LinkedListNode<ResourceCapacityInterval> firstAffectedNode = lastRCINode;

            // Find the last intersecting interval.
            LinkedListNode<ResourceCapacityInterval> lastAffectedNode = firstAffectedNode;

            // Find the node that contains the Block.EndTicks. This presumes the node starts prior to Block.EndTicks
            // and that a node will be found, otherwise an null  exception will occur.
            while (!(lastAffectedNode.Value.StartDate <= b.EndTicks && lastAffectedNode.Value.EndDate > b.EndTicks))
            {
                lastAffectedNode = lastAffectedNode.Next;
            }

            if (firstAffectedNode == lastAffectedNode)
            {
                if (b.StartTicks == firstAffectedNode.Value.StartDate && b.EndTicks == firstAffectedNode.Value.EndDate)
                {
                    // The block occupies all of the time.

                    // Change the interval type to an occupied interval.
                    firstAffectedNode.Value.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Occupied;

                    // Set the last node position
                    lastRCINode = firstAffectedNode;
                }
                else if (b.StartTicks > firstAffectedNode.Value.StartDate && b.EndTicks < firstAffectedNode.Value.EndDate)
                {
                    // The block occupies some time between the start and end of the interval.

                    // Create a new interval of the same type of time to represent the time between the completion of the block and the original finish time of the first affected block.
                    ResourceCapacityInterval firstAffectedRCI = firstAffectedNode.Value;
                    ResourceCapacityInterval newInterval = new (firstAffectedRCI.Id, firstAffectedRCI.IntervalType, b.EndTicks, firstAffectedRCI.EndDate, firstAffectedRCI.NbrOfPeople, firstAffectedRCI.GetIntervalProfile());
                    ResultantCapacity.AddAfter(firstAffectedNode, newInterval);

                    // Set the last node position
                    lastRCINode = firstAffectedNode.Next;

                    // Adjust the scheduled finish date of the first interval so that it completes at the start of the block.
                    firstAffectedNode.Value.EndDate = b.StartTicks;

                    // Create a new interval of occupied time for the block.
                    ResourceCapacityInterval occupiedInterval = new (BaseId.NULL_ID, CapacityIntervalDefs.capacityIntervalTypes.Occupied, b.StartTicks, b.EndTicks, IntervalProfile.DefaultProfile);
                    ResultantCapacity.AddAfter(firstAffectedNode, occupiedInterval);
                }
                else if (b.StartTicks == firstAffectedNode.Value.StartDate && b.EndTicks < firstAffectedNode.Value.EndDate)
                {
                    // The block occupies some time from the start of the interval until some time before the end of the interval.

                    // Create a new interval of the same type that spans the time from the blocks completion until the first capacity intervals end time.
                    ResourceCapacityInterval firstAffectedRCI = firstAffectedNode.Value;
                    ResourceCapacityInterval newInterval = new (firstAffectedRCI.Id, firstAffectedRCI.IntervalType, b.EndTicks, firstAffectedRCI.EndDate, firstAffectedRCI.NbrOfPeople, firstAffectedRCI.GetIntervalProfile());
                    ResultantCapacity.AddAfter(firstAffectedNode, newInterval);

                    // Set the last node position
                    lastRCINode = firstAffectedNode.Next;

                    // Convert the capacity interval to an occupied interval that ends at the end of the block.
                    firstAffectedRCI.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Occupied;
                    firstAffectedRCI.EndDate = b.EndTicks;
                }
                else if (b.StartTicks > firstAffectedNode.Value.StartDate && b.EndTicks == firstAffectedNode.Value.EndDate)
                {
                    // The block occupies some time after the start of the interval until the end of the interval.

                    // Adjust the end time of the capacity interval to the start time of the block.
                    ResourceCapacityInterval firstAffectedRCI = firstAffectedNode.Value;
                    firstAffectedRCI.EndDate = b.StartTicks;

                    // Create a new occupied capacity interval for the block.
                    ResourceCapacityInterval newRCI = new (BaseId.NULL_ID, CapacityIntervalDefs.capacityIntervalTypes.Occupied, b.StartTicks, b.EndTicks, IntervalProfile.DefaultProfile);
                    ResultantCapacity.AddAfter(firstAffectedNode, newRCI);

                    // Set the last node position
                    lastRCINode = firstAffectedNode.Next;
                }
                else
                {
                    throw new PTException("An resultant capacity interval couldn't be determined.");
                }
            }
            else
            {
                //// Delete any other blocks between the first block and the last block.
                //LinkedListNode<ResourceCapacityInterval> cur = firstAffectedNode;
                //while (cur != lastAffectedNode)
                //{
                //    LinkedListNode<ResourceCapacityInterval> next = cur.Next;
                //    ResultantCapacity.Remove(cur);
                //    cur = next;
                //}
                // Delete any other blocks between the first block and the last block.
                while (firstAffectedNode.Next != lastAffectedNode)
                {
                    ResultantCapacity.Remove(firstAffectedNode.Next);
                }

                if (firstAffectedNode.Value.StartDate == b.StartTicks && lastAffectedNode.Value.EndDate == b.EndTicks)
                {
                    // The block consumes both intervals.

                    // Merge the 2 intervals.
                    firstAffectedNode.Value.EndDate = b.EndTicks;
                    ResultantCapacity.Remove(lastAffectedNode);

                    // Set the last node position
                    lastRCINode = firstAffectedNode;
                }
                else if (firstAffectedNode.Value.StartDate < b.StartTicks && lastAffectedNode.Value.EndDate > b.EndTicks)
                {
                    // The block consumes portions of both intervals.

                    // Trim both intervals.
                    firstAffectedNode.Value.EndDate = b.StartTicks;
                    lastAffectedNode.Value.StartDate = b.EndTicks;

                    // Create a new interval for the time consumed.
                    ResourceCapacityInterval newInterval = new (BaseId.NULL_ID, CapacityIntervalDefs.capacityIntervalTypes.Occupied, b.StartTicks, b.EndTicks, IntervalProfile.DefaultProfile);
                    ResultantCapacity.AddAfter(firstAffectedNode, newInterval);

                    // Set the last node position
                    lastRCINode = lastAffectedNode;
                }
                else if (firstAffectedNode.Value.StartDate < b.StartTicks && lastAffectedNode.Value.EndDate == b.EndTicks)
                {
                    // The block consumes some of the first interval and all of the last interval.

                    // Trim the first interval.
                    firstAffectedNode.Value.EndDate = b.StartTicks;

                    // Consume the last interval.
                    lastAffectedNode.Value.StartDate = b.StartTicks;
                    lastAffectedNode.Value.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Occupied;

                    // Set the last node position
                    lastRCINode = lastAffectedNode;
                }
                else if (firstAffectedNode.Value.StartDate == b.StartTicks && lastAffectedNode.Value.EndDate > b.EndTicks)
                {
                    // The block consumes all of the first interval and part of the last interval.

                    // Consume the first interval.
                    firstAffectedNode.Value.EndDate = b.EndTicks;
                    firstAffectedNode.Value.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Occupied;

                    // Trim the last interval.
                    lastAffectedNode.Value.StartDate = b.EndTicks;

                    // Set the last node position
                    lastRCINode = lastAffectedNode;
                }
                else
                {
                    throw new PTException("An resultant capacity interval couldn't be determined.");
                }
            }

            current = current.Next;
        }
    }

    /// <summary>
    /// Calculate the setup time required for an activity taking its status and reported setup time into consideration.
    /// The minimum time this function will return is 0.
    /// </summary>
    /// <returns>Setup time in ticks.</returns>
    private RequiredSpanPlusClean CalculateStandardCleanSpan(InternalActivity a_act)
    {
        long cleanTicks = 0;
        int cleanoutGrade = 0;

        InternalActivityDefs.productionStatuses productionStatus = a_act.ProductionStatus;
        decimal productionCleanoutCost = 0;
        decimal resourceCleanoutCost = 0;
        if (productionStatus == InternalActivityDefs.productionStatuses.Finished)
        {
            cleanTicks = 0;
        }
        else
        {
            if (UseOperationCleanout)
            {
                ProductionInfo productionInfo = a_act.GetResourceProductionInfo(this);
                cleanTicks = (long)Math.Ceiling((decimal)productionInfo.CleanSpanTicks);
                cleanoutGrade = productionInfo.CleanoutGrade;
                if (cleanTicks > 0)
                {
                    productionCleanoutCost = productionInfo.CleanoutCost;
                }
            }

            if (UseResourceCleanout)
            {
                if (StandardCleanoutGrade > cleanoutGrade)
                {
                    //Use the higher grade
                    cleanTicks = StandardCleanSpanTicks;
                    cleanoutGrade = StandardCleanoutGrade;
                    if (cleanTicks > 0)
                    {
                        //Standard clean is higher grade, so we are only incurring the ResourceCleanout and its cost
                        productionCleanoutCost = 0;
                    }
                }
                else if (StandardCleanoutGrade == cleanoutGrade)
                {
                    //Use the larger of the two if they are the same grade
                    if (StandardCleanSpanTicks > cleanTicks)
                    {
                        cleanTicks = StandardCleanSpanTicks;
                        cleanoutGrade = StandardCleanoutGrade;
                        if (cleanTicks > 0)
                        {
                            //Standard Clean is same grade but longer so we are incurring the Standard Clean and its cost
                            productionCleanoutCost = 0;
                        }
                    }
                }

            }
        }

        cleanTicks = Math.Max(cleanTicks, 0);
        resourceCleanoutCost = cleanTicks > 0 ? ResourceCleanoutCost : 0;
        RequiredSpanPlusClean requiredSpan = new (cleanTicks, false, cleanoutGrade);
        requiredSpan.SetStaticCosts(0, productionCleanoutCost, resourceCleanoutCost);
        return requiredSpan;
    }

    #region Setup Number Dipatch Helpers
    #region Operation SetupNumber
    public decimal? GetCurOpSetupNbr(long a_simClock)
    {
        if (CodesClear)
        {
            return null;
        }

        LeftNeighborSequenceValues leftNeighborSequenceValues = CreateDefaultLeftNeighborSequenceValues(a_simClock);
        return leftNeighborSequenceValues.Activity?.Operation.SetupNumber;
    }
    #endregion

    #region Attribute SetupNumber
    //internal decimal GetCurAttrSetupNbr(string a_attrName)
    //{
    //    if (LastScheduledBlockListNode == null)
    //    {
    //        return 0;
    //    }

    //    OperationAttribute attr = LastScheduledBlockListNode.Data.Activity.Operation.Attributes.Find(a_attrName);

    //    if (attr == null
    //        || CodesClear)
    //    {
    //        return 0;
    //    }
    //    else
    //    {
    //        return attr.Number;
    //    }
    //}
    #endregion

    private enum sawtoothDirection { Up, Down, NotSet }

    #region Operation Sawtooth Setup Number
    /// <summary>
    /// Indicates if the Resource has scheduled at least two blocks and a sawtooth direction has been set.
    /// </summary>
    /// <returns></returns>
    private sawtoothDirection GetCurrentSawtoothDirection(ResourceBlockList.Node a_lastScheduled, ResourceBlockList.Node a_secondToLastScheduled)
    {
        if (CodesClear || a_lastScheduled == null || a_secondToLastScheduled == null || !(Dispatcher.DispatcherDefinition is BalancedCompositeDispatcherDefinition))
        {
            return sawtoothDirection.NotSet;
        }

        if (a_lastScheduled.Data.Batch.FirstActivity.Operation.SetupNumber > a_secondToLastScheduled.Data.Batch.FirstActivity.Operation.SetupNumber)
        {
            return sawtoothDirection.Up;
        }

        if (a_lastScheduled.Data.Batch.FirstActivity.Operation.SetupNumber < a_secondToLastScheduled.Data.Batch.FirstActivity.Operation.SetupNumber)
        {
            return sawtoothDirection.Down;
        }

        return sawtoothDirection.NotSet;
    }
    #endregion

    #region Attribute Sawtooth Setup Number
    private sawtoothDirection m_curAttribDirection = sawtoothDirection.NotSet;
    #endregion
    #endregion Setup Number Dipatch Helpers

    /// <summary>
    /// Determine whether there is a difference between the product produced by this activity and the product produced by the last activity scheduled.
    /// The CurrentProductSetup is looked at too if it has been set.
    /// </summary>
    /// <param name="activity"></param>
    /// <returns></returns>
    private bool ProductChanged(InternalActivity a_activity, long a_simClock)
    {
        if (CodesClear)
        {
            return false;
        }

        if (LastScheduledBlockListNode == null)
        {
            //Nothing scheduled, check last finished
            InternalActivity lastRunActivity = GetLastRunActivity(a_simClock);
            if (lastRunActivity == null)
            {
                //No history stored either
                return true;
            }

            return Common.Text.TextUtil.EqualsNoWS(lastRunActivity.Operation.ManufacturingOrder.ProductName, a_activity.Operation.ManufacturingOrder.ProductName, true) == false;
        }

        InternalOperation io = LastScheduledBlockListNode.Data.Activity.Operation;

        if (io.Products.Count != 0 || a_activity.Operation.Products.Count != 0)
        {
            return !io.Products.Equals(a_activity.Operation.Products);
        }

        if (io.ManufacturingOrder.ProductName.Length == 0)
        {
            return true;
        }

        return Common.Text.TextUtil.EqualsNoWS(LastScheduledBlockListNode.Data.Activity.Operation.ManufacturingOrder.ProductName, a_activity.Operation.ManufacturingOrder.ProductName, true) == false;
    }

    /// <summary>
    /// Determine whether the Setup Number of this operation is different from the Setup Number of the last operation scheduled.
    /// If there is no last operation that has been scheduled then the Current Setup Number is used.
    /// </summary>
    private bool SetupNumberChanged(InternalActivity a_activity, long a_simClock)
    {
        if (CodesClear)
        {
            return false;
        }

        if (LastScheduledBlockListNode == null)
        {
            return GetLastRunActivity(a_simClock)?.Operation.SetupNumber != a_activity.Operation.SetupNumber;
        }

        return LastScheduledBlockListNode.Data.Activity.Operation.SetupNumber != a_activity.Operation.SetupNumber;
    }


    /// <summary>
    /// Determine whether the Setup Number of this operation is higher from the Setup Number of the last operation scheduled.
    /// If there is no last operation that has been scheduled then the Current Setup Number is used.
    /// </summary>
    private bool SetupNumberIncreased(InternalActivity a_activity, long a_simClock)
    {
        if (CodesClear)
        {
            return false;
        }

        if (LastScheduledBlockListNode == null)
        {
            return GetLastRunActivity(a_simClock)?.Operation.SetupNumber < a_activity.Operation.SetupNumber;
        }

        return LastScheduledBlockListNode.Data.Activity.Operation.SetupNumber < a_activity.Operation.SetupNumber;
    }

    /// <summary>
    /// Determine whether the Setup Number of this operation is lower from the Setup Number of the last operation scheduled.
    /// If there is no last operation that has been scheduled then the Current Setup Number is used.
    /// </summary>
    private bool SetupNumberDecreased(InternalActivity a_activity, long a_simClock)
    {
        if (CodesClear)
        {
            return false;
        }

        if (LastScheduledBlockListNode == null)
        {
            return GetLastRunActivity(a_simClock)?.Operation.SetupNumber > a_activity.Operation.SetupNumber;
        }

        return LastScheduledBlockListNode.Data.Activity.Operation.SetupNumber > a_activity.Operation.SetupNumber;
    }

    /// <summary>
    /// [PrependSetupToMoveBlocksRightNeighbors] 3-2: Calculate the setup time even though the setup codes are clear.
    /// A special override of CalculateSetupTime with the added parameter a_ignoreCodesClearTrue that allows you to
    /// calculate the setup time even if the setup codes have been cleared. Examples of when the setup codes are clear include:
    /// when a simulation isn't running, and after a cleanout interval.
    /// </summary>
    /// <param name="a_act"></param>
    /// <param name="a_leftNeighborBlockListNode"></param>
    /// <param name="a_deductReported"></param>
    /// <param name="a_useSetupSequencing"></param>
    /// <param name="a_cachedSetupSpan"></param>
    /// <param name="a_startTicks"></param>
    /// <param name="a_ignoreCodesClearTrue">Whether to  calculate the setup time even if the setup codes are clear.</param>
    /// <returns></returns>
    internal RequiredSpanPlusSetup CalculateSetupTime(long a_simClock,
                                                      InternalActivity a_act,
                                                      LeftNeighborSequenceValues a_leftNeighborActivityValues,
                                                      bool a_useSetupSequencing,
                                                      RequiredSpanPlusSetup a_cachedSetupSpan,
                                                      long a_startTicks,
                                                      ExtensionController a_timeCustomizer,
                                                      bool a_ignoreCodesClearTrue = false)
    {
        bool codesClearOrig = CodesClear; // Save the CodesClear value.

        if (a_ignoreCodesClearTrue)
        {
            // turn off the CodesClear so setups are calculated.
            CodesClear = false;
        }

        LeftNeighborSequenceValues sequenceVals = a_leftNeighborActivityValues;
        if (!a_leftNeighborActivityValues.Initialized)
        {
            sequenceVals = CreateDefaultLeftNeighborSequenceValues(a_simClock);
        }

        RequiredSpanPlusSetup setup = CalculateSetupTime(a_simClock, a_act, sequenceVals, a_useSetupSequencing, a_cachedSetupSpan, a_startTicks, a_timeCustomizer);

        if (a_ignoreCodesClearTrue)
        {
            CodesClear = codesClearOrig;
        }

        return setup;
    }

    public RequiredSpanPlusSetup CalculateSetupTimeExternally(InternalActivity a_act, ScenarioDetail a_sd, long a_startTicks)
    {
        LeftNeighborSequenceValues sequenceValues = CreateDefaultLeftNeighborSequenceValues(a_startTicks);

        return CalculateSetupTime(a_sd.SimClock, a_act, sequenceValues, true, RequiredSpanPlusSetup.s_notInit, a_startTicks, a_sd.ExtensionController);
    }

    public RequiredSpanPlusSetup CalculateSetupTimeExternally(InternalActivity a_act, ScenarioDetail a_sd, long a_startTicks, LeftNeighborSequenceValues a_previousActivityValues)
    {
        return CalculateSetupTime(a_sd.SimClock, a_act, a_previousActivityValues, true, RequiredSpanPlusSetup.s_notInit, a_startTicks, a_sd.ExtensionController);
    }

    /// <summary>
    /// Calculates the net Cleanout time -- this is the duration to be scheduled for clean.
    /// This is only the clean that needs to occur before this operation, not the clean that will be scheduled afterwards
    /// The minimum value that this function will return is 0.
    /// </summary>
    /// <returns></returns>
    public RequiredSpanPlusClean CalculateSequenceCleanoutTimeExternally(InternalActivity a_act, ScenarioDetail a_sd, LeftNeighborSequenceValues a_sequenceVals, long a_simClock)
    {
        LeftNeighborSequenceValues sequenceVals = a_sequenceVals;
        if (!sequenceVals.Initialized)
        {
            sequenceVals = CreateDefaultLeftNeighborSequenceValues(a_simClock);
        }

        if (sequenceVals.Activity is InternalActivity leftNeighborAct)
        {
            ResourceRequirement rr;
            if (leftNeighborAct.Batch is Batch scheduledBatch)
            {
                ResourceBlockList.Node leftNeighborNode = scheduledBatch.BlockForResource(this);
                rr = leftNeighborAct.Operation.ResourceRequirements.GetByIndex(leftNeighborNode.Data.ResourceRequirementIndex);
            }
            else
            {
                //Assume the primary, for example in MaxDelay the future sequence may not be scheduled
                rr = leftNeighborAct.Operation.ResourceRequirements.PrimaryResourceRequirement;
            }

            if (rr.UsageEnd != MainResourceDefs.usageEnum.Clean)
            {
                //The previous block does not run cleans, so there is nothing to calculate here.
                return RequiredSpanPlusClean.s_notInit;
            }

            if (leftNeighborAct.Operation.PreventSplitsFromIncurringClean && leftNeighborAct.Operation == a_act.Operation)
            {
                //The operation doesn't incur clean between activities.
                return RequiredSpanPlusClean.s_notInit;
            }
        }

        //Calculate any sequence based cleanouts
        RequiredSpanPlusClean totalCleansBefore = RequiredSpanPlusClean.s_notInit;

        //Start with any existing cleanouts that were scheduled at the end of the last block
        if (sequenceVals.Initialized)
        {
            totalCleansBefore = sequenceVals.ScheduledCleanout;
        }

        RequiredSpanPlusClean sequenceCleans = CalculateSequenceCleanoutTime(a_sd.SimClock, a_act, sequenceVals, true, a_sd.ExtensionController, totalCleansBefore.CleanoutGrade);
        totalCleansBefore = totalCleansBefore.Merge(sequenceCleans, true);

        return totalCleansBefore;
    }

    /// <summary>
    /// Calculates the remaining setup time -- this is the duration to be scheduled for setup.
    /// Setup inclusion, production status, and reported setup time are taken into consideration.
    /// The minimum value that this function will return is 0.
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="deductReported">
    /// Whether to deduct reported setup from the total setup required. If called during a simulation this should be yes, otherwise is should be false (for instance when
    /// determining the JIT).
    /// </param>
    /// <param name="useSetupSequencing">
    /// Whether to include sequence dependant setup times. If called during a simulation this should be yes, otherwise it should be false (for instance when determining the
    /// JIT).
    /// </param>
    /// <returns></returns>
    internal RequiredSpanPlusSetup CalculateSetupTime(long a_simClock,
                                                      InternalActivity a_act,
                                                      LeftNeighborSequenceValues a_leftNeighborSequenceValues,
                                                      bool a_useSetupSequencing,
                                                      RequiredSpanPlusSetup a_cachedSetupSpan,
                                                      long a_startTicks,
                                                      ExtensionController a_timeCustomizer)
    {
        //No need to calculate setup since we are in a higher production stage than setting up
        if (a_act.ProductionStatus > InternalActivityDefs.productionStatuses.SettingUp)
        {
            return RequiredSpanPlusSetup.s_notInit;
        }

        InternalOperation operation = (ResourceOperation)a_act.Operation;
        try
        {
            if (a_timeCustomizer.RunRequiredCapacityExtension)
            {
                RequiredSpanPlusSetup? setup = a_timeCustomizer.CalculateSetup(a_act, this, a_startTicks, a_leftNeighborSequenceValues);
                if (setup.HasValue)
                {
                    return setup.Value;
                }
            }
        }
        catch (Exception e)
        {
            CustomizationExceptionHelpers.THROW_RequiredCapacityCutomization_EXCEPTION(e, operation);
        }

        RequiredSpanPlusSetup setupSpan = RequiredSpanPlusSetup.s_notInit;
        decimal setupCost = 0;

        if (a_leftNeighborSequenceValues.Initialized
            && a_act.Operation.PreventSplitsFromIncurringSetup
            && a_leftNeighborSequenceValues.Activity.Operation == a_act.Operation)
        {
            //This operation doesn't incur setup between activities
            return RequiredSpanPlusSetup.s_notInit;
        }

        // Check to see if setup time need to be omitted
        if (!a_leftNeighborSequenceValues.Initialized)
        {
            if (OmitSetupOnFirstActivity || OmitSetupOnFirstActivityInShift)
            {
                return RequiredSpanPlusSetup.s_notInit;
            }
        }
        else if (OmitSetupOnFirstActivityInShift)
        {
            if (ResultantCapacity.Count > 0)
            {
                LinkedListNode<ResourceCapacityInterval> capacityNodeOfLeftNeighbor = ResultantCapacity.FindFirstOnline(a_leftNeighborSequenceValues.EndDate - 1, null);
                if (capacityNodeOfLeftNeighbor != null)
                {
                    ResourceCapacityInterval rci = capacityNodeOfLeftNeighbor.Value;
                    if (rci.EndDate < a_startTicks) //The capacity interval that the left neighbor finished on has ended. So this activity will be the first one on a different capacity interval (IS THIS ALWAYS TRUE?)
                    {
                        return RequiredSpanPlusSetup.s_notInit;
                    }
                }
            }
        }

        ProductionInfo productionInfo = a_act.GetResourceProductionInfo(this);
        bool useSetupOverride = productionInfo.SetupSpanOverride;
        if (useSetupOverride)
        {
            long setupTicks = productionInfo.SetupSpanTicks;
            //Use the override
            setupTicks -= a_act.ReportedSetup;

            bool overrun = setupTicks < 0;
            setupTicks = Math.Max(0, setupTicks);
            RequiredSpanPlusSetup actSetupOverride = new RequiredSpanPlusSetup(setupTicks, overrun, productionInfo.SetupSpanTicks);
            decimal resourceSetupCost = setupTicks > 0 ? ResourceSetupCost : 0;
            actSetupOverride.SetStaticCosts(0, productionInfo.ProductionSetupCost, resourceSetupCost);
            return actSetupOverride;
        }

        if (a_cachedSetupSpan != RequiredSpanPlusSetup.s_notInit)
        {
            setupSpan = a_cachedSetupSpan;
        }
        else
        {
            long remainingSetupToReport = a_act.ReportedSetup;
            RequiredSpanPlusSetup resourceRSP = CalculateResourceSetup(ref remainingSetupToReport);
            setupSpan.MergeSetup(resourceRSP, ConsecutiveSetupTimes);

            RequiredSpanPlusSetup operationRSP = CalculateProductionSetup(a_act, ref remainingSetupToReport, out decimal operationSetupCost);
            setupSpan.MergeSetup(operationRSP, ConsecutiveSetupTimes);

            RequiredSpanPlusSetup sequencedRSP = CalculateSequencedSetup(a_simClock, a_act, a_leftNeighborSequenceValues, a_useSetupSequencing, ref remainingSetupToReport, out decimal sequencedSetupCost);
            setupSpan.MergeSetup(sequencedRSP, ConsecutiveSetupTimes);
            setupSpan.SetOverrun(a_act.ProductionStatus);

            decimal resourceSetupCost = setupSpan.TimeSpanTicks > 0 ? ResourceSetupCost : 0;
            setupSpan.SetStaticCosts(sequencedSetupCost, operationSetupCost, resourceSetupCost);
        }

        try
        {
            RequiredSpanPlusSetup? adjustedSetup = a_timeCustomizer.AdjustSetupTime(a_act, this, a_startTicks, setupSpan, a_leftNeighborSequenceValues);
            if (adjustedSetup.HasValue)
            {
                setupSpan = adjustedSetup.Value;
            }
        }
        catch (Exception e)
        {
            CustomizationExceptionHelpers.THROW_RequiredCapacityCutomization_EXCEPTION(e, operation);
        }

        return setupSpan;
    }

    private RequiredSpanPlusSetup CalculateSequencedSetup(long a_simClock, InternalActivity a_act, LeftNeighborSequenceValues a_leftNeighborSequenceValues, bool a_useSetupSequencing, ref long a_remainingSetupToReport, out decimal o_sequencedSetupCost)
    {
        o_sequencedSetupCost = 0;
        //Infinite and multitasking resources are limited to non-sequence depended setup.
        if (CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            // This setting only applicable to finite capacity resources.
            AttributesCollection previousOpAtts = null;

            if (!CodesClear)
            {
                if (!a_useSetupSequencing)
                {
                    previousOpAtts = null;
                }
                else if (a_leftNeighborSequenceValues.Initialized)
                {
                    previousOpAtts = a_leftNeighborSequenceValues.Activity.Operation.Attributes;
                }
                else
                {
                    previousOpAtts = GetLastRunActivity(a_simClock)?.Operation.Attributes;
                }
            }

            CalculateOperationAttributeChangeovers(previousOpAtts, a_act.Operation.Attributes, PTAttributeDefs.EIncurAttributeType.Setup, out long sequencedSetupTicks, out o_sequencedSetupCost, out int cleanoutGrade, out bool incurResourceSetup);
            
            if (UseSequencedSetupTime || incurResourceSetup)
            {
                sequencedSetupTicks = (long)Math.Ceiling(sequencedSetupTicks / ChangeoverSetupEfficiencyMultiplier);

                if (sequencedSetupTicks > a_remainingSetupToReport)
                {
                    long newSequenceSetupTicks = sequencedSetupTicks - a_remainingSetupToReport;
                    a_remainingSetupToReport = 0;
                    return new RequiredSpanPlusSetup(newSequenceSetupTicks, false, sequencedSetupTicks);
                }
                else
                {
                    //We are reporting a setup span that's at least as long as the sequenced Setup. There won't be any sequenced Setup to schedule.
                    a_remainingSetupToReport -= sequencedSetupTicks;
                }
            }
        }

        //Not applying sequenced setup, we can reset the cost to 0
        o_sequencedSetupCost = 0;
        return RequiredSpanPlusSetup.s_notInit;
    }

    private RequiredSpanPlusSetup CalculateProductionSetup(InternalActivity a_act, ref long a_remainingSetupToReport, out decimal o_productionSetupCost)
    {
        o_productionSetupCost = 0;
        if (!UseOperationSetupTime)
        {
            return RequiredSpanPlusSetup.s_notInit;
        }

        ProductionInfo prodInfo = a_act.GetResourceProductionInfo(this);
        if (prodInfo.SetupSpanTicks > a_remainingSetupToReport)
        {
            o_productionSetupCost = prodInfo.ProductionSetupCost;
            long productionSetupTicks = prodInfo.SetupSpanTicks - a_remainingSetupToReport;
            a_remainingSetupToReport = 0;
            RequiredSpanPlusSetup requiredSpanPlusSetup = new RequiredSpanPlusSetup(productionSetupTicks, false, prodInfo.SetupSpanTicks);
            return requiredSpanPlusSetup;
        }
        else
        {
            //We are reporting a setup span that's at least as long as the Production Setup. There won't be any Production Setup to schedule.
            //Update the remaining reported value so that it can be subtracted by the other Setup Spans.
            a_remainingSetupToReport -= prodInfo.SetupSpanTicks;
        }
        
        return RequiredSpanPlusSetup.s_notInit;
    }

    internal RequiredSpanPlusSetup CalculateResourceSetup(ref long a_remainingSetupToReport)
    {
        if (UseResourceSetupTime)
        {
            if (SetupSpanTicks > a_remainingSetupToReport)
            {
                long resSetupTicks = SetupSpanTicks - a_remainingSetupToReport;
                a_remainingSetupToReport = 0;
                return new RequiredSpanPlusSetup(resSetupTicks, false, SetupSpanTicks);
            }
            else
            {
                //We are reporting a setup span that's at least as long as the Resource Setup. There won't be any Resource Setup to schedule.
                //Update the remaining reported value so that it can be subtracted by the other Setup Spans.
                a_remainingSetupToReport -= SetupSpanTicks;
            }
        }

        return RequiredSpanPlusSetup.s_notInit;
    }

    /// <summary>
    /// Calculates the total cleanout that needs to
    /// </summary>
    internal RequiredSpanPlusClean CalculateSequenceCleanoutTime(long a_simClock,
                                                                 InternalActivity a_act,
                                                                 LeftNeighborSequenceValues a_leftNeighborSequenceValues,
                                                                 bool a_useSetupSequencing,
                                                                 ExtensionController a_timeCustomizer,
                                                                 int a_existingCleanoutGrade)
    {
        //TODO: Clean extensions
        //InternalOperation operation = (ResourceOperation)a_act.Operation;
        //try
        //{
        //    if (a_timeCustomizer.RunRequiredCapacityExtension)
        //    {
        //        RequiredSpanPlusSetup? setup = a_timeCustomizer.CalculateSetup(a_act, this, a_startTicks, a_leftNeighborSetupValues);
        //        if (setup.HasValue)
        //        {
        //            return setup.Value;
        //        }
        //    }
        //}
        //catch (Exception e)
        //{
        //    PT.Scheduler.Simulation.Customizations.CustomizationExceptionHelpers.THROW_RequiredCapacityCutomization_EXCEPTION(e, operation);
        //}

        RequiredSpanPlusClean cleanSpan = RequiredSpanPlusClean.s_notInit;

        // only single tasking resources can make use of all different types of setup.
        if (CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            RequiredSpanPlusClean rspClean = RequiredSpanPlusClean.s_notInit;

            if (UseAttributeCleanouts)
            {
                // This setting only applicable to finite capacity resources.
                AttributesCollection previousOpAtts = null;
                if (!CodesClear && a_useSetupSequencing)
                {
                    if (a_leftNeighborSequenceValues.Initialized)
                    {
                        rspClean = a_leftNeighborSequenceValues.Activity.Batch?.CleanSpan ?? RequiredSpanPlusClean.s_notInit;
                    }

                    if (a_leftNeighborSequenceValues.Activity?.Operation.Id == a_act.Operation.Id && a_act.Operation.PreventSplitsFromIncurringClean)
                    {
                        return cleanSpan;
                    }

                    previousOpAtts = a_leftNeighborSequenceValues.Activity?.Operation.Attributes;
                }

                CalculateOperationAttributeChangeovers(previousOpAtts, a_act.Operation.Attributes, PTAttributeDefs.EIncurAttributeType.Clean, out long cleanTicks, out decimal cleanCost, out int grade, out bool a_incurResourceClean);

                if ((!a_act.BeingMoved && !a_act.MoveIntoBatch))
                {
                    //If the scheduling activity is being moved, we don't want to use a potentially
                    //existing cleanout as it was incurred based on a different sequence
                    if (rspClean != RequiredSpanPlusClean.s_notInit)
                    {
                        if (rspClean.CleanoutGrade > grade)
                        {
                            //Another cleanout was run here.
                            return RequiredSpanPlusClean.s_notInit;
                        }
                        else if (rspClean.CleanoutGrade == grade && rspClean.TimeSpanTicks > cleanTicks)
                        {
                            //Take longer cleanout if grades are the same
                            cleanTicks = rspClean.TimeSpanTicks;
                        }
                    }
                }

                decimal resourceCleanoutCost = cleanTicks > 0 ? ResourceCleanoutCost : 0;
                RequiredSpanPlusClean attributeClean = new (cleanTicks, false, grade);
                attributeClean.SetStaticCosts(cleanCost, 0, resourceCleanoutCost);
                return attributeClean;
            }
        }

        return cleanSpan;
    }

    /// <summary>
    /// Calculates the cleanout time that would need to be scheduled based on the last cleanout and the sim clock
    /// </summary>
    internal RequiredSpanPlusClean CalculateResourceCleanout(long a_simClock,
                                                             InternalActivity a_act,
                                                             RequiredSpanPlusClean a_leftNeighborSequenceValues,
                                                             long a_lastCleanoutEndTime,
                                                             ExtensionController a_timeCustomizer)
    {
        //TODO: Clean extensions
        //InternalOperation operation = (ResourceOperation)a_act.Operation;
        //try
        //{
        //    if (a_timeCustomizer.RunRequiredCapacityExtension)
        //    {
        //        RequiredSpanPlusSetup? setup = a_timeCustomizer.CalculateSetup(a_act, this, a_startTicks, a_leftNeighborSetupValues);
        //        if (setup.HasValue)
        //        {
        //            return setup.Value;
        //        }
        //    }
        //}
        //catch (Exception e)
        //{
        //    PT.Scheduler.Simulation.Customizations.CustomizationExceptionHelpers.THROW_RequiredCapacityCutomization_EXCEPTION(e, operation);
        //}

        if (a_leftNeighborSequenceValues == RequiredSpanPlusClean.s_notInit || a_lastCleanoutEndTime <= a_act.ScenarioDetail.Clock)
        {
            //There was no previous cleanout.
            ResourceBlockList.Node block = Blocks.First;
            while (block != null)
            {
                //TODO: Here we calculate the resource based cleanouts such as time, cycle, product units.

                block = block.Next;
            }
        }

        //TODO: Clean
        return RequiredSpanPlusClean.s_notInit;
    }

    /// <summary>
    /// Calculates the setup time and cost using both the AttributeCodeTable and AttributeNumberTable.
    /// </summary>
    private void CalculateOperationAttributeChangeovers(AttributesCollection a_previousOpAttributes,
                                                        AttributesCollection a_nextOpAttributes, 
                                                        PTAttributeDefs.EIncurAttributeType a_attributeType, 
                                                        out long o_duration, out decimal o_cost, out int o_grade, out bool o_incurResourceSetup)
    {
        o_cost = 0;
        o_grade = 0;
        o_incurResourceSetup = false;

        if (a_nextOpAttributes == null || a_nextOpAttributes.Count == 0)
        {
            o_duration = 0;
            return;
        }

        //Create a hashtable of the prevOpAttributes for quick searching.
        //Hashtable prevOpAttsHash = new Hashtable();
        Dictionary<string, OperationAttribute> prevOpAttsHash = new ();

        if (a_previousOpAttributes != null)
        {
            for (int i = 0; i < a_previousOpAttributes.Count; i++)
            {
                OperationAttribute prevAtt = a_previousOpAttributes[i];
                if (prevAtt.PTAttribute.AttributeType == a_attributeType)
                {
                    //Only include the types of attributes we are calculating for
                    prevOpAttsHash[prevAtt.PTAttribute.Name] = prevAtt;
                }
            }
        }

        if (a_attributeType == PTAttributeDefs.EIncurAttributeType.Setup)
        {
            //Iterate through each of the Attributes in the nextOp collection and get the setup values
            //Need to keep track of concurrent and consecutive separately.
            long setupTimeConcurrent = 0;
            long setupTimeConsecutive = 0;

            for (int i = 0; i < a_nextOpAttributes.Count; i++)
            {
                OperationAttribute nxtAtt = a_nextOpAttributes[i];
                OperationAttribute operationAtt = null;

                if (nxtAtt.PTAttribute.AttributeType != a_attributeType)
                {
                    continue;
                }

                o_incurResourceSetup |= nxtAtt.PTAttribute.IncurResourceSetup;
                prevOpAttsHash.TryGetValue(nxtAtt.PTAttribute.Name, out operationAtt);

                long nxtSetupTimeConcurrent;
                long nxtSetupTimeConsecutive;
                decimal nxtSetupCost;

                CalculateOperationAttributeSetupsHelper(operationAtt, nxtAtt, out nxtSetupTimeConcurrent, out nxtSetupTimeConsecutive, out nxtSetupCost);

                setupTimeConcurrent = Math.Max(setupTimeConcurrent, nxtSetupTimeConcurrent);
                setupTimeConsecutive += nxtSetupTimeConsecutive;
                o_cost += nxtSetupCost;
            }

            o_duration = setupTimeConcurrent + setupTimeConsecutive;
        }
        else if (a_attributeType == PTAttributeDefs.EIncurAttributeType.Clean)
        {
            o_duration = 0;

            for (int i = 0; i < a_nextOpAttributes.Count; i++)
            {
                OperationAttribute nxtAtt = a_nextOpAttributes[i];
                OperationAttribute operationAtt = null;

                if (nxtAtt.PTAttribute.AttributeType != a_attributeType)
                {
                    continue;
                }

                prevOpAttsHash.TryGetValue(nxtAtt.PTAttribute.Name, out operationAtt);

                if (operationAtt == null)
                {
                    //Skip
                    continue;
                }

                CalculateOperationAttributeCleanoutsHelper(operationAtt, nxtAtt, out long nextCleanoutTime, out int nextGrade, out decimal cost);

                if (nextGrade > o_grade)
                {
                    o_duration = nextCleanoutTime;
                    o_grade = nextGrade;
                    o_cost = cost;
                }
                else if (nextGrade == o_grade) //TODO: Maybe order by cost instead of time, or have an option
                {
                    if (nextCleanoutTime > o_duration)
                    {
                        o_duration = nextCleanoutTime;
                        o_cost = cost;
                    }
                }
                //lesser grade is ignored
            }
        }
        else
        {
            o_duration = 0;
        }
    }

    private void CalculateOperationAttributeSetupsHelper(OperationAttribute a_previousOpAttribute, OperationAttribute a_nextOpAttribute, out long o_setupTimeConcurrent, out long o_setupTimeConsecutive, out decimal o_setupCost)
    {
        o_setupTimeConcurrent = 0;
        o_setupTimeConsecutive = 0;
        o_setupCost = 0;

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.Always)
        {
            if (a_nextOpAttribute.PTAttribute.ConsecutiveSetup)
            {
                o_setupTimeConsecutive = a_nextOpAttribute.Duration.Ticks;
            }
            else
            {
                o_setupTimeConcurrent = a_nextOpAttribute.Duration.Ticks;
            }

            o_setupCost = a_nextOpAttribute.Cost;
            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.Never)
        {
            return;
        }


        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.CodeChanges)
        {
            if (a_previousOpAttribute == null || a_previousOpAttribute.Code != a_nextOpAttribute.Code)
            {
                if (a_nextOpAttribute.PTAttribute.ConsecutiveSetup)
                {
                    o_setupTimeConsecutive = a_nextOpAttribute.Duration.Ticks;
                }
                else
                {
                    o_setupTimeConcurrent = a_nextOpAttribute.Duration.Ticks;
                }

                o_setupCost = a_nextOpAttribute.Cost;
            }

            return;
        }


        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.NumberChanges)
        {
            if (a_previousOpAttribute == null || a_previousOpAttribute.Number != a_nextOpAttribute.Number)
            {
                if (a_nextOpAttribute.PTAttribute.ConsecutiveSetup)
                {
                    o_setupTimeConsecutive = a_nextOpAttribute.Duration.Ticks;
                }
                else
                {
                    o_setupTimeConcurrent = a_nextOpAttribute.Duration.Ticks;
                }

                o_setupCost = a_nextOpAttribute.Cost;
            }

            return;
        }

        //All other options require a non-null previousOpAttribute
        if (a_previousOpAttribute == null)
        {
            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.NumberHigher)
        {
            if (a_previousOpAttribute.Number < a_nextOpAttribute.Number)
            {
                if (a_nextOpAttribute.PTAttribute.ConsecutiveSetup)
                {
                    o_setupTimeConsecutive = a_nextOpAttribute.Duration.Ticks;
                }
                else
                {
                    o_setupTimeConcurrent = a_nextOpAttribute.Duration.Ticks;
                }

                o_setupCost = a_nextOpAttribute.Cost;
            }

            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.NumberLower)
        {
            if (a_previousOpAttribute.Number > a_nextOpAttribute.Number)
            {
                if (a_nextOpAttribute.PTAttribute.ConsecutiveSetup)
                {
                    o_setupTimeConsecutive = a_nextOpAttribute.Duration.Ticks;
                }
                else
                {
                    o_setupTimeConcurrent = a_nextOpAttribute.Duration.Ticks;
                }

                o_setupCost = a_nextOpAttribute.Cost;
            }

            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.LookupByCode)
        {
            if (AttributeCodeTable != null)
            {
                AttributeCodeTable.LookupSetup(a_nextOpAttribute.PTAttribute.ExternalId, a_previousOpAttribute.Code, a_nextOpAttribute.Code, out long attSetupTime, out o_setupCost);

                if (a_nextOpAttribute.PTAttribute.ConsecutiveSetup)
                {
                    o_setupTimeConsecutive = attSetupTime;
                }
                else
                {
                    o_setupTimeConcurrent = attSetupTime;
                }
            }

            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.LookupByRange)
        {
            if (a_previousOpAttribute.Number != a_nextOpAttribute.Number)
            {
                if (FromToRanges != null)
                {
                    RangeLookup.FromRangeSet fromRangeSet = FromToRanges.Find(a_nextOpAttribute.PTAttribute.ExternalId);
                    if (fromRangeSet != null)
                    {
                        RangeLookup.FromRange fromRange = fromRangeSet.Find(a_previousOpAttribute.Number);
                        if (fromRange != null)
                        {
                            RangeLookup.ToRange toRange = fromRange.Find(a_nextOpAttribute.Number);
                            if (toRange != null)
                            {
                                if (a_nextOpAttribute.PTAttribute.ConsecutiveSetup)
                                {
                                    o_setupTimeConsecutive = toRange.SetupTicks;
                                }
                                else
                                {
                                    o_setupTimeConcurrent = toRange.SetupTicks;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void CalculateOperationAttributeCleanoutsHelper(OperationAttribute a_previousOpAttribute, OperationAttribute a_nextOpAttribute, out long o_cleanTime, out int o_grade, out decimal o_cost)
    {
        o_cleanTime = 0;
        o_grade = 0;
        o_cost = 0;

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.Always)
        {
            o_cleanTime = a_nextOpAttribute.Duration.Ticks;
            o_cost = a_nextOpAttribute.Cost;
            o_grade = a_nextOpAttribute.PTAttribute.CleanoutGrade;
            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.Never)
        {
            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.CodeChanges)
        {
            if (a_previousOpAttribute == null || a_previousOpAttribute.Code != a_nextOpAttribute.Code)
            {
                o_cleanTime = a_nextOpAttribute.Duration.Ticks;
                o_cost = a_nextOpAttribute.Cost;
                o_grade = a_nextOpAttribute.PTAttribute.CleanoutGrade;
            }

            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.NumberChanges)
        {
            if (a_previousOpAttribute == null || a_previousOpAttribute.Number != a_nextOpAttribute.Number)
            {
                o_cleanTime = a_nextOpAttribute.Duration.Ticks;
                o_cost = a_nextOpAttribute.Cost;
                o_grade = a_nextOpAttribute.PTAttribute.CleanoutGrade;
            }

            return;
        }

        //All other options require a non-null previousOpAttribute
        if (a_previousOpAttribute == null)
        {
            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.NumberHigher)
        {
            if (a_previousOpAttribute.Number < a_nextOpAttribute.Number)
            {
                o_cleanTime = a_nextOpAttribute.Duration.Ticks;
                o_cost = a_nextOpAttribute.Cost;
                o_grade = a_nextOpAttribute.PTAttribute.CleanoutGrade;
            }

            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.NumberLower)
        {
            if (a_previousOpAttribute.Number > a_nextOpAttribute.Number)
            {
                o_cleanTime = a_nextOpAttribute.Duration.Ticks;
                o_cost = a_nextOpAttribute.Cost;
                o_grade = a_nextOpAttribute.PTAttribute.CleanoutGrade;
            }

            return;
        }

        if (a_nextOpAttribute.PTAttribute.AttributeTrigger == PTAttributeDefs.EAttributeTriggerOptions.LookupByCode)
        {
            if (AttributeCodeTable != null)
            {
                AttributeCodeTable.LookupCleanout(a_nextOpAttribute.PTAttribute.ExternalId, a_previousOpAttribute.Code, a_nextOpAttribute.Code, out o_cleanTime, out o_cost, out o_grade);
            }
        }

        //TODO: Do we add another table, or extend the range row?
        //if (a_nextOpAttribute.IncurSetupWhen == PtAttributeDefs.IncurSetupOptions.LookupByRange)
        //{
        //    if (a_previousOpAttribute.Number != a_nextOpAttribute.Number)
        //    {
        //        if (FromToRanges != null)
        //        {
        //            RangeLookup.FromRangeSet fromRangeSet = FromToRanges.Find(a_nextOpAttribute.Name);
        //            if (fromRangeSet != null)
        //            {
        //                RangeLookup.FromRange fromRange = fromRangeSet.Find(a_previousOpAttribute.Number);
        //                if (fromRange != null)
        //                {
        //                    RangeLookup.ToRange toRange = fromRange.Find(a_nextOpAttribute.Number);
        //                    if (toRange != null)
        //                    {
        //                        o_cleanTime = toRange.SetupTicks;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return;
        //}
    }

    /// <summary>
    /// A helper function for calculating the total required capacity.
    /// </summary>
    /// <param name="a_simClock">The current simulation time.</param>
    /// <param name="a_act">The activity whose setup you need to calculate.</param>
    /// <param name="a_leftNeighborSetupVals">Pass in if you already have these values and pass them in, it will reduce the amount of work this function performs. If not, pass in null. </param>
    /// <param name="a_deductReported"></param>
    /// <param name="a_useSetupSequencing"></param>
    /// <param name="a_cachedSetupSpan">Pass in if you already have these values, it will reduce the amount of work this function needs to perform. If not, pass in RequiredSpanPlusSetup.s_notInit.</param>
    /// <param name="a_startDateTicks"></param>
    /// <returns></returns>
    public RequiredCapacityPlus CalculateTotalRequiredCapacityExternally(ScenarioDetail a_sd, InternalActivity a_act, long a_startDateTicks)
    {
        LeftNeighborSequenceValues sequenceValues = CreateDefaultLeftNeighborSequenceValues(a_startDateTicks);

        return CalculateTotalRequiredCapacity(a_sd.SimClock, a_act, sequenceValues, true, a_startDateTicks, a_sd.ExtensionController);
    }   
    
    public RequiredCapacityPlus CalculateTotalRequiredCapacityExternally(ScenarioDetail a_sd, InternalActivity a_act, long a_startDateTicks, decimal a_splitOffQty)
    {
        LeftNeighborSequenceValues sequenceValues = CreateDefaultLeftNeighborSequenceValues(a_startDateTicks);

        if (a_splitOffQty == a_act.RemainingQty || a_act.Operation.AutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.None)
        {
            //No need to split, calculate for the full quantity
            return CalculateTotalRequiredCapacity(a_sd.SimClock, a_act, sequenceValues, true, a_startDateTicks, a_sd.ExtensionController);
        }

        //Split off some quantity to calculate
        ResourceOperation operationCopyForSplit = (ResourceOperation)((ResourceOperation)a_act.Operation).Clone();
        operationCopyForSplit.RestoreReferences(a_act.Operation.ManufacturingOrder.OperationManager, a_act.Operation.ManufacturingOrder, a_sd.WarehouseManager, a_sd.ItemManager, a_sd.SalesOrderManager, a_sd.TransferOrderManager, a_sd.AttributeManager, a_sd.IdGen, a_sd.PlantManager, a_sd.CapabilityManager);

        //Update sim product arrays
        operationCopyForSplit.SimulationInitializationOfActivities(operationCopyForSplit.Activities);

        //Initialize the existing activities, the new activities will be initialized during the Split function
        foreach (InternalActivity internalActivity in operationCopyForSplit.Activities)
        {
            internalActivity.ResetSimulationStateVariables(a_sd);
            internalActivity.SimulationInitialization(a_sd.PlantManager, a_sd.ProductRuleManager, a_sd.ExtensionController, a_sd);
            //Filter out Manual Only resources
            internalActivity.Operation.AdjustedPlantResourceEligibilitySets_Filter(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter.NoFilter());
        }

        operationCopyForSplit.SplitByQtyList(new List<decimal> { a_splitOffQty }, operationCopyForSplit.WholeNumberSplits, new ScenarioDataChanges());
        foreach (InternalActivity internalActivity in operationCopyForSplit.Activities)
        {
            if (internalActivity.RemainingQty == a_splitOffQty)
            {
                return CalculateTotalRequiredCapacity(a_sd.SimClock, internalActivity, sequenceValues, true, a_startDateTicks, a_sd.ExtensionController);
            }
        }

        return null;
    }

    /// <summary>
    /// A helper function for calculating the total required capacity.
    /// </summary>
    /// <param name="a_simClock">The current simulation time.</param>
    /// <param name="a_act">The activity whose setup you need to calculate.</param>
    /// <param name="a_leftNeighborSequenceVals">Pass in if you already have these values and pass them in, it will reduce the amount of work this function performs. If not, pass in null. </param>
    /// <param name="a_useSetupSequencing"></param>
    /// <param name="a_startDateTicks"></param>
    /// <param name="a_timeCustomizer"></param>
    /// <param name="a_cachedCapacity"></param>
    /// a_calculateCleanout is necessary since when calculating JIT, we don't actually need to calculate cleanout since there's no sequencing yet
    /// <returns></returns>
    internal RequiredCapacityPlus CalculateTotalRequiredCapacity(long a_simClock,
                                                                 InternalActivity a_act,
                                                                 LeftNeighborSequenceValues a_leftNeighborSequenceVals,
                                                                 bool a_useSetupSequencing,
                                                                 long a_startDateTicks,
                                                                 ExtensionController a_timeCustomizer,
                                                                 RequiredCapacityPlus a_cachedCapacity = null)
    {
        if (a_cachedCapacity == null)
        {
            a_cachedCapacity = RequiredCapacityPlus.s_notInit;
        }
        
        //Calculate all Cleanouts. These all represent time between the previous activity and this one.
        RequiredSpanPlusClean netCleanSpanBefore = RequiredSpanPlusClean.s_notInit;
        if (a_act.ProductionStatus < InternalActivityDefs.productionStatuses.SettingUp)
        {
            if (a_cachedCapacity.CleanBeforeSpan != RequiredSpanPlusClean.s_notInit)
            {
                netCleanSpanBefore = a_cachedCapacity.CleanBeforeSpan;
            }
            else
            {
                //If it is running, then there is no cleanout needed beforehand, even if it should have been required.
                netCleanSpanBefore = CalculateTotalRequiredCleanout(a_simClock, a_act, a_leftNeighborSequenceVals, a_useSetupSequencing, a_timeCustomizer, a_simClock);
            }
        }

        RequiredSpanPlusSetup setupSpan;
        if (a_act.Operation.ResourceRequirements.RequirementsUseSetup)
        {
            setupSpan = CalculateSetupTime(a_simClock, a_act, a_leftNeighborSequenceVals, a_useSetupSequencing, a_cachedCapacity.SetupSpan, a_startDateTicks, a_timeCustomizer);
        }
        else
        {
            setupSpan = new RequiredSpanPlusSetup(0, false, 0);
        }

        a_act.CalculateProcessingTimeSpan(this, out RequiredSpan processingSpan, out long requiredNumberOfCycles, out decimal requiredQty);

        RequiredSpan postProcessingSpan;
        if (a_act.Operation.ResourceRequirements.RequirementsUsePostProcessing)
        {
            postProcessingSpan = a_act.CalculatePostProcessingSpan(this);
        }
        else
        {
            postProcessingSpan = new RequiredSpan();
        }        
        
        RequiredSpan storageSpan;
        if (a_act.Operation.ResourceRequirements.RequirementsUseStorage)
        {
            storageSpan = a_act.CalculateStorageSpan(this);
        }
        else
        {
            storageSpan = new RequiredSpan();
        }

        RequiredSpanPlusClean netCleanAfterSpan;
        if (a_cachedCapacity.CleanAfterSpan != RequiredSpanPlusClean.s_notInit)
        {
            netCleanAfterSpan = a_cachedCapacity.CleanAfterSpan;
        }
        else
        {
            //Calculate the least potential future cleanout based on all activities in the dispatcher. 
            //This will get the minimum cleanout that could schedule after this activity. This is necessary because there may be a blocking interval
            // that could prevent this from scheduling. If the min cleanout after doesn't fit, don't schedule this activity here.
            netCleanAfterSpan = CalculateMinimumCleanoutAfter(a_simClock, a_act, a_leftNeighborSequenceVals, a_useSetupSequencing, a_timeCustomizer, false);
        }

        RequiredCapacityPlus rc = new (
            netCleanSpanBefore,
            setupSpan,
            processingSpan,
            postProcessingSpan,
            storageSpan,
            netCleanAfterSpan,
            requiredNumberOfCycles,
            requiredQty);

        ResourceOperation operation = (ResourceOperation)a_act.Operation;
        if (a_timeCustomizer != null)
        {
            try
            {
                RequiredCapacityPlus? updatedRc = a_timeCustomizer.AfterRequiredCapacityCalculation(a_act, this, rc, a_startDateTicks);
                if (updatedRc != null)
                {
                    rc = updatedRc;
                }
            }
            catch (Exception e)
            {
                CustomizationExceptionHelpers.THROW_RequiredCapacityCutomization_EXCEPTION(e, operation);
            }
        }

        return rc;
    }

    /// <summary>
    /// Calculates total clean required between the previous block and an activity
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_act">The subsequent activity</param>
    /// <param name="a_leftNeighborSequenceVals">The previous activity block values</param>
    /// <param name="a_useSetupSequencing"></param>
    /// <param name="a_timeCustomizer"></param>
    /// <param name="a_postProcessingEndDate">If available, this is the post processing end from a calculated SI</param>
    /// <returns></returns>
    public RequiredSpanPlusClean CalculateTotalRequiredCleanout(long a_simClock, InternalActivity a_act, LeftNeighborSequenceValues a_leftNeighborSequenceVals, bool a_useSetupSequencing, ExtensionController a_timeCustomizer, long a_postProcessingEndDate)
    {
        RequiredSpanPlusClean netCleanSpanBefore = RequiredSpanPlusClean.s_notInit;
        
        //First check if we even need to calculate cleanout
        if (!a_leftNeighborSequenceVals.Initialized || a_simClock > a_act.ScenarioDetail.EndOfPlanningHorizon)
        {
            return netCleanSpanBefore;
        }

        ProductionInfo productionInfo = a_leftNeighborSequenceVals.Activity.GetResourceProductionInfo(this);
        if (productionInfo.CleanSpanOverride)
        {
            decimal resourceCleanoutCost = productionInfo.CleanSpanTicks > 0 ? productionInfo.CleanoutCost : 0;
            netCleanSpanBefore = new RequiredSpanPlusClean(productionInfo.CleanSpanTicks, false, productionInfo.CleanoutGrade);
            netCleanSpanBefore.SetStaticCosts(0, productionInfo.CleanoutCost, resourceCleanoutCost);
        }
        else
        {

            if (a_leftNeighborSequenceVals.Activity is InternalActivity leftNeighborAct)
            {
                if (!ValidateLeftNeighborForClean(a_act, leftNeighborAct))
                {
                    return RequiredSpanPlusClean.s_notInit;
                }

                netCleanSpanBefore = CalculateStandardCleanSpan(a_leftNeighborSequenceVals.Activity);
            }
            else
            {
                if (a_act.Operation.PreventSplitsFromIncurringClean && a_act.Operation.Id == GetLastRunActivity(a_simClock)?.Operation.Id)
                {
                    return RequiredSpanPlusClean.s_notInit;
                }
            }

            List<LeftActivityInfo> previousActivitiesOrdered = GetOrderedPreviousActivityInfos(a_leftNeighborSequenceVals.BlockNode, a_simClock);

            //Calculate any sequence based cleanouts
            RequiredSpanPlusClean opBasedClean = CalculateResourceOperationCountCleansBefore(previousActivitiesOrdered, netCleanSpanBefore.CleanoutGrade, a_act);
            netCleanSpanBefore = netCleanSpanBefore.Merge(opBasedClean, true);

            RequiredSpanPlusClean sequencedCleans = CalculateSequenceCleanoutTime(a_simClock, a_act, a_leftNeighborSequenceVals, a_useSetupSequencing, a_timeCustomizer, netCleanSpanBefore.CleanoutGrade);
            netCleanSpanBefore = netCleanSpanBefore.Merge(sequencedCleans, true);

            RequiredSpanPlusClean timeBasedClean = CalculateResourceTimeBasedCleans(previousActivitiesOrdered, a_postProcessingEndDate, netCleanSpanBefore.CleanoutGrade, null, out _);
            netCleanSpanBefore = netCleanSpanBefore.Merge(timeBasedClean, true);

            RequiredSpanPlusClean productionBasedClean = CalculateResourceProductionUnitCleans(previousActivitiesOrdered, a_act.ScenarioDetail.ScenarioOptions, netCleanSpanBefore.CleanoutGrade, a_act);
            netCleanSpanBefore = netCleanSpanBefore.Merge(productionBasedClean, true);
        }

        //We have determined that some Clean has to be merged to the previous Activity. We need to deduct
        //any possible Reported Clean if it's in Cleaning
        if (a_leftNeighborSequenceVals.Activity.ProductionStatus >= InternalActivityDefs.productionStatuses.Cleaning)
        {
            if (netCleanSpanBefore.TimeSpanTicks > a_leftNeighborSequenceVals.Activity.ReportedClean)
            {
                RequiredSpanPlusClean cleanoutMinusReportedSpan = new RequiredSpanPlusClean(netCleanSpanBefore.TimeSpanTicks - a_leftNeighborSequenceVals.Activity.ReportedClean, false, netCleanSpanBefore.CleanoutGrade);
                cleanoutMinusReportedSpan.SetActivityToIncurKey(a_leftNeighborSequenceVals.Activity);
                cleanoutMinusReportedSpan.SetStaticCosts(cleanoutMinusReportedSpan.SequencedCleanoutCost, cleanoutMinusReportedSpan.ProductionCleanoutCost, cleanoutMinusReportedSpan.ResourceCleanoutCost);
                return cleanoutMinusReportedSpan;
            }
            else
            {
                //overrun, no cost incurred
                return new RequiredSpanPlusClean(netCleanSpanBefore.TimeSpanTicks, true, netCleanSpanBefore.CleanoutGrade);
            }
        }

        if (netCleanSpanBefore.TimeSpanTicks > 0)
        {
            netCleanSpanBefore.SetActivityToIncurKey(a_leftNeighborSequenceVals.Activity);
        }

        return netCleanSpanBefore;
    }

    /// <summary>
    /// Get a list of previously scheduled Activities merged with the Actuals in descending order of Start Date
    /// </summary>
    /// <param name="a_previousBlockNode"></param>
    /// <param name="a_simClock"></param>
    /// <returns></returns>
    private List<LeftActivityInfo> GetOrderedPreviousActivityInfos(ResourceBlockList.Node a_previousBlockNode, long a_simClock)
    {
        List<LeftActivityInfo> previousActivities = new();
        ResourceBlockList.Node blockNode = a_previousBlockNode;
        while (blockNode != null)
        {
            LeftActivityInfo leftActInfo = new LeftActivityInfo(blockNode.Data.Batch.FirstActivity);
            previousActivities.Add(leftActInfo);
            blockNode = blockNode.Previous;
        }

        foreach (InternalActivity act in m_cleanoutHistoryData.GetActivityEnumerator())
        {
            LeftActivityInfo leftActInfo = new LeftActivityInfo(act);
            //Activities can be reported finished out of order, we don't want to include activities that were finished in the future
            if (leftActInfo.StartTicks < a_simClock)
            {
                previousActivities.Add(leftActInfo);
            }
        }

        List<LeftActivityInfo> previousActivitiesOrdered = previousActivities.OrderByDescending(a => a.StartTicks).ToList();
        return previousActivitiesOrdered;
    }

    private bool ValidateLeftNeighborForClean(InternalActivity a_act, InternalActivity a_leftNeighborAct)
    {
        if (a_act.Operation.PreventSplitsFromIncurringClean && a_act.Operation.Id == a_leftNeighborAct.Operation.Id)
        {
            return false;
        }

        ResourceRequirement rr;
        if (a_leftNeighborAct.Batch is Batch scheduledBatch)
        {
            ResourceBlockList.Node leftNeighborNode = scheduledBatch.BlockForResource(this);
            rr = a_leftNeighborAct.Operation.ResourceRequirements.GetByIndex(leftNeighborNode.Data.ResourceRequirementIndex);
        }
        else if (a_leftNeighborAct.ProductionStatus == InternalActivityDefs.productionStatuses.Finished)
        {
            //previous activity was finished, it should have included the cleanout already.
            return false;
        }
        else
        {
            //Assume the primary, for example in MaxDelay the future sequence may not be scheduled
            rr = a_leftNeighborAct.Operation.ResourceRequirements.PrimaryResourceRequirement;
        }

        if (rr.UsageEnd != MainResourceDefs.usageEnum.Clean)
        {
            //The previous block does not run cleans, so there is nothing to calculate here.
            return false;
        }

        return true;
    }

    /// <summary>
    /// Calculates total clean required between the previous block and an activity
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_act">The subsequent activity</param>
    /// <param name="a_leftNeighborSequenceVals"></param>
    /// <param name="a_useSetupSequencing"></param>
    /// <param name="a_timeCustomizer"></param>
    /// <param name="a_horizonCleanout"></param>
    /// <returns></returns>
    public RequiredSpanPlusClean CalculateMinimumCleanoutAfter(long a_simClock, InternalActivity a_act, LeftNeighborSequenceValues a_leftNeighborSequenceVals, bool a_useSetupSequencing, ExtensionController a_timeCustomizer, bool a_horizonCleanout)
    {
        //First check if we even need to calculate cleanout
        if (a_simClock > a_act.ScenarioDetail.EndOfPlanningHorizon 
            || (a_act.Operation.PreventSplitsFromIncurringClean && !a_act.Operation.SchedulingFinalActivity))
        {
            return RequiredSpanPlusClean.s_notInit;
        }

        ResourceRequirement rr = a_act.Operation.ResourceRequirements.PrimaryResourceRequirement;

        if (rr.UsageEnd != MainResourceDefs.usageEnum.Clean)
        {
            //The previous block does not run cleans, so there is nothing to calculate here.
            return RequiredSpanPlusClean.s_notInit;
        }

        RequiredSpanPlusClean netCleanSpanAfter = RequiredSpanPlusClean.s_notInit;
        
        ProductionInfo productionInfo = a_act.GetResourceProductionInfo(this);
        if (productionInfo.CleanSpanOverride)
        {
            decimal resourceCleanoutCost = productionInfo.CleanSpanTicks > 0 ? productionInfo.CleanoutCost : 0;
            netCleanSpanAfter = new RequiredSpanPlusClean(productionInfo.CleanSpanTicks, false, productionInfo.CleanoutGrade);
            netCleanSpanAfter.SetStaticCosts(0, productionInfo.CleanoutCost, resourceCleanoutCost);
        }
        else
        {
            netCleanSpanAfter =  CalculateStandardCleanSpan(a_act);

            //Calculate any sequence based cleanouts
            List<LeftActivityInfo> previousActivityCleanoutInfos = GetOrderedPreviousActivityInfos(a_leftNeighborSequenceVals.BlockNode, a_simClock);
            RequiredSpanPlusClean opBasedClean = CalculateResourceOperationCountCleansAfter(previousActivityCleanoutInfos, netCleanSpanAfter.CleanoutGrade, a_act.Operation);
            netCleanSpanAfter = netCleanSpanAfter.Merge(opBasedClean, false);

            if (a_horizonCleanout)
            {
                RequiredSpanPlusClean timeBasedCleanout = CalculateResourceTimeBasedCleans(previousActivityCleanoutInfos, a_act.Batch.PostProcessingEndTicks, netCleanSpanAfter.CleanoutGrade, null, out _);
                netCleanSpanAfter = netCleanSpanAfter.Merge(timeBasedCleanout, false);
            }
        }

        //Check for reported Clean
        if (a_act.ProductionStatus == InternalActivityDefs.productionStatuses.Cleaning)
        {
            if (netCleanSpanAfter.TimeSpanTicks >= a_act.ReportedClean)
            {
                long remainingCleanTicks = netCleanSpanAfter.TimeSpanTicks - a_act.ReportedClean;
                RequiredSpanPlusClean cleanoutMinusReportedSpan = new RequiredSpanPlusClean(remainingCleanTicks, remainingCleanTicks <= 0, netCleanSpanAfter.CleanoutGrade);
                cleanoutMinusReportedSpan.SetStaticCosts(cleanoutMinusReportedSpan.SequencedCleanoutCost, cleanoutMinusReportedSpan.ProductionCleanoutCost, cleanoutMinusReportedSpan.ResourceCleanoutCost);
                return cleanoutMinusReportedSpan;
            }
            else
            {
                //Overrun , not cost incurred
                return new RequiredSpanPlusClean(netCleanSpanAfter.TimeSpanTicks, true, netCleanSpanAfter.CleanoutGrade);
            }
        }

        return netCleanSpanAfter;

        //TODO: Best attempts at determining future cleanouts
        List<InternalActivity> potentialNextActivities = new ();
        if (a_act.Sequenced && a_act.Sequential[0].SequencedSimulationResource == this)
        {
            if (a_act.Sequential[0].RightSequencedBatchNeighbor?.FirstActivity is InternalActivity rightSequenceAct)
            {
                potentialNextActivities.Add(rightSequenceAct);
            }
        }
        else
        {
            potentialNextActivities.AddRange(Dispatcher);
        }

        foreach (InternalActivity dispatcherAct in potentialNextActivities)
        {
            if (dispatcherAct == a_act)
            {
                continue;
            }

            
            //LeftNeighborSequenceValues sequenceValues = new LeftNeighborSequenceValues(a_act, a_simClock); //TODO: Is SimClock value work here?

            //RequiredSpanPlusClean sequencedCleans = CalculateSequenceCleanoutTime(a_simClock, dispatcherAct, sequenceValues, a_useSetupSequencing, RequiredSpanPlusClean.s_notInit, a_timeCustomizer, netCleanSpanAfter.CleanoutGrade);
            //netCleanSpanAfter = netCleanSpanAfter.Merge(sequencedCleans, false);

            //RequiredSpanPlusClean timeBasedClean = CalculateResourceTimeBasedCleans(a_leftNeighborSequenceVals?.BlockNode, a_simClock, netCleanSpanBefore.CleanoutGrade);
            //netCleanSpanBefore = netCleanSpanBefore.Merge(timeBasedClean, false);

            //RequiredSpanPlusClean productionBasedClean = CalculateResourceProductionUnitCleans(a_leftNeighborSequenceVals?.BlockNode, a_act.ScenarioDetail.ScenarioOptions, netCleanSpanBefore.CleanoutGrade, a_act);
            //netCleanSpanBefore = netCleanSpanBefore.Merge(productionBasedClean, false);
        }


        return netCleanSpanAfter;
    }

    internal RequiredSpanPlusClean TimeBasedCleanoutRequired(SchedulableInfo a_si, long a_simClock, out long triggerValueUsed)
    {
        triggerValueUsed = 0;
        LeftNeighborSequenceValues leftNeighborSequenceValues = CreateDefaultLeftNeighborSequenceValues(a_simClock);
        List<LeftActivityInfo> previousActivitiesOrdered = GetOrderedPreviousActivityInfos(leftNeighborSequenceValues.BlockNode, a_simClock);
        RequiredSpanPlusClean rspClean = CalculateResourceTimeBasedCleans(previousActivitiesOrdered, a_si.m_postProcessingFinishDate, 0, a_si, out triggerValueUsed);
        return rspClean;
    }

    /// <summary>
    /// Calculate resource time based cleanouts using the Time Cleanout Trigger Table
    /// </summary>
    /// <param name="a_previousActivities"></param>
    /// <param name="a_postProcessingEndDate">The scheduling activity's end of post processing</param>
    /// <param name="a_existingCleanoutGrade"></param>
    /// <param name="a_schedulingInfoForCurrentAct"></param>
    /// <param name="o_triggerValueUsed"></param>
    /// <returns></returns>
    private RequiredSpanPlusClean CalculateResourceTimeBasedCleans(List<LeftActivityInfo> a_previousActivities, long a_postProcessingEndDate, int a_existingCleanoutGrade, SchedulableInfo a_schedulingInfoForCurrentAct, out long o_triggerValueUsed)
    {
        o_triggerValueUsed = 0;
        RequiredSpanPlusClean totalResourceCleanSpan = RequiredSpanPlusClean.s_notInit;
        if (TimeCleanoutTriggerTable != null && (a_previousActivities.Count > 0 || a_schedulingInfoForCurrentAct != null))
        {
            foreach (TimeCleanoutTriggerTableRow trigger in TimeCleanoutTriggerTable)
            {
                int cleanoutGrade = trigger.CleanoutGrade;

                if (cleanoutGrade < a_existingCleanoutGrade)
                {
                    //A higher grade cleanout is already running, no need to calculate this one
                    continue;
                }

                bool isFirstBlock = true;
                long lastCleanoutGradeRunTime = 0;
                long processingTime = 0;
                long postProcessingTime = 0;
                RequiredSpanPlusClean calculatedCleanSpan = RequiredSpanPlusClean.s_notInit;

                bool cleanoutFound = false;
                bool triggered = false;

                if (a_schedulingInfoForCurrentAct != null)
                {
                    //Auto-splitting: we need to check if the total run/postprocessing time of the current activity triggers a cleanout
                    if (!trigger.UseProcessingTime && !trigger.UsePostprocessingTime)
                    {
                        if (a_schedulingInfoForCurrentAct.m_postProcessingFinishDate - a_schedulingInfoForCurrentAct.m_scheduledStartDate > trigger.TriggerValue.Ticks)
                        {
                            triggered = true;
                        }
                    }
                    else
                    {
                        if (trigger.UseProcessingTime)
                        {
                            processingTime = a_schedulingInfoForCurrentAct.m_ocp.ProductionProfile.CapacityFound;
                        }

                        if (trigger.UsePostprocessingTime)
                        {
                            postProcessingTime = a_schedulingInfoForCurrentAct.m_ocp.PostprocessingProfile.CapacityFound;
                        }

                        if (processingTime + postProcessingTime >= trigger.TriggerValue.Ticks)
                        {
                            triggered = true;
                        }
                    }

                    if (triggered)
                    {
                        o_triggerValueUsed = trigger.TriggerValue.Ticks;
                        decimal resourceCleanoutCost = trigger.Duration.Ticks > 0 ? ResourceCleanoutCost : 0;
                        calculatedCleanSpan = new RequiredSpanPlusClean(trigger.Duration.Ticks, false, trigger.CleanoutGrade);
                        calculatedCleanSpan.SetStaticCosts(trigger.CleanCost, 0, resourceCleanoutCost);

                        return calculatedCleanSpan;
                    }
                }

                foreach (LeftActivityInfo previousActInfo in a_previousActivities)
                {
                    RequiredSpanPlusClean rspClean = previousActInfo.GetCleanSpan();

                    if (rspClean.CleanoutGrade > cleanoutGrade)
                    {
                        //Another cleanout was run here.
                        break;
                    }
                    else if (rspClean.CleanoutGrade == cleanoutGrade)
                    {
                        if (!isFirstBlock || rspClean.TimeSpanTicks >= trigger.Duration.Ticks)
                        {
                            break;
                        }
                    }

                    isFirstBlock = false;

                    //The first block on a resource may be partially finished. Check the reported start date and use it if it's set.
                    long startOfBlock = previousActInfo.StartTicks;
                    long endOfBlock = previousActInfo.EndTicks;

                    if (trigger.TriggerAtEnd)
                    {
                        lastCleanoutGradeRunTime = endOfBlock;
                    }
                    else
                    {
                        lastCleanoutGradeRunTime = startOfBlock;
                    }

                    if (!trigger.UseProcessingTime && !trigger.UsePostprocessingTime)
                    {
                        //This trigger is just calendar time
                        if (a_postProcessingEndDate - lastCleanoutGradeRunTime >= trigger.TriggerValue.Ticks)
                        {
                            //Triggered
                            triggered = true;
                        }
                    }
                    else
                    {
                        //Duration based trigger
                        processingTime += previousActInfo.ProcessingSpanTicks;
                        postProcessingTime += previousActInfo.PostProcessingSpanTicks;

                        long totalDuration = 0;
                        if (trigger.UseProcessingTime)
                        {
                            totalDuration += processingTime;
                        }
                        if (trigger.UsePostprocessingTime)
                        {
                            totalDuration += postProcessingTime;
                        }

                        if (totalDuration >= trigger.TriggerValue.Ticks)
                        {
                            //Triggered
                            triggered = true;
                        }
                    }

                    if (triggered)
                    {
                        o_triggerValueUsed = trigger.TriggerValue.Ticks;
                        decimal resourceCleanoutCost = trigger.Duration.Ticks > 0 ? ResourceCleanoutCost : 0;
                        calculatedCleanSpan = new RequiredSpanPlusClean(trigger.Duration.Ticks, false, trigger.CleanoutGrade);
                        calculatedCleanSpan.SetStaticCosts(trigger.CleanCost, 0, resourceCleanoutCost);
                        break;
                    }
                    else if (Blocks.Count == 0 && !trigger.UsePostprocessingTime && !trigger.UseProcessingTime)
                    {
                        //If this is the first block scheduling and not triggering a time based clean, record when the last time one was run so that we
                        //can add an event if this op schedules.
                        if (m_sortedTimeBasedCleanoutTriggers.TryGetValue(trigger.CleanoutGrade, out TimeBasedCleanoutTriggerInfo triggerInfo))
                        {
                            triggerInfo.LastCleanoutRunTime = lastCleanoutGradeRunTime;
                        }
                    }
                }

                //Merge last trigger with total
                totalResourceCleanSpan = totalResourceCleanSpan.Merge(calculatedCleanSpan, true);
            }
        }

        return totalResourceCleanSpan;
    }

    /// <summary>
    /// Calculate resource time based cleanouts using the Operation Count Trigger Table
    /// This cleanout is calculated for the activity clean phase, not for the previous batch
    /// </summary>
    /// <param name="a_previousActInfos"></param>
    /// <param name="a_existingCleanoutGrade"></param>
    /// <param name="a_currentOp"></param>
    /// <returns></returns>
    private RequiredSpanPlusClean CalculateResourceOperationCountCleansBefore(List<LeftActivityInfo> a_previousActInfos, int a_existingCleanoutGrade, InternalActivity a_schedulingActivity)
    {
        RequiredSpanPlusClean totalResourceCleanSpan = RequiredSpanPlusClean.s_notInit;
        if (OperationCountCleanoutTriggerTable != null)
        {
            foreach (OperationCountCleanoutTriggerTableRow trigger in OperationCountCleanoutTriggerTable)
            {
                int cleanoutGrade = trigger.CleanoutGrade;

                if (cleanoutGrade < a_existingCleanoutGrade)
                {
                    //A higher grade cleanout is already running, no need to calculate this one
                    continue;
                }

                long operationCountSinceLastCleanout = 0; //Start at one to include this operation
                bool triggered = false;
                bool isFirstBlock = true;
                InternalOperation currentOp = a_schedulingActivity.Operation;

                if (a_previousActInfos.Count == 0)
                {
                    //first op is attempting to schedule
                    if (operationCountSinceLastCleanout >= trigger.TriggerValue)
                    {
                        triggered = true;
                    }
                }
                else
                {
                    foreach (LeftActivityInfo previousAct in a_previousActInfos)
                    {
                        //For Split Ops don't count consecutively scheduled blocks if PreventSplitsFromIncurringClean is enabled
                        InternalOperation previousOp = previousAct.GetOperation();
                        if (previousOp.Id == currentOp.Id && currentOp.PreventSplitsFromIncurringClean)
                        {
                            currentOp = previousOp;
                            continue;
                        }

                        RequiredSpanPlusClean rspClean = previousAct.GetCleanSpan();

                        if (rspClean.CleanoutGrade > cleanoutGrade)
                        {
                            //Another cleanout was run here.
                            break;
                        }
                        else if (rspClean.CleanoutGrade == cleanoutGrade)
                        {
                            if (!isFirstBlock || rspClean.TimeSpanTicks >= trigger.Duration.Ticks)
                            {
                                break;
                            }
                        }

                        isFirstBlock = false;

                        operationCountSinceLastCleanout++;

                        if (operationCountSinceLastCleanout >= trigger.TriggerValue)
                        {
                            triggered = true;
                            break;
                        }

                        currentOp = previousOp;
                    }
                }

                RequiredSpanPlusClean calculatedCleanSpan = RequiredSpanPlusClean.s_notInit;
                if (triggered)
                {
                    decimal resourceCleanoutCost = trigger.Duration.Ticks > 0 ? ResourceCleanoutCost : 0;
                    calculatedCleanSpan = new RequiredSpanPlusClean(trigger.Duration.Ticks, false, trigger.CleanoutGrade);
                    calculatedCleanSpan.SetStaticCosts(trigger.CleanCost, 0, resourceCleanoutCost);
                }

                //Merge last trigger with total
                totalResourceCleanSpan = totalResourceCleanSpan.Merge(calculatedCleanSpan, true);
            }
        }

        return totalResourceCleanSpan;
    }

    /// <summary>
    /// Calculate resource time based cleanouts using the Operation Count Trigger Table
    /// This cleanout is calculated for the activity clean phase, not for the previous batch
    /// </summary>
    /// <param name="a_previousActInfos"></param>
    /// <param name="a_existingCleanoutGrade"></param>
    /// <param name="a_currentOp"></param>
    /// <returns></returns>
    private RequiredSpanPlusClean CalculateResourceOperationCountCleansAfter(List<LeftActivityInfo> a_previousActInfos, int a_existingCleanoutGrade, InternalOperation a_currentOp)
    {
        RequiredSpanPlusClean totalResourceCleanSpan = RequiredSpanPlusClean.s_notInit;
        if (OperationCountCleanoutTriggerTable != null)
        {
            foreach (OperationCountCleanoutTriggerTableRow trigger in OperationCountCleanoutTriggerTable)
            {
                int cleanoutGrade = trigger.CleanoutGrade;

                if (cleanoutGrade < a_existingCleanoutGrade)
                {
                    //A higher grade cleanout is already running, no need to calculate this one
                    continue;
                }

                bool triggered = false;
                int operationsUntilTrigger = trigger.TriggerValue;

                if (operationsUntilTrigger == 1)
                {
                    //Trigger every time
                    triggered = true;
                }
                else
                {
                    InternalOperation currentOp = a_currentOp;
                    foreach (LeftActivityInfo previousActInfo in a_previousActInfos)
                    {
                        InternalOperation previousOp = previousActInfo.GetOperation();
                        if (previousOp.Id == currentOp.Id && currentOp.PreventSplitsFromIncurringClean)
                        {
                            currentOp = previousOp;
                            continue;
                        }

                        RequiredSpanPlusClean rspClean = previousActInfo.GetCleanSpan();

                        if (rspClean.CleanoutGrade > cleanoutGrade)
                        {
                            break;
                        }
                        else if (rspClean.CleanoutGrade == cleanoutGrade)
                        {
                            if (rspClean.TimeSpanTicks >= trigger.Duration.Ticks)
                            {
                                break;
                            }
                        }

                        operationsUntilTrigger--;

                        if (operationsUntilTrigger == 0)
                        {
                            triggered = true;
                            break;
                        }

                        currentOp = previousOp;
                    }
                }

                RequiredSpanPlusClean calculatedCleanSpan = RequiredSpanPlusClean.s_notInit;
                if (triggered && !(a_currentOp.PreventSplitsFromIncurringClean && !a_currentOp.SchedulingFinalActivity))
                {
                    decimal resourceCleanoutCost = trigger.Duration.Ticks > 0 ? ResourceCleanoutCost : 0;
                    calculatedCleanSpan = new RequiredSpanPlusClean(trigger.Duration.Ticks, false, trigger.CleanoutGrade);
                    calculatedCleanSpan.SetStaticCosts(trigger.CleanCost, 0, resourceCleanoutCost);
                }

                //Merge last trigger with total
                totalResourceCleanSpan = totalResourceCleanSpan.Merge(calculatedCleanSpan, true);
            }
        }

        return totalResourceCleanSpan;
    }

    /// <summary>
    /// Calculate resource cleanouts using the Time Cleanout Trigger Table
    /// </summary>
    /// <param name="a_roundingOptions"></param>
    /// <param name="a_existingCleanoutGrade"></param>
    /// <param name="a_previousActInfos"></param>
    /// <param name="a_productionAct"></param>
    /// <returns></returns>
    private RequiredSpanPlusClean CalculateResourceProductionUnitCleans(List<LeftActivityInfo> a_previousActInfos, ScenarioOptions a_roundingOptions, int a_existingCleanoutGrade, InternalActivity a_productionAct)
    {
        RequiredSpanPlusClean totalResourceCleanSpan = RequiredSpanPlusClean.s_notInit;
        if (ProductionUnitsCleanoutTriggerTable != null && a_previousActInfos.Count > 0)
        {
            foreach (ProductionUnitsCleanoutTriggerTableRow trigger in ProductionUnitsCleanoutTriggerTable)
            {
                int cleanoutGrade = trigger.CleanoutGrade;

                if (cleanoutGrade < a_existingCleanoutGrade)
                {
                    //A higher grade cleanout is already running, no need to calculate this one
                    continue;
                }

                decimal unitsSinceLastCleanout;
                switch (trigger.ProductionUnit)
                {
                    case CleanoutDefs.EProductionUnitsCleanType.Cycles:
                        //This should be a whole number since we don't schedule partial cycles
                        unitsSinceLastCleanout = a_productionAct.RemainingCycles;
                        break;
                    case CleanoutDefs.EProductionUnitsCleanType.ProductUnits:
                        unitsSinceLastCleanout = a_productionAct.PrimaryProductQty;
                        break;
                    case CleanoutDefs.EProductionUnitsCleanType.Quantity:
                        unitsSinceLastCleanout = a_productionAct.RemainingQty;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                bool triggered = false;
                bool isFirstBlock = true;

                foreach (LeftActivityInfo previousActInfo in a_previousActInfos)
                {
                    RequiredSpanPlusClean rspClean = previousActInfo.GetCleanSpan();

                    if (rspClean.CleanoutGrade > cleanoutGrade)
                    {
                        //Another cleanout was run here.
                        break;
                    }
                    else if (rspClean.CleanoutGrade == cleanoutGrade)
                    {
                        if (!isFirstBlock || rspClean.TimeSpanTicks >= trigger.Duration.Ticks)
                        {
                            break;
                        }
                    }

                    isFirstBlock = false;

                    switch (trigger.ProductionUnit)
                    {
                        case CleanoutDefs.EProductionUnitsCleanType.Cycles:
                            //This should be a whole number since we don't schedule partial cycles
                            unitsSinceLastCleanout += a_roundingOptions.RoundQty(previousActInfo.RemainingQty / previousActInfo.QtyPerCycle);
                            break;
                        case CleanoutDefs.EProductionUnitsCleanType.ProductUnits:
                            unitsSinceLastCleanout += previousActInfo.PrimaryProductQty;
                            break;
                        case CleanoutDefs.EProductionUnitsCleanType.Quantity:
                            unitsSinceLastCleanout += previousActInfo.RemainingQty; //The scheduled qty
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (unitsSinceLastCleanout >= trigger.TriggerValue)
                    {
                        triggered = true;
                        break;
                    }
                }

                RequiredSpanPlusClean calculatedCleanSpan = RequiredSpanPlusClean.s_notInit;
                if (triggered)
                {
                    decimal resourceCleanoutCost = trigger.Duration.Ticks > 0 ? ResourceCleanoutCost : 0;
                    calculatedCleanSpan = new RequiredSpanPlusClean(trigger.Duration.Ticks, false, trigger.CleanoutGrade);
                    calculatedCleanSpan.SetStaticCosts(trigger.CleanCost, 0, resourceCleanoutCost);
                }

                //Merge last trigger with total
                totalResourceCleanSpan = totalResourceCleanSpan.Merge(calculatedCleanSpan, true);
            }
        }

        return totalResourceCleanSpan;
    }

    internal ResourceBlockList.Node FindBlock(ResourceBlock a_block)
    {
        ResourceBlockList.Node current = m_blocks.First;

        while (true)
        {
            if (current.Data == a_block)
            {
                return current;
            }

            current = current.Next;
        }
    }

    internal ResourceBlock GetBlockAt(long a_date)
    {
        ResourceBlockList.Node current = m_blocks.First;

        while (current != null)
        {
            ResourceBlock block = current.Data;

            if (a_date >= block.StartTicks && a_date < block.EndTicks)
            {
                return block;
            }

            if (block.StartTicks > a_date)
            {
                return null;
            }

            current = current.Next;
        }

        return null;
    }

    /// <summary>
    /// Find the first block that starts and finishes before a date or null if there's not block.
    /// </summary>
    /// <param name="a_ticks">The date to search for.</param>
    /// <param name="a_actBatch"></param>
    /// <returns></returns>
    internal ResourceBlock FindBlockBefore(long a_ticks, Batch a_actBatch)
    {
        ResourceBlockList.Node cur = m_blocks.First;
        ResourceBlockList.Node prev = null;
        while (cur != null)
        {
            if (Interval.Contains(cur.Data.StartTicks, cur.Data.EndTicks, a_ticks) || Interval.GreaterOrEqual(cur.Data.StartTicks, cur.Data.EndTicks, a_ticks))
            {
                // Return the left neighbor of the first block that contains or is greater than the search date.
                if (prev != null)
                {
                    return prev.Data;
                }

                return null;
            }

            prev = cur;
            cur = cur.Next;
        }

        if (prev != null && prev.Data.Batch != a_actBatch)
        {
            return prev.Data;
        }

        return null;
    }

    #region Simulation
    internal struct TimeBasedCleanoutTriggerInfo
    {
        internal long CleanoutDurationTicks;
        internal long LastCleanoutRunTime;
    }

    private readonly SortedList<int, TimeBasedCleanoutTriggerInfo> m_sortedTimeBasedCleanoutTriggers = new();
    private bool m_reservedForResourceRequirement;

    /// <summary>
    /// This is a state variable used by the simulation algorithm.
    /// In the process of scheduling operations this is set to true when a resource requirement
    /// reserves this resource to satisfy itself. To set
    /// This value should be reset by the same code that sets it.
    /// </summary>
    internal bool ReservedForResourceRequirement
    {
        get => m_reservedForResourceRequirement;
        set => m_reservedForResourceRequirement = value;
    }

    #region Reserved move date stuff. This is for handling an activity that is dragged and dropped.
    /// <summary>
    /// The start of the interval to reserve for the move.
    /// </summary>
    private long m_reservedMoveStartTicks;

    internal long ReservedMoveStartTicks => m_reservedMoveStartTicks;

    // [USAGE_CODE] m_reservedMoveEndTicks: Currently this is only used when the moved block can't schedule at the move time. In this case the 
    /// <summary>
    /// If specified, the end of the interval to reserve for the move.
    /// </summary>
    private long m_reservedMoveEndTicks;

    internal long ReservedMoveEndTicks => m_reservedMoveEndTicks;

    private ActivityKey m_reservedMoveActKey;

    /// <summary>
    /// This is used to reserve a start time on a resource for the operation that is
    /// being dragged and dropped.
    /// This only affects SingleTasking resources.
    /// </summary>
    internal void SetReservedMoveDate(ActivityKey a_moveActKey, long a_startTicks, long a_endTicks)
    {
        if (CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            m_reservedMoveStartTicks = a_startTicks;
            m_reservedMoveEndTicks = a_endTicks;
            m_reservedMoveActKey = a_moveActKey;
        }
    }

    internal const int NOT_RESERVED = -1;

    internal void ClearReservedMoveTime()
    {
        m_reservedMoveStartTicks = NOT_RESERVED;
        m_reservedMoveEndTicks = NOT_RESERVED;
        m_reservedMoveActKey = ActivityKey.NullKey;
    }

    private bool IntersectsReservedMoveTime(ScenarioDetail a_sd, InternalActivity a_ia, long a_startTicks, long a_endTicks, out LeftNeighborSequenceValues o_sequenceVals)
    {
        // The move into batch is the batch moved activities are being moved into and so MoveIntoBatches can schedule at the ReservedMoveDate.
        bool ret;
        o_sequenceVals = LeftNeighborSequenceValues.NullValues;
        if (a_ia.BeingMoved || a_ia.MoveIntoBatch)
        {
            ret = false;
        }
        else if (m_reservedMoveStartTicks != NOT_RESERVED)
        {
            InternalActivity reservedMoveAct = a_sd.JobManager.FindActivity(m_reservedMoveActKey);
            if (m_reservedMoveEndTicks == NOT_RESERVED)
            {
                o_sequenceVals = new LeftNeighborSequenceValues(reservedMoveAct, m_reservedMoveStartTicks);
                ret = Interval.Contains(a_startTicks, a_endTicks, m_reservedMoveStartTicks);
            }
            else
            {
                o_sequenceVals = new LeftNeighborSequenceValues(reservedMoveAct, m_reservedMoveEndTicks);
                // [USAGE_CODE] IntersectsReservedMoveTime: Check whether an activity intersects the time the move activity needs during a replay of a move.
                ret = Interval.Intersection(a_startTicks, a_endTicks, m_reservedMoveStartTicks, m_reservedMoveEndTicks);
            }
        }
        else
        {
            ret = false;
        }

        return ret;
    }
    #endregion

    #region Block Reservation
    // [USAGE_CODE]: Members to store, add, remove, and check for Intersection of BlockReservations.

    /// <summary>
    /// The set of times past the simulation clock that are reserved for specific activities' blocks.
    /// </summary>
    private readonly BlockReservationSet m_blockReservations = new ();

    /// <summary>
    /// Reserve a interval of time for an activities blocks.
    /// </summary>
    /// <param name="a_act">The activity to reserve time for.</param>
    /// <param name="a_rr">The RR of the activity time is being reserved for.</param>
    /// <param name="a_startTicks">The start of the interval of time to reserve.</param>
    /// <param name="a_endTicks">The end the the interval.</param>
    /// <param name="a_si"></param>
    /// <returns></returns>
    internal BlockReservation AddBlockReservation(InternalActivity a_act, ResourceRequirement a_rr, long a_startTicks, long a_endTicks, SchedulableInfo a_si)
    {
        BlockReservation reservation = new (a_act, a_rr, a_startTicks, a_endTicks, a_si);
        m_blockReservations.Add(reservation);
        return reservation;
    }

    /// <summary>
    /// Remove a block reservation from the block reservation set.
    /// </summary>
    /// <param name="a_reservation">The block reservation to remove.</param>
    internal void RemoveBlockReservation(BlockReservation a_reservation)
    {
        m_blockReservations.Remove(a_reservation);
    }

    /// <summary>
    /// Remove a firm future reservation, likely the activity has scheduled
    /// </summary>
    /// <param name="a_reservation">The block reservation to remove.</param>
    internal void RemoveFutureReservation(ResourceReservation a_reservation)
    {
        m_resourceReservationSet.m_reservations.Remove(a_reservation);
    }

    internal bool IntersectsBlockReservation(InternalActivity a_act, long a_startTicks, long a_endTicks, out LeftNeighborSequenceValues o_sequenceValues)
    {
        o_sequenceValues = LeftNeighborSequenceValues.NullValues;
        if (m_blockReservations.Intersects(a_startTicks, a_endTicks, out BlockReservation blockReservation))
        {
            o_sequenceValues = new LeftNeighborSequenceValues(blockReservation.Activity, blockReservation.EndTicks);
            return true;
        }

        if (m_resourceReservationSet.Intersects(a_act, a_startTicks, a_endTicks, out ResourceReservation resReservation))
        {
            o_sequenceValues = new LeftNeighborSequenceValues(resReservation.m_ia, resReservation.EndTicks);
            return true;
        }

        return false;
    }
    #endregion

    internal void ResetSimulationStateVariables(long a_clock, ScenarioOptions a_so)
    {
        ResetSimulationStateVariables(a_clock);
        InitResourceReservationsSet(a_so);
    }

    internal override void ResetSimulationStateVariables(long a_clock)
    {
        base.ResetSimulationStateVariables(a_clock);

        m_reservedForResourceRequirement = false;
        m_sortedTimeBasedCleanoutTriggers.Clear();

        if (TimeCleanoutTriggerTable != null)
        {
            foreach (TimeCleanoutTriggerTableRow timeCleanRow in TimeCleanoutTriggerTable)
            {
                if (!timeCleanRow.UseProcessingTime && !timeCleanRow.UsePostprocessingTime)
                {
                    TimeBasedCleanoutTriggerInfo timeBasedCleanoutTriggerInfo = new ()
                    {
                        CleanoutDurationTicks = timeCleanRow.TriggerValue.Ticks,
                        LastCleanoutRunTime = 0
                    };

                    m_sortedTimeBasedCleanoutTriggers.Add(timeCleanRow.CleanoutGrade, timeBasedCleanoutTriggerInfo);
                }
            }
        }

        ClearReservedMoveTime();
    }

    /// <summary>
    /// If necessary, initialize the member variables used for max delay.
    /// </summary>
    /// <param name="a_so">Needed to check whether max delay should be enforced.</param>
    private void InitResourceReservationsSet(ScenarioOptions a_so)
    {
        if (m_resourceReservationComparer == null && a_so.EnforceMaxDelay)
        {
            m_resourceReservationComparer = new ResourceReservationComparer();
            m_resourceSpans = new AVLTree<ResourceSpan, ResourceSpan>(m_resourceReservationComparer);
        }
    }

    #region Attention Percent
    /// <summary>
    /// Determine whether the resource has some percentage of availablility at a given time.
    /// </summary>
    /// <param name="a_act">The activity attention is being sought for.</param>
    /// <param name="a_startOfReqTicks">The time at which the available percentage is needed.</param>
    /// <param name="a_endOfReqTicks">The finish time of the needed attention.</param>
    /// <param name="a_rr">The ResourceRequirement attention is being sought for.</param>
    /// <param name="a_resourceRequirementSatiaters">Can be null (when working on the primary resource requirement).Earmarked resources to satisify the resource requirements.</param>
    /// <param name="a_si">Can be null (when working on the primary resource requirement). The SchedulableInfo determined by the primary resource requirement.</param>
    /// <param name="a_availableNode">Can be -1 for Primary Resource Requirement. The index of a_resourceRequirementSatiaters corresponding to a_rr</param>
    /// <returns></returns>
    private ResourceCapacityInterval.AttnAvailResult AttentionAvailable(InternalActivity a_act, long a_startOfReqTicks, long a_endOfReqTicks, ResourceRequirement a_rr, ResourceSatiator[] a_resourceRequirementSatiaters, SchedulableInfo a_si, LinkedListNode<ResourceCapacityInterval> a_availableNode)
    {
        //When this function is called, it first adds all the required attentions of the current ResourceRequirement to the RCINeededPercentManager, 
        //then it adds the required attentions of any other ResourceRequirement's RequiredAttentions to the RCINeededPercentManager.
        //These will be compared to what's actually scheduled on the capacity intervals to check whether there's enough attention available.           
        m_neededPctMgr.Clear();
        m_neededPctMgr.AddRCINeededPercents(a_availableNode, a_act, a_rr, a_startOfReqTicks, a_endOfReqTicks);

        //TODO: This adding of scheduled blocks section was added for #12000. It has been tweaked several times.
        // I don't think this is needed any more now that improvements to the neededPctMgr have been made.
        //??? This section adds in scheduled blocks, but a reference is already stored to the scheduled blocks as they schedule. Why do we do this???
        //Add in Scheduled activities starting with the last node. As long as it spans this rci, it is taking capacity
        //ResourceBlockList.Node scheduledNode = LastScheduledBlockListNode;
        //TODO: How to efficiently get all of the blocks that span this interval. Blocks are sorted by start date, so when looping backwards, we can't reliably stop based on an end date. 
        //TODO: Block 1 can start, then Block 2 can start, then Block 2 can end, in this case going backwards and stopping based on end date we would not find Block 1.
        //while (scheduledNode?.Data is ResourceBlock block /* && block.StartTicks <= a_availableNode.Data.EndDate && block.EndTicks > a_availableNode.Data.StartDate*/)
        //{
        //    if (block.Intersection(a_startOfReqTicks, a_endOfReqTicks))
        //    {
        //        //This block intersects the activity attempting to schedule. Add it's need percents.
        //        InternalActivity activity = block.Batch.FirstActivity;
        //        m_neededPctMgr.AddRCINeededPercents(a_availableNode, activity, activity.Operation.ResourceRequirements.GetByIndex(block.ResourceRequirementIndex), block.StartTicks, block.EndTicks);
        //    }

        //    scheduledNode = scheduledNode.previous;
        //}

        //Add in Max Delay reservations
        foreach (ResourceReservation reservation in m_resourceReservationSet)
        {
            if (reservation.m_ia != a_act && reservation.EndTicks >= a_startOfReqTicks)
            {
                m_neededPctMgr.AddRCINeededPercents(a_availableNode,
                    reservation.m_ia,
                    reservation.m_ia.Operation.ResourceRequirements.GetByIndex(reservation.m_rrIndex),
                    reservation.StartTicks,
                    reservation.EndTicks);
            }
        }

        //Add in Helper reservations where the usage didn't start the same as the primary
        foreach (BlockReservation blockReservation in m_blockReservations)
        {
            if (blockReservation.Activity != a_act && blockReservation.EndTicks >= a_startOfReqTicks)
            {
                m_neededPctMgr.AddRCINeededPercents(a_availableNode,
                    blockReservation.Activity,
                    blockReservation.ResReq,
                    blockReservation.StartTicks,
                    blockReservation.EndTicks);
            }
        }

        // The code below is for calculating percentUsed in the case where multiple resource requirements attempt to use the same resource as their resource
        // satiator. For instance:
        // ResourceRequirement 1: AttentionPercent=33; RequiredCapability=Mixer.
        // ResourceRequirement 2: AttentionPercent=33; RequiredCapability=Blender.
        // Resource Henry is able to perform both required capabilities and is a multi-tasking resource.
        // In this case the resource is capable of handling both ResourceRequirements at the same time; hence the need for the code below.
        if (a_resourceRequirementSatiaters != null)
        {
            ResourceRequirementManager rrm = a_act.Operation.ResourceRequirements;

            for (int resourceRequirementSatiatersI = 0; resourceRequirementSatiatersI < a_resourceRequirementSatiaters.Length; ++resourceRequirementSatiatersI)
            {
                ResourceSatiator rs = a_resourceRequirementSatiaters[resourceRequirementSatiatersI];

                if (rs != null)
                {
                    Resource resourceRequirementSatiater = rs.Resource;
                    if (resourceRequirementSatiater == this)
                    {
                        ResourceRequirement satiatedRR = rrm.GetByIndex(resourceRequirementSatiatersI);

                        if (satiatedRR == a_rr)
                        {
                            continue;
                        }

                        // One of the resource requirements has already earmarked this resource as a requirement satiator.
                        // Add its attention percent to the amount of attention to consider as being consumed.
                        long satiatedRRsFinishTime = a_si.GetUsageEnd(satiatedRR);
                        m_neededPctMgr.AddRCINeededPercents(a_availableNode, a_act, satiatedRR, a_startOfReqTicks, satiatedRRsFinishTime);
                    }
                }
            }
        }

        ResourceCapacityInterval.AttnAvailResult attnAvail = m_neededPctMgr.AttentionAvailable(a_act, a_startOfReqTicks, a_endOfReqTicks);
        return attnAvail;
    }
    #endregion Attention Percent

    #endregion

    #region Miscellaneous
    protected override int BlockCount => m_blocks.Count;

    /// <summary>
    /// Find the ResourceCapacityInterval that contains the specified time.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private ResourceCapacityInterval FindRCI(long a_time)
    {
        ResourceCapacityInterval rci;
        ResourceCapacityIntervalsCollection cp = capacityProfile.ProfileIntervals;

        for (int rciI = 0; rciI < cp.Count; ++rciI)
        {
            rci = cp[rciI];
            if (rci.StartDate <= a_time && a_time < rci.EndDate)
            {
                return rci;
            }
        }

        throw new PTException("FindRCI() couldn't find a resource capacity interval for the specified time.");
    }
    #endregion

    private decimal GetResourceCustomizedQtyPerCycle(InternalActivity a_ia, ExtensionController a_timeCustomizer)
    {
        decimal? qtyPerCycle = a_timeCustomizer.GetQtyPerCycle(a_ia, this);
        if (qtyPerCycle.HasValue)
        {
            return qtyPerCycle.Value;
        }

        return a_ia.ScheduledOrDefaultProductionInfo.QtyPerCycle;
    }

    /// <summary>
    /// Note this function was moved simulation resource file because it now uses one of the simulation functions.
    /// Determine whether an operation is capable of being manufactured on resource based on quantity such as:
    /// batch volume, min quantity per cycle, and Max quantity per cycle.
    /// In the case of batch volume only operations with a single activity are tested against the resources batch volume.
    /// For operations that have multiple activities, the batch volume is not tested. Instead,
    /// batch testing is performed on an activity by activity basis during the simulation.
    /// </summary>
    /// <param name="a_op"></param>
    /// <returns></returns>
    public bool IsCapableBasedOnMinMaxQtyPerCycle(InternalOperation a_op, ProductRuleManager a_prs)
    {
        bool ret = true;
        // [BATCH_CODE]
        if (BatchResource() && BatchVolume > 0)
        {
            if (!a_op.Split)
            {
                InternalActivity act = a_op.Activities.GetByIndex(0);
                decimal reqQty = act.BatchAmount;
                if (BatchVolume < reqQty)
                {
                    ret = false;
                }
            }
        }
        else
        {
            decimal opQtyPerCycle = a_op.QtyPerCycle;
            //Product Rules
            if (a_op.Products.PrimaryProduct is Product primaryProduct)
            {
                ProductRule productRule = a_prs.GetProductRule(Id, primaryProduct.Item.Id, a_op.ProductCode);
                if (productRule != null && productRule.UseQtyPerCycle)
                {
                    opQtyPerCycle = productRule.QtyPerCycle;
                }
            }

            if (opQtyPerCycle > MaxQtyPerCycle || opQtyPerCycle < MinQtyPerCycle)
            {
                ret = false;
            }
            else
            {
                ret = true;
            }
        }

        return ret;
    }

    /// <summary>
    /// In the case of batch volume only operations with a single activity are tested against the resources batch volume.
    /// For operations that have multiple activities, the batch volume is not tested. Instead,
    /// batch testing is performed on an activity by activity basis during the simulation.
    /// </summary>
    /// <param name="a_op"></param>
    /// <returns></returns>
    public bool IsCapableBasedOnMinMaxVolume(InternalOperation a_op, ProductRuleManager a_prs)
    {
        if (a_op.Split)
        {
            foreach (InternalActivity activity in a_op.Activities)
            {
                if (IsCapableBasedOnMinMaxVolume(activity, a_op.AutoSplitInfo, a_prs))
                {
                    //Only one activity needs to be eligible for the operation to be eligible on the resource
                    //The activity eligibility will be calculated elsewhere.
                    return true;
                }
            }

            //No activities eligible
            return false;
        }

        return IsCapableBasedOnMinMaxVolume(a_op.Activities.GetByIndex(0), a_op.AutoSplitInfo, a_prs);
    }

    /// <summary>
    /// In the case of batch volume only operations with a single activity are tested against the resources batch volume.
    /// For operations that have multiple activities, the batch volume is not tested. Instead,
    /// batch testing is performed on an activity by activity basis during the simulation.
    /// </summary>
    /// <param name="a_act"></param>
    /// <param name="a_opAutoSplitInfo"></param>
    /// <param name="a_op"></param>
    /// <returns></returns>
    public bool IsCapableBasedOnMinMaxVolume(InternalActivity a_act, AutoSplitInfo a_opAutoSplitInfo, ProductRuleManager a_prs)
    {
        if (a_act.Operation.Products.Count == 0)
        {
            return true;
        }

        // [BATCH_CODE]
        if (BatchResource() && BatchVolume > 0)
        {
            if (!a_act.Operation.Split)
            {
                if (BatchVolume < a_act.BatchAmount)
                {
                    return false;
                }
            }

            return true;
        }

        //When split type is manual, allow to override constraints so users can manually split the operation once scheduled
        if (a_opAutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.Manual)
        {
            return true;
        }

        decimal minVolume = MinVolume;
        bool minVolumeConstrained = MinVolumeConstrained;

        decimal maxVolume = MaxVolume;
        bool maxVolumeConstrained = MaxVolumeConstrained;

        //Check for Product Rule override
        if (a_act.Operation.Products.PrimaryProduct is Product primaryProduct)
        {
            ProductRule productRule = a_prs.GetProductRule(Id, primaryProduct.Item.Id, a_act.Operation.ProductCode);
            if (productRule != null)
            {
                if (productRule.UseMinVolume)
                {
                    minVolume = productRule.MinVolume;
                    minVolumeConstrained = productRule.MinVolume != 0m && productRule.MinVolume != decimal.MinValue; ;
                }

                if (productRule.UseMaxVolume)
                {
                    maxVolume = productRule.MaxVolume;
                    maxVolumeConstrained = productRule.MaxVolume != 0m && productRule.MaxVolume != decimal.MaxValue;
                }
            }
        }

        //Check if resources are actually constrained by Max Qty
        if (!minVolumeConstrained && !maxVolumeConstrained)
        {
            return true;
        }

        decimal productVolume = a_act.RemainingRequiredVolume;

        if (minVolumeConstrained && productVolume < minVolume)
        {
            //Can't scheduler here as the Resource Min Vol exceeds the activity product Vol
            return false;
        }

        if (maxVolumeConstrained && productVolume <= maxVolume)
        {
            return true;
        }

        //Check whether this operation could be autosplit and still be eligible
        if (a_opAutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.ResourceVolumeCapacity)
        {
            if (a_opAutoSplitInfo.MinAutoSplitAmount != 0m && a_opAutoSplitInfo.MinAutoSplitAmount > maxVolume)
            {
                return false;
            }

            if (a_opAutoSplitInfo.MaxAutoSplitAmount != 0m && a_opAutoSplitInfo.MaxAutoSplitAmount < minVolume)
            {
                return false;
            }

            return true;
        }

        if (a_opAutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.PredecessorMaterials
            || a_opAutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.PredecessorQuantityRatio)
        {
            if (a_act.Operation.Predecessors.Count == 0)
            {
                return false;
            }

            ResourceOperation predOp = a_act.Operation.Predecessors[0].Predecessor.Operation;
            if (predOp.AutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.None)
            {
                return false;
            }

            //Pred op can split, allow this op to at least attempt to schedule
            return true;
        }

        //No activities were eligible
        return false;
    }

    public enum ESawtoothDirection { Up, Down, NotSet }

    /// <summary>
    /// Indicates if the Resource has scheduled at least two blocks and a sawtooth direction has been set.
    /// </summary>
    /// <returns></returns>
    public ESawtoothDirection GetCurrentSawtoothDirection(decimal? a_lastBlockSetup, decimal? a_secondToLastBlockSetup)
    {
        if (CodesClear || !a_lastBlockSetup.HasValue || !a_secondToLastBlockSetup.HasValue || !(Dispatcher.DispatcherDefinition is BalancedCompositeDispatcherDefinition))
        {
            return ESawtoothDirection.NotSet;
        }

        //Trending up
        if (a_lastBlockSetup.Value > a_secondToLastBlockSetup.Value)
        {
            return ESawtoothDirection.Up;
        }

        //Trending down
        if (a_lastBlockSetup.Value < a_secondToLastBlockSetup.Value)
        {
            return ESawtoothDirection.Down;
        }

        //No trend, but we should choose the nearest so it can go in either direction
        return ESawtoothDirection.NotSet;
    }

    /// <summary>
    /// Add a final cleanout due to the planning horizon.
    /// </summary>
    /// <param name="a_sd"></param>
    internal void AppendHorizonCleanout(ScenarioDetail a_sd)
    {
        Batch lastBatch = Blocks.Last.Data.Batch;

        LeftNeighborSequenceValues leftNeighborSequenceValues = CreateDefaultLeftNeighborSequenceValues(a_sd.SimClock, Blocks.Last);
        RequiredSpanPlusClean cleanoutAfter = CalculateMinimumCleanoutAfter(a_sd.SimClock, lastBatch.FirstActivity, leftNeighborSequenceValues, true, a_sd.ExtensionController, true);

        if (a_sd.ExtensionController.RunOverrideCleanExtension)
        {
            RequiredSpanPlusClean overrideLastActCleanSpan = a_sd.ExtensionController.OverrideLastActCleanSpan(a_sd, lastBatch.FirstActivity, this);
            cleanoutAfter = cleanoutAfter.Merge(overrideLastActCleanSpan, true);
        }

        if (cleanoutAfter.TimeSpanTicks > 0)
        {
            long cleanStartDate = lastBatch.EndOfStorageTicks;

            //Find the starting node
            LinkedListNode<ResourceCapacityInterval> startPosition = ResultantCapacity.Find(cleanStartDate, LastResultantCapacityNode);
            ResourceBlockList.Node batchBlock = lastBatch.BlockForResource(this);
            ResourceRequirement rr = lastBatch.FirstActivity.Operation.ResourceRequirements.GetByIndex(batchBlock.Data.ResourceRequirementIndex);
            //TODO: Verify this works when using the previously  scheduled activity.
            //TODO: The last parameter (lastBatch.FirstActivity.PrimaryResourceRequirement) RR is always the primary, test this for helpers.
            FindCapacityResult capacityResult = FindCapacity(cleanStartDate,
                cleanoutAfter.TimeSpanTicks,
                true,
                startPosition,
                MainResourceDefs.usageEnum.Clean,
                false,
                false,
                rr.CapacityCode,
                false,
                lastBatch.FirstActivity.PeopleUsage,
                lastBatch.FirstActivity.NbrOfPeople,
                out bool _);

            if (capacityResult.ResultStatus == SchedulableSuccessFailureEnum.Success)
            {
                lastBatch.MergeCleanout(cleanoutAfter, capacityResult.CapacityUsageProfile);
            }
            else
            {
                //Try again without the profile constraints
                capacityResult = FindCapacity(cleanStartDate,
                    cleanoutAfter.TimeSpanTicks,
                    true,
                    startPosition,
                    MainResourceDefs.usageEnum.Clean,
                    false,
                    false,
                    rr.CapacityCode,
                    true,
                    lastBatch.FirstActivity.PeopleUsage,
                    lastBatch.FirstActivity.NbrOfPeople
                    , out bool _);

                if (capacityResult.ResultStatus == SchedulableSuccessFailureEnum.Success)
                {
                    lastBatch.MergeCleanout(cleanoutAfter, capacityResult.CapacityUsageProfile);
                }
                else
                {
                    //What do we want to do here? The sequence has determined that cleanout is required, but it can't fit.
                    DebugException.ThrowInDebug("Failed to add clean before span due to invalid capacity (2)");
                }
            }
        }
    }

    /// <summary>
    /// Returns a LeftNeighborSequence vals based on the last scheduled or finished activity. This method should only be used during scheduling
    /// for example in a sequencing factor since it uses simulation variables that may not be set outside of a simulation.
    /// If no finished or scheduled activities exist for this resource, it will return an empty unitialized sequence vals.
    /// </summary>
    /// <returns></returns>
    public LeftNeighborSequenceValues CreateDefaultLeftNeighborSequenceValues(long a_simClock)
    {
        LeftNeighborSequenceValues sequenceValues = LeftNeighborSequenceValues.NullValues;
        if (LastScheduledBlockListNode != null)
        {
            sequenceValues = new LeftNeighborSequenceValues(LastScheduledBlockListNode);
        }

        InternalActivity lastRunActivity = GetLastRunActivity(a_simClock);
        if (lastRunActivity != null)
        {
            if (!sequenceValues.Initialized || lastRunActivity.ReportedFinishDateTicks > sequenceValues.EndDate)
            {
                sequenceValues = new LeftNeighborSequenceValues(lastRunActivity, lastRunActivity.ReportedFinishDateTicks);
            }
        }

        return sequenceValues;
    }

    /// <summary>
    /// Returns a LeftNeighborSequence vals based on the provided block node or a finished activity that ends later. This method should only be used during scheduling
    /// for example in a sequencing factor since it uses simulation variables that may not be set outside of a simulation.
    /// If no finished or scheduled activities exist for this resource, it will return an empty unitialized sequence vals.
    /// </summary>
    /// <returns></returns>
    public LeftNeighborSequenceValues CreateDefaultLeftNeighborSequenceValues(long a_simClock, ResourceBlockList.Node a_leftNode)
    {
        LeftNeighborSequenceValues sequenceValues = LeftNeighborSequenceValues.NullValues;
        if (a_leftNode != null)
        {
            sequenceValues = new LeftNeighborSequenceValues(a_leftNode);
        }

        InternalActivity lastRunActivity = GetLastRunActivity(a_simClock);
        if (lastRunActivity != null)
        {
            if (!sequenceValues.Initialized || lastRunActivity.ReportedFinishDateTicks > sequenceValues.EndDate)
            {
                sequenceValues = new LeftNeighborSequenceValues(lastRunActivity, lastRunActivity.ReportedFinishDateTicks);
            }
        }

        return sequenceValues;
    }

    /// <summary>
    /// Get LeftneighborSequencingValues of the last scheduled or finished Activity. This method can be used outside of scheduling.
    /// </summary>
    /// <returns></returns>
    public LeftNeighborSequenceValues GetLastSequenceValue()
    {
        IEnumerable<LeftActivityInfo> leftActivityEnumerator = LeftActivityEnumerator();
        LeftActivityInfo leftActInfo = leftActivityEnumerator.FirstOrDefault();
        if (leftActInfo != null)
        {
            return new LeftNeighborSequenceValues(leftActInfo.Activity, leftActInfo.EndTicks);
        }

        return LeftNeighborSequenceValues.NullValues;
    }

    /// <summary>
    /// Enumerates through the Scheduled blocks and Track Actuals backwards in time.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<LeftActivityInfo> LeftActivityEnumerator()
    {
        ResourceBlockList.Node blockNode = Blocks.Last;

        IEnumerator<InternalActivity> actualsEnumerator = m_cleanoutHistoryData.GetActivityEnumerator().GetEnumerator();
        actualsEnumerator.MoveNext();
        InternalActivity lastActual = actualsEnumerator.Current;


        while (blockNode != null || lastActual != null)
        {
            long currentBlockEndTicks = blockNode?.Data?.EndTicks ?? 0;
            long currentActualEndTicks = lastActual?.ReportedFinishDateTicks ?? 0;

            if (currentActualEndTicks >= currentBlockEndTicks)
            {
                InternalActivity currAct = lastActual;
                actualsEnumerator.MoveNext();
                lastActual = actualsEnumerator.Current;
                yield return new LeftActivityInfo(currAct);
            }
            else
            {
                InternalActivity currBlock = blockNode.Data.Batch.FirstActivity;
                blockNode = blockNode.Previous;
                yield return new LeftActivityInfo(currBlock);
            }
        }
    }
}