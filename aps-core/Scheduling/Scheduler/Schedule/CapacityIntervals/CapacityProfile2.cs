using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// This part of this class has been broken off into a different file to segregate the code that regenerates the capacity profile.
/// </summary>
public partial class CapacityProfile
{
    /// <summary>
    /// Regenerate the profile intervals from the raw intervals. This is necessary anytime the raw intervals have changed. The raw intervals are the set of all intervals
    /// within this profile. They might not be sorted and can overlap.
    /// The profile intervals are sorted by time and do not overlap. Inactive intervals (offline types of intervals) override Active intervals (online interval types) and
    /// overlapping Active intervals are merged into new intervals whose NbrOfPeople is equal to the sum of all the intersecting intervals.
    /// </summary>
    /// <param name="a_sd"></param>
    internal void Regenerate(long a_planningHorizonEndTicks)
    {
        List<ResourceCapacityInterval> testResult = CapacityProfileRegenerator.Regenerate(a_planningHorizonEndTicks, m_rawIntervals);

        m_profileIntervals.Clear();
        foreach (ResourceCapacityInterval rci in testResult)
        {
            m_profileIntervals.Add(rci);
        }
    }

    /// <summary>
    /// A helper class designed to segregate the capacity profile generation code from other code.
    /// </summary>
    private class CapacityProfileRegenerator
    {
        /// <summary>
        /// Given the raw capacity intervals, which may overlap. This returns a non-overlapping version of the raw intervals. Overlapping sections of intervals are broken off as new intervals.
        /// Offline intervals override sections of online intervals they overlap with.
        /// </summary>
        /// <param name="a_sd"></param>
        /// <param name="a_rawIntervals">The complete set of intervals you want to generate a non-overlaping capacity profile of.</param>
        /// <returns></returns>
        internal static List<ResourceCapacityInterval> Regenerate(long a_planningHorizonEndTicks, ResourceCapacityIntervalsCollection a_rawIntervals)
        {
            List<ResourceCapacityInterval> testActive = ActiveOrInactiveCapacityMerger.CreateMergedCapacityIntervals(a_rawIntervals, true);
            List<ResourceCapacityInterval> testInactive = ActiveOrInactiveCapacityMerger.CreateMergedCapacityIntervals(a_rawIntervals, false);

            List<ResourceCapacityInterval> testResult = ActiveAndInactiveIntervalMerger.CreateMergedActiveAndInactiveIntervalSet(a_planningHorizonEndTicks, testActive, testInactive);

            return testResult;
        }

        /// <summary>
        /// Whether an event marks the start of an interval or the end of an interval. Each capacity interval has 2 events, a start and and end.
        /// </summary>
        private enum EEventType
        {
            /// <summary>
            /// The event is the start of an interval.
            /// </summary>
            StartInterval,

            /// <summary>
            /// The event is the end of an interval.
            /// </summary>
            EndInterval
        }

        /// <summary>
        /// Represents an event for either the start or end of a capacity interval.
        /// </summary>
        private class IntervalEvent
        {
            /// <summary>
            /// Create a capacity interval event.
            /// </summary>
            /// <param name="a_eventTime">The event time.</param>
            /// <param name="a_event">Either the start or end of an interval.</param>
            /// <param name="a_active">Whether the capacity interval is Active (some type of online interval) or Inactive (some type of offline interval).</param>
            /// <param name="a_rci">The ResourceCapacityInterval this event is for.</param>
            internal IntervalEvent(long a_eventTime, EEventType a_event, bool a_active, ResourceCapacityInterval a_rci)
            {
                Ticks = a_eventTime;
                EventType = a_event;
                Active = a_active;
                RCI = a_rci;
            }

            /// <summary>
            /// The DateTime ticks of the start or end of an interval.
            /// </summary>
            internal long Ticks { get; private set; }

            /// <summary>
            /// Either the start or end of an interval.
            /// </summary>
            internal EEventType EventType { get; private set; }

            /// <summary>
            /// Whether the interval is Active(some online type) or Inactive(some offline type).
            /// </summary>
            internal bool Active { get; private set; }

