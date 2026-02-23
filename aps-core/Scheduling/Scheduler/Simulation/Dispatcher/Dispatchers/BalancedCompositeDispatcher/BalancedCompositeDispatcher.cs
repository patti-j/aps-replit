namespace PT.Scheduler;

/// <summary>
/// Summary description for BalancedCompositeDispatcher.
/// </summary>
internal class BalancedCompositeDispatcher : SortedListDispatcher
{
    private readonly BalancedCompositeDispatcherDefinition m_balancedCompositeDispatcherDefinition;

    public BalancedCompositeDispatcher(BalancedCompositeDispatcher a_od)
        : base(a_od)
    {
        m_balancedCompositeDispatcherDefinition = a_od.m_balancedCompositeDispatcherDefinition;
    }

    internal BalancedCompositeDispatcher(BalancedCompositeDispatcherDefinition a_dispatcherDefinition)
        : base(a_dispatcherDefinition)
    {
        m_balancedCompositeDispatcherDefinition = a_dispatcherDefinition;
    }

    protected override KeyAndActivity CreateKey(InternalActivity a_activity, int a_activityIdx)
    {
        decimal compositeScore = m_balancedCompositeDispatcherDefinition.ComputeComposite(Resource, a_activity);

        if (a_activity.MinimumScoreNotMet)
        {
            return null;
        }

        BalancedCompositeDispatcherDefinition.Key key = new (compositeScore, a_activity.Id.Value, a_activity.OriginalSimultaneousSequenceIdxScore);
        return new KeyAndActivity(key, a_activity);
    }

    public override void BeginDispatch(long a_time, bool a_optimize, bool a_enforceKeepSuccessors)
    {
        bool enforceKeepSuccessorsOverride;

        if (BalancedCompositeDispatcherDefinition.OnlyDispatchBestComposites)
        {
            enforceKeepSuccessorsOverride = false;
        }
        else
        {
            enforceKeepSuccessorsOverride = a_enforceKeepSuccessors;
        }

        base.BeginDispatch(a_time, a_optimize, enforceKeepSuccessorsOverride);

        if (BalancedCompositeDispatcherDefinition.OnlyDispatchBestComposites)
        {
            decimal bestComposite = 0;
            List<KeyAndActivity> updatedCompositeSort = new ();

            for (int compositeSortIdx = 0; compositeSortIdx < m_compositeSort.Count; ++compositeSortIdx)
            {
                KeyAndActivity keyAndObj = m_compositeSort[compositeSortIdx];
                BalancedCompositeDispatcherDefinition.Key key = (BalancedCompositeDispatcherDefinition.Key)keyAndObj.Key;

                if (compositeSortIdx == 0)
                {
                    bestComposite = key.Composite;
                    updatedCompositeSort.Add(keyAndObj);
                }
                else
                {
                    if (key.Composite == bestComposite)
                    {
                        updatedCompositeSort.Add(keyAndObj);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            m_compositeSort = updatedCompositeSort;
        }
    }

    public BalancedCompositeDispatcherDefinition BalancedCompositeDispatcherDefinition => (BalancedCompositeDispatcherDefinition)DispatcherDefinition;

    public override object Clone()
    {
        return new BalancedCompositeDispatcher(this);
    }
}