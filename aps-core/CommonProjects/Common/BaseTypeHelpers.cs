namespace PT.Common;

public class BaseTypeHelpers
{
    /// <summary>
    /// Generic Try parse for number base types
    /// </summary>
    /// <param name="a_compareValue">Value to parse</param>
    /// <param name="a_value">Value to use as the type to parse</param>
    /// <param name="o_parsedValue">out value if parse is successful</param>
    /// <returns></returns>
    public bool TryParseNumber(object a_compareValue, object a_value, out object o_parsedValue)
    {
        o_parsedValue = null;
        if (a_value is int)
        {
            int parsedValue;
            if (!int.TryParse(a_compareValue.ToString(), out parsedValue))
            {
                return false;
            }

            o_parsedValue = parsedValue;
            return true;
        }

        if (a_value is long)
        {
            long parsedValue;
            if (!long.TryParse(a_compareValue.ToString(), out parsedValue))
            {
                return false;
            }

            o_parsedValue = parsedValue;
            return true;
        }

        if (a_value is decimal)
        {
            decimal parsedValue;
            if (!decimal.TryParse(a_compareValue.ToString(), out parsedValue))
            {
                return false;
            }

            o_parsedValue = parsedValue;
            return true;
        }

        if (a_value is double)
        {
            double parsedValue;
            if (!double.TryParse(a_compareValue.ToString(), out parsedValue))
            {
                return false;
            }

            o_parsedValue = parsedValue;
            return true;
        }

        if (a_value is uint)
        {
            uint parsedValue;
            if (!uint.TryParse(a_compareValue.ToString(), out parsedValue))
            {
                return false;
            }

            o_parsedValue = parsedValue;
            return true;
        }

        if (a_value is ulong)
        {
            ulong parsedValue;
            if (!ulong.TryParse(a_compareValue.ToString(), out parsedValue))
            {
                return false;
            }

            o_parsedValue = parsedValue;
            return true;
        }

        return false;
    }
}