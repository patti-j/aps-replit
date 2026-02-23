using System;

namespace PT.ServerManagerSharedLib.Dates
{
    public class TimeSpanHelper
    {
        public static readonly long TICKS_PER_HALF_MILLISECOND = TimeSpan.TicksPerMillisecond / 2;
        public static readonly long TICKS_PER_500_MILLISECONDS = TimeSpan.TicksPerMillisecond * 500;

        public static readonly long TICKS_PER_30_SECONDS = TimeSpan.TicksPerSecond * 30;

        public static readonly long TICKS_PER_30_MINUTES = TimeSpan.TicksPerMinute * 30;
        public static readonly long TICKS_PER_15_MINUTES = TimeSpan.TicksPerMinute * 15;

        public static readonly decimal TICKS_PER_HOUR_DECIMAL = TimeSpan.TicksPerHour;
    }
}