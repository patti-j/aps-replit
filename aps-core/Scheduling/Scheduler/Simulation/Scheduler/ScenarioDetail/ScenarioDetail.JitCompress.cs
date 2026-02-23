using PT.APSCommon;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;
using PT.Transmissions;

//-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

//From: Peter Nelson [mailto:pete.nelson@planettogether.com] 
//Sent: Thursday, September 17, 2009 2:08 PM
//To: 'Larry'
//Subject: RE: Weekly Aircom update

//Larry,

//I won’t be able to answer your questions today directly from them, but this is what I expect and might help you prioritize

//1.	No multi-tasking resources to start.
//***2.	Probably overlap
//3.	No materials
//***4.	Probably multiple resources, but no more than 2 at a time.
//5.	No setups to start.

//Anything I can tell them based on these guesses?

//-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

//  Create a backwards schedule.

//  Simulate the schedule out the add in the setup time.

//  Create operation/activity release dates based on the simulated time. 
// Such as scheduled date - 1 week, bounded by some maximum depending on where it's already scheduled.

//  Resimulate to create the final schedule

// Treat activities as through they were independant operations scheduled at the same level with the same set of predecessor and successor operations.

//-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

// 2015.08.31: Renamed from NeedCompress to JitCompress
namespace PT.Scheduler;

public partial class ScenarioDetail
{
    private void JitCompress(ScenarioIdBaseT a_jitCompressT, IScenarioDataChanges a_dataChanges)
    {
        SimulationType simType = SimulationType.JitCompress;

        try
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_jitCompressT, SimulationProgress.Status.Initializing, m_nbrOfSimulationsSinceStartup);
            Common.Testing.Timing ts = CreateTiming("JitCompress");

            #if TEST
                SimDebugSetup();
            #endif
            List<Pair<InternalActivity, Resource>> activitesSortedByScheduledNbr = GetPairActivitesPrimaryResourceSortedByScheduledNbr();
            List<JitCompressReleaseEvent> jitCompressEventList = new ();
            Dictionary<BaseId, JitCompressReleaseEvent> activityJitCompressEventDictionary = new ();
            Dictionary<BaseId, long> lastStartTickForResDictionary = new ();

            SimulationInitialization1();
            for (int actI = activitesSortedByScheduledNbr.Count - 1; actI >= 0; --actI)
            {
                Pair<InternalActivity, Resource> pair = activitesSortedByScheduledNbr[actI];
                InternalActivity act = pair.value1;

                Resource primaryResource = act.Batch.PrimaryResource;
                m_simStartTime = GetScheduleDefaultStartTime(CompressSettings);
                long startDate = m_simStartTime.GetTimeForResource(act.Batch.PrimaryResource);

                SimulationTimePoint simEndTimePoint = GetScheduleDefaultEndTime(CompressSettings);
                long endDate = simEndTimePoint.GetTimeForResource(act.Batch.PrimaryResource);

                //For some cases the activity should not be moved.
                if (act.Anchored || act.Batch.StartTicks < startDate || act.Batch.StartTicks >= endDate)
                {
                    long startTicks = act.ScheduledStartTicks();
                    JitCompressReleaseEvent arce = new(startTicks, act);
                    jitCompressEventList.Add(arce);
                    activityJitCompressEventDictionary.Add(act.Id, arce);
                    lastStartTickForResDictionary[primaryResource.Id] = startTicks;
                    continue;
                }

                RequiredCapacity rc = new (
                    RequiredSpanPlusClean.s_notInit, //TODO: Clean. I don't think we need to set this, since this portion is already on the previous op
                    act.Batch.SetupCapacitySpan,
                    act.Batch.ProcessingCapacitySpan,
                    act.Batch.PostProcessingSpan,
                    act.Batch.StorageSpan,
                    act.Batch.CleanSpan);

                long adjustedNeedTicks = act.NeedTicks - act.Operation.StandardOperationBufferTicks;
                
                bool hasLastStartTicksForRes = lastStartTickForResDictionary.ContainsKey(primaryResource.Id); // ********** Test for null. **********

                if (hasLastStartTicksForRes)
                {
                    long ticks = lastStartTickForResDictionary[primaryResource.Id]; // ********** Test for null. **********
                    if (ticks < adjustedNeedTicks)
                    {
                        adjustedNeedTicks = ticks;
                    }
                }

                long earliestSucReleaseTicks = GetEarliestSuccessorReleaseHelper(act.Operation, activityJitCompressEventDictionary);
                if (earliestSucReleaseTicks < adjustedNeedTicks)
                {
                    adjustedNeedTicks = earliestSucReleaseTicks;
                }

                long jitCompressTicks = SimClock; //!!This is always 0 here as the simulation hasn't started...
                Resource.FindStartFromEndResult startFromEndResult = pair.value2.FindCapacityReverse(Clock, adjustedNeedTicks, rc, null, act);
                if (startFromEndResult.Success)
                {
                    jitCompressTicks = startFromEndResult.StartTicks;
                }

                //If the JIT date is earlier, just use the already calculated JIT date.
                if (act.DbrJitStartTicks < jitCompressTicks)
                {
                    jitCompressTicks = act.DbrJitStartTicks;
                }

                JitCompressReleaseEvent rce = new (jitCompressTicks, act);
                jitCompressEventList.Add(rce);
                activityJitCompressEventDictionary.Add(act.Id, rce);

                if (hasLastStartTicksForRes)
                {
                    lastStartTickForResDictionary[primaryResource.Id] = jitCompressTicks; // ********** Test for null. **********
                }
                else
                {
                    lastStartTickForResDictionary.Add(primaryResource.Id, jitCompressTicks); // ********** Test for null. **********
                }
            }

