using System.Collections;

using PT.Common.PTMath;

namespace PT.Scheduler;

/// <summary>
/// Stores TrackActual data for enumeration
/// </summary>
public class CleanoutHistoryData : IEnumerable<CleanoutHistory>
{
    private List<CleanoutHistory> m_sortedHistory = new (); //Finished activities in this resource collection starting with the most recently finished.
    private bool m_sorted;
    public bool Empty => m_sortedHistory.Count == 0;

    internal void ResetSimulationVariables()
    {
        m_sortedHistory.Clear();
        m_sorted = false;
    }

    internal void AddCleanoutHistory(long a_startDate, long a_endDate, InternalActivity a_activity)
    {
        m_sortedHistory.Add(new (a_startDate, a_endDate, a_activity));
        m_sorted = false;
    }

    /// <summary>
    /// Sorts track actual data for enumeration. Orders by most recently finished.
    /// If bad data exists for multiple activities starting and/or ending at the same time, it will sort by start date then ID.
    /// </summary>
    private void Sort()
    {
        if (!m_sorted)
        {
            m_sortedHistory = m_sortedHistory.OrderByDescending(x => x.EndTicks).ThenByDescending(x => x.StartTicks).ThenBy(x => x.Activity.Id).ToList();
        }

        m_sorted = true;
    }

    /// <summary>
    /// Returns the most recently finished <see cref="InternalActivity"/>.
    /// If the history is empty, it returns <c>null</c>.
    /// </summary>
    internal InternalActivity LastRunActivity
    {
        get 
        { 
            Sort();

            if (Empty) return null;

            return m_sortedHistory[0].Activity;
        }
    }

    /// <summary>
    /// Returns the most recently finished <see cref="InternalActivity"/> reported end ticks.
    /// If the history is empty, it returns <c>null</c>.
    /// </summary>
    internal long LastRunEndTicks
    {
        get
        {
            Sort();

            if (Empty) return 0;

            return m_sortedHistory[0].EndTicks;
        }
    }

    /// <summary>
    /// Returns this finished cleanout data starting with the most recently finished.
    /// </summary>
    public IEnumerator<CleanoutHistory> GetEnumerator()
    {
        Sort();
        for (int i = 0; i < m_sortedHistory.Count; i++)
        {
            yield return m_sortedHistory[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Get the activity at the specified index, starting with the most recently finished one.
    /// </summary>
    public InternalActivity GetByIndex(int a_index)
    {
        if (m_sortedHistory.Count > a_index)
        {
            Sort();
            return m_sortedHistory[a_index].Activity;
        }

        return null;
    }

    public IEnumerable<InternalActivity> GetActivityEnumerator()
    {
        Sort();
        for (int i = 0; i < m_sortedHistory.Count; i++)
        {
            yield return m_sortedHistory[i].Activity;
        }
    }

    public InternalActivity GetLastRunActivityBeforeSimClock(long a_simClock)
    {
        Sort();

        //
        if (a_simClock == -1)
        {
            if (m_sortedHistory.Count > 0)
            {
                return m_sortedHistory[0].Activity;
            }

            return null;
        }

        for (var i = 0; i < m_sortedHistory.Count; i++)
        {
            if (m_sortedHistory[i].Activity.ReportedStartDateTicks > a_simClock)
            {
                continue;
            }

            return m_sortedHistory[i].Activity;
        }

        return null;
    }
}

public class CleanoutHistory : Interval
{
    public InternalActivity Activity;

    internal CleanoutHistory(long a_start, long a_end, InternalActivity a_activity) 
        : base(a_start, a_end)
    {
        Activity = a_activity;
    }
}
