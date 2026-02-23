using PT.Common.Collections;
using PT.Common.Range;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for CapacityProfile.
/// </summary>
public partial class CapacityProfile : IPTSerializable
{
    public const int UNIQUE_ID = 322;

    #region IPTSerializable Members
    public CapacityProfile(IReader reader, ResourceCapacityIntervalsCollection resourceCapacityIntervals)
    {
        m_rawIntervals = resourceCapacityIntervals;

        if (reader.VersionNumber >= 497)
        {
            // This is no longer being serialized.
        }
        else if (reader.VersionNumber >= 1)
        {
            m_profileIntervals = new ResourceCapacityIntervalsCollection(reader);
        }
    }

    public void Serialize(IWriter writer) { }

    public int UniqueId => UNIQUE_ID;
    #endregion

    private readonly ResourceCapacityIntervalsCollection m_rawIntervals; //Before making continuous and non-overlapping

    public CapacityProfile(ResourceCapacityIntervalsCollection resourceCapacityIntervals)
    {
        m_rawIntervals = resourceCapacityIntervals;
    }

    private readonly ResourceCapacityIntervalsCollection m_profileIntervals = new (); //Continuous and non-overlapping

    /// <summary>
    /// This is the final resulting list of continuous, non-overlapping ResourceCapacityIntervals that span
    /// from the minDate to maxDate.
    /// </summary>
    internal ResourceCapacityIntervalsCollection ProfileIntervals => m_profileIntervals;

    internal void PurgeIntervalsEndingBeforeClock(long clock)
    {
        for (int i = ProfileIntervals.Count - 1; i >= 0; i--)
        {
            ResourceCapacityInterval rci = ProfileIntervals[i];
            if (rci.EndDate < clock)
            {
                ProfileIntervals.Remove(i);
            }
        }

        for (int r = m_rawIntervals.Count - 1; r >= 0; r--)
        {
            ResourceCapacityInterval rci = m_rawIntervals[r];
            if (rci.EndDate < clock)
            {
                m_rawIntervals.Remove(r);
            }
        }
    }

    /// <param name="a_start">The time for the first bucket to start.</param>
    /// <param name="a_endInclusive">The last time that must be included.  The last bucket may run past this time.</param>
    /// <param name="a_bucketLength">The time between the start and end of each bucket.</param>
    internal TimeBucketList GetBucketedCapacity(DateTime a_start, DateTime a_endInclusive, TimeSpan a_bucketLength)
    {
        TimeBucketList capacityBuckets = new (a_start, a_endInclusive, a_bucketLength);
        for (int i = 0; i < m_profileIntervals.Count; i++)
        {
            ResourceCapacityInterval rci = m_profileIntervals[i];
            if (rci.Active)
            {
                capacityBuckets.AddTime(rci.StartDateTime, rci.EndDateTime, rci.NbrOfPeople);
            }
        }

        return capacityBuckets;
    }

    public DateTime GetAdjustedDateTime(DateTime a_clockDate, DateTime a_adjustedDateTime, TimeSpan a_headStartSpan)
    {
        DateTime adjustedDate = a_adjustedDateTime;
        if (m_profileIntervals.Count == 0)
        {
            //There is no capacity on this resource
            return a_adjustedDateTime;
        }

        //Get collection of online capacity intervals from m_profileIntervals
        CapacityIntervalCollection capIntervals = new (m_profileIntervals.Count);
        for (int i = 0; i < m_profileIntervals.Count; i++)
        {
            ResourceCapacityInterval ci = m_profileIntervals[i];
            if (ci.Active)
            {
                capIntervals.Add(ci);
            }
        }

        //Find our current shift from a_adjustedDateTime
        ISearchableRange searchableRange = GenericRangeSearch.FindRange(capIntervals, a_adjustedDateTime);
        ResourceCapacityInterval currentShift = (ResourceCapacityInterval)searchableRange;
        int currentIntervalIdx = capIntervals.IndexOf(searchableRange);
        if (currentShift == null)
        {
            currentShift = m_profileIntervals.FindFirstOnlineAtOrBefore(a_adjustedDateTime);
            if (currentShift == null)
            {
                //no more capacity
                return a_adjustedDateTime;
            }
        }

        //It's possible for searchable range to be null in some contexts, if so, we need to find searchableRange from our currentRange.start instead of a_adjustedTime
        if (searchableRange == null)
        {
            searchableRange = GenericRangeSearch.FindRange(capIntervals, currentShift.Start);
        }

        TimeSpan remainingHeadStartSpan = a_headStartSpan;

        //If the start of our searchableRange + the remaining headstart span is less than the adjustedDate then we will return adjustedDate - the remaining headstart span.
        if (searchableRange.Start.Add(remainingHeadStartSpan) <= adjustedDate)
        {
            return adjustedDate.Subtract(remainingHeadStartSpan);
        }

        //Loop backwards through our shifts and subtract enough time from the remaining headstart span until we are able to schedule at the appropriate time
        remainingHeadStartSpan -= TimeSpan.FromHours((adjustedDate - searchableRange.Start).TotalHours);
        while (currentIntervalIdx > 0)
        {
            currentIntervalIdx -= 1;
            ISearchableRange prevRange = capIntervals.GetByIdx(currentIntervalIdx);
            currentShift = (ResourceCapacityInterval)prevRange;
            if (prevRange.Start.Add(remainingHeadStartSpan) <= prevRange.End)
            {
                return prevRange.End.Subtract(remainingHeadStartSpan);
            }

            remainingHeadStartSpan -= prevRange.End - prevRange.Start;
        }

        //Return the clockdate if we are unable to find a spot while in the loop
        return a_clockDate;
    }
}