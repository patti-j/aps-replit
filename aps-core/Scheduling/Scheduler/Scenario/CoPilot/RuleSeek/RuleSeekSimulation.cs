using PT.APSCommon;
using PT.Scheduler.PackageDefs;
using PT.Scheduler.Simulation.Dispatcher.Dispatchers.BalancedCompositeDispatcher;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.CoPilot.RuleSeek;

/// <summary>
/// This class performs the actual RuleSeek simulation. It is started by RuleSeekSimulationManager.
/// </summary>
internal class RuleSeekSimulation
{
    internal RuleSeekSimulation(Scenario a_scenario, RuleSeekSettings a_settings, IPackageManager a_packageManager, DateTime a_transmissionTimeStamp)
    {
        m_settings = a_settings;
        m_simUtilities = new CoPilotSimulationUtilities(a_packageManager);

        m_simUtilities.CopyAndStoreScenario(a_scenario, out m_sdByteArray, out m_ssByteArray); //Create a new simulation scenario

        if (a_settings.SliderAdjustmentModeType == RuleSeekSettings.SliderAdjustmentLogic.Random || a_settings.SliderAdjustmentModeType == RuleSeekSettings.SliderAdjustmentLogic.SequentialRandom)
        {
            m_rand = new Random((int)a_transmissionTimeStamp.Ticks);
        }

        m_scores = new RuleSeekScoreSet(m_settings.MaxScenariosToKeep);
        Status = CoPilotSimulationStatus.STOPPED;
    }

    #region EVENTS
    /// <summary>
    /// Event to report scores back to SimulationManager
    /// </summary>
    internal class RuleSeekProgressEventArgs : EventArgs
    {
        internal RuleSeekProgressEventArgs(string a_KpiName, string a_kpiFormatString, RuleSeekScore a_score)
        {
            m_score = a_score;
            m_kpiName = a_KpiName;
            m_kpiFormatString = a_kpiFormatString;
        }

        private readonly RuleSeekScore m_score;

        internal RuleSeekScore Score => m_score;

        private readonly string m_kpiName;

        internal string KpiName => m_kpiName;

        private readonly string m_kpiFormatString;

        internal string KpiFormatString => m_kpiFormatString;
    }

    internal delegate void RuleSeekSimulationProgressEventHandler(object sender, RuleSeekProgressEventArgs a_e);

    internal event RuleSeekSimulationProgressEventHandler RuleSeekSimulationProgressEvent;

    /// <summary>
    /// Event to report scores back to SimulationManager
    /// </summary>
    internal class RuleSeekCompletedEventArgs : EventArgs
    {
        internal RuleSeekCompletedEventArgs(CoPilotSimulationStatus a_status, Exception a_error)
        {
            m_status = a_status;
            m_error = a_error;
        }

        private readonly CoPilotSimulationStatus m_status;

        internal CoPilotSimulationStatus CompletionStatus => m_status;

        private readonly Exception m_error;

        internal Exception CompletionError => m_error;
    }

    internal delegate void RuleSeekSimulationCompleteEventHandler(object sender, RuleSeekCompletedEventArgs a_e);

    internal event RuleSeekSimulationCompleteEventHandler RuleSeekSimulationCompleteEvent;

    /// <summary>
    /// An improved score is found from this simulation. Reports back to SimulationManager
    /// </summary>
    /// <param name="a_score"></param>
    private void NewScoreFound(RuleSeekScore a_score)
    {
        if (RuleSeekSimulationProgressEvent != null)
        {
            RuleSeekProgressEventArgs args = new (m_settings.KpiToRun, m_kpiFormatString, a_score);
            RuleSeekSimulationProgressEvent(this, args);
        }
        else
        {
            //Nothing is listening, just end the simulation.
            Status = CoPilotSimulationStatus.COMPLETED;
        }
    }
    #endregion

    #region MEMBERS
    private readonly RuleSeekSettings m_settings;
    private readonly Random m_rand;
    internal CoPilotSimulationStatus Status;
    private readonly RuleSeekScoreSet m_scores;

    public RuleSeekScoreSet ScoreSet => m_scores;

    private bool m_lowerIsBetter;
    private string m_kpiFormatString;
    private decimal m_bestKPIScore;
    private readonly byte[] m_sdByteArray;
    private readonly byte[] m_ssByteArray;
    private readonly CoPilotSimulationUtilities m_simUtilities;
    private TimeSpan m_simDuration;
    private TimeSpan m_simStartDelay;

