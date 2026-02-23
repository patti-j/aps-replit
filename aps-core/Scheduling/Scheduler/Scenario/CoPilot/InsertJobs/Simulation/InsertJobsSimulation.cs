using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// This class performs the actual InsertJobs simulation. It is started by InsertJobsSimulationmanager.
/// A Copy of the provided scenario is stored.
/// The simulation will run through the list of available expedite times. The simulation will end if an expedite time is found or all times are tried.
/// If a KPI is used, the KPI will be calculated for every expedite time.  After all times are tried, the expedite (if any exist) at the best score  will be returned.
/// </summary>
internal class InsertJobSimulation
{
    internal InsertJobSimulation(byte[] a_sdByteArray, byte[] a_ssByteArray, BaseId a_instigatorId, InsertJobsSimSettings a_simSettings, IExpediteTimesList a_availableExpediteTimes, IPackageManager a_packageManager)
    {
        Status = CoPilotSimulationStatus.STOPPED;
        m_sdByteArray = a_sdByteArray;
        m_ssByteArray = a_ssByteArray;
        m_instigatorId = a_instigatorId;
        m_simSettings = a_simSettings;
        m_availableExpediteTimes = a_availableExpediteTimes;
        simUtilities = new CoPilotSimulationUtilities(a_packageManager);

        //Set up the simulation functionality based on provided sim settings.
        m_simCore = new SimulationCore();
        m_simCore.AddModule(new SimulationModuleImpactJobScheduled());
        if (a_simSettings.CurrentPhase == InsertJobsSimulationManager.SimulationPhase.ScheduleOnOrBeforeNeedDate)
        {
            m_simCore.AddModule(new SimulationModuleImpactNoLateness(m_simSettings.StartingTotalJobLateness));
        }
        else
        {
            m_simCore.AddModule(new SimulationModuleImpactNoOtherLateness(m_simSettings.StartingTotalJobLateness));
        }

        if (m_simSettings.CheckAlternatePaths)
        {
            m_simCore.AddModule(new SimulationModuleInsertPreProcessingPaths());
        }

        if (m_simSettings.NoAnchorDrift)
        {
            m_simCore.AddModule(new SimulationModuleAnchorDrift(m_simSettings.StartingTotalAnchorDrift));
        }
    }

    #region EVENTS
    /// <summary>
    /// Event to report scores back to SimulationManager
    /// </summary>
    protected internal delegate void InsertJobsSimulationProgressEventHandler(object sender, InsertJobsSimulationProgressEventArgs e);

    protected internal event InsertJobsSimulationProgressEventHandler InsertJobsSimulationProgressEvent;

    internal class InsertJobsSimulationProgressEventArgs : EventArgs
    {
        internal InsertJobsSimulationProgressEventArgs(ScenarioDetailExpediteJobsT a_expediteT, BaseId a_instigatorId, decimal a_kpiScore, bool a_success, int a_pathIdx, bool a_inKpiThreshold, int a_pathPreference)
        {
            m_expediteT = a_expediteT;
            m_kpiScore = a_kpiScore;
            m_success = a_success;
            m_pathIdx = a_pathIdx;
            m_inKpiThreshold = a_inKpiThreshold;
            m_pathPreference = a_pathPreference;
            m_instigatorId = a_instigatorId;
        }
        /// <summary>
        /// The Id of the user whole trigger the insert jobs action
        /// </summary>
        private readonly BaseId m_instigatorId;
        internal BaseId InstigatorId => m_instigatorId;
        /// <summary>
        /// The best expedite transmission. Will be null if one wasn't found.
        /// </summary>
        private readonly ScenarioDetailExpediteJobsT m_expediteT;

        internal ScenarioDetailExpediteJobsT ExpediteT => m_expediteT;

        /// <summary>
        /// Bool for wheter the simulation competed successfuly.
        /// </summary>
        private readonly bool m_success;

        internal bool Success => m_success;

        /// <summary>
        /// Bool for wheter the simulation ended because kpi was within threshold
        /// </summary>
        private readonly bool m_inKpiThreshold;

        internal bool InKpiThreshold => m_inKpiThreshold;

        /// <summary>
        /// The KPI score for the best expedite transmission if using KPI.
        /// </summary>
        private readonly decimal m_kpiScore;

