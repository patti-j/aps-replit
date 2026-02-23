namespace PT.Common.Range;

public interface ISearchableRangeCollection : IEnumerable<ISearchableRange>
{
    /// <summary>
    /// Number of ranges in the collection
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Get range by index
    /// </summary>
    /// <param name="a_idx"></param>
    /// <returns></returns>
    ISearchableRange GetByIdx(int a_idx);

    int IndexOf(ISearchableRange a_searchableRange);
}

public interface ISearchableRange
{
    /// <summary>
    /// Range start time
    /// </summary>
    DateTime Start { get; }

    /// <summary>
    /// Range end time
    /// </summary>
    DateTime End { get; }

    /// <summary>
    /// Check if range contains point in time
    /// </summary>
    /// <param name="a_dt"></param>
    /// <returns></returns>
    bool ContainsPoint(DateTime a_dt);
}

public readonly struct SearchableRange
{
    public long Start { get; }
    public long End { get; }
    
    public SearchableRange(long a_start, long a_end)
    {
        Start = a_start;
        End = a_end;
    }

    public bool ContainsPoint(long a_dt)
    {
        return a_dt >= Start && a_dt <= End;
    }
}