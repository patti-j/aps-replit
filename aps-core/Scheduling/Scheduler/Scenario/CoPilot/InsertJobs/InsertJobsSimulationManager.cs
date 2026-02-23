using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.Localization;
using PT.PackageDefinitions.Settings;
using PT.Scheduler.CoPilot;
using PT.Scheduler.CoPilot.InsertJobs;
using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.Definitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages all RuleSeek simulations.  Only one scenario can be used to run simulations at once.
/// Raises an event when a new scenario can be created.
/// The simulation gets a list of all new jobs based on the options.
/// Phase 1 (ScheduleBeforeNeedDate)
/// One by one, jobs are picked and a list of available expedite times on or before the job's need date is created.
/// The release dates are also taken into account according to the options.
/// An expedite is performed for each time depending on if a KPI is specified, but an Expedite will only be chosen
/// if no additional jobs are late, including the job being expedited.
/// If a KPI is specified, each expedite time is checked to find the one with the best KPI score.
/// If no KPI is specified, the expedite time closest to the need date is chosen.
/// Any job that cannot be expedited successfuly on or before the need date is skipped until the next phase.
/// Phase 2 (ScheduleEvenIfLate)
/// One by one, jobs are picked and a list of available expedite times after the job's need date is created.
/// The job is expedited using the same KPI rules as phase 1.
/// In this phase a job can be scheduled if it is late, but no other jobs are late.
/// All jobs that can be scheduled will be scheduled during this phase. Any remaining jobs had errors during scheduling.
/// Much of the functionality of InsertJobs is contained in modules. These modules are added to Classes that handle the InsertJobs functionality.
/// In the starting function, these modules are added based on settings provided in InsertJobs. To customize InsertJobs functionality, it may be possible to
/// create a new Module for the required functionality and add it during the starting function.
/// </summary>
internal class InsertJobsSimulationManager
{
    private readonly ISystemLogger m_errorReporter;

    internal InsertJobsSimulationManager(IPackageManager a_packageManager, ISystemLogger a_errorReporter, CoPilotSettings a_coPilotSettings)
    {
        m_errorReporter = a_errorReporter;
        m_coPilotSettings = a_coPilotSettings;
        m_activeSimulations = new List<InsertJobSimulation>();
        m_simSettings = new InsertJobsSimSettings();
        m_packageManager = a_packageManager;
        m_simUtilities = new CoPilotSimulationUtilities(a_packageManager);
    }

    #region MEMBERS
    //List of running InsertJobs simulations.
    private readonly List<InsertJobSimulation> m_activeSimulations;

    //The current status of simulations.
    private CoPilotSimulationStatus m_status;

    public CoPilotSimulationStatus Status => m_status;

    private readonly InsertJobsSimSettings m_simSettings;

    internal enum SimulationPhase { ScheduleOnOrBeforeNeedDate, ScheduleEvenIfLate }

    private Scenario m_originalCopyScenario;
    private Scenario m_currentWorkingScenario;
    private readonly IPackageManager m_packageManager;
    private InsertJobsSettings m_insertJobsSettings;
    private readonly object m_simulationsLock = new ();
    private readonly object m_resultsListLock = new ();
    private readonly object m_performanceValuesLock = new ();

    private JobSequencer m_jobSequencer;

    //List<BaseId> m_jobIdsToExpedite = new List<BaseId>();
    private IExpediteTimesList m_expediteTimeList;
    private InsertJobsConstraintTimeCalculator m_earliestTimeCalculator;
    private readonly List<ScenarioDetailExpediteJobsT> m_expediteJobsTransmissions = new ();
    private int m_jobsOrignalToSchedule;
    private int m_jobsOnTimeCount;
    private int m_jobsInsertedSoFar;
    private int m_jobsExcluded;
    private InsertJobsDiagnostics.InsertJobsResultsList m_jobDiagnosticsResultList = new ();
    private readonly CoPilotSimulationUtilities m_simUtilities;
    private bool m_recalculateJitOriginalSetting;
    private BaseIdList m_insertSourceList;
    private bool m_scenarioChangedDuringIntialization;

    /// <summary>
    /// Copilot Settings. May be updated live if the SaveCopilotSettings is called.
    /// </summary>
    private CoPilotSettings m_coPilotSettings;

    public CoPilotSettings CoPilotSettings
    {
        set => m_coPilotSettings = value;
    }
    #endregion

    /// <summary>
    /// Diagnostic info retrieved by Scenariomanager
    /// The number of jobs to be inserted when the simulation was started
    /// </summary>
    internal int OriginalNumberOfJobsToInsert => m_jobsOrignalToSchedule;

    internal BaseId CurrentJobId
    {
        get
        {
            if (m_simSettings.JobSetToExpedite != null && m_simSettings.JobSetToExpedite.Count > 0)
            {
                return m_simSettings.JobSetToExpedite[0].Id;
            }

            return BaseId.NULL_ID;
        }
    }

    /// <summary>
    /// Diagnostic info retrieved by Scenariomanager
    /// Current number of jobs inserted.
    /// </summary>
    internal int NumberOfJobsInsertSoFar => m_jobsInsertedSoFar;

    /// <summary>
    /// Diagnostic info retrieved by Scenariomanager
    /// Progress of current Job
    /// </summary>
    internal decimal CurrentJobProgressPercent
    {
        get
        {
            int totalNeeded = m_expediteTimeList != null ? m_expediteTimeList.GetCount() : 0;
            if (totalNeeded > 0)
            {
                int expeditesSoFar = 0;
                lock (m_simulationsLock)
                {
                    for (int i = 0; i < m_activeSimulations.Count; i++)
                    {
                        expeditesSoFar += m_activeSimulations[i].CurrentExpeditesPerformed;
                    }
                }

                return expeditesSoFar / (decimal)totalNeeded * 100;
            }

            return 0;
        }
    }