        internal decimal KpiScore => m_kpiScore;

        /// <summary>
        /// The Alternate path that was set as the default. BaseId.NULL_Value if no path was specified.
        /// </summary>
        private readonly int m_pathIdx;

        internal int PathIndex => m_pathIdx;

        /// <summary>
        /// The Alternate path that was set as the default. BaseId.NULL_Value if no path was specified.
        /// </summary>
        private readonly int m_pathPreference;

        internal int PathPreference => m_pathPreference;

        /// <summary>
        /// This will be set if an exception has occurred.
        /// </summary>
        internal PTException Error;
    }

    /// <summary>
    /// A valid expedite was found.
    /// </summary>
    /// <param name="a_score"></param>
    protected void SimulationCompleted(ScenarioDetailExpediteJobsT a_expediteT, int a_pathIdx, bool a_inKpiThreshold, int a_pathPreference)
    {
        SendSimulationCompleteEvent(a_expediteT,true, a_pathIdx, a_inKpiThreshold, a_pathPreference);
    }

    /// <summary>
    /// Simluation was aborted and the result is not needed.
    /// </summary>
    protected void SimulationAborted()
    {
        SendSimulationCompleteEvent(null, false, -1, false, -1);
    }

    /// <summary>
    /// The Simulation exhausted all expedite times without finding a result.
    /// </summary>
    protected void SimulationCompletedWithoutResult()
    {
        SendSimulationCompleteEvent(null, false, -1, false, -1);
    }

    /// <summary>
    /// Sends the completion event back to InsertJobsManager.
    /// </summary>
    /// <param name="a_expediteT">ExpediteT if one was found</param>
    /// <param name="a_success">Bool for wheter the simulation found a result</param>
    protected void SendSimulationCompleteEvent(ScenarioDetailExpediteJobsT a_expediteT, bool a_success, int a_pathIdx, bool a_inKpiThreshold, int a_pathPreference)
    {
        if (InsertJobsSimulationProgressEvent != null)
        {
            InsertJobsSimulationProgressEventArgs args = new (a_expediteT, m_instigatorId,  m_bestKpiScore, a_success, a_pathIdx, a_inKpiThreshold, a_pathPreference);
            InsertJobsSimulationProgressEvent(this, args);
        }
        else
        {
            //Nothing is listening, just end the simulation.
            Status = CoPilotSimulationStatus.COMPLETED;
        }
    }
    #endregion

    #region MEMBERS
    protected internal CoPilotSimulationStatus Status;
    protected byte[] m_sdByteArray;
    protected byte[] m_ssByteArray;
    private readonly BaseId m_instigatorId;
    protected IExpediteTimesList m_availableExpediteTimes;
    protected CoPilotSimulationUtilities simUtilities;
    protected InsertJobsSimSettings m_simSettings;
    protected decimal m_bestKpiScore = decimal.MaxValue;

    internal enum SimulationCoreResults { UseCurrentExpedite, UseBestExpedite, Skip, Error }

    private readonly SimulationCore m_simCore;
    private int m_currentExpediteCount;

    public int CurrentExpeditesPerformed => m_currentExpediteCount;

    //Note: This could be turned into a list of Tuples so that it could later be sorted.
    //      doing so would allow values other than the KPI to determine the best expedite.
    protected ScenarioDetailExpediteJobsT m_bestSimulationExpediteResult;
    protected int m_bestSimulationExpeditePathIdx;
    protected int m_bestSimulationExpeditePathPreference;
    #endregion

    /// <summary>
    /// Signals the simulation to stop at the next opportunity.
    /// </summary>
    internal void SignalStop()
    {
        Status = CoPilotSimulationStatus.CANCELED;
    }

    /// <summary>
    /// Begins the simulation in a new Thread.
    /// </summary>
    internal void StartSimulation()
    {
        Task.Factory.StartNew(Simulate);
    }

