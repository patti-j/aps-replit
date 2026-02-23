namespace PT.Scheduler.CoPilot.PruneScenario;

internal interface IPruneScenarioRecursiveModule : IPruneScenarioModule
{
    /// <summary>
    /// Signals that the last attempt to prune was not validated successfuly. Reconfigure the module to prune differently.
    /// The manager will continue to validate and reconfigure the module until the Prune function returns false.
    /// </summary>
    bool Reconfigure(Scenario a_scenario);

    /// <summary>
    /// Sets up the module for pruning. This function will be called only once, before any other on this module.
    /// </summary>
    bool InitialConfiguration(Scenario a_scenario);
}