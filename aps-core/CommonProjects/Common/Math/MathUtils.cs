namespace PT.Common.Utility;

public static class MathUtils
{
    /// <summary>
    /// Returns a value within the inclusive min max range
    /// </summary>
    public static T ConstrainRange<T>(T a_min, T a_value, T a_max) where T : struct, IComparable
    {
        if (a_value.CompareTo(a_min) <= 0)
        {
            return a_min;
        }

        if (a_value.CompareTo(a_max) >= 1)
        {
            return a_max;
        }

        return a_value;
    }
}