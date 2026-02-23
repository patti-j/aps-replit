using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Scheduler.CoPilot;
using PT.Scheduler.CoPilot.PruneScenario;
using PT.Scheduler.CoPilot.Pruning.Modules;
using PT.Scheduler.CoPilot.Pruning.Validators;
using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Work in progress.
/// </summary>
internal class PruneScenarioManager
{
    #region MEMBERS
    //The current status of simulations.
    private CoPilotSimulationStatus m_status;

    public CoPilotSimulationStatus Status => m_status;

    private Scenario m_originalCopyScenario;
    private Scenario m_currentWorkingScenario;
    private readonly List<ScenarioBaseT> m_transmissionToPrune = new ();
    private readonly object m_simulationsLock = new ();
    private CoPilotSimulationUtilities m_simUtilities;
    private readonly List<IPruneScenarioModule> m_storedModules = new ();
    private ValidatorManager m_validationManager;
    private byte[] m_sdByteArray;
    private byte[] m_ssByteArray;
    #endregion

    /// <summary>
    /// Begin running simulations.
    /// This is the entry point to initiate all simulations.
    /// Throws an exception if the simulations cannot be started.
    /// </summary>
    /// <param name="a_scenario">Scenario to use</param>
    /// <param name="a_settings">Current RuleSeekSettings</param>
    /// <param name="a_packageManager">Loads the customizations when scenario is copied</param>
    /// <returns></returns>
    internal void BeginPruneScenarioSimulation(Scenario a_scenario, PruneScenarioT a_t, IPackageManager a_packageManager)
    {
        try
        {
            m_simUtilities = new CoPilotSimulationUtilities(a_packageManager);

            if (Status != CoPilotSimulationStatus.STOPPED && Status != CoPilotSimulationStatus.COMPLETED)
            {
                throw new PTValidationException("2868", new object[] { (int)Status });
            }

            m_status = CoPilotSimulationStatus.INITIALIZING;
            m_originalCopyScenario = a_scenario;

            //Load the transmimssion files
            List<byte[]> transmissionsList = a_t.PTTransmissionBytesList;
            foreach (byte[] bytes in transmissionsList)
            {
                using (BinaryMemoryReader reader = new (bytes))
                {
                    object transmission = PTSystem.TrnClassFactory.Deserialize(reader);
                    if (transmission is ScenarioBaseT)
                    {
                        m_transmissionToPrune.Add((ScenarioBaseT)transmission);
                    }
                    //skipped transmission
                }
            }

            if (m_transmissionToPrune.Count == 0)
            {
                //Add a default time adjustment for basic load testing
                m_transmissionToPrune.Add(new ScenarioTouchT());
            }

            //Keep last transmission seperate to replay during simulation
            int idx = m_transmissionToPrune.Count - 1;

            if (!(m_transmissionToPrune[idx] is ScenarioBaseT))
            {
                throw new PTException(string.Format("{0} {1}", "Transmission is not a ScenarioBaseT and cannot be used to prune a scenario:".Localize(), m_transmissionToPrune[idx].GetType()));
            }

            ScenarioBaseT lastTransmission = m_transmissionToPrune[idx];
            m_transmissionToPrune.RemoveAt(idx);

            //Copy the scenario so it can be modified without affecting the original.
            lock (m_simulationsLock)
            {
                m_simUtilities.CopyAndStoreScenario(m_originalCopyScenario, out m_sdByteArray, out m_ssByteArray); //Create a new simulation scenario
                m_currentWorkingScenario = m_simUtilities.CopySimScenario(m_sdByteArray, m_ssByteArray);
            }

            //Play through all the extra recordings to get a scenario right before the problem transmission
            foreach (ScenarioBaseT ptTransmission in m_transmissionToPrune)
            {
                try
                {
                    m_currentWorkingScenario.Receive(ptTransmission);
                }
                catch (PTHandleableException) { }
            }

            //Store the updated scenario
            m_simUtilities.CopyAndStoreScenario(m_currentWorkingScenario, out m_sdByteArray, out m_ssByteArray);

            //Initialize the validation modules
            m_validationManager = new ValidatorManager(m_sdByteArray, m_ssByteArray, lastTransmission, a_packageManager);

            m_status = CoPilotSimulationStatus.RUNNING;

            // Run static pruning modules
            RunModules(GetBasicModules());

            // run recursive modules
            RunModules(GetRecursiveModules());

            // run generated modules
            List<IPruneScenarioModule> newModules = new CoPilot.Pruning.ModuleGenerators.GenerateDeleteManufacturingOrderModules().Generate(m_currentWorkingScenario);
            RunModules(newModules);

            //Attempt to delete unused resources after jobs have been deleted
            newModules.Clear();
            newModules.Add(new DeleteAllUnusedResourcesModule());
            RunModules(newModules);

            // delete alternate paths
            newModules = new CoPilot.Pruning.ModuleGenerators.GenerateDeleteAlternatePathModules().Generate(m_currentWorkingScenario);
            RunModules(newModules);

            // mark Activities as finished.
            newModules = new CoPilot.Pruning.ModuleGenerators.GenerateFinishActivityModules().Generate(m_currentWorkingScenario);
            RunModules(newModules);

            // try re-running the modules that previously failed.
            newModules = new List<IPruneScenarioModule>(m_storedModules.ToArray());
            m_storedModules.Clear(); //failed modules that fail again will be added to the list.
            RunModules(newModules);

            //generatedModules = new CoPilot.Pruning.ModuleGenerators.GenerateDeleteHelperResourceRequirementModules().Generate(m_currentWorkingScenario);
            //RunModules(generatedModules);

            //Simulations complete
            SendCurrentScenario();
        }
        catch (PTValidationException ve)
        {
            throw;
        }
        catch (Exception e)
        {
            m_status = CoPilotSimulationStatus.ERROR;
            throw;
        }
    }