    #region SIMULATION
    /// <summary>
    /// Bigins the simulation.
    /// This function will loop until a result is found, it is canceled by the SimulationManager, or if there is an error.
    /// </summary>
    protected void Simulate()
    {
        try
        {
            Status = CoPilotSimulationStatus.RUNNING;

            while (true) //Loop breaks when simulation is canceled or a result is found
            {
                if (!CheckStatusIsRunning(null))
                {
                    break;
                }

                //Copy scenario
                Scenario newScenario = simUtilities.CopySimScenario(m_sdByteArray, m_ssByteArray);

                ScenarioDetail newSd;
                newScenario.ScenarioDetailLock.EnterWrite(out newSd);

                if (!CheckStatusIsRunning(newScenario))
                {
                    break;
                }

                //Expedite at the next time
                m_simCore.InsertPreProcessing(newSd, m_simSettings.JobSetToExpedite, m_availableExpediteTimes);
                ScenarioDetailExpediteJobsT expediteT = simUtilities.CreateExpediteT(m_simSettings.JobSetToExpedite, m_availableExpediteTimes.GetCurrentItem().Ticks);

                List<Job> expediteJobList = new ();
                try
                {
                    m_currentExpediteCount++;
                    newSd.ExpediteJobs(expediteT, new ScenarioDataChanges());

                    if (!CheckStatusIsRunning(newScenario))
                    {
                        break;
                    }

                    for (int i = 0; i < m_simSettings.JobSetToExpedite.Count; i++)
                    {
                        expediteJobList.Add(newSd.JobManager.GetById(m_simSettings.JobSetToExpedite[i].Id));
                    }

                    if (m_simCore.AnalyzeImpact(expediteJobList, newSd))
                    {
                        if (CheckKPIAndSendResultIfNeeded(newSd, expediteT))
                        {
                            break;
                        }
                    }
                }
                catch (ScenarioDetail.ExpediteValidationException)
                {
                    //This path cannot be scheduled. Remove all expedite times for this path
                    RemoveCurrentPath(m_availableExpediteTimes.GetCurrentItem());
                }

                newScenario.Dispose();

                //Check for end conditions
                if (CheckEndConditions())
                {
                    break;
                }
            }

            //Simulation has ended. The task will terminate.
        }
        catch (Exception e)
        {
            if (InsertJobsSimulationProgressEvent != null)
            {
                InsertJobsSimulationProgressEventArgs args = new (null, m_instigatorId, m_bestKpiScore, false, -1, false, -1);
                args.Error = new PTException(e.Message, e.InnerException);
                InsertJobsSimulationProgressEvent(this, args);
            }
            else
            {
                //Nothing is listening, just end the simulation.
                Status = CoPilotSimulationStatus.COMPLETED;
            }
        }
    }

    private bool CheckKPIAndSendResultIfNeeded(ScenarioDetail a_sd, ScenarioDetailExpediteJobsT a_expediteT)
    {
        //The job was expedited and no additional jobs are late
        if (!m_simSettings.UseKpi)
        {
            //Without any other values to check, the value closest to the need date is the best expedite.
            SimulationCompleted(a_expediteT, m_availableExpediteTimes.GetCurrentItem().PathIndex, false, m_availableExpediteTimes.GetCurrentItem().PathPreference);
            return true;
        }

        //Store the expediteT and score if needed. If not within the KPI threshold, the rest of the times will be tried.
        decimal currentKpi = CalculateAndValidateNewScore(a_sd, a_expediteT, m_availableExpediteTimes.GetCurrentItem().PathIndex, m_availableExpediteTimes.GetCurrentItem().PathPreference);
        if (m_simSettings.CheckKpiThreshold)
        {
            if (CheckKpiThreshold(currentKpi))
            {
                SimulationCompleted(a_expediteT, m_availableExpediteTimes.GetCurrentItem().PathIndex, true, m_availableExpediteTimes.GetCurrentItem().PathPreference);
                return true;
            }
        }

        UpdateAvailableExpediteTimes();

        return false;
    }

    /// <summary>
    /// Returns whether the calculated KPI value is better than the original after the threshold adjustment.
    /// Example. Threshold of 5, original KPI is 50:
    /// The new KPI would have to be greater or equal to 45 or less than or equal to 55 if lower is better.
    /// </summary>
    /// <param name="a_kpi"></param>
    /// <returns></returns>
    protected bool CheckKpiThreshold(decimal a_kpi)
    {
        if (m_simSettings.KpiIsLowerBetter)
        {
            return a_kpi < m_simSettings.CurrentKpi + m_simSettings.KpiThreshold;
        }

        return a_kpi > m_simSettings.CurrentKpi - m_simSettings.KpiThreshold;
    }

