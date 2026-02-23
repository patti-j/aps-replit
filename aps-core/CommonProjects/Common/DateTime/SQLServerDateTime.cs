namespace PT.Common;

/// <summary>
/// Summary description for SQLServerDateTime.
/// </summary>
public class SQLServerConversions
{
    public static DateTime GetValidDateTime(DateTime a_dt)
    {
        if (a_dt.Ticks < PTDateTime.MinDateTime.Ticks)
        {
            return PTDateTime.MinDateTime;
        }

        if (a_dt.Ticks > PTDateTime.MaxDateTime.Ticks)
        {
            return PTDateTime.MaxDateTime;
        }

        return a_dt;
    }

    public static long GetValidDateTime(long a_dateTime)
    {
        if (a_dateTime < PTDateTime.MinDateTime.Ticks)
        {
            return PTDateTime.MinDateTime.Ticks;
        }

        if (a_dateTime > PTDateTime.MaxDateTime.Ticks)
        {
            return PTDateTime.MaxDateTime.Ticks;
        }

        return a_dateTime;
    }
}