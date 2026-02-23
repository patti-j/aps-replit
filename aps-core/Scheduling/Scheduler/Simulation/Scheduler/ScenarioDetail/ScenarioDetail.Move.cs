using Microsoft.Extensions.Primitives;
using PT.APSCommon;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;
using PT.Scheduler.Simulation.Extensions;
using PT.Scheduler.Simulation.UndoReceive.Move;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using System.Collections;
using System.Diagnostics;
using System.Text;

using PT.Scheduler.AuditObject;
using static PT.Scheduler.ManufacturingOrder;

namespace PT.Scheduler;

partial class ScenarioDetail
{
    /// <summary>
    /// If a move failure has occurred, handle it and return true.
    /// </summary>
    /// <param name="a_moveT">The move transmission.</param>
    /// <param name="simType">The type of simulation.</param>
    /// <param name="moveResult">The move result.</param>
    /// <returns>Whether a move failure has been handled.</returns>
    private bool HandleMoveFailure(ScenarioDetailMoveT a_moveT, SimulationType simType, MoveResult moveResult, IScenarioDataChanges a_dataChanges)
    {
        bool failureHandled = false;
        if (moveResult.FailureCount != 0)
        {
            MoveFailedAudit(a_moveT, moveResult, a_dataChanges);
         
            

            ScenarioEvents se;
            using (_scenario.AutoEnterScenarioEvents(out se))
            {
                se.FireMoveFailedEvent(this, moveResult);
                SimulationProgress.FireSimulationProgressEvent(this, simType, a_moveT, SimulationProgress.Status.Terminated, m_nbrOfSimulationsSinceStartup);
                failureHandled = true;
            }
        }

        return failureHandled;
    }

