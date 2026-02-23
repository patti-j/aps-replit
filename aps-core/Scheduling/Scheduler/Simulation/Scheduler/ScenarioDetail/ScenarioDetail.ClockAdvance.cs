using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

partial class ScenarioDetail
{
    #region Clock Advance
    /// <summary>
    /// This method doesn't call FireSimulateCompleteEvent(). It is up to the caller to
    /// invoke that method if appropriate.
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="scenarioT"></param>
    private void AdvanceClock(long a_startTime, ScenarioClockAdvanceT a_scenarioT, IScenarioDataChanges a_dataChanges)
    {
        SimulationType simType = SimulationType.ClockAdvance;

        // I need this in case the advance clock fails.
        long originalClockTime = Clock;

        try
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_scenarioT, SimulationProgress.Status.Initializing, m_nbrOfSimulationsSinceStartup);
            Common.Testing.Timing ts = CreateTiming("AdvanceClock");

            #if TEST
                SimDebugSetup();
            #endif

            try
            {
                DateErrorCheck(a_startTime);
            }
            catch (DateErrorCheckException)
            {
                throw new SimulationValidationException("2522");
            }

            //Alert Jobs of clock advance since AutoProgressReporting may be in use.
            TimeSpan clockAdvanceBy = new (a_startTime - ClockDate.Ticks);
            JobManager.AdvanceClock(clockAdvanceBy, new DateTime(a_startTime), a_scenarioT.autoFinishAllActivities, a_scenarioT.autoReportProgressOnAllActivities);
            PurgeCapacityIntervals(a_startTime, a_dataChanges);
            ScenarioOptions.AdvanceFiscalYearEndIfNecessary(a_startTime);

            m_purchaseToStockManager.AdvanceClock(a_startTime);
            if (a_scenarioT.adjustDemoData)
            {
                JobManager.AdjustDemoDataForClockAdvance(a_startTime, clockAdvanceBy.Ticks);
                PurchaseToStockManager.AdjustDemoDataForClockAdvance(clockAdvanceBy.Ticks);
            }

            OptimizeSettings.AdvancingClock(clockAdvanceBy);

            MainResourceSet availableResources;
            CreateActiveResourceList(out availableResources);
            m_clock = a_startTime;
            SimulationInitializationAll(availableResources, a_scenarioT, simType, null);

            ResourceActivitySets sequentialResourceActivities = new (availableResources);
            UnscheduleInProcessActivitiesAndAddInProcessReleaseEvents(true);
            UnscheduleActivities(availableResources, new SimulationTimePoint(originalClockTime), new SimulationTimePoint(EndOfPlanningHorizon), sequentialResourceActivities, simType, a_startTime);
            SimulationResourceInitialization(new SimulationResourceDispatcherUsageArgs(Clock, OptimizeSettings.dispatcherSources.NormalRules));
            Simulate(a_startTime, sequentialResourceActivities, simType, a_scenarioT);

            StopTiming(ts, false);

            #if TEST
                TestSchedule("Clock Advancement");
            #endif
            SimulationActionComplete();
            m_simulationProgress.PostSimulationWorkComplete();
            CheckForRequiredAdditionalSimulation(a_dataChanges);
        }
        catch (SimulationValidationException e)
        {
            m_clock = originalClockTime;
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_scenarioT, SimulationProgress.Status.Terminated, m_nbrOfSimulationsSinceStartup);
            FireSimulationValidationFailureEvent(e, a_scenarioT);
            throw;
        }
        catch (Exception)
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_scenarioT, SimulationProgress.Status.Exception, m_nbrOfSimulationsSinceStartup);
            throw;
        }
    }
    #endregion Clock Advance
}