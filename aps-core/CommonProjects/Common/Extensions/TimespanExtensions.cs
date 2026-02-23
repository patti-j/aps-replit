namespace PT.Common.Extensions;

public static class TimespanExtensions
{
    /// <summary>
    /// Constrains the resulting addition to TimeSpan.MaxValue or TimeSpan.MinValue.
    /// This protects from OverflowExceptions
    /// </summary>
    public static TimeSpan AddBound(this TimeSpan a_timespan, TimeSpan a_spanToAdd)
    {
        try
        {
            return a_timespan.Add(a_spanToAdd);
        }
        catch (OverflowException)
        {
            if (a_timespan.Ticks > 0)
            {
                //We can't overflow negative
                return TimeSpan.MaxValue;
            }

            //We must have added two negative timespans that overflowed.
            return TimeSpan.MinValue;
        }
    }

    /// <summary>
    /// Constrains the resulting subtraction to TimeSpan.MaxValue or TimeSpan.MinValue.
    /// This protects from OverflowExceptions
    /// </summary>
    public static TimeSpan SubtractBound(this TimeSpan a_timespan, TimeSpan a_spanToAdd)
    {
        try
        {
            return a_timespan.Subtract(a_spanToAdd);
        }
        catch (OverflowException)
        {
            if (a_timespan.Ticks > 0)
            {
                //We can't overflow positive
                return TimeSpan.MaxValue;
            }

            return TimeSpan.MinValue;
        }
    }
}