using PT.Scheduler.Simulation;

namespace PT.Scheduler;

/// <summary>
/// Abstract class used to derive all Resources.
/// </summary>
public abstract partial class BaseResource
{
    internal long m_v_simSort;

    private bool m_usedAsPrimaryResource;

    /// <summary>
    /// Whether the resource can be used by one of the jobs to be scheduled
    /// as a primary resource.
    /// </summary>
    internal bool UsedAsPrimaryResource
    {
        get => m_usedAsPrimaryResource;
        set => m_usedAsPrimaryResource = value;
    }

    internal virtual void ResetSimulationStateVariables(long a_clock)
    {
        m_primaryResAllowedHelperIdx = -1;
        m_helperResAllowedHelperIdx = -1;
        UsedAsPrimaryResource = false;
        m_sequentialResourceIdx = -1;
    }

    [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
    internal PQOptimizeNode pqOptimizeNode = new ();

    internal virtual void SimulationInitialization(long a_clock, ScenarioDetail a_sd, long a_planningHorizonEndTicks) { }

    // Delete if it's not used by the connections enhancement.
    //internal bool ConnectionIntersection(BaseResource br)
    //{
    //    Dictionary<BaseId, Connector>.Enumerator e = Connectors.GetEnumerator();
    //    while (e.MoveNext())
    //    {
    //        if (br.Connectors.ContainsKey(e.Current.Key))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    /// <summary>
    /// Resources in the system are assigned numbers from 0 to n (where n=the number of resource - 1).
    /// This number changes as resources are added, removed, etc.
    /// </summary>
    internal long m_productRuleResIdx = -1;

    /// <summary>
    /// Assigned at simulation time to identify the row the primary resource corresponds to in a BoolMatrix.
    /// </summary>
    internal int m_primaryResAllowedHelperIdx;

    /// <summary>
    /// Assigned at simulation time to identify the row the helper resource corresponds to in a BoolMatrix.
    /// </summary>
    internal int m_helperResAllowedHelperIdx;

    internal bool IsAllowedHelperResource(BaseResource a_helperRes)
    {
        return Plant.AllowedHelperManager.IsAllowedHelperResourceSimVersion(this, a_helperRes);
    }

    /// <summary>
    /// The index into the available resource set.
    /// </summary>
    internal int m_sequentialResourceIdx;
}