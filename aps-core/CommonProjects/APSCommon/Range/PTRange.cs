namespace PT.APSCommon.Range;

public class PTRange<T> : PT.Common.Range.Range<T> where T : IComparable<T>
{
    public PTRange(T a_start, T a_end)
        : base(a_start, a_end) { }

    public override void ThrowException()
    {
        throw new PTRangeException("2736", new object[] { ToString() });
    }
}