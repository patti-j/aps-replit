using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of ResourceCapacityIntervals.
/// </summary>
public partial class ResourceCapacityIntervalsCollection : IPTSerializable
{
    public const int UNIQUE_ID = 386;

    #region IPTSerializable Members
    public ResourceCapacityIntervalsCollection(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ResourceCapacityInterval rci = new (reader);
                Add(rci);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    private readonly SortedList<DateIntKey, ResourceCapacityInterval> m_capacityIntervals = new ();

    public class ResourceCapacityIntervalsCollectionException : PTException
    {
        public ResourceCapacityIntervalsCollectionException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    public ResourceCapacityIntervalsCollection() { }

    internal ResourceCapacityIntervalsCollection(ResourceCapacityIntervalsCollection original)
    {
        for (int originalI = 0; originalI < original.Count; ++originalI)
        {
            Add(new ResourceCapacityInterval(original[originalI]));
        }
    }
    #endregion

    #region Properties and Methods
    public Type ElementType => typeof(ResourceCapacityInterval);

    internal ResourceCapacityInterval Add(ResourceCapacityInterval ci)
    {
        m_capacityIntervals.Add(new DateIntKey(ci.StartDateTime, m_capacityIntervals.Count), ci);
        return ci;
    }

    internal void Remove(int index)
    {
        m_capacityIntervals.RemoveAt(index);
    }

    public ResourceCapacityInterval this[int index] => m_capacityIntervals.Values[index];

    public ResourceCapacityInterval GetByIndex(int index)
    {
        return m_capacityIntervals.Values[index];
    }

    public int Count => m_capacityIntervals.Count;

    public void Clear()
    {
        m_capacityIntervals.Clear();
    }
    #endregion

    /// <summary>
    /// Iterates through Capacity Intervals to find the first one containing the date (inclusive) of a type specified
    /// Returns null if not found.
    /// </summary>
    /// <param name="aDate"></param>
    /// <returns></returns>
    public ResourceCapacityInterval FindFirstStartingAtOrAfter(DateTime aDate, bool a_online)
    {
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval ci = this[i];
            if (ci.StartDate >= aDate.Ticks)
            {
                if (a_online == ci.Active)
                {
                    return ci;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Iterates through Capacity Intervals to find the first one containing the date (inclusive) that is Online, Overtime, or Potential Overtime.
    /// Returns null if not found.
    /// </summary>
    public ResourceCapacityInterval FindFirstOnlineOrOvertimeIntervalContainingDate(DateTime a_date)
    {
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval ci = this[i];
            if (ci.Active && ci.StartDate <= a_date.Ticks && ci.EndDate >= a_date.Ticks)
            {
                return ci;
            }
        }

        return null;
    }
    /// <summary>
    /// Iterates through Capacity Intervals to find the ones which start between the specified date range that is active.
    /// Returns empty list if not found.
    /// </summary>
    public List<ResourceCapacityInterval> FindAllActiveIntervalWithinDateRange(DateTime a_startDate, DateTime a_endDate)
    {
        List<ResourceCapacityInterval> intervals = new();
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval ci = this[i];
            if (ci.Active && ci.StartDate >= a_startDate.Ticks && ci.StartDate <= a_endDate.Ticks && 
                (ci.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Online 
                || ci.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Occupied 
                || ci.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.ReservedOnline)
                )
            {
                intervals.Add(ci);
            }

            //Further intervals are past the date
            if (ci.StartDate > a_endDate.Ticks)
            {
                break;
            }
        }

        return intervals;
    }
    /// <summary>
    /// Iterates through Capacity Intervals to find the all containing the date (inclusive) that is Online, Overtime, or Potential Overtime.
    /// Returns empty list if not found.
    /// </summary>
    public List<ResourceCapacityInterval> FindAllActiveIntervalContainingDate(DateTime a_date)
    {
        List<ResourceCapacityInterval> intervals = new ();
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval ci = this[i];
            if (ci.Active && ci.StartDate <= a_date.Ticks && ci.EndDate >= a_date.Ticks)
            {
                intervals.Add(ci);
            }

            //Further intervals are past the date
            if (ci.StartDate > a_date.Ticks)
            {
                break;
            }
        }

        return intervals;
    }

    /// <summary>
    /// Iterates through Capacity Intervals to find the all containing the date (inclusive) that are not active
    /// Returns null if not found.
    /// </summary>
    public List<ResourceCapacityInterval> FindAllOfflineIntervalContainingDate(DateTime a_date)
    {
        List<ResourceCapacityInterval> intervals = new ();
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval ci = this[i];
            if (!ci.Active  
                && ci.StartDate <= a_date.Ticks 
                && ci.EndDate >= a_date.Ticks)
            {
                intervals.Add(ci);
            }

            //Further intervals are past the date
            if (ci.StartDate > a_date.Ticks)
            {
                break;
            }
        }

        return intervals;
    }

    /// <summary>
    /// Iterates through Capacity Intervals to find the first one containing the date (inclusive) that is Cleanout.
    /// Returns null if not found.
    /// </summary>
    /// <param name="a_date"></param>
    /// <returns></returns>
    public ResourceCapacityInterval FindFirstOnlineIntervalNotUsedForRunAfterDate(DateTime a_date)
    {
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval ci = this[i];
            if (!ci.UsedForRun
                && ci.Active
                && ci.StartDate > a_date.Ticks)
            {
                return ci;
            }
        }

        return null;
    }