            /// <summary>
            /// The ResourceCapacityInterval this event is for.
            /// </summary>
            internal ResourceCapacityInterval RCI { get; private set; }

            protected virtual string GetDescription()
            {
                return string.Format("Time={0}; Type={1}; Active={2}".Localize(), DateTimeHelper.ToLocalTimeFromUTCTicks(Ticks), EventType, Active);
            }

            public override string ToString()
            {
                return GetDescription();
            }
        }

        /// <summary>
        /// Used to merge either Active(online types) or Inactive(offline types) capacity intervals.
        /// </summary>
        private class ActiveOrInactiveCapacityMerger
        {
            /// <summary>
            /// Merge capacity intervals of a specific type (Active or Inactive) into a non-intersecting set of ordered capacity intervals.
            /// Any segment where capacity intervals intersect will be converted into a new capacity interval
            /// whose properties are summed together. For instance if 3 intervals intersected, the new
            /// capacity interval created to represent the intersection would start and end where they intersect
            /// and have NbrOfPeople equal to the sum of the 3 intersecting capacity intervals.
            /// </summary>
            /// <param name="a_rawIntervals">The set of potentially overlapping capacity intervals.</param>
            /// <param name="a_active">The type of capacity intervals to merge; either Active or Inactive.</param>
            /// <returns>A non-overlapping set of capacity intervals that represents the raw capacity intervals. The intervals are sorted by time.</returns>
            internal static List<ResourceCapacityInterval> CreateMergedCapacityIntervals(ResourceCapacityIntervalsCollection a_rawIntervals, bool a_active)
            {
                //
                // For each inactive interval add StartInterval and EndInterval events to a list and sort the list by time.
                //
                List<IntervalEvent> events = new ();

                for (int i = 0; i < a_rawIntervals.Count; ++i)
                {
                    ResourceCapacityInterval rci = a_rawIntervals[i];
                    #if DEBUG
                    if (rci.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Occupied)
                    {
                        throw new Exception("Occupied intervals shouldn't be encountered here since they're only created during a simulation.");
                    }
                    #endif
                    if (rci.Active == a_active && rci.NbrOfPeople > 0) // This check shouldn't be necessary.
                    {
                        events.Add(new IntervalEvent(rci.StartDate, EEventType.StartInterval, a_active, rci));
                        events.Add(new IntervalEvent(rci.EndDate, EEventType.EndInterval, a_active, rci));
                    }
                }

                events.Sort(CompareIntervalEventsByDateAndEventType);

                //
                // Create a new list of capacity intervals equivalent to the original set of intervals except all overlapping intervals are merged into new intervals.
                //
                List<ResourceCapacityInterval> mergedIntervals = new ();

                if (events.Count > 0)
                {
                    IntervalEvent evt = events[0];

                    #region Debug must be false: inactiveEvent.EventType == InactiveEventTypeEnum.EndInactiveEvent
                    #if DEBUG
                    if (evt.EventType == EEventType.EndInterval)
                    {
                        throw new Exception("The first event must always be a Start. There must be a bad Resource Capacity Interval above.");
                    }
                    #endif
                    #endregion

                    //
                    // process the 0th event.
                    //

                    long startOfIntervalTicks = evt.Ticks; // The start of the next interval that will be added.
                    long curEventsBeingProcessedTicks = events[1].Ticks; // The time of the current set of events being processed. Event processing will start at index 1 below. It's possible for multiple events to occur at the same time.
                    decimal nbrOfPeople = evt.RCI.NbrOfPeople; // The total number of people that go into the next capacity interval.

                    HashSet<ResourceCapacityInterval> startedIntervalsHash = new (); // The set of intervals that are started within the next capacity interval.
                    startedIntervalsHash.Add(evt.RCI);

                    bool createNextRCI = false; // Whether to create the next ResourceCapacityInterval

                    //
                    // Values used to create the next ResourceCapacityInterval.
                    //

                    // Whether a start or end event caused the end of the last interval. It's possible multiple intervals can be ending at the same time.
                    // This is used to prevent multiple changes to the end event values when processing events at a point in time.
                    bool endOfLastInterval = false;
                    CapacityIntervalDefs.capacityIntervalTypes endIntervalCapType = CapacityIntervalDefs.capacityIntervalTypes.Online;
                    decimal endNbrOfPeople = 0;

                    // Process all the events. The events are either all Active or all Inactive; this simplifies the processing logic below.
                    for (int i = 1; i < events.Count; ++i) // Starting at index 1 becaue index 0 was processed above.
                    {
                        evt = events[i];

                        //
                        // Process the event.
                        //
                        if (evt.EventType == EEventType.EndInterval)
                        {
                            // Among all the events at a single point in time, end intervals are processed first. The way the events list is sorted
                            // insures this. But based on code changes that have occurred since the initial code was written it appears it doesn't
                            // matter whether StartIntervals or EndIntervals are processed first. The sequence of processing appears unimportant
                            // in the code that handles events in the "if-else" conditional immediately below.
                            if (!endOfLastInterval) // Checked because it's possible multiple intervals can be ending at the same time.
                            {
                                // An end interval will always cause a capacity interval to be created.

                                #region NbrOfPeople > 0
                                if (nbrOfPeople <= 0)
                                {
                                    throw new Exception("NbrOfPeople must be greater than 0.".Localize());
                                }
                                #endregion

                                createNextRCI = true;

                                // Store the values that will be used to create the interval.
                                endIntervalCapType = GetGreatestCapacityTypeInSet(startedIntervalsHash, endIntervalCapType, nbrOfPeople);
                                endNbrOfPeople = nbrOfPeople;
                                endOfLastInterval = true;
                            }

                            // Remove interval from the set of intervals currently being processed.
                            nbrOfPeople -= evt.RCI.NbrOfPeople;
                            startedIntervalsHash.Remove(evt.RCI);
                        }
                        else // evt.EventType==EventTypeEnum.StartInterval
                        {
                            if (startOfIntervalTicks == long.MaxValue)
                            {
                                startOfIntervalTicks = evt.Ticks;
                            }

                            if (!endOfLastInterval &&
                                nbrOfPeople > 0 // Checked because it's possible multiple intervals can be ending at the same time.
                                &&
                                startOfIntervalTicks != evt.Ticks)
                            {
                                // nbrOfPeople>0 means
                                // If Active intervals are already being processed, a start of an interval can also cause the creation of a capacity interval 
                                // since it will cause a new interval to be be created with additional people; at the start time the number of people increases.
                                createNextRCI = true;

                                // Store the values that will be used to create the interval.
                                endIntervalCapType = GetGreatestCapacityTypeInSet(startedIntervalsHash, endIntervalCapType, nbrOfPeople);
                                endNbrOfPeople = nbrOfPeople;
                                endOfLastInterval = true;
                            }

                            startedIntervalsHash.Add(evt.RCI);
                            nbrOfPeople += evt.RCI.NbrOfPeople;
                        }

                        // 
                        // Setup to process the next event and create the next RCI if an interval has ended.
                        //
                        int nextEvtIdx = i + 1;
                        IntervalEvent nextEvt = nextEvtIdx >= events.Count ? null : events[nextEvtIdx];

                        if ((nextEvt != null && curEventsBeingProcessedTicks != nextEvt.Ticks) // All the events at the last event time have been processed.
                            ||
                            nextEvt == null) // There are no more events left to process. Handle creation of the last RCI.
                        {
                            bool reset = false;
                            if (createNextRCI)
                            {
                                // Add the interval. The interval is created here after all events at the interval end time have been processed.
                                ResourceCapacityInterval rci = new(events[i - 1].RCI.Id, endIntervalCapType, startOfIntervalTicks, evt.Ticks, endNbrOfPeople, events[i - 1].RCI.GetIntervalProfile());
                                mergedIntervals.Add(rci);

                                //
                                // Reset end interval data.
                                //
                                createNextRCI = false;
                                endIntervalCapType = CapacityIntervalDefs.capacityIntervalTypes.Online;
                                endOfLastInterval = false;
                                endNbrOfPeople = 0;

                                startOfIntervalTicks = long.MaxValue;
                                curEventsBeingProcessedTicks = long.MaxValue;

                                reset = true;
                            }

                            if ((reset && evt.EventType == EEventType.StartInterval) // A new event is starting.
                                ||
                                (reset && evt.EventType == EEventType.EndInterval && nbrOfPeople > 0)) // Number of people > 0 indicates an events are still being processed.
                            {
                                // The start interval time in increased because all Active intervals still haven't completed.
                                startOfIntervalTicks = evt.Ticks;
                            }

                            if (nextEvt != null)
                            {
                                curEventsBeingProcessedTicks = nextEvt.Ticks;
                            }
                        }
                    }
                }

                return mergedIntervals;
            }