    //Diagnostics
    private long m_currentIterations;
    private long m_timesFoundBestKPI;
    #endregion

    /// <summary>
    /// Signals the simulation to stop at the next opportunity.
    /// </summary>
    /// <param name="a_abort">Signal that the simulation was completed in a way that any found results are not needed</param>
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

    /// <summary>
    /// The current number of optimizes performed by this simulation
    /// </summary>
    internal long CurrentOptimizeIterations => m_currentIterations;

    /// <summary>
    /// The number of times the best kpi for this simulation has been found.
    /// </summary>
    internal long NbrOfTimesFoundBestKpiScore => m_timesFoundBestKPI;

    #region SIMULATION
    /// <summary>
    /// Bigins the simulation.
    /// This function will loop until specifically told to cancel by the SimulationManager, or if there is an error.
    /// </summary>
    private void Simulate()
    {
        try
        {
            #if DEBUG
            Thread.CurrentThread.Name = "RuleSeekSimulation";
            #endif
            Status = CoPilotSimulationStatus.RUNNING;
            InitialSimulationProcessing();
            bool firstSimulation = true;

            while (true) //Loop breaks when simulation is canceled
            {
                if (Status != CoPilotSimulationStatus.RUNNING)
                {
                    break;
                }

                if (m_simStartDelay > TimeSpan.Zero)
                {
                    Thread.Sleep(m_simStartDelay);
                    m_simStartDelay = TimeSpan.Zero;
                }

                DateTime start = DateTime.UtcNow;

                //Copy scenario
                Scenario newScenario = m_simUtilities.CopySimScenario(m_sdByteArray, m_ssByteArray);

                ScenarioDetail newSd;
                using (newScenario.ScenarioDetailLock.EnterWrite(out newSd))

                {
                    if (!firstSimulation)
                    {
                        for (int i = 0; i < newSd.DispatcherDefinitionManager.Count; i++)
                        {
                            if (m_settings.RuleSets.Contains(newSd.DispatcherDefinitionManager[i].Id))
                            {
                                if (m_settings.RuleAdjustmentMode == RuleSeekSettings.SlidersToAdjustMode.InUse)
                                {
                                    AdjustRuleValuesInUseRand(newSd.DispatcherDefinitionManager[i]);
                                }
                                else if (m_settings.RuleAdjustmentMode == RuleSeekSettings.SlidersToAdjustMode.All)
                                {
                                    AdjustRuleValuesAllRand(newSd.DispatcherDefinitionManager[i], newSd);
                                }
                            }
                        }
                    }
                }

                if (Status != CoPilotSimulationStatus.RUNNING)
                {
                    newScenario.Dispose();
                    break;
                }

                //Optimize
                ScenarioDetailOptimizeT optimizeT = new (BaseId.NULL_ID, null, false);
                newSd.OptimizeHandler(optimizeT, newSd.OptimizeSettings, new ScenarioDataChanges());

                firstSimulation = false;

                if (Status != CoPilotSimulationStatus.RUNNING)
                {
                    newScenario.Dispose();
                    break;
                }

                //Caluate and record score
                decimal newScore = newScenario.KpiController.CalculateKPIByName(newSd, m_settings.KpiToRun).Calculation;

                //Update Diagnostics
                m_currentIterations++;

                if (m_bestKPIScore == newScore)
                {
                    m_timesFoundBestKPI++;
                }
                else if ((!m_lowerIsBetter && newScore > m_bestKPIScore) || (m_lowerIsBetter && newScore < m_bestKPIScore))
                {
                    if (m_bestKPIScore != decimal.MaxValue && m_bestKPIScore != decimal.MinValue)
                    {
                        List<BalancedCompositeDispatcherDefinition> rulesList = new ();
                        for (int i = 0; i < m_settings.RuleSets.Count; i++)
                        {
                            rulesList.Add((BalancedCompositeDispatcherDefinition)newSd.DispatcherDefinitionManager.GetById(m_settings.RuleSets[i]));
                        }

                        NewScoreFound(new RuleSeekScore(newScore, m_lowerIsBetter, rulesList.ToArray()));
                        m_timesFoundBestKPI = 1;
                    }

                    m_bestKPIScore = newScore;
                }

                newScenario.Dispose();

                m_simDuration = DateTime.UtcNow - start;
            }


            //The simulation is done. The Simulation Manager is listening to the event
            if (RuleSeekSimulationCompleteEvent != null)
            {
                RuleSeekCompletedEventArgs args = new (Status, null);
                RuleSeekSimulationCompleteEvent(this, args);
                //End simulation
            }
        }
        catch (Exception e)
        {
            if (RuleSeekSimulationCompleteEvent != null)
            {
                RuleSeekCompletedEventArgs args = new (Status, e);
                RuleSeekSimulationCompleteEvent(this, args);
                //End simulation
            }
        }
    }
    #endregion

