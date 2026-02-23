using System.Globalization;

namespace PT.Common;

public class MathStatics
{
    /// <summary>
    /// Use this value to take into consideration the inaccuracies of floating point calculations.
    /// Consider values less than this to be zero.
    /// </summary>
    public static readonly decimal CloseToZero000001 = 0.000001m;

    /// <summary>
    /// Use this value to take into consideration the inaccuracies of floating point calculations. The referenced value
    /// will be set to zero if it is less than 0.000001.
    /// </summary>
    /// <param name="r_value"></param>
    public static void CloseToZeroAdjuster000001(ref decimal r_value)
    {
        if (Math.Abs(r_value) < CloseToZero000001)
        {
            r_value = 0;
        }
    }

    /// <summary>
    /// Whether the value is reasonably close enough to zero to consider it to be zero.
    /// Currently this is "return Math.Abs(value)<CloseToZero000001".
    /// 
    /// 
    /// 
    /// </summary>
    /// <param name="a_value"></param>
    /// <returns></returns>
    public static bool IsZero(decimal a_value)
    {
        return Math.Abs(a_value) < CloseToZero000001;
    }

    /// <summary>
    /// Whether two values are close enough to be considered equal.
    /// </summary>
    /// <param name="a_value1"></param>
    /// <param name="a_value2"></param>
    /// <returns></returns>
    public static bool IsEqual(decimal a_value1, decimal a_value2)
    {
        decimal diff = a_value1 - a_value2;
        return IsZero(diff);
    }

    /// <summary>
    /// Whether a value is greater than another. If they are close in value, but not equal, they may be considered equal and affect the results of this function call. This function is useful in cases where
    /// tiny differences in numbers aren't important and where imprecise calculations are made.
    /// </summary>
    /// <param name="a_value1">Determine if this is greater than value2.</param>
    /// <param name="a_value2"></param>
    /// <returns></returns>
    public static bool IsGreaterThan(decimal a_value1, decimal a_value2)
    {
        return a_value1 > a_value2 + CloseToZero000001;
    }

    /// <summary>
    /// Whether the string is a long value and if so the out parameter is set.
    /// </summary>
    /// <param name="a_stringValue"></param>
    /// <param name="o_number"></param>
    /// <returns></returns>
    public static bool IsLong(string a_stringValue, out long o_number)
    {
        return long.TryParse(a_stringValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out o_number);
    }

    /// <summary>
    /// Use this function to round small negative values up to zero.
    /// In DEBUG mode if the value isn't approximately equal to zero, an exception is thrown.
    /// </summary>
    /// <param name="r_v"></param>
    public static void RoundNegativeValuesToZero(ref decimal r_v)
    {
        #if DEBUG
        if (r_v < 0 && !IsZero(r_v))
        {
            throw new Exception("Number shouldn't be negative.");
        }
        #endif
        if (r_v < 0)
        {
            r_v = 0;
        }
    }

    public static bool Fraction(decimal x)
    {
        #if DEBUG
        // If you put another 9 at the front of this number this function won't work. A larger value may work though.
        // The reason for this is decimals have 15 to 16 digits of precision. The number below has 16 digits.
        if (Math.Abs(x) > 999999999999999.9m)
        {
            throw new Exception("Fraction() parameter exceeds tested working value");
        }
        #endif
        return x % 1 != 0;
    }
}

public class FloatingPoint
{
    private const decimal c_e = 0.00005m;

    /// <summary>
    /// A qty less than this will be set to this in the scheduler.
    /// </summary>
    public static decimal ApproximatelyEqualZeroQty => c_e;

    public static bool ApproximatelyEqual(decimal a_x, decimal a_y)
    {
        decimal diff = a_x - a_y;
        decimal absDiff = Math.Abs(diff);

        decimal sum = a_x + a_y;
        decimal absSum = Math.Abs(sum);
        decimal approxComparison = absSum * c_e;

        bool ret = absDiff < approxComparison;
        return ret;
    }

    public bool ApproximatelyEqual(decimal a_x, decimal a_y, decimal a_epsilon)
    {
        decimal diff = a_x - a_y;
        decimal absDiff = Math.Abs(diff);

        bool ret = absDiff < a_epsilon;
        return ret;
    }

    public bool ApproximatelyEqual(double a_x, double a_y, decimal a_epsilon)
    {
        double diff = a_x - a_y;
        double absDiff = Math.Abs(diff);

        bool ret = absDiff < Convert.ToDouble(a_epsilon);
        return ret;
    }

