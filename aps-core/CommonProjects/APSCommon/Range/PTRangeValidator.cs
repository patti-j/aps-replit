namespace PT.APSCommon.Range;

public class PTRangeValidator<T> : PT.Common.Range.RangeValidator<T> where T : IComparable<T>
{
    public override void ThrowException(PT.Common.Range.Range<T> a_r1, PT.Common.Range.Range<T> a_r2)
    {
        throw new PTRangeException("2731", new object[] { a_r1, a_r2 });
    }
}