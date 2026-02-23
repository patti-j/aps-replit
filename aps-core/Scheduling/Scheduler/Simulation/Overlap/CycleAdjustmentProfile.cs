using System.Collections;

namespace PT.Scheduler;

/// <summary>
/// Summary description for CycleCompletionProfile.
/// </summary>
public class CycleAdjustmentProfile
{
    private readonly ArrayList m_cycleCompletions = new ();

    internal void Add(CycleAdjustment a_completion)
    {
        m_cycleCompletions.Add(a_completion);
    }

    public int Count => m_cycleCompletions.Count;

    public CycleAdjustment this[int a_index] => (CycleAdjustment)m_cycleCompletions[a_index];

    /// <summary>
    /// Calculates the sum of all the cycle completions. If you need this more than once, cache the value since it's completion time is based on the number of elements in the profile. O(n).
    /// </summary>
    internal decimal CalcTotalQty()
    {
        decimal qty = 0;

        for (int i = 0; i < Count; ++i)
        {
            qty += this[i].Qty;
        }

        return qty;
    }

    public override string ToString()
    {
        return string.Format("CycleCompletionProfile: Qty={0}; Count={1};", CalcTotalQty(), Count);
    }

    internal void RemoveAt(int a_ccpCount)
    {
        m_cycleCompletions.RemoveAt(a_ccpCount);
    }
}