using System.Text;

namespace PT.Common;

public class DateTimeHelper
{
    #region Rounding
    public static DateTime RoundMilliseconds(DateTime dt)
    {
        return new DateTime(RoundMilliseconds(dt.Ticks));
    }

    public static long RoundMilliseconds(long dtTicks)
    {
        return Round(dtTicks, TimeSpan.TicksPerMillisecond, TimeSpanHelper.TICKS_PER_HALF_MILLISECOND);
    }

    /// <summary>
    /// Round the date to the nearest second.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static DateTime RoundSeconds(DateTime dt)
    {
        return new DateTime(RoundSeconds(dt.Ticks));
    }

    /// <summary>
    /// Round the date to the nearest second.
    /// </summary>
    /// <param name="dtTicks"></param>
    /// <returns></returns>
    public static long RoundSeconds(long dtTicks)
    {
        return Round(dtTicks, TimeSpan.TicksPerSecond, TimeSpanHelper.TICKS_PER_500_MILLISECONDS);
    }

    /// <summary>
    /// Round the date to the nearest minute.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static DateTime RoundMinutes(DateTime dt)
    {
        return new DateTime(RoundMinutes(dt.Ticks));
    }

    /// <summary>
    /// Round the date to the nearest minute.
    /// </summary>
    /// <param name="dtTicks"></param>
    /// <returns></returns>
    private static long RoundMinutes(long dtTicks)
    {
        return Round(dtTicks, TimeSpan.TicksPerMinute, TimeSpanHelper.TICKS_PER_30_SECONDS);
    }

    /// <summary>
    /// Round to the nearest half hour.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static DateTime RoundToNearest30Minutes(DateTime dt)
    {
        return new DateTime(RoundToNearest30Minutes(dt.Ticks));
    }

    /// <summary>
    /// Round to the nearest half hour.
    /// </summary>
    /// <param name="dtTicks"></param>
    /// <returns></returns>
    public static long RoundToNearest30Minutes(long dtTicks)
    {
        return Round(dtTicks, TimeSpanHelper.TICKS_PER_30_MINUTES, TimeSpanHelper.TICKS_PER_15_MINUTES);
    }

    /// <summary>
    /// Round to the nearest hour.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static DateTime RoundToNearestHour(DateTime dt)
    {
        return new DateTime(RoundToNearestHour(dt.Ticks));
    }

    /// <summary>
    /// Round to the nearest hour.
    /// </summary>
    /// <param name="dtTicks"></param>
    /// <returns></returns>
    public static long RoundToNearestHour(long dtTicks)
    {
        return Round(dtTicks, TimeSpan.TicksPerHour, TimeSpanHelper.TICKS_PER_30_MINUTES);
    }

    /// <summary>
    /// This only works for even intervals.
    /// </summary>
    /// <param name="dtTicks"></param>
    /// <param name="roundInterval">This must be an even number.</param>
    /// <param name="halfInterval">Exactly half of the roundInterval parameter.</param>
    /// <returns></returns>
    private static long Round(long dtTicks, long roundInterval, long halfInterval)
    {
        long m = dtTicks % roundInterval;
        long r;

        if (m >= halfInterval)
        {
            r = dtTicks + roundInterval - m;
        }
        else
        {
            r = dtTicks - m;
        }

        return r;
    }
    #endregion

    /// <summary>
    /// </summary>
    /// <param name="a_d"></param>
    /// <returns></returns>
    public static string ToLongDateTimeString(DateTimeOffset a_d)
    {
        StringBuilder sb = new ();
        sb.Append(a_d.DateTime.ToLongDateString());
        sb.Append(" ");
        sb.Append(a_d.DateTime.ToLongTimeString());
        return sb.ToString();
    }

    [Obsolete("Use GetDisplayTime() in TimeZoneAdjuster")]
    public static DateTime ToLocalTimeFromUTCTicks(long a_utcTicks)
    {
        DateTimeOffset temp = new (a_utcTicks, TimeSpan.Zero);
        return temp.ToDisplayTime().DateTime;
    }

