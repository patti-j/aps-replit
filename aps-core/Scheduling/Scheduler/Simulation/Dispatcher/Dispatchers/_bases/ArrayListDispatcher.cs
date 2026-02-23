using PT.APSCommon;
using PT.Common.Exceptions;

namespace PT.Scheduler;

internal abstract partial class ArrayListDispatcher : ReadyActivitiesDispatcher
{
    /// <summary>
    /// </summary>
    /// <param name="a_od">Original dispatcher.</param>
    protected ArrayListDispatcher(ArrayListDispatcher a_od) 
        : this(a_od.m_dispatcherDefinition)
    {
    }

    internal ArrayListDispatcher(DispatcherDefinition a_dispatcherDefinition)
    {
        m_dispatcherDefinition = a_dispatcherDefinition;
        m_activityValueCache = new Dictionary<BaseId, long>();
    }

    private readonly List<InternalActivity> m_activities = new ();
    /// <summary>
    /// This contains the ID and the activity sequence buffer end date
    /// </summary>
    private readonly Dictionary<BaseId, long> m_activityValueCache;


    #region m_activities wrapper functions. All changes to m_activities must occur through one of these functions to make it easier to debug.
    public override void Clear()
    {
        base.Clear();
        m_activities.Clear();
        m_activityValueCache.Clear();
    }

    public override void Add(InternalActivity a_activity, long a_sequenceHeadStartEndDate)
    {
        if (!m_activityValueCache.TryAdd(a_activity.Id, a_sequenceHeadStartEndDate))
        {
            throw new PTException("The activity has already been inserted into the dispatcher.");
        }

        m_activities.Add(a_activity);
    }

    public override bool Remove(InternalActivity a_activity)
    {
        if (m_activityValueCache.Remove(a_activity.Id))
        {
            return m_activities.Remove(a_activity);
        }

        return false;
    }

    internal override int Count => m_activities.Count;

    internal override bool Contains(InternalActivity a_act)
    {
        return m_activityValueCache.ContainsKey(a_act.Id);
    }
    #endregion m_activities wrapper functions

    private readonly DispatcherDefinition m_dispatcherDefinition;

    public override DispatcherDefinition DispatcherDefinition => m_dispatcherDefinition;

    #region IReadyActivitiesDispatcher Members
    protected InternalActivity GetActivity(int a_index)
    {
        return m_activities[a_index];
    }
    #endregion

    internal override Enumerator GetEnumeratorOfActivitiesOnDispatcher()
    {
        return new ArrayListDispatcherEnumerator(m_activities);
    }

    public override string ToString()
    {
        return "Activities in Queue = " + Count;
    }

    public override bool HasReadyActivity => m_hasReadyActivity;

    private bool m_hasReadyActivity;

    public override void BeginDispatch(long a_time, bool a_optimize, bool a_enforceKeepSuccessors)
    {
        if (!a_optimize)
        {
            m_hasReadyActivity = true;
            return;
        }

        m_hasReadyActivity = false;

        if (m_activityValueCache.Count > 0)
        {
            long minimumSequenceHeadStartEnd = m_activityValueCache.Values.Min();
            if (minimumSequenceHeadStartEnd <= a_time)
            {
                m_hasReadyActivity = true;
            }
        }
    }
}