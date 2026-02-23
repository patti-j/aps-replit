namespace PT.Scheduler;

/// <summary>
/// Stores a list of TimeSpans in contiguous buckets of the same length.
/// </summary>
public class TimeBucketList
{
    /// <param name="start">The time for the first bucket to start.</param>
    /// <param name="endInclusive">The last time that must be included.  The last bucket may run past this time.</param>
    /// <param name="bucketLength">The time between the start and end of each bucket.</param>
    public TimeBucketList(DateTime start, DateTime endInclusive, TimeSpan bucketLength)
    {
        if (endInclusive < start)
        {
            throw new APSCommon.PTValidationException("Can't create a TimeBucketList with end before start.");
        }

        if (bucketLength.Ticks <= 0)
        {
            throw new APSCommon.PTValidationException("Can't create a TimeBucketList with non-positive bucketLength.");
        }

        this.bucketLength = bucketLength.Ticks;
        this.start = start.Ticks;
        TimeSpan duration = endInclusive.Subtract(start);
        count = (long)Math.Ceiling(duration.Ticks / (decimal)bucketLength.Ticks);
        end = this.start + bucketLength.Ticks * count;
        buckets = new long[count];
        bucketOverflows = new long[count];
    }

    private string key;

    /// <summary>
    /// Can be used to identify the list when storing in a TimeBucketListHash.
    /// </summary>
    public string Key
    {
        get => key;
        set => key = value;
    }

    private readonly long[] buckets; //Store ticks that indicate the time stored in the bucket
    private readonly long[] bucketOverflows; //store overflow count for each bucket

    //This is just to view the list in the debugger in readable format.
    private TimeSpan[] BucketTimeSpans
    {
        get
        {
            TimeSpan[] bucketTimeSpans = new TimeSpan[buckets.Length];
            for (int i = 0; i < buckets.Length; i++)
            {
                bucketTimeSpans.SetValue(new TimeSpan((long)buckets.GetValue(i)), i);
            }

            return bucketTimeSpans;
        }
    }

    private readonly long count;

    /// <summary>
    /// The number of buckets.
    /// </summary>
    public long Count => count;

    public TimeSpan this[long idx]
    {
        get
        {
            if (idx >= Count) //This is here because convertint to server time can screw up the bucket counts and the UI end up with an extra 
            {
                return new TimeSpan(0);
            }

            return new TimeSpan(buckets[idx]);
        }
        set => buckets[idx] = value.Ticks;
    }

    private readonly long bucketLength;

    /// <summary>
    /// This is the time between the start and end of each bucket.
    /// </summary>
    public TimeSpan BucketLength => new (bucketLength);

    private readonly long start;

    /// <summary>
    /// The start time of the first bucket.
    /// </summary>
    public DateTime Start => new (start);

    private readonly long end;

    /// <summary>
    /// The end time of the last bucket.
    /// </summary>
    public DateTime End => new (end);

    /// <summary>
    /// Fills the appropriate time buckets using the parameters provided.
    /// </summary>
    /// <param name="intervalStart">The start of the time interval being added.</param>
    /// <param name="intervalEnd">The end of the time interval being added.</param>
    /// <param name="multiplier">The time multiplier used for Capacity Intervals that can have multipliers.</param>
    public void AddTime(DateTime intervalStart, DateTime intervalEnd, decimal nbrPeople)
    {
        if (intervalEnd < Start || intervalStart > End)
        {
            return; //nothing to do
        }

        long intervalsFirstBucket = (long)Math.Floor((intervalStart.Ticks - start) / (decimal)bucketLength);
        if (intervalsFirstBucket < 0)
        {
            AddTime(start, intervalEnd.Ticks, 0, nbrPeople); //truncate everything before the start of bucket 0.
        }
        else
        {
            AddTime(intervalStart.Ticks, intervalEnd.Ticks, intervalsFirstBucket, nbrPeople);
        }
    }

    private void AddTime(long a_intervalStart, long a_intervalEnd, long a_currentBucket, decimal a_nbrPeople)
    {
        long bucketStartTime = start + a_currentBucket * bucketLength;
        long bucketEndTime = bucketStartTime + bucketLength;
        while (true)
        {
            if (a_currentBucket >= buckets.Length)
            {
                break;
            }

            if (a_intervalEnd > bucketEndTime) //interval extends past bucket end so add the overlapping time to the bucket
            {
                buckets[a_currentBucket] += (long)((bucketEndTime - a_intervalStart) * a_nbrPeople);
                a_intervalStart = bucketEndTime;
                bucketStartTime = bucketEndTime;
                bucketEndTime = bucketStartTime + bucketLength;
                a_currentBucket++;
            }
            else
            {
                buckets[a_currentBucket] += (long)((a_intervalEnd - a_intervalStart) * a_nbrPeople);
                break;
            }
        }
    }
    public void AddTime(long a_operationStart, long a_capacityUsed)
    {
        long operationEnd = a_operationStart + a_capacityUsed;

        while (a_operationStart < operationEnd)
        {
            int currentBucket = (int)((a_operationStart - start) / bucketLength); // Calculate the correct bucket index based on the start time

            if (currentBucket >= buckets.Length)
            {
                break; // Exit if we've exhausted all buckets
            }

            long bucketStartTime = start + (currentBucket * bucketLength);
            long bucketEndTime = bucketStartTime + bucketLength;

            if (operationEnd > bucketEndTime)
            {
                // The operation spans the current bucket, so add only the portion that fits in this bucket
                long overlapDuration = bucketEndTime - a_operationStart;
                buckets[currentBucket] += overlapDuration;

                // Move the operation's start to the next bucket's start
                a_operationStart = bucketEndTime;
            }
            else
            {
                // The operation fits completely within the current bucket
                long overlapDuration = operationEnd - a_operationStart;
                buckets[currentBucket] += overlapDuration;
                break; // Operation is fully allocated, no need to continue
            }
        }
    }
    /// <summary>
    /// Add the buckets from the listToAbsorb into the existing TimeBucketList.
    /// </summary>
    /// <param name="listToAbsorb"></param>
    public void Absorb(TimeBucketList listToAbsorb)
    {
        int nbrBucketsToAbsorb = (int)Math.Min(Count, listToAbsorb.Count);
        for (int i = 0; i < nbrBucketsToAbsorb; i++)
        {
            long newVal = Math.Min(this[i].Ticks + listToAbsorb[i].Ticks, TimeSpan.MaxValue.Ticks); //prevent overflows
            this[i] = TimeSpan.FromTicks(newVal);
        }
    }

    //Keeps track of the overflow count and value. This is used to the timespans don't overflow during productionUnits calculation
    public readonly long OverflowValue = new TimeSpan(1000, 0, 0).Ticks;

    public void AddOverflowCount(long a_index, int a_count = 1)
    {
        bucketOverflows[a_index] = bucketOverflows[a_index] + a_count;
    }

    public long GetOverflowCount(long a_index)
    {
        return bucketOverflows[a_index];
    }
}