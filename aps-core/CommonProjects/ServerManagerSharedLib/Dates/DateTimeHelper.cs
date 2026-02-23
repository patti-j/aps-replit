using System;
using System.Text;

namespace PT.ServerManagerSharedLib.Dates
{
    public class DateTimeHelper
    {
        #region Rounding
        public static System.DateTime RoundMilliseconds(System.DateTime dt)
        {
            return new System.DateTime(RoundMilliseconds(dt.Ticks));
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
        public static System.DateTime RoundSeconds(System.DateTime dt)
        {
            return new System.DateTime(RoundSeconds(dt.Ticks));
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
        public static System.DateTime RoundMinutes(System.DateTime dt)
        {
            return new System.DateTime(RoundMinutes(dt.Ticks));
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
        public static System.DateTime RoundToNearest30Minutes(System.DateTime dt)
        {
            return new System.DateTime(RoundToNearest30Minutes(dt.Ticks));
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
        public static System.DateTime RoundToNearestHour(System.DateTime dt)
        {
            return new System.DateTime(RoundToNearestHour(dt.Ticks));
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
        /// 
        /// </summary>
        /// <param name="a_d"></param>
        /// <returns></returns>
        public static string ToLongDateTimeString(System.DateTime a_d)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(a_d.ToLongDateString());
            sb.Append(" ");
            sb.Append(a_d.ToLongTimeString());
            return sb.ToString();
        }

        /// <summary>
        /// Keeps the year, month, day, hour and minute components when creating a new DateTime. That is, the seconds are truncated.
        /// </summary>
        /// <param name="a_originalDt"></param>
        /// <returns></returns>
        public static System.DateTime RemoveSeconds(System.DateTime a_originalDt)
        {
            return new System.DateTime(a_originalDt.Year, a_originalDt.Month, a_originalDt.Day, a_originalDt.Hour, a_originalDt.Minute, 0);
        }

        public static string LocalDate(System.DateTime a_localDate)
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
        public static string ToSortableString(System.DateTime a_localDate)
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
            return ToSortableFileString(new System.DateTime(a_localTicks));
        }

        /// <summary>
        /// {0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}
        /// </summary>
        /// <param name="a_localDate"></param>
        /// <returns></returns>
        public static string ToSortableFileString(System.DateTime a_localDate)
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
            StringBuilder sbn = new StringBuilder();
            sbn.Append(a_x);

            int zerosNeeded = a_length - sbn.Length;
            if (zerosNeeded > 0)
            {
                StringBuilder sbz = new StringBuilder();
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
            return LocalDate(new System.DateTime(a_localDate));
        }

        private static readonly int[] AMPMConvertHour = new int[24] {12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};

        private static readonly string[] AMPM = new string[24]
        {
            "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "PM", "PM", "PM", "PM", "PM", "PM", "PM", "PM", "PM", "PM", "PM",
            "PM"
        };

        /// <summary>
        /// Display a UTC date in local time.
        /// </summary>
        /// <param name="a_ticks"></param>
        /// <returns></returns>
        [Obsolete("Use TimeZoneAdjuster class instead")]
        public static string UTCDate(long a_ticks)
        {
            System.DateTime date = new System.DateTime(a_ticks);
            return DateTimeHelper.LocalDate(date.ToLocalTime());
        }

        /// <summary>
        /// Display a UTC date in local time.
        /// </summary>
        /// <param name="a_utcDate"></param>
        /// <returns></returns>
        [Obsolete("Remove this function. Use TimeZoneAdjuster class instead")]
        public static string UTCDate(System.DateTime a_utcDate)
        {
            return DateTimeHelper.LocalDate(a_utcDate.ToLocalTime());
        }

        /// <summary>
        /// Show the ticks as a TimeSpan.ToString().
        /// </summary>
        /// <param name="a_ticks"></param>
        /// <returns></returns>
        public static string ShowTimeSpan(long a_ticks)
        {
            return new TimeSpan(a_ticks).ToString();
        }

        /// <summary>
        /// Show the ticks as {Days}.{Hours}:{Minutes}:{Seconds}.
        /// </summary>
        /// <param name="a_timeSpan"></param>
        /// <returns></returns>
        public static string PrintTimeSpan(long a_timeSpan)
        {
            TimeSpan ts = new TimeSpan(a_timeSpan);
            return string.Format("{0}.{1}:{2}:{3}", ts.Days.ToString("D2"), ts.Hours.ToString("D2"), ts.Minutes.ToString("D2"),
                ts.Seconds.ToString("D2"));
        }
    }
}