    /// <summary>
    /// Diagnostic info retrieved by Scenariomanager
    /// Copies the current list of job ids of inserted jobs and whether it was late or not.
    /// </summary>
    /// <returns></returns>
    internal InsertJobsDiagnostics.InsertJobsResultsList BuildCurrentResultsList()
    {
        InsertJobsDiagnostics.InsertJobsResultsList newList = new ();

        List<Tuple<BaseId, bool, string>> currentResults;
        lock (m_resultsListLock)
        {
            currentResults = m_jobDiagnosticsResultList.GetValuesAsTupleList();
        }

        for (int i = 0; i < currentResults.Count; i++)
        {
            newList.AddJobResult(currentResults[i].Item1, currentResults[i].Item2, currentResults[i].Item3);
        }

        return newList;
    }

    /// <summary>
    /// Terminates all running simulations.
    /// It is possible that a scenario is being created a transmission will be generated after this method is called.
    /// </summary>
    private void Abort()
    {
        m_status = CoPilotSimulationStatus.STOPPED;
        lock (m_simulationsLock)
        {
            foreach (InsertJobSimulation sim in m_activeSimulations)
            {
                sim.SignalStop();
            }
        }
    }

    /// <summary>
    /// Just calls Abort(), then sets the status to CANCELED afterwards because Abort()
    /// sets the status to STOPPED
    /// </summary>
    internal void CancelInsertJobsSimulation()
    {
        Abort();
        m_status = CoPilotSimulationStatus.CANCELED;
    }

    /// <summary>
    /// Terminates all running simulations and sends the current working scenario.
    /// It is possible that a scenario is being created a transmission will be generated after this method is called.
    /// </summary>
    internal void StopAndSendScenario(BaseId a_originalInstigatorId)
    {
        Abort();
        SendCurrentScenario(a_originalInstigatorId);
    }

    /// <summary>
    /// Begin running simulations.
    /// This is the entry point to initiate all simulations.
    /// Throws an exception if the simulations cannot be started.
    /// </summary>
    /// <param name="a_sourceScenarioId"></param>
    /// <param name="a_scenario">Scenario to use</param>
    /// <param name="a_simType"></param>
    /// <param name="a_jobListToInsert"></param>
    /// <param name="a_packageManager"></param>
    /// <param name="a_originalInstigatorId">The Id of user who sent the original transmission</param>
    /// <param name="a_insertJobsSettings"></param>
    /// <returns></returns>
    internal void BeginInsertJobsSimulation(BaseId a_sourceScenarioId,
                                            Scenario a_scenario,
                                            InsertJobsSettings a_insertJobsSettings,
                                            InsertJobsSimulationTypes a_simType,
                                            BaseIdList a_jobListToInsert,
                                            IPackageManager a_packageManager,
                                            BaseId a_originalInstigatorId)
    {
        try
        {
            if (Status != CoPilotSimulationStatus.STOPPED && Status != CoPilotSimulationStatus.COMPLETED && Status != CoPilotSimulationStatus.CANCELED)
            {
                throw new PTValidationException("2868", new object[] { Status.ToString() });
            }

            m_status = CoPilotSimulationStatus.INITIALIZING;
            m_originalCopyScenario = a_scenario;
            m_insertJobsSettings = a_insertJobsSettings;
            m_jobsOnTimeCount = 0;
            m_jobsInsertedSoFar = 0;
            m_expediteJobsTransmissions.Clear();
            m_jobDiagnosticsResultList = new InsertJobsDiagnostics.InsertJobsResultsList();
            m_insertSourceList = a_jobListToInsert;
            m_simSettings.CurrentPhase = SimulationPhase.ScheduleOnOrBeforeNeedDate;
            m_simSettings.CheckAlternatePaths = a_insertJobsSettings.CheckAlternatePaths;
            m_simSettings.NoAnchorDrift = a_insertJobsSettings.NoAnchorDrift;
            m_simSettings.CheckKpiThreshold = false;
            lock (m_performanceValuesLock)
            {
                // TODO: Uncomment the code out once the logic has been fleshed out a bit more.
                // coPilotSettings.AverageCpuUsage is set in the instance manager
                // Just going to hard-code this to 5 for demo purposes. 
                //m_simSettings.MaxSimulations = (int)m_coPilotSettings.AverageCpuUsage;
                m_simSettings.MaxSimulations = 5;
            }

            m_simSettings.SimType = a_simType;
            m_simSettings.ConstrainByReleaseDate = a_insertJobsSettings.InsertAfterMaxReleaseDate;
            m_simSettings.KpiThreshold = (decimal)a_insertJobsSettings.KpiThresholdValue;

            if (m_simSettings.CheckAlternatePaths)
            {
                m_expediteTimeList = new ExpediteTimesAndPathsList();
            }
            else
            {
                m_expediteTimeList = new ExpediteTimesList();
            }

            if (a_insertJobsSettings.MinimumAttemptInterval.Ticks > 0)
            {
                m_expediteTimeList.AddModule(new ExpediteTimesListFilterSpacing(a_insertJobsSettings.MinimumAttemptInterval));
            }

            //Copy the scenario so it can be modified without affecting the original.
            lock (m_simulationsLock)
            {
                m_simUtilities.CopyAndStoreScenario(m_originalCopyScenario, out byte[] sdBytes, out byte[] ssBytes); //Create a new simulation scenario
                m_currentWorkingScenario = m_simUtilities.CopySimScenario(sdBytes, ssBytes);
            }

            //Remove all jobs not involved in the simulation to reduce scenario size.
            // Commenting this out for now because it's throwing a null reference error, but
            // we'd like to include this for performance purposes.
            //m_scenarioChangedDuringIntialization = m_simUtilities.RemoveUnusedJobsForSimulation(m_currentWorkingScenario, a_jobListToInsert);
            //if(!m_scenarioChangedDuringIntialization)
            //{
            //    //The scenario was not changed, so dispose the copy and use the original.
            //    //m_currentWorkingScenario.Dispose();
            //    //m_currentWorkingScenario = m_originalCopyScenario;
            //    m_originalCopyScenario.Dispose();
            //}

            m_simSettings.UseKpi = m_insertJobsSettings.KpiToRun != "    ";

            if (m_simSettings.UseKpi)
            {
                m_simSettings.CheckKpiThreshold = m_simSettings.KpiThreshold > 0;
                m_simSettings.KpiName = m_insertJobsSettings.KpiToRun;
                m_simSettings.KpiIsLowerBetter = false;

                //Find KPI
                bool kpiFound = false;
                foreach (KPI kpi in m_currentWorkingScenario.KpiController.KpiList.UnsortedKpiList)
                {
                    if (kpi.CalculatorName == m_insertJobsSettings.KpiToRun)
                    {
                        m_simSettings.KpiIsLowerBetter = kpi.Calculator.LowerIsBetter;
                        kpiFound = true;
                        break;
                    }
                }

                if (!kpiFound)
                {
                    //InsertJobs TODO: Make this visible to the user.
                    throw new PTValidationException("KPI Not Found");
                }
            }

            ScenarioDetail sd;
            using (m_currentWorkingScenario.ScenarioDetailLock.EnterWrite(out sd))
            {
                //Get Scenario Detail data
                m_recalculateJitOriginalSetting = sd.ScenarioOptions.RecalculateJITOnOptimizeEnabled;
                sd.ScenarioOptions.RecalculateJITOnOptimizeEnabled = false;
            }

            //Add simulation Modules.
            AddSimulationModules();

            //Calculate new jobs
            List<Tuple<BaseId, string>> excludedReasonsList = m_jobSequencer.CreateJobSet(m_currentWorkingScenario, m_insertSourceList);
            m_jobsOrignalToSchedule = m_jobSequencer.GetCount();
            m_jobsExcluded = excludedReasonsList.Count;

            //Add any excluded jobs to the diagnostics and set the failed to schedule reason.
            Scenario scenarioToUpdateOriginalJobs;
            if (m_scenarioChangedDuringIntialization)
            {
                scenarioToUpdateOriginalJobs = m_originalCopyScenario;
            }
            else
            {
                scenarioToUpdateOriginalJobs = m_currentWorkingScenario;
            }

            //Set initial schedule status
            lock (m_resultsListLock)
            {
                Job currentJob;
                using (scenarioToUpdateOriginalJobs.ScenarioDetailLock.EnterWrite(out sd))
                {
                    //Initially set all statuses to new. Jobs not scheduled at the end of the simulation will be failed to schedule.
                    foreach (BaseId jobId in a_jobListToInsert)
                    {
                        sd.JobManager.GetById(jobId).ScheduledStatus_set = JobDefs.scheduledStatuses.New;
                    }

                    //Set status for jobs that are exluded and the reason for exlusion.
                    foreach (Tuple<BaseId, string> reason in excludedReasonsList)
                    {
                        m_jobDiagnosticsResultList.AddJobResult(reason.Item1, false, reason.Item2);
                        currentJob = sd.JobManager.GetById(reason.Item1);
                        currentJob.ScheduledStatus_set = JobDefs.scheduledStatuses.FailedToSchedule;
                        currentJob.FailedToScheduleReason = reason.Item2;
                    }
                }
            }

            m_status = CoPilotSimulationStatus.RUNNING;
            InsertNextJob(a_originalInstigatorId);
        }
        catch (PTValidationException)
        {
            throw;
        }
        catch (Exception)
        {
            m_status = CoPilotSimulationStatus.ERROR;
            throw;
        }
    }

