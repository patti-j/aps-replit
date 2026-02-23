using PT.APSCommon;
using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.Definitions;
using PT.Transmissions;

namespace PT.Scheduler.CoPilot.RuleSeek;

/// <summary>
/// Manages all RuleSeek simulations.  Only one scenario can used to run simulations at once.
/// Raises an event when a new score is found by one of the running simulations.
/// </summary>
internal class RuleSeekSimulationManager
{
    internal RuleSeekSimulationManager(CoPilotSettings a_coPilotSettings)
    {
        m_activeSimulations = new List<RuleSeekSimulation>();
        m_sysEvalAndSimStartTimer.Elapsed += new System.Timers.ElapsedEventHandler(RecurringSysEvalAndSimManagement);
        m_broadcastResultsTimer = new System.Timers.Timer();
        m_broadcastResultsTimer.Elapsed += new System.Timers.ElapsedEventHandler(SendNewScenario);
        m_bestSimResultStack = new Stack<ScenarioCopyForRuleSeekT>();
        m_coPilotSettings = a_coPilotSettings;

        //Initialize performance counters
        //m_cpuCounterTotal = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        //m_cpuCounterGalaxy = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
        //m_memoryCounterTotal = new PerformanceCounter("Memory", "Available MBytes");
        //new PerformanceCounter("Memory", "Available MBytes", Process.GetCurrentProcess().ProcessName);
    }

    #region MEMBERS
    //List of running simulations
    private readonly List<RuleSeekSimulation> m_activeSimulations;

    //List of Averages
    private const int c_MAX_QUEUE_LENGTH = 10;
    private readonly Queue<int> m_cpuPercentageTotalQueue = new (c_MAX_QUEUE_LENGTH);
    private readonly Queue<int> m_cpuPercentageGalaxyQueue = new (c_MAX_QUEUE_LENGTH);

    //The current status of simulations. Also maintains the monitoring timer for the manager.
    private CoPilotSimulationStatus m_status;

    public CoPilotSimulationStatus Status => m_status;

    private readonly System.Timers.Timer m_sysEvalAndSimStartTimer = new (5000);
    private Scenario m_originalScenario;
    private BaseId m_originalScenarioId;
    private long m_scenarioSizeInBytes;
    private RuleSeekSettings m_settings;

    /// <summary>
    /// Copilot Settings. May be updated live if the SaveCopilotSettings is called.
    /// </summary>
    private CoPilotSettings m_coPilotSettings;

    public CoPilotSettings CoPilotSettings
    {
        set => m_coPilotSettings = value;
    }

    private readonly object m_scoresLock = new ();
    private readonly object m_simulationsLock = new ();
    private static RuleSeekScoreSet s_topScores;
    //private readonly PerformanceCounter m_cpuCounterTotal;
    //private readonly PerformanceCounter m_cpuCounterGalaxy;
    //private readonly PerformanceCounter m_memoryCounterTotal;
    private readonly Stack<ScenarioCopyForRuleSeekT> m_bestSimResultStack;
    private bool m_isBroadcastResultsTimerRunning;
    private bool m_isFirstResult = true;
    private readonly System.Timers.Timer m_broadcastResultsTimer;
    private readonly IScenarioDataChanges m_dataChanges;
    private IPackageManager m_packageManager;
    #endregion

    #region DIAGNOSTICS
    private DateTime m_simulationStartTime = DateTime.MinValue;
    private DateTime m_simulationStopTime = DateTime.MinValue;
    private DateTime m_timeBestResultWasFound = DateTime.MinValue;

    private long m_simulationiterationsFromCompletedTasks;
/*
        long m_kpiTimesBestValueFound;
*/