            private static bool IsEitherCapType(CapacityIntervalDefs.capacityIntervalTypes a_type, CapacityIntervalDefs.capacityIntervalTypes a_cap1, CapacityIntervalDefs.capacityIntervalTypes a_cap2)
            {
                return a_cap1 == a_type || a_cap2 == a_type;
            }

            /// <summary>
            /// Determine the greatest capacity type of 2 intervals.
            /// Capacity intervals from greatest to lowest are Occupied, Cleanout, Offline, ReservedOnline, PotentialOvertime, Overtime, and NormalOnline.
            /// So if you compare a NormalOnline interval type to a any other type, the other type will be returned since NormalOnline has the lowest value.
            /// This function was used to determine the CapacityIntervalDefs.capacityIntervalTypes for intervals created for overlapping intervals.
            /// So if PotentialOvertime, Overtime, and NormalOvertime overlap, the CapacityIntervalDefs.capacityIntervalTypes for the new interval
            /// representing the overlap will be PotentialOvertime.
            /// </summary>
            /// <param name="a_cap1">The first interval type.</param>
            /// <param name="a_cap2">The second interval type.</param>
            /// <returns></returns>
            private static CapacityIntervalDefs.capacityIntervalTypes GetGreatestCapacityType(CapacityIntervalDefs.capacityIntervalTypes a_cap1, CapacityIntervalDefs.capacityIntervalTypes a_cap2)
            {
                CapacityIntervalDefs.capacityIntervalTypes capType;

                if (IsEitherCapType(capType = CapacityIntervalDefs.capacityIntervalTypes.Occupied, a_cap1, a_cap2))
                {
                    return capType;
                }

                if (IsEitherCapType(capType = CapacityIntervalDefs.capacityIntervalTypes.Offline, a_cap1, a_cap2))
                {
                    return capType;
                }

                if (IsEitherCapType(capType = CapacityIntervalDefs.capacityIntervalTypes.ReservedOnline, a_cap1, a_cap2))
                {
                    return capType;
                }

                if (IsEitherCapType(capType = CapacityIntervalDefs.capacityIntervalTypes.Online, a_cap1, a_cap2))
                {
                    return capType;
                }

                throw new Exception("CapacityProfile.cs. An unknown CapacityIntervalDefs.capacityIntervalTypes was encountered."); // No need to localize. This should be caught by Mass Recordings.
            }

