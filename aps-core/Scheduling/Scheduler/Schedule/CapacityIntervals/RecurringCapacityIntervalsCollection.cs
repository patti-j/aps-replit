using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.Range;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

#region RecurringCapacityIntervalsCollection
/// <summary>
/// Stores a list of RecurringCapacityIntervals.
/// </summary>
public class RecurringCapacityIntervalsCollection : ICopyTable
{
    #region Declarations
    private readonly SortedList<BaseId, RecurringCapacityInterval> m_capacityIntervals = new ();

    public class RecurringCapacityIntervalsCollectionException : PTException
    {
        public RecurringCapacityIntervalsCollectionException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    #endregion

    #region Properties and Methods
    public Type ElementType => typeof(RecurringCapacityInterval);

    internal RecurringCapacityInterval Add(RecurringCapacityInterval ci)
    {
        m_capacityIntervals.Add(ci.Id, ci);
        return ci;
    }

    internal void Remove(int index)
    {
        m_capacityIntervals.RemoveAt(index);
    }

    internal void Remove(RecurringCapacityInterval ci)
    {
        m_capacityIntervals.Remove(ci.Id);
    }

    public RecurringCapacityInterval this[int index] => m_capacityIntervals.Values[index];

    public object GetRow(int index)
    {
        return m_capacityIntervals.Values[index];
    }

    public BaseId GetKeyByIndex(int index)
    {
        return m_capacityIntervals.Keys[index];
    }

    public RecurringCapacityInterval GetById(BaseId a_rciId)
    {
        if (m_capacityIntervals.ContainsKey(a_rciId))
        {
            return m_capacityIntervals[a_rciId];
        }

        return null;
    }

    public int Count => m_capacityIntervals.Count;

    /// <summary>
    /// Check if a recurring capacity interval exists
    /// </summary>
    /// <param name="a_id"></param>
    /// <returns></returns>
    public bool Contains(BaseId a_id)
    {
        return m_capacityIntervals.ContainsKey(a_id);
    }

    public RecurringCapacityInterval FindActiveIntervalAtPointInTime(DateTime a_dt)
    {
        for (int i = 0; i < Count; i++)
        {
            RecurringCapacityInterval rci = this[i];

            if (GenericRangeSearch.FindRange(rci, a_dt) is RecurringCapacityInterval.RCIExpansion && rci.Active)
            {
                return rci;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the first online time (Online or Overtime or Potential Overtime) that starts after the point in time specified.
    /// Returns DateTime.MinValue if none is found.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public DateTime FindFirstOnlineAtOrAfterPointInTime(DateTime dt)
    {
        DateTime earliestSoFar = PTDateTime.MaxDateTime;
        for (int i = 0; i < Count; i++)
        {
            RecurringCapacityInterval rci = this[i];
            if (rci.ExpansionsCount > 0 && rci.GetExpansionAtIdx(rci.ExpansionsCount - 1).Start.Ticks >= dt.Ticks && rci.Active)
            {
                //RCI is worth scanning for a match
                RecurringCapacityInterval.RCIExpansion rciExpansion = rci.FindExpansionAtOrAfterPoint(dt);
                if (rciExpansion != null && rciExpansion.Start.Ticks < earliestSoFar.Ticks)
                {
                    earliestSoFar = rciExpansion.Start;
                }
            }
        }

        return earliestSoFar;
    }

    /// <summary>
    /// Returns an Online Expansion covering the point in time.  The first Regular Online is returned.  If there is no Regular Online
    /// then an Overtime or Potential Overtime may be returned.
    /// </summary>
    /// <param name="point">The point in time that the Expansion should cover.</param>
    /// <param name="rci">The RCI related to the Expansion if found.  Else null.</param>
    /// <param name="expansion">The expansion covering the point if found.  Else null.</param>
    /// <returns>True if ANY Expansion is found covering the point.  Else false.</returns>
    public bool FindExpansionCoveringPoint(DateTime dt, out RecurringCapacityInterval returnRCI, out RecurringCapacityInterval.RCIExpansion returnExpansion)
    {
        returnExpansion = null;
        returnRCI = null;
        for (int i = 0; i < Count; i++)
        {
            RecurringCapacityInterval rci = this[i];
            if (rci.ExpansionsCount > 0 && rci.GetExpansionAtIdx(0).Start.Ticks <= dt.Ticks && rci.GetExpansionAtIdx(rci.ExpansionsCount - 1).End.Ticks > dt.Ticks && rci.Active)
            {
                //RCI is worth scanning for a match
                RecurringCapacityInterval.RCIExpansion rciExpansion = rci.FindExpansionCoveringPoint(dt);
                if (rciExpansion != null)
                {
                    if (rci.Active)
                    {
                        returnExpansion = rciExpansion;
                        returnRCI = rci;
                        return true;
                    }
                }
            }
        }

        return returnExpansion != null;
    }
    #endregion
}
#endregion