using PT.Common.Debugging;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Reports progress during simulation. This doesn't include any time prior to starting the simulation, such as the time it takes to
/// initialize the simulation.
/// </summary>
public class SimulationProgress
{
    /// <summary>
    /// One of these stauses are sent with each Progress event.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The simulation hasn't started yet. The scenario data is being initialized for a simulation.
        /// </summary>
        Initializing,

        /// <summary>
        /// This is the state at the start of the simulation.
        /// </summary>
        Started,

        /// <summary>
        /// The state of progress while activities are being scheduled.
        /// </summary>
        Scheduling,

        /// <summary>
        /// The scheduling of activities has completed.
        /// </summary>
        Complete,

        /// <summary>
        /// After simulation, some clean up work or other code may be run. After all post simulation work is done, this event is fired.
        /// </summary>
        PostSimulationWorkComplete,

        /// <summary>
        /// The simulation never started and was terminated as a result of parameters not validating.
        /// </summary>
        Terminated,

        /// <summary>
        /// An unhandled exception occurred either while initializing or simulating.
        /// </summary>
        Exception,
        Clear,
        MRP_LowLevelCodes,
        MRP_StartInitialOptimize,
        MRP_GeneratingJobs,
        MRP_GeneratingPurchaseOrders,
        MRP_GeneratingSafetyStock,
        MRP_GenerateJobsFromSafetyStock,

