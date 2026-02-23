using PT.APSCommon;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of CapacityIntervals.
/// </summary>
[Serializable]
public class CapacityIntervalsCollection : ICopyTable
{
    #region Declarations
    private SortedList<BaseId, CapacityInterval> m_capacityIntervals = new ();

    public class CapacityIntervalsCollectionException : PTException
    {
        public CapacityIntervalsCollectionException(string message) : base(message) { }
    }
    #endregion

    #region Construction
    #endregion

    #region Properties and Methods
    public Type ElementType => typeof(CapacityInterval);

    internal CapacityInterval Add(CapacityInterval ci)
    {
        m_capacityIntervals.Add(ci.Id, ci);
        return ci;
    }

    internal void Remove(int index)
    {
        m_capacityIntervals.RemoveAt(index);
    }

    internal void Remove(CapacityInterval ci)
    {
        m_capacityIntervals.Remove(ci.Id);
    }

    public CapacityInterval this[int index] => m_capacityIntervals.Values[index];

    public object GetRow(int index)
    {
        return m_capacityIntervals.Values[index];
    }

    public int Count => m_capacityIntervals.Count;

    public CapacityInterval Find(BaseId a_id)
    {
        return m_capacityIntervals[a_id];
    }

    public bool Contains(BaseId a_id)
    {
        return m_capacityIntervals.ContainsKey(a_id);
    }

    /// <summary>
    /// Returns the first online (Online or Overtime or Potential Overtime) that contains the point in time specified.
    /// Returns null if none is found.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public CapacityInterval FindOnlineAtPointInTime(DateTime dt)
    {
        for (int i = 0; i < Count; i++)
        {
            CapacityInterval ci = this[i];
            if (dt.Ticks >= ci.StartDateTime.Ticks && dt.Ticks <= ci.EndDateTime.Ticks && ci.Active)
            {
                return ci;
            }
        }

        return null; //not found
    }

    /// <summary>
    /// Returns the first online start (Online or Overtime or Potential Overtime) that starts at or after the point in time specified.
    /// Returns PtDateTime.MAX_DATE_TIME if none is found.
    /// </summary>
    public DateTime FindOnlineAtOrAfterPointInTime(DateTime dt)
    {
        DateTime earliestSoFar = PTDateTime.MaxDateTime;

        for (int i = 0; i < Count; i++)
        {
            CapacityInterval ci = this[i];
            if (ci.StartDateTime.Ticks >= dt.Ticks && ci.StartDateTime.Ticks < earliestSoFar.Ticks && ci.Active)
            {
                earliestSoFar = ci.StartDateTime;
            }
        }

        return earliestSoFar;
    }

    /// <summary>
    /// Returns an Online Interval covering the point in time.  The first Regular Online is returned.  If there is no Regular Online
    /// then an Overtime or Potential Overtime may be returned.
    /// </summary>
    /// <param name="dt">The point in time that the Expansion should cover.</param>
    /// <param name="ci">The Capacity Interval related covering the point.  Else null.</param>
    /// <returns>True if ANY CI is found covering the point.  Else false.</returns>
    public bool FindOnlineIntervalCoveringPoint(DateTime dt, out CapacityInterval returnCI)
    {
        returnCI = null;

        for (int i = 0; i < Count; i++)
        {
            CapacityInterval ci = this[i];
            if (ci.StartDateTime.Ticks <= dt.Ticks && ci.EndDateTime.Ticks > dt.Ticks) //if two expansions are up against each other expansion n's end =n+1's start.  A block in n+1 starts at that instant and we want to consider it to start in n+1.
            {
                if (ci.Active)
                {
                    returnCI = ci;
                    return true;
                }
            }
        }

        return returnCI != null;
    }
    #endregion
}