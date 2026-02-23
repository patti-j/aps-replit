using PT.APSCommon;
using PT.Common.Compression;
using PT.Common.Exceptions;
using PT.PackageDefinitions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitions.Settings;
using PT.Scheduler.CoPilot.RuleSeek;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.Definitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

//Contains The members and functions in ScenarioManager relating to CoPilot.
//There still may be some references to CoPilot features in the main ScenarioManager file
//  and in the Receive method
public partial class ScenarioManager
{
    #region MEMBERS
    private CoPilotSettings m_copilotSettings;
    private InsertJobsDiagnostics m_insertJobsDiagnostics;

    /// <summary>
    /// The CoPilot diagnostic values as last calculated. Get is used by the client.
    /// The server calculates these values with CalcCoPilotDiagnostics()
    /// </summary>
    public InsertJobsDiagnostics InsertJobsDiagnostics => m_insertJobsDiagnostics;

    private RuleSeekDiagnositcs m_ruleSeekDiagnostics;

    /// <summary>
    /// The CoPilot diagnostic values as last calculated. Get is used by the client.
    /// The server calculates these values with CalcCoPilotDiagnostics()
    /// </summary>
    public RuleSeekDiagnositcs RuleSeekDiagnostics => m_ruleSeekDiagnostics;

    private RuleSeekSettings m_ruleSeekSettings;

    /// <summary>
    /// Settings related to RuleSeek Simulations. These are updated by sending a CoPilotSettingsChangeT
    /// </summary>
    public RuleSeekSettings RuleSeekSettings => m_ruleSeekSettings;

    private InsertJobsSettings m_insertJobsSettings;

    /// <summary>
    /// Settings related to InsertJobs Simulations. These are updated by sending a CoPilotSettingsChangeT
    /// </summary>
    public InsertJobsSettings InsertJobsSettings => m_insertJobsSettings;

    //This timer is used to initiate a RuleSeekStartT. 
    private System.Timers.Timer m_ruleSeekTimer;

    private RuleSeekSimulationManager m_ruleSeekManager;
    private InsertJobsSimulationManager m_insertJobsManager;
    private PruneScenarioManager m_pruneScenarioManager;
    #endregion

    /// <summary>
    /// Builds the CoPilot Diagnostics with current data from InsertJobsManager and RuleSeekmanager
    /// </summary>
    /// <returns></returns>
    internal (RuleSeekDiagnositcs, InsertJobsDiagnostics)? CalcCoPilotDiagnosticsForUpdate()
    {
        InsertJobsDiagnostics insertJobsDiagnostics = new ();
        RuleSeekDiagnositcs ruleSeekDiagnostics = new ();

        try
        {
            if (m_ruleSeekManager != null)
            {
                ruleSeekDiagnostics = m_ruleSeekManager.CalcRuleSeekDiagnostics();
            }
            else
            {
                ruleSeekDiagnostics.RuleSeekStatus = CoPilotSimulationStatus.STOPPED;
            }

            if (m_insertJobsManager != null)
            {
                insertJobsDiagnostics.InsertJobsStatus = m_insertJobsManager.Status;
                insertJobsDiagnostics.CurrentInsertedJobsCount = m_insertJobsManager.NumberOfJobsInsertSoFar;
                insertJobsDiagnostics.TotalJobsToInsert = m_insertJobsManager.OriginalNumberOfJobsToInsert;
                insertJobsDiagnostics.InsertJobsResultList = m_insertJobsManager.BuildCurrentResultsList();
                insertJobsDiagnostics.CurrentWorkingJob = m_insertJobsManager.CurrentJobId;
                insertJobsDiagnostics.CurrentJobProgress = (double)m_insertJobsManager.CurrentJobProgressPercent;
            }
            else
            {
                insertJobsDiagnostics.InsertJobsStatus = CoPilotSimulationStatus.STOPPED;
            }
        }
        catch
        {
            return null;
        }

        return (ruleSeekDiagnostics, insertJobsDiagnostics);
    }