    /// <summary>
    /// Adds basic pruning modules
    /// </summary>
    private List<IPruneScenarioModule> GetBasicModules()
    {
        List<IPruneScenarioModule> basicModules = new ();

        basicModules.Add(new DeleteAllUnusedResourcesModule());
        basicModules.Add(new DeleteAllCellsModule());
        basicModules.Add(new DeleteAllAllowedHelpersModule());
        basicModules.Add(new DeleteAllResourceConnectorsModule());
        basicModules.Add(new DeleteAllSalesOrdersModule());
        basicModules.Add(new DeleteAllForecastsModule());
        basicModules.Add(new DeleteAllTransferOrdersModule());
        basicModules.Add(new DeleteAllSetupTablesModule());
        basicModules.Add(new DeleteAllProductRulesModule());
        basicModules.Add(new DeleteAllPurchaseOrdersModule());

        basicModules.Add(new DeleteAllOpAttributesModule());
        basicModules.Add(new DeleteAllOpMaterialRequirementsModule());
        basicModules.Add(new DeleteAllOpProductsModule());
        basicModules.Add(new DeleteAllOpHelperResourceRequirementsModule());

        return basicModules;
    }

    /// <summary>
    /// Adds recursive pruning modules
    /// </summary>
    private List<IPruneScenarioModule> GetRecursiveModules()
    {
        List<IPruneScenarioModule> recursiveModules = new ();

        recursiveModules.Add(new DeleteJobModule());

        return recursiveModules;
    }

    //void RunModuleTest(List<IPruneScenarioModule> a_modules)
    //{
    //    while (a_modules.Count > 0)
    //    {
    //        IPruneScenarioModule mod = a_modules[0];
    //        a_modules.Remove(mod);
    //        mod.Prune(m_currentWorkingScenario);
    //    }
    //}