    private void MoveFailedAudit(ScenarioDetailMoveT a_moveT, MoveResult moveResult, IScenarioDataChanges a_dataChanges)
    {
        foreach (MoveBlockKeyData moveBlockKeyData in a_moveT)
        {
            IEnumerator<ActivityKey> enumerator = moveBlockKeyData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ActivityKey activityKey = enumerator.Current;
                InternalActivity internalActivity = JobManager.FindActivity(activityKey);
                Department department = internalActivity.Batch?.PrimaryResource?.Department;

                ActivityMoveFailedAudit activityMoveAudit = new ActivityMoveFailedAudit();
                activityMoveAudit.ActivityId = internalActivity.Id.ToString();
                activityMoveAudit.OperationName = internalActivity.Operation.Name;
                activityMoveAudit.JobName = internalActivity.Operation.Job.Name;
                activityMoveAudit.ManufacturingOrderName = internalActivity.ManufacturingOrder.Name;
                activityMoveAudit.FromDepartmentName = department?.Name ?? string.Empty;
                activityMoveAudit.FromPlantName = department?.Plant?.Name ?? string.Empty;
                activityMoveAudit.FromResourceName = internalActivity.Batch?.PrimaryResource?.Name ?? string.Empty;
                activityMoveAudit.MoveFromDate = internalActivity.ScheduledStartDate;

                if (department != null)
                {
                    activityMoveAudit.MovedFromDepartmentFrozenSpan = ClockDate.Add(department.FrozenSpan) > activityMoveAudit.MoveFromDate;
                    activityMoveAudit.MovedFromPlantStableSpan = ClockDate.Add(department.Plant!.StableSpan) > activityMoveAudit.MoveFromDate;
                }

                StringBuilder builder = new StringBuilder();
                foreach (MoveBlockProblems moveProblem in moveResult)
                {
                    foreach (MoveProblem moveBlockProblem in moveProblem)
                    {
                        builder.Append(moveBlockProblem.MoveProblemEnum);
                        builder.Append(",");
                    }
                }

                activityMoveAudit.FailedReason = builder.ToString();

                a_dataChanges.AuditEntry(activityMoveAudit.GetAuditEntry(), true);
            }
        }
    }

    private MoveData CreateMoveInfo(ScenarioDetailMoveT a_moveT, MoveResult a_result)
    {
        if (a_moveT.PreBatchSerialized)
        {
            // Convert from PreBatch MoveT.
            // This is for backwards compatbility with recordings that pre-date resource batching.
            // The FromResourceKey and Activities need to be filled in.
            IEnumerator<MoveBlockKeyData> etr = a_moveT.GetEnumerator();
            etr.MoveNext();
            MoveBlockKeyData mbkd = etr.Current;

            if (mbkd.ResourceKey == null)
            {
                List<Resource> resources = PlantManager.GetResourceList();
                foreach (Resource res in resources)
                {
                    Block block = res.Blocks.FindByKey(mbkd.BlockKey);
                    if (res != null)
                    {
                        ResourceKey rk = res.GetResourceKey();
                        mbkd.SetResourceKey(rk);
                        break;
                    }
                }
            }
        }

        MoveData md = new (a_moveT);

        // Find and validate to resource.
        if (a_moveT.ToResourceKey == null)
        {
            a_result.SetFailureReason(MoveFailureEnum.ToResNotSpecified);
        }

        md.ToResource = MoveHelperFindResource(a_moveT.ToResourceKey);


        if (md.ToResource == null)
        {
            a_result.SetFailureReason(MoveFailureEnum.ToResourceNotFound);
        }

        if (md.ToResource != null && !md.ToResource.Active)
        {
            a_result.SetFailureReason(MoveFailureEnum.ToResNotActive);
        }

        if (md.ToResource != null && md.ToResource.DisallowDragAndDrops)
        {
            a_result.SetFailureReason(MoveFailureEnum.ToResDisallowsDragAndDrop);
        }


        // Add the Move blocks.
        IEnumerator<MoveBlockKeyData> mbkdEtr = a_moveT.GetEnumerator();
        while (mbkdEtr.MoveNext())
        {
            // Whether to exclude the block because it has problems that prevent it from being moved.
            bool excludedBlockFromMoveDueToProblems = false;
            MoveBlockKeyData mbkd = mbkdEtr.Current;

            Resource fromRes = MoveHelperFindResource(mbkd.ResourceKey);
            if (fromRes == null)
            {
                // This error shouldn't happen to users unless there's a bug in the UI that shouldn't have made it into a released build.
                a_result.SetFailureReason(MoveFailureEnum.ResourceNotFound);
                continue;
            }

            ResourceBlock block = fromRes.Blocks.FindByKey(mbkd.BlockKey);
            if (block == null)
            {
                // This error shouldn't happen to users unless there's a bug in the UI that shouldn't have made it into a released build.
                a_result.SetFailureReason(MoveFailureEnum.BlockNotFound);
                continue;
            }

            List<InternalActivity> activities = new ();
            IEnumerator<ActivityKey> actEtr = mbkd.GetEnumerator();
            // Whether the block is in production.
            bool inProduction = false;

            while (actEtr.MoveNext())
            {
                ActivityKey actKey = actEtr.Current;

                InternalActivity act = block.Batch.FindActivity(actKey.ActivityId);
                if (act == null)
                {
                    // This error shouldn't happen to users unless there's a bug in the UI that shouldn't have made it into a released build.
                    a_result.SetFailureReason(MoveFailureEnum.ActivityNotFound);
                    continue;
                }

                if (act.InProduction())
                {
                    inProduction = true;
                }

                activities.Add(act);
            }

            if (activities.Count == 0)
            {
                // This error shouldn't happen to users unless there's a bug in the UI that shouldn't have made it into a released build.
                a_result.SetFailureReason(MoveFailureEnum.NoMoveActivitiesSpecified);
                continue;
            }

            MoveBlockData mbd = new (block, activities);

            if (fromRes.DisallowDragAndDrops)
            {
                a_result.Add(new MoveProblem(MoveProblemEnum.FromResDisallowDragAndDrop), mbd);
                excludedBlockFromMoveDueToProblems = true;
            }

            if (inProduction)
            {
                a_result.Add(new MoveProblem(MoveProblemEnum.InProduction), mbd);
                excludedBlockFromMoveDueToProblems = true;
            }

            if (!excludedBlockFromMoveDueToProblems)
            {
                md.AddMoveBlock(mbd);
            }
        }

        md.StartTicksAdjusted = md.StartTicks;

        if (md.StartTicksAdjusted < Clock)
        {
            md.StartTicksAdjusted = Clock;
        }

        if (!md.Exact)
        {
            //Update the move time to just after any block currently scheduled at the move time
            md.StartTicksAdjusted = AdjustMoveTimeForNonExactMove(md);
        }

        // Add the KeepOnCurResActivities
        a_moveT.ValidateMoveAndKeepOnSetsAreDisjoint();
        IEnumerator<ActivityKey> keepOnEtr = a_moveT.GetKeepOnCurResActivitiesEnumerator();

        while (keepOnEtr.MoveNext())
        {
            InternalActivity ia = JobManager.FindActivity(keepOnEtr.Current);
            md.AddKeepOnCurResAct(ia);
        }

        if (md.Count > 0)
        {
            ResourceBlock moveBlock = md[0].Block;
            md.MoveRRIdx = moveBlock.ResourceRequirementIndex;
        }

        return md;
    }

    /// <summary>
    /// Returns a move time based on the desired move time taking into account current scheduling data.
    /// Considers Blocks scheduled at the time and the operation's latest constraint.
    /// Accepts and returns times in Display format.
    /// </summary>
    /// <returns>A time greater or equal to the initial time</returns>
    private long AdjustMoveTimeForNonExactMove(MoveData a_moveData)
    {
        DateTime originalDate = new (a_moveData.StartTicksAdjusted);

        long maxTime = originalDate.Ticks;
        //Set to EndTime of the target block if dragged onto a block.
        Resource r = a_moveData.ToResource;

        if (r.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            List<ResourceBlockList.Node> blocksList = r.Blocks.FindAllBlocksContainingDate(maxTime);

            if (a_moveData.CampaignMove)
            {
                //Move at the end of the current campaign
                List<Campaign> campaigns = r.GetCampaigns(DateTime.MaxValue);
                foreach (Campaign c in campaigns)
                {
                    if (c.Start <= originalDate && c.End >= originalDate)
                    {
                        maxTime = c.End.Ticks + 1;
                        break;
                    }
                }
            }
            else
            {
                foreach (ResourceBlockList.Node block in blocksList)
                {
                    //The block exists. Validate that the block being dropped on, is not one of the blocks moving
                    bool blockAtDropTimeNotBeingMoved = true;
                    if (block.Data.Batched)
                    {
                        //This block may have activities that are not being moved.
                        blockAtDropTimeNotBeingMoved = false;
                        IEnumerator<InternalActivity> actList = block.Data.Batch.GetEnumerator();
                        while (actList.MoveNext())
                        {
                            foreach (MoveBlockData keyData in a_moveData)
                            {
                                foreach (InternalActivity act in keyData)
                                {
                                    if (act.Id != actList.Current.Id)
                                    {
                                        //There is an activity in the block that isn't moving.
                                        blockAtDropTimeNotBeingMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (MoveBlockData keyData in a_moveData)
                        {
                            foreach (InternalActivity act in keyData)
                            {
                                if (block.Data.Batch.FindActivity(act.Id) != null)
                                {
                                    //The block contains an activity being moved.
                                    blockAtDropTimeNotBeingMoved = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (blockAtDropTimeNotBeingMoved)
                    {
                        maxTime = Math.Max(maxTime, block.Data.Batch.EndOfStorageTicks);
                    }
                }
            }
        }
        else if (r.CapacityType == InternalResourceDefs.capacityTypes.Infinite)
        {
            //No change to MaxTime. It can schedule at moved time
        }
        else if (r.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
        {
            //TODO: Calculate available people and attention %s to see if the job can schedule at the same time as other jobs
        }

        return maxTime;
    }

    /// <summary>
    /// Reset the release date of the MOs to the move start date.
    /// </summary>
    /// <param name="a_moveT"></param>
    /// <param name="a_md"></param>
    private void MoveResetHoldDateHelper(MoveData a_md)
    {
        if (a_md.ResetHoldDate)
        {
            foreach (MoveBlockData mbd in a_md)
            {
                foreach (InternalActivity act in mbd)
                {
                    act.ManufacturingOrder.HoldUntilTicks = a_md.StartTicksAdjusted;
                }
            }
        }
    }

    /// <summary>
    /// Used to make sure the move resources have been released at the move time. If the activities that are being
    /// moved can't be released for a long period past the move to time and the move resource reservations aren't
    /// released, other activities won't be able to use the resources and large blank spots will appear in the
    /// schedule.
    /// After an attempt to schedule has been performed, if the move to resources haven't been released,
    /// the resources will be released and other activiites will be allowed to try to schedule at the move time.
    /// During a move, the states pass linearly through the values below.
    /// </summary>
    private enum MoveReleaseState
    {
        /// <summary>
        /// The simulation isn't in the move state.
        /// </summary>
        NotMove,

        /// <summary>
        /// A move is occurring by the move ticks hasn't been reached.
        /// </summary>
        WaitingForMoveTicks,

        /// <summary>
        /// The move resources can be released after the next attempt to schedule has been made or if none of the move activities have been placed in resource dispatchers.
        /// </summary>
        CanBeReleased,

        /// <summary>
        /// The move resources have been released.
        /// </summary>
        Released
    }

    private MoveReleaseState m_moveTicksState;

    /// <summary>
    /// Information about a move such as the blocks to move and the move time.
    /// </summary>
    private MoveData m_moveData;

    // [USAGE_CODE] m_undoReapplyMoveT: used to undo and re-apply a MoveT
    /// <summary>
    /// Used to undo and re-apply a MoveT
    /// </summary>
    private UndoReceiveMove m_undoReapplyMoveT;

    internal class MoveNextActSIData
    {
        internal MoveNextActSIData() { }

        internal MoveNextActSIData(InternalActivity a_nextMoveAct, SchedulableInfo a_nextMoveActSI)
        {
            m_nextAct = a_nextMoveAct;
            m_nextMoveActSI = a_nextMoveActSI;
        }

        private readonly InternalActivity m_nextAct;

        internal InternalActivity NextMoveAct => m_nextAct;

        private readonly SchedulableInfo m_nextMoveActSI;

        internal SchedulableInfo NextMoveActSI => m_nextMoveActSI;
    }

    #region [USAGE_CODE]: The set of activities that intersect the move date.
    /// <summary>
    /// The set of RRs whose UsageStart==Run that intersected the move reservation.
    /// If a move needs to be re-received, these are used to backwards calculate when the intersecting activities
    /// should start to best squeeze them in around the move reservation. The end of setup is the start of run. From the
    /// end of setup, the setup time can be back calculated to give the best start time of the activity.
    /// </summary>
    private readonly List<IntersectingRRs> m_moveIntersectors = new ();

    /// <summary>
    /// Add an activity to the set of activities that intersect the move date.
    /// If the move activity schedules at the move date, the start times of these activities can
    /// be back calculated and these re-released to fit perfectly into the schedule, including
    /// to prior to the simulation clock (when the transmission is reapplied).
    /// This is
    /// Step 1
    /// When intersecting activities are found they're kept track of.
    /// Step 2
    /// Occurs after the moved activity is scheduled.
    /// New release times are back calculated for activities in this set.
    /// The times allow the activities to tightly schedule without
    /// intersecting with the move time.
    /// For more details see another function in this class that's called
    /// to save the constraint times.
    /// Step 3
    /// Occurs when the transmission is reapplied by Scenario.
    /// </summary>
    /// <param name="a_intersector"></param>
    /// <param name="a_intersectorRR"></param>
    /// <param name="a_res"></param>
    internal void AddMoveIntersector(long a_attemptedStartTicks, long a_intersectionTicks, InternalActivity a_intersector, ResourceRequirement a_intersectorRR, int a_rrIdx, Resource a_res)
    {
        // No need to optimize finding the IntersectingRRs since there shouldn't be many intersectors.
        // On single tasking resources you should only need to worry about the first intersecting RR
        // since placing a constaint on it will constrain its right neighbors.
        IntersectingRRs irrs = null;

        foreach (IntersectingRRs intRRs in m_moveIntersectors)
        {
            if (intRRs.Activity == a_intersector)
            {
                irrs = intRRs;
            }
        }

        if (irrs == null)
        {
            irrs = new IntersectingRRs(a_intersector);
        }

        IntersectingRR irr = new (a_attemptedStartTicks, a_intersector, a_intersectorRR, a_rrIdx, a_res);
        irrs.AddRR(irr);
        m_moveIntersectors.Add(irrs);
    }

    /// <summary>
    /// Iterate through the intersecting activities.
    /// This is used to calculate how to squeeze in intersecting activities.
    /// </summary>
    /// <returns></returns>
    internal IEnumerator<IntersectingRRs> GetEnumeratorOfMoveIntersectors()
    {
        return m_moveIntersectors.GetEnumerator();
    }
    #endregion

    /// <summary>
    /// Used to create a gap between simulations which makes it easier to identify them in the profiler plot
    /// </summary>
    [Conditional("TEST")]
    private void TestSleep() { }

    // [BATCH_CODE]
    private CancelSimulationEnum Move(MoveData a_md, MoveResult a_result, IScenarioDataChanges a_dataChanges)
    {
        TestSleep();

        // Used by activities to lock in on the resources they will be scheduled on. 
        // In a batch merge, it's a copy of the resources of the batch that the activities are being merged to.
        // If it's a regular move, it's the resources it's currently scheduled on with resource requirement being moved changed to the move resource.
        Resource[] rrSatiators = null;

        // Whether blocks being moved are to be merged with the block they're dropped onto.
        bool batchMergeMove = false;
        SimDetailsGroupings simDetails = null; // Must set after SimulationInitialization.

        if (a_md.JoinWithBatchDroppedOntoIfPossible && a_md.ToResource.BatchType != MainResourceDefs.batchType.None)
        {
            ResourceBlockList.Node currentBlockNode = a_md.ToResource.Blocks.FindFirstBlockContainingDate(a_md.StartTicksAdjusted);
            if (currentBlockNode != null)
            {
                simDetails = new SimDetailsGroupings();
                simDetails.BatchSimDetails = new BatchSimDetails();
                simDetails.BatchSimDetails.MoveMerge = new BatchSimDetails.MoveMergeDetails();
                simDetails.BatchSimDetails.MoveMerge.MergeIntoBatch = currentBlockNode.Data.Batch;

                // If the first batch that contains the start date has identicle resource requirements the batches can be merged:
                // The start time will be set to the batch's start time instead of the drop time; both the transmission's and move info's time will be adjusted.
                // The Resource Requirement satiators array will be created and its values set to the Resources used by the Batch the activities are being merged into.
                // Remove blocks from the MoveInfo that shouldn't be included in the move such as activities being dropped from the move to batch back onto to the batch 
                // and activities that aren't compatible with the move to batch.
                ResourceBlock moveToBatchBlock = currentBlockNode.Data;
                InternalActivity moveToBatchActivity = moveToBatchBlock.Activity;

                // If there's only 1 move block and it's the move block, then revert to a normal move.
                // If there are multiple move blocks, only allow the ones that aren't part of the drop to batch to take part in the batch merge.
                if (a_md.Count == 1 && a_md[0].Block == moveToBatchBlock)
                {
                    // The block was dropped on itself. For instance moved a little to the right. No additional processing. Peform a normal move of the batch.
                }
                else
                {
                    batchMergeMove = true;

                    for (int i = a_md.Count - 1; i >= 0; --i)
                    {
                        MoveBlockData mbd = a_md[i];
                        if (mbd.Block == moveToBatchBlock)
                        {
                            // The batch block can't be merged with itself.
                            a_md.RemoveMoveBlock(mbd);
                        }

                        InternalActivity act0 = mbd[0];
                        if (act0.Operation.ResourceRequirements.SimilarityComparison(moveToBatchActivity.Operation.ResourceRequirements) != 0)
                        {
                            // The operation being moved isn't similar enough with the batch to be merged with it.
                            a_md.RemoveMoveBlock(mbd);
                            a_result.Add(new MoveProblem(MoveProblemEnum.NotCompatibleWithMergeBatch), mbd);
                        }
                    }

                    if (a_md.Count == 0)
                    {
                        // All of the activities to be moved weren't eligible to be merged with the batch.
                        a_result.SetFailureReason(MoveFailureEnum.NoMovedActivitiesCompatibleWithMoveToBlocksBatch);
                        return CancelSimulationEnum.Continue;
                    }

                    a_md.StartTicksAdjusted = moveToBatchBlock.StartTicks;
                    // Use a copy of the resource requirement satiators of the Merge To batch as the resource requirement satiators of
                    // the activities being merged.
                    rrSatiators = moveToBatchActivity.CreateArrayOfRRSatiators();
                }
            }
        }

        Common.Testing.Timing ts = CreateTiming("Move");

        #if TEST
            SimDebugSetup();
        #endif

        MoveHelperForResourceEligibilityForAllMoveBlocks(a_md, a_result);

        if (a_md.Count == 0)
        {
            a_result.SetFailureReason(MoveFailureEnum.AllBlocksHadProblems);
            return CancelSimulationEnum.Continue;
        }

        UnlockActivities(a_md, a_result);

        // [TANK_CODE]: This is done to prevent paths with multiple operations that must be scheduled on the same tank from ending up partially scheduled.
        // This is done here because the list must be created before the SimulationInitialization.
        List<TankOperation> tankOpsEvaluated = ConfigureMoveToTheSameTank(a_md);

        SimulationType simType = a_md.ExpediteSuccessors || a_md.MoveT.ExpediteSpecificSuccessors ? SimulationType.MoveAndExpedite : SimulationType.Move;

        List<Tuple<long, long, InternalActivity>> moveActsOrdered = new();
        Hashtable sucMoIdsHash = new();

        foreach (MoveBlockData mbd in a_md)
        {
            foreach (InternalActivity moveAct in mbd)
            {
                moveActsOrdered.Add(new Tuple<long, long, InternalActivity>(moveAct.ScheduledStartTicks(), moveAct.m_scheduledSequence, moveAct));
                if (ScenarioOptions.AllowChangeOfMaterialSuccessorSequenceOnMove)
                {
                    //Collect Material successors recursively 
                    moveAct.ManufacturingOrder.AddMaterialSuccessorsRecursively(sucMoIdsHash, 0);
                }
            }
        }

        // Sort by Scheduled Date. First scheduled are rescheduled first. This should handle subcomponents.
        // Sub sort by ScheduledSequence. This should handle successor operations, MOs, and ties between multi activity operations.
        // The sort must be done before SimulationInitialization wipes out ScheduledSequence.

        //*******************************************************************************************************************************************************************************************************************************************************************
        // What about moving unscheduled jobs? Does this hit the Move function? If so you'll need a stratedgy that can handle a mixture of scheduled and unscheudled jobs.
        // Automatically undo a move that causes unscheduled jobs.
        //*******************************************************************************************************************************************************************************************************************************************************************
        moveActsOrdered.Sort((x, y) =>
        {
            int res = Comparer<long>.Default.Compare(x.Item1, y.Item1);
            if (res == 0)
            {
                res = Comparer<long>.Default.Compare(x.Item2, y.Item2);
            }

            return res;
        });

        MainResourceSet availableResources;
        CreateActiveResourceList(out availableResources);
        SimulationInitializationAll(availableResources, a_md.MoveT, simType, null, simDetails);

        foreach (Job job in JobManager.JobEnumerator)
        {
            //If MOs were resized, set back to original Qty.
            foreach (ManufacturingOrder mo in job.ManufacturingOrders)
            {
                if (mo.Resized)
                {
                    mo.AdjustToOriginalQty(null, this.ProductRuleManager);
                }
            }
        }

        if (simDetails != null && simDetails.BatchSimDetails != null && simDetails.BatchSimDetails.MoveMerge != null)
        {
            foreach (InternalActivity act in simDetails.BatchSimDetails.MoveMerge.MergeIntoBatch)
            {
                // Mark each activity in the move into batch as being part of the move to batch.
                act.MoveIntoBatch = true;
            }
        }

        //Set BeingMoved to true for the moving activities so that the bool can be used when adjusting the move date below
        foreach (MoveBlockData mbd in a_md)
        {
            foreach (InternalActivity moveAct in mbd)
            {
                moveAct.BeingMoved = true;
            }
        }


        m_moveActivitySequence = new List<InternalActivity>();

        InitializeScheduledBlockSequence();
        AdjustMoveDateHelper(a_md);

        // If anchored, unanchor, then reanchor after the move.

        // The MOs involved in the move. This set is used to expedite successors if that move options is set. 
        Dictionary<BaseId, ManufacturingOrder> moveMOs = new ();
        MoveResetHoldDateHelper(a_md);

        // Obtain a list of all down stream activities sorted by scheduled date.
        // First create a set of all the operations being moved.
        // Then get all the downstream activities of the operations being moved.
        // Finally sort the list.

        HashSet<InternalOperation> opsBeingMovedHashSet = new ();
        InternalActivity prevMoveAct = null;

        foreach (Tuple<long, long, InternalActivity> actTuple in moveActsOrdered)
        {
            InternalActivity moveAct = actTuple.Item3;

            if (a_md.MoveT.PrependSetupToMoveBlocksRightNeighbors)
            {
                // [PrependSetupToMoveBlocksRightNeighbors] 2-1: Find what will be the new left neigghr of the move blocks right neighbors.
                // Set this block's right neighbor's PrependSetupToMoveBlocksRightNeighborsNewLeftNeighbor
                // to what will become its new left neighbor. This value will be used later during the
                // move initialization to calculate the setup time bbetween this block and its new right
                // neighbor. The release date of the block will then be backed off by this setup time so 
                // processing can start at the same time it did before the move and the block's end time 
                // remains stable. 
                ResourceBlockList.Node moveActsBlockNode = moveAct.Batch.GetBlockNodeAtIndex(0);
                if (moveActsBlockNode.Previous != null)
                {
                    if (moveActsBlockNode.Next != null)
                    {
                        ResourceBlock prevBlock = moveActsBlockNode.Previous.Data;
                        ResourceBlock nextBlock = moveActsBlockNode.Next.Data;

                        InternalActivity rightNeighbor = nextBlock.Batch.GetFirstActivity();

                        rightNeighbor.PrependSetupToMoveBlocksRightNeighborsNewLeftNeighbor = moveActsBlockNode.Previous;
                    }
                }
            }

            // Configure the sequence the blocks must be scheduled on the resource.
            ++m_unscheduledActivitiesBeingMovedCount;
            bool sameBatchAsPrevAct = prevMoveAct != null && prevMoveAct.Batch == moveAct.Batch;
            moveAct.WaitForLeftMoveBlockToSchedule = m_unscheduledActivitiesBeingMovedCount > 1 && !batchMergeMove && !sameBatchAsPrevAct; // Don't wait for left neighbor if batches are being merged.
            m_moveActivitySequence.Add(moveAct);

            if (!opsBeingMovedHashSet.Contains(moveAct.Operation))
            {
                opsBeingMovedHashSet.Add(moveAct.Operation);
            }

            prevMoveAct = moveAct;
        }

        DownStreamActivities activitiesToUnschedule = new ();

        foreach (Tuple<long, long, InternalActivity> actTuple in moveActsOrdered)
        {
            InternalActivity moveAct = actTuple.Item3;
            activitiesToUnschedule.AddActSchedTicksAndDownstreamActs(moveAct);
        }

        activitiesToUnschedule.ReverseSortByScheduledStartDate();

        /**********************************************************************************************
         * 1. Get rid of UnscheduleOpAndSuccessors()
         * 2. What's going on with Tank unschedule.
         * /**********************************************************************************************
Change the code so that all of the activities that are to be unscheduled are unscheduled from the latest scheduled activity to the earliest. Otherwise you may still end up with the same problems you're running into right now. The code changes you have now can still fail if the sequence of activities is just right.
For instance A->AA and B->BB if they're scheduled as:
A      B
               X   BB   AA
Then B and BB will be unscheduled first.
Then A and AA will be unscheduled and clear the RightNeighbor value.
        /**********************************************************************************************/
        UnscheduleType unscheduleType;
        if (a_md.ExpediteSuccessors || a_md.MoveT.ExpediteSpecificSuccessors)
        {
            unscheduleType = UnscheduleType.MoveAndExpediteSuccessors;
        }
        else
        {
            unscheduleType = UnscheduleType.Move;
        }

        // Link the individual activities to their destination resource. Note they all must be moved from the same resource
        // and typically are destined for the same resource, thought it's possible for them to be destined to different resources.
        IEnumerator<MoveBlockData> resEtr = a_md.GetEnumerator();
        resEtr.MoveNext();
        Resource fromRes = resEtr.Current.Block.ScheduledResource;
        MoveFailureEnum failReason = SetMoveToResources(a_md, fromRes, rrSatiators);
        if (ReserveNextMoveActivity(Clock, a_md.StartTicksAdjusted) == CancelSimulationEnum.Cancel)
        {
            return CancelSimulationEnum.Cancel;
        }

        if (failReason != MoveFailureEnum.None)
        {
            a_result.SetFailureReason(failReason);
            return CancelSimulationEnum.Continue;
        }

        HashSet<BaseId> activitiesToReAnchor = new ();
        // Unschedule and setup each activity being moved for the move and
        // unschedule their suceessors.
        foreach (Tuple<long, long, InternalActivity> actTuple in moveActsOrdered)
        {
            InternalActivity moveAct = actTuple.Item3;
            moveAct.BreakAwayResReqsEligibility();

            // Handle reanchoring.
            if (a_md.AnchorMove)
            {
                moveAct.Operation.SetAnchor(moveAct, true, ScenarioOptions);
            }

            if (moveAct.Anchored)
            {
                moveAct.ReanchorSetup();
                activitiesToReAnchor.Add(moveAct.Id);
                moveAct.Operation.SetAnchor(moveAct, false, ScenarioOptions);
            }

            if (simType == SimulationType.MoveAndExpedite)
            {
                moveAct.ManufacturingOrder.ReanchorSetup();
            }

            // [TANK_CODE]: Unschedule tank
            /*
             * This is wrong. This should only be called once, at the time the activity is being configured. Verify the change will make sense and make the fix.
             * */
            foreach (TankOperation to in tankOpsEvaluated)
            {
                UnscheduleOpAndSuccessors(to, unscheduleType, a_md, false);
            }

            moveAct.Unschedule(false, true);
            moveAct.SetupWaitForAnchorSetFlag();
            moveAct.Operation.ManufacturingOrder.UnscheduledMOMarker = true;

            //***************************************************************************
            // It's not clear why this is being done for each move activity.
            // It seems like it might be doable after each activity is scheduled.
            // Maybe it's to release all the move activities at the same time allowing
            // whichever one is capable of scheduling to schedule.
            // Test this on GPI multiple move examples.
            //***************************************************************************
            AddEvent(new MoveActivityTimeEvent(a_md.StartTicksAdjusted, moveAct, a_md.ToResource));

            moveAct.WaitForActivityMovedEvent = true;

            SetupMaterialConstraints(moveAct, 0);

            if (!moveMOs.ContainsKey(moveAct.ManufacturingOrder.Id))
            {
                moveMOs.Add(moveAct.ManufacturingOrder.Id, moveAct.ManufacturingOrder);
            }

            // Unschedule the successor operations of the activity being moved.
            InternalOperation moveOp = moveAct.Operation;
            AlternatePath.Node moveOpNode = moveOp.AlternatePathNode;
            foreach (AlternatePath.Association sucAsn in moveOpNode.Successors)
            {
                UnschedMoveSucOpn(sucAsn.Successor.Operation, unscheduleType, a_md);
            }

            //Unschedule material successors
            IDictionaryEnumerator enumerator = sucMoIdsHash.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ManufacturingOrderLevel moLvl = (ManufacturingOrderLevel)enumerator.Value;
                if (moLvl.MO.Id == moveOp.ManufacturingOrder.Id)
                {
                    continue;
                }

                for (var i = 0; i < moLvl.MO.OperationManager.Count; i++)
                {
                    BaseOperation suppliedOp = moLvl.MO.OperationManager.GetByIndex(i);
                    if (suppliedOp is InternalOperation iOp && !iOp.IsFinishedOrOmitted)
                    {
                        AlternatePath.Node suppliedOpNode = iOp.AlternatePathNode;
                        foreach (AlternatePath.Association sucAsn in suppliedOpNode.Successors)
                        {
                            UnschedMoveSucOpn(sucAsn.Successor.Operation, unscheduleType, a_md);
                        }

                        foreach (InternalActivity iOpActivity in iOp.Activities)
                        {
                            UnscheduleSuccessorActOfMovedAct(iOpActivity, unscheduleType, false);
                        }
                    }
                }
            }
        }

        if (a_md.ExpediteSuccessors || a_md.MoveT.ExpediteSpecificSuccessors)
        {
            foreach (ManufacturingOrder mo in moveMOs.Values)
            {
                mo.AdjustedPlantResourceEligibilitySets_Filter(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter.NoFilter());
            }
        }

        // Unschedule all other activities in the system and prepare for a simulation.
        UnscheduleInProcessActivitiesAndAddInProcessReleaseEvents(true);

        ResourceActivitySets nonExpediteActivities = new (availableResources);
        UnscheduleActivities(availableResources, new SimulationTimePoint(Clock), new SimulationTimePoint(EndOfPlanningHorizon), nonExpediteActivities, simType, Clock);

        MoveDispatcherDefinition dispatcherDefinition = new (new BaseId(0));
        SimulationResourceInitialization(new SimulationResourceDispatcherUsageArgs(Clock, dispatcherDefinition));

        CreatePreventMoveIntersectionEventsAndResetUndoReapplyMoveT();

        // Sequentially schedule the other activities and the moved activities
        // successor activities.
        // Once a successor activities constraints have been satisfied it will
        // receive priority during the sequential rescheduling. Its constraints are:
        // Its predecessor activities ready time and the original time it was scheduled.
        // Unschedule the activity to be expedited and its successors.
        if (Simulate(Clock, nonExpediteActivities, simType, a_md.MoveT) == CancelSimulationEnum.Cancel)
        {
            return CancelSimulationEnum.Cancel;
        }

        StopTiming(ts, false);

        MoveReanchoringAndResourceLocking(a_md, simType, activitiesToReAnchor);

        MoveClearDefaultsResource(a_md);

#if TEST
            TestSchedule(simType.ToString());
#endif
        if (SetScheduledStatusAndUnschedPartiallySchedJobs() > 0)
        {
            a_result.SetFailureReason(MoveFailureEnum.JobsFailedToSchedule);
            return CancelSimulationEnum.Cancel;
        }

        TestSleep();

        return CancelSimulationEnum.Continue;
    }

    private void ActivityPreMoveAudit(MoveData a_moveData, Dictionary<ActivityKey, ActivityMoveAudit> a_moveAudits)
    {
        foreach (MoveBlockData moveBlockData in a_moveData)
        {
            foreach (InternalActivity moveAct in moveBlockData)
            {
                Department department = moveAct.Batch?.PrimaryResource?.Department;

                ActivityMoveAudit activityMoveAudit = new ActivityMoveAudit();
                activityMoveAudit.ActivityId = moveAct.Id.ToString();
                activityMoveAudit.OperationName = moveAct.Operation.Name;
                activityMoveAudit.JobName = moveAct.Operation.Job.Name;
                activityMoveAudit.ManufacturingOrderName = moveAct.ManufacturingOrder.Name;
                activityMoveAudit.FromDepartmentName = department?.Name ?? string.Empty;
                activityMoveAudit.FromPlantName = department?.Plant?.Name ?? string.Empty;
                activityMoveAudit.FromResourceName = moveAct.Batch?.PrimaryResource?.Name ?? string.Empty;
                activityMoveAudit.MoveFromDate = moveAct.ScheduledStartDate;

                activityMoveAudit.MovedFromDepartmentFrozenSpan = department != null && ClockDate.Add(department.FrozenSpan) > activityMoveAudit.MoveFromDate;
                activityMoveAudit.MovedFromPlantStableSpan = department != null && ClockDate.Add(department.Plant.StableSpan) > activityMoveAudit.MoveFromDate;

                a_moveAudits.Add(moveAct.CreateActivityKey(), activityMoveAudit);
            }
        }
    }
    private void ActivityPostMoveAudit(IScenarioDataChanges a_dataChanges, MoveData md, Dictionary<ActivityKey, ActivityMoveAudit> moveAudits)
    {
        foreach (MoveBlockData moveBlockData in md)
        {
            foreach (InternalActivity internalActivity in moveBlockData)
            {
                ActivityKey activityKey = internalActivity.CreateActivityKey();


                if (moveAudits.TryGetValue(activityKey, out ActivityMoveAudit activityMoveAudit))
                {
                    Department department = internalActivity.Batch?.PrimaryResource?.Department;

                    activityMoveAudit.ToDepartmentName = department?.Name ?? string.Empty;
                    activityMoveAudit.ToPlantName = department?.Plant?.Name ?? string.Empty;
                    activityMoveAudit.ToResourceName = internalActivity.Batch?.PrimaryResource?.Name ?? string.Empty;
                    activityMoveAudit.MoveToDate = internalActivity.ScheduledStartDate;

                    activityMoveAudit.MovedToDepartmentFrozenSpan = department != null && ClockDate.Add(department.FrozenSpan) < activityMoveAudit.MoveToDate;
                    activityMoveAudit.MovedToPlantStableSpan = department != null && ClockDate.Add(department.Plant.StableSpan) < activityMoveAudit.MoveToDate;

                    a_dataChanges.AuditEntry(activityMoveAudit.GetAuditEntry(), true);
                }
            }
        }
    }

    /// <summary>
    /// A helper of Move.
    /// Unschedule the successors of the move operations.
    /// The successor is unscheduled and its successors are recursively unscheduled
    /// and configured for sequential simulation (must wait for unfinished, unomitted
    /// operations to schedule  first.
    /// </summary>
    /// <param name="a_sucMoveOpn"></param>
    /// <param name="a_unscheduleType"></param>
    /// <param name="a_md"></param>
    private void UnschedMoveSucOpn(InternalOperation a_sucMoveOpn, UnscheduleType a_unscheduleType, MoveData a_md)
    {
        // Whether the activity being scheduled has to wait on a moving predecessor before being scheduled.
        if (a_sucMoveOpn.IsNotFinishedAndNotOmitted)
        {
            bool unschedule = true;
            if (a_md != null && a_md.MoveT.ExpediteSpecificSuccessors)
            {
                HashSet<BaseId> successorOpIdsToExpedite = a_md.MoveT.GetSuccessorOpIdsToExpedite(a_sucMoveOpn.Job.Id);
                if (!successorOpIdsToExpedite.Contains(a_sucMoveOpn.Id))
                {
                    unschedule = false;
                }
            }

            if (unschedule || a_md.ExpediteSuccessors)
            {
                for (int actI = 0; actI < a_sucMoveOpn.Activities.Count; ++actI)
                {
                    InternalActivity act = a_sucMoveOpn.Activities.GetByIndex(actI);
                    UnscheduleSuccessorActOfMovedAct(act, a_unscheduleType, true);
                }
            }

            AlternatePath.Node apNode = a_sucMoveOpn.AlternatePathNode;
            foreach (AlternatePath.Association sucAsn in apNode.Successors)
            {
                InternalOperation successorOperation = sucAsn.Successor.Operation;
                UnschedMoveSucOpn(successorOperation, a_unscheduleType, a_md);
            }
        }
    }

    /// <summary>
    /// [USAGE_CODE]: Helper of Move() & MoveAlternatePath. Prevent activities that intersect the move time (found in the previous sim) from intersecting in the current sim.
    /// </summary>
    private void CreatePreventMoveIntersectionEventsAndResetUndoReapplyMoveT()
    {
        // [USAGE_CODE] Move: Create PreventMoveIntersectionEvent events during a re-receive of a moveT.
        if (m_undoReapplyMoveT != null)
        {
            IEnumerator<ActivityReleaseTime> constrainedEtr = m_undoReapplyMoveT.GetEnumeratorOfIntersectorReleases();
            while (constrainedEtr.MoveNext())
            {
                ActivityKey ak = constrainedEtr.Current.ActivityKey;
                InternalActivity act = JobManager.FindActivity(ak);
                PreventMoveIntersectionEvent evt = new (constrainedEtr.Current.ReleaseTicks, act);
                act.PreventMoveIntersectionEvent = true;
                AddEvent(evt);
            }

            m_undoReapplyMoveT.Reset();
        }
    }

    /// <summary>
    /// Adjust the move date for factors such as Usage, block intersection, offline capacity intervals, exact/non-exact, etc.
    /// Usages can vary among the Resource Requirements of an operation, but the Move simuation accepts a move start time of
    /// the activity to be moved. So in the event a block for a resource requirement is moved but doesn't require time setup time
    /// while others do, the move time must be adjusted back to translate the the drop timeof the resource requirement into the start time
    /// of the activity.
    /// For example:
    /// RR1: Requires 2 hours of setup and processing
    /// RR2: Requires only processing.
    /// If RR2 is moved, the move date needs to be back adjusted by 2 hours worth of capacity to translate the move of the RR
    /// into a move of the activity.
    /// This function currently only handles the exclusion of Setup.
    /// </summary>
    /// <param name="a_md"></param>
    //void AdjustMoveDateHelper(MoveData a_md)
    //{
    //    //---------------------------------------------------------------------------------------------------------------------------------
    //    // If UsageStarts vary between ResourceRequirements, find the most constraining helper resource. The time it can schedule its block
    //    // will determine how much the move time will need to be offset forwards in time.
    //    //---------------------------------------------------------------------------------------------------------------------------------
    //    ResourceBlock moveBlock = a_md[0].Block;
    //    Batch moveBatch = moveBlock.Batch;
    //    InternalActivity moveAct = moveBatch.GetFirstActivity();
    //    InternalOperation moveOp = moveAct.Operation;

    //    ResourceRequirement moveRR = moveOp.ResourceRequirements.GetByIndex(a_md.MoveRRIdx);

    //    Resource primaryRes = moveBatch.PrimaryResource;

    //    // If any predecessor operations are scheduled after the move date, use the available date of the predecessor operation as the adjusted move date.
    //    long predOpConstraint = moveOp.Predecessors.CalcLatestScheduledDate();
    //    a_md.StartTicksAdjusted = Math.Max(a_md.StartTicksAdjusted, predOpConstraint);

    //    // [USAGE_CODE] AdjustMoveDateHelper: Calculate the adjusted start time on the primary based on the interval of time where the block will be scheduled.
    //    if (moveRR.UsageStart == MainResourceDefs.usageEnum.Run)
    //    {
    //        // The move time is the start of processing because the block being dropped starts at the Run.
    //        // It needs to be adjusted backwards by the amount of setup.
    //        // From the start of processing, find the start of setup.
    //        RequiredCapacity rc = new RequiredCapacity(moveBatch.SetupSpan, RequiredSpan.ZeroRequiredSpan, RequiredSpan.ZeroRequiredSpan);
    //        Resource.FindStartFromEndResult findResult = primaryRes.FindStartTimeFromEndDate(Clock, moveAct, rc, a_md.StartTicksAdjusted);

    //        if (findResult.success)
    //        {
    //            a_md.StartTicksAdjusted = findResult.startTicks;
    //        }
    //        else
    //        {
    //            // I presume the start time couldn't be found because it would be before the Clock.
    //            a_md.StartTicksAdjusted = Clock;
    //        }
    //    }

    //    // Find the batch that intersects the move time.
    //    Batch batchThatIntersectsMoveTime = null;
    //    if (moveBlock.ScheduledResource.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
    //    {
    //        ResourceBlock intersectingBlock = primaryRes.GetBlockAt(a_md.StartTicksAdjusted);
    //        if (intersectingBlock != null)
    //        {
    //            batchThatIntersectsMoveTime = intersectingBlock.Batch;
    //        }
    //    }

    //    // Determine the start times of different stages.
    //    SchedulableResult sr = primaryRes.PrimarySchedulable(this, Clock, a_md.StartTicksAdjusted, moveBlock);

    //    // [USAGE_CODE] AdjustMoveDateHelper(): If the block being moved resource requirement's UsageStart==Run, use the processing start ticks as the needs ticks.
    //    if (sr.m_result == SchedulableSuccessFailureEnum.Success) // else we can't calculate an adjustment to the move start ticks.
    //    {
    //        long earliestStartConstraint = sr.m_si.m_scheduledStartDate;
    //        for (int rrI = 0; rrI < moveBatch.BlockCount; ++rrI)
    //        {
    //            ResourceRequirement curRR = moveOp.ResourceRequirements.GetByIndex(rrI);
    //            long needTicks;
    //            if (curRR.UsageStart == MainResourceDefs.usageEnum.Run)
    //            {
    //                needTicks = sr.m_si.ProcStartDate;
    //            }
    //            else
    //            {
    //                needTicks = sr.m_si.m_scheduledStartDate;
    //            }
    //            long maxCIConstraint = SoonestActiveTimeHelper(needTicks, a_md.ToResource);
    //            long resMax = Math.Max(earliestStartConstraint, maxCIConstraint);

    //            if (curRR.UsageStart == MainResourceDefs.usageEnum.Run)
    //            {
    //                // Convert the time from start of processing to start of setup.
    //                RequiredCapacity rc = new RequiredCapacity(moveBatch.SetupSpan, RequiredSpan.ZeroRequiredSpan, RequiredSpan.ZeroRequiredSpan);
    //                Resource.FindStartFromEndResult findResult = primaryRes.FindStartTimeFromEndDate(Clock, moveAct, rc, resMax);
    //                if (findResult.success)
    //                {
    //                    resMax = findResult.startTicks;
    //                }
    //                else
    //                {
    //                    resMax = Clock;
    //    }
    //            }
    //            earliestStartConstraint = Math.Max(earliestStartConstraint, resMax);
    //            a_md.StartTicksAdjusted = earliestStartConstraint;
    //        }
    //    }

    //    // [USAGE_CODE] AdjustMoveDateHelper(): Save the EndOfProcessing and EndOfPostProcessing.
    //    if (moveRR.UsageStart == MainResourceDefs.usageEnum.Run)
    //    {
    //        // Find the end of processing and the end of post processing.
    //        SchedulableSuccessFailureEnum findCapacityResult;

    //        long endOfProcessing;
    //        findCapacityResult = primaryRes.FindCapacity(a_md.StartTicksAdjusted, moveBatch.ProcessingSpan.TimeSpanTicks, moveOp.CanPause, moveAct, true, out endOfProcessing);

    //        long endOfPostProcessing = endOfProcessing;

    //        if (findCapacityResult == SchedulableSuccessFailureEnum.Success)
    //        {
    //            findCapacityResult = primaryRes.FindCapacity(endOfProcessing, moveBatch.PostProcessingSpan.TimeSpanTicks, moveOp.CanPause, moveAct, true, out endOfPostProcessing);
    //        }

    //        if (findCapacityResult != SchedulableSuccessFailureEnum.Success)
    //        {
    //            throw new PTValidationException("The move couldn't be performed because of " + findCapacityResult.ToString());
    //        }

    //        a_md.EndOfProcessingTicks = endOfProcessing;
    //        a_md.EndOfPostProcessingTicks = endOfPostProcessing;
    //    }
    //}
    private void AdjustMoveDateHelper(MoveData a_md)
    {
        //---------------------------------------------------------------------------------------------------------------------------------
        // If UsageStarts vary between ResourceRequirements, find the most constraining helper resource. The time it can schedule its block
        // will determine how much the move time will need to be offset forwards in time.
        //---------------------------------------------------------------------------------------------------------------------------------
        ResourceBlock moveBlock = a_md[0].Block;
        Batch moveBatch = moveBlock.Batch;
        InternalActivity moveAct = moveBatch.GetFirstActivity();
        InternalOperation moveOp = moveAct.Operation;

        ResourceRequirement moveRR = moveOp.ResourceRequirements.GetByIndex(a_md.MoveRRIdx);
        Resource toRes = a_md.ToResource;

        //Adjust move date if needed due to latest predecessor overlap or transfer constraint
        long predOpConstraint = Clock;
        for (int i = 0; i < moveOp.Predecessors.Count; ++i)
        {
            AlternatePath.Association association = moveOp.Predecessors[i];
            InternalOperation predOp = association.Predecessor.Operation;
            if (predOp.IsOmitted)
            {
                continue;
            }

            foreach (InternalActivity predOpActivity in predOp.Activities)
            {
                long constrainedStartTime = predOpActivity.GetOverlapConstrainedReleaseDateForSuccessor(moveAct, association.OverlapType);
                if (constrainedStartTime > predOpConstraint)
                {
                    predOpConstraint = constrainedStartTime;
                }
            }
        }

        a_md.StartTicksAdjusted = Math.Max(a_md.StartTicksAdjusted, predOpConstraint);
        long helperStartTicks = a_md.StartTicksAdjusted;
        
        // [USAGE_CODE] AdjustMoveDateHelper: Calculate the adjusted start time on the primary based on the interval of time where the block will be scheduled.
        if (a_md.MoveRRIdx != 0 && moveRR.UsageStart == MainResourceDefs.usageEnum.Run)
        {
            // The move time is the start of processing because the block being dropped starts at the Run.
            // It needs to be adjusted backwards by the amount of setup.
            // From the start of processing, find the start of setup.
            RequiredCapacity rc = new (RequiredSpanPlusClean.s_notInit, moveBatch.SetupCapacitySpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
            Resource.FindStartFromEndResult findResult = toRes.FindCapacityReverse(Clock, a_md.StartTicksAdjusted, rc, null, moveAct);

            if (findResult.Success)
            {
                a_md.StartTicksAdjusted = findResult.StartTicks;
            }
            else
            {
                // I presume the start time couldn't be found because it would be before the Clock.
                a_md.StartTicksAdjusted = Clock;
            }
        }

        SchedulableSuccessFailureEnum capacityResult = SchedulableSuccessFailureEnum.NotSet;
        SchedulableResult sr;
        long retryTime = 0;
        if (a_md.MoveRRIdx == 0)
        {
            sr = toRes.PrimarySchedulable(this, a_md.StartTicksAdjusted, a_md.StartTicksAdjusted, moveBlock, ExtensionController, false);
            capacityResult = sr.m_result;
            retryTime = sr.RetryTime;
        }
        else
        {
            // Determine the start times of different stages.
            sr = moveBatch.PrimaryResource.PrimarySchedulable(this, a_md.StartTicksAdjusted, a_md.StartTicksAdjusted, moveBatch.PrimaryResourceBlock, ExtensionController, false);
            retryTime = sr.RetryTime;
            if (sr.m_result == SchedulableSuccessFailureEnum.Success)
            {
                //We are moving a helper
                ResourceSatiator[] satiators = new ResourceSatiator[moveOp.ResourceRequirements.Count];
                satiators[0] = new ResourceSatiator(moveBatch.PrimaryResource);
                LinkedListNode<ResourceCapacityInterval> theNode = toRes.FindFirstOnlineRCIForward(helperStartTicks);
                
                Resource.NonPrimarySchedulableResult nonPrimarySchedulableResult = toRes.NonPrimarySchedulable(this, moveAct, moveRR, satiators, sr.m_si, theNode);
                capacityResult = nonPrimarySchedulableResult.Availability;
                retryTime = nonPrimarySchedulableResult.TryAgainTicks;
            }
        }

        // Determine the start times of different stages.

        // [USAGE_CODE] AdjustMoveDateHelper(): If the block being moved UsageStart==Run, use the processing start ticks as the needs ticks.
        if (capacityResult == SchedulableSuccessFailureEnum.Success) // else we can't calculate an adjustment to the move start ticks.
        {
            long earliestStartConstraint = a_md.StartTicksAdjusted;
            for (int blockI = 0; blockI < moveBatch.BlockCount; ++blockI)
            {
                ResourceBlockList.Node blockINode = moveBatch.GetBlockNodeAtIndex(blockI);
                if (blockINode != null) // It's possible the RR doesn't need to be scheduled. For example if it's only necessary setup, but there's no setup. The block won't be scheduled.
                {
                    ResourceBlock curBlock = blockINode.Data;
                    ResourceRequirement curBlockRR = moveOp.ResourceRequirements.GetByIndex(curBlock.ResourceRequirementIndex);
                    long needTicks = a_md.StartTicksAdjusted;
                    //if (curBlockRR.UsageStart == MainResourceDefs.usageEnum.Run)
                    //{
                    //    needTicks = sr.m_si.ProcStartDate;
                    //}

                    long maxCIConstraint = SoonestActiveTimeHelper(needTicks, a_md.ToResource);
                    long resMax = Math.Max(earliestStartConstraint, maxCIConstraint);

                    if (curBlockRR.UsageStart == MainResourceDefs.usageEnum.Run)
                    {
                        // Convert the time from start of processing to start of setup.
                        RequiredCapacity rc = new (RequiredSpanPlusClean.s_notInit, moveBatch.SetupCapacitySpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        Resource.FindStartFromEndResult findResult = toRes.FindCapacityReverse(Clock, resMax, rc, null, moveAct);
                        if (findResult.Success)
                        {
                            resMax = findResult.StartTicks;
                        }
                        else
                        {
                            resMax = Clock;
                        }
                    }

                    earliestStartConstraint = Math.Max(earliestStartConstraint, resMax);
                }
            }

            a_md.StartTicksAdjusted = Math.Max(earliestStartConstraint, a_md.StartTicksAdjusted);
        }
        else
        {
            a_md.StartTicksAdjusted = Math.Max(a_md.StartTicksAdjusted, retryTime);
        }

        //TODO: this is probably not needed? MoveData.EndOfProcessingTicks and MoveData.EndPostProcessingTicks are not used anywhere
        // [USAGE_CODE] AdjustMoveDateHelper(): Save the EndOfProcessing and EndOfPostProcessing.
        //if (moveRR.UsageStart == MainResourceDefs.usageEnum.Run)
        //{
        //    bool late = !moveAct.JITStartDateNotSet && a_md.StartTicksAdjusted > moveAct.JitStartTicks;

        //    // Find the end of processing and the end of post processing.
        //    ActivityResourceBufferInfo bufferInfo = moveAct.GetBufferResourceInfo(primaryRes.Id);
        //    FindCapacityResult findCapacityResult = primaryRes.FindCapacity(a_md.StartTicksAdjusted, moveBatch.ProcessingCapacitySpan.TimeSpanTicks, moveOp.CanPause, null, MainResourceDefs.usageEnum.Run, false, false, moveRR.CapacityCode, false, moveAct.JITStartDateNotSet, moveAct.JitStartTicks, moveAct.PeopleUsage, moveAct.NbrOfPeople);

        //    a_md.EndOfProcessingTicks = findCapacityResult.FinishDate;

        //    if (findCapacityResult.ResultStatus == SchedulableSuccessFailureEnum.Success)
        //    {
        //        findCapacityResult = primaryRes.FindCapacity(a_md.EndOfProcessingTicks, moveBatch.PostProcessingSpan.TimeSpanTicks, moveOp.CanPause, null, MainResourceDefs.usageEnum.PostProcessing, false, false, moveRR.CapacityCode, false, moveAct.JITStartDateNotSet, moveAct.JitStartTicks, moveAct.PeopleUsage, moveAct.NbrOfPeople);
        //    }

        //    if (findCapacityResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
        //    {
        //        throw new PTValidationException("The move couldn't be performed because of " + findCapacityResult);
        //    }

        //    a_md.EndOfPostProcessingTicks = findCapacityResult.FinishDate;
        //}
    }

    /// <summary>
    /// This is a helper function of AdjustMoveDatesForUsages().
    /// Return the soonest active time on a resource on or after a date.
    /// </summary>
    /// <param name="a_procStartTicks">Start time of search for active time on the resource.</param>
    /// <param name="a_res">The resource to search.</param>
    /// <returns>The first active time found.</returns>
    private long SoonestActiveTimeHelper(long a_procStartTicks, Resource a_res)
    {
        long soonestAvailableTicks = a_procStartTicks;
        LinkedListNode<ResourceCapacityInterval> node = a_res.FindContainingCapacityIntervalNode(a_procStartTicks, null);

        if (!node.Value.Active)
        {
            // Search forwards for the next active time.
            while ((node = node.Next) != null)
            {
                if (node.Value.Active)
                {
                    soonestAvailableTicks = node.Value.StartDate;
                    break;
                }
            }
        }

        return soonestAvailableTicks;
    }

    // No longer necessary after MoveT.Exact effectively became obsolete; all moves are exact. The client handles non-exact moves by specifying a date after the end of another 
    // block.
    ///// <summary>
    ///// This is a helper function of AdjustMoveDatesForUsages().
    ///// Return the end of a block that intersects a time (if any).
    ///// </summary>
    ///// <param name="a_procStartTicks">The time at which to look for a block.</param>
    ///// <param name="a_res">The resource to look for an intersecting block on.</param>
    ///// <param name="a_batch">The block is ignored if it's part of this batch.</param>
    ///// <returns>The end of the intersecting block or the search time parameter if no block intesects it.</returns>
    //long EndOfIntersectingBlockHelper(long a_procStartTicks, ResourceRequirement a_moveRR, ResourceRequirement a_curBlocksRR, Batch a_batchIntersectingMoveBlock, Resource a_res, Batch a_batch)
    //{
    //    long blockConstraint = a_procStartTicks;

    //    ResourceBlock intersectingBlock = a_res.GetBlockAt(a_procStartTicks);
    //    Batch intersectingBatch = null;

    //    if (intersectingBlock != null)
    //    {
    //        intersectingBatch = intersectingBlock.Batch;
    //    }

    //    if ((a_moveRR == a_curBlocksRR) || (a_batchIntersectingMoveBlock != null && a_batchIntersectingMoveBlock == intersectingBatch))
    //    {
    //        if (intersectingBlock != null && intersectingBlock.Batch != a_batch)
    //        {
    //            blockConstraint = intersectingBlock.EndTicks;
    //        }
    //    }

    //    return blockConstraint;
    //}

    private static void UnlockActivities(MoveData a_md, MoveResult a_result)
    {
        foreach (MoveBlockData mbd in a_md)
        {
            foreach (InternalActivity act in mbd)
            {
                ResourceBlock block = mbd.Block;
                if (a_md.ToResource.Id != block.ScheduledResource.Id)
                {
                    if (act.ResourceRequirementLocked(block.ResourceRequirementIndex))
                    {
                        act.UnlockResourceRequirement(block.ResourceRequirementIndex);
                    }
                }
            }
        }
    }

    private MoveFailureEnum SetMoveToResources(MoveData a_md, Resource a_fromRes, Resource[] rrSatiators)
    {
        // Set the resources the activity will schedule on.
        foreach (MoveBlockData mbd in a_md)
        {
            foreach (InternalActivity act in mbd)
            {
                if (rrSatiators != null)
                {
                    act.InitMoveRRSet(rrSatiators);
                }
                else
                {
                    /// Verify the block being moved isn't dropped onto a resource that's already being used to satisfy a different RR.
                    act.InitMoveRRSet();
                    int maxRRIdx = act.Operation.ResourceRequirements.Requirements.Count;
                    for (int i = 0; i < maxRRIdx; ++i)
                    {
                        if (mbd.Block.ResourceRequirementIndex != i)
                        {
                            Resource res = act.GetMoveResource(i);
                            if (res != null &&
                                res.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking // non-single tasking resources may be able to satisfy multiple RRs.
                                &&
                                a_md.ToResource == res) // The move to resource can't be to a resource in use by another RR.
                            {
                                return MoveFailureEnum.MoveToResUsedByOtherReq;
                            }
                        }
                    }

                    act.SetMoveResource(a_md.ToResource, mbd.Block.ResourceRequirementIndex);
                }
            }
        }

        // Verify multitasking resource isn't asked to supply more than 100 percent of attention if
        // an attempt is made to move an activity onto a resource that satisfies multiple
        // resource requirements of the activity.
        foreach (MoveBlockData mbd in a_md)
        {
            foreach (InternalActivity act in mbd)
            {
                if (AttentionPctCheck(act, a_md.ToResource) == false)
                {
                    return MoveFailureEnum.MoveToResUsedByOtherReq;
                }
            }
        }

        return MoveFailureEnum.None;
    }

    /// <summary>
    /// A helper function of move.
    /// Whether the multitasking resources total required attention percent is less than or equal to 100 percent.
    /// For example if an activity had 2 resource requirements.
    /// 1.Requirement 1 needing 50 percent attention on Resource A.
    /// 2.Requirement 2 needing 51 percent attention scheduled on Resoure B but Resource A is also capable of satisfying the requirment.
    /// If an attempt is made to move Requirement 2 from Resource B to Resource A then this function will return false
    /// indicating there's not enough attention to satisfy both resource requirements.
    /// </summary>
    /// <param name="a_act">The activity being moved to check. </param>
    /// <param name="a_moveToRes">The resource activities are being moved to.</param>
    /// <returns></returns>
    private bool AttentionPctCheck(InternalActivity a_act, Resource a_moveToRes)
    {
        bool ret = true;
        if (a_moveToRes.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
        {
            int pct = 0;
            for (int rrI = 0; rrI < a_act.Operation.ResourceRequirements.Count; ++rrI)
            {
                Resource res = a_act.GetMoveResource(rrI);
                if (res == null)
                {
                    //It's possible the helper doesn't use capacity because the usage isn't scheduled.
                    continue;
                }
                if (res.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking && res == a_moveToRes)
                {
                    ResourceRequirement rr = a_act.Operation.ResourceRequirements.GetByIndex(rrI);
                    pct += rr.AttentionPercent;
                }
            }

            // Whether the total percent is less than or equal to 100 percent. 
            ret = pct <= 100;
        }

        return ret;
    }

    private void MoveReanchoringAndResourceLocking(MoveData a_md, SimulationType a_simType, HashSet<BaseId> a_activitiesToReAnchor)
    {
        foreach (MoveBlockData mbd in a_md)
        {
            foreach (InternalActivity act in mbd)
            {
                if (a_activitiesToReAnchor.Contains(act.Id))
                {
                    act.Operation.SetAnchor(act, true, ScenarioOptions);

                    if (a_simType == SimulationType.MoveAndExpedite)
                    {
                        act.ManufacturingOrder.Reanchor(ScenarioOptions);
                    }
                }

                if (a_md.LockMove)
                {
                    act.LockResourceRequirement(mbd.Block.ResourceRequirementIndex);
                }
            }
        }
    }

    private void MoveClearDefaultsResource(MoveData a_md)
    {
        List<Job> recomputeJobs = new ();
        /*
         * There's too much error checking in here that could be covering up bugs.
         * Remove error checking and run through mass recordings.
         * */
        foreach (MoveBlockData mbd in a_md)
        {
            ResourceBlock block = mbd.Block;
            if (block.SatisfiedRequirement != null)
            {
                if (a_md.ToResource != null)
                {
                    if (block.SatisfiedRequirement.DefaultResource != null && block.SatisfiedRequirement.DefaultResource != a_md.ToResource)
                    {
                        block.SatisfiedRequirement.DefaultResource_Clear();
                        recomputeJobs.Add(block.SatisfiedRequirement.Operation.Job);
                    }
                }
            }
        }

        foreach (Job j in recomputeJobs)
        {
            j.ComputeEligibilityAndUnscheduleIfIneligible(m_productRuleManager);
        }
    }

    // [USAGE_CODE2] ReserveNextMoveActivity: delay a move activity to when it should be scheduled based on prior application of the MoveT.
    /// <summary>
    /// Delay the next move activity if the time it should be scheduled has been determined by a previous re-apply
    /// of the ScenarioDetailMoveT.
    /// </summary>
    /// <param name="a_moveScheduleTicks">The expected move release time (unless replaying is being done).</param>
    private CancelSimulationEnum ReserveNextMoveActivity(long a_simClock, long a_moveScheduleTicks)
    {
        InternalActivity nextMoveActivity = GetNextSequencedMoveActivity();
        CancelSimulationEnum ret = CancelSimulationEnum.Continue;
        long moveTicks = a_moveScheduleTicks;

        if (m_undoReapplyMoveT != null)
        {
            ActivityReleaseTime moveDelay;
            bool knownSchedTime = m_undoReapplyMoveT.GetKnownMoveTicks(m_nextSequencedMoveActivityToSchedule, out moveDelay);
            if (knownSchedTime)
            {
                moveTicks = Math.Max(a_moveScheduleTicks, moveDelay.ReleaseTicks);
            }
        }

        m_moveTicksState = MoveReleaseState.WaitingForMoveTicks;
        MoveTicksEvent mte = new (moveTicks, nextMoveActivity);
        AddEvent(mte);

        if (m_undoReapplyMoveT != null && m_undoReapplyMoveT.ReapplyType != UndoReceiveMove.ReapplyTypeEnum.NonLockingMoveRelease) // The move activity must schedule at the move time.
        {
            ret = ReserveMoveResources(a_simClock, nextMoveActivity, moveTicks);
        }

        return ret;
    }

    private CancelSimulationEnum ReserveMoveResources(long a_simClock, InternalActivity a_nextMoveAct, long a_moveTicks)
    {
        // For subsequent move activities, you need to calculate the SI.
        // There are 2 possibilities:
        // 

        SchedulableInfo si;
        CancelSimulationEnum ret = CancelSimulationEnum.Continue;
        Resource primaryRes = a_nextMoveAct.GetMoveResource(a_nextMoveAct.Operation.ResourceRequirements.PrimaryResourceRequirementIndex);
        SchedulableResult sr = primaryRes.PrimarySchedulable(this, a_moveTicks, a_moveTicks, a_nextMoveAct, ExtensionController, false);
        if (sr.m_result != SchedulableSuccessFailureEnum.Success)
        {
            // Find the next online interval and move it there.
            //PT.Scheduler.LinkedListNode<ResourceCapacityInterval> node =primaryRes.FindContainingCapacityIntervalNode(a_moveTicks, null);
            //DateTime nextOnlineCapacityInterval= primaryRes.FindFirstOnlineIntervalStartOnOrAfterPoint(node.Data.EndDateTime);
            //node = primaryRes.FindContainingCapacityIntervalNode(nextOnlineCapacityInterval.Ticks, null);
            //m_undoReapplyMoveT.UndoReceivedTransmission = true;
            //m_undoReapplyMoveT.ReReceiveTransmission = true;
            //m_undoReapplyMoveT.ReReceiveMoveTicks = node.Data.StartDate;

            return CancelSimulationEnum.Continue;
        }

        si = sr.m_si;
       
        a_nextMoveAct.ReserveMoveResources(a_simClock, a_moveTicks, si);
        return CancelSimulationEnum.Continue;
    }

    // [TANK_CODE]: TODO: Should this also be applied during an optimize? It seems like it's possible for tank2 to be scheduled first. Or maybe levels should be used as a tie breaker during an optimize.

    /// <summary>
    /// [TANK CODE]: Prevent jobs with multiple activities on the same tank from becoming parially scheduled during a move.
    /// Adds related right neighbors as constrained activities of the moved activity.
    /// Prevent a move from causing a tank deadlock condition.
    /// For instance given ops tank1, tank2, pack1, pack 2
    /// Where
    /// tank1's successor is pack1,
    /// tank2's successor is pack2
    /// pack1's successor is pack2
    /// and an optimize that schedules as follows on the tank and pack line
    /// Pack Line:      pack1     pack2
    /// Tank:      tank1     tank2
    /// If you were to drag operation Tank1 after Tank2, Tank1 and its successors wouldn't be able to schedule because they'd be waiting on Tank2 to be released,
    /// but Tank2 won't be released until Pack2 is run, but Pack2 can't schedule until Pack1 has been scheduled.
    /// </summary>
    /// <param name="a_md"></param>
    /// <returns>A list of operations that should be unscheduled and rescheduled after the activities of the blocks being moved.</returns>
    private List<TankOperation> ConfigureMoveToTheSameTank(MoveData a_md)
    {
        List<TankOperation> tanksOperationsEvaluated = new ();

        if (a_md.ToResource.IsTank)
        {
            foreach (MoveBlockData mbd in a_md)
            {
                if (mbd.Block.ScheduledResource == a_md.ToResource)
                {
                    Resource tankRes = a_md.ToResource;
                    ResourceBlockList.Node curBlockNode = mbd.Block.MachineBlockListNode.Next;
                    while (curBlockNode != null)
                    {
                        ResourceBlock block = curBlockNode.Data;
                        IEnumerator<InternalActivity> batchActIt = block.Batch.GetEnumerator();
                        while (batchActIt.MoveNext())
                        {
                            InternalActivity act = batchActIt.Current;
                            foreach (InternalActivity movedAct in mbd)
                            {
                                if (act.ManufacturingOrder == movedAct.ManufacturingOrder)
                                {
                                    //TODO: SA
                                    //TankOperation to = act.Operation as TankOperation;
                                    //if (to != null)
                                    //{
                                    //    List<Product> productsStoredInTank = to.GetProductsToStoreInTank();

                                    //    if (productsStoredInTank.Count > 0)
                                    //    {
                                    //        SameTankMoveHelper_EvaluateSuccessors(to, to, (ResourceOperation)movedAct.Operation, movedAct, productsStoredInTank);
                                    //        tanksOperationsEvaluated.Add(to);
                                    //    }
                                    //}
                                }
                            }
                        }

                        curBlockNode = curBlockNode.Next;
                    }
                }
            }
        }

        return tanksOperationsEvaluated;
    }

    // [TANK CODE]: Prevent activities on the same tank from becoming parially scheduled.
    private void SameTankMoveHelper_EvaluateSuccessors(ResourceOperation a_opToEvaluate, ResourceOperation a_rightNeighborOpOnTank, ResourceOperation a_movedOpOnTank, InternalActivity a_MovedActOnTank, List<Product> a_productsStoredInTank)
    {
        for (int sucI = 0; sucI < a_opToEvaluate.Successors.Count; ++sucI)
        {
            ResourceOperation suc = a_opToEvaluate.Successors[sucI].Successor.Operation;

            if (suc.Scheduled)
            {
                for (int matlI = 0; matlI < suc.MaterialRequirements.Count; ++matlI)
                {
                    MaterialRequirement mr = suc.MaterialRequirements[matlI];
                    for (int prodI = 0; prodI < a_productsStoredInTank.Count; ++prodI)
                    {
                        Product p = a_productsStoredInTank[prodI];
                        if (mr.Item == p.Item && mr.MRSupply != null) //MRSupply can be null if the successor is past the Planning Horizon
                        {
                            foreach (Adjustment adjustment in mr.MRSupply)
                            {
                                InternalActivity supply = adjustment.GetReason() as InternalActivity;
                                if (supply.Operation == a_rightNeighborOpOnTank)
                                {
                                    SameTankMoveHelper_EvaluatePredecessors(suc, a_rightNeighborOpOnTank, a_movedOpOnTank, a_MovedActOnTank, a_productsStoredInTank);
                                }
                            }
                        }
                    }
                }
            }

            SameTankMoveHelper_EvaluateSuccessors(suc, a_rightNeighborOpOnTank, a_movedOpOnTank, a_MovedActOnTank, a_productsStoredInTank);
        }
    }

    // [TANK CODE]: Prevent activities on the same tank from becoming parially scheduled.
    /// <summary>
    /// Adds related right neighbors as constrained activities of the moved activity.
    /// </summary>
    /// <param name="a_opToEvaluate"></param>
    /// <param name="a_rightNeighborOpOnTank"></param>
    /// <param name="a_movedOpOnTank"></param>
    /// <param name="a_movedActOnTank"></param>
    /// <param name="a_productsStoredInTank"></param>
    private void SameTankMoveHelper_EvaluatePredecessors(ResourceOperation a_opToEvaluate, ResourceOperation a_rightNeighborOpOnTank, ResourceOperation a_movedOpOnTank, InternalActivity a_movedActOnTank, List<Product> a_productsStoredInTank)
    {
        for (int predI = 0; predI < a_opToEvaluate.Predecessors.Count; ++predI)
        {
            ResourceOperation pred = (ResourceOperation)a_opToEvaluate.Predecessors[predI].Predecessor.Operation;

            if (pred == a_rightNeighborOpOnTank)
            {
                continue;
            }

            if (pred == a_movedOpOnTank)
            {
                a_movedActOnTank.AddMoveRelatedConstrainedActivity(a_rightNeighborOpOnTank.Activities.GetByIndex(0));
                //for (int matlI = 0; matlI < pred.MaterialRequirements.Count; ++matlI)
                //{
                //    MaterialRequirement mr = pred.MaterialRequirements[matlI];

                //    for (int prodI = 0; prodI < a_productsStoredInTank.Count; ++prodI)
                //    {
                //        Product prod = a_productsStoredInTank[prodI];
                //        if (mr.Item == prod.Item)
                //        {
                //            Dictionary<long, MRSupplyNode>.Enumerator etr = mr.MRSupply.SupplySet.GetEnumerator();
                //            while (etr.MoveNext())
                //            {
                //                InternalActivity supply = etr.Current.Value.Supply as InternalActivity;
                //                if (supply == a_MovedActOnTank)
                //                {
                //                    // Create constraints.
                //                    a_MovedActOnTank.AddMoveRelatedConstrainedActivity(supply);
                //                }
                //            }
                //        }
                //    }
                //}
            }

            SameTankMoveHelper_EvaluatePredecessors(pred, a_rightNeighborOpOnTank, a_movedOpOnTank, a_movedActOnTank, a_productsStoredInTank);
        }
    }

    //void SameTankMoveHelper_EvaluatePredecessors(ResourceOperation a_opToEvaluate, ResourceOperation a_rightNeighborOpOnTank, ResourceOperation a_movedOpOnTank, InternalActivity a_MovedActOnTank, List<Product> a_productsStoredInTank)
    //{
    //    for (int predI = 0; predI < a_opToEvaluate.Predecessors.Count; ++predI)
    //    {
    //        ResourceOperation pred = (ResourceOperation)a_opToEvaluate.Predecessors[predI].Predecessor.Operation;

    //        if (pred == a_rightNeighborOpOnTank)
    //        {
    //            continue;
    //        }

    //        if (pred.Scheduled)
    //        {
    //            for (int matlI = 0; matlI < pred.MaterialRequirements.Count; ++matlI)
    //            {
    //                MaterialRequirement mr = pred.MaterialRequirements[matlI];

    //                for (int prodI = 0; prodI < a_productsStoredInTank.Count; ++prodI)
    //                {
    //                    Product prod = a_productsStoredInTank[prodI];
    //                    if (mr.Item == prod.Item)
    //                    {
    //                        Dictionary<long, MRSupplyNode>.Enumerator etr = mr.MRSupply.SupplySet.GetEnumerator();
    //                        while (etr.MoveNext())
    //                        {
    //                            InternalActivity supply = etr.Current.Value.Supply as InternalActivity;
    //                            if (supply == a_MovedActOnTank)
    //                            {
    //                                // Create constraints.
    //                                a_MovedActOnTank.AddMoveRelatedConstrainedActivity(supply);
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        SameTankMoveHelper_EvaluatePredecessors(pred, a_rightNeighborOpOnTank, a_movedOpOnTank, a_MovedActOnTank, a_productsStoredInTank);
    //    }
    //}

    private Resource MoveHelperFindResource(ResourceKey a_rk)
    {
        Resource res = null;

        Plant plant = PlantManager.GetById(a_rk.Plant);
        if (plant != null)
        {
            Department department = plant.Departments.GetById(a_rk.Department);
            if (department != null)
            {
                res = department.Resources.GetById(a_rk.Resource);
            }
        }

        return res;
    }

    /// <summary>
    /// Perform this validation check prior to starting simulation.
    /// </summary>
    /// <param name="res"></param>
    /// <param name="activity"></param>
    /// <param name="resReqIdx"></param>
    private void MoveHelperForResourceEligibilityForAllMoveBlocks(MoveData a_md, MoveResult a_result)
    {
        for (int moveBlockI = a_md.Count - 1; moveBlockI >= 0; --moveBlockI)
        {
            MoveBlockData mbd = a_md[moveBlockI];
            Resource targetResource = a_md.ToResource;
            int resReqIdx = mbd.Block.ResourceRequirementIndex;
            // Just test the first activity. Presume other activities in the same block are all part of the same
            // batch and have identicle resource requirements.
            foreach (InternalActivity act in mbd)
            {
                MoveHelperTestMoveBlockDataResEligibility(a_md, a_result, resReqIdx, mbd, act);
            }
        }

        // Remove activities from MoveInfo that have problems that prevent them from being moved.
        a_result.RemoveProblemedActivities();
        a_md.RemoveEmptyMoveBlocks();
    }

    /// <summary>
    /// A preprocessing step to move to verify the move activities in a MoveBlockData are eligible to be moved to the move ToResource.
    /// Activities that aren't eligible are removed from the MoveData and an problem is added to the MoveResult.
    /// </summary>
    /// <param name="a_md">Contains information about a move being performed. This function will remove ineligible activities from this MoveData, which will indicate they shouldn't be moved.</param>
    /// <param name="a_result">The result of a move. Any eligibility problems that prevent an activity from being moved will be reported through this object.</param>
    /// <param name="a_resReqIdx">The index of the resource requirement that is being moved.</param>
    /// <param name="a_mbd">A MoveBlockData whose activities are being tested for eligibility.</param>
    /// <param name="a_moveActIdx">The index of the activity being processed in a_mbd.</param>
    private void MoveHelperTestMoveBlockDataResEligibility(MoveData a_md, MoveResult a_result, int a_resReqIdx, MoveBlockData a_mbd, InternalActivity a_act)
    {
        ResourceOperation op = (ResourceOperation)a_act.Operation;

        bool rightMachine = false;
        PlantResourceEligibilitySet pres = op.AlternatePathNode.ResReqsMasterEligibilitySet[a_resReqIdx];
        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();

        while (ersEtr.MoveNext())
        {
            EligibleResourceSet eligibleResourceSet = ersEtr.Current.Value;

            for (int i = 0; i < eligibleResourceSet.Count; ++i)
            {
                Resource currentResource = (Resource)eligibleResourceSet[i];
                if (a_md.ToResource == currentResource)
                {
                    if (op.Split)
                    {
                        //We need to check activity eligibility
                        if (currentResource.IsCapableBasedOnMinMaxVolume(a_act, a_act.Operation.AutoSplitInfo, m_productRuleManager) && currentResource.IsCapableBasedOnMinMaxQtys(a_act, a_act.Operation.AutoSplitInfo, m_productRuleManager))
                        {
                            rightMachine = true;
                        }
                    }
                    else
                    {
                        rightMachine = true;
                    }

                    break;
                }
            }
        }

        if (!rightMachine)
        {
            a_md.RemoveMoveBlock(a_mbd);
            ActivityMoveProblem amp = new (MoveProblemEnum.NotEligibleOnTargetRes, a_act, a_resReqIdx);
            a_result.Add(amp, a_mbd);
        }

        // Test eligibility on batch resource.
        if (op.ResourceRequirements.PrimaryResourceRequirementIndex == a_resReqIdx)
        {
            IsBatchResourceCapableOfSchedulingActivityEnum isCapable = IsBatchResourceCapableOfSchedulingActivity(a_md.ToResource, a_act);
            if (isCapable == IsBatchResourceCapableOfSchedulingActivityEnum.NoReqFinQtyIsGreaterThanQtyPerCycle)
            {
                a_result.Add(new MoveProblem(MoveProblemEnum.NotEligibleForBatchResReqCyclesGT1), a_mbd);
                rightMachine = false;
            }
            else if (isCapable == IsBatchResourceCapableOfSchedulingActivityEnum.NoReqFinQtyIsGreaterThanResBatchVolume)
            {
                a_result.Add(new MoveProblem(MoveProblemEnum.NotEligibleForBatchResReqFinQtyGTBatchVolume), a_mbd);
                rightMachine = false;
            }
        }

        if (!rightMachine)
        {
            a_md.RemoveMoveBlock(a_mbd);
        }
    }

    private static RequiredCapacityPlus CalcReqCapacityOfPrimaryMoveRes(long a_simClock, long a_startTicks, InternalActivity moveAct, ExtensionController a_timeCustomizer)
    {
        moveAct.GetMoveResource(0, out Resource movePrimaryRes);

        long start = a_startTicks;
        if (movePrimaryRes.LastScheduledBlockListNode != null)
        {
            start = movePrimaryRes.LastScheduledBlockListNode.Data.EndTicks;
        }

        RequiredCapacityPlus rc = movePrimaryRes.CalculateTotalRequiredCapacity(a_simClock, moveAct, LeftNeighborSequenceValues.NullValues, true, start, a_timeCustomizer);
        return rc;
    }

    private CancelSimulationEnum MoveAlternatePath(ScenarioDetailAlternatePathMoveT a_moveT, MoveData a_md, MoveResult a_moveResult, IScenarioDataChanges a_dataChanges)
    {
        ///****************************************************************************
        /// 2. Handle MOs that have a single path eligible on the resource.
        /// 3. Convert activities already scheduled on the path and resource to regular moves.
        ///****************************************************************************
        Common.Testing.Timing ts = CreateTiming("MoveAlternatePathHelper");

        #if TEST
            SimDebugSetup();
        #endif

        SimulationType simType = a_moveT.ExpediteSuccessors ? SimulationType.MoveAndExpedite : SimulationType.Move;
        MainResourceSet availableResources;
        CreateActiveResourceList(out availableResources);
        SimulationInitializationAll(availableResources, a_moveT, simType, null);

        List<Job> movedJobs = new ();

        Dictionary<InternalActivity, ActivitiesCollection> mbdLeads = new ();

        // Validate activities.
        for (int i = a_md.Count - 1; i >= 0; --i)
        {
            MoveBlockData mbd = a_md[i];
            foreach (InternalActivity mbdAct in mbd)
            {
                ManufacturingOrder mo = mbdAct.ManufacturingOrder;
                AlternatePath newPath;

                if (a_moveT.ActivityAltPaths[mbdAct.Id] == mo.CurrentPath.ExternalId)
                {
                    a_md.RemoveMoveBlock(mbd);
                    a_moveResult.Add(new MoveProblem(MoveProblemEnum.AlternatePathMustDifferFromCurrentPath), mbd);
                }
                else if ((newPath = mo.AlternatePaths.FindByExternalId(a_moveT.ActivityAltPaths[mbdAct.Id])) == null)
                {
                    // Verify the new Alternate Path.

                    // Find all of the paths whose lead operation is eligible on the MoveTo resource.
                    // If there's only 1 path then use that as the new path.
                    // If there are multiple, reject the move of the MO and mark the reason as no eligible path found.
                    a_md.RemoveMoveBlock(mbd);
                    a_moveResult.Add(new ActivityMoveProblem(MoveProblemEnum.AlternatePathNotFound, mbdAct, mbd.Block.ResourceRequirementIndex), mbd);
                }
                else
                {
                    ActivitiesCollection leads = new ();
                    AddOutterUnscheduledActivities(mo, newPath, leads);

                    if (leads.Count != 1)
                    {
                        a_md.RemoveMoveBlock(mbd);
                        a_moveResult.Add(new MoveProblem(MoveProblemEnum.AlternatePathMovesMustHave1LeadActivity), mbd);
                    }
                    else
                    {
                        MoveHelperTestMoveBlockDataResEligibility(a_md, a_moveResult, leads[0].Operation.ResourceRequirements.PrimaryResourceRequirementIndex, mbd, leads[0]);
                        mbdLeads.Add(mbdAct, leads);
                    }
                }
            }
        }

        a_moveResult.RemoveProblemedActivities();

        //Check if all blocks were removed due to problems and set failure reason if so
        if (a_md.Count == 0)
        {
            a_moveResult.SetFailureReason(MoveFailureEnum.AllBlocksHadProblems);
            return CancelSimulationEnum.Cancel;
        }

        MoveResetHoldDateHelper(a_md);

        m_moveActivitySequence = new List<InternalActivity>();

        foreach (MoveBlockData mbd in a_md)
        {
            foreach (InternalActivity mbdAct in mbd)
            {
                ManufacturingOrder mo = mbdAct.ManufacturingOrder;
                
                // Verify the new Alternate Path.
                AlternatePath oldPath = mo.CurrentPath;
                AlternatePath newPath = mo.AlternatePaths.FindByExternalId(a_moveT.ActivityAltPaths[mbdAct.Id]);

                ActivitiesCollection leads = mbdLeads[mbdAct];
                InternalActivity leadAct = leads[0];
                int resourceRequirementIndex = leadAct.Operation.ResourceRequirements.PrimaryResourceRequirementIndex;

                mo.Unschedule();
                mo.CurrentPath_setter = newPath;

                InitializeScheduledBlockSequence();

                leadAct.InitMoveRRSet();

                // Make this the first eligible primary operation,
                // or the first eligible secondary operation of one
                // of the lead activities.
                // TODO
                leadAct.SetMoveResource(a_md.ToResource, resourceRequirementIndex);

                // Unschedule the activity and set it up for a move.
                ++m_unscheduledActivitiesBeingMovedCount;
                leadAct.BeingMoved = true;
                mo.UnscheduledMOMarker = true;

                AddEvent(new MoveActivityTimeEvent(a_md.StartTicksAdjusted, leadAct, a_md.ToResource));
                leadAct.WaitForActivityMovedEvent = true;
                leadAct.WaitForLeftMoveBlockToSchedule = m_unscheduledActivitiesBeingMovedCount > 1;
                m_moveActivitySequence.Add(leadAct);

                SetupMaterialConstraints(leadAct, 0);

                // Unschedule all other activities in the system and prepare for a simulation.
                ReserveMoveResources(Clock, leadAct, a_md.StartTicksAdjusted);

                movedJobs.Add(mo.Job);
            }
        }

        ///****************************************************************************
        /// lock the first lead activity here and sequence the rest like move
        ///****************************************************************************
        UnscheduleInProcessActivitiesAndAddInProcessReleaseEvents(true);
        ResourceActivitySets nonExpediteActivities = new (availableResources);
        UnscheduleActivities(availableResources, new SimulationTimePoint(Clock), new SimulationTimePoint(EndOfPlanningHorizon), nonExpediteActivities, simType, Clock);

        MoveDispatcherDefinition dispatcherDefinition = new (new BaseId(0));
        SimulationResourceInitialization(new SimulationResourceDispatcherUsageArgs(Clock, dispatcherDefinition));

        CreatePreventMoveIntersectionEventsAndResetUndoReapplyMoveT();

        // Sequentially schedule the other activities and the moved activities
        // successor activities.
        // Once a successor activities constraints have been satisfied it will
        // receive priority during the sequential rescheduling. Its constraints are:
        // Its predecessor activities ready time and the original time it was scheduled.
        // Unschedule the activity to be expedited and its successors.
        if (Simulate(Clock, nonExpediteActivities, simType, a_moveT) == CancelSimulationEnum.Cancel)
        {
            return CancelSimulationEnum.Cancel;
        }

        foreach (Job j in movedJobs)
        {
            j.UpdateScheduledStatus();
        }

        StopTiming(ts, false);
        #if TEST
            TestSchedule(simType.ToString());
        #endif
        return CancelSimulationEnum.Continue;
    }

    private enum UnscheduleType { Regular, Move, MoveAndExpediteSuccessors }

    /// <summary>
    /// When a move occurs this is initially set to the number of activities being moved.
    /// As the activities being moved are scheduled, this value is decremented.
    /// </summary>
    private int m_unscheduledActivitiesBeingMovedCount;

    /// <summary>
    /// The sequence the activities to be moved should be scheduled in.
    /// </summary>
    private List<InternalActivity> m_moveActivitySequence;

    /// <summary>
    /// The number of move activities that have been scheduled.
    /// </summary>
    private int m_nextSequencedMoveActivityToSchedule;

    /// <summary>
    /// Get the next move activity to schedule.
    /// </summary>
    /// <returns></returns>
    private InternalActivity GetNextSequencedMoveActivity()
    {
        return m_moveActivitySequence[m_nextSequencedMoveActivityToSchedule];
    }

    /// <summary>
    /// Used to create a set of all the activities of operations and their successor operation's activities.
    /// </summary>
    private class DownStreamActivities : IEnumerable<Tuple<long, InternalActivity>>
    {
        private readonly List<Tuple<long, InternalActivity>> m_actList = new ();
        private readonly HashSet<InternalActivity> m_addedActHashSet = new ();

        internal void AddActSchedTicksAndDownstreamActs(InternalActivity a_act)
        {
            AddActSchedTicks(a_act);
            AddDownStreamActivitiesOfSuccessors(a_act.Operation);
        }

        internal void AddActSchedTicks(InternalActivity a_act)
        {
            long scheduledTicks = a_act.ScheduledStartTicks();
            if (!m_addedActHashSet.Contains(a_act))
            {
                m_addedActHashSet.Add(a_act);
                m_actList.Add(new Tuple<long, InternalActivity>(scheduledTicks, a_act));
            }
        }

        /// <summary>
        /// Adds all the activities of an operation and its successors to the set along with the activity's scheuled start dates.
        /// </summary>
        /// <param name="a_op"></param>
        /// <returns></returns>
        internal void AddDownstreamScheduledTicksAndActivities(InternalOperation a_op)
        {
            // Recursively add success operations.
            AddDownStreamActivitiesOfSuccessors(a_op);

            for (int actI = 0; actI < a_op.Activities.Count; ++actI)
            {
                InternalActivity act = a_op.Activities.GetByIndex(actI);
                AddActSchedTicks(act);
            }
        }

        /// <summary>
        /// Created to recursively add successor operations activities to a list.
        /// </summary>
        /// <param name="a_op">The operation whose successor operation's activities are to be added a list.</param>
        /// <param name="a_actList">The list of activities that's being added to.</param>
        /// <param name="a_addedActHashSet">Used to prevent the same activity from being added to the list multiple times.</param>
        private void AddDownStreamActivitiesOfSuccessors(InternalOperation a_op)
        {
            for (int successorI = 0; successorI < a_op.AlternatePathNode.Successors.Count; ++successorI)
            {
                BaseOperation bo = a_op.AlternatePathNode.Successors[successorI].Successor.Operation;
                if (bo is BaseOperation)
                {
                    InternalOperation op = (InternalOperation)bo;
                    AddDownstreamScheduledTicksAndActivities(op);
                }
            }
        }

        /// <summary>
        /// After adding downstream activities is complete you can call this function to reverse sort the activities by scheduled start date.
        /// </summary>
        internal void ReverseSortByScheduledStartDate()
        {
            m_actList.Sort((x, y) => -Comparer<long>.Default.Compare(x.Item1, y.Item1));
        }

        /// <summary>
        /// Enumerate the set of activities in the order they've been sorted in (if a sort was applied).
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Tuple<long, InternalActivity>> GetEnumerator()
        {
            return m_actList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Format("{0} activities", m_actList.Count);
        }
    }

    private void UnscheduleSuccessors(InternalOperation a_operation, UnscheduleType a_setupForNormalMove, MoveData a_md)
    {
        for (int successorI = 0; successorI < a_operation.AlternatePathNode.Successors.Count; successorI++)
        {
            if (a_operation.AlternatePathNode.Successors[successorI].Successor.Operation is InternalOperation iOp)
            {
                if (a_md != null && a_md.MoveT.ExpediteSpecificSuccessors)
                {
                    HashSet<BaseId> successorOpIdsToExpedite = a_md.MoveT.GetSuccessorOpIdsToExpedite(iOp.Job.Id);
                    if (!successorOpIdsToExpedite.Contains(iOp.Id) && !a_md.ExpediteSuccessors)
                    {
                        continue;
                    }
                }

                UnscheduleOpAndSuccessors(iOp, a_setupForNormalMove, a_md);
            }
        }
    }

    // [TANK_CODE]: added parameter a_hasPredToWaitOn. During a move, if any tank operations need to be unscheduled to prevent a
    // manufacuring operation from ending up partially scheduled, this parameter is passed in as false.
    /// <summary>
    /// This was originally designed for use by UnscheduleSuccessors(). It unschedules the operation and calls UnscheduleSuccessors() to unschedule the operation's
    /// successors.
    /// </summary>
    /// <param name="a_operation">The operation to unschedule. It's successors will also be unscheduled.</param>
    /// <param name="a_setupForNormalMove">The type of Move being performed.</param>
    /// <param name="a_hasPredToWaitOn">
    /// Default = true. Added so this function can be called without setting activity property
    /// WaitForPredecessorOperationReleaseEvent to true. Pass this paramter in as false when the operation being unscheduled doesn't have a predecesor
    /// operation that needed to be rescheduled as part of the Move.
    /// </param>
    /// <param name="a_md"></param>
    private void UnscheduleOpAndSuccessors(InternalOperation a_operation, UnscheduleType a_setupForNormalMove, MoveData a_md, bool a_hasPredToWaitOn = true)
    {
        UnscheduleSuccessors(a_operation, a_setupForNormalMove, a_md);

        if (a_operation.Scheduled)
        {
            for (int activityI = 0; activityI < a_operation.Activities.Count; ++activityI)
            {
                InternalActivity activity = a_operation.Activities.GetByIndex(activityI);
                UnscheduleSuccessorActOfMovedAct(activity, a_setupForNormalMove, a_hasPredToWaitOn);
            }
        }
    }

    // This goes back to ConstraintsChangeAdjustment. How does that fit in with the changes made for Move that prevent the batch from being moved.
    // I think you might need to add a parameter to determine how the batches are handled. 
    // In the case of ConstraintsChangeAdjustment I think you want to remove the activity from the batch since it's allowed to move out of the batch and
    // move further out.

    private void UnscheduleSuccessorActOfMovedAct(InternalActivity act, UnscheduleType a_setupForNormalMove, bool a_hasPredToWaitOn)
    {
        InternalOperation op = act.Operation;

        if (act.Scheduled)
        {
            if (act.InProduction())
            {
                UnscheduleInProcessActivityAndAddInProcessReleaseEvent(act);
            }
            else
            {
                switch (a_setupForNormalMove)
                {
                    case UnscheduleType.Regular:
                        break;

                    case UnscheduleType.MoveAndExpediteSuccessors:
                    case UnscheduleType.Move:
                        // Because multiple activities can be moved, it's possible for the values below to have been set to true
                        // by an early upstream activity and to be part of an operation that's being moved (corresponding to false).
                        // In this case we don't want to allow the true value from being overwritten.
                        if (!act.OpOrPredOpBeingMoved)
                        {
                            act.WaitForPredecessorOperationReleaseEvent = a_hasPredToWaitOn;
                        }

                        break;
                }

                if (act.SetupWaitForAnchorSetFlag())
                {
                    AddAnchorReleaseEvent(act, act.AnchorDateTicks);
                }

                if (a_setupForNormalMove == UnscheduleType.Move)
                {
                    act.OpOrPredOpBeingMoved = true;

                    if (ContinuouslyInsert(op))
                    {
                        act.InitMoveRRSet();
                    }
                    else
                    {
                        act.WaitForScheduledDateBeforeMoveEvent = true;
                        ScheduledDateBeforeMoveEvent upstreamEvent = new (act.GetScheduledStartTicks(), act);
                        AddEvent(upstreamEvent);

                        act.InitMoveRRSet();
                        ResourceBlock rb = act.PrimaryResourceRequirementBlock;

                        if (rb != null && rb.ScheduledResource.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
                        {
                            // Determine whether there is a left neighbor.
                            ResourceBlockList.Node node = rb.MachineBlockListNode;
                            ResourceBlockList.Node previousNode = node.Previous;

                            while (previousNode != null)
                            {
                                IEnumerator<InternalActivity> actEtr = previousNode.Data.Batch.GetEnumerator();
                                bool entireBlockBeingMoved = true;
                                while (actEtr.MoveNext())
                                {
                                    if (!actEtr.Current.BeingMoved)
                                    {
                                        entireBlockBeingMoved = false;
                                        break;
                                    }
                                }

                                if (!entireBlockBeingMoved)
                                {
                                    // Previous node is now set to the node that this activity must wait on before being scheduled.
                                    break;
                                }

                                previousNode = previousNode.Previous;
                            }

                            ///*********************************************************************************************************
                            /// Only consider activities that aren't downstream of a moving activity.
                            /// 1. premark all the downstream activities.
                            /// 2. if you hit a downstream activitiy, skip it and look further back.
                            /// 3. handle multiple activities that have the same upstream left neighbor.
                            ///*********************************************************************************************************
                            if (previousNode != null)
                            {
                                ResourceBlock previousBlock = previousNode.Data;
                                if (previousBlock.Activity.Operation.ManufacturingOrder != op.ManufacturingOrder)
                                {
                                    /// The previous block can't already have been configured for a successor. This can happen when the MO that's being moved has multiple activities on the same resource scheuled back to back.
                                    /// Predecessor and successors within the MO being moved are released in a different way.
                                    if (previousBlock.Activity.RightNeighbor == null)
                                    {
                                        previousBlock.Activity.RightNeighbor = act;
                                        act.HasLeftConstrainingNeighbor = true;
                                        ///*********************************************************************************************************
                                        /// Rename to WaitForLeftNeighborReleaseEvent & document 
                                        ///*********************************************************************************************************
                                        act.WaitForRightMovingNeighborReleaseEvent = true;
                                    }
                                }
                            }
                        }
                    }
                }

                act.Unschedule(false, false);
                act.Operation.ManufacturingOrder.UnscheduledMOMarker = true;
                SetupMaterialConstraints(act, 0);
            }
        }
    }

    /// <summary>
    /// For the primary resource requirement of each scheduled block store the index of where the block was scheduled on the resource.
    /// </summary>
    internal void InitializeScheduledBlockSequence()
    {
        for (int plantI = 0; plantI < PlantManager.Count; ++plantI)
        {
            Plant plant = PlantManager[plantI];

            for (int departmentI = 0; departmentI < plant.Departments.Count; ++departmentI)
            {
                Department department = plant.Departments[departmentI];

                for (int resourceI = 0; resourceI < department.Resources.Count; ++resourceI)
                {
                    Resource resource = department.Resources[resourceI];
                    ResourceBlockList.Node node = resource.Blocks.First;

                    int blocksIndex = -1;

                    while (node != null)
                    {
                        ResourceBlock rb = node.Data;

                        if (rb.ResourceRequirementIndex == rb.Activity.Operation.ResourceRequirements.PrimaryResourceRequirementIndex)
                        {
                            rb.Activity.PrimaryResourceBlockActivityIndex = blocksIndex;
                            IEnumerator<InternalActivity> etr = rb.Batch.GetEnumerator();
                            while (etr.MoveNext())
                            {
                                etr.Current.PrimaryResourceBlockActivityIndex = blocksIndex;
                            }
                        }

                        --blocksIndex;
                        node = node.Next;
                    }
                }
            }
        }
    }
}
