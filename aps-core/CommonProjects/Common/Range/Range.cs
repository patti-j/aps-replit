namespace PT.Common.Range;

public class Range<T> where T : IComparable<T>
{
    public Range(T a_start, T a_end)
    {
        Start = a_start;
        End = a_end;

        if (Start.CompareTo(End) > 0)
        {
            ThrowException();
        }
    }

    public T Start, End;

    public override string ToString()
    {
        return PrintRange(Start, End);
    }

    public static string PrintRange(T a_start, T a_end)
    {
        return string.Format("[{0}, {1}]", a_start, a_end);
    }

    public virtual void ThrowException()
    {
        throw new CommonException("2736: The start must be less than or equal to the end." + ToString());
    }
}