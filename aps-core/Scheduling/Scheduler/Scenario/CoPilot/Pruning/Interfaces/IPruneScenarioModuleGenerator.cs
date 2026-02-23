namespace PT.Scheduler.CoPilot.PruneScenario;

internal interface IPruneScenarioModuleGenerator
{
    /// <summary>
    /// Returns a list of modules to use in the prune scenario simulations.
    /// </summary>
    List<IPruneScenarioModule> Generate(Scenario a_scenario);
}