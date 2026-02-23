namespace PT.Scheduler;

/// <summary>
/// Used by Max delay to track and identify timespans where blocks are scheduled and blocks have been planned to be scheduled by max delay.
/// </summary>
internal abstract class ResourceSpan
{
    internal ResourceSpan(object a_reason, long a_start, long a_end)
    {
        Reason = a_reason;
        Start = a_start;
        End = a_end;
    }

    /// <summary>
    /// The start of the timespan.
    /// </summary>
    internal long Start { get; set; }

    /// <summary>
    /// The end of the timespan.
    /// </summary>
    internal long End { get; set; }

    /// <summary>
    /// The reason the timespan is taken or reserved. The type of this object can vary from one ResourceSpan to the next.
    /// </summary>
    internal object Reason { get; set; }

    public override string ToString()
    {
        string start = DateTimeHelper.ToLocalTimeFromUTCTicks(Start).ToString();
        string end = DateTimeHelper.ToLocalTimeFromUTCTicks(End).ToString();
        return string.Format("{0} to {1}", start, end);
    }
}