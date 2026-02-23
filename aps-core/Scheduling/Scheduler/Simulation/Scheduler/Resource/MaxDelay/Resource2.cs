using PT.Common.Collections;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public partial class Resource
{
    private ResourceReservationComparer m_resourceReservationComparer;

    /// <summary>
    /// This is a set of all the timespans of ResourceReservations and Blocks on this resource.
    /// AVL trees tend to insert sorted data efficiently.
    /// This set can be enumerated in order to determine if a desired timespan is consumed.
    /// </summary>
    private AVLTree<ResourceSpan, ResourceSpan> m_resourceSpans;

    /// <summary>
    /// Whether this resource has been initialized to enforce Max Delay.
    /// </summary>
    internal bool EnforceMaxDelay => m_resourceSpans != null; // This data structure is only created if max delay should be enforced. 

    /// <summary>
    /// Attempt to create a MaxDelay resource reservation for the successor operation in an association.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_asn"></param>
    /// <param name="a_opReleaseTicks"></param>
    /// <param name="a_predecessorResource"></param>
    /// <param name="a_mustStartByTicks"></param>
    /// <returns></returns>
    internal ResourceReservationResult CreateContinuousReservation(ScenarioDetail a_sd, long a_simClock, AlternatePath.Association a_asn, long a_opReleaseTicks, Resource a_predecessorResource, long a_mustStartByTicks)
    {
        ResourceOperation ro = a_asn.Successor.Operation;
        InternalActivity ia = ro.Activities.GetByIndex(0);

        //The max start ticks is based on the successor release + any allowed delay
        long successorStartTicks = a_opReleaseTicks;

        //We need to try and keep activities where they were anchored or previously scheduled if 
        //anchored or sequential
        if (ia.Anchored && ia.AnchorDateTicks < a_mustStartByTicks)
        {
            //only use anchor date if it was anchored after the calculated release time
            successorStartTicks = Math.Max(successorStartTicks, ia.AnchorDateTicks);
        }
        else if (ia.Sequenced)
        {
            //Attempt to leave it where it was, but it may be delayed. If so, it can't start before the clock.
            if (ia.OriginalScheduledStartTicks < a_mustStartByTicks && ia.OriginalScheduledStartTicks >= a_simClock)
            {
                successorStartTicks = Math.Max(successorStartTicks, ia.OriginalScheduledStartTicks);
            }
        }

        // The resource will be online. Determine the required capacity and finish date.
        // Calaculate how long the operation will take.
        // This include the setup, processing, and post processing time as well as Schedulable info?
        // Take material post processing time into consideration. 
        // Can the activity cross offline periods.

        LeftNeighborSequenceValues sequenceVals = CreateDefaultLeftNeighborSequenceValues(successorStartTicks);
        long startTicks = NextAvailableDate(ia, successorStartTicks, a_mustStartByTicks, sequenceVals, out LinkedListNode<ResourceCapacityInterval> rciNode, a_sd.ExtensionController);
        if (!StartDateOkay(successorStartTicks, a_mustStartByTicks, startTicks))
        {
            return new ResourceReservationResult(startTicks);
        }

        long nextAttemptStartTicks = startTicks;
        bool triedSchedulingInPlanningHorizon = false;
        int retriedWithNewSequenceVals = 0;
        while (retriedWithNewSequenceVals <= 1)
        {
            RequiredCapacityPlus rc = CalculateTotalRequiredCapacity(a_simClock, ia, sequenceVals, true, nextAttemptStartTicks, a_sd.ExtensionController);
            SchedulableResult result = PrimarySchedulable(a_sd, a_simClock, ia, false, ro.ResourceRequirements.PrimaryResourceRequirement, nextAttemptStartTicks, sequenceVals, rc, rciNode, false);

            if (result.m_result != SchedulableSuccessFailureEnum.Success)
            {
                if (result.m_result == SchedulableSuccessFailureEnum.LackCapacity)
                {
                    throw new Exception("The resource isn't online.");
                }

                sequenceVals = result.LeftSequenceValues;
                //The result failed because of a reservation, try again with new sequence values
                if (result.RetryTime == 0)
                {
                    retriedWithNewSequenceVals++;
                    continue;
                }

                nextAttemptStartTicks = result.RetryTime;
                LinkedListNode<ResourceCapacityInterval> retryInterval = ResultantCapacity.FindFirstOnline(nextAttemptStartTicks, rciNode);

                if (retryInterval.Value.IsPastPlanningHorizon)
                {
                    if (triedSchedulingInPlanningHorizon)
                    {
                        //We failed past the planning horizon. For example a cleanout or offline is in the way. 
                        //Instead of trying to calculate the reason, just try again later.
                        return new ResourceReservationResult(nextAttemptStartTicks + TimeSpan.FromDays(1).Ticks);
                    }

                    //The next attempt will be past the planning horizon.
                    triedSchedulingInPlanningHorizon = true;
                }

                nextAttemptStartTicks = Math.Max(result.RetryTime, retryInterval.Value.StartDate);
                rciNode = retryInterval;

                if (nextAttemptStartTicks <= a_mustStartByTicks)
                {
                    retriedWithNewSequenceVals = 0; //We moved to a new time, reset the sequenceVals counter
                    continue;
                }

                //The primary is not available until after the max start ticks, 
                break;
            }

            //Successful Result
            ResourceReservation futureReservation = new (result.m_si, ia, a_predecessorResource);
            return new ResourceReservationResult(futureReservation, result);
        }

        //The primary is not available until after the max start ticks, 
        return new ResourceReservationResult(nextAttemptStartTicks);
    }

    /// <summary>
    /// For use with Max Delay.
    /// Returns the next date it's possible to fit the activity onto the resource.
    /// It could return the input release tick  up to the max start ticks. In which case it's possible to schedule the operation within  its max delay time.
    /// Or if it's not possible to schedule it within the max delay time period, it will return a date further out in time when it
    /// might be possible to schedule this operation. Though by the time that time occurs some other operations might have already
    /// scheduled.
    /// </summary>
    /// <param name="a_ia">The activity that needs to be scheduled.</param>
    /// <param name="a_earliestStartTicks">The time the activity is being released; the earliest it can start. This must be after the simulation clock. </param>
    /// <param name="a_maxStartTicks">The latest the operation can start and still satisfy the max delay constraint.</param>
    /// <param name="a_sequenceVals"></param>
    /// <param name="o_rciNode">The capacity interval containing the next available date this activity can start at. </param>
    /// <param name="a_timeCustomizer"></param>
    /// <returns>
    /// The next date this activity might be able to be scheduled. It might be at the release date, it might be later than the release date but less than the max delay time, or it might be beyond
    /// the max delay.
    /// </returns>
    internal long NextAvailableDate(InternalActivity a_ia, long a_earliestStartTicks, long a_maxStartTicks, LeftNeighborSequenceValues a_sequenceVals, out LinkedListNode<ResourceCapacityInterval> o_rciNode, ExtensionController a_timeCustomizer)
    {
        long nextPotentialStartTicks = a_earliestStartTicks;
        o_rciNode = ResultantCapacity.FindForwards(nextPotentialStartTicks, null);

        if (!o_rciNode.Value.Active)
        {
            // If the capacity interval isn't active, find the first one that is. 
            o_rciNode = ResultantCapacity.FindFirstOnline(nextPotentialStartTicks, o_rciNode);
            nextPotentialStartTicks = o_rciNode.Value.StartDate;
        }

        if (EnforceMaxDelay && CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            //Calculate finishTicks
            RequiredCapacityPlus reqCapacity = CalculateTotalRequiredCapacity(a_earliestStartTicks, a_ia, a_sequenceVals, true, a_earliestStartTicks, a_timeCustomizer);
            FindCapacityResult findCapacityResult = FindFullCapacity(a_earliestStartTicks, reqCapacity, a_ia.Operation.CanPause, o_rciNode, a_ia, false);

            if (findCapacityResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
            {
                o_rciNode = ResultantCapacity.FindFirstOnline(findCapacityResult.NextStartTime, o_rciNode);
                return findCapacityResult.NextStartTime;
            }

            // If the required timespan intersects any blocks or resource reservations, return the next potential start time. 
            using AVLTree<ResourceSpan, ResourceSpan>.Enumerator curEtr = m_resourceSpans.GetEnumerator();
            while (curEtr.MoveNext())
            {
                ResourceSpan curRR = curEtr.Current.Value;
                if (Common.PTMath.Interval.Intersection(a_earliestStartTicks, findCapacityResult.FinishDate, curRR.Start, curRR.End))
                {
                    o_rciNode = ResultantCapacity.FindFirstOnline(curRR.End, o_rciNode);
                    nextPotentialStartTicks = curRR.End;
                    break;
                }
            }

            //Check currently scheduled blocks
            ResourceBlockList.Node blockNode = Blocks.Last;
            while (blockNode != null)
            {
                if (blockNode.Data.Batch.EndTicks < nextPotentialStartTicks)
                {
                    break;
                }

                if (blockNode.Data.Batch.StartTicks <= nextPotentialStartTicks)
                {
                    nextPotentialStartTicks = blockNode.Data.Batch.EndTicks;
                    break;
                }

                blockNode = blockNode.Previous;
            }
        }

        return nextPotentialStartTicks;

        //// The next time the activity might be able to be scheduled.
        //long finishTicks;
        //long totalReqCapacity;
        //ResourceBlockList.Node curBlockNode = m_blocks.First;
        //if (curBlockNode != null)
        //{
        //    ResourceBlock curBlock = curBlockNode.Data;
        //    reqCapacity = CalculateTotalRequiredCapacity(a_earliestStartTicks, a_ia, null, true, true, RequiredSpanPlusSetup.s_notInit, a_earliestStartTicks);
        //    totalReqCapacity = reqCapacity.TotalRequiredCapacity();
        //    FindCapacity(a_earliestStartTicks, totalReqCapacity, a_ia.Operation.CanPause, a_ia, true, out finishTicks);

        //    nextPotentialStartTicks = Blocks.Last.Data.EndTicks;
        //    o_rciNode = ResultantCapacity.FindForwards(nextPotentialStartTicks, null);

        //    o_rciNode = null;
        //    //It's completely before the first block
        //    NextAvailableDateBlockTest(curBlock, a_earliestStartTicks, finishTicks, ref o_rciNode, ref nextPotentialStartTicks);

        //    // The first block has been checked, move onto the next block.
        //    curBlockNode = curBlockNode.Next;

        //    while (curBlockNode != null)
        //    {
        //        // for each block,
        //        curBlock = curBlockNode.Data;
        //        ResourceBlock prevBlock = curBlockNode.Previous.Data; // There's always a previous block since this loop is only entered if there are multiple blocks and it starts off at the second block. 
        //        LeftNeighborSetupValues suVals = new LeftNeighborSetupValues(prevBlock.Batch.FirstActivity.Operation, prevBlock.EndTicks);
        //        reqCapacity = CalculateTotalRequiredCapacity(a_earliestStartTicks, a_ia, null, true, true, RequiredSpanPlusSetup.s_notInit, a_earliestStartTicks);
        //        totalReqCapacity = reqCapacity.TotalRequiredCapacity();
        //        FindCapacity(totalReqCapacity, a_earliestStartTicks, a_ia.Operation.CanPause, a_ia, true, out finishTicks);
        //        NextAvailableDateBlockTest(curBlock, a_earliestStartTicks, finishTicks, ref o_rciNode, ref nextPotentialStartTicks);

        //        //// and if not whether it fits between the previous block and the next block. 
        //        //if (prevBlockNode != null)
        //        //{


        //        //    if (PT.Common.PTMath.Interval.Intersection(a_releaseTicks, finishTicks, curBlock.StartTicks, curBlock.EndTicks) || PT.Common.PTMath.Interval.Intersection(a_releaseTicks, finishTicks, prevBlock.StartTicks, prevBlock.EndTicks))
        //        //    {
        //        //        // The interval intersects one of the resource's scheduled blocks.
        //        //        // Try again at the end of the the current block.

        //        //        nextPotentialStartTicks = curBlock.EndTicks;
        //        //        break;
        //        //    }

        //        //    // Try the max delay start point.
        //        //    long maxDelayEnd = a_maxStartTicks + totalReqCapacity;
        //        //    if (PT.Common.PTMath.Interval.Intersection(a_releaseTicks, finishTicks, curBlock.StartTicks, curBlock.EndTicks) || PT.Common.PTMath.Interval.Intersection(a_releaseTicks, finishTicks, prevBlock.StartTicks, prevBlock.EndTicks))
        //        //    {
        //        //        // The interval intersects one of the resource's scheduled blocks.
        //        //        // Try again at the end of the the current block.

        //        //        nextPotentialStartTicks = curBlock.EndTicks;
        //        //        break;
        //        //    }

        //        //}
        //        curBlockNode = curBlockNode.Next;
        //    }
        //}

        //// Check whether any scheduled blocks intersect the required time span.


        ////************************************************************************
        //// Find the first online capacity interval the activity could schedule in.
        ////************************************************************************

        //// Determine the next time this resource is available.
        //// Find the capacity interval that contains the release ticks or the first online interval.

        //if (!o_rciNode.Data.Active)
        //{
        //    // The capacity interval containing the release ticks isn't active.
        //    // Find the first online capacity interval.
        //    LinkedListNode<ResourceCapacityInterval> nextOnlineRCI = ResultantCapacity.FindFirstOnline(nextPotentialStartTicks, null);
        //    nextPotentialStartTicks = nextOnlineRCI.Data.StartDate;

        //    if (!StartDateOkay(a_earliestStartTicks, a_maxStartTicks, nextPotentialStartTicks))
        //    {
        //        // Failed to find a good start date.
        //        // The first online capacity interval's start exceeds the max delay.
        //        // The operation may eventually be scheduled past the max delay. 
        //        //return startTicks;
        //    }
        //}

        //// The activity has to schedule past the last scheduled block on the resource, if there's one scheduled. 
        //// Check whether there is something already scheduled. 
        ////if (CapacityType == SchedulerDefinitions.InternalResourceDefs.capacityTypes.SingleTasking)
        ////{
        ////    // Check whether the last scheduled block interferes with the desired start time.
        ////    ResourceBlockList.Node node = m_blocks.Last;

        ////    if (node != null)
        ////    {
        ////        ResourceBlock rb = node.Data;

        ////        if (rb.EndTicks > nextPotentialStartTicks)
        ////        {
        ////            // Try using the first online time past the last block scheduled on the resource. 
        ////            LinkedListNode<ResourceCapacityInterval> nextOnlineRCI = ResultantCapacity.FindFirstOnline(rb.EndTicks, null);
        ////            nextPotentialStartTicks = Math.Max(nextOnlineRCI.Data.StartDate, rb.EndTicks);

        ////            if (!StartDateOkay(a_releaseTicks, a_maxStartTicks, nextPotentialStartTicks))
        ////            {
        ////                // Failed to find a good start date.
        ////                // The next available time exceeds the activities max delay. 
        ////                // The operation may eventually be scheduled past the max delay. 
        ////                //return startTicks;
        ////            }
        ////        }
        ////    }
        ////}

        //// Compare the potential start time with the reservations already on this resource.
        //// The reservations might make it impossible to use the current best start time found. 
        //// Check whether it's possible to 
        //// Is there a reservation at this time?
        //// Reservations aren't added to Infinite Resources.
        //if (Reservations.Count > 0)
        //{
        //    ResourceReservation lastResRv = Reservations.Last;

        //    //if (lastResRv.FinishesAfter(startTicks))
        //    {

        //        LinkedListNode<ResourceCapacityInterval> nextOnlineRCI = ResultantCapacity.FindFirstOnline(lastResRv.m_si.m_finishDate, null);
        //        nextPotentialStartTicks = Math.Max(nextOnlineRCI.Data.StartDate, lastResRv.m_si.m_finishDate);

        //        // scan the resource reservations to see whether a spot between the reservations can be used to accomodate the activity at the release ticks.
        //        System.Collections.Generic.IEnumerator<ResourceReservation> resRevEtr = Reservations.GetEnumerator();
        //        bool intersectsReservation = false;
        //        ResourceReservation prevResRev = null;
        //        while (resRevEtr.MoveNext())
        //        {

        //            if (PT.Common.PTMath.Interval.Contains(resRevEtr.Current.m_si.m_scheduledStartDate, resRevEtr.Current.m_si.m_finishDate, a_earliestStartTicks))
        //            {
        //                // The start of the block intesects the current resource reservation. 
        //                intersectsReservation = true;
        //            }
        //            // if the release date is between 2 reservations, verify there is enough time between the reservations to accommodate the block. 
        //            if (prevResRev != null)
        //            {
        //                if (a_earliestStartTicks >= prevResRev.m_si.m_finishDate && a_earliestStartTicks < resRevEtr.Current.m_si.m_scheduledStartDate)
        //                {
        //                    // The release ticks is between 2 resource reservations. 
        //                    // Is there enough time between the two reservations for this block to fit between them?
        //                    LeftNeighborSetupValues suVals = new LeftNeighborSetupValues(prevResRev.m_ia.Operation, prevResRev.m_si.m_setupFinishDate);
        //                    RequiredSpanPlusSetup noSUVals = RequiredSpanPlusSetup.s_notInit;
        //                    reqCapacity = CalculateTotalRequiredCapacity(a_earliestStartTicks, a_ia, suVals, true, true, noSUVals, a_earliestStartTicks);
        //                    totalReqCapacity = reqCapacity.TotalRequiredCapacity();
        //                    finishTicks = a_earliestStartTicks + totalReqCapacity;
        //                    if (/*finishTicks <= resRevEtr.Current.m_si.m_scheduledStartDate*/PT.Common.PTMath.Interval.Intersection(a_earliestStartTicks, finishTicks, resRevEtr.Current.m_si.m_scheduledStartDate, resRevEtr.Current.m_si.m_setupFinishDate))
        //                    {
        //                        intersectsReservation = true;
        //                    }
        //                    else
        //                    {
        //                        // The block would intersect the current resource reservation.
        //                        //intersectsReservation = true;
        //                    }
        //                }
        //            }
        //            prevResRev = resRevEtr.Current;

        //        }
        //        if (!intersectsReservation)
        //        {
        //            // None of the scheduled blocks or resource reservations intersect with the start date.
        //            nextPotentialStartTicks = lastResRv.m_si.m_finishDate;
        //        }

        //        if (!StartDateOkay(a_earliestStartTicks, a_maxStartTicks, nextPotentialStartTicks))
        //        {
        //            // Failed to find a good start date.
        //            nextPotentialStartTicks = lastResRv.m_si.m_finishDate;
        //        }
        //    }
        //}

        //if ((o_rciNode != null && !o_rciNode.Data.Contains(nextPotentialStartTicks)) || o_rciNode == null)
        //{
        //    o_rciNode = ResultantCapacity.FindForwards(nextPotentialStartTicks, null);
        //}
        //return nextPotentialStartTicks;
    }

    private void NextAvailableDateBlockTest(ResourceBlock a_curBlock, long a_startTicks, long a_finishTicks, ref LinkedListNode<ResourceCapacityInterval> r_rciNode, ref long o_nextPotentialStartTicks)
    {
        if (a_startTicks >= a_curBlock.EndTicks)
        {
            // Starts after the block
            o_nextPotentialStartTicks = a_startTicks;
        }
        else if (a_finishTicks < a_curBlock.StartTicks)
        {
            // finishes before the block.
            o_nextPotentialStartTicks = a_startTicks;
        }

        else if (Common.PTMath.Interval.Intersection(a_startTicks, a_finishTicks, a_curBlock.StartTicks, a_curBlock.EndTicks))
        {
            //If it intersects the block, then it's not possible to schedule the activity at this time.
            o_nextPotentialStartTicks = a_curBlock.EndTicks;
        }

        r_rciNode = ResultantCapacity.FindForwards(o_nextPotentialStartTicks, null);
    }

    /// <summary>
    /// Reservations aren't added to Infinite resources since batches scheduled on them don't consume capacity.
    /// Do not modify this set from customizations.
    /// Reservations are always created from smallest start date to largest start date.
    /// They can't overlap and only the first thing on the resource can be scheduled at a time.
    /// </summary>
    private readonly ResourceReservationSet m_resourceReservationSet = new ();

    /// <summary>
    /// Reservations aren't added to Infinite resources since batches scheduled on them don't consume capacity.
    /// Do not modify this set from customizations.
    /// Reservations are always created from smallest start date to largest start date.
    /// They can't overlap and only the first thing on the resource can be scheduled at a time.
    /// </summary>
    internal ResourceReservationSet Reservations => m_resourceReservationSet;

    internal void AddReservation(ResourceReservation a_resRv)
    {
        #if DEBUG
        if (Reservations.Count > 0)
        {
            if (a_resRv.StartTicks < Reservations.Last.EndTicks)
            {
                //throw new Exception("The reservation has been created earlier than it should have.");
            }
        }
        #endif

        ResourceSpan rrb = new ResourceReservationSpan(a_resRv.m_ia, a_resRv.StartTicks, a_resRv.EndTicks);
        AddToResourceUsedAndReserveSpansSet(rrb);

        Reservations.Add(a_resRv);
    }

    /// <summary>
    /// If max delay operations are being scheduled on this resource,
    /// ResourceSpans must be added for each ResourceReservations and Blocks
    /// as they are created.
    /// </summary>
    /// <param name="rrb"></param>
    private void AddToResourceUsedAndReserveSpansSet(ResourceSpan rrb)
    {
        AVLTree<ResourceSpan, ResourceSpan>.TreeNode n = m_resourceSpans.Find(rrb);
        if (n != null)
        {
            // A resource reservation is initally added for prior to scheduling.
            // Once a block is scheduled, the resource reservation for an activity
            // is replaced with a resource reservation for a block. 
            m_resourceSpans.Remove(rrb);
        }

        m_resourceSpans.Add(rrb, rrb);
    }

    private void SimulationInitializationExtra()
    {
        Reservations.Clear();
    }

    /// <summary>
    /// Whether the possible start date is before the maximum time the operation can start.
    /// </summary>
    /// <param name="a_earliestStartTicks"></param>
    /// <param name="a_maximumStartTicks"></param>
    /// <param name="a_possibleStartTicks"></param>
    /// <returns></returns>
    private static bool StartDateOkay(long a_earliestStartTicks, long a_maximumStartTicks, long a_possibleStartTicks)
    {
        return a_possibleStartTicks <= a_maximumStartTicks;
    }

    internal SchedulableResult RecalcSchedulableResult(ScenarioDetail a_sd,
                                                       long a_simClock,
                                                       InternalActivity a_ia,
                                                       bool a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable,
                                                       ResourceBlock a_block,
                                                       ResourceBlockList.Node a_leftNeighborBlockListNode,
                                                       RequiredSpanPlusSetup a_cachedSetupSpan)
    {
        LinkedListNode<ResourceCapacityInterval> rciNode = ResultantCapacity.FindForwards(a_block.StartTicks, null);
        ResourceOperation ro = (ResourceOperation)a_ia.Operation;

        LeftNeighborSequenceValues sequenceVals = new LeftNeighborSequenceValues(a_leftNeighborBlockListNode);
        RequiredCapacityPlus cachedSpan = new (RequiredSpanPlusClean.s_notInit, a_cachedSetupSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit, 0, 0m);
        RequiredCapacityPlus rc = CalculateTotalRequiredCapacity(a_simClock, a_ia, sequenceVals, true, a_block.StartTicks, a_sd.ExtensionController, cachedSpan);
        SchedulableResult result = PrimarySchedulable(a_sd, a_simClock, a_ia, a_findCapacityOnlyIgnoreReservedAndMultiTaskingAttentionAvailable, ro.ResourceRequirements.PrimaryResourceRequirement, a_block.StartTicks, sequenceVals, rc, rciNode, false);

        return result;
    }
}