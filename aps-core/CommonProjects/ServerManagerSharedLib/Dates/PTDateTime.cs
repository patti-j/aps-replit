using System;

namespace PT.ServerManagerSharedLib.Dates
{
    /// <summary>
    /// Static DateTime values
    /// TODO: Move APS related properties to APSCommon
    /// </summary>
    public static class PTDateTime
    {
        public static readonly System.DateTime MaxDateTime = new System.DateTime(9000, 1, 1);
        public static readonly System.DateTime MaxDisplayDateTime = new System.DateTime(3000, 1, 1);
        public static readonly System.DateTime MinDateTime = new System.DateTime(1800, 1, 1);
        public static readonly System.DateTime MinDisplayDateTime = new System.DateTime(1900, 1, 1);
        public static readonly long MaxDateTimeTicks = MaxDateTime.Ticks;
        public static readonly long MinDateTimeTicks = MinDateTime.Ticks;
        public static readonly System.DateTime InvalidDateTime = System.DateTime.MinValue;

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

        public static System.DateTime Now
        {
            get { return System.DateTime.Now; }
        }

        /// <summary>
        /// Return DateTime's Now without any seconds.
        /// </summary>
        public static System.DateTime NowNoSeconds
        {
            get { return DateTimeHelper.RemoveSeconds(System.DateTime.Now); }
        }

        public static System.DateTime UtcNow
        {
            get { return System.DateTime.UtcNow; }
        }

        /// <summary>
        /// Return Utc.Now with the seconds truncated.
        /// </summary>
        public static System.DateTime UtcNowNoSeconds
        {
            get { return DateTimeHelper.RemoveSeconds(System.DateTime.UtcNow); }
        }

        /// <summary>
        /// Return the current year based on DateTime's UtcNow.
        /// </summary>
        public static int UTCYear
        {
            get { return System.DateTime.UtcNow.Year; }
        }
        public static TimeSpan GetSafeTimeSpan(double a_hrs)
        {
            if (a_hrs >= TimeSpan.MaxValue.TotalHours)
            {
                return TimeSpan.MaxValue;
            }
            else
            {
                 return TimeSpan.FromHours(a_hrs);
            }
        }

        public static TimeSpan GetSafeTimeSpan(decimal a_hrs)
        {
            return GetSafeTimeSpan(Convert.ToDouble(a_hrs));
        }

        public static void ValidateDateTime(long a_dt)
        {
            if (a_dt > MaxDateTimeTicks)
            {
                throw new Exception("4042");
            }

            if (a_dt < MinDateTimeTicks)
            {
                throw new Exception("4043");
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
        public static bool IsValidDateTime(System.DateTime a_dt)
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
        public static bool IsValidDateTimeBetweenMinMax(System.DateTime a_dt)
        {
            return IsValidDateTimeBetweenMinMax(a_dt.Ticks);
        }

        /// <summary>
        /// Returns whether the date time is an actual value and not a min/max used as a place holder
        /// </summary>
        public static bool IsValidDateTimeForDisplay(System.DateTime a_dt)
        {
            return a_dt > MinDisplayDateTime && a_dt < MaxDisplayDateTime;
        }

        /// <summary>
        /// The maximum date that the Infragistics date picker.
        /// </summary>
        public static System.DateTime DatePickerMaxDate
        {
            get { return new System.DateTime(9998, 12, 31); }
        }

        /// <summary>
        /// Minimum date to use in Gantt. Not sure what limitactually is.
        /// </summary>
        public static System.DateTime GanttMinDate
        {
            get { return new System.DateTime(1900, 1, 1); }
        }

        /// <summary>
        /// Maximum date to use in Gantt. Not sure what limitactually is.
        /// </summary>
        public static System.DateTime GanttMaxDate
        {
            get { return new System.DateTime(3000, 12, 31); }
        }

        public static System.DateTime GetValidDateTime(System.DateTime a_dt)
        {
            if (a_dt.Ticks < MinDateTimeTicks)
            {
                return MinDateTime;
            }
            else if (a_dt.Ticks > MaxDateTimeTicks)
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
            else if (a_ticks > MaxDateTimeTicks)
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

        public static System.DateTime Max(System.DateTime a_1, System.DateTime a_2)
        {
            return new System.DateTime(Math.Max(a_1.Ticks, a_2.Ticks));
        }

        public static System.DateTime Min(System.DateTime a_1, System.DateTime a_2)
        {
            return new System.DateTime(Math.Min(a_1.Ticks, a_2.Ticks));
        }

        /// <summary>
        /// The maximum days that can be represented as a duration.
        /// </summary>
        public static decimal MaxValueForDays
        {
            get { return Convert.ToDecimal(TimeSpan.MaxValue.TotalDays - 1); }
        }

        public static TimeSpan ReasonableMaxDuration = TimeSpan.FromDays(365 * 10);
    }
}