    /// <summary>
    /// Returns the text of a DateTime formatted to show milliseconds.
    /// </summary>
    /// <param name="a_utcTicks"></param>
    /// <returns></returns>
    public static string ToLongLocalTimeFromUTCTicks(long a_utcTicks)
    {
        DateTimeOffset dateTime = new (a_utcTicks, TimeSpan.Zero);
        return dateTime.ToDisplayTime().ToString("MM/dd/yyyy hh:mm:ss.fff tt");
    }

    public static string LocalDate(DateTime a_localDate)
    {
        int year = a_localDate.Year;
        int month = a_localDate.Month;
        int day = a_localDate.Day;
        int hour = a_localDate.Hour;
        int minute = a_localDate.Minute;
        int second = a_localDate.Second;
        int milli = a_localDate.Millisecond;

        return string.Format("{0}/{1}/{2} {3}:{4}:{5}.{6} {7}",
            month,
            day,
            year,
            AMPMConvertHour[hour],
            LengthFormatter(minute, 2),
            LengthFormatter(second, 2),
            milli,
            AMPM[hour]);
    }

    /// <summary>
    /// Contains colons, which aren't compatible with file and directory names.{0}.{1}.{2} {3}:{4}:{5}.{6} {7}
    /// </summary>
    /// <param name="a_localDate"></param>
    /// <returns></returns>
    public static string ToSortableString(DateTime a_localDate)
    {
        int year = a_localDate.Year;
        int month = a_localDate.Month;
        int day = a_localDate.Day;
        int hour = a_localDate.Hour;
        int minute = a_localDate.Minute;
        int second = a_localDate.Second;
        int milli = a_localDate.Millisecond;

        return string.Format("{0}.{1}.{2} {3}:{4}:{5}.{6} {7}",
            LengthFormatter(year, 4),
            LengthFormatter(month, 2),
            LengthFormatter(day, 2),
            LengthFormatter(AMPMConvertHour[hour], 2),
            LengthFormatter(minute, 2),
            LengthFormatter(second, 2),
            LengthFormatter(milli, 3),
            AMPM[hour]);
    }

    public static string ToSortableString(long a_localTicks)
    {
        return ToSortableFileString(new DateTime(a_localTicks));
    }

    /// <summary>
    /// {0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}
    /// </summary>
    /// <param name="a_localDate"></param>
    /// <returns></returns>
    public static string ToSortableFileString(DateTime a_localDate)
    {
        int year = a_localDate.Year;
        int month = a_localDate.Month;
        int day = a_localDate.Day;
        int hour = a_localDate.Hour;
        int minute = a_localDate.Minute;
        int second = a_localDate.Second;
        int milli = a_localDate.Millisecond;

        return string.Format("{0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}",
            LengthFormatter(year, 4),
            LengthFormatter(month, 2),
            LengthFormatter(day, 2),
            LengthFormatter(AMPMConvertHour[hour], 2),
            LengthFormatter(minute, 2),
            LengthFormatter(second, 2),
            LengthFormatter(milli, 3),
            AMPM[hour]);
    }

    private static string LengthFormatter(int a_x, int a_length)
    {
        StringBuilder sbn = new ();
        sbn.Append(a_x);

        int zerosNeeded = a_length - sbn.Length;
        if (zerosNeeded > 0)
        {
            StringBuilder sbz = new ();
            for (int i = 0; i < zerosNeeded; ++i)
            {
                sbz.Append("0");
            }

            sbn.Insert(0, sbz.ToString());
        }

        return sbn.ToString();
    }

    public static string LocalDate(long a_localDate)
    {
        return LocalDate(new DateTime(a_localDate));
    }

    private static readonly int[] AMPMConvertHour = new int[24] { 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

    private static readonly string[] AMPM = new string[24]
    {
        "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "PM", "PM", "PM", "PM", "PM", "PM", "PM", "PM", "PM", "PM", "PM",
        "PM"
    };
    
    /// <summary>
    /// Show the ticks as {Days}.{Hours}:{Minutes}:{Seconds}.
    /// </summary>
    /// <param name="a_timeSpan"></param>
    /// <returns></returns>
    public static string PrintTimeSpan(long a_timeSpan)
    {
        TimeSpan ts = new (a_timeSpan);
        return string.Format("{0}.{1}:{2}:{3}",
            ts.Days.ToString("D2"),
            ts.Hours.ToString("D2"),
            ts.Minutes.ToString("D2"),
            ts.Seconds.ToString("D2"));
    }
}