    /// <summary>
    /// Broadcasts a new RuleSeekStatusUpdateT
    /// </summary>
    /// <param name="a_status">Current Status</param>
    /// <param name="a_errorStatus">Error Status</param>
    private void SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues a_status, CoPilotStatusUpdateT.CoPilotErrorValues a_errorStatus)
    {
        CoPilotStatusUpdateT updateT = new (a_status, a_errorStatus);
        SystemController.ClientSession.SendClientAction(updateT);
    }

    /// <summary>
    /// Starts or stops the timer depending on the current RuleSeekSettings.  Also resets the timer if RuleSeek is enabled
    /// Sends an event for the UI if the simulation is stopped.
    /// </summary>
    private void UpdateRuleSeekTimer(RuleSeekEndReasons a_endReason)
    {
        if (PTSystem.Server)
        {
            if (m_ruleSeekTimer == null)
            {
                m_ruleSeekTimer = new System.Timers.Timer();
                m_ruleSeekTimer.Elapsed += new System.Timers.ElapsedEventHandler(RuleSeekTimerElapsed);
            }

            m_ruleSeekTimer.Stop();
            if (m_ruleSeekManager != null && m_ruleSeekManager.Status == CoPilotSimulationStatus.RUNNING)
            {
                m_ruleSeekManager.Abort();
                //Simulation was stopped
                RuleSeekCompletionT completionT = new (a_endReason);
                SystemController.ClientSession.SendClientAction(completionT);
            }

            if (a_endReason != RuleSeekEndReasons.Startup)
            {
                DeleteAllScenariosOfType(ScenarioTypes.RuleSeek);
            }

            if (CheckRuleSeekScenarioExists() && m_copilotSettings.Enabled && m_ruleSeekSettings.Enabled)
            {
                m_ruleSeekTimer.Interval = m_ruleSeekSettings.IdleTimeDuration.TotalMilliseconds;
                m_ruleSeekTimer.Start();
            }
        }
    }

    /// <summary>
    /// Gets the ScenarioId RuleSeek will run on, or a default if not yet set.
    /// </summary>
    /// <returns></returns>
    public BaseId GetRuleSeekSourceScenarioId()
    {
        if (m_ruleSeekSettings.SourceScenarioId == 0)
        {
            return GetFirstProductionScenario()?.Id ?? BaseId.NULL_ID;
        }

        return m_ruleSeekSettings.SourceScenarioId;
    }

    /// <summary>
    /// Confirms if Scenario which RuleSeek is set to run on still exists, and sends a notification otherwise.
    /// </summary>
    /// <returns></returns>
    private bool CheckRuleSeekScenarioExists()
    {
        bool exists = true;

        Scenario ruleSeekScenario = Find(m_ruleSeekSettings.SourceScenarioId);

        if (ruleSeekScenario == null)
        {
            // Scenario removed; clear Id ref and notify client the need for refreshing.
            m_ruleSeekSettings.SourceScenarioId = BaseId.NULL_ID;
            m_ruleSeekSettings.Enabled = false;
            FireCopilotSettingsInvalidatedEvent();
            exists = false;
        }

        return exists;
    }

    /// <summary>
    /// Excludes specific ScenarioIdBase transmissions from stopping the RuleSeek simulations.
    /// </summary>
    /// <param name="a_t">Received transmission</param>
    private void ValidateTransmissionForRuleSeekTimerUpdate(ScenarioIdBaseT a_t)
    {
        if (a_t.scenarioId == m_ruleSeekSettings.SourceScenarioId)
        {
            UpdateRuleSeekTimer(RuleSeekEndReasons.ScenarioAction);
        }
    }

    /// <summary>
    /// RuleSeek timer has elapsed. Send a RuleSeekStart transmission
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void RuleSeekTimerElapsed(object source, System.Timers.ElapsedEventArgs e)
    {
        if (m_insertJobsManager != null && m_insertJobsManager.Status == CoPilotSimulationStatus.RUNNING)
        {
            //Don't start RuleSeek if InsertJobs is running.
            return;
        }

        if (CheckRuleSeekScenarioExists())
        {
            ScenarioRuleSeekStartT start = new (GetRuleSeekSourceScenarioId());
            SystemController.ClientSession.SendClientAction(start);
        }

        m_ruleSeekTimer.Stop();
    }

    /// <summary>
    /// Deletes any older RuleSeek Scenarios that are no longer needed. Sends ScenarioDeleteTs and a ScenarioCopyForRuleSeekT.
    /// This is called by an event in RuleSeekSimulationManager when a new RuleSeek Scenario is to be created.
    /// </summary>
    private void CreateNewRuleSeekScenario(object sender, RuleSeekSimulationManager.NewRuleSeekScenarioEventArgs a_args)
    {
        lock (m_deleteOldScenariosLock) //there could potentially be multiple Scenarios created at a time.
        {
            ScenarioDeleteT deleteT = null;

            if (m_ruleSeekSettings.MaxScenariosToKeep == 1)
            {
                DeleteAllScenariosOfType(ScenarioTypes.RuleSeek);
            }
            else
            {
                //Delete specific RuleSeek Scenarios
                Scenario s;
                ScenarioSummary ss;
                List<Tuple<BaseId, decimal>> sortingList = new ();

                for (int i = 0; i < LoadedScenarioCount; i++)
                {
                    s = GetByIndex(i);
                    using (s.ScenarioSummaryLock.EnterRead(out ss))
                    {
                        if (ss.Type == ScenarioTypes.RuleSeek)
                        {
                            sortingList.Add(new Tuple<BaseId, decimal>(s.Id, ss.RuleSeekScore));
                        }
                    }
                }

                if (sortingList.Count > 1 && sortingList.Count >= m_ruleSeekSettings.MaxScenariosToKeep)
                {
                    //build the deleteList
                    if (a_args.NewRuleSeekT.IsLowerBetter)
                    {
                        sortingList.Sort((t1, t2) => t1.Item2.CompareTo(t2.Item2));
                    }
                    else
                    {
                        sortingList.Sort((t1, t2) => t2.Item2.CompareTo(t1.Item2));
                    }

                    deleteT = new ScenarioDeleteT(sortingList[sortingList.Count - 1].Item1);
                }
            }

            if (deleteT != null)
            {
                PacketT packet = new ();
                packet.AddT(deleteT);
                SystemController.ClientSession.SendClientActionsPacket(deleteT, a_args.NewRuleSeekT);
            }

            SystemController.ClientSession.SendClientAction(a_args.NewRuleSeekT);

            //Send the new scenario
        }
    }

    /// <summary>
    /// Deletes any older InsertJobs Scenarios that are no longer needed. Sends ScenarioDeleteTs and a ScenarioCopyForInsertJobsT.
    /// This is called by an event in InsertJobsSimulationManager when a new InsertJobs Scenario is to be created.
    /// </summary>
    private void CreateNewInsertJobsScenario(object sender, InsertJobsSimulationManager.NewInsertJobsScenarioEventArgs a_args)
    {
        lock (m_deleteOldScenariosLock) //there could potentially be multiple Scenarios created at a time.
        {
            //InsertJobst TODO
            if (true)
            {
                DeleteAllScenariosOfType(ScenarioTypes.InsertJobs);
            }

            //BaseId newScenarioId = GetNextScenarioId();

            ScenarioSummary ss;
            using (a_args.NewInsertJobsScenario.ScenarioSummaryLock.EnterWrite(out ss))
            {
                ss.Name = a_args.NewScenarioName;
                ss.SetType(ScenarioTypes.InsertJobs);
                ScenarioPlanningSettings planningSettings = ss.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings());
                planningSettings.Production = false;
                // Force all inserted scenarios to not be production
                ss.ScenarioSettings.SaveSetting(planningSettings);
                //ss.Id = newScenarioId;
            }

            Scenario.UndoSets us;
            using (a_args.NewInsertJobsScenario.UndoSetsLock.EnterWrite(out us))
            {
                bool notify;
                us.Clear(out notify);
            }

            byte[] bs;
            using (BinaryMemoryWriter testWriter = new (ECompressionType.Fast))
            {
                a_args.NewInsertJobsScenario.Serialize(testWriter);
                bs = testWriter.GetBuffer();
            }

            //Send the new scenario
            ScenarioAddNewT addT = new (bs, BaseId.NULL_ID, a_args.NewScenarioName, a_args.OriginalInstigatorId);
            SystemController.ClientSession.SendClientAction(addT);
            SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.InsertJobsStopped, CoPilotStatusUpdateT.CoPilotErrorValues.None);
        }
    }

    #region Events
    public delegate void RuleSeekStoppedDelegate(ScenarioManager a_sm, RuleSeekEndReasons a_reason);

    public event RuleSeekStoppedDelegate RuleSeekStoppedEvent;

    private void FireRuleSeekStoppedEvent(RuleSeekEndReasons a_reason)
    {
        if (RuleSeekStoppedEvent != null)
        {
            RuleSeekStoppedEvent(this, a_reason);
        }
    }

    public delegate void RuleSeekStatusDelegate(ScenarioManager a_sm, CoPilotStatusUpdateT a_t);

    public event RuleSeekStatusDelegate RuleSeekStatusEvent;

    private void FireRuleSeekStatusEvent(CoPilotStatusUpdateT a_t)
    {
        if (RuleSeekStatusEvent != null)
        {
            RuleSeekStatusEvent(this, a_t);
        }
    }

    public delegate void CopilotSettingsInvalidatedDelegate();

    public event CopilotSettingsInvalidatedDelegate CopilotSettingsInvalidatedEvent;

    private void FireCopilotSettingsInvalidatedEvent()
    {
        CopilotSettingsInvalidatedEvent?.Invoke();
    }
    #endregion

    #region Transmission Processing
    private void ProcessInsertJobsStartT(InsertJobsStartT a_t)
    {
        if (m_insertJobsManager == null)
        {
            m_insertJobsManager = new InsertJobsSimulationManager(m_packageManager, m_errorReporter, m_copilotSettings);
            m_insertJobsManager.NewInsertJobsScenarioEvent += new InsertJobsSimulationManager.NewInsertJobsScenarioEventHandler(CreateNewInsertJobsScenario);
        }

        try
        {
            if (m_insertJobsManager.Status == CoPilotSimulationStatus.INITIALIZING || m_insertJobsManager.Status == CoPilotSimulationStatus.RUNNING)
            {
                throw new PTValidationException("2868", new object[] { (int)m_insertJobsManager.Status });
            }

            if (m_ruleSeekManager != null && m_ruleSeekManager.Status != CoPilotSimulationStatus.STOPPED)
            {
                throw new PTValidationException("2865", new object[] { m_ruleSeekManager.Status.ToString() });
            }

            Scenario originalScenario = Find(a_t.ScenarioId);
            //This scenario is not added to the list of scenarios.
            Scenario simScenario = CopyScenario(originalScenario, ScenarioTypes.InsertJobs, a_t.Instigator, false, true);
            using (simScenario.ScenarioSummaryLock.EnterWrite(out ScenarioSummary scenarioSummary))
            {
                ScenarioPlanningSettings planningSettings = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings());
                planningSettings.Production = false;

                ScenarioPermissionSettings permissionSettings = new ScenarioPermissionSettings();
                permissionSettings.UserIdToAccessLevel[a_t.Instigator] = EUserAccess.Edit;
                // Force all inserted scenarios to not be production
                scenarioSummary.ScenarioSettings.SaveSetting(new SettingData(planningSettings), false);
                scenarioSummary.ScenarioSettings.SaveSetting(new SettingData(permissionSettings), false);
            }

            m_insertJobsManager.BeginInsertJobsSimulation(a_t.ScenarioId, simScenario, InsertJobsSettings, a_t.SimulationType, a_t.ListOfJobsToExpedite, m_packageManager, a_t.Instigator);
            SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.InsertJobsStarted, CoPilotStatusUpdateT.CoPilotErrorValues.None);
        }
        catch (PTException pt)
        {
            SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Error, CoPilotStatusUpdateT.CoPilotErrorValues.FailedToStart);
            m_errorReporter.LogException(pt, a_t, ELogClassification.PtSystem, pt.LogToSentry);
        }
        catch (Exception e)
        {
            SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Error, CoPilotStatusUpdateT.CoPilotErrorValues.FailedToStart);
            m_errorReporter.LogException(e, a_t, ELogClassification.Fatal, true);
        }
    }

    private void ProcessScenarioRuleSeekStartT(ScenarioBaseT a_t)
    {
        if (!m_copilotSettings.Enabled)
        {
            //Copilot is not enabled, we can't start it.
            return;
        }

        //Initiate RuleSeek manager if it does not yet exist.
        if (m_ruleSeekManager == null)
        {
            m_ruleSeekManager = new RuleSeekSimulationManager(m_copilotSettings);
            m_ruleSeekManager.NewRuleSeekScenarioEvent += new RuleSeekSimulationManager.NewRuleSeekScenarioEventHandler(CreateNewRuleSeekScenario);
        }

        //Attempt to start the simulation. Fire an event signaling if the simulation has started.
        try
        {
            //Always run RuleSeek on the live scenario
            Scenario s = Find(m_ruleSeekSettings.SourceScenarioId);
            Scenario simScenario = CopyScenario(s, ScenarioTypes.RuleSeek, a_t.Instigator, false, true);

            int scenarioNumRemaining = MaxNumberOfScenarios - LoadedScenarioCount;
            m_ruleSeekManager.BeginRuleSeekSimulations(simScenario, s.Id, RuleSeekSettings, m_packageManager, a_t.TimeStamp.ToDateTime(), scenarioNumRemaining);
            SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.RuleSeekStarted, CoPilotStatusUpdateT.CoPilotErrorValues.None);
        }
        catch (PTException pt)
        {
            SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Error, CoPilotStatusUpdateT.CoPilotErrorValues.FailedToStart);
            m_errorReporter.LogException(pt, a_t, ELogClassification.PtSystem, pt.LogToSentry);
        }
        catch (Exception e)
        {
            SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Error, CoPilotStatusUpdateT.CoPilotErrorValues.FailedToStart);
            m_errorReporter.LogException(e, a_t, ELogClassification.Fatal, true);
        }
    }

    private void ProcessCoPilotSettingsChangeT(ScenarioBaseT a_t)
    {
        CoPilotSettingsChangeT newSettings = a_t as CoPilotSettingsChangeT;

        if (newSettings.RuleSeekSettingsChanged)
        {
            m_ruleSeekSettings = newSettings.RuleSeekSettings;
            UpdateRuleSeekTimer(RuleSeekEndReasons.Dissabled);
        }

        if (newSettings.InsertJobsSettingsChanged)
        {
            m_insertJobsSettings = newSettings.InsertJobsSettings;
        }
    }
    #endregion

    /// <summary>
    /// Updates copilot settings such that they take effect immediately.
    /// This needs to handle the possibility that copilot is running/ impacting the current UI.
    /// </summary>
    /// <param name="a_copilotSettings"></param>
    public void UpdateCopilotSettings(CoPilotSettings a_copilotSettings)
    {
        if (PTSystem.Server)
        {
            RuleSeekEndReasons changeType = GetCopilotUpdateRuleSeekEndReason(a_copilotSettings);

            UpdateCopilotSettingsReferences(a_copilotSettings);

            UpdateRuleSeekTimer(changeType);

            FireCopilotSettingsInvalidatedEvent();
        }
    }

    /// <summary>
    /// Determines the nature of the Copilot Settings change, particularly if the change would involve enabling/disabling it.
    /// </summary>
    /// <param name="a_newCopilotSettings"></param>
    /// <returns></returns>
    private RuleSeekEndReasons GetCopilotUpdateRuleSeekEndReason(CoPilotSettings a_newCopilotSettings)
    {
        // In general, if the change was just to settings/resources, we set to a flag that indicates copilot can continue.
        RuleSeekEndReasons endReason = RuleSeekEndReasons.CopilotSettingsChanged;

        // Otherwise, if toggled on or off, we want to capture that
        if (m_copilotSettings.Enabled && !a_newCopilotSettings.Enabled)
        {
            endReason = RuleSeekEndReasons.Dissabled;
        }
        else if (!m_copilotSettings.Enabled && a_newCopilotSettings.Enabled)
        {
            endReason = RuleSeekEndReasons.Enabled;
        }

        return endReason;
    }

    private void UpdateCopilotSettingsReferences(CoPilotSettings a_coPilotSettings)
    {
        m_copilotSettings = a_coPilotSettings;

        // Existing RuleSeekManager holds onto old reference unless updated as well
        if (m_ruleSeekManager != null)
        {
            m_ruleSeekManager.CoPilotSettings = m_copilotSettings;
        }

        // Same thing with InsertJobsManager
        if (m_insertJobsManager != null)
        {
            m_insertJobsManager.CoPilotSettings = m_copilotSettings;
        }

        // Disable ruleSeek or insertJobs if new copilot settings wouldn't allow it.
        if (m_ruleSeekSettings.Enabled &&
            (!m_copilotSettings.Enabled || !PTSystem.LicenseKey.IncludeCoPilot))
        {
            m_ruleSeekSettings.Enabled = false;
        }

        if (m_insertJobsSettings.Enabled &&
            (!m_copilotSettings.Enabled || !PTSystem.LicenseKey.IncludeCoPilot))
        {
            m_insertJobsSettings.Enabled = false;
        }
    }

    private void PruneScenario(PruneScenarioT a_t)
    {
        //Initiate PruneScenarioManager if it does not yet exist.
        if (m_pruneScenarioManager == null)
        {
            m_pruneScenarioManager = new PruneScenarioManager();
            m_pruneScenarioManager.NewPrunedScenarioEvent += new PruneScenarioManager.NewPrunedScenarioEventHandler(CreateNewPrunedScenario);
        }

        //Attempt to start the simulation. Fire an event signaling if the simulation has started.
        try
        {
            //Always run RuleSeek on the live scenario
            DeleteAllScenariosExceptOfType(ScenarioTypes.Live);
            Scenario SimScenario = CopyScenario(GetFirstProductionScenario(), ScenarioTypes.Pruned, a_t.Instigator, false, true);
            m_pruneScenarioManager.BeginPruneScenarioSimulation(SimScenario, a_t, m_packageManager);
        }
        catch (PTException pt)
        {
            SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Error, CoPilotStatusUpdateT.CoPilotErrorValues.FailedToStart);
            m_errorReporter.LogException(pt, a_t, ELogClassification.PtSystem, pt.LogToSentry);
        }
        catch (Exception e)
        {
            SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Error, CoPilotStatusUpdateT.CoPilotErrorValues.FailedToStart);
            m_errorReporter.LogException(e, a_t, ELogClassification.Fatal, true);
        }
    }

    /// <summary>
    /// Updates scenario type and broadcasts the new scenario
    /// </summary>
    private void CreateNewPrunedScenario(object sender, PruneScenarioManager.NewPrunedScenarioEventArgs a_args)
    {
        lock (m_deleteOldScenariosLock) //there could potentially be multiple Scenarios created at a time.
        {
            //Send the new scenario
            ScenarioSummary ss;
            using (a_args.NewPrunedScenario.ScenarioSummaryLock.EnterWrite(out ss))
            {
                ss.Name = a_args.NewScenarioName;
                ss.SetType(ScenarioTypes.Pruned);
            }

            Scenario.UndoSets us;
            using (a_args.NewPrunedScenario.UndoSetsLock.EnterWrite(out us))
            {
                bool notify;
                us.Clear(out notify);
            }

            byte[] bs;
            using (BinaryMemoryWriter testWriter = new (ECompressionType.Fast))
            {
                a_args.NewPrunedScenario.Serialize(testWriter);
                bs = testWriter.GetBuffer();
            }

            //Send the new scenario
            ScenarioAddNewT addT = new ScenarioAddNewPrunedT(bs, BaseId.NULL_ID);
            SystemController.ClientSession.SendClientAction(addT);
        }
    }
}