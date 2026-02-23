namespace PT.Scheduler.Simulation;

public partial class MRSupply
{
    private readonly AdjustmentArray m_supplyAdjustments;

    internal long CalcLatestSourceValue()
    {
        long max = 0;
        foreach (Adjustment adjustment in m_supplyAdjustments)
        {
            max = Math.Max(max, adjustment.AdjDate.Ticks);
        }

        return max;
    }

    internal void ResetSimulationStateVariables()
    {
        m_supplyAdjustments.Clear();
    }

    internal void AddAdjustment(Adjustment a_adjustment)
    {
        m_supplyAdjustments.Add(a_adjustment);
    }

    //Re-link inventory adjustments that are not linked by storage
    internal void RestoreReferences()
    {
        foreach (Adjustment supplyAdjustment in m_supplyAdjustments)
        {
            if (supplyAdjustment.Storage == null)
            {
                supplyAdjustment.Inventory.Adjustments.Add(supplyAdjustment);
            }
        }
    }

    internal void SortAdjustments()
    {
        m_supplyAdjustments.Sort();
    }
}