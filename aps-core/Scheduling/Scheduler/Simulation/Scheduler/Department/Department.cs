using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Shows which ERP Work Center the Resource belongs too.   Can be used for creating GanttViews or in scheduling algorithms.
/// </summary>
public partial class Department
{
    #region Simulation
    internal void ResetSimulationStateVariables(long a_clock, ScenarioOptions a_so)
    {
        m_resources.ResetSimulationStateVariables(a_clock, a_so);
    }

    /// <summary>
    /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// It will set FrozenSpan and UseDeptFrozenSpan fields if they've been set by the customization.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal void PostSimStageCustExecute(ScenarioDetail.SimulationType a_simType, Transmissions.ScenarioBaseT a_t, ScenarioDetail a_sd, int a_currentSimStageIdx, int a_lastSimStageIdx)
    {
        ChangableDeptValues changable = null;
        a_sd.ExtensionController.PostSimStageChangeDept(a_simType, a_t, a_sd, this, a_currentSimStageIdx, a_lastSimStageIdx, out changable);
        if (changable != null)
        {
            if (changable.FrozenSpanSet)
            {
                FrozenSpanTicks = changable.FrozenSpanTicks;
            }
        }
    }

    /// <summary>
    /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// It will set FrozenSpan and UseDeptFrozenSpan fields if they've been set by the customization.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal void EndOfSimulationCustExecute(ScenarioDetail.SimulationType a_simType, Transmissions.ScenarioBaseT a_t, ScenarioDetail a_sd)
    {
        ChangableDeptValues changable = null;
        a_sd.ExtensionController.EndOfSimulationChangeDept(a_simType, a_t, a_sd, this, out changable);
        if (changable != null)
        {
            if (changable.FrozenSpanSet)
            {
                FrozenSpanTicks = changable.FrozenSpanTicks;
            }
        }
    }
    #endregion

    #region outside simulation setup
    internal void AddResources(List<InternalResource> a_resources)
    {
        for (int resI = 0; resI < Resources.Count; ++resI)
        {
            a_resources.Add(Resources[resI]);
        }
    }
    #endregion

    #region Debug
    public override string ToString()
    {
        return string.Format("Dept={0}", ExternalId);
    }

    internal void PrintResultantCapacity()
    {
        Resources.PrintResultantCapacity();
    }
    #endregion
}