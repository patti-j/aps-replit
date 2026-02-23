using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using PT.APSCommon.Extensions;
using PT.Common.Localization;
using PT.UIDefinitions.Cursors;

namespace PT.UIDefinitions;

/// <summary>
/// Summary description for Appearance.
/// </summary>
public class PTAppearance
{
    public static int fastDrawThreshold = 1000; //1 second
    public static TimeSpan fastDrawCancelThreshold = new (24, 0, 0); //1 day

    private const string c_fontFamily = "Verdana";
    private const int c_smallFontSize = 8;
    private const int c_largeFontSize = 10;
    public static Font NormalFont = new (c_fontFamily, c_smallFontSize, FontStyle.Regular);
    public static Font HeaderFont = new (c_fontFamily, c_smallFontSize, FontStyle.Bold);
    public static Font LargeFont = new (c_fontFamily, c_largeFontSize, FontStyle.Regular);
    public static Font SelectedFont = new (c_fontFamily, c_largeFontSize, FontStyle.Bold);
    public static Font SelectedFontAlternate = new (c_fontFamily, c_largeFontSize, FontStyle.Underline);
    public static Font LegendFont = new (c_fontFamily, c_largeFontSize, FontStyle.Regular);
    public static Font GanttOverlayFont = new (c_fontFamily, 65F, FontStyle.Regular, GraphicsUnit.Point, 0);

    /// <summary>
    /// Can be used for all brands.
    /// Advanced Planning & Scheduling
    /// </summary>
    public static readonly string AppNameGenericLong = "Advanced Planning & Scheduling";

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

    public static string GetLongDateTimeString(DateTime dt)
    {
        return dt.ToLongDateString() + "  " + dt.ToShortTimeString();
    }

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

    public static string GetVeryShortDayDateString(DateTime dt)
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

    #region Gantt Appearance
    public static readonly Font TooltipFont = new ("Arial", 8);
    public static readonly Font CapacityIntervalFont = new ("Arial", 8);
    #endregion

    #region Cursors
    public static Cursor CursorDefault;
    public static Cursor CursorSelectOnly;
    public static Cursor CursorLeftClick;

    public static void LoadCursors(object parent)
    {
        CursorDefault = System.Windows.Forms.Cursors.Default;
        CursorLeftClick = System.Windows.Forms.Cursors.Arrow;

        //Custom cursors
        //Block
        CursorSelectOnly = new Cursor(typeof(CursorsNamespace), "SelectOnly.cur");
        CursorSelect_DragAll = new Cursor(typeof(CursorsNamespace), "Select_DragAll.cur");
        CursorSelect_DragUpDownRight = new Cursor(typeof(CursorsNamespace), "Select_DragUpDownRight.cur");
        CursorSelect_DragLeftRight = new Cursor(typeof(CursorsNamespace), "Select_DragLeftRight.cur");

        //CapacityIntervals
        CursorToolDrag_CapacityIntervalRegularOnline = new Cursor(typeof(CursorsNamespace), "CapacityIntervalRegularOnline.cur");
    }

    //All possible modes that the gantt point can be in and therefore need distinctive cursors
    public enum ganttPointerModes
    {
        Default = 0,
        BlockDrag,
        BlockResize,
        CapacityIntervalRegularOnlineNew,
        CapacityIntervalRegularOnlineCopy,
        CapacityIntervalRegularOnlineShare,
        CapacityIntervalRegularOnlineResize,
        CapacityIntervalOvertimeNew,
        CapacityIntervalPotentialOvertimeNew,
        CapacityIntervalOfflineNew,
        CapacityIntervalCleanoutNew
    }

    //Gantt Block Cursors
    public static Cursor CursorSelect_DragAll;
    public static Cursor CursorSelect_DragLeftRight;
    public static Cursor CursorDragVertical;
    public static Cursor CursorDragRight;
    public static Cursor CursorSelect_DragUpDownRight;

    // Tool cursors
    public static Cursor CursorToolDrag_CapacityIntervalRegularOnline;

    public enum toolDragCursors { RegularOnlineTime = 1, Overtime = 2, OfflineTime = 3, PotentialOvertime = 4 }

    /// <summary>
    /// Gets cursors for dragging of tools to drop them in the gantt.
    /// </summary>
    /// <returns></returns>
    public static Cursor GetCursor(ganttPointerModes mode)
    {
        if (mode == ganttPointerModes.CapacityIntervalRegularOnlineNew ||
            mode == ganttPointerModes.CapacityIntervalOfflineNew ||
            mode == ganttPointerModes.CapacityIntervalOvertimeNew ||
            mode == ganttPointerModes.CapacityIntervalPotentialOvertimeNew)
        {
            return CursorToolDrag_CapacityIntervalRegularOnline;
        }

        return CursorDefault;
    }

    //Scrolling Cursors
    public static Cursor CursorScrollAll = System.Windows.Forms.Cursors.NoMove2D;
    public static Cursor CursorScrollHorizontal = System.Windows.Forms.Cursors.NoMoveHoriz;
    public static Cursor CursorScrollVertical = System.Windows.Forms.Cursors.NoMoveVert;

    /// <summary>
    /// Get a cursor for scrolling.
    /// </summary>
    public static Cursor GetCursor(bool canScrollVertical, bool canScrollHorizontal)
    {
        if (canScrollVertical && canScrollHorizontal)
        {
            return CursorScrollAll;
        }

        if (canScrollVertical)
        {
            return CursorScrollVertical;
        }

        if (canScrollHorizontal)
        {
            return CursorScrollHorizontal;
        }

        return CursorDefault;
    }
    #endregion
}