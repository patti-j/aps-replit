using System.Text;

namespace PT.Scheduler.Simulation;

/// <summary>
/// Originally written to help determine difference between fields of manufacturing orders that are being batched together.
/// </summary>
internal class DifferencesStatics
{
    private static void AppendDifferences(string a_fieldName, string a_v1, string a_v2, StringBuilder a_sb)
    {
        a_sb.Append(string.Format("{0} ({1} ««««« {2});", a_fieldName, a_v1, a_v2));
    }

    internal static bool Differences(string a_fieldName, string a_v1, string a_v2, StringBuilder a_sb)
    {
        if (a_v1 != a_v2)
        {
            AppendDifferences(a_fieldName, a_v1, a_v2, a_sb);
            return true;
        }

        return false;
    }

    internal static bool Differences(string a_fieldName, int a_v1, int a_v2, StringBuilder a_sb)
    {
        if (a_v1 != a_v2)
        {
            AppendDifferences(a_fieldName, a_v1.ToString(), a_v2.ToString(), a_sb);
            return true;
        }

        return false;
    }

    internal static bool Differences(string a_fieldName, long a_v1, long a_v2, StringBuilder a_sb)
    {
        if (a_v1 != a_v2)
        {
            AppendDifferences(a_fieldName, a_v1.ToString(), a_v2.ToString(), a_sb);
            return true;
        }

        return false;
    }

    internal static bool Differences(string a_fieldName, double a_v1, double a_v2, StringBuilder a_sb)
    {
        if (a_v1 != a_v2)
        {
            AppendDifferences(a_fieldName, a_v1.ToString(), a_v2.ToString(), a_sb);
            return true;
        }

        return false;
    }

    internal static bool Differences(string a_fieldName, decimal a_v1, decimal a_v2, StringBuilder a_sb)
    {
        if (a_v1 != a_v2)
        {
            AppendDifferences(a_fieldName, a_v1.ToString(), a_v2.ToString(), a_sb);
            return true;
        }

        return false;
    }

    internal static bool TimespanDifferences(string a_fieldName, long a_v1, long a_v2, StringBuilder a_sb)
    {
        if (a_v1 != a_v2)
        {
            string s1 = new TimeSpan(a_v1).ToString();
            string s2 = new TimeSpan(a_v2).ToString();
            if (s1 != s2)
            {
                AppendDifferences(a_fieldName, s1, s2, a_sb);
            }
            else
            {
                AppendDifferences(a_fieldName, a_v1.ToString(), a_v2.ToString(), a_sb);
            }

            return true;
        }

        return false;
    }

    internal static bool Differences(string a_fieldName, System.Drawing.Color a_v1, System.Drawing.Color a_v2, StringBuilder a_sb)
    {
        if (a_v1 != a_v2)
        {
            AppendDifferences(a_fieldName, a_v1.ToArgb().ToString(), a_v2.ToArgb().ToString(), a_sb);
            return true;
        }

        return false;
    }

    internal static bool Differences(string a_fieldName, bool a_b1, bool a_b2, StringBuilder a_sb)
    {
        if (a_b1 != a_b2)
        {
            AppendDifferences(a_fieldName, a_b1.ToString(), a_b2.ToString(), a_sb);
            return true;
        }

        return false;
    }
}