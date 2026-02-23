namespace PT.Scheduler;

/// <summary>
/// Summary description for SortedListDispatcher.
/// </summary>
internal abstract partial class SortedListDispatcher : ArrayListDispatcher
{
    internal SortedListDispatcher(DispatcherDefinition a_dispatcherDefinition)
        : base(a_dispatcherDefinition) { }

    internal SortedListDispatcher(SortedListDispatcher a_od)
        : base(a_od) { }

    protected List<KeyAndActivity> m_compositeSort = new ();

    #region SortedListDispatcher.m_compositeSort.
    /// <summary>
    /// positions correspond to array index -1.
    /// For instance Positions 1 corresponds to index position 0
    /// </summary>
    private int m_position;

    /// <summary>
    /// Clears the composite sorted list of activities. This function was added to make it easier to detect when the set is cleared.
    /// </summary>
    private void ClearCompositeSort()
    {
        m_compositeSort.Clear();
    }

    public override void BeginDispatch(long a_time, bool a_optimize, bool a_enforceKeepSuccessors)
    {
        base.BeginDispatch(a_time, a_optimize, a_enforceKeepSuccessors);

        m_position = 0;
        m_hasNextResourceActivityEligibleTime = false;
        ClearCompositeSort();

        if (!HasReadyActivity)
        {
            //We don't have any activities that can schedule now, so no need to sort
            return;
        }

        int addActivityIdx = 0;

        for (int activityIdx = 0; activityIdx < Count; activityIdx++)
        {
            InternalActivity activity = GetActivity(activityIdx);
            
            if (a_enforceKeepSuccessors)
            {
                KeepSuccessorResult ksr = EnforceKeepSuccessor(Resource, a_time, activity);

                switch (ksr)
                {
                    case KeepSuccessorResult.Normal:
                        AddActivityToCompositeSort(activity, addActivityIdx);
                        ++addActivityIdx;

                        break;

                    case KeepSuccessorResult.Skip:
                        break;

                    case KeepSuccessorResult.Exclusive:
                        ClearCompositeSort();
                        AddActivityToCompositeSort(activity, 0);
                        break;
                }
            }
            else
            {
                AddActivityToCompositeSort(activity, addActivityIdx);
                ++addActivityIdx;
            }
        }

        if (m_compositeSort.Count > 0)
        {
            m_compositeSort.Sort(DispatcherDefinition.Comparer);
        }
    }

    private void AddActivityToCompositeSort(InternalActivity a_activity, int a_index)
    {
        KeyAndActivity keyAndObj = CreateKey(a_activity, a_index);

        //object can be null if activity doesn't meet minimum score requirement 
        if (keyAndObj != null)
        {
            m_compositeSort.Add(keyAndObj);
        }
    }

    protected abstract KeyAndActivity CreateKey(InternalActivity a_activity, int a_activityIdx);

    public override void EndDispatch() { }

    /// <summary>
    /// Get the next activitiy off the dispatcher.
    /// </summary>
    /// <returns></returns>
    public override InternalActivity GetNext()
    {
        KeyAndActivity keyAndObj = GetNextKeyAndActivity();
        if (keyAndObj == null)
        {
            return null;
        }

        InternalActivity activity = keyAndObj.Activity;

        return activity;
    }

    public override KeyAndActivity GetNextKeyAndActivity()
    {
        if (m_position > m_compositeSort.Count - 1)
        {
            return null;
        }

        KeyAndActivity keyAndObj = m_compositeSort[m_position];
        ++m_position;

        return keyAndObj;
    }

    /// <summary>
    /// Peek at the next activity on the dispatcher.
    /// </summary>
    /// <returns></returns>
    public override InternalActivity PeekNext()
    {
        KeyAndActivity keyAndObj = PeekNextKeyAndActivity();
        if (keyAndObj == null)
        {
            return null;
        }

        InternalActivity activity = keyAndObj.Activity;
        return activity;
    }

    public override KeyAndActivity PeekNextKeyAndActivity()
    {
        if (m_position > m_compositeSort.Count - 1)
        {
            return null;
        }

        KeyAndActivity keyAndObj = m_compositeSort[m_position];
        return keyAndObj;
    }

    #region INextResourceActivityReadyTime Members
    private bool m_hasNextResourceActivityEligibleTime;

    public bool HasNextResourceActivityEligibleTime => m_hasNextResourceActivityEligibleTime;
    #endregion

    protected override Enumerator GetInternalActivityTemplateEnumerator()
    {
        return new SortedListDispatcherEnumerator(this);
    }

    /// <summary>
    /// This override removes the activity from the composite sort.
    /// It runs in the time it takes to search linearly, however scheduling activities are likely at the front of the list.
    /// If this is ever found to be slow, consider replacing the m_compositeSort data structure with a
    /// linked list and store the activity's linked list node in the activity so it can be deleted from the list without any overhead.
    /// </summary>
    /// <param name="a_activity"></param>
    public override bool Remove(InternalActivity a_activity)
    {
        for (int i = 0; i < m_compositeSort.Count; ++i)
        {
            KeyAndActivity ka = m_compositeSort[i];
            if (ka.Activity == a_activity)
            {
                m_compositeSort.RemoveAt(i);

                //We must keep the position in line with the length of the array.
                if (i < m_position)
                {
                    //If this activity scheduled or was otherwise removed and it is before the current position
                    //  we need to update the position in order to not skip an activity.
                    // Note that m_position gets updated before the activity at that position can be removed.
                    //   So when i == m_position nothing will be skipped. 
                    m_position--;
                }
            }
        }

        //It's possible that the m_composite sort doesn't have this activity yet as this resource may not have attempted to schedule
        //Return the removal from the base collection
        return base.Remove(a_activity);
    }
    #endregion
}