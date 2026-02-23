namespace PT.Scheduler;

/// <summary>
/// Summary description for CondensedAdjustmentArray.
/// </summary>
public class CondensedAdjustmentArray
{
    internal CondensedAdjustmentArray() { }

    private readonly List<CondensedAdjustment> m_adjustments = new ();

    public CondensedAdjustment this[int a_index] => m_adjustments[a_index];

    internal void Add(CondensedAdjustment a_adjustment)
    {
        m_adjustments.Add(a_adjustment);
    }

    public int Count => m_adjustments.Count;

    public void SortByTime()
    {
        m_adjustments.Sort(CondensedAdjustmentCompareByTime);
    }

    private static int CondensedAdjustmentCompareByTime(CondensedAdjustment a_thisCondensedAdjustment, CondensedAdjustment a_otherCondensedAdjustment)
    {
        if (a_thisCondensedAdjustment == null)
        {
            if (a_otherCondensedAdjustment == null)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            if (a_otherCondensedAdjustment == null)
            {
                return 1;
            }
            else
            {
                if (a_thisCondensedAdjustment.Time > a_otherCondensedAdjustment.Time)
                {
                    return 1;
                }
                else if (a_thisCondensedAdjustment.Time < a_otherCondensedAdjustment.Time)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}