    /// <summary>
    /// Any quantity smaller that this number may be rounded to zero.
    /// </summary>
    /// <returns></returns>
    public static decimal GetMinimumAllowedQty()
    {
        return c_e;
    }

    public static bool ApproximatelyZero(decimal a_x)
    {
        return Math.Abs(a_x) < c_e;
    }

    public static decimal AdjustForPossibleEvenMultipleByAdjustingQtyDown(decimal a_nbr, decimal a_divisor)
    {
        decimal remainder = a_nbr % a_divisor;

        if (remainder > 0 && ApproximatelyZero(remainder))
        {
            decimal adjustedNbr = a_nbr - remainder;
            return adjustedNbr;
        }

        return a_nbr;
    }

    public static decimal RtnPrimaryValIfApproxEq(decimal a_primaryValue, decimal a_value)
    {
        if (ApproximatelyEqual(a_primaryValue, a_value))
        {
            return a_primaryValue;
        }

        return a_value;
    }
}

public class Rounding
{
    //static void Main(string[] args)
    //{
    //    for (int i = -100; i <= 99; ++i)
    //    {
    //        if (i % 10 == 0)
    //            Console.WriteLine();

    //        long rounded = Rounding.Round(i, Rounding.Place.Hundreds);
    //        Console.Write(rounded + "; ");
    //    }
    //    Console.WriteLine();
    //}

    public enum EPlace
    {
        None,
        Tens, // When rounding a timespan, round to the nearest a millionth of a second
        Hundreds, // When rounding a timespan, round to the nearest a hundred thousandths of a second
        Thousands, // When rounding a timespan, round to the nearest a ten thousandths of a second
        TenThousands, // When rounding a timespan, round to the nearest a thousandths of a second
        HundredThousands, // When rounding a timespan, round to the nearest a hundredth of a second
        Millions, // When rounding a timespan, round to the nearest tenth of a second
        TenMillions, // When rounding a timespan, round to the nearest second
        HundredMillions,
        Billions,
        TenBillions,
        HundredBillions,
        Trillions,
        TenTrillions,
        HundredTrillions,
        Quadrillions,
        TenQuadrillions,
        HundredQuadrillions,
        QuinTrillions
    }

    /// <summary>
    /// These are used in combination with Place to determine the quotient and remainder.
    /// </summary>
    private static readonly long[] s_divisors;

    static Rounding()
    {
        s_divisors = new long[(long)EPlace.QuinTrillions + 1];
        s_divisors[0] = 1;

        for (int i = 1; i < s_divisors.Length; ++i)
        {
            s_divisors[i] = s_divisors[i - 1] * 10;
        }

        s_roundPoints = new long[s_divisors.Length];
        s_roundPoints[0] = 1;
        for (int i = 1; i < s_divisors.Length; ++i)
        {
            s_roundPoints[i] = 5 * s_divisors[i - 1];
        }
    }

    /// <summary>
    /// These are used to adjust the round point by the place.
    /// For instance if the place is Tens, the round point is 5.
    /// If the place is Hundreds, the round point is 50.
    /// </summary>
    private static readonly long[] s_roundPoints;

    /// <summary>
    /// Round a long to the nearest Tens, Hundreds, Thousands, etc. Rounding is away from 0. IE 0.5 would round to 1. -0.5 would round to -1.
    /// </summary>
    /// <param name="a_dividend">The number to round.</param>
    /// <param name="a_place">The place to round up to.</param>
    /// <returns>The rounded number.</returns>
    public static long Round(long a_dividend, EPlace a_place)
    {
        long quotient;
        long remainder;

        long divisor = s_divisors[(int)a_place];
        long roundPoint = s_roundPoints[(int)a_place];

        quotient = Math.DivRem(a_dividend, divisor, out remainder);

        long roundedValue = quotient * divisor;
        if (remainder >= roundPoint)
        {
            // The dividend was greater than 0.
            roundedValue += divisor;
        }
        else if (remainder <= -roundPoint)
        {
            // The dividend was less than 0.
            roundedValue -= divisor;
        }

        #if DEBUG
        long test = quotient * divisor + remainder;
        if (test != a_dividend)
        {
            throw new Exception("ERROR!!!!!");
        }
        #endif

        return roundedValue;
    }
}