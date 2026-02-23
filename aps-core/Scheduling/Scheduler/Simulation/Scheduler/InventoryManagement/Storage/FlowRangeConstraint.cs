using System.Collections;

using PT.Common.PTMath;

namespace PT.Scheduler;

internal class FlowRangeConstraint : IEnumerable<IInterval>
{
    private readonly int m_limit;

    internal FlowRangeConstraint(int a_limit)
    {
        m_inUseRanges = new ();
        m_limit = a_limit;
    }
    
    internal int Count => m_inUseRanges.Count;


    private readonly List<IInterval> m_inUseRanges;
    private IInterval m_allocatedUsage;

    internal long ScheduleUsage()
    {
        if (m_allocatedUsage == null || m_allocatedUsage.Duration == 0)
        {
            return 0;
        }

        #if DEBUG
        if (!VerifyAllocationRange(m_allocatedUsage, out long _))
        {
            //throw new DebugException("Connector scheduled for use when it isn't available");
        }
        #endif

        m_inUseRanges.Add(m_allocatedUsage);
        long usageEnd = m_allocatedUsage.EndTicks;
        m_allocatedUsage = null;
        return usageEnd;
    }

    internal void Purge(long a_simClock)
    {
        for (int i = m_inUseRanges.Count - 1; i >= 0; i--)
        {
            IInterval interval = m_inUseRanges[i];
            if (interval.EndTicks <= a_simClock)
            {
                m_inUseRanges.RemoveAt(i);
            }
        }
    }

    internal void AllocateUsage(long a_usageTimePoint)
    {
        if (m_allocatedUsage == null)
        {
            m_allocatedUsage = new Interval(a_usageTimePoint, a_usageTimePoint);
        }
        else
        {
            ((Interval)m_allocatedUsage).EndTicks = a_usageTimePoint;
        }
    }
    
    internal void AllocateUsage(IInterval a_profileUsageRange)
    {
        if (a_profileUsageRange.Duration > 0) //Instant transfers will not have a duration
        {
            m_allocatedUsage = a_profileUsageRange;
        }
    }

    internal bool VerifyAllocationRange(IInterval a_range, out long o_intersectionEndTicks)
    {
        //if (m_allocatedUsage != null)
        //{
        //    //this was already allocated. For example when a MR and Product attempt to use the same connector
        //    o_intersectionEndTicks = -1;
        //    return false;
        //}
        o_intersectionEndTicks = -1;

            //TODO: Should this be constrained with existing in use ranges? If so, check intersections first
        if (a_range.Duration <= 0)
        {
            //This is an instantaneous transfer
            return true;
        }

        if (m_inUseRanges.Count < m_limit)
        {
            //Quick check so we can avoid intersections if the limit is high.
            return true;
        }

        o_intersectionEndTicks = 0;
        int intersections = 0;
        //TODO: We could find the latest intersection and return that date for performance
        foreach (IInterval inUseRange in m_inUseRanges)
        {
            if (inUseRange.Intersection(a_range))
            {
                intersections++;
                o_intersectionEndTicks = inUseRange.EndTicks;

                if (intersections == m_limit)
                {
                    return false;
                }
            }
        }

        return true;
    }
     
    internal void ResetAllocation()
    {
        m_allocatedUsage = null;
    }

    public IEnumerator<IInterval> GetEnumerator()
    {
        foreach (IInterval inUseRange in m_inUseRanges)
        {
            yield return inUseRange;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Return a sorted IInterval usages list by end date.
    /// </summary>
    /// <returns></returns>
    internal List<IInterval> GetSortedUsages()
    {
        if (m_inUseRanges.Count == 0)
        {
            return new List<IInterval>();
        }

        return m_inUseRanges.OrderBy(r => r.EndTicks).ToList();
    }
}
