namespace PT.Common;

public class WeekNumberCalculator
{
    [Obsolete("This uses current culture, but needs to be updated to use User's timezone")]
    public static int GetWeekNumberForCurrentCulture(DateTimeOffset a_date)
    {
        //got this from here: http://konsulent.sandelien.no/VB_help/Week/

        // Using builtin .NET culture features and C#

        //System.Globalization.CultureInfo.CreateSpecificCulture("no");
        // Get the Norwegian calendar from the culture object
        //System.Globalization.Calendar cal = norwCulture.Calendar;
        //System.Globalization.CultureInfo.CurrentCulture.Calendar.
        // Use the GetWeekOfYear method on the calendar object to 

        System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CurrentCulture;
        // get the correct week of the year
        return culture.Calendar.GetWeekOfYear(a_date.DateTime,
            culture.DateTimeFormat.CalendarWeekRule,
            culture.DateTimeFormat.FirstDayOfWeek);
    }

    public static DateTime GetBeginningOfWeek(DateTimeOffset a_date)
    {
        DateTime dateWithoutTime = a_date.ToDateTime().Date;
        //subtract the number of days from Sunday
        switch (dateWithoutTime.DayOfWeek)
        {
            case DayOfWeek.Sunday:
                return dateWithoutTime;
            case DayOfWeek.Monday:
                return dateWithoutTime.AddDays(-1);
            case DayOfWeek.Tuesday:
                return dateWithoutTime.AddDays(-2);
            case DayOfWeek.Wednesday:
                return dateWithoutTime.AddDays(-3);
            case DayOfWeek.Thursday:
                return dateWithoutTime.AddDays(-4);
            case DayOfWeek.Friday:
                return dateWithoutTime.AddDays(-5);
            case DayOfWeek.Saturday:
                return dateWithoutTime.AddDays(-6);
            default:
                return dateWithoutTime;
        }
    }
}