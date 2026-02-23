namespace PT.Scheduler;

/// <summary>
/// Stores a list of InternalActivities.
/// </summary>
public class ActivitiesCollection : ICopyTable
{
    private readonly List<InternalActivity> m_activities = new ();

    #region Properties and Methods
    public Type ElementType => typeof(InternalActivity);

    public InternalActivity Add(InternalActivity a_activity)
    {
        m_activities.Add(a_activity);
        return a_activity;
    }

    public void Remove(int a_index)
    {
        m_activities.RemoveAt(a_index);
    }

    public object GetRow(int a_index)
    {
        return m_activities[a_index];
    }

    public InternalActivity this[int a_idx] => m_activities[a_idx];

    public int Count => m_activities.Count;
    #endregion

    #region Debug
    internal string de
    {
        get
        {
            string test;
            test = string.Format("Count={0}", Count);
            return test;
        }
    }
    #endregion

    public override string ToString()
    {
        string s = string.Format("Activities.Count={0}", m_activities.Count);

        if (Count == 1)
        {
            s = string.Format("{0}; Activity={1}", s, m_activities[0]);
        }

        return s;
    }
}