using PT.Common.Collections;
using PT.Common.PTMath;
using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Defines a capacity change for a specific time interval for a specific resource.
/// Each Calendar Resource contains two of these to define its start and end conditions plus any number of these can
/// be added by CapacityIntervals and expanded RecurringCapacityIntervals.
/// </summary>
public partial class ResourceCapacityInterval
{
    internal void SimulationInitialization(Resource a_res)
    {
        if (a_res.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
        {
            // Initialize members needed to calculate remaining attention.
            if (m_attentionComparer == null)
            {
                // initialization for attention percent.
                m_attentionComparer = new RequiredAttentionComparer();
                m_scheduledAttention = new SortedDictionary<RequiredAttention, RequiredAttention>(m_attentionComparer);

                m_attnEvtComparer = new AttentionEventComparer();
            }
            else
            {
                m_seqNbr = 0;
                m_scheduledAttention.Clear();

                m_neededPercentMember = null;
            }

            //m_attentionAvailable = 100;
            //m_multitaskingReleases = new LinkedList<MultitaskingRelease>();
        }
    }

    #region Multitasking Resource
    private struct MultitaskingRelease
    {
        internal MultitaskingRelease(InternalActivity a_act, decimal a_percent, long a_date)
        {
            Activity = a_act;
            Percent = a_percent;
            Date = a_date;
        }

        private InternalActivity Activity { get; set; }
        internal decimal Percent { get; set; }
        internal long Date { get; set; }
    }

    // This was approximately how attention percent was implemented prior to the usage enhancement and a bug
    // fix that allows multitasking resources to satisfy multiple resource requirements in the same operation.
    // It probably works faster than the new implementation. If you need to try this out again or use
    // it in the case where a scenario doesn't use the new usage feature and multitasking resource don't 
    // fulfill multiple resource requirements of the same operation, you can uncomment this and using 
    // a conditional check, use this approach instead of the newer approach; if not using usages and it's not 
    // possible for a multitasking resource to fulfill more than one Resource requirement of the same
    // operation then use the old implementation, otherwise use the new one. 

    //decimal m_attentionAvailable = 100;
    //LinkedList<MultitaskingRelease> m_multitaskingReleases = new LinkedList<MultitaskingRelease>();

    /// <summary>
    /// The attention spans scheduled on this interval.
    /// Attentions are keyed on time and a sequence number.
    /// This was originally an AVLTree, but the SortedDictionary turned out to be faster.
    /// </summary>
    private SortedDictionary<RequiredAttention, RequiredAttention> m_scheduledAttention;

    /// <summary>
    /// Used to compare RequiredAttentions in the committedAttention set.
    /// </summary>
    private RequiredAttentionComparer m_attentionComparer;

    /// <summary>
    /// RequiredAttentions can be identicle, this is used to make attentions unique.
    /// </summary>
    private long m_seqNbr;

    /// <summary>
    /// Creates an attention unique within this interval.
    /// </summary>
    /// <param name="a_startOfReqTicks"></param>
    /// <param name="a_endOfReqTicks"></param>
    /// <param name="a_reqAttnPct"></param>
    /// <returns></returns>
    private RequiredAttention CreateReqAttention(RCINeededPercentManager.RCINeededPercent a_needRequirement)
    {
        ++m_seqNbr;
        RequiredAttention attn = new (a_needRequirement.Activity, a_needRequirement.ResourceRequirement, a_needRequirement.StartTicks, a_needRequirement.EndTicks, a_needRequirement.PercentNeeded, m_seqNbr);
        return attn;
    }

    /// <summary>
    /// Creates an attention unique within this interval.
    /// </summary>
    /// <param name="a_startOfReqTicks"></param>
    /// <param name="a_endOfReqTicks"></param>
    /// <param name="a_reqAttnPct"></param>
    /// <returns></returns>
    private RequiredAttention CreateReqAttention(InternalActivity a_act, ResourceRequirement a_rr, long a_startOfReqTicks, long a_endOfReqTicks, decimal a_reqAttnPct)
    {
        ++m_seqNbr;
        RequiredAttention attn = new (a_act, a_rr, a_startOfReqTicks, a_endOfReqTicks, a_reqAttnPct, m_seqNbr);
        return attn;
    }

    // These 2 variables are for testing attention of activities with multiple requirements.
    // They're used to determine attention when multiple requirements of the same activity use the same resource.

    /// <summary>
    /// An event that corresponds to either consumption or release of attention.
    /// Each required attention has 2 events. The first for the consumption of attention and the second
    /// for its release.
    /// </summary>
    private class AttentionEvent
    {
        internal AttentionEvent(long a_time, bool a_increment, RequiredAttention a_attention, long a_seqNbr)
        {
            Time = a_time;
            ReleaseAttention = a_increment;
            Attention = a_attention;
            SeqNbr = a_seqNbr;
        }

        /// <summary>
        /// The time of the event.
        /// </summary>
        internal readonly long Time;

        /// <summary>
        /// If this is true the event is for the release of attention.
        /// Otherwise the event is for the comsumption of attention.
        /// </summary>
        internal readonly bool ReleaseAttention;

        /// <summary>
        /// The amount of attention consumed or released.
        /// </summary>
        internal readonly RequiredAttention Attention;

        /// <summary>
        /// Used to make an RCIs attentions unique.
        /// </summary>
        internal readonly long SeqNbr;

        public override string ToString()
        {
            System.Text.StringBuilder sb = new ();

            decimal attention = 0;
            if (ReleaseAttention)
            {
                attention = Attention.Attention;
            }
            else
            {
                attention = -Attention.Attention;
            }

            sb.AppendFormat("Date:{0}; Attention:{1}; Seq#:{2}", DateTimeHelper.ToLocalTimeFromUTCTicks(Time), attention, SeqNbr);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Used to sort AttentionEvent objects.
    /// </summary>
    private AttentionEventComparer m_attnEvtComparer;

    /// <summary>
    /// Sorts by time, then by attention, then by sequence number as a tiebreaker
    /// Releasing attention should come before adding attention, so if one block ends at the same time as another start,
    ///  the attention is released first
    /// </summary>
    private class AttentionEventComparer : IPTCollectionsComparer<AttentionEvent>
    {
        public bool LessThan(AttentionEvent a_n1, AttentionEvent a_n2)
        {
            if (a_n1.Time == a_n2.Time)
            {
                if (a_n1.ReleaseAttention == a_n2.ReleaseAttention)
                {
                    return a_n1.SeqNbr < a_n2.SeqNbr;
                }

                if (a_n1.ReleaseAttention)
                {
                    return true;
                }

                if (a_n2.ReleaseAttention)
                {
                    return false;
                }
            }

            return a_n1.Time < a_n2.Time;
        }

        public bool LessThanOrEqual(AttentionEvent a_n1, AttentionEvent a_n2)
        {
            if (a_n1.Time == a_n2.Time)
            {
                if (a_n1.ReleaseAttention == a_n2.ReleaseAttention)
                {
                    return a_n1.SeqNbr <= a_n2.SeqNbr;
                }

                if (a_n1.ReleaseAttention)
                {
                    return true;
                }

                if (a_n2.ReleaseAttention)
                {
                    return false;
                }

                return a_n1.SeqNbr <= a_n2.SeqNbr;
            }

            return a_n1.Time <= a_n2.Time;
        }

        public bool GreaterThan(AttentionEvent a_n1, AttentionEvent a_n2)
        {
            if (a_n1.Time == a_n2.Time)
            {
                if (a_n1.ReleaseAttention == a_n2.ReleaseAttention)
                {
                    return a_n1.SeqNbr > a_n2.SeqNbr;
                }

                if (a_n1.ReleaseAttention)
                {
                    return false;
                }

                if (a_n2.ReleaseAttention)
                {
                    return true;
                }
            }

            return a_n1.Time > a_n2.Time;
        }

        public bool GreaterThanOrEqual(AttentionEvent a_n1, AttentionEvent a_n2)
        {
            if (a_n1.Time == a_n2.Time)
            {
                if (a_n1.ReleaseAttention == a_n2.ReleaseAttention)
                {
                    return a_n1.SeqNbr >= a_n2.SeqNbr;
                }

                if (a_n1.ReleaseAttention)
                {
                    return false;
                }

                if (a_n2.ReleaseAttention)
                {
                    return true;
                }
            }

            return a_n1.Time >= a_n2.Time;
        }

        public bool EqualTo(AttentionEvent a_n1, AttentionEvent a_n2)
        {
            if (a_n1.Time == a_n2.Time)
            {
                if (a_n1.ReleaseAttention == a_n2.ReleaseAttention)
                {
                    return a_n1.SeqNbr == a_n2.SeqNbr;
                }
            }

            return false;
        }

        public bool NotEqualTo(AttentionEvent a_n1, AttentionEvent a_n2)
        {
            return !EqualTo(a_n1, a_n2);
        }
    }

    /// <summary>
    /// The result of a call to AttentionAvailable.
    /// </summary>
    internal class AttnAvailResult
    {
        internal AttnAvailResult(long a_availTics, SchedulableSuccessFailureEnum a_result, ResourceRequirement a_conflictingRR)
        {
            AvailableTicks = a_availTics;
            SuccessResult = a_result;
            ConflictingRR = a_conflictingRR;
        }

        /// <summary>
        /// If the SuccessResult is Success, this is the start of when the resource has enough attention to fulfill the required attention.
        /// Otherwise this is the time to try to find capacity again.
        /// </summary>
        internal readonly long AvailableTicks;

        /// <summary>
        /// Whether the attention is available in the specified timespan.
        /// </summary>
        internal readonly SchedulableSuccessFailureEnum SuccessResult;

        /// <summary>
        /// If the SuccessResult is a conflict due to another ResourceRequirement already consuming some of the needed timespan,
        /// this is the first ResourceRequirement in conflict.
        /// </summary>
        internal readonly ResourceRequirement ConflictingRR;
    }

    /// <summary>
    /// Whether a multi tasking resource's interval has some amount of percent available.
    /// If an activity with multiple resource requirements needs attention available, this function expects each requirement's attention to be
    /// checked one after another. It stores information about the activity currently being checked and resets the information when a
    /// different activity is checked.
    /// </summary>
    internal AttnAvailResult AttentionAvailable(List<RCINeededPercentManager.RCINeededPercent> a_rciNps, InternalActivity a_actTestingCapacity, long a_startOfReqTicks, long a_endOfReqTicks)
    {
        // Create consume and release attentionevents for all required attention's.
        PriorityQueue<AttentionEvent> eventQueue = new (m_attnEvtComparer);
        SortedDictionary<RequiredAttention, RequiredAttention>.Enumerator etr = m_scheduledAttention.GetEnumerator();
        long evtSeqNbr = 0;
        while (etr.MoveNext())
        {
            RequiredAttention ra = etr.Current.Value;
            ra.AttenReleased = false;

            if (ra.Intersection(a_startOfReqTicks, a_endOfReqTicks))
            {
                ++evtSeqNbr;
                AttentionEvent start = new (ra.StartTicks, false, ra, evtSeqNbr);
                eventQueue.InitialInsert(start);

                ++evtSeqNbr;
                AttentionEvent end = new (ra.EndTicks, true, ra, evtSeqNbr);
                eventQueue.InitialInsert(end);
            }
        }

        //Now add the test required attention.
        foreach (RCINeededPercentManager.RCINeededPercent np in a_rciNps)
        {
            RequiredAttention reqAttn = CreateReqAttention(np);
            ++evtSeqNbr;
            AttentionEvent start = new(reqAttn.StartTicks, false, reqAttn, evtSeqNbr);
            eventQueue.InitialInsert(start);

            ++evtSeqNbr;
            AttentionEvent end = new(reqAttn.EndTicks, true, reqAttn, evtSeqNbr);
            eventQueue.InitialInsert(end);
        }

        eventQueue.InitialInsertionComplete();

        // Initialize processing of the events.
        decimal attention = 100;
        long curEventTime = 0;
        if (eventQueue.ContainsElements)
        {
            AttentionEvent ae = eventQueue.PeekMin();
            curEventTime = ae.Time;
        }

        // This value is only set if there's not enough attention. It indicates the check has failed.
        RequiredAttention conflictingAttn = null;
        // Process the events.
        while (eventQueue.ContainsElements && eventQueue.PeekMin().Time <= a_endOfReqTicks)
        {
            AttentionEvent curAttn = eventQueue.PeekMin();

            if (curAttn.Time != curEventTime)
            {
                // All the events at a point in time have been processed. 
                // Check whether attention isn't available. 
                if (attention > 100)
                {
                    throw new Exception("Attention's greater than 100%.");
                }

                if (attention < 0)
                {
                    // There's not enough attention available.
                    //First check if another activity is the cause
                    foreach (AttentionEvent ae in eventQueue)
                    {
                        RequiredAttention ra = ae.Attention;
                        if (ra.Activity != a_actTestingCapacity && ra.AttnConsumed && !ra.AttenReleased)
                        {
                            conflictingAttn = ra;
                            break;
                        }
                    }

                    if (conflictingAttn == null)
                    {
                        //There were no other activities conflicting, see if it this activity
                        foreach (AttentionEvent ae in eventQueue)
                        {
                            RequiredAttention ra = ae.Attention;
                            if (ra.AttnConsumed && !ra.AttenReleased)
                            {
                                conflictingAttn = ra;
                                break;
                            }
                        }
                    }

                    break;
                }

                // Move onto the next set of attention releases at a point in time.
                curEventTime = curAttn.Time;
            }

            eventQueue.DeleteMin();

            // Increment or decrement attention.
            // Don't check for available attention until after all events after the current time have passed.
            if (curAttn.ReleaseAttention)
            {
                attention += curAttn.Attention.Attention;
                curAttn.Attention.AttenReleased = true;
            }
            else
            {
                curAttn.Attention.AttnConsumed = true;
                attention -= curAttn.Attention.Attention;
            }
        }

        long availTicks;
        ResourceRequirement conflictingRR = null;

        if (attention < 0)
        {
            // Use the last scheduled attention as the Try again ticks.
            availTicks = conflictingAttn.EndTicks;
            SortedDictionary<RequiredAttention, RequiredAttention>.Enumerator tryAgainEtr = m_scheduledAttention.GetEnumerator();
            while (tryAgainEtr.MoveNext())
            {
                RequiredAttention ra = tryAgainEtr.Current.Value;
                if (ra.EndTicks > a_startOfReqTicks)
                {
                    availTicks = ra.EndTicks;
                    break;
                }
            }
        }
        else
        {
            availTicks = a_startOfReqTicks;
        }

        SchedulableSuccessFailureEnum success;
        if (conflictingAttn != null)
        {
            conflictingRR = conflictingAttn.ResReq;
            success = SchedulableSuccessFailureEnum.AttentionConflictBetweenMultipleRRs;
        }
        else if (attention < 0)
        {
            success = SchedulableSuccessFailureEnum.AttentionNotAvailable;
        }
        else
        {
            success = SchedulableSuccessFailureEnum.Success;
        }

        AttnAvailResult result = new (availTicks, success, conflictingRR);

        return result;
    }

    private static void ProcNextAttentionAvailableEvent(PriorityQueue<AttentionEvent> eventQueue, ref decimal attention, ref long curEventTime)
    {
        AttentionEvent curAttn = eventQueue.PeekMin();

        if (curAttn.Time != curEventTime)
        {
            // All the events at a point in time have been processed. 
            // Check whether attention isn't available. 
            if (attention > 100)
            {
                throw new Exception("Attention's greater than 100%.");
            }

            if (attention < 0)
            {
                // There's not enough attention available.
                return;
            }

            // Move onto the next set of attention releases at a point in time.
            curEventTime = curAttn.Time;
        }

        eventQueue.DeleteMin();

        // Increment or decrement attention.
        // Don't check for available attention until after all events after the current time have passed.
        if (curAttn.ReleaseAttention)
        {
            attention += curAttn.Attention.Attention;
        }
        else
        {
            attention -= curAttn.Attention.Attention;
        }
    }

    /// <summary>
    /// Calculate attention available at a point in time.
    /// </summary>
    /// <param name="a_ticks"></param>
    /// <returns></returns>
    internal decimal CalcAvailableAttentionPointInTime(long a_ticks)
    {
        decimal attention = 100;
        SortedDictionary<RequiredAttention, RequiredAttention>.Enumerator e = m_scheduledAttention.GetEnumerator();

        // Each interval that contains the tested point in time consumes attention.
        while (e.MoveNext() && Interval.Contains(e.Current.Key.StartTicks, e.Current.Key.EndTicks, a_ticks))
        {
            KeyValuePair<RequiredAttention, RequiredAttention> cur = e.Current;
            attention -= cur.Value.Attention;

            #if DEBUG
            AvailableAttentionHelpers.ValidateAttentionPercent(attention);
            #endif
        }

        return attention;
    }

    /// <summary>
    /// Add some attention that's required. This presumes it's already been determined that the attention is available over the given interval.
    /// This is currently being done when the BlockReserveration is made; before the activity is scheduled.
    /// </summary>
    internal void ScheduleAttention(InternalActivity a_act, long a_simClock, long a_startTicks, long a_endTicks, decimal a_attentionPct)
    {
        // part of old implementation of attention percent. Kept for reference in case
        // you need to revert to it for performance purposes.
        //m_attentionAvailable -= a_attentionPct;
        //MultitaskingRelease mtr = new MultitaskingRelease(null, a_attentionPct, a_endTicks);
        //m_multitaskingReleases.AddLast(mtr);

        RequiredAttention attention = CreateReqAttention(a_act, null, a_startTicks, a_endTicks, a_attentionPct);
        SortedDictionary<RequiredAttention, RequiredAttention>.Enumerator etr = m_scheduledAttention.GetEnumerator();
        List<RequiredAttention> removeList = new ();

        // Remove any RequiredAttentions that are past the simulation clock. These are no longer needed because attention is only needed over timespans at or past the simulation clock.
        // Keeping the set of attentions small improves the performance of finding attention.
        while (etr.MoveNext())
        {
            if (etr.Current.Key.EndTicks < a_simClock)
            {
                removeList.Add(etr.Current.Key);
            }
            else
            {
                break;
            }
        }

        foreach (RequiredAttention ra in removeList)
        {
            m_scheduledAttention.Remove(ra);
        }

        m_scheduledAttention.Add(attention, attention);
    }

    /// <summary>
    /// When some attention is available. If no attention is available this returns long.MaxValue.
    /// This function assumes that the current available attention is 0.
    /// </summary>
    /// <returns></returns>
    internal long GetNextReleaseDate(long a_simClock)
    {
        long releaseTicks = long.MaxValue;
        SortedDictionary<RequiredAttention, RequiredAttention>.Enumerator e = m_scheduledAttention.GetEnumerator();
        while (e.MoveNext())
        {
            KeyValuePair<RequiredAttention, RequiredAttention> cur = e.Current;
            if (cur.Value.EndTicks > a_simClock)
            {
                // The end ticks corresponds to a release.
                releaseTicks = Math.Min(cur.Value.EndTicks, releaseTicks);
                if (cur.Value.StartTicks > releaseTicks)
                {
                    // It's not possible for further releases to be after the current release ticks, terminate the loop.
                    break;
                }
            }
        }

        return releaseTicks;
    }

    /// <summary>
    /// Used by other code to reference the helper NeededPercentManager created for this interval.
    /// </summary>
    internal RCINeededPercentManager m_neededPercentMember;
    #endregion

    /// <summary>
    /// Whether this capacity interval can run the specified production
    /// </summary>
    /// <param name="a_productionUsage">The production needed to run on this interval</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal bool CanScheduleProductionUsage(MainResourceDefs.usageEnum a_productionUsage)
    {
        switch (a_productionUsage)
        {
            case MainResourceDefs.usageEnum.Unspecified:
                return true;
            case MainResourceDefs.usageEnum.Setup:
                return UsedForSetup;
            case MainResourceDefs.usageEnum.Run:
                return UsedForRun;
            case MainResourceDefs.usageEnum.PostProcessing:
                return UsedForPostProcessing;
            case MainResourceDefs.usageEnum.Storage:
                return UsedForStorage;
            case MainResourceDefs.usageEnum.Clean:
                return UsedForClean;
            default:
                throw new ArgumentOutOfRangeException(nameof(a_productionUsage), a_productionUsage, null);
        }
    }
}