            /// <summary>
            /// Get the greatest capacity type of all the intervals within a HashSet.
            /// </summary>
            /// <param name="a_intervalSet">The set of intervals to search.</param>
            /// <param name="a_defaultValue">The initial value to use. This value will also be returned if the a_intervalSet has no entries.</param>
            /// <param name="a_testNbrOfPeople">Used to test the number of people hasn't dropped below 0. This has no affect on the value returned and can be deleted.</param>
            /// <returns></returns>
            private static CapacityIntervalDefs.capacityIntervalTypes GetGreatestCapacityTypeInSet(HashSet<ResourceCapacityInterval> a_intervalSet, CapacityIntervalDefs.capacityIntervalTypes a_defaultValue, decimal a_testNbrOfPeople)
            {
                CapacityIntervalDefs.capacityIntervalTypes createEndIntervalCapType = a_defaultValue;
                HashSet<ResourceCapacityInterval>.Enumerator etr = a_intervalSet.GetEnumerator();
                if (etr.MoveNext())
                {
                    #region Test NbrOfPeople
                    if (a_testNbrOfPeople <= 0)
                    {
                        throw new Exception("nbrOfPeople <=0. An interval can't be in process.");
                    }
                    #endregion

                    //
                    // When starting a new interval, an in-process interval needs to be ended by adding an end interval.
                    //

                    // Find the interval type.
                    createEndIntervalCapType = etr.Current.IntervalType; // Initialized to the first elements value.

                    while (etr.MoveNext())
                    {
                        createEndIntervalCapType = GetGreatestCapacityType(createEndIntervalCapType, etr.Current.IntervalType);
                    }
                }

                return createEndIntervalCapType;
            }

