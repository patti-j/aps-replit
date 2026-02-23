namespace PT.Scheduler;

/// <summary>
/// Used by Resource to compare ResourceSpans.
/// </summary>
internal class ResourceReservationComparer : IComparer<ResourceSpan>
{
    #region IComparer<TestData> Members
    public int Compare(ResourceSpan a_x, ResourceSpan a_y)
    {
        if (a_x.Start < a_y.Start)
        {
            return -1;
        }

        if (a_x.Start > a_y.Start)
        {
            return 1;
        }

        return 0;
    }
    #endregion
}