    /// <summary>
    /// Adds the simulation modules based on provided InsertJobs settings
    /// </summary>
    private void AddSimulationModules()
    {
        m_earliestTimeCalculator = new InsertJobsConstraintTimeCalculator(m_packageManager);
        m_earliestTimeCalculator.AddModule(new TimeCalculatorModulePhase1());
        m_earliestTimeCalculator.AddModule(new TimeCalculatorModuleFrozenSpan());
        if (m_simSettings.CheckAlternatePaths)
        {
            m_earliestTimeCalculator.AddModule(new TimeCalculatorModuleAlternatePaths());
        }
        else
        {
            m_earliestTimeCalculator.AddModule(new TimeCalculatorModule());
        }

        if (m_simSettings.ConstrainByReleaseDate)
        {
            m_earliestTimeCalculator.AddModule(new TimeCalculatorModuleReleaseDates());
        }

        m_jobSequencer = new JobSequencer(m_packageManager);
        m_jobSequencer.AddFilter(new JobSequencerModuleFilterRunning());
        m_jobSequencer.AddFilter(new JobSequencerModuleFilterFinished());
        if (m_simSettings.SimType == InsertJobsSimulationTypes.InsertGroup)
        {
            m_jobSequencer.SetGroupingLogic(new JobSequencerModuleGrouping());
        }
        else
        {
            m_jobSequencer.SetGroupingLogic(new JobSequencerModuleNoGrouping());
        }

        m_jobSequencer.SetPrioritySortingLogic(new JobSequencerSortingNeedDate());
        if (m_simSettings.CheckAlternatePaths)
        {
            //add alternate path filter
            m_jobSequencer.AddFilter(new JobSequencerModuleFilterPath());
        }
    }