    /// <summary>
    /// Calculate and returns the current Diagnostics for RuleSeek
    /// </summary>
    /// <returns></returns>
    public RuleSeekDiagnositcs CalcRuleSeekDiagnostics()
    {
        RuleSeekDiagnositcs currentDiagnostics = new ();
        currentDiagnostics.RuleSeekStatus = m_status;
        currentDiagnostics.SimulationDuration = CalcCurrentSimulationDuration();
        currentDiagnostics.DurationToFindBestScenario = GetDurationForBestScenario();
        currentDiagnostics.RuleSeekSimulationIterations = CalcCurrentSimulationIterations();
        currentDiagnostics.SimulationStartTime = m_simulationStartTime.ToServerTime();

        return currentDiagnostics;
    }

    /// <summary>
    /// Calculates the duration the simulation has been running.
    /// </summary>
    private TimeSpan CalcCurrentSimulationDuration()
    {
        if (m_status == CoPilotSimulationStatus.RUNNING)
        {
            return DateTime.Now - m_simulationStartTime;
        }

        if (m_simulationStartTime != DateTime.MinValue && m_simulationStopTime != DateTime.MinValue)
        {
            return m_simulationStopTime - m_simulationStartTime;
        }

        return new TimeSpan(0);
    }

    /// <summary>
    /// Calculates the number of optimizes that have been performed since the simulation began.
    /// </summary>
    private long CalcCurrentSimulationIterations()
    {
        //Since ruleseek stops and starts simulations as CPU usage changes,
        // the number of optimizes from completed tasks are stored when they stop.
        lock (m_simulationsLock)
        {
            long currentIterations = 0;
            for (int i = 0; i < m_activeSimulations.Count; i++)
            {
                currentIterations += m_activeSimulations[i].CurrentOptimizeIterations;
            }

            return currentIterations + m_simulationiterationsFromCompletedTasks;
        }
    }

    /// <summary>
    /// Returns the duration the simulation ran before it found the best simulation.
    /// </summary>
    /// <returns></returns>
    private TimeSpan GetDurationForBestScenario()
    {
        if (m_status == CoPilotSimulationStatus.RUNNING && m_timeBestResultWasFound != DateTime.MinValue)
        {
            return m_timeBestResultWasFound - m_simulationStartTime;
        }

        return new TimeSpan(0);
    }
    #endregion

