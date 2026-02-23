namespace PT.Common;

public class RandomHelper
{
    public static long GetNext(Random aR, long aMin, long aMax)
    {
        decimal min = aMin;
        decimal max = aMax;
        return (long)GetNext(aR, min, max);
    }

    public static decimal GetNext(Random aR, decimal aMin, decimal aMax)
    {
        decimal spread = aMax - aMin;
        decimal random = (decimal)aR.NextDouble();
        decimal offset = random * spread;
        decimal value = aMin + offset;
        return value;
    }

    public static double GetNext(Random aR, double aMin, double aMax)
    {
        decimal min = (decimal)aMin;
        decimal max = (decimal)aMax;
        return (double)GetNext(aR, min, max);
    }
}