            MainResourceSet availableResources;
            CreateActiveResourceList(out availableResources);
            SimulationInitializationAll(availableResources, a_jitCompressT, simType, null);

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

            ResourceActivitySets sequentialResourceActivities = new (availableResources);
            UnscheduleInProcessActivitiesAndAddInProcessReleaseEvents(true);
            UnscheduleActivities(availableResources, new SimulationTimePoint(Clock), new SimulationTimePoint(EndOfPlanningHorizon), sequentialResourceActivities, simType, 0);
            SimulationResourceInitialization(new SimulationResourceDispatcherUsageArgs(Clock, OptimizeSettings.dispatcherSources.NormalRules));

            for (int i = 0; i < jitCompressEventList.Count; ++i)
            {
                JitCompressReleaseEvent e = jitCompressEventList[i];
                e.Activity.WaitForRightCompressReleaseEvent = true;
                AddEvent(e);
            }

            Simulate(Clock, sequentialResourceActivities, simType, a_jitCompressT);

            StopTiming(ts, false);

            m_signals.Clear();

            #if TEST
                TestSchedule("JitCompress");
            #endif
            SimulationActionComplete();
            m_simulationProgress.PostSimulationWorkComplete();
            CheckForRequiredAdditionalSimulation(a_dataChanges);
        }
        catch (Exception)
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_jitCompressT, SimulationProgress.Status.Exception, m_nbrOfSimulationsSinceStartup);
            throw;
        }
    }

    /// <summary>
    /// Return the earliest JitCompressRelease.Time of successor operations. If a successor is omitted or finished, this will recursively process its successors.
    /// If there are no successors or all are omitted or finished, this will return long.MaxValue.
    /// </summary>
    /// <param name="a_io"></param>
    /// <param name="a_activityJitCompressEventDictionary"></param>
    /// <returns></returns>
    private long GetEarliestSuccessorReleaseHelper(InternalOperation a_io, Dictionary<BaseId, JitCompressReleaseEvent> a_activityJitCompressEventDictionary)
    {
        long earliestTicks = long.MaxValue;

        long transferSpan = 0;

        //Get the longest transfer span for successor MOs
        int successMoCount = a_io.ManufacturingOrder.SuccessorMOs.Count;
        for (int i = 0; i < successMoCount; i++)
        {
            SuccessorMO manufacturingOrderSuccessorMO = a_io.ManufacturingOrder.SuccessorMOs[i];

            transferSpan = Math.Max(manufacturingOrderSuccessorMO.TransferSpan, transferSpan);
            
            InternalOperation operation = manufacturingOrderSuccessorMO.Operation as InternalOperation;
            
            if (operation == null)
            {
                Job job = JobManager.GetByExternalId(manufacturingOrderSuccessorMO.SuccessorJobExternalId);
                ManufacturingOrder manufacturingOrder = job?.ManufacturingOrders.GetByExternalId(manufacturingOrderSuccessorMO.SuccessorManufacturingOrderExternalId);
                operation = manufacturingOrder?.GetLeadOperation() as InternalOperation;
            }

            if (operation == null)
            {
                continue;
            }

            bool activityFound = false;
            long activityEarliestTicks = GetEarliestJITCompressReleaseTime(a_activityJitCompressEventDictionary, operation, transferSpan, ref activityFound);

            earliestTicks = Math.Min(activityEarliestTicks, earliestTicks);
        }


        for (int sucI = 0; sucI < a_io.Successors.Count; ++sucI)
        {
            AlternatePath.Association ioSuccessor = a_io.Successors[sucI];
            InternalOperation sucOp = (InternalOperation)ioSuccessor.Successor.Operation;
            bool activityFound = false;
            earliestTicks = GetEarliestJITCompressReleaseTime(a_activityJitCompressEventDictionary, sucOp, ioSuccessor.TransferSpanTicks, ref activityFound);

            if (!activityFound)
            {
                long tempTicks = GetEarliestSuccessorReleaseHelper(sucOp, a_activityJitCompressEventDictionary);
                if (tempTicks < earliestTicks)
                {
                    earliestTicks = tempTicks;
                }
            }
        }

        return earliestTicks;
    }
    private static long GetEarliestJITCompressReleaseTime(Dictionary<BaseId, JitCompressReleaseEvent> a_activityJitCompressEventDictionary, InternalOperation a_sucOp, long a_transferSpan, ref bool a_activityFound)
    {
        long earliestTicks = long.MaxValue;
        for (int sucOpActI = 0; sucOpActI < a_sucOp.Activities.Count; ++sucOpActI)
        {
            InternalActivity sucAct = a_sucOp.Activities.GetByIndex(sucOpActI);
            if (a_activityJitCompressEventDictionary.TryGetValue(sucAct.Id, out JitCompressReleaseEvent jitCompressReleaseEvent))
            {
                a_activityFound = true;
                long earliestReleaseTime = jitCompressReleaseEvent.Time - a_transferSpan;
                if (earliestReleaseTime < earliestTicks)
                {
                    earliestTicks = earliestReleaseTime;
                }
            }
        }

        return earliestTicks;
    }

    private class PairActivityResScheduledSeqNbrSortComparer : IComparer<Pair<InternalActivity, Resource>>
    {
        #region IComparer<Pair<InternalActivity,InternalResource>> Members
        public int Compare(Pair<InternalActivity, Resource> a_x, Pair<InternalActivity, Resource> a_y)
        {
            // TODO: TANK This might not be compatible with the Batch feature since 
            return a_x.value1.m_scheduledSequence.CompareTo(a_y.value1.m_scheduledSequence);
        }
        #endregion
    }

    private List<Pair<InternalActivity, Resource>> GetPairActivitesPrimaryResourceSortedByScheduledNbr()
    {
        List<Resource> resources = PlantManager.GetResourceList();
        List<Pair<InternalActivity, Resource>> activities = new ();

        for (int resI = 0; resI < resources.Count; ++resI)
        {
            Resource res = resources[resI];
            ResourceBlockList.Node curNode = res.Blocks.First;

            while (curNode != null)
            {
                if (curNode.Data.Activity.PrimaryResourceRequirementBlock == curNode.Data) // ********** Test for null. **********
                {
                    activities.Add(new Pair<InternalActivity, Resource>(curNode.Data.Activity, res));
                }

                curNode = curNode.Next;
            }
        }

        activities.Sort(new PairActivityResScheduledSeqNbrSortComparer());
        return activities;
    }
}