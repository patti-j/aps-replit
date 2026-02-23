namespace PT.Common.Range;

/// <summary>
/// Class for performing binary search through a sorted collection of date ranges to find the range containing a point in time.
/// </summary>
public class GenericRangeSearch
{
    /// <summary>
    /// Enumerator representing various return options in the FindRange method.
    /// </summary>
    public enum EReturnOption
    {
        /// <summary>
        /// Returns the earliest range.
        /// </summary>
        Earlier,

        /// <summary>
        /// Returns the latest range.
        /// </summary>
        Later,

        /// <summary>
        /// Returns no range.
        /// </summary>
        None
    }

    /// <summary>
    /// Perform a binary search through a sorted collection of date ranges to find the range containing a point in time. Pass an optional mid point to use as
    /// the initial split point if it is known that the point in time is more likely to be near the beginning of the date ranges. This can slightly improve
    /// the performance of the algorithm
    /// </summary>
    /// <param name="a_rangeCollection"></param>
    /// <param name="a_dt"></param>
    /// <param name="a_returnOption"></param>
    /// <param name="a_optionalMidPoint"></param>
    /// <returns></returns>
    public static ISearchableRange FindRange(ISearchableRangeCollection a_rangeCollection, DateTime a_dt, EReturnOption a_returnOption = EReturnOption.None, int a_optionalMidPoint = -1)
    {
        if (a_rangeCollection == null || a_rangeCollection.Count == 0)
        {
            return null;
        }

        //TODO: IMPROVE
        int startIdx = 0;
        int endIdx = a_rangeCollection.Count - 1;
        int midIdx = a_optionalMidPoint != -1 ? a_optionalMidPoint : endIdx / 2;

        if (a_rangeCollection.Count > 1)
        {
            while (endIdx - startIdx != 1)
            {
                if (a_rangeCollection.GetByIdx(midIdx).Start >= a_dt)
                {
                    endIdx = midIdx;
                }
                else
                {
                    startIdx = midIdx;
                }

                midIdx = (endIdx + startIdx) / 2;
            }
        }

        ISearchableRange lastRange = a_rangeCollection.GetByIdx(startIdx);
        if (lastRange.ContainsPoint(a_dt))
        {
            return lastRange;
        }

        if (lastRange.End < a_dt)
        {
            ISearchableRange nextRange = a_rangeCollection.GetByIdx(endIdx);
            if (nextRange.ContainsPoint(a_dt))
            {
                return nextRange;
            }
        }

        return null;
    }

    /*
     Improvements made (From Chat GPT):

        Instead of checking endIdx - startIdx != 1, the condition in the while loop is startIdx <= endIdx. This ensures that all elements are being checked.
        The calculation of the midIdx has been improved to avoid potential integer overflow.
        The a_rangeCollection.Count > 1 condition is removed as the new while loop condition (startIdx <= endIdx) already handles the case when the collection only has one element.
        The midIdx range is checked for containing the point a_dt in the while loop itself, improving the algorithm's efficiency.
        The code after the while loop is removed as it's redundant now. The optimized version of the binary search will have already returned the correct range if it exists or null if the range doesn't exist.

        Note: Make sure your collection is sorted by the Start of ISearchableRange as Binary Search only works on sorted collections. Also, make sure that ContainsPoint checks if the DateTime point is in between the start and end of the range, inclusive of the boundaries.
     */
    //public static ISearchableRange FindRange(ISearchableRangeCollection a_rangeCollection, DateTime a_dt, EReturnOption a_returnOption = EReturnOption.None, int a_optionalMidPoint = -1)
    //{
    //    if (a_rangeCollection == null || a_rangeCollection.Count == 0)
    //    {
    //        return null;
    //    }

    //    int startIdx = 0;
    //    int endIdx = a_rangeCollection.Count - 1;

    //    while (startIdx <= endIdx)
    //    {
    //        int midIdx = a_optionalMidPoint != -1 ? a_optionalMidPoint : startIdx + (endIdx - startIdx) / 2;

    //        ISearchableRange midRange = a_rangeCollection.GetByIdx(midIdx);

    //        if (midRange.ContainsPoint(a_dt))
    //        {
    //            return midRange;
    //        }

    //        if (midRange.Start > a_dt)
    //        {
    //            endIdx = midIdx - 1;
    //        }
    //        else
    //        {
    //            startIdx = midIdx + 1;
    //        }
    //    }

    //    return null;
    //}

