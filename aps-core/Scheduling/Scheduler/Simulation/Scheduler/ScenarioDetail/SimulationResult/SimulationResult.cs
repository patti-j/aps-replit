namespace PT.Scheduler;

/// <summary>
/// Base class for the results of a simulation; in particular errors and problems.
/// </summary>
public abstract class SimulationResult
{
    /// <summary>
    /// Whether the simulation couldn't be performed.
    /// </summary>
    public abstract bool Failed { get; }
}