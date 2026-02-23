using System.Diagnostics;
using System.Globalization;

using PT.Common.Debugging;

namespace PT.Common;

//PTDateTime is a class that contains static methods for dealing with DateTime objects.
//It is used to ensure that all DateTime objects are in the same format and to provide
//a central location for any DateTime related functions.
public static class PTDateTime
{
    private static readonly CultureInfo s_enUsCultureInfo = new ("en-US");
    public static readonly DateTimeOffset MaxValue = new (9000, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset MinValue = new (1800, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public static readonly DateTime MaxDateTime = new (9000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    public static readonly DateTime MaxDisplayDateTime = new (3000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    public static readonly DateTime MinDateTime = new (1800, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    public static readonly DateTime MinDisplayDateTime = new (1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    public static readonly DateTime InvalidDateTime = DateTime.MinValue;
    public static readonly long MaxDateTimeTicks = MaxDateTime.Ticks;
    public static readonly long MinDateTimeTicks = MinDateTime.Ticks;
    public static readonly long InvalidDateTimeTicks = InvalidDateTime.Ticks;

    /// <summary>
    /// DateTime.MaxValue.Ticks. Though this value should be used in case the MaxValue is changed in the future.
    /// </summary>
    [Obsolete("Use MaxDateTimeTicks instead. Note that we need to validate this doesn't change scheduling results")]
    public static readonly long MAX_DATE_TICKS = 3155378975999999999; // DateTime.MaxValue; 12/31/9999 11:59:59 PM

    /// <summary>
    /// Equal to SqlServerMinDate.Ticks
    /// DateTime dt = new DateTime(1753, 1, 1, 12, 0, 0);
    /// </summary>
    [Obsolete("Transition to MinDateTime or MinDateTimeTicks instead")]
    public static readonly long MinDateTicks = 552878352000000000;

    /// <summary>
    /// DateTime.MaxValue.Ticks
    /// </summary>
    [Obsolete("Transition to MaxDateTime or MaxDateTimeTicks instead")]
    public static readonly long MaxDateTicks = 3155378975999999999;

    public static TimeSpan GetSafeTimeSpan(double a_hrs)
    {
        if (a_hrs >= TimeSpan.MaxValue.TotalHours)
        {
            return TimeSpan.MaxValue;
        }

        return TimeSpan.FromHours(a_hrs);
    }

    public static TimeSpan GetSafeTimeSpan(decimal a_hrs)
    {
        return GetSafeTimeSpan(Convert.ToDouble(a_hrs));
    }

    public static void ValidateDateTime(long a_dt)
    {
        if (a_dt > MaxDateTimeTicks)
        {
            throw new CommonException("4042");
        }

        if (a_dt < MinDateTimeTicks)
        {
            throw new CommonException("4043");
        }
    }

    /// <summary>
    /// Returns whether the date time is a valid PT time
    /// </summary>
    public static bool IsValidDateTime(long a_dt)
    {
        return a_dt >= MinDateTimeTicks && a_dt <= MaxDateTimeTicks;
    }

    /// <summary>
    /// Returns whether the date time is a valid PT time
    /// </summary>
    public static bool IsValidDateTime(DateTime a_dt)
    {
        return IsValidDateTime(a_dt.Ticks);
    }

    /// <summary>
    /// Returns whether the date time is valid and also greater than the minimum and less than the maximum
    /// </summary>
    public static bool IsValidDateTimeBetweenMinMax(long a_dt)
    {
        return a_dt > MinDateTimeTicks && a_dt < MaxDateTimeTicks;
    }

    /// <summary>
    /// Returns whether the date time is valid and also greater than the minimum and less than the maximum
    /// </summary>
    public static bool IsValidDateTimeBetweenMinMax(DateTime a_dt)
    {
        return IsValidDateTimeBetweenMinMax(a_dt.Ticks);
    }

    /// <summary>
    /// Returns whether the date time is an actual value and not a min/max used as a place holder
    /// </summary>
    public static bool IsValidDateTimeForDisplay(DateTime a_dt)
    {
        return a_dt > MinDisplayDateTime && a_dt < MaxDisplayDateTime;
    }

    /// <summary>
    /// The maximum date that the Infragistics date picker.
    /// </summary>
    public static DateTimeOffset DatePickerMaxDate => new (9998, 12, 31, 0, 0, 0, TimeZoneAdjuster.CurrentTimeZoneInfo.GetUtcOffset(MaxDateTime));

    /// <summary>
    /// Minimum date to use in Gantt. Not sure what limitactually is.
    /// </summary>
    public static DateTimeOffset GanttMinDate => new (1900, 1, 1, 0, 0, 0, TimeZoneAdjuster.CurrentTimeZoneInfo.GetUtcOffset(MinDateTime));

    /// <summary>
    /// Maximum date to use in Gantt. Not sure what limitactually is.
    /// </summary>
    public static DateTimeOffset GanttMaxDate => new (3000, 12, 31, 0, 0, 0, TimeZoneAdjuster.CurrentTimeZoneInfo.GetUtcOffset(MaxDateTime));

    public static DateTime GetValidDateTime(DateTime a_dt)
    {
        if (a_dt.Ticks < MinDateTimeTicks)
        {
            return MinDateTime;
        }

        if (a_dt.Ticks > MaxDateTimeTicks)
        {
            return MaxDateTime;
        }

        return a_dt;
    }

    public static long GetValidDateTime(long a_ticks)
    {
        if (a_ticks < MinDateTimeTicks)
        {
            return MinDateTimeTicks;
        }

        if (a_ticks > MaxDateTimeTicks)
        {
            return MaxDateTimeTicks;
        }

        return a_ticks;
    }

    public static TimeSpan Max(TimeSpan a_1, TimeSpan a_2)
    {
        return new TimeSpan(Math.Max(a_1.Ticks, a_2.Ticks));
    }

    public static TimeSpan Min(TimeSpan a_1, TimeSpan a_2)
    {
        return new TimeSpan(Math.Min(a_1.Ticks, a_2.Ticks));
    }

    public static DateTime Max(DateTime a_1, DateTime a_2)
    {
        return new DateTime(Math.Max(a_1.Ticks, a_2.Ticks));
    }

    public static DateTimeOffset Max(DateTimeOffset a_1, DateTimeOffset a_2)
    {
        return a_1 >= a_2 ? a_1 : a_2;
    }

    public static DateTime Min(DateTime a_1, DateTime a_2)
    {
        return new DateTime(Math.Min(a_1.Ticks, a_2.Ticks));
    }

    public static DateTimeOffset Min(DateTimeOffset a_1, DateTimeOffset a_2)
    {
        return a_1 <= a_2 ? a_1 : a_2;
    }

    /// <summary>
    /// The maximum days that can be represented as a duration.
    /// </summary>
    public static decimal MaxValueForDays => Convert.ToDecimal(TimeSpan.MaxValue.TotalDays - 1);

    public static TimeSpan ReasonableMaxDuration = TimeSpan.FromDays(365 * 10);

    [Conditional("DEBUG")]
    public static void ValidateUtc(DateTimeOffset a_value)
    {
        if (a_value.Offset != TimeSpan.Zero)
        {
            throw new DebugException("UTC date being set with non UTC datetimeoffset");
        }
    }

    public static DateTime ToDateTime(this DateTimeOffset a_dateTime)
    {
        return a_dateTime.DateTime;
    }

    /// <summary>
    /// Converts a UTC or Server time into the DateTimeOffset equivalent with 0 offset.
    /// The time value is not changed
    /// </summary>
    public static DateTimeOffset ToServerOffset(this DateTime a_dateTime)
    {
        return new DateTimeOffset(a_dateTime.Ticks, TimeSpan.Zero);
    }

    /// <summary>
    /// Converts a DateTime into a DateTimeOffset with the specified offset. Use this to specify the offset of a known datetime
    /// The DateTime value is not changed, only the offset is set
    /// </summary>
    public static DateTimeOffset ToDateTimeOffset(this DateTime a_dateTime, TimeSpan a_offset)
    {
        return new DateTimeOffset(a_dateTime.Ticks, a_offset);
    }

    /// <summary>
    /// Converts a DateTime into a DateTimeOffset with the offset of the specified timezone. Use this to specify the offset of a known datetime
    /// The DateTime value is not changed, only the offset is set
    /// </summary>
    public static DateTimeOffset ToDateTimeOffset(this DateTime a_dateTime, TimeZoneInfo a_timeZoneInfo)
    {
        return new DateTimeOffset(a_dateTime.Ticks, a_timeZoneInfo.GetUtcOffset(a_dateTime));
    }

    /// <summary>
    /// The user's DateTimeNow. This is set according to the user's timezone preference. 
    /// </summary>
    public static DateTimeOffset UserDateTimeNow
    {
        get
        {
            DateTimeOffset utcNow = new (DateTime.UtcNow, TimeSpan.Zero);
            return utcNow.ToDisplayTime();
        }
    }

    public static DateTimeOffset LocalDateTimeNow => DateTimeOffset.Now;

    public static DateTimeOffset UtcNow => new (DateTime.UtcNow, TimeSpan.Zero);

    /// <summary>
    /// Keeps the year, month, day, hour and minute components when creating a new DateTime. That is, the seconds are truncated.
    /// </summary>
    /// <param name="a_originalDt"></param>
    /// <returns></returns>
    public static DateTimeOffset RemoveSeconds(this DateTimeOffset a_originalDt)
    {
        return new DateTimeOffset(a_originalDt.Year, a_originalDt.Month, a_originalDt.Day, a_originalDt.Hour, a_originalDt.Minute, 0, a_originalDt.Offset);
    }

    /// <summary>
    /// Keeps the year, month, day, hour and minute components when creating a new DateTime. That is, the seconds are truncated.
    /// </summary>
    /// <param name="a_originalDt"></param>
    /// <returns></returns>
    public static DateTime RemoveSeconds(this DateTime a_originalDt)
    {
        return new DateTime(a_originalDt.Year, a_originalDt.Month, a_originalDt.Day, a_originalDt.Hour, a_originalDt.Minute, 0, a_originalDt.Kind);
    }

    public static string ToShortDateString(this DateTimeOffset a_date)
    {
        return a_date.ToDateTime().ToShortDateString();
    }

    public static string ToShortTimeString(this DateTimeOffset a_date)
    {
        return a_date.ToDateTime().ToShortTimeString();
    }

    public static string ToLongDateString(this DateTimeOffset a_date)
    {
        return a_date.ToDateTime().ToLongDateString();
    }

    public static DateTimeOffset ToDateNoTime(this DateTimeOffset a_dateTime)
    {
        return new DateTimeOffset(a_dateTime.Year, a_dateTime.Month, a_dateTime.Day, 0, 0, 0, a_dateTime.Offset);
    }

    public static DateTime GetSafeDateTime(DateTime a_dateTime)
    {
        if (a_dateTime > MaxDateTime)
        {
            return MaxDateTime;
        }

        if (a_dateTime < MinDateTime)
        {
            return MinDateTime;
        }

        return a_dateTime;
    }

    public static string ToStringUniversalFormatUsCulture(this DateTime a_dateTime)
    {
        return a_dateTime.ToString("u", s_enUsCultureInfo);
    }

    public static string ToStringUniversalFormatUsCulture(this DateTimeOffset a_dateTimeOffset)
    {
        return a_dateTimeOffset.ToString("u", s_enUsCultureInfo);
    }
}