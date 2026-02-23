namespace PT.Scheduler.Simulation.Customizations;

/// <summary>
/// Provides change helper classes for activities and capacity intervals.
/// </summary>
public class SchedulabilityChangeHelper
{
    private readonly List<ActivityValueChanges> m_actChanges;
    private readonly List<BaseOperationValueChanges> m_opChanges;
    private readonly CapacityIntervalChanger m_ciChanges;
    private readonly object m_threadLock = new ();

    public SchedulabilityChangeHelper()
    {
        m_actChanges = new List<ActivityValueChanges>();
        m_opChanges = new List<BaseOperationValueChanges>();
        m_ciChanges = new CapacityIntervalChanger();
    }

    public void AddToDeleteSet(CapacityIntervalsCollection a_capSet, CapacityInterval a_interval)
    {
        lock (m_threadLock)
        {
            m_ciChanges.AddToDeleteSet(a_capSet, a_interval);
        }
    }

    /// <summary>
    /// Add pending changes for an activity
    /// </summary>
    /// <param name="a_actChanges"></param>
    public void AddActivityChanger(ActivityValueChanges a_changes)
    {
        lock (m_threadLock)
        {
            m_actChanges.Add(a_changes);
        }
    }

    /// <summary>
    /// Add pending changes for an activity
    /// </summary>
    /// <param name="a_actChanges"></param>
    public void AddOperationChanger(BaseOperationValueChanges a_changes)
    {
        lock (m_threadLock)
        {
            m_opChanges.Add(a_changes);
        }
    }

    public CapacityIntervalChanger CapacityChanges => m_ciChanges;

    /// <summary>
    /// Perform all pending changes for capacity intervals and activities.
    /// </summary>
    internal void Execute()
    {
        m_ciChanges.ExecuteDeletes();
        m_ciChanges.ExecuteAdds();
        m_ciChanges.ExecuteChanges();
        foreach (ActivityValueChanges change in m_actChanges)
        {
            change.ExecuteChanges();
        }

        m_actChanges.Clear();
        foreach (BaseOperationValueChanges change in m_opChanges)
        {
            change.ExecuteChanges();
        }

        m_opChanges.Clear();
    }
}