    /// <summary>
    /// This is the starting point for inserting the next job. It is called after each job is scheduled or skipped.
    /// A list of job BaseIds are retrieved from the scenario's remaining jobs to be inserted.
    /// The BaseIds are used instead of references because the scenario is later copied to be used in multiple simultaneous simulations.
    /// Once the list is built, a list of expedite times is created for the first job.
    /// If there are no more BaseIds in the jobs list, either the next phase will be started, or the simulation will end
    /// depedning on the current phase.
    /// </summary>
    private void InsertNextJob(BaseId a_originalInstigatorId)
    {
        //If there are no more jobs to expedite, go to the next phase or complete the simulation
        if (m_jobSequencer.GetCount() == 0)
        {
            if (m_simSettings.CurrentPhase == SimulationPhase.ScheduleOnOrBeforeNeedDate)
            {
                //Phase 1 is complete. Rebuild remaining jobs list.
                TransisitonPhases();
                if (m_jobSequencer.GetCount() == 0)
                {
                    SimulationComplete(a_originalInstigatorId);
                    return;
                }
            }
            else
            {
                //All phases are complete and all jobs have been scheduled/skipped. End simulation.
                SimulationComplete(a_originalInstigatorId);
                return;
            }
        }

        //Build the list of expedite times for the next job. There will always be atleast 1 time.
        try
        {
            CreateListOfAvailableExpediteTimes(m_jobSequencer.GetNextExpediteSet(), m_earliestTimeCalculator);

            //There is atleast 1 job left to insert.
            m_simSettings.JobSetToExpedite = m_jobSequencer.GetNextExpediteSet();
            if (m_simSettings.UseKpi)
            {
                ScenarioDetail sd;
                using (m_currentWorkingScenario.ScenarioDetailLock.EnterRead(out sd))
                {
                    m_simSettings.CurrentKpi = m_currentWorkingScenario.KpiController.CalculateKPIByName(sd, m_simSettings.KpiName).Calculation;
                }
            }

            StartNextSimulation(a_originalInstigatorId);
        }
        catch (Exception e)
        {
            m_status = CoPilotSimulationStatus.ERROR;
            m_errorReporter.LogException(new PTHandleableException("2869", new object[] { e.Message }), null);
        }
    }

    /// <summary>
    /// Changes and configurations that will be different in Phase2.
    /// </summary>
    private void TransisitonPhases()
    {
        //Transision to Phase2
        m_simSettings.CurrentPhase = SimulationPhase.ScheduleEvenIfLate;
        m_earliestTimeCalculator.RemoveModuleByType(new TimeCalculatorModulePhase1());
        m_jobSequencer.AddFilter(new JobSequencerModuleFilterScheduledOnTime());
        lock (m_resultsListLock)
        {
            m_jobSequencer.CreateJobSet(m_currentWorkingScenario, m_insertSourceList);
        }
    }

    /// <summary>
    /// The simulation is finished and the result is reported to ScenaioManager. No more simulations are started after this point.
    /// </summary>
    private void SimulationComplete(BaseId a_originalInstigatorId)
    {
        SendCurrentScenario(a_originalInstigatorId);
        m_status = CoPilotSimulationStatus.COMPLETED;
        m_currentWorkingScenario.Dispose();
        if (m_scenarioChangedDuringIntialization)
        {
            m_originalCopyScenario.Dispose();
        }
    }

    /// <summary>
    /// Sends the current working scenario as a result to ScenarioManager.
    /// This can be called to send the current result without ending the simulation.
    /// </summary>
    private void SendCurrentScenario(BaseId a_originalInstigatorId)
    {
        //Transmissions.ScenarioCopyForInsertJobsT sendT = new Transmissions.ScenarioCopyForInsertJobsT(m_originalScenarioId, SchedulerDefinitions.ScenarioTypes.InsertJobs, m_expediteJobsTransmissions, m_onTimeJobsCount, m_orignalJobsToSchedule);

        Scenario newScenario;
        int jobsFailedToSchedule = 0;
        lock (m_simulationsLock)
        {
            //Since this if sending the scenario via event, it cannot lock the scenario. Copy it instead.
            byte[] sdByteArray;
            byte[] ssByteArray;
            if (m_scenarioChangedDuringIntialization)
            {
                m_simUtilities.CopyAndStoreScenario(m_originalCopyScenario, out sdByteArray, out ssByteArray); //Create a new simulation scenario
            }
            else
            {
                m_simUtilities.CopyAndStoreScenario(m_currentWorkingScenario, out sdByteArray, out ssByteArray); //Create a new simulation scenario
            }

            newScenario = m_simUtilities.CopySimScenario(sdByteArray, ssByteArray);
            ScenarioDetail sd;
            using (newScenario.ScenarioDetailLock.EnterWrite(out sd))
            {
                //Change the changed setting back to thier original values.
                sd.ScenarioOptions.RecalculateJITOnOptimizeEnabled = m_recalculateJitOriginalSetting;

                //Mark all jobs that did no schedule as Failed To Schedule
                foreach (BaseId currentId in m_insertSourceList)
                {
                    Job currentJob = sd.JobManager.GetById(currentId);
                    if (!currentJob.Scheduled && currentJob.ScheduledStatus != JobDefs.scheduledStatuses.FailedToSchedule)
                    {
                        currentJob.ScheduledStatus_set = JobDefs.scheduledStatuses.FailedToSchedule;
                        currentJob.FailedToScheduleReason = Localizer.GetString("Unable to insert into schedule");
                        jobsFailedToSchedule++;
                    }
                }
            }
        }

        string newName = "No new jobs";
        if (m_jobsOrignalToSchedule + m_jobsExcluded > 0)
        {
            newName = "";
            if (m_jobsOnTimeCount != 0)
            {
                newName += m_jobsOnTimeCount + "B ";
            }

            if (m_jobsOrignalToSchedule - m_jobsOnTimeCount - jobsFailedToSchedule > 0)
            {
                newName += m_jobsOrignalToSchedule - m_jobsOnTimeCount - jobsFailedToSchedule + "L ";
            }

            if (jobsFailedToSchedule > 0)
            {
                newName += jobsFailedToSchedule + "F ";
            }

            if (m_jobsExcluded > 0)
            {
                newName += m_jobsExcluded + "E ";
            }

            newName += m_jobsOrignalToSchedule + m_jobsExcluded + "T";
        }

        if (NewInsertJobsScenarioEvent != null)
        {
            NewInsertJobsScenarioEvent(this, new NewInsertJobsScenarioEventArgs(newScenario, newName, a_originalInstigatorId));
        }
    }

