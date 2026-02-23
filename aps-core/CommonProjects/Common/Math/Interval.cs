using System.Diagnostics;

namespace PT.Common.PTMath;

public interface IInterval
{
    long StartTicks { get; }
    long EndTicks { get; }
    bool Intersection(long a_start, long a_end);
    bool Intersection(IInterval a_interval);
    
    long Duration => EndTicks - StartTicks;
}

public class Interval : IInterval
{
    public long StartTicks { get; set; }
    public long EndTicks { get; set; }

    public Interval() { }

    public Interval(long a_start, long a_end)
    {
        StartTicks = a_start;
        EndTicks = a_end;
    }

    public bool Intersection(long a_start, long a_end)
    {
        ValidateInterval(a_start, a_end);

        //Find the intersecting interval, if there is one.
        if (a_end < StartTicks || a_start > EndTicks)
        {
            return false;
        }

        long largestStart = StartTicks > a_start ? StartTicks : a_start; // The largest start of the 2 intervals.
        long smallestEnd = EndTicks < a_end ? EndTicks : a_end; // The smallest end of the 2 intervals.
        return largestStart < smallestEnd;
    }

    public bool Intersection(IInterval a_interval)
    {
        ValidateInterval(a_interval);

        return Intersection(a_interval.StartTicks, a_interval.EndTicks);
    }

    /// <summary>
    /// Find the intersection between 2 intervals. The return value indicate whether there was intersection. If false
    /// the "out" values are no good.
    /// </summary>
    /// <param name="a_i1Start">The start of the first interval.</param>
    /// <param name="a_i1End">The end of the first interval.</param>
    /// <param name="a_i2Start">The start of the second interval.</param>
    /// <param name="a_i2End">The end of the second interval.</param>
    /// <param name="o_start">The start of the intersection.</param>
    /// <param name="o_end">The end of the intersection.</param>
    /// <returns>Whether the intervals intersect.</returns>
    public static bool Intersection(long a_i1Start, long a_i1End, long a_i2Start, long a_i2End, out long o_start, out long o_end)
    {
        ValidateInterval(a_i1Start, a_i1End);
        ValidateInterval(a_i2Start, a_i2End);

        //Find the intersecting interval, if there is one.
        if (a_i2Start > a_i1End || a_i1Start > a_i2End)
        {
            o_start = 0;
            o_end = 0;
            return false;
        }

        o_start = a_i1Start > a_i2Start ? a_i1Start : a_i2Start; // The largest start of the 2 intervals.
        o_end = a_i1End < a_i2End ? a_i1End : a_i2End; // The smallest end of the 2 intervals.
        return o_start < o_end;
    }

    /// <summary>
    /// Whether 2 intervals intersect. This isn't working if the 2nd interval starts past the first interval. This needs to be fixed.
    /// </summary>
    /// <param name="a_i1Start">The start of the first interval.</param>
    /// <param name="a_i1End">The end of the first interval.</param>
    /// <param name="a_i2Start">The start of the second interval.</param>
    /// <param name="a_i2End">The end of the second interval.</param>
    /// <returns>Whether the intervals intersect.</returns>
    public static bool Intersection(long a_i1Start, long a_i1End, long a_i2Start, long a_i2End)
    {
        ValidateInterval(a_i1Start, a_i1End);
        ValidateInterval(a_i2Start, a_i2End);

        long startIntersection;
        long endIntersection;
        return Intersection(a_i1Start, a_i1End, a_i2Start, a_i2End, out startIntersection, out endIntersection);
    }

    /// <summary>
    /// Whether an interval contains a date.
    /// </summary>
    public static bool Contains(long a_intervalStart, long a_intervalEnd, long a_date)
    {
        ValidateInterval(a_intervalStart, a_intervalEnd);
        return a_date >= a_intervalStart && a_date < a_intervalEnd;
    }

    public static bool GreaterOrEqual(long a_intervalStart, long a_intervalEnd, long a_date)
    {
        ValidateInterval(a_intervalStart, a_intervalEnd);
        return a_intervalStart >= a_date;
    }

    [Conditional("DEBUG")]
    public static void ValidateInterval(long a_start, long a_end)
    {
        if (a_start > a_end)
        {
            throw new Exception("The interval is invalid.");
        }
    }

    public static void ValidateInterval(IInterval a_interval)
    {
        if (a_interval.StartTicks > a_interval.EndTicks)
        {
            throw new Exception("The interval is invalid.");
        }
    }

    public override string ToString()
    {
        return $"{new DateTime(StartTicks)} - {new DateTime(EndTicks)}";
    }
}