    /// <summary>
    /// Iterates through Capacity Intervals to find the first one containing the date (inclusive) that is of the specified types.
    /// Returns null if not found.
    /// </summary>
    /// TODO: This function has no references. Should it be deleted?
    public ResourceCapacityInterval FindFirstIntervalAfter(DateTime a_date, params CapacityIntervalDefs.capacityIntervalTypes[] a_capacityTypes)
    {
        HashSet<CapacityIntervalDefs.capacityIntervalTypes> quickLookup = new (a_capacityTypes);
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval ci = this[i];
            if (ci.StartDate > a_date.Ticks && quickLookup.Contains(ci.IntervalType))
            {
                return ci;
            }
        }

        return null;
    }

    /// <summary>
    /// Iterates through Capacity Intervals to find the first one containing the date (inclusive) that is Offline.
    /// Returns null if not found.
    /// </summary>
    /// <param name="a_date"></param>
    /// <returns></returns>
    public ResourceCapacityInterval FindFirstOfflineAfter(DateTime a_date)
    {
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval ci = this[i];
            // Checking for not Active here would also allow Occupied to pass through.
            // We are tentatively saying that this is okay
            if (!ci.Active
                && ci.StartDate > a_date.Ticks)
            {
                return ci;
            }
        }

        return null;
    }

    /// <summary>
    /// Iterates through Capacity Intervals to find the first one after the date (inclusive) that is Online.
    /// Returns null if not found.
    /// </summary>
    public ResourceCapacityInterval FindFirstOnlineAfter(DateTime a_date)
    {
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval ci = this[i];
            if (ci.StartDate > a_date.Ticks)
            {
                if (ci.Active)
                {
                    return ci;
                }
            }

            if (ci.IsPastPlanningHorizon)
            {
                return ci;
            }
        }

        return null;
    }

    /// <summary>
    /// Iterates through Capacity Intervals to find the first one before the date (inclusive) that is Online.
    /// Returns null if not found.
    /// </summary>
    public ResourceCapacityInterval FindFirstOnlineAtOrBefore(DateTime a_date)
    {
        ResourceCapacityInterval match = null;
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval ci = this[i];
            if (!ci.Active)
            {
                continue;
            }

            if (ci.EndDateTime < a_date)
            {
                match = ci;
            }
            else
            {
                //This interval is past the date
                if (ci.StartDateTime <= a_date)
                {
                    return ci;
                }

                return match;
            }
        }

        //Return the latest one before the date, or null
        return match;
    }

    /// <summary>
    /// O(log(n)). Find the ResourceCapacityInterval that contains a date. Returns -1 if not found.
    /// </summary>
    internal int FindIdx(long date)
    {
        #if DEBUG
        if (date <= 0)
        {
            throw new Exception("Find filed because date<=0");
        }
        #endif
        if (Count > 0)
        {
            return FindIdx(0, Count - 1, date);
        }

        return -1;
    }

    /// <summary>
    /// O(log(n)). Find the ResourceCapacityInterval that contains a date. Returns -1 if not found.
    /// </summary>
    private int FindIdx(int startIdx, int stopIdx, long date)
    {
        #if DEBUG
        if (startIdx > stopIdx)
        {
            throw new Exception("An error occurred while trying to find a capacity interval.");
        }
        #endif
        ResourceCapacityInterval rci;

        if (stopIdx - startIdx <= 4)
        {
            for (int i = startIdx; i <= stopIdx; ++i)
            {
                rci = GetByIndex(i);
                if (rci.ContainsStartPoint(date) == ResourceCapacityInterval.ContainmentType.Contains)
                {
                    return i;
                }
            }

            return -1;
        }

        int halfIdx = startIdx + (stopIdx - startIdx) / 2;
        rci = GetByIndex(halfIdx);
        ResourceCapacityInterval.ContainmentType tContainment = rci.ContainsStartPoint(date);

        if (tContainment == ResourceCapacityInterval.ContainmentType.LessThan)
        {
            return FindIdx(startIdx, halfIdx - 1, date);
        }

        if (tContainment == ResourceCapacityInterval.ContainmentType.GreaterThan)
        {
            return FindIdx(halfIdx + 1, stopIdx, date);
        }

        return halfIdx;
    }

    /// <summary>
    /// Sums the hours for all Overtime Intervals.
    /// </summary>
    /// <returns></returns>
    public decimal GetTotalOvertimeHours()
    {
        long durationSum = 0;
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval rci = this[i];
            if (rci.Active && rci.Overtime)
            {
                durationSum += rci.GetDuration().Ticks;
            }
        }

        return (decimal)new TimeSpan(durationSum).TotalHours;
    }

    /// <summary>
    /// Sums the hours for all Intervals that are online but not Overtime.
    /// </summary>
    /// <returns></returns>
    public decimal GetTotalOnlineNonOvertimeHours()
    {
        long durationSum = 0;
        for (int i = 0; i < Count; i++)
        {
            ResourceCapacityInterval rci = this[i];
            if (rci.Active && !rci.Overtime)
            {
                durationSum += rci.GetDuration().Ticks;
            }
        }

        return (decimal)new TimeSpan(durationSum).TotalHours;
    }

    /// <summary>
    /// returns a TimeSpan equal to the total schedulable time wihtout considering anything
    /// that's already scheduled there (Online time - offline time).
    /// </summary>
    /// <param name="a_start"></param>
    /// <param name="a_end"></param>
    /// <returns></returns>
    public TimeSpan GetTotalSchedulableCapacityBetweenDates(DateTime a_start, DateTime a_end)
    {
        TimeSpan capacity = TimeSpan.Zero;

        // add up online CIs that are between a_start and a_end/
        for (int onlineIdx = 0; onlineIdx < Count; onlineIdx++)
        {
            ResourceCapacityInterval onlineRci = this[onlineIdx];
            if (onlineRci.StartDateTime > a_end)
            {
                break;
            }

            if (onlineRci.Active)
            {
                if ((onlineRci.StartDateTime <= a_start && onlineRci.EndDateTime > a_start) || (onlineRci.StartDateTime >= a_start && onlineRci.StartDateTime < a_end))
                {
                    DateTime start = onlineRci.StartDateTime <= a_start ? a_start : onlineRci.StartDateTime;
                    DateTime end = onlineRci.EndDateTime > a_end ? a_end : onlineRci.EndDateTime;
                    TimeSpan onlineDuration = end.Subtract(start);

                    // subtract any offline time during this online interval
                    for (int offlineIdx = 0; offlineIdx < Count; offlineIdx++)
                    {
                        ResourceCapacityInterval offlineRci = this[offlineIdx];
                        if (offlineRci.StartDateTime > end)
                        {
                            break;
                        }

                        if (offlineRci.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Offline)
                            // Don't use !Active here because I don't think we can schedule on Occupied capacity intervals
                        {
                            if ((offlineRci.StartDateTime <= start && offlineRci.EndDateTime > start) || (offlineRci.StartDateTime >= start && offlineRci.StartDateTime < end))
                            {
                                DateTime offlineStart = offlineRci.StartDateTime <= start ? start : offlineRci.StartDateTime;
                                DateTime offlineEnd = offlineRci.EndDateTime > end ? end : offlineRci.EndDateTime;
                                TimeSpan offlineDuration = offlineEnd.Subtract(offlineStart);
                                if (offlineDuration > onlineDuration)
                                {
                                    onlineDuration = TimeSpan.Zero;
                                    break;
                                }

                                onlineDuration = onlineDuration.Subtract(offlineDuration);
                            }
                        }
                    }

                    capacity = capacity.Add(onlineDuration);
                }
            }
        }

        return capacity;
    }

    public override string ToString()
    {
        return string.Format("Count: {0}".Localize(), Count);
    }

    /// <summary>
    /// Whether this resource has any online capacity.
    /// </summary>
    /// <returns></returns>
    public bool HasOnlineCapacity()
    {
        return FindFirstOnlineAfter(PTDateTime.MinDateTime) != null;
    }
}