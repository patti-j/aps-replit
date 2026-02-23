using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.PruneScenario;

internal interface IPruneScenarioModule
{
    /// <summary>
    /// Returns whether any change was made. If false, the manager may remove this module and continue without running a simulation.
    /// Return true if changes cannot be verified so a simulation is run.
    /// </summary>
    bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges);
}