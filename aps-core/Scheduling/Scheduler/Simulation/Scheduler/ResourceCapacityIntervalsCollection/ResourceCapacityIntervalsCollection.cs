namespace PT.Scheduler;

/// <summary>
/// Stores a list of ResourceCapacityIntervals.
/// </summary>
public partial class ResourceCapacityIntervalsCollection
{
    internal void SimulationInitialization(Resource a_resource)
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].SimulationInitialization(a_resource);
        }
    }
}