    #region EVENTS
    internal delegate void NewInsertJobsScenarioEventHandler(object sender, NewInsertJobsScenarioEventArgs a_e);

    internal event NewInsertJobsScenarioEventHandler NewInsertJobsScenarioEvent;

    /// <summary>
    /// This event args class holds the transmissions that are used by Scenario manager to create a new scenario.
    /// </summary>
    internal class NewInsertJobsScenarioEventArgs : EventArgs
    {
        internal NewInsertJobsScenarioEventArgs(Scenario a_scenarioToSend, string a_scenarioName, BaseId a_originalInstigatorId)
        {
            m_scenarioToSend = a_scenarioToSend;
            m_scenarioName = a_scenarioName;
            m_originalInstigatorId = a_originalInstigatorId;
        }

        private readonly Scenario m_scenarioToSend;

        internal Scenario NewInsertJobsScenario => m_scenarioToSend;

        private readonly string m_scenarioName;

        internal string NewScenarioName => m_scenarioName;

        /// <summary>
        /// Scenarios created from Insert Jobs will typically have the instigator as
        /// copilot however the user who requested the insert jobs access if being flagged as the OriginalInstigator here
        /// </summary>
        private readonly BaseId m_originalInstigatorId;
        /// <summary>
        /// Scenarios created from Insert Jobs will typically have the instigator as
        /// copilot however the user who requested the insert jobs access if being flagged as the OriginalInstigator here
        /// </summary>
        internal BaseId OriginalInstigatorId => m_originalInstigatorId;
    }
    #endregion

    /// <summary>
    /// Creates a list of Expedite times for the next job.
    /// Times are restricted by eligible resource and need date.
    /// There will always be at least one time (the clock)
    /// </summary>
    /// <param name="a_jobId"></param>
    private void CreateListOfAvailableExpediteTimes(List<JobToInsert> a_jobIdToExpedite, InsertJobsConstraintTimeCalculator a_earliestTimeCalculator)
    {
        long earliestExpediteTime;
        HashSet<InsertJobsTimeItem> hashList = new ();
        m_expediteTimeList.Clear();

        //Find end DateTime of all scheduled blocks
        ScenarioDetail newSd;
        using (m_currentWorkingScenario.ScenarioDetailLock.EnterRead(out newSd))
        {
            Job jobToExpedite = newSd.JobManager.GetById(a_jobIdToExpedite[0].Id);

            //Copy current scenario and expedite job to get the earliest time it can schedule.
            earliestExpediteTime = a_earliestTimeCalculator.CalculateMinScheduleDate(m_currentWorkingScenario, a_jobIdToExpedite);
            if (earliestExpediteTime == long.MaxValue)
            {
                //This job cannot be scheduled
                m_expediteTimeList.Add(new InsertJobsTimeItem(newSd.PlantManager.GetEarliestFrozenSpanEnd(newSd.ClockDate.Ticks)));
                return;
            }

            //Collect times after blocks on eligible resources
            long lastScheduledBlockEndTicks = 0;
            List<Department> deptList = newSd.PlantManager.GetDepartments();
            for (int deptI = 0; deptI < deptList.Count; deptI++)
            {
                Department d = deptList[deptI];
                for (int rI = 0; rI < d.ResourceCount; rI++)
                {
                    Resource r = d.Resources[rI];

                    if (r.Blocks.Count > 0)
                    {
                        if (m_simSettings.CurrentPhase == SimulationPhase.ScheduleEvenIfLate)
                        {
                            lastScheduledBlockEndTicks = Math.Max(lastScheduledBlockEndTicks, r.Blocks.Last.Data.EndTicks);
                        }

                        List<Tuple<int, int>> pathList = new ();
                        if (m_simSettings.CheckAlternatePaths)
                        {
                            pathList = DeterminePathEligibility(jobToExpedite, r);
                            if (pathList.Count == 0)
                            {
                                //All operations without predecessors cannot schedule on this resource.
                                continue;
                            }
                        }
                        else
                        {
                            if (!DetermineResourceEligibility(jobToExpedite, r))
                            {
                                //All operations without predecessors cannot schedule on this resource.
                                continue;
                            }
                        }

                        FilterEligibleTimesForResource(earliestExpediteTime, hashList, jobToExpedite, r, pathList);
                    }
                }
            }

            m_expediteTimeList.AddHashSet(hashList);

            //Add extra times after the last block on eligible resources and the last scheduled block.


            AddEarliestScheduleTime(jobToExpedite, earliestExpediteTime);

            //Now add times after all blocks but before Need Date if in phase 1
            //This will allow a job to schedule before it's need date but after any blocks.
            if (m_simSettings.CurrentPhase == SimulationPhase.ScheduleOnOrBeforeNeedDate)
            {
                m_expediteTimeList.AddTimesBeforeNeedDate(jobToExpedite, earliestExpediteTime);
            }
            else
            {
                m_expediteTimeList.AddTimesAfterLastBlock(jobToExpedite, lastScheduledBlockEndTicks);
            }

            m_expediteTimeList.FilterList();

            if (m_simSettings.CurrentPhase == SimulationPhase.ScheduleOnOrBeforeNeedDate)
            {
                m_expediteTimeList.SortByTimeAndPathDescending();
            }
            else
            {
                m_expediteTimeList.SortByTimeAndPathPreference();
            }
        }
    }

