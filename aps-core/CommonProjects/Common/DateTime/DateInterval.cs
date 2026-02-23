namespace PT.Common;

public class DateInterval
{
    public enum EInterval
    {
        /// <summary>
        /// 90 days
        /// 4 buckets/year
        /// </summary>
        Quarter,

        /// <summary>
        /// 30 days
        /// 12 buckets/year
        /// </summary>
        Month,

        /// <summary>
        /// 7 days
        /// 52 buckets/year
        /// </summary>
        Week,

        /// <summary>
        /// 365 / year
        /// </summary>
        Day
    }

    public static TimeSpan GetIntervalTimeSpan(EInterval a_period)
    {
        if (a_period == EInterval.Quarter)
        {
            return TimeSpan.FromDays(90);
        }

        if (a_period == EInterval.Month)
        {
            return TimeSpan.FromDays(30);
        }

        if (a_period == EInterval.Week)
        {
            return TimeSpan.FromDays(7);
        }

        // daily
        return TimeSpan.FromDays(1);
    }

    public static int GetIntervalCountPerYear(EInterval a_frequency)
    {
        if (a_frequency == EInterval.Quarter)
        {
            return 4;
        }

        if (a_frequency == EInterval.Month)
        {
            return 12;
        }

        if (a_frequency == EInterval.Week)
        {
            return 52;
        }

        // day
        return 365;
    }

    /// <summary>
    /// An interval breaks a year into buckets (4 buckets for Interval=Quarter). Given a DateTime This function returns which
    /// bucket the date falls into.
    /// </summary>
    /// <param name="a_datetime"></param>
    /// <param name="a_interval"></param>
    /// <returns></returns>
    public static int GetBucketIndex(DateTime a_datetime, EInterval a_interval)
    {
        if (a_interval == EInterval.Quarter)
        {
            if (a_datetime.Month <= 3)
            {
                return 0;
            }

            if (a_datetime.Month <= 6)
            {
                return 1;
            }

            if (a_datetime.Month <= 9)
            {
                return 2;
            }

            return 3;
        }

        if (a_interval == EInterval.Month)
        {
            return a_datetime.Month - 1;
        }

        if (a_interval == EInterval.Week)
        {
            return GetWeekOfYear(a_datetime);
        }

        return Math.Min(a_datetime.DayOfYear - 1, 364);
    }

    /// <summary>
    /// returns
    /// </summary>
    /// <param name="a_datetime"></param>
    /// <returns></returns>
    private static int GetWeekOfYear(DateTime a_datetime)
    {
        System.Globalization.DateTimeFormatInfo dateTimeInfo = new ();
        return dateTimeInfo.Calendar.GetWeekOfYear(a_datetime, System.Globalization.CalendarWeekRule.FirstDay, dateTimeInfo.FirstDayOfWeek);
    }
}