        /// <summary>
        /// If OptimizeSettings.MrpUseJitDates is on, Mrp runs an additional simulation to set need dates once material constraint are turned back on.
        /// </summary>
        MRP_StartOptimizeToSetNeedDates,
        MRP_StartFinalOptimize,
        MRP_OptimizeForLevel,
        MRP_OptimizeSafetyStock,
        MRP_Complete,
        MRP_JobCleanUpStart,
        MRP_JobCleanUpDelete,
        MRP_ItemAdjustmentStart,
        MRP_JobNotes,
        MRP_SplittingJobs,
        MRP_Start,
        MRP_OptimizePurchaseOrders,
        MRP_FinishedWithException,
        MRP_DeletePurchaseStocks
    }

    /// <summary>
    /// Create simulation progress.
    /// </summary>
    /// <param name="a_scenario">Required to access the Scenario's ScenarioEvents member.</param>
    /// <param name="a_totalActivitiesToSchedule">The number of activities to schedule.</param>
    /// <param name="a_reportingPercentFrequency">
    /// Report progress after this percent of activities have been scheduled. The value must be greater than 0 and less than 1. Though there are internal limits that
    /// might make the actual value a little larger or smaller.
    /// </param>
    internal SimulationProgress(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, long a_totalActivitiesToSchedule, long a_simNbr, int a_reportingPercentFrequency, bool a_supressEvents)
    {
        #if !DEBUG
            try
            {
        #endif
        m_sd = a_sd;
        m_simType = a_simType;
        m_t = a_t;
        SimulationNumber = a_simNbr;

        m_reportingFrequencyPercent = a_reportingPercentFrequency;
        SetTotalActivitiesToSchedule(a_totalActivitiesToSchedule);
        StartDateTime = DateTime.Now;
        FireSimulationProgressEvent(Status.Started);
        #if !DEBUG
            }
            catch (Exception e)
            {
                // Ignore exceptions in release mode since the end result is not that important
            }
        #endif
    }

    /// <summary>
    /// Used to gain access to ScenarioEvents and is passed as part of the progress event.
    /// </summary>
    private readonly ScenarioDetail m_sd;

    /// <summary>
    /// The type of simulation.
    /// </summary>
    private readonly ScenarioDetail.SimulationType m_simType;

    private Status m_simStatus;

    /// <summary>
    /// The transmission that cause the simulation to occur.
    /// </summary>
    private ScenarioBaseT m_t;

    /// <summary>
    /// The total number of activities that have been scheduled.
    /// </summary>
    private long m_totalActivitiesScheduled;

    /// <summary>
    /// The total number of activities that are to be scheduled.
    /// </summary>
    private long m_totalActivitiesToSchedule;

    /// <summary>
    /// Report progress after this many activities have been scheduled.
    /// </summary>
    private long m_reportAfterThisManyActivitiesAreScheduled;

    /// <summary>
    /// Report progress after this percent of activities have been scheduled. The value must be greater than 0 and less than 1. Though there are internal limits that might make the actual value a little
    /// larger or smaller.
    /// </summary>
    private readonly int m_reportingFrequencyPercent;

    /// <summary>
    /// Report progress when the number of activities scheduled equals this number.
    /// </summary>
    private decimal m_nextNbrOfActivitiesScheduledThreshold;

    private DateTime m_startTime = DateTime.MinValue;

    internal DateTime StartDateTime
    {
        get => m_startTime;
        private set => m_startTime = value;
    }

    private DateTime m_completeTime = DateTime.MinValue;

    internal DateTime CompleteDateTime
    {
        get => m_completeTime;
        set => m_completeTime = value;
    }

    private DateTime m_postSimulationWorkCompleteDateTime = DateTime.MinValue;

    internal DateTime PostSimulationWorkCompleteDateTime
    {
        get => m_postSimulationWorkCompleteDateTime;
        set => m_postSimulationWorkCompleteDateTime = value;
    }

    /// <summary>
    /// Call this function when the total number of activities to schedule is changed.
    /// </summary>
    /// <param name="a_totalActivitiesToSchedule"></param>
    /// <param name="a_reportingPercentFrequency"></param>
    private void SetTotalActivitiesToSchedule(long a_totalActivitiesToSchedule)
    {
        if (m_totalActivitiesToSchedule != a_totalActivitiesToSchedule)
        {
            m_totalActivitiesToSchedule = a_totalActivitiesToSchedule;
            m_reportAfterThisManyActivitiesAreScheduled = (long)Math.Ceiling(m_totalActivitiesToSchedule / (decimal)(100 * Math.Pow(10.0, m_reportingFrequencyPercent)));
            m_nextNbrOfActivitiesScheduledThreshold = m_reportAfterThisManyActivitiesAreScheduled;

            if (m_totalActivitiesScheduled > 0)
            {
                m_simStatus = Status.Scheduling;
                FireSimulationProgressEvent(m_simStatus);
                IncrementNextNbrOfActivitiesScheduledThreshold();
            }
        }
    }

    /// <summary>
    /// This function must be called each time an activity is called.
    /// </summary>
    internal void ActivityScheduled()
    {
        #if !DEBUG
            try
            {
        #endif
        ++m_totalActivitiesScheduled;

        bool reportProgress = m_totalActivitiesScheduled >= m_nextNbrOfActivitiesScheduledThreshold;
        #if TEST
            // Always report progress in test
            reportProgress = true;
        #endif

        if (reportProgress)
        {
            IncrementNextNbrOfActivitiesScheduledThreshold();
            m_simStatus = Status.Scheduling;
            FireSimulationProgressEvent(m_simStatus);
        }
        #if !DEBUG
            }
            catch (Exception e)
            {
                // ignore exceptions in reelase mode since the end result isn't important.
            }
        #endif
    }

    internal void AutoSplitActivity()
    {
        m_totalActivitiesToSchedule++;
    }

    internal void AutoJoinedActivity()
    {
        m_totalActivitiesToSchedule--;
    }

    internal long SimulationNumber { get; private set; }

    /// <summary>
    /// Determine the next point, the number of scheduled activities, when the progress event will be fired.
    /// </summary>
    private void IncrementNextNbrOfActivitiesScheduledThreshold()
    {
        m_nextNbrOfActivitiesScheduledThreshold = m_totalActivitiesScheduled + m_reportAfterThisManyActivitiesAreScheduled;
        if (m_nextNbrOfActivitiesScheduledThreshold > m_totalActivitiesToSchedule)
        {
            m_nextNbrOfActivitiesScheduledThreshold = m_totalActivitiesToSchedule;
        }
    }

    /// <summary>
    /// The simulation must call this function once the simulation has completed.
    /// The final Fire of the progress event is sent when this is called.
    /// </summary>
    internal void SchedulingComplete()
    {
        #if !DEBUG
            try
            {
        #endif
        CompleteDateTime = DateTime.Now;
        m_totalActivitiesScheduled = m_totalActivitiesToSchedule;
        m_simStatus = Status.Complete;
        FireSimulationProgressEvent(Status.Complete);
        #if !DEBUG
            }
            catch (Exception e)
            {
                // ignore exceptions in reelase mode since the end result isn't important.
            }
        #endif
    }

    /// <summary>
    /// Fired after all work after the simulation is complete.
    /// </summary>
    internal void PostSimulationWorkComplete()
    {
        PostSimulationWorkCompleteDateTime = DateTime.Now;
        m_simStatus = Status.PostSimulationWorkComplete;
        FireSimulationProgressEvent(Status.PostSimulationWorkComplete);
        m_t = null;
    }

    /// <summary>
    /// Adjust the number of activities to schedule by a positive or negative amount.
    /// </summary>
    /// <param name="a_nbrOfActivitiesChange">A positive or negative number.</param>
    internal void AdjustNbrOfActivitiesToSchedule(long a_nbrOfActivitiesChange)
    {
        long totalActivitiesToSchedule = m_totalActivitiesToSchedule + a_nbrOfActivitiesChange;
        SetTotalActivitiesToSchedule(totalActivitiesToSchedule);
    }

    /// <summary>
    /// This is called whenever the number of activities scheduled exceeds the update frequency percent.
    /// </summary>
    /// <param name="a_status"></param>
    private void FireSimulationProgressEvent(Status a_status)
    {
        ScenarioEvents se;

        using (m_sd.Scenario.AutoEnterScenarioEvents(out se))
        {
            if (se.HasSimulationProgressListeners)
            {
                decimal percentComplete;
                if (m_totalActivitiesToSchedule != 0)
                {
                    percentComplete = m_totalActivitiesScheduled / (decimal)m_totalActivitiesToSchedule;
                }
                else
                {
                    percentComplete = 1m;
                }

                if (percentComplete > 1m)
                {
                    DebugException.ThrowInTest("Simulation scheduled more activities than expected");
                    return; //Don't fire the event, it shows an obvious issue to the UI and causes many events to fire.
                }

                se.FireSimulationProgressEvent(m_sd, m_simType, m_t, percentComplete, a_status, SimulationNumber);
            }
        }
    }

    /// <summary>
    /// Used to send progress messages prior to the creating of an object of this type.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_status"></param>
    internal static void FireSimulationProgressEvent(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, Status a_status, long a_simNbr)
    {
        ScenarioEvents se;
        using (a_sd.Scenario.AutoEnterScenarioEvents(out se))
        {
            if (se.HasSimulationProgressListeners)
            {
                se.FireSimulationProgressEvent(a_sd, a_simType, a_t, 0, a_status, a_simNbr);
            }
        }
    }

    public override string ToString()
    {
        return string.Format("{0}: {1}/{2}", m_simStatus, m_totalActivitiesScheduled, m_totalActivitiesToSchedule);
    }
}