    /// <summary>
    /// Adds the earliest schedule time if shceduling in phase 1 or in phase 2 if there are no other times.
    /// This will result in at least 1 time for inserting.
    /// </summary>
    private void AddEarliestScheduleTime(Job a_jobToExpedite, long a_earliestTimeTicks)
    {
        //Even if the job will be late, add this time if there are not blocks so it can schedule.
        if (m_simSettings.CurrentPhase == SimulationPhase.ScheduleOnOrBeforeNeedDate)
        {
            m_expediteTimeList.Add(a_jobToExpedite, a_earliestTimeTicks);
        }
        else
        {
            m_expediteTimeList.Add(a_jobToExpedite, a_earliestTimeTicks);
            if (m_expediteTimeList.GetCount() == 0)
            {
                //There are no blocks after the need date to schedule after. Add earliest schedule time in case the job can be eary 
                //  (this is possible in this phase when inserting as group where some jobs are early and some are late)
                //Add needdate if it is later than earliest schedule date, which will be the shchedule date unless the group fits early.
                if (a_jobToExpedite.NeedDateTicks > a_earliestTimeTicks)
                {
                    m_expediteTimeList.Add(a_jobToExpedite, a_jobToExpedite.NeedDateTicks);
                }
            }
        }
    }

    private void FilterEligibleTimesForResource(long a_earliestExpediteTime, HashSet<InsertJobsTimeItem> a_hashList, Job a_jobToExpedite, Resource a_r, List<Tuple<int, int>> a_pathList)
    {
        if (m_simSettings.CurrentPhase == SimulationPhase.ScheduleOnOrBeforeNeedDate)
        {
            ResourceBlockList.Node currentNode = a_r.Blocks.First;
            while (currentNode != null)
            {
                if (currentNode.Data.EndTicks < a_jobToExpedite.NeedDateTime.Ticks)
                {
                    if (currentNode.Data.EndTicks >= a_earliestExpediteTime)
                    {
                        try
                        {
                            if (a_pathList.Count == 0)
                            {
                                a_hashList.Add(new InsertJobsTimeItem(currentNode.Data.EndTicks + 1)); //InsertJobs TODO: Check EndTicks and EndTicks + 1.
                            }
                            else
                            {
                                foreach (Tuple<int, int> path in a_pathList)
                                {
                                    a_hashList.Add(new InsertJobsTimeItem(currentNode.Data.EndTicks + 1, path.Item1, path.Item2)); //InsertJobs TODO: Check EndTicks and EndTicks + 1.
                                }
                            }
                        }
                        catch
                        {
                            //time has already been added.
                        }
                    }
                }
                else
                {
                    //The block time is after the needdate. No need to look at further blocks.
                    break;
                }

                currentNode = currentNode.Next;
            }
        }
        else
        {
            ResourceBlockList.Node currentNode = a_r.Blocks.Last;
            while (currentNode != null)
            {
                if (currentNode.Data.EndTicks >= a_jobToExpedite.NeedDateTime.Ticks)
                {
                    if (currentNode.Data.EndTicks >= a_earliestExpediteTime)
                    {
                        try
                        {
                            if (a_pathList.Count == 0)
                            {
                                a_hashList.Add(new InsertJobsTimeItem(currentNode.Data.EndTicks + 1)); //InsertJobs TODO: Check EndTicks and EndTicks + 1.
                            }
                            else
                            {
                                foreach (Tuple<int, int> path in a_pathList)
                                {
                                    a_hashList.Add(new InsertJobsTimeItem(currentNode.Data.EndTicks + TimeSpan.FromHours(1).Ticks, path.Item1, path.Item2)); //InsertJobs TODO: Check EndTicks and EndTicks + 1.
                                }
                            }
                        }
                        catch
                        {
                            //time has already been added. 
                        }
                    }
                }
                else
                {
                    //The block time is before the need date. No need to look at earlier blocks.
                    break;
                }

                currentNode = currentNode.Previous;
            }
        }
    }

