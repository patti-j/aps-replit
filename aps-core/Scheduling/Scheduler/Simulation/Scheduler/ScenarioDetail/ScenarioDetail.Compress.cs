using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

partial class ScenarioDetail
{
    /// <summary>
    /// This value is only valid when a CompressT is being processed.
    /// Note the simulation performed is a SimulationType.TimeAdjustment. In the future you can change this to a Compress.
    /// </summary>
    private OptimizeSettings m_activeCompressSettings;

    //This is a workaround to handle subsequent compresses performed from extensions. Ideally settings would not be a member variable
    private readonly Queue<OptimizeSettings> m_cachedActiveCompressSettings = new ();

    private void Compress(ScenarioDetailCompressT a_compressT, OptimizeSettings a_compressSettings, IScenarioDataChanges a_scenarioDataChanges)
    {
        SimulationType simType = SimulationType.Compress;

        try
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_compressT, SimulationProgress.Status.Initializing, m_nbrOfSimulationsSinceStartup);
            Common.Testing.Timing ts = CreateTiming("Compress");

            #if TEST
                SimDebugSetup();
            #endif
            if (m_activeCompressSettings != null)
            {
                m_cachedActiveCompressSettings.Enqueue(m_activeCompressSettings);
            }

            m_activeCompressSettings = a_compressSettings;

            SimulationTimePoint endTime;

            try
            {
                m_simStartTime = GetScheduleDefaultStartTime(m_activeCompressSettings);
                endTime = GetScheduleDefaultEndTime(m_activeCompressSettings);
            }
            catch (DateErrorCheckException)
            {
                throw new SimulationValidationException("2523");
            }

            MainResourceSet availableResources;
            CreateActiveResourceList(out availableResources);
            SimulationInitializationAll(availableResources, a_compressT, simType, null);

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

            ConfigureCompressLimits(m_activeCompressSettings, availableResources);
            UnscheduleInProcessActivitiesAndAddInProcessReleaseEvents(true);
            UnscheduleActivities(availableResources, m_simStartTime, endTime, sequentialResourceActivities, simType, 0);
            SimulationResourceInitialization(new SimulationResourceDispatcherUsageArgs(Clock, OptimizeSettings.dispatcherSources.NormalRules));

            // SimulationType should be set to TimeAdjustment. Check whether it's possible to change this to a Compress.
            Simulate(Clock, sequentialResourceActivities, SimulationType.TimeAdjustment, a_compressT);

            StopTiming(ts, false);

            #if TEST
                TestSchedule("Compress");
            #endif
            SimulationActionComplete();
            m_simulationProgress.PostSimulationWorkComplete();
            CheckForRequiredAdditionalSimulation(a_scenarioDataChanges);
        }
        catch (SimulationValidationException e)
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_compressT, SimulationProgress.Status.Terminated, m_nbrOfSimulationsSinceStartup);
            FireSimulationValidationFailureEvent(e, a_compressT);
            throw;
        }
        catch (Exception)
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_compressT, SimulationProgress.Status.Exception, m_nbrOfSimulationsSinceStartup);
            throw;
        }
        finally
        {
            if (m_cachedActiveCompressSettings.Count > 0)
            {
                m_activeCompressSettings = m_cachedActiveCompressSettings.Dequeue();
            }
            else
            {
                m_activeCompressSettings = null;
            }
            #if TEST
                SimDebugCleanup();
            #endif
        }
    }

    private void ConfigureCompressLimits(OptimizeSettings a_optSettings, MainResourceSet a_availableResources)
    {
        for (int resI = 0; resI < a_availableResources.Count; ++resI)
        {
            if (a_availableResources[resI] is Resource res)
            {
                ResourceBlockList.Node curResNode = res.Blocks.First;
                while (curResNode != null)
                {
                    ResourceBlock block = curResNode.Data;

                    IEnumerator<InternalActivity> batchEtr = block.Batch.GetEnumerator();

                    while (batchEtr.MoveNext())
                    {
                        InternalActivity act = batchEtr.Current;

                        ManufacturingOrder mo = act.ManufacturingOrder;
                        long moReleaseTicks = mo.GetEarliestDepartmentalEndSpan(new SimulationTimePoint(Clock));

                        if (!mo.CompressLimitedByDateDetermined)
                        {
                            if (mo.GetConstrainedReleaseTicks(out long constrainedReleaseTicks))
                            {
                                moReleaseTicks = Math.Max(moReleaseTicks, constrainedReleaseTicks);
                            }

                            //long startTicks = mo.GetScheduledStartTicks();
                            //Note startTicks will be PTDateTime.MaxValue if no act is Started or higher production status
                            //Finished activities are excluded

                            //TODO: This is where we can add an option to no constrain compress to the Headstart
                            //if (!mo.InProcess && startTicks >= DBRReleaseTicks)
                            //{
                            //    mo.CompressLimitedByDBRReleaseTicks = DBRReleaseTicks;
                            //    mo.CompressLimitedByDBRReleaseType = releaseType;
                            //}

                            mo.CompressLimitedByDateDetermined = true;
                        }

                        if (!act.InProductionOrPostProcessing)
                        {
                            if (!act.CompressLimitedByTicksDetermined)
                            {
                                Resource primaryRes = block.Batch.GetRRSatiator(0).Resource;
                                long releaseTicks = ReleaseRuleCaculator.CalculateActivityReleaseDateForReleaseRuleJobRelease(primaryRes, act, a_optSettings, ClockDate, m_productRuleManager);

                                releaseTicks = Math.Max(releaseTicks, moReleaseTicks);

                                if (block.StartTicks >= releaseTicks)
                                {
                                    act.CompressLimitedByTicks = releaseTicks;
                                }

                                act.CompressLimitedByTicksDetermined = true;
                            }
                        }
                    }

                    curResNode = curResNode.Next;
                }
            }
        }
    }

    /// <summary>
    /// Whether the Time Adjustment is a compression. Compression uses the TimeAdjustment simulation type.
    /// </summary>
    private bool IsCompressTimeAdjustment => /*m_activeSimulationType == SimulationType.TimeAdjustment &&*/m_activeCompressSettings != null;
}