    /// <summary>
    /// Performs a single initial configuration for the Simulation.
    /// </summary>
    private void InitialSimulationProcessing()
    {
        Status = CoPilotSimulationStatus.RUNNING;
        Scenario tempScenario = m_simUtilities.CopySimScenario(m_sdByteArray, m_ssByteArray);

        m_lowerIsBetter = false;
        //Find KPI
        foreach (KPI kpi in tempScenario.KpiController.KpiList.UnsortedKpiList)
        {
            if (kpi.CalculatorName == m_settings.KpiToRun)
            {
                m_lowerIsBetter = kpi.Calculator.LowerIsBetter;
                m_kpiFormatString = kpi.Calculator.FormatString;
                break;
            }
        }

        m_bestKPIScore = decimal.MinValue;
        if (m_lowerIsBetter)
        {
            m_bestKPIScore = decimal.MaxValue;
        }

        tempScenario.Dispose();
    }

    #region RULLE SET ADJUSTMENTS
    //TODO: Have copilot settings for modifying resource multipliers and custom factor settings
    /// <summary>
    /// Randomly adjust all active factors.
    /// </summary>
    private void AdjustRuleValuesInUseRand(BalancedCompositeDispatcherDefinition a_rule)
    {
        foreach (string activeRule in a_rule.OptimizeSettings.GetActiveRules())
        {
            decimal originalPoints = a_rule.OptimizeSettings.GetRulePoints(activeRule);
            int minRand = originalPoints > 0 ? 0 : -1000;
            int maxRand = originalPoints > 0 ? 1000 : 0;
            a_rule.OptimizeSettings.AddOrUpdateMapping(activeRule,
                new OptimizeRuleWeightSettings.OptimizeRuleElementSettings
                {
                    ResourceMultiplier = a_rule.OptimizeSettings.GetRuleResourceMultiplier(activeRule), //Keep original resource multiplier
                    Settings = a_rule.OptimizeSettings.GetRuleSettings(activeRule),
                    Points = m_rand.Next(minRand, maxRand)
                });
        }
    }

    /// <summary>
    /// Adjusts factors values randomly
    /// </summary>
    private void AdjustRuleValuesAllRand(BalancedCompositeDispatcherDefinition a_rule, ScenarioDetail a_sd)
    {
        foreach (string activeRule in a_rule.OptimizeSettings.GetAllRules())
        {
            decimal originalPoints = a_rule.OptimizeSettings.GetRulePoints(activeRule);
            int minRand = originalPoints > 0 ? 0 : -1000;
            int maxRand = originalPoints > 0 ? 1000 : 0;
            a_rule.OptimizeSettings.AddOrUpdateMapping(activeRule,
                new OptimizeRuleWeightSettings.OptimizeRuleElementSettings
                {
                    ResourceMultiplier = a_rule.OptimizeSettings.GetRuleResourceMultiplier(activeRule),
                    Settings = a_rule.OptimizeSettings.GetRuleSettings(activeRule),
                    Points = m_rand.Next(minRand, maxRand)
                });
        }

        //TODO: Why is this here on this function but not AdjustRuleValuesInUseRand()? Is this actually needed?
        //This will initialize a new dispatcher rule with the updated rules that are set. 
        //NOTE: If the recieve function is changed, it may be necessary to create an updateT instead of using null.
        //a_rule.Receive(null, a_sd.Scenario, a_sd, a_dataChanges);
    }
    #endregion

    //Used to reduce CPU usage below 1 active simulation
    public void AddOneTimeDelay(decimal a_percentage)
    {
        if (m_simDuration > TimeSpan.Zero)
        {
            m_simStartDelay = TimeSpan.FromTicks(Convert.ToInt64(m_simDuration.Ticks * a_percentage));
        }
    }
}