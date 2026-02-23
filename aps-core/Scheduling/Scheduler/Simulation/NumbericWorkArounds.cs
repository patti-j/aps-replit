namespace PT.Scheduler.Simulation;

internal class NumbericWorkArounds
{
    // Don't use this one. Use FloatingPoint in PT.Common.
    // quantity
    // 9999999.99999999 possible max; 
    // allowing for 7 digits greater than 0.
    // 6 digits less than 0, any digits beyond that are for errors, truncate. (In the debugger digits beyond this point would are rounded)
    // ********************************************************************************************************************************************
    // Get rid of this function. Use the new FloatingPoint class defined in common that contains a functional 
    // Called ApproximatelyEqual
    // ********************************************************************************************************************************************
    //public static bool ApproxEqual(decimal d1, decimal d2)
    //{
    //    if (d1 == d2)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        const int DECIMALS = 6;

    //        decimal r1 = Math.Round(d1, DECIMALS, MidpointRounding.AwayFromZero);
    //        decimal r2 = Math.Round(d2, DECIMALS, MidpointRounding.AwayFromZero);

    //        return r1 == r2;
    //    }
    //}
}