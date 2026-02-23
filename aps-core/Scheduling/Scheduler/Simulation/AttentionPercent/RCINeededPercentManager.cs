using PT.Common.Collections;
using PT.Common.PTMath;

namespace PT.Scheduler;

/// <summary>
/// Initially the RCI describes points online where capacity is needed to satisfy a resource requirement.
/// Creates a set of requirements on all RCIs.
/// This is then used to determine if attention is available.
/// </summary>
internal partial class RCINeededPercentManager
{
    // The set of all RCIs that attention is required from.
    private readonly DictionaryCollection<ResourceCapacityInterval, RCINeededPercent> m_rciNeedRequirements;

    internal RCINeededPercentManager()
    {
        m_rciNeedRequirements = new DictionaryCollection<ResourceCapacityInterval, RCINeededPercent>();
    }

    /// <summary>
    /// Create a set of the ResourceCapcityIntervals that are needed;one for each ResourceCapacityInterval that's required, starting from the specified node.
    /// Each required ResourceCapacityInterval is linked back to this object.
    /// </summary>
    /// <param name="a_currentRCINode">The first available node on or before the interval.</param>
    /// <param name="a_ia">The activity of the requirement.</param>
    /// <param name="a_rr">The RR</param>
    /// <param name="a_startDate">The start of the interval or needed percent.</param>
    /// <param name="a_endDate">The end of the interval or needed percent.</param>
    internal void AddRCINeededPercents(LinkedListNode<ResourceCapacityInterval> a_currentRCINode, InternalActivity a_ia, ResourceRequirement a_rr, long a_startDate, long a_endDate)
    {
        // Determine the set of all RCIs attention is required from. 
        LinkedListNode<ResourceCapacityInterval> cur = a_currentRCINode;
        while (cur != null)
        {
            ResourceCapacityInterval rci = cur.Value;
            #if DEBUG
            if (rci.m_neededPercentMember != this)
            {
                rci.m_neededPercentMember = this;
            }
            #endif
            if (rci.StartDate >= a_endDate)
            {
                break;
            }

            if (rci.Active)
            {
                if (Interval.Intersection(rci.StartDate, rci.EndDate, a_startDate, a_endDate))
                {
                    decimal neededPercent = a_ia.GetAdjustedAttentionPercent(a_rr, rci);
                    m_rciNeedRequirements.Add(rci, new RCINeededPercent(a_ia, rci, neededPercent, a_rr, a_startDate, a_endDate));
                }
            }

            cur = cur.Next;
        }
    }

    /// <summary>
    /// Clear this collection.
    /// </summary>
    internal void Clear()
    {
        m_rciNeedRequirements.Clear();
    }

    private class Failure
    {
        internal Failure(InternalActivity a_act, long a_simClock, long a_startOfReqTicks, long a_endOfReqTicks)
        {
            m_act = a_act;
            m_simClock = a_simClock;
            m_startOfReqTicks = a_startOfReqTicks;
            m_endOfReqTicks = a_endOfReqTicks;
        }

        private readonly InternalActivity m_act;
        private readonly long m_simClock;
        private readonly long m_startOfReqTicks;
        private readonly long m_endOfReqTicks;

        public override string ToString()
        {
            return string.Format("act:{0}; Clock:{1}; ReqStart:{2}; ReqEnd:{3}", m_act.Job.Name, DateTimeHelper.ToLocalTimeFromUTCTicks(m_simClock), DateTimeHelper.ToLocalTimeFromUTCTicks(m_startOfReqTicks), DateTimeHelper.ToLocalTimeFromUTCTicks(m_endOfReqTicks));
        }
    }

    private List<Failure> z_failures = new ();

    /// <summary>
    /// Whether the attention required by each ResourceCapacityInterval is available.
    /// </summary>
    internal ResourceCapacityInterval.AttnAvailResult AttentionAvailable(InternalActivity a_act, long a_startOfReqTicks, long a_endOfReqTicks)
    {
        ResourceCapacityInterval.AttnAvailResult res = null;

        foreach (KeyValuePair<ResourceCapacityInterval, List<RCINeededPercent>> rciNeeds in m_rciNeedRequirements.OrderBy(kvp => kvp.Key.StartDate))
        {
            res = rciNeeds.Key.AttentionAvailable(rciNeeds.Value, a_act, a_startOfReqTicks, a_endOfReqTicks);
            if (res.AvailableTicks != a_startOfReqTicks)
            {
                //z_failures.Add(new Failure(a_act, a_simClock, a_startOfReqTicks, a_endOfReqTicks));

                //z_failures.Add(new Failure(a_act, a_simClock, a_startOfReqTicks, a_endOfReqTicks));
                break;
            }
        }

        return res;
    }

    public override string ToString()
    {
        string ret = "count=" + m_rciNeedRequirements.Count;
        foreach (KeyValuePair<ResourceCapacityInterval, List<RCINeededPercent>> pair in m_rciNeedRequirements)
        {
            ret += ";" + pair;
        }

        return ret;
    }
}