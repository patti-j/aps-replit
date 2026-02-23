using System.Collections.ObjectModel;

using PT.Common.Debugging;

namespace PT.Common;

/// <summary>
/// Summary description for TimeZoneAdjuster.
/// </summary>
public static class TimeZoneAdjuster
{
    private static TimeZoneInfo s_timeZoneInfo; //Cannot be used before being set
    private static bool s_useDaylightSavingAdjustment;

    public static TimeZoneInfo CurrentTimeZoneInfo => s_timeZoneInfo;

    /// <summary>
    /// Change the date time to the current user's time zone, if not already in that timezone.
    /// </summary>
    /// <param name="a_serverTime"></param>
    /// <returns></returns>
    public static DateTimeOffset ToDisplayTime(this DateTime a_serverTime)
    {
        return ToDisplayTime(a_serverTime.ToDateTimeOffset(TimeZoneInfo.Utc));
    }

    /// <summary>
    /// Change the date time to the current user's time zone, if not already in that timezone.
    /// </summary>
    /// <param name="a_serverTime"></param>
    /// <returns></returns>
    public static DateTimeOffset ToDisplayTime(this DateTimeOffset a_serverTime)
    {
        if (s_timeZoneInfo == null)
        {
            //In case the client hasn't logged in yet and is trying to log a time
            return a_serverTime;
        }
        
        if (a_serverTime <= PTDateTime.MinValue)
        {
            return PTDateTime.MinValue;
        }

        if (a_serverTime >= PTDateTime.MaxValue)
        {
            return PTDateTime.MaxValue;
        }

        try
        {
            if (s_useDaylightSavingAdjustment &&
                a_serverTime.Offset == s_timeZoneInfo.GetUtcOffset(a_serverTime))
            {
                return a_serverTime;
            }
            #if DEBUG

            if (a_serverTime.Offset == TimeZoneInfo.Local.BaseUtcOffset)
            {
                //This is a local time, and should not have been set
            }
            else if (a_serverTime.Offset != TimeZoneInfo.Utc.BaseUtcOffset) // keep this as BaseUtcOffset since utc doesn't observe DST so it doesn't matter
            {
                //TODO: this should not be the case
            }
            #endif

            return TimeZoneInfo.ConvertTime(a_serverTime, s_timeZoneInfo);
        }
        catch (Exception)
        {
            return a_serverTime;
        }
    }

    public static DateTimeOffset ToLocalMachineTime(this DateTime a_serverTime)
    {
        return ToLocalMachineTime(a_serverTime.ToDateTimeOffset(TimeZoneInfo.Utc));
    }

    public static DateTimeOffset ToLocalMachineTime(this DateTimeOffset a_serverTime)
    {
        return TimeZoneInfo.ConvertTime(a_serverTime, TimeZoneInfo.Local);
    }

    /// <summary>
    /// IMPORTANT: This method assumes that the current time being converted has a user offset value. If this is a PT server time, it will convert again.
    /// </summary>
    /// <param name="a_serverTime"></param>
    /// <returns></returns>
    public static DateTime ToServerTime(this DateTime a_serverTime)
    {
        if (a_serverTime.Kind == DateTimeKind.Utc)
        {
            throw new DebugException("Server time being converted");
        }


        if (a_serverTime <= PTDateTime.MinDateTime)
        {
            return new DateTime(PTDateTime.MinValue.Ticks, DateTimeKind.Utc);
        }

        if (a_serverTime >= PTDateTime.MaxDateTime)
        {
            return new DateTime(PTDateTime.MaxValue.Ticks, DateTimeKind.Utc);
        }

        return ToServerTime(a_serverTime.ToDateTimeOffset(s_timeZoneInfo));
    }

    /// <summary>
    /// Change the date time to UTC time zone, if not already in that timezone.
    /// </summary>
    /// <param name="a_displayTime">The display time to convert</param>
    /// <returns></returns>
    public static DateTime ToServerTime(this DateTimeOffset a_displayTime)
    {
        if (a_displayTime <= PTDateTime.MinValue)
        {
            return new DateTime(PTDateTime.MinValue.Ticks, DateTimeKind.Utc);
        }

        if (a_displayTime >= PTDateTime.MaxValue)
        {
            return new DateTime(PTDateTime.MaxValue.Ticks, DateTimeKind.Utc);
        }

        try
        {
            if (a_displayTime.Offset == TimeZoneInfo.Utc.GetUtcOffset(a_displayTime))
            {
                //Time has already been converted, don't modify
                return new DateTime(a_displayTime.Ticks, DateTimeKind.Utc);
            }

            #if DEBUG
            if (a_displayTime.Offset != s_timeZoneInfo.GetUtcOffset(a_displayTime))
            {
                //TODO: this should not be the case
            }
            #endif

            if (!s_useDaylightSavingAdjustment && s_timeZoneInfo.IsDaylightSavingTime(a_displayTime))
            {
                a_displayTime = a_displayTime.Add(TimeSpan.FromHours(1));
            }

            DateTimeOffset utcTime = TimeZoneInfo.ConvertTime(a_displayTime, TimeZoneInfo.Utc);
            return new DateTime(utcTime.Ticks, DateTimeKind.Utc);
        }
        catch (Exception)
        {
            return new DateTime(a_displayTime.Ticks, DateTimeKind.Utc);
        }
    }

