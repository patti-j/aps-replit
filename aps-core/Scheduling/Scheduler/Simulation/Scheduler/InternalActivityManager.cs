namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of InternalActivity objects.
/// </summary>
public partial class InternalActivityManager : IPTSerializable
{
    /// <summary>
    /// Adjust the required finish quantity of every activity by a ratio. For instance an activity quantity of 10 and ratio of 3/2 would result in 15.
    /// </summary>
    /// <param name="ratio"></param>
    internal void AdjustRequiredQty(decimal a_ratio, decimal a_newRequiredMOQty)
    {
        for (int actI = 0; actI < Activities.Count; ++actI)
        {
            InternalActivity act = (InternalActivity)Activities.GetByIndex(actI);
            act.AdjustRequiredQty(a_ratio, a_newRequiredMOQty);
        }
    }

    /// <summary>
    /// Whether any activity has any of its blocks scheduled within its frozan span.
    /// </summary>
    /// <param name="a_spanCalc">Used to calculate the resource frozen span.</param>
    /// <returns></returns>
    internal bool AnyActivityInSpan(long a_clockDate, OptimizeSettings.ETimePoints a_spanType)
    {
        for (int i = 0; i < Count; ++i)
        {
            InternalActivity ia = GetByIndex(i);
            if (!ia.Finished && ia.Scheduled)
            {
                if (ia.Batch.AnyBlocksInSpan(a_clockDate, a_spanType))
                {
                    return true;
                }
            }
        }

        return false;
    }
}