    private bool ValidateSettings()
    {
        //Validate KPI exists
        List<KPI> kpiList;
        kpiList = m_originalScenario.KpiController.KpiList.UnsortedKpiList;

        bool kpiFound = false;
        foreach (KPI kpi in kpiList)
        {
            if (kpi.CalculatorName == m_settings.KpiToRun)
            {
                kpiFound = true;
                break;
            }
        }

        if (!kpiFound)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Terminates all running simulations.
    /// It is possible that a new score is being reported and a new scenario will be generated after this method is called.
    /// </summary>
    internal void Abort()
    {
        m_simulationStopTime = DateTime.Now;
        m_status = CoPilotSimulationStatus.STOPPED;
        lock (m_scoresLock)
        {
            m_bestSimResultStack.Clear();
        }

        m_sysEvalAndSimStartTimer.Stop();
        m_broadcastResultsTimer.Stop();

        lock (m_simulationsLock)
        {
            foreach (RuleSeekSimulation sim in m_activeSimulations)
            {
                sim.SignalStop();
            }
        }
        //RecurringSysEvalAndSimManagement(this, null);
    }

    /// <summary>
    /// Begin running simulations.
    /// This is the entry point to initiate all simulations.
    /// Throws an exception if the simulations cannot be started.
    /// </summary>
    /// <param name="a_scenario">Scenario to use</param>
    /// <param name="a_originalScenarioId"></param>
    /// <param name="a_performanceSettings">CPU usage performance settings</param>
    /// <param name="a_settings">Current RuleSeekSettings</param>
    /// <param name="a_packageManager">Loads the customizations when scenario is copied</param>
    /// <param name="a_transmissionTimeStamp"></param>
    /// <param name="a_dataChanges"></param>
    /// <param name="a_scenarioNumRemaining"></param>
    /// <returns></returns>
    internal void BeginRuleSeekSimulations(Scenario a_scenario,
                                           BaseId a_originalScenarioId,
                                           RuleSeekSettings a_settings,
                                           IPackageManager a_packageManager,
                                           DateTime a_transmissionTimeStamp,
                                           int a_scenarioNumRemaining)
    {
        try
        {
            if (Status != CoPilotSimulationStatus.STOPPED)
            {
                throw new PTValidationException("2865", new object[] { Status });
            }

            m_simulationStopTime = PTDateTime.MinValue.ToDateTime();
            m_status = CoPilotSimulationStatus.RUNNING;
            s_topScores = new RuleSeekScoreSet(a_settings.MaxScenariosToKeep);
            m_originalScenario = a_scenario;
            m_originalScenarioId = a_originalScenarioId;
            m_settings = a_settings;

            m_settings.MaxScenariosToKeep = Math.Min(a_scenarioNumRemaining, a_settings.MaxScenariosToKeep);
            m_packageManager = a_packageManager;
            m_isFirstResult = true;

            if (!ValidateSettings())
            {
                throw new PTValidationException("2866");
            }

            //Calculate the memory of a scenario
            m_scenarioSizeInBytes = m_originalScenario.CopyInMemory(out ScenarioDetail _, out ScenarioSummary _);

            //start the first simulation
            StartNewSimulation(a_transmissionTimeStamp);

            //Start the timers
            //TODO: These can be m,ade configurable on SM Copilot Settings UI
            m_broadcastResultsTimer.Interval = 12 * 1000;
            m_sysEvalAndSimStartTimer.Interval = 10 * 1000;
            m_sysEvalAndSimStartTimer.Start();

            //Reset diagnostics
            m_simulationStartTime = DateTime.Now;
            m_simulationiterationsFromCompletedTasks = 0;
            m_timeBestResultWasFound = DateTime.MinValue;
        }
        catch (PTValidationException)
        {
            throw;
        }
        catch (Exception)
        {
            m_sysEvalAndSimStartTimer.Stop();
            m_status = CoPilotSimulationStatus.ERROR;
            throw;
        }
    }

    public void UpdateScenarioLimitSetting(int a_scenariosRemaining)
    {
        m_settings.MaxScenariosToKeep = Math.Min(a_scenariosRemaining, m_settings.MaxScenariosToKeep);
    }

    /// <summary>
    /// Creates a new simulation and starts it in a new task.
    /// A new simulation will not be created if MaxSimulations has been reached.
    /// </summary>
    /// <param name="a_timeStamp"></param>
    private void StartNewSimulation(DateTime a_timeStamp)
    {
        lock (m_simulationsLock)
        {
            RuleSeekSimulation newSim = new (m_originalScenario, m_settings, m_packageManager, a_timeStamp);
            newSim.RuleSeekSimulationProgressEvent += new RuleSeekSimulation.RuleSeekSimulationProgressEventHandler(NewScoreSet);
            newSim.RuleSeekSimulationCompleteEvent += new RuleSeekSimulation.RuleSeekSimulationCompleteEventHandler(SimulationComplete);
            m_activeSimulations.Add(newSim);
            newSim.StartSimulation();
        }
    }

    /// <summary>
    /// Removes the last started simulation.
    /// The simulation will not be removed if there is only 1 running simulation.
    /// </summary>
    private void RemoveSimulation()
    {
        lock (m_simulationsLock)
        {
            if (m_activeSimulations.Count > 1)
            {
                m_activeSimulations[m_activeSimulations.Count - 1].SignalStop();
            }
        }
    }

    #region EVENTS
    internal delegate void NewRuleSeekScenarioEventHandler(object sender, NewRuleSeekScenarioEventArgs e);

    internal event NewRuleSeekScenarioEventHandler NewRuleSeekScenarioEvent;

    internal class NewRuleSeekScenarioEventArgs : EventArgs
    {
        internal NewRuleSeekScenarioEventArgs(ScenarioCopyForRuleSeekT a_copyT)
        {
            m_scenarioCopyT = a_copyT;
        }

        private readonly ScenarioCopyForRuleSeekT m_scenarioCopyT;

        internal ScenarioCopyForRuleSeekT NewRuleSeekT => m_scenarioCopyT;
    }

    /// <summary>
    /// Handles a new score set found by RuleSeekSimulation.
    /// </summary>
    /// <param name="a_args">RuleSeekEventArgs</param>
    private void NewScoreSet(object a_sender, RuleSeekSimulation.RuleSeekProgressEventArgs a_args)
    {
        bool newTopScore;
        lock (m_scoresLock)
        {
            //Check if score is better than previously created scenarios.
            newTopScore = s_topScores.Add(a_args.Score);
        }

        if (newTopScore)
        {
            m_timeBestResultWasFound = DateTime.Now;

            //send a new scenario transmission.             
            List<BalancedCompositeDispatcherDefinitionUpdateT> updateList = new ();

            //Add all of the modified rule sets to the transmission
            for (int i = 0; i < a_args.Score.RuleSet.Count; i++)
            {
                BalancedCompositeDispatcherDefinitionUpdateT updateT;
                updateT = new BalancedCompositeDispatcherDefinitionUpdateT(m_originalScenarioId, a_args.Score.RuleSet[i].Id);
                GetBalancedCompositeDispatcherDfinitionUpdateT(updateT, a_args.Score.RuleSet[i], a_args.Score.Score, a_args.KpiName);
                updateList.Add(updateT);
            }

            ScenarioCopyForRuleSeekT copyT = new (m_originalScenarioId, ScenarioTypes.RuleSeek, updateList, a_args.KpiName, a_args.Score.Score, a_args.Score.LowerIsBetter, a_args.KpiFormatString);

            //This will be sent later to avoid rapidly sending out multiple results.
            m_bestSimResultStack.Push(copyT);
            if (m_isFirstResult)
            {
                m_isFirstResult = false;
                SendNewScenario(null, null);
            }
            else if (!m_isBroadcastResultsTimerRunning)
            {
                m_isBroadcastResultsTimerRunning = true;
                if (Status == CoPilotSimulationStatus.RUNNING)
                {
                    m_broadcastResultsTimer.Start();
                }
            }
        }
    }

    /// <summary>
    /// Gets the next highest score and send the result.
    /// If the next score is no longer valid it is not sent and is removed from the queue.
    /// Called by the broadcastResultsTimer.
    /// </summary>
    private void SendNewScenario(object sender, EventArgs a_args)
    {
        lock (m_scoresLock)
        {
            m_broadcastResultsTimer.Stop();

            //Report result to ScenarioManager
            ScenarioCopyForRuleSeekT copyT = m_bestSimResultStack.Pop();

            //Only send the transmission if it is still in the top score list.
            if ((copyT.IsLowerBetter && copyT.Score <= s_topScores.WorstScore.Score) || (!copyT.IsLowerBetter && copyT.Score >= s_topScores.WorstScore.Score))
            {
                NewRuleSeekScenarioEvent(this, new NewRuleSeekScenarioEventArgs(copyT));
            }

            if (m_bestSimResultStack.Count == 0 || Status != CoPilotSimulationStatus.RUNNING)
            {
                m_isBroadcastResultsTimerRunning = false;
            }
            else
            {
                m_isBroadcastResultsTimerRunning = true;
                m_broadcastResultsTimer.Start();
            }
        }
    }

    private void SimulationComplete(object a_sender, RuleSeekSimulation.RuleSeekCompletedEventArgs a_args)
    {
        lock (m_simulationsLock)
        {
            //This simulation is competing and will terminate itself. Remove the reference and update diagnostic info.
            RuleSeekSimulation endingSim = a_sender as RuleSeekSimulation;
            if (endingSim != null)
            {
                m_simulationiterationsFromCompletedTasks += endingSim.CurrentOptimizeIterations;
            }

            m_activeSimulations.Remove(a_sender as RuleSeekSimulation);
            if (a_args.CompletionStatus == CoPilotSimulationStatus.ERROR)
            {
                //RuleSeek TODO: handle this error
            }
        }
    }
    #endregion

    /// <summary>
    /// Converts a RuleSeekScore into a BalancedCompositeDispatcherDfinitionUpdateT transmission
    /// </summary>
    /// <param name="a_updateT">BalancedCompositeDispatcherDfinitionUpdateT</param>
    /// <param name="a_rule">BalancedCompositeDispatcherDfinition</param>
    /// <param name="a_score">KPI score value</param>
    /// <param name="a_kpiName">KPI name to append to the rule name</param>
    private void GetBalancedCompositeDispatcherDfinitionUpdateT(BalancedCompositeDispatcherDefinitionUpdateT a_updateT, BalancedCompositeDispatcherDefinition a_rule, decimal a_score, string a_kpiName)
    {
        string coPilotString = " | CoPilot: ".Localize();
        try
        {
            //Overwrite the previous score part of the name if it exists.
            if (a_rule.Name.Contains(coPilotString))
            {
                string[] stringNameParts = a_rule.Name.Split(new[] { coPilotString }, StringSplitOptions.None);
                a_updateT.Name = $"{stringNameParts[0]} {coPilotString} {a_kpiName} ({a_score:F4})";
            }
            else
            {
                a_updateT.Name = $"{a_rule.Name} {coPilotString} {a_kpiName} ({a_score:F4})";
            }
        }
        catch
        {
            a_updateT.Name = $"{a_rule.Name} {a_kpiName}: {a_score:F4}";
        }

        a_updateT.GlobalMinScore = a_rule.GlobalMinScore;

        foreach (string activeRule in a_rule.OptimizeSettings.GetAllRules())
        {
            (decimal minScore, bool useMinScore) = a_rule.OptimizeSettings.GetMinimumScore(activeRule);
            BalancedCompositeDispatcherDefinitionUpdateT.OptimizeRuleElementSettings settings = new ()
            {
                Points = a_rule.OptimizeSettings.GetRulePoints(activeRule),
                ResourceMultiplier = a_rule.OptimizeSettings.GetRuleResourceMultiplier(activeRule),
                MinimumScore = minScore,
                UseMinimumScore = useMinScore,
                Settings = a_rule.OptimizeSettings.GetRuleSettings(activeRule)
            };
            a_updateT.UpdatedMappings.Add((activeRule, settings));
        }
    }

    #region SYSTEM EVALUATION
    /// <summary>
    /// Checks memory and CPU usage to manage running instances
    /// </summary>
    private void RecurringSysEvalAndSimManagement(object Sender, EventArgs a_args)
    {
        m_sysEvalAndSimStartTimer.Stop();

        //Check if a new simulation can be started
        decimal result = EvaluateSystemResourcesForNewSimulation();
        if (result >= 1)
        {
            //Attempt to start another simulation
            StartNewSimulation(DateTime.UtcNow);
        }
        else if (result < 0)
        {
            //Remove the last created simulation
            RemoveSimulation();
        }
        else if (result != 0) //A ratio between 0 and 1, keep 1 sim, but add a delay
        {
            m_activeSimulations[0].AddOneTimeDelay(1 - result);
        }

        if (Status == CoPilotSimulationStatus.RUNNING)
        {
            m_sysEvalAndSimStartTimer.Start();
        }
    }

    private decimal m_concurrentSimulationFraction;

    /// <summary>
    /// This function will evaluate system memory and cpu usages to determine if a new Simulation should be started.
    /// It takes into account RuleSeekSettings.PerformanceValues and average cpu/memory usages
    /// This can be a sizable overhead when running CoPilot using multiple simulations.
    /// </summary>
    /// <returns>Whether a new simulation can be started</returns>
    private decimal EvaluateSystemResourcesForNewSimulation()
    {
        DateTime burstTimeEnd = m_simulationStartTime.Add(m_coPilotSettings.BurstDuration);

        decimal cpuAllowedUsage = DateTime.UtcNow < burstTimeEnd
            ? m_coPilotSettings.AverageCpuUsage * (m_coPilotSettings.BoostPercentage / 100m)
            : m_coPilotSettings.AverageCpuUsage;

        cpuAllowedUsage += m_concurrentSimulationFraction; //Append the remainder from last check. This will average out the CPU usage

        decimal floor = Math.Floor(cpuAllowedUsage);
        if (floor == 0)
        {
            //We always need 1 to run but we can delay it
            m_concurrentSimulationFraction += cpuAllowedUsage;
            return m_concurrentSimulationFraction;
        }

        m_concurrentSimulationFraction = cpuAllowedUsage - floor;

        if (m_activeSimulations.Count < floor)
        {
            return 1;
        }

        if (m_activeSimulations.Count > floor)
        {
            return -1;
        }

        //Active Sims == cpuAllowedUsage
        return 0;


        //TODO: Remove or refactor obsolete system mem calculations. This is not reliable enough for a server with many instances

        //Even if the max simulations have been reached, this should continue so the average cpu usages will be calculated.
        //try
        //{
        //    //Calculate memory
        //    float currentAvailableMemoryMB = m_memoryCounterTotal.NextValue();

        //    //Calculate CPU %
        //    float cpuValueTotal = m_cpuCounterTotal.NextValue();
        //    float cpuValueGalaxy = m_cpuCounterGalaxy.NextValue() / Environment.ProcessorCount;

        //    if (cpuValueTotal == 0)
        //    {
        //        //This can occur the first time the value is queried
        //        System.Threading.Thread.Sleep(1000);
        //        cpuValueTotal = m_cpuCounterTotal.NextValue();
        //        cpuValueGalaxy = m_cpuCounterGalaxy.NextValue() / Environment.ProcessorCount;
        //    }

        //    //Add current CPU value to CPU % average queue
        //    if (m_cpuPercentageTotalQueue.Count >= c_MAX_QUEUE_LENGTH)
        //    {
        //        m_cpuPercentageTotalQueue.Dequeue();
        //        m_cpuPercentageGalaxyQueue.Dequeue();
        //    }
        //    m_cpuPercentageTotalQueue.Enqueue((int)cpuValueTotal);
        //    m_cpuPercentageGalaxyQueue.Enqueue((int)cpuValueGalaxy);

        //    //Get average CPU value
        //    decimal currentCPUTotalAverage = (decimal)m_cpuPercentageTotalQueue.Average();
        //    decimal currentCPUGalaxyAverage = (decimal)m_cpuPercentageGalaxyQueue.Average();

        //    //Check if system can handle another simulation
        //    if (currentCPUGalaxyAverage <= m_performanceValues.CpuLimitPercentageGalaxy)
        //    {
        //        if (currentCPUTotalAverage <= m_performanceValues.CpuLimitPercentageTotal)
        //        {
        //            if ((m_scenarioSizeInBytes * m_performanceValues.MemoryScalar) < (currentAvailableMemoryMB * 1024 * 1024))
        //            {
        //                return SystemEvaluationResults.CanStartNewSimulation;
        //            }
        //        }
        //    }

        //    if (currentCPUGalaxyAverage > m_performanceValues.CpuLimitPercentageGalaxy)
        //    {
        //        return SystemEvaluationResults.ShouldRemoveSimulation;
        //    }

        //    if (currentCPUTotalAverage > m_performanceValues.CpuLimitPercentageTotal)
        //    {
        //        return SystemEvaluationResults.ShouldRemoveSimulation;
        //    }

        //    return SystemEvaluationResults.NoSimulationChangesNeeded;
        //}
        //catch
        //{
        //    return SystemEvaluationResults.NoSimulationChangesNeeded;
        //}
    }
    #endregion
}