            /// <summary>
            /// Sorted by date.
            /// Subsorted by EventType. StartInterval treated as greater than EndInterval.
            /// </summary>
            /// <param name="a_x"></param>
            /// <param name="a_y"></param>
            /// <returns></returns>
            private static int CompareIntervalEventsByDateAndEventType(IntervalEvent a_x, IntervalEvent a_y)
            {
                int c = Comparer<long>.Default.Compare(a_x.Ticks, a_y.Ticks);

                if (c == 0)
                {
                    if (a_x.EventType == EEventType.StartInterval && a_y.EventType == EEventType.EndInterval)
                    {
                        c = 1;
                    }
                    else if (a_x.EventType == EEventType.EndInterval && a_y.EventType == EEventType.StartInterval)
                    {
                        c = -1;
                    }
                }

                return c;
            }
        }

        /// <summary>
        /// Merges Active(online types) and Inactive(offline types) capacity intervals. Inactive intervals overlapping with Active intervals cancel out the overlapping segment making them Inactive.
        /// </summary>
        private class ActiveAndInactiveIntervalMerger
        {
            /// <summary>
            /// Merge the Active and Inactive intervals resulting in a non-overlapping continuous set of intervals from PtDateTime.MIN_DATE to PtDateTime.MAX_DATE_TICKS.
            /// Everything from the MIN_DATE to the first interval is offline.
            /// Everything past the planning horizon is made online.
            /// </summary>
            /// <param name="a_activeIntervals">The set of active intervals. There is no overlap between active intervals.</param>
            /// <param name="a_inactiveIntervals">The set of inactive intervals. There is no overlap between the inactive intervals.</param>
            /// <returns></returns>
            internal static List<ResourceCapacityInterval> CreateMergedActiveAndInactiveIntervalSet(long a_planningHorizonEndTicks, List<ResourceCapacityInterval> a_activeIntervals, List<ResourceCapacityInterval> a_inactiveIntervals)
            {
                //
                // For each inactive interval add StartInterval and EndInterval events to a list and sort the list by time.
                //
                List<IntervalEvent> events = new ();

                foreach (ResourceCapacityInterval rci in a_activeIntervals)
                {
                    events.Add(new IntervalEvent(rci.StartDate, EEventType.StartInterval, true, rci));
                    events.Add(new IntervalEvent(rci.EndDate, EEventType.EndInterval, true, rci));
                }

                foreach (ResourceCapacityInterval rci in a_inactiveIntervals)
                {
                    events.Add(new IntervalEvent(rci.StartDate, EEventType.StartInterval, false, rci));
                    events.Add(new IntervalEvent(rci.EndDate, EEventType.EndInterval, false, rci));
                }

                events.Sort(CompareIntervals);

                //
                // Create a new list of capacity intervals equalivalent to the original set of inactive intervals except all overlapping intervals have been merged.
                //
                List<ResourceCapacityInterval> capIntervals = new ();

                if (events.Count > 0)
                {
                    IntervalEvent evt = events[0];

                    #if DEBUG
                    #endif

                    // Add the initial RCI which starts at MIN_DATE and ends at the start of the first interval event.
                    if (evt.Ticks != PTDateTime.MinDateTime.Ticks)
                    {
                        ResourceCapacityInterval initialRCI = new (BaseId.NULL_ID, CapacityIntervalDefs.capacityIntervalTypes.Offline, PTDateTime.MinDateTime.Ticks, events[0].Ticks, 1, IntervalProfile.DefaultProfile);
                        capIntervals.Add(initialRCI);
                    }

                    #region Debug must be false: inactiveEvent.EventType == InactiveEventTypeEnum.EndInactiveEvent
                    #if DEBUG
                    if (evt.EventType == EEventType.EndInterval)
                    {
                        throw new Exception("The first event must always be a Start. There must be a bad Resource Capacity Interval above.");
                    }
                    #endif
                    #endregion

                    // process the 0th event.

                    // The Active interval whose start has been reached but its end hasn't.
                    IntervalEvent activeIntervalStartEvt = null;

                    // An Inactive inteval whose start has been reached but its end hasn't.
                    IntervalEvent inactiveIntervalStartEvt = null;

                    if (evt.Active)
                    {
                        activeIntervalStartEvt = evt;
                    }
                    else
                    {
                        inactiveIntervalStartEvt = evt;
                    }

                    for (int i = 1; i < events.Count; ++i) // Starting at index 1 becaue index 0 was processed above.
                    {
                        evt = events[i];

                        // To facilitate processing of events, the event list is sorted by date, subsorted by EventTypeEnum (ends come first), and sub sorted by whether the event is for an Active interval (actives come first).
                        // Processing below matches the sort.
                        // The sort allows ending Active intervals to determine if an RCI should be created. If an Inactive intervals were processed first, the RCI would incorrectly be placed on the list.
                        if (evt.EventType == EEventType.EndInterval)
                        {
                            if (evt.Active)
                            {
                                // The end of an active interval.
                                #if DEBUG
                                if (activeIntervalStartEvt == null)
                                {
                                    throw new Exception("An active interval wasn't in process.");
                                }
                                #endif

                                if (inactiveIntervalStartEvt == null)
                                {
                                    // An Inactive event isn't overridding the Active interval. Create the Active RCI.

                                    // The start isn't necessarily the start of the Active interval since it's possible an overlapping Inactive interval
                                    // may have overridden some of the active interval.
                                    long lastIntEndTicks = capIntervals[capIntervals.Count - 1].EndDate; // The point of possible overlap with an Inactive interval. The overlap can't with an Active interval since they don't overlap.
                                    long intStartTicks = Math.Max(activeIntervalStartEvt.Ticks, lastIntEndTicks);

                                    ResourceCapacityInterval onlineRCI = new (evt.RCI.Id, evt.RCI.IntervalType, intStartTicks, evt.Ticks, evt.RCI.NbrOfPeople, evt.RCI.GetIntervalProfile());
                                    capIntervals.Add(onlineRCI);
                                }

                                // The interval has been completely processed.
                                activeIntervalStartEvt = null;
                            }
                            else // end of Inactive interval
                            {
                                #if DEBUG
                                if (inactiveIntervalStartEvt == null)
                                {
                                    throw new Exception("An inactive interval wasn't in process.");
                                }
                                #endif

                                // Create the RCI for the Inactive interval. There are no special check for creating Inactive intervals since they don't overlap and override Active intervals they
                                // may intersect with.
                                ResourceCapacityInterval rci = new (inactiveIntervalStartEvt.RCI.Id, inactiveIntervalStartEvt.RCI.IntervalType, inactiveIntervalStartEvt.Ticks, evt.Ticks, inactiveIntervalStartEvt.RCI.NbrOfPeople, inactiveIntervalStartEvt.RCI.GetIntervalProfile());
                                capIntervals.Add(rci);

                                // The interval has been completely processed.
                                inactiveIntervalStartEvt = null;
                            }
                        }
                        else // (evt.EventType == EventTypeEnum.Start)
                        {
                            if (evt.Active)
                            {
                                #if DEBUG
                                if (activeIntervalStartEvt != null)
                                {
                                    throw new Exception("Two active intervals can't processing at the same time since the Active intervals passed to this function can't overlap.");
                                }
                                #endif

                                // Set the Active interval being processed.
                                activeIntervalStartEvt = evt;
                            }
                            else // A start of an inactive interval.
                            {
                                #if DEBUG
                                if (inactiveIntervalStartEvt != null)
                                {
                                    throw new Exception("Two inactive intervals can't processing at the same time since the Inactive intervals passed to this function don't overlap.");
                                }
                                #endif
                                // Set the Inactive interval being processed.
                                inactiveIntervalStartEvt = evt;

                                if (activeIntervalStartEvt != null)
                                {
                                    // The Active interval will be cut off at the start of the Inactive interval.
                                    long intStartTicks = Math.Max(activeIntervalStartEvt.Ticks, capIntervals[capIntervals.Count - 1].EndDate);

                                    if (intStartTicks != evt.Ticks) // The Active interval must not have started at the same time as the Inactive Interval.
                                    {
                                        // Insert an Active interval up to the start of the Inactive interval.
                                        ResourceCapacityInterval rci = new (activeIntervalStartEvt.RCI.Id, activeIntervalStartEvt.RCI.IntervalType, intStartTicks, evt.Ticks, activeIntervalStartEvt.RCI.NbrOfPeople, activeIntervalStartEvt.RCI.GetIntervalProfile());
                                        capIntervals.Add(rci);
                                    }
                                }
                            }
                        }
                    }
                }

                if (capIntervals.Count > 0)
                {
                    // Add an offline interval from the last interval up to the end of the planning horizon.
                    long lastIntervalEnd = capIntervals[capIntervals.Count - 1].EndDate;
                    if (lastIntervalEnd < a_planningHorizonEndTicks)
                    {
                        ResourceCapacityInterval rightOffline = new (BaseId.NULL_ID, CapacityIntervalDefs.capacityIntervalTypes.Offline, lastIntervalEnd, a_planningHorizonEndTicks, 1, IntervalProfile.DefaultProfile);
                        capIntervals.Add(rightOffline);
                    }
                }
                else // (resultingActiveIntervals.Count == 0)
                {
                    // There are no capacity intervals. Create a single Offline capacity interval spanning MIN_Date to the end of the planning horizon.
                    capIntervals.Add(new ResourceCapacityInterval(BaseId.NULL_ID, CapacityIntervalDefs.capacityIntervalTypes.Offline, PTDateTime.MinDateTime.Ticks, a_planningHorizonEndTicks, 1, IntervalProfile.DefaultProfile));
                }

                // Add an online interval for all time past the end of the last interval to maximum possible date.
                long finalIntervalEnd = capIntervals[capIntervals.Count - 1].EndDate;
                ResourceCapacityInterval unlimitedOnline = new (BaseId.NULL_ID, CapacityIntervalDefs.capacityIntervalTypes.Online, finalIntervalEnd, PTDateTime.MAX_DATE_TICKS, 1, IntervalProfile.DefaultProfile); //TODO: Find a good way to replace MAX_DATE_TICKS with MaxDateTimeTicks.
                capIntervals.Add(unlimitedOnline);

                // Make the intervals continuous. Fill in any gaps between intervals with an offline interval.
                List<ResourceCapacityInterval> result = new ();

                result.Add(capIntervals[0]);

                for (int i = 1; i < capIntervals.Count; ++i)
                {
                    ResourceCapacityInterval nextRCI = capIntervals[i];
                    ResourceCapacityInterval prevRCI = result[result.Count - 1];

                    if (prevRCI.EndDate != nextRCI.StartDate)
                    {
                        ResourceCapacityInterval extraRCI = new (BaseId.NULL_ID, CapacityIntervalDefs.capacityIntervalTypes.Offline, prevRCI.EndDate, nextRCI.StartDate, nextRCI.GetIntervalProfile());
                        result.Add(extraRCI);
                    }

                    result.Add(nextRCI);
                }

                return result;
            }

            /// <summary>
            /// Sorted by date,
            /// subsorted by EventTypeEnum where End comes first,
            /// subsorted by Active where Active comes first.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            private static int CompareIntervals(IntervalEvent x, IntervalEvent y)
            {
                int comp = Comparer<long>.Default.Compare(x.Ticks, y.Ticks);

                if (comp == 0)
                {
                    int xx = x.EventType == EEventType.EndInterval ? 0 : 1;
                    int yy = y.EventType == EEventType.EndInterval ? 0 : 1;

                    comp = Comparer<int>.Default.Compare(xx, yy);

                    if (comp == 0)
                    {
                        xx = x.Active ? 0 : 1;
                        yy = y.Active ? 0 : 1;

                        comp = Comparer<int>.Default.Compare(xx, yy);
                    }
                }

                return comp;
            }
        }
    }
}