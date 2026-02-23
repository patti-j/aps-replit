using System.Text;

namespace PT.APSCommon.Extensions;

//TODO: Localize values as staic strings to improve performance instead of localizing each time the string is build.
public static class TimeSpanExtensions
{
    public static string ToReadableStringDayPrecision(this TimeSpan a_span)
    {
        string validation = ValidateMinMax(a_span);
        if (validation.Length > 0)
        {
            return validation;
        }

        string readableString = GetDayPrecision(a_span);
        if (readableString.Length == 0)
        {
            return "Less than 1 day".Localize();
        }

        return readableString;
    }

    public static string ToReadableStringHourPrecision(this TimeSpan a_span)
    {
        string validation = ValidateMinMax(a_span);
        if (validation.Length > 0)
        {
            return validation;
        }

        string readableString = GetHourPrecision(a_span);
        if (readableString.Length == 0)
        {
            return "Less than 1 hour".Localize();
        }

        return readableString;
    }

    public static string ToReadableStringSecondPrecision(this TimeSpan a_span)
    {
        string validation = ValidateMinMax(a_span);
        if (validation.Length > 0)
        {
            return validation;
        }

        string readableString = GetSecondPrecision(a_span);
        if (readableString.Length == 0)
        {
            return "Less than 1 second".Localize();
        }

        return readableString;
    }

    public static string ToReadableStringUnits(this TimeSpan a_span)
    {
        string validation = ValidateMinMax(a_span);
        if (validation.Length > 0)
        {
            return validation;
        }

        if (a_span < TimeSpan.FromMinutes(1))
        {
            return string.Format("{0} seconds".Localize(), Math.Floor(a_span.TotalSeconds));
        }

        if (a_span < TimeSpan.FromMinutes(60))
        {
            return string.Format("{0} minutes".Localize(), Math.Floor(a_span.TotalMinutes));
        }

        if (a_span < TimeSpan.FromHours(24))
        {
            return string.Format("{0} hours".Localize(), Math.Floor(a_span.TotalHours));
        }

        if (a_span < TimeSpan.FromDays(365))
        {
            return string.Format("{0} days".Localize(), Math.Floor(a_span.TotalDays));
        }

        return string.Format("{0} years".Localize(), Math.Round(a_span.TotalDays / 365, 2));
    }

    private static string ValidateMinMax(TimeSpan a_span)
    {
        if (a_span == TimeSpan.MaxValue || a_span == TimeSpan.MinValue)
        {
            return "Not Set".Localize();
        }

        if (a_span == TimeSpan.Zero)
        {
            return "Zero".Localize();
        }

        return string.Empty;
    }

    private static string GetDayPrecision(TimeSpan a_timespan)
    {
        StringBuilder sb = new ();
        DateTime timeBuilder = new (a_timespan.Duration().Ticks);

        if (timeBuilder.Year > 2)
        {
            sb.Append(string.Format("{0} Years".Localize() + " ", timeBuilder.Year - 1));
        }
        else if (timeBuilder.Year > 1)
        {
            sb.Append("1 Year".Localize() + " ");
        }

        if (timeBuilder.Month > 2)
        {
            sb.Append(string.Format("{0} months".Localize() + " ", timeBuilder.Month - 1));
        }
        else if (timeBuilder.Month > 1)
        {
            sb.Append("1 Month".Localize() + " ");
        }

        if ((timeBuilder.Day - 1) % 7 == 0)
        {
            int weeks = (timeBuilder.Day - 1) / 7;
            if (weeks > 1)
            {
                sb.Append(string.Format("{0} weeks".Localize() + " ", weeks));
            }
            else if (weeks == 1)
            {
                sb.Append("1 week".Localize() + " ");
            }
        }
        else if (timeBuilder.Day > 2)
        {
            sb.Append(string.Format("{0} days".Localize() + " ", timeBuilder.Day - 1));
        }
        else if (timeBuilder.Day > 1)
        {
            sb.Append(string.Format("{0} day".Localize() + " ", timeBuilder.Day - 1));
        }

        return sb.ToString().TrimEnd();
    }

    private static string GetHourPrecision(TimeSpan a_timespan)
    {
        StringBuilder sb = new (GetDayPrecision(a_timespan));
        DateTime timeBuilder = new (a_timespan.Duration().Ticks);

        if (sb.Length > 0)
        {
            sb.Append(" "); //spacing
        }

        if (timeBuilder.Hour > 1)
        {
            sb.Append(string.Format("{0} hours".Localize() + " ", timeBuilder.Hour));
        }
        else if (timeBuilder.Hour == 1)
        {
            sb.Append("1 hour".Localize() + " ");
        }

        return sb.ToString().TrimEnd();
    }

    private static string GetSecondPrecision(TimeSpan a_timespan)
    {
        StringBuilder sb = new (GetHourPrecision(a_timespan));
        DateTime timeBuilder = new (a_timespan.Duration().Ticks);

        if (sb.Length > 0)
        {
            sb.Append(" "); //spacing
        }

        if (timeBuilder.Minute > 1)
        {
            sb.Append(string.Format("{0} minutes".Localize() + " ", timeBuilder.Minute));
        }
        else if (timeBuilder.Minute == 1)
        {
            sb.Append("1 minute".Localize() + " ");
        }

        if (timeBuilder.Second > 1)
        {
            sb.Append(string.Format("{0} seconds".Localize() + " ", timeBuilder.Second));
        }
        else if (timeBuilder.Second == 1)
        {
            sb.Append("1 second".Localize() + " ");
        }

        return sb.ToString().TrimEnd();
    }
}