    /// <summary>
    /// Function to call in a seperate thread so that if it times out, it can be aborted
    /// </summary>
    /// <returns></returns>
    private static bool PruneScenario(IPruneScenarioModule a_module, Scenario a_scenario)
    {
        try
        {
            ScenarioDataChanges dataChanges = new ();
            return a_module.Prune(a_scenario, dataChanges);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Runs all provided modules sequentially. Will handle the different module interfaces
    /// </summary>
    /// <param name="a_modules"></param>
    private void RunModules(List<IPruneScenarioModule> a_modules)
    {
        foreach (IPruneScenarioModule module in a_modules)
        {
            if (module is IPruneScenarioRecursiveModule)
            {
                //Initialize the module first with a copy of the working scenario
                Scenario testScenario = m_simUtilities.CopySimScenario(m_sdByteArray, m_ssByteArray);
                IPruneScenarioRecursiveModule recursiveModule = (IPruneScenarioRecursiveModule)module;
                if (!recursiveModule.InitialConfiguration(testScenario))
                {
                    //Initial setup determined there is nothing to do
                    testScenario.Dispose();
                    continue;
                }

                testScenario.Dispose();

                //Continually run and configure the module until the Reconfigure function determines there is nothing left to do
                do
                {
                    RunModule(recursiveModule);
                    testScenario = m_simUtilities.CopySimScenario(m_sdByteArray, m_ssByteArray);
                } while (recursiveModule.Reconfigure(testScenario));
            }
            else
            {
                //Simple type, just run once
                RunModule(module);
            }
        }
    }

    /// <summary>
    /// Hanldes running the pruning module, validating it, and updating the working scenario if needed.
    /// Modules that fail to validate successfully are stored for potential reruns later.
    /// </summary>
    private void RunModule(IPruneScenarioModule a_module)
    {
        lock (m_simulationsLock)
        {
            //Process Modules
            Scenario testScenario = m_simUtilities.CopySimScenario(m_sdByteArray, m_ssByteArray);

            //Create a new token so the prune attempt can be cancelled in case of a timeout
            CancellationTokenSource tokenSource = new ();
            PTSystem.CancelToken = tokenSource.Token;
            try
            {
                Task<bool> task = Task.Run(() => PruneScenario(a_module, testScenario), tokenSource.Token);

                if (!task.Wait(TimeSpan.FromMinutes(3)))
                {
                    try
                    {
                        tokenSource.Cancel();
                        task.Wait();
                    }
                    catch (Exception) { }

                    return;
                }

                //Task did not prune
                if (!task.Result)
                {
                    return;
                }
            }
            catch (Exception)
            {
                //Error occurred during pruning. This module should not be validated.
                return;
            }

            //Validate pruned scenario
            try
            {
                ValidatorManager.EValidationResult result = m_validationManager.Validate(testScenario);
                if (result != ValidatorManager.EValidationResult.ErrorValidated)
                {
                    //Bad, either no error or a different error
                    m_storedModules.Add(a_module);
                    testScenario.Dispose();
                }
                else
                {
                    //Good, error still exists. Update the working scenario
                    testScenario.Dispose();
                    ScenarioDataChanges dataChanges = new ();
                    a_module.Prune(m_currentWorkingScenario, dataChanges);
                    try
                    {
                        m_simUtilities.CopyAndStoreScenario(m_currentWorkingScenario, out m_sdByteArray, out m_ssByteArray);
                    }
                    catch (Exception)
                    {
                        //Error with serialization. This is a result of the prune, but wasn't caught by any validation modules
                    }
                }
            }
            catch (PTException)
            {
                throw new PTException("Unknown error when validating prune result".Localize());
            }
        }
    }

    /// <summary>
    /// Sends the current working scenario as a result to ScenarioManager.
    /// This can be called to send the current result without ending the simulation.
    /// </summary>
    private void SendCurrentScenario()
    {
        try
        {
            ScenarioBaseT scenarioBaseT = m_validationManager.GetValidationTransmission();
            string fileSuffix = "prunedRecording_" + scenarioBaseT.GetType().FullName;
            string path = Path.Combine(PTSystem.WorkingDirectory.Pruned, PTSystem.WorkingDirectory.GetRecordingFileName(Convert.ToInt64(1), fileSuffix));
            DirectoryInfo dirInfo = new (Path.GetDirectoryName(path));
            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                fileInfo.Delete();
            }

            if (scenarioBaseT is ScenarioIdBaseT idBaseT)
            {
                idBaseT.Destination = ScenarioIdBaseT.EDestinations.ToLiveScenario;
            }

            TransmissionRecording tr = new (scenarioBaseT);
            using (BinaryFileWriter writer = new (path, Common.Compression.ECompressionType.Fast))
            {
                tr.Serialize(writer);
            }
        }
        catch (Exception e)
        {
            throw new PTException("Unable to save the recording for use with the pruned scenario".Localize(), e);
        }

        if (NewPrunedScenarioEvent != null)
        {
            NewPrunedScenarioEvent(this, new NewPrunedScenarioEventArgs(m_currentWorkingScenario, "Pruned"));
        }
    }

    #region EVENTS
    internal delegate void NewPrunedScenarioEventHandler(object sender, NewPrunedScenarioEventArgs a_e);

    internal event NewPrunedScenarioEventHandler NewPrunedScenarioEvent;

    /// <summary>
    /// This event args class holds the transmissions that are used by ScenarioManager to create a new scenario.
    /// </summary>
    internal class NewPrunedScenarioEventArgs : EventArgs
    {
        internal NewPrunedScenarioEventArgs(Scenario a_scenarioToSend, string a_scenarioName)
        {
            m_scenarioToSend = a_scenarioToSend;
            m_scenarioName = a_scenarioName;
        }

        private readonly Scenario m_scenarioToSend;

        internal Scenario NewPrunedScenario => m_scenarioToSend;

        private readonly string m_scenarioName;

        internal string NewScenarioName => m_scenarioName;
    }
    #endregion
}