    protected bool CheckEndConditions()
    {
        m_availableExpediteTimes.MoveNext();
        if (m_availableExpediteTimes.AtEndOfList())
        {
            if (!m_simSettings.UseKpi || m_bestSimulationExpediteResult == null)
            {
                //It is possible that this simulation's set of times doesn't contain one that can be successfuly expedited. Return failure.
                SimulationCompletedWithoutResult();
                return true;
            }

            //this job can be scheduled. Return the best result based on KPI scores
            SimulationCompleted(m_bestSimulationExpediteResult, m_bestSimulationExpeditePathIdx, false, m_bestSimulationExpeditePathPreference);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the KPI calculated value and stores the expedite if it is better than the current score.
    /// </summary>
    protected decimal CalculateAndValidateNewScore(ScenarioDetail a_newSd, ScenarioDetailExpediteJobsT a_expediteT, int a_pathIdx, int a_pathPreference)
    {
        decimal newScore = a_newSd.Scenario.KpiController.CalculateKPIByName(a_newSd, m_simSettings.KpiName).Calculation;
        if (ValidateNewScore(newScore))
        {
            m_bestSimulationExpediteResult = a_expediteT;
            m_bestSimulationExpeditePathIdx = a_pathIdx;
            m_bestSimulationExpeditePathPreference = a_pathPreference;
        }

        return newScore;
    }
    #endregion

    /// <summary>
    /// Returns a bool for whether a kpi is better than the current score.
    /// </summary>
    /// <param name="a_score"></param>
    /// <returns></returns>
    protected bool ValidateNewScore(decimal a_score)
    {
        if (m_bestKpiScore == decimal.MaxValue || (m_simSettings.KpiIsLowerBetter && a_score < m_bestKpiScore) || (!m_simSettings.KpiIsLowerBetter && a_score > m_bestKpiScore))
        {
            m_bestKpiScore = a_score;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check the status to see if the simulation has been canceled.
    /// </summary>
    /// <param name="a_scenario">Working Scenaro</param>
    /// <returns>Bool for whether the simulation should continue to run</returns>
    protected bool CheckStatusIsRunning(Scenario a_scenario)
    {
        if (Status != CoPilotSimulationStatus.RUNNING)
        {
            if (a_scenario != null)
            {
                a_scenario.Dispose();
            }

            SimulationAborted();
            return false;
        }

        return true;
    }

    protected void UpdateAvailableExpediteTimes()
    {
        if (m_simSettings.CheckAlternatePaths)
        {
            int currentPreference = m_availableExpediteTimes.GetCurrentItem().PathPreference;

            List<int> indexToDeleteList = new ();
            int loopCount = m_availableExpediteTimes.GetCount();
            for (int i = m_availableExpediteTimes.GetCurrentIndex() + 1; i < loopCount; i++)
            {
                if (m_availableExpediteTimes.GetByIndex(i).PathPreference > currentPreference)
                {
                    indexToDeleteList.Add(i);
                    m_availableExpediteTimes.MoveNext();
                }
            }

            for (int i = indexToDeleteList.Count - 1; i >= 0; i--)
            {
                m_availableExpediteTimes.RemoveAtIndex(indexToDeleteList[i]);
            }
        }
    }

    /// <summary>
    /// Remove all times for the path that is being used. Adjusts the list position accordingly.
    /// </summary>
    protected void RemoveCurrentPath(InsertJobsTimeItem a_currentItem)
    {
        int currentPath = m_availableExpediteTimes.GetCurrentItem().PathIndex;

        List<int> indexToDeleteList = new ();
        int loopCount = m_availableExpediteTimes.GetCount();
        for (int i = m_availableExpediteTimes.GetCurrentIndex() + 1; i < loopCount; i++)
        {
            if (m_availableExpediteTimes.GetByIndex(i).PathIndex == currentPath)
            {
                indexToDeleteList.Add(i);
                m_availableExpediteTimes.MoveNext();
            }
        }

        for (int i = indexToDeleteList.Count - 1; i >= 0; i--)
        {
            m_availableExpediteTimes.RemoveAtIndex(indexToDeleteList[i]);
        }
    }
}