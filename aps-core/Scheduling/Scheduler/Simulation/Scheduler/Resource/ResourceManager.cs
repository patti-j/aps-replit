using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Machine objects.
/// </summary>
public partial class ResourceManager
{
    internal void ResetSimulationStateVariables(long a_clock, ScenarioOptions a_so)
    {
        for (int i = 0; i < Count; ++i)
        {
            Resource m = this[i];
            m.ResetSimulationStateVariables(a_clock, a_so);
        }
    }

    #region Diagnostics
    internal void PrintResultantCapacity()
    {
        for (int i = 0; i < Count; ++i)
        {
            Resource r = this[i];
            string fileName = string.Format("e:\\_APS_DIAGNOSTICS\\{0}.txt", r.Name);
            r.PrintResultantCapacity(fileName);
        }
    }
    #endregion
}