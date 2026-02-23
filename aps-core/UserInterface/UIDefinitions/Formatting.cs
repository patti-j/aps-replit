using System.Globalization;

using PT.APSCommon.Extensions;
using PT.Common.Localization;

namespace PT.UIDefinitions;

public static class Formatting
{
    #region Formats and Masks
    public static readonly string DateFormat = "d";

    public static readonly string TimeFormat = DateTimeFormatInfo.CurrentInfo.ShortTimePattern;

    public static readonly string TimeMask = DateTimeFormatInfo.CurrentInfo.ShortTimePattern.ToLower();

    public static readonly string DateTimeFormat = "g";

    public static readonly string GanttDateTimeFormat = "ddd, mmm dd, yyyy";

    public static readonly string WholeNumberFormat = "N0";

    public static readonly string NumberFormat = "n";

    public static readonly string InputMaskDecimal = "###############.####";

    public static readonly string DecimalFormat = "n4";

    /// <summary>
    /// the number displays a zero if there is no corresponding values for the '#' otherwise,
    /// there is precision of four on each side of the decimal place.
    /// </summary>
    public static readonly string NumberWithDecimalPrecisionFour = "{0:n4}";

    /// <summary>
    /// Input mask for Grid DateTimes.
    /// </summary>
    public static readonly string DateTimeMask = "{date} {time}";

    /// <summary>
    /// Input mask for Grid TimeSpans.
    /// </summary>
    public static readonly string TimeSpanMaskSecondsPrecision = "hh\\:mm\\:ss";

    public static readonly string DateMask = "";

    public static readonly string CurrencyFormat = "c";

    public static readonly string IntFormat = "#,###";

    /// <summary>
    /// Width of borders in forms.
    /// </summary>
    public static readonly int BorderWidth = 9;

    /// <summary>
    /// Returns a string that has been created by formatting the object based on its type.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetFormattedString(object input)
    {
        if (input == null)
        {
            return "";
        }

        if (input is TimeSpan)
        {
            return ((TimeSpan)input).ToReadableStringSecondPrecision();
        }
        //else if(input.GetType()==typeof(Decimal)) 
        //    return ((Decimal)input).ToString("C");  //causes currency to show up when it shouldn't. Need to handle in the UI on a case-by-case basis.

        if (input is DateTime)
        {
            return GetShortDateTimeString((DateTime)input);
        }

        if (input is double)
        {
            return Math.Round((double)input, 2).ToString();
        }

        return input.ToString();
    }

    public static string GetRoundedString(double val)
    {
        return Math.Round(val, 2).ToString("N");
    }

    public static string GetRoundedString(decimal val)
    {
        return Math.Round(val, 4).ToString("N");
    }

    public static readonly string DOWDayTimeDateFormat = "ddd h:mm tt, d MMM yyyy";

    [Obsolete("Use DateTimeOffset instead")]
    public static string GetLongDateTimeString(DateTime dt)
    {
        return dt.ToLongDateString() + "  " + dt.ToShortTimeString();
    }

    public static string GetLongDateTimeString(DateTimeOffset dt)
    {
        return dt.ToDateTime().ToLongDateString() + "  " + dt.ToDateTime().ToShortTimeString();
    }

    [Obsolete("Use DateTimeOffset instead")]
    public static string GetShortDateTimeString(DateTime dt)
    {
        return dt.ToShortDateString() + "  " + dt.ToShortTimeString();
    }

    public static string GetShortDateTimeString(DateTimeOffset dt)
    {
        return dt.DateTime.ToShortDateString() + "  " + dt.DateTime.ToShortTimeString();
    }

    public static string GetVeryShortDayDateTimeString(DateTime dt)
    {
        return dt.ToString("ddd dd MMM HH"); //    dddd, MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo) + "  " + dt.ToShortTimeString();
    }

    public static string GetVeryShortDayString(DateTime dt)
    {
        return dt.ToString("ddd HH:mm");
    }

    public static string GetTimeDayDateString(DateTime dt)
    {
        return dt.ToString("hh:mm tt ddd dd MMM"); //    dddd, MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo) + "  " + dt.ToShortTimeString();
    }

    public static string GetVeryShortDayDateMonthString(DateTime dt)
    {
        return dt.ToString("ddd dd MMM"); //dddd, MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo) + "  " + dt.ToShortTimeString();
    }

    public static string GetVeryShortDayDateMonthString(DateTimeOffset dt)
    {
        return dt.DateTime.ToString("ddd dd MMM"); //dddd, MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo) + "  " + dt.ToShortTimeString();
    }

    public static string GetVeryShortDayDateString(DateTimeOffset dt)
    {
        return dt.ToString("ddd dd");
    }

    public static string GetVeryShortDayDateHrString(DateTime dt)
    {
        return dt.ToString("ddd dd MMM HH"); //dddd, MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo) + "  " + dt.ToShortTimeString();
    }

    public static string GetVeryShortDateTimeString(DateTime dt)
    {
        return dt.ToString("dd MMM HH"); //    dddd, MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo) + "  " + dt.ToShortTimeString();
    }

    public static string GetStringSortableDateTimeString(DateTime dt)
    {
        return dt.ToString("yyyy-MM-dd HH:mm"); //    dddd, MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo) + "  " + dt.ToShortTimeString();
    }

    public static string GetMonthYearString(DateTime dt)
    {
        return dt.ToString("MMMM yyyy");
    }

    public static string GetVeryShortDayDateYearString(DateTime dt)
    {
        return dt.ToString("ddd dd MMM yyyy"); //dddd, MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo) + "  " + dt.ToShortTimeString();
    }

    /// <summary>
    /// Retuns localized strings like:
    /// 2 milliseconds
    /// 1.2 minutes
    /// 5 hours
    /// </summary>
    /// <param name="a_timespan"></param>
    /// <returns></returns>
    public static string GetDurationString(TimeSpan a_timespan)
    {
        if (a_timespan.TotalSeconds < 1)
        {
            return string.Format(Localizer.GetString("{0:N2} milliseconds"), a_timespan.TotalMilliseconds);
        }

        if (a_timespan.TotalMinutes < 1)
        {
            return string.Format(Localizer.GetString("{0:N2} seconds"), a_timespan.TotalSeconds);
        }

        if (a_timespan.TotalHours < 1)
        {
            return string.Format(Localizer.GetString("{0:N2} minutes"), a_timespan.TotalMinutes);
        }

        if (a_timespan.TotalDays < 1)
        {
            return string.Format(Localizer.GetString("{0:N2} hours"), a_timespan.TotalHours);
        }

        return string.Format(Localizer.GetString("{0:N2} days"), a_timespan.TotalDays);
    }
    #endregion Formats
}