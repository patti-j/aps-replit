namespace PT.Common.Range;

public class RangeValidator<T> where T : IComparable<T>
{
    private readonly List<Range<T>> m_ranges = new ();

    public void Add(T a_start, T a_end)
    {
        m_ranges.Add(new Range<T>(a_start, a_end));
    }

    public void Add(Range<T> a_range)
    {
        m_ranges.Add(a_range);
    }

    public void Validate()
    {
        m_ranges.Sort(Compare);

        #if DEBUG
        for (int i = 1; i < m_ranges.Count; ++i)
        {
            Range<T> r1 = m_ranges[i - 1];
            Range<T> r2 = m_ranges[i];

            if (r1.End.CompareTo(r2.Start) >= 0)
            {
                ThrowException(r1, r2);
            }
        }
        #endif
    }

    private int Compare(Range<T> a_r1, Range<T> a_r2)
    {
        if (a_r1.Start.CompareTo(a_r2.Start) < 0)
        {
            return -1;
        }

        if (a_r1.Start.CompareTo(a_r2.Start) > 0)
        {
            return 1;
        }

        return 0;
    }

    public virtual void ThrowException(Range<T> a_r1, Range<T> a_r2)
    {
        throw new RangeException(string.Format("2731: Ranges {0} and {1} overlap. Ranges aren't allowed to overlap.", a_r1, a_r2));
    }

    private enum EPointType { Start, End }

    private static int ComparePoint(Tuple<T, EPointType> p1, Tuple<T, EPointType> p2)
    {
        if (p1.Item1.CompareTo(p2.Item1) < 0)
        {
            return -1;
        }

        if (p1.Item1.CompareTo(p2.Item1) == 0)
        {
            if (p1.Item2 < p2.Item2)
            {
                return -1;
            }

            if (p1.Item2 == p2.Item2)
            {
                return 0;
            }

            return 1;
        }

        return 1;
    }

    /// <summary>
    /// Finds the first and last point in which any intervals intersect the most often
    /// </summary>
    /// <param name="o_firstMaxIntersectionPoint">The first point at which the most intersection occur</param>
    /// <param name="o_lastMaxIntersectionPoint">The last point at which the most intersection occur</param>
    /// <returns>The maximum number of ranges that intersected at once</returns>
    public int FindIntersection(out T o_firstMaxIntersectionPoint, out T o_lastMaxIntersectionPoint)
    {
        List<Tuple<T, EPointType>> pointList = new ();
        foreach (Range<T> range in m_ranges)
        {
            pointList.Add(new Tuple<T, EPointType>(range.Start, EPointType.Start));
            pointList.Add(new Tuple<T, EPointType>(range.End, EPointType.End));
        }

        pointList.Sort(ComparePoint);

        int currentIntersections = 0;
        int maxIntersections = 0;
        o_firstMaxIntersectionPoint = default(T);
        o_lastMaxIntersectionPoint = default(T);

        for (int rI = 0; rI < pointList.Count; rI++)
        {
            Tuple<T, EPointType> point = pointList[rI];
            if (point.Item2 == EPointType.Start)
            {
                currentIntersections++;
                if (currentIntersections > maxIntersections)
                {
                    maxIntersections = currentIntersections;
                    o_firstMaxIntersectionPoint = point.Item1;
                    o_lastMaxIntersectionPoint = point.Item1;
                }
                else if (currentIntersections == maxIntersections)
                {
                    o_lastMaxIntersectionPoint = point.Item1;
                }
            }
            else
            {
                currentIntersections--;
            }
        }

        return maxIntersections;
    }
}