    /// <summary>
    /// Determines if any of the job's unfinished operations have a primary resource requirement for a specific resource.
    /// </summary>
    /// <returns></returns>
    private bool DetermineResourceEligibility(Job a_jobToExpedite, Resource a_resource)
    {
        try
        {
            List<BaseOperation> opList = a_jobToExpedite.GetOperations();
            for (int i = 0; i < opList.Count; i++)
            {
                if (opList[i] is InternalOperation && opList[i].Predecessors != null && !opList[i].Finished)
                {
                    InternalOperation internalOp = (InternalOperation)opList[i];
                    List<Resource> resourceList = internalOp.ResourceRequirements.PrimaryResourceRequirement.EligibleResources.GetResources();
                    if (resourceList.Contains(a_resource))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a list of AlternatePath Ids that have an operation with a RR for the specified resource.
    /// </summary>
    /// <returns></returns>
    private List<Tuple<int, int>> DeterminePathEligibility(Job a_jobToExpedite, Resource a_resource)
    {
        List<Tuple<int, int>> pathIdList = new ();
        try
        {
            for (int moI = 0; moI < a_jobToExpedite.ManufacturingOrderCount; moI++)
            {
                for (int pathI = 0; pathI < a_jobToExpedite.ManufacturingOrders[moI].AlternatePaths.Count; pathI++)
                {
                    AlternatePath path = a_jobToExpedite.ManufacturingOrders[moI].AlternatePaths[pathI];
                    if (path.Preference > 0)
                    {
                        for (int i = 0; i < path.AlternateNodeSortedList.Count; i++)
                        {
                            InternalOperation op = (InternalOperation)path.GetNodeByIndex(i).Operation;
                            List<Resource> resourceList = op.ResourceRequirements.PrimaryResourceRequirement.EligibleResources.GetResources();
                            if (!op.Finished && resourceList.Contains(a_resource))
                            {
                                pathIdList.Add(new Tuple<int, int>(pathI, path.Preference));
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            //InsertJobs TODO
        }

        return pathIdList;
    }

    /// <summary>
    /// Creates multiple InsertJobs simulations in new tasks.
    /// The number of tasks depends on the number of available expedite times and the max simulations allowed.
    /// </summary>
    /// <param name="a_instigatorId"></param>
    private void StartNextSimulation(BaseId a_instigatorId)
    {
        if (m_status != CoPilotSimulationStatus.RUNNING)
        {
            return;
        }

        lock (m_simulationsLock)
        {
            //Create a copy of the working scenario for the simulations.
            m_simUtilities.CopyAndStoreScenario(m_currentWorkingScenario, out byte[] sdByteArray, out byte[] ssByteArray); //Create a new simulation scenario

            ScenarioDetail tempSd;
            using (m_currentWorkingScenario.ScenarioDetailLock.EnterRead(out tempSd))
            {
                m_simSettings.StartingTotalJobLateness = tempSd.JobManager.CalcTotalJobLateness();
                if (m_simSettings.NoAnchorDrift)
                {
                    m_simSettings.StartingTotalAnchorDrift = tempSd.JobManager.CalculateJobAnchorDrift();
                }
            }

            lock (m_performanceValuesLock)
            {
                int rangeSplitLength; //the number of expedite times to get from the original list.
                //Get the number of times to expedite per task. 
                //Don't create multiple tasks if there are less than 5 times otherwise the overhead of creating tasks will be too high.
                if (m_expediteTimeList.GetCount() > 5)
                {
                    rangeSplitLength = (int)Math.Ceiling(m_expediteTimeList.GetCount() / (float)m_simSettings.MaxSimulations);
                }
                else
                {
                    rangeSplitLength = m_expediteTimeList.GetCount();
                }

                int currentRangeStart = 0;
                //Create a new simulation with a smaller range of available expedite times.
                for (int i = 0; i < m_simSettings.MaxSimulations; i++)
                {
                    int nextRangeLength = Math.Min(rangeSplitLength, m_expediteTimeList.GetCount() - currentRangeStart);
                    if (nextRangeLength <= 0)
                    {
                        break;
                    }

                    IExpediteTimesList timeListSubset = m_expediteTimeList.GetSubsetList(currentRangeStart, nextRangeLength);

                    InsertJobSimulation newSim = new (sdByteArray, ssByteArray, a_instigatorId, m_simSettings, timeListSubset, m_packageManager);
                    newSim.InsertJobsSimulationProgressEvent += SimulationSetComplete;
                    m_activeSimulations.Add(newSim);

                    currentRangeStart += rangeSplitLength;
                }
            }

            for (int i = 0; i < m_activeSimulations.Count; i++)
            {
                m_activeSimulations[i].StartSimulation();
            }
        }
    }

    private ScenarioDetailExpediteJobsT m_bestSimExpediteResult;
    private decimal m_bestSimKpiResult = decimal.MaxValue;
    private long m_bestSimExpediteTime = long.MaxValue;
    private int m_bestSimExpeditePath = int.MaxValue;
    private int m_bestSimExpeditePreference = int.MaxValue;

    /// <summary>
    /// Event handler for when an InsertJobs simulation task is complete.
    /// If the job was expedited it will save the result if it has the best KPI score.
    /// </summary>
    /// <param name="a_args">RuleSeekEventArgs</param>
    private void SimulationSetComplete(object a_sender, InsertJobSimulation.InsertJobsSimulationProgressEventArgs a_args)
    {
        try
        {
            lock (m_simulationsLock)
            {
                //This simulation is completing and will terminate on its own. Remove the reference.
                m_activeSimulations.Remove(a_sender as InsertJobSimulation);

                if (a_args.Error != null)
                {
                    //Ended in error. Only perform this action for the first simulation that failed.
                    //  otherwise duplicate error messages will be added.
                    if ((a_sender as InsertJobSimulation).Status == CoPilotSimulationStatus.RUNNING)
                    {
                        foreach (InsertJobSimulation sim in m_activeSimulations)
                        {
                            sim.Status = CoPilotSimulationStatus.CANCELED;
                        }

                        m_jobDiagnosticsResultList.AddJobResult(m_jobSequencer.GetNextExpediteSet()[0].Id, m_simSettings.CurrentPhase == SimulationPhase.ScheduleEvenIfLate, a_args.Error.Message);
                    }
                }
                else if (!a_args.Success)
                {
                    //The job couldn't be scheduled in this phase
                }
                else if (a_args.PathPreference <= m_bestSimExpeditePreference)
                {
                    if (m_simSettings.CheckKpiThreshold && a_args.InKpiThreshold)
                    {
                        //A kpi that is good enough has been found. Stop trying the rest of the values.
                        foreach (InsertJobSimulation sim in m_activeSimulations)
                        {
                            sim.Status = CoPilotSimulationStatus.CANCELED;
                        }
                    }

                    //InsertJobs TODO: These if statements can be refactored.
                    if (!m_simSettings.UseKpi)
                    {
                        //Not using KPI so save the result based on Need Date time. Higher is better for SchedulBeforeNeedDate, lower is better for ScheduleEvenIfLate.
                        if (m_bestSimExpediteTime == long.MaxValue || a_args.PathPreference < m_bestSimExpeditePreference || (m_simSettings.CurrentPhase == SimulationPhase.ScheduleOnOrBeforeNeedDate && a_args.ExpediteT.Date > m_bestSimExpediteTime) || (m_simSettings.CurrentPhase == SimulationPhase.ScheduleEvenIfLate && a_args.ExpediteT.Date < m_bestSimExpediteTime))
                        {
                            m_bestSimExpediteResult = a_args.ExpediteT;
                            m_bestSimExpediteTime = a_args.ExpediteT.Date;
                            m_bestSimExpeditePath = a_args.PathIndex;
                            m_bestSimExpeditePreference = a_args.PathPreference;
                        }
                    }
                    else
                    {
                        //KPI is being used so only save the score with the best KPI value.  If the KPI values are the same, save better expedite time.
                        if (m_bestSimKpiResult == decimal.MaxValue || a_args.PathPreference < m_bestSimExpeditePreference || (m_simSettings.KpiIsLowerBetter && a_args.KpiScore < m_bestSimKpiResult) || (!m_simSettings.KpiIsLowerBetter && a_args.KpiScore > m_bestSimKpiResult) || (a_args.KpiScore == m_bestSimKpiResult && ((m_simSettings.CurrentPhase == SimulationPhase.ScheduleOnOrBeforeNeedDate && a_args.ExpediteT.Date > m_bestSimExpediteResult.Date) || (m_simSettings.CurrentPhase == SimulationPhase.ScheduleEvenIfLate && a_args.ExpediteT.Date < m_bestSimExpediteResult.Date))))
                        {
                            m_bestSimExpediteResult = a_args.ExpediteT;
                            m_bestSimKpiResult = a_args.KpiScore;
                            m_bestSimExpeditePath = a_args.PathIndex;
                            m_bestSimExpeditePreference = a_args.PathPreference;
                        }
                    }
                }

                //If there are no more active simulations then report the best result if there is one and InsertNextJob.
                if (m_activeSimulations.Count == 0)
                {
                    if (m_bestSimExpediteResult != null)
                    {
                        //Send the expedite to the working and original scenario if needed.
                        UpdateScenario(m_currentWorkingScenario);
                        if (m_scenarioChangedDuringIntialization)
                        {
                            UpdateScenario(m_originalCopyScenario);
                        }

                        if (m_simSettings.CurrentPhase == SimulationPhase.ScheduleOnOrBeforeNeedDate)
                        {
                            m_jobsOnTimeCount++;
                        }

                        m_jobsInsertedSoFar++;
                        lock (m_resultsListLock)
                        {
                            m_jobDiagnosticsResultList.AddJobResult(m_bestSimExpediteResult.Jobs.GetFirst(), m_simSettings.CurrentPhase == SimulationPhase.ScheduleEvenIfLate);
                        }

                        m_bestSimExpediteResult = null;
                        m_bestSimKpiResult = decimal.MaxValue;
                        m_bestSimExpediteTime = long.MaxValue;
                        m_bestSimExpeditePath = int.MaxValue;
                        m_bestSimExpeditePreference = int.MaxValue;
                    }
                    else if (m_simSettings.CurrentPhase == SimulationPhase.ScheduleEvenIfLate)
                    {
                        m_jobsInsertedSoFar++;
                        m_jobDiagnosticsResultList.AddJobResult(m_jobSequencer.GetNextExpediteSet()[0].Id, false, Localizer.GetString("Unable to insert into schedule"));
                    }

                    m_jobSequencer.RemoveCurrentSet();

                    if (Status == CoPilotSimulationStatus.RUNNING)
                    {
                        Task.Factory.StartNew(() => InsertNextJob(a_args.InstigatorId));
                    }
                }
            }
        }
        catch (Exception e)
        {
            m_status = CoPilotSimulationStatus.STOPPED;
            m_errorReporter.LogException(new PTHandleableException("2869", new object[] { e.Message }), null);
        }
    }

    private void UpdateScenario(Scenario a_scenario)
    {
        //Send the change to the current working scenario
        ScenarioDetail tempSd;
        //Job scheduledJob;
        using (a_scenario.ScenarioDetailLock.EnterWrite(out tempSd))
        {
            List<Tuple<BaseId, int, AlternatePathDefs.AutoUsePathEnum>> originalEnumValueList = new ();
            List<Job> affectedJobList = new ();
            //scheduledJob = tempSd.JobManager.Find(m_bestSimExpediteResult.Jobs.First.Data);
            if (m_simSettings.CheckAlternatePaths)
            {
                if (m_bestSimExpeditePath != -1)
                {
                    //Since this is a specific path, the path must be set as the default and all paths must be temporarily set to IfCurrent
                    for (int jobI = 0; jobI < m_simSettings.JobSetToExpedite.Count; jobI++)
                    {
                        Job currentJob = tempSd.JobManager.GetById(m_simSettings.JobSetToExpedite[jobI].Id);
                        affectedJobList.Add(currentJob);
                        for (int moI = 0; moI < currentJob.ManufacturingOrderCount; moI++)
                        {
                            AlternatePathsCollection paths = currentJob.ManufacturingOrders[moI].AlternatePaths;
                            for (int pathI = 0; pathI < paths.Count; pathI++)
                            {
                                if (paths[pathI].AutoUse != AlternatePathDefs.AutoUsePathEnum.IfCurrent)
                                {
                                    originalEnumValueList.Add(new Tuple<BaseId, int, AlternatePathDefs.AutoUsePathEnum>(currentJob.Id, pathI, paths[pathI].AutoUse));
                                    paths[pathI].AutoUse = AlternatePathDefs.AutoUsePathEnum.IfCurrent;
                                }

                                if (pathI == m_bestSimExpeditePath)
                                {
                                    currentJob.ManufacturingOrders[moI].CurrentPath_setter = paths[pathI];
                                }
                            }
                        }
                    }
                }
            }

            tempSd.ExpediteJobs(m_bestSimExpediteResult, new ScenarioDataChanges());
            ScenarioTouchT touchT = new ();
            tempSd.Touch(touchT); //A touch is needed to keep simulation data in sync between expedites. 
            foreach (Job job in affectedJobList)
            {
                job.DoNotSchedule = false;
            }

            //Reset path AutoUse if needed
            if (m_simSettings.CheckAlternatePaths)
            {
                foreach (Job job in affectedJobList)
                {
                    for (int moI = 0; moI < job.ManufacturingOrderCount; moI++)
                    {
                        AlternatePathsCollection paths = job.ManufacturingOrders[moI].AlternatePaths;
                        for (int pathI = 0; pathI < paths.Count; pathI++)
                        {
                            for (int i = 0; i < originalEnumValueList.Count; i++)
                            {
                                if (originalEnumValueList[i].Item2 == pathI)
                                {
                                    paths[pathI].AutoUse = originalEnumValueList[i].Item3;
                                    //originalEnumValueList.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}