    /// <summary>
    /// Determines if a_timeToAdjust goes through a day change when it gets adjusted
    /// to the timezone/offset of the a_timeToAdjustTo.
    /// Given the way timezones and days work, the range of for day change is just {-1, 0, 1}.
    /// </summary>
    /// <param name="a_displayTime"> The time to be adjusted</param>
    /// <returns>
    /// An integer of the set {-1, 0, 1} that corresponds to how many day was adjusted.
    /// </returns>
    public static int GetDayAdjustment(DateTimeOffset a_displayTime)
    {
        DateTime utcTime = a_displayTime.ToServerTime();

        if (utcTime.Date > a_displayTime.Date)
        {
            return 1;
        }

        if (utcTime.Date < a_displayTime.Date)
        {
            return -1;
        }

        return 0;
    }

    public static DateTimeOffset ConvertToServerRoundMinutes(DateTimeOffset a_displayTime)
    {
        return DateTimeHelper.RoundMinutes(ToServerTime(a_displayTime));
    }

    public static DateTimeOffset ConvertToServerTimeRoundToNearest30Minutes(DateTimeOffset a_displayTime)
    {
        return DateTimeHelper.RoundToNearest30Minutes(ToServerTime(a_displayTime));
    }

    public static void SetTimeZoneInfo(TimeZoneInfo a_timeZoneInfo)
    {
        s_timeZoneInfo = s_useDaylightSavingAdjustment ? a_timeZoneInfo : TimeZoneInfo.CreateCustomTimeZone(a_timeZoneInfo.Id, a_timeZoneInfo.BaseUtcOffset, $"{a_timeZoneInfo.DisplayName} (Standard Time)", a_timeZoneInfo.StandardName);
    }

    public static void SetUseDaylightSavingAdjustment(bool a_useDaylightSavingAdjustment)
    {
        s_useDaylightSavingAdjustment = a_useDaylightSavingAdjustment;
    }

    public static TimeZoneInfo GetTimeZoneInfoBasedOnOffset(int a_offset)
    {
        if (a_offset == 0)
        {
            return TimeZoneInfo.Utc;
        }

        TimeSpan utcOffset = TimeSpan.FromHours(a_offset);
        ReadOnlyCollection<TimeZoneInfo> timezones = TimeZoneInfo.GetSystemTimeZones();
        TimeZoneInfo tz = timezones.FirstOrDefault(x => x.GetUtcOffset(DateTime.UtcNow) == utcOffset);
        return tz ?? TimeZoneInfo.Utc;
    }

    /// <summary>
    /// Returns the same time value with the user's time zone specified.
    /// The returned DateTimeOffset can be converted ToServerTime and ToDisplayTime safely.
    /// </summary>
    /// <param name="a_localComputerTime"></param>
    /// <returns></returns>
    public static DateTimeOffset ToDateTimeOffsetUserTime(this DateTime a_localComputerTime)
    {
        //TODO: Remove this once all dates are valid
        if (a_localComputerTime == DateTime.MaxValue)
        {
            #if TEST
                throw new DebugException("DateTime being converted should be within PTDateTime Min/Max range");
            #endif
            return PTDateTime.MaxValue;
        }

        if (a_localComputerTime == DateTime.MinValue)
        {
            #if TEST
                throw new DebugException("DateTime being converted should be within PTDateTime Min/Max range");
            #endif
            return PTDateTime.MinValue;
        }

        return new DateTimeOffset(a_localComputerTime, s_timeZoneInfo.GetUtcOffset(a_localComputerTime));
    }

    /// <summary>
    /// Returns the same time value with the UTC time zone specified.
    /// The returned DateTimeOffset can be converted ToServerTime and ToDisplayTime safely.
    /// </summary>
    /// <param name="a_userTime"></param>
    /// <returns></returns>
    public static DateTimeOffset ToDateTimeOffsetUtc(this DateTime a_userTime)
    {
        return new DateTimeOffset(a_userTime, TimeZoneInfo.Utc.BaseUtcOffset);
    }

    //public static DateTimeOffset ToDateTimeOffsetUserTimeFromLocalTime(this DateTime a_localComputerTime)
    //{
    //    return new DateTimeOffset(a_localComputerTime, s_timeZoneInfo.BaseUtcOffset);
    //}
}