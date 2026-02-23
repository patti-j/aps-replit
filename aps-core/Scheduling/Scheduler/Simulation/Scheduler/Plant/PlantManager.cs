using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Plant objects.
/// </summary>
public partial class PlantManager
{
    #region Simulation
    internal void ResetSimulationStateVariables(long a_clock, ScenarioOptions a_so)
    {
        for (int plantI = 0; plantI < Count; plantI++)
        {
            this[plantI].ResetSimulationStateVariables(a_clock, a_so);
        }
    }

    internal bool ContainsMultipleStages()
    {
        int firstResourceStage = int.MaxValue;
        for (int plantI = 0; plantI < Count; ++plantI)
        {
            Plant p = this[plantI];
            for (int departmentI = 0; departmentI < p.Departments.Count; ++departmentI)
            {
                Department d = p.Departments[departmentI];
                for (int resourceI = 0; resourceI < d.Resources.Count; ++resourceI)
                {
                    Resource r = d.Resources[resourceI];
                    if (firstResourceStage == int.MaxValue)
                    {
                        firstResourceStage = r.Stage;
                    }

                    if (r.Stage != firstResourceStage)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Return the complete set of resource in all plants and departments.
    /// </summary>
    /// <returns></returns>
    internal List<Resource> GetResourceArrayList()
    {
        List<Resource> ral = new ();

        for (int plantI = 0; plantI < Count; ++plantI)
        {
            Plant p = this[plantI];
            for (int deptI = 0; deptI < p.Departments.Count; ++deptI)
            {
                Department d = p.Departments[deptI];
                for (int resI = 0; resI < d.Resources.Count; ++resI)
                {
                    Resource r = d.Resources[resI];
                    ral.Add(r);
                }
            }
        }

        return ral;
    }

    /// <summary>
    /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal void PostSimStageCust(ScenarioDetail.SimulationType a_simType, Transmissions.ScenarioBaseT a_t, ScenarioDetail a_sd, int a_curSimStageIdx, int a_finalSimStageIdx)
    {
        for (int pI = 0; pI < Count; ++pI)
        {
            this[pI].PostSimStageCustExecute(a_simType, a_t, a_sd, a_curSimStageIdx, a_finalSimStageIdx);
        }
    }

    /// <summary>
    /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal void EndOfSimulationCustExecute(ScenarioDetail.SimulationType a_simType, Transmissions.ScenarioBaseT a_t, ScenarioDetail a_sd)
    {
        for (int pI = 0; pI < Count; ++pI)
        {
            this[pI].EndofSimulationCustExecute(a_simType, a_t, a_sd);
        }
    }
    #endregion

    #region Diagnostics
    internal void PrintResultantCapacity()
    {
        for (int i = 0; i < Count; ++i)
        {
            Plant p = this[i];
            p.PrintResultantCapacity();
        }
    }
    #endregion

    public long GetEarliestFrozenSpanEnd(long a_clock)
    {
        long earliestFrozenSpanEnd = long.MaxValue;

        foreach (Plant plant in this)
        {
            earliestFrozenSpanEnd = Math.Min(earliestFrozenSpanEnd, plant.GetEarliestFrozenSpanEnd(a_clock));
        }

        if (earliestFrozenSpanEnd == long.MaxValue) //No department frozen span was set
        {
            return a_clock;
        }

        return earliestFrozenSpanEnd;
    }

    public long GetEarliestStableSpanEnd(long a_clock)
    {
        long earliestFrozenSpanEnd = long.MaxValue;

        foreach (Plant plant in this)
        {
            earliestFrozenSpanEnd = Math.Min(earliestFrozenSpanEnd, plant.GetEarliestFrozenSpanEnd(a_clock)) + plant.StableSpanTicks;
        }

        if (earliestFrozenSpanEnd == long.MaxValue) //No department frozen span was set
        {
            return a_clock;
        }

        return earliestFrozenSpanEnd;
    }
}