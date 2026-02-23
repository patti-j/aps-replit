using System.Collections;

using PT.Common.Range;

namespace PT.Common.Collections;

public class CapacityIntervalCollection : ISearchableRangeCollection
{
    private readonly List<ISearchableRange> m_intervals;

    public CapacityIntervalCollection(int a_maxCount)
    {
        m_intervals = new List<ISearchableRange>(a_maxCount);
    }

    public int Count => m_intervals.Count;

    public void Add(ISearchableRange a_range)
    {
        m_intervals.Add(a_range);
    }

    public IEnumerator<ISearchableRange> GetEnumerator()
    {
        return m_intervals.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ISearchableRange GetByIdx(int a_idx)
    {
        return m_intervals[a_idx];
    }

    public int IndexOf(ISearchableRange a_range)
    {
        return m_intervals.IndexOf(a_range);
    }
}