    /// <summary>
    /// Perform a binary search through a sorted collection of date ranges to find the range containing a point in time. Pass an optional mid point to use as
    /// the initial split point if it is known that the point in time is more likely to be near the beginning of the date ranges. This can slightly improve
    /// the performance of the algorithm
    /// </summary>
    /// <param name="a_rangeCollection"></param>
    /// <param name="a_dt"></param>
    /// <param name="a_optionalMidPoint"></param>
    /// <returns></returns>
    public static ISearchableRange FindRangeOrEarlier(ISearchableRangeCollection a_rangeCollection, DateTime a_dt, int a_optionalMidPoint = -1)
    {
        if (a_rangeCollection == null || a_rangeCollection.Count == 0)
        {
            return null;
        }

        int startIdx = 0;
        int endIdx = a_rangeCollection.Count - 1;
        int midIdx = a_optionalMidPoint != -1 ? a_optionalMidPoint : endIdx / 2;

        if (a_rangeCollection.Count > 1)
        {
            while (endIdx - startIdx != 1)
            {
                if (a_rangeCollection.GetByIdx(midIdx).Start >= a_dt)
                {
                    endIdx = midIdx;
                }
                else
                {
                    startIdx = midIdx;
                }

                midIdx = (endIdx + startIdx) / 2;
            }
        }

        ISearchableRange lastRange = a_rangeCollection.GetByIdx(startIdx);
        if (lastRange.ContainsPoint(a_dt))
        {
            return lastRange;
        }

        if (lastRange.End < a_dt)
        {
            ISearchableRange nextRange = a_rangeCollection.GetByIdx(endIdx);
            if (nextRange.ContainsPoint(a_dt) || nextRange.End < a_dt)
            {
                return nextRange;
            }

            return lastRange;
        }

        return null;
    }

    /// <summary>
    /// Perform a binary search through a sorted collection of date ranges to find the range containing a point in time. Pass an optional mid point to use as
    /// the initial split point if it is known that the point in time is more likely to be near the beginning of the date ranges. This can slightly improve
    /// the performance of the algorithm.
    /// </summary>
    /// <param name="a_rangeCollection">The collection of searchable ranges.</param>
    /// <param name="a_dt">The point in time to find the range for.</param>
    /// <param name="a_optionalMidPoint">An optional midpoint to use for the initial split. The default value is -1.</param>
    /// <returns>The latest range containing the given point in time.</returns>
    public static ISearchableRange FindRangeOrNext(ISearchableRangeCollection a_rangeCollection, DateTime a_dt, int a_optionalMidPoint = -1)
    {
        if (a_rangeCollection == null || a_rangeCollection.Count == 0)
        {
            return null;
        }

        int startIdx = 0;
        int endIdx = a_rangeCollection.Count - 1;
        int midIdx = a_optionalMidPoint != -1 ? a_optionalMidPoint : endIdx / 2;

        if (a_rangeCollection.Count > 1)
        {
            while (endIdx - startIdx != 1)
            {
                if (a_rangeCollection.GetByIdx(midIdx).Start >= a_dt)
                {
                    endIdx = midIdx;
                }
                else
                {
                    startIdx = midIdx;
                }

                midIdx = (endIdx + startIdx) / 2;
            }
        }

        ISearchableRange finalRange = a_rangeCollection.GetByIdx(startIdx);
        if (finalRange.ContainsPoint(a_dt))
        {
            return finalRange;
        }

        if (finalRange.End < a_dt)
        {
            ISearchableRange nextRange = a_rangeCollection.GetByIdx(endIdx);
            return nextRange;
        }

        return finalRange;
    }

    public static DateTime? FitDateToEarlierRange(ISearchableRangeCollection a_rangeCollection, DateTime a_dt, out ISearchableRange o_searchableRange)
    {
        o_searchableRange = null;

        if (a_rangeCollection == null || a_rangeCollection.Count == 0)
        {
            return null;
        }

        ISearchableRange searchableRange = FindRange(a_rangeCollection, a_dt, EReturnOption.Earlier);
        if (searchableRange == null)
        {
            return null;
        }

        o_searchableRange = searchableRange;

        if (searchableRange.ContainsPoint(a_dt))
        {
            return a_dt;
        }

        return searchableRange.End;
    }

    public static DateTime? FitDateToNextRange(ISearchableRangeCollection a_rangeCollection, DateTime a_dt, out ISearchableRange o_searchableRange)
    {
        o_searchableRange = null;

        if (a_rangeCollection == null || a_rangeCollection.Count == 0)
        {
            return null;
        }

        int startIdx = 0;
        int endIdx = a_rangeCollection.Count - 1;
        int midIdx = endIdx / 2;

        if (a_rangeCollection.Count > 1)
        {
            while (endIdx - startIdx != 1)
            {
                if (a_rangeCollection.GetByIdx(midIdx).Start >= a_dt)
                {
                    endIdx = midIdx;
                }
                else
                {
                    startIdx = midIdx;
                }

                midIdx = (endIdx + startIdx) / 2;
            }
        }

        ISearchableRange lastRange = a_rangeCollection.GetByIdx(startIdx);
        if (lastRange.ContainsPoint(a_dt))
        {
            o_searchableRange = lastRange;
            return a_dt;
        }

        if (lastRange.End < a_dt)
        {
            ISearchableRange nextRange = a_rangeCollection.GetByIdx(endIdx);

            o_searchableRange = nextRange;

            if (nextRange.ContainsPoint(a_dt))
            {
                return a_dt;
            }

            return nextRange.Start;
        }


        return null;
    }
}