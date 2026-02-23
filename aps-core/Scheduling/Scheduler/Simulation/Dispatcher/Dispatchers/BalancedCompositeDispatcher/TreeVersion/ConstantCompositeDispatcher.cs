using PT.APSCommon;
using PT.Common.Collections;

namespace PT.Scheduler.Simulation.Dispatcher;

/// <summary>
/// Use this dispatcher when the composite value never changes. Some examples of values that don't change include: JobNeedDate, Priority, and Hot.
/// Values related to setup may cause the composite value to change as blocks are scheduled on a resource.
/// </summary>
internal partial class ConstantCompositeDispatcher : ReadyActivitiesDispatcher
{
    private readonly AVLTree<ConstantCompositeKey, KeyAndActivity> m_tree = new (new ConstantCompositeKeyComparer());
    private readonly AVLTree<ConstantCompositeKey, KeyAndActivity>.Enumerator m_treeEtr;
    private readonly BalancedCompositeDispatcherDefinition m_dispatcherDefinition;

    public ConstantCompositeDispatcher(BalancedCompositeDispatcherDefinition a_dispatcherDefinition)
    {
        m_dispatcherDefinition = a_dispatcherDefinition;
        m_treeEtr = m_tree.GetEnumerator();
    }

    public ConstantCompositeDispatcher(ConstantCompositeDispatcher a_scd)
        : this(a_scd.m_dispatcherDefinition) { }

    public override void Clear()
    {
        base.Clear();

        m_tree.Clear();
        m_peekKeyAndActivity = null;
        m_dispatcherChangedSinceLastKeepSuccesssorNoInterruptProcessing = false;
    }

    public override void Add(InternalActivity a_activity, long a_sequenceHeadStartEndDate)
    {
        if (!a_activity.StaticCompositeSet)
        {
            a_activity.StaticCompositeSet = true;
            a_activity.m_staticComposite = m_dispatcherDefinition.ComputeComposite(Resource, a_activity, false);
        }

        BalancedCompositeDispatcherDefinition.Key key = new (a_activity.m_staticComposite, a_activity.Id.Value, a_activity.OriginalSimultaneousSequenceIdxScore);
        KeyAndActivity ka = new (key, a_activity);
        ConstantCompositeKey staticCompositeKey = CreateStaticCompositeValue(a_activity);
        m_tree.Add(new AVLTree<ConstantCompositeKey, KeyAndActivity>.TreeNode(staticCompositeKey, ka));

        m_dispatcherChangedSinceLastKeepSuccesssorNoInterruptProcessing = true;
    }

    public override bool HasReadyActivity => Count > 0;

    internal override int Count => m_tree.Count;

    //TODO: We need to remove by activity.Id. The Key from this activity can change when it schedules (the simultaneous sequence ID)
    public override bool Remove(InternalActivity a_activity)
    {
        bool removed = m_tree.Remove(CreateStaticCompositeValue(a_activity));
        m_treeEtr.Reset();
        m_dispatcherChangedSinceLastKeepSuccesssorNoInterruptProcessing = true;
        return removed;
    }

    private static ConstantCompositeKey CreateStaticCompositeValue(InternalActivity a_act)
    {
        return new ConstantCompositeKey(a_act.m_staticComposite, a_act.Id.Value, a_act.OriginalSimultaneousSequenceIdxScore);
    }

    public override InternalActivity GetNext()
    {
        KeyAndActivity ka = GetNextKeyAndActivity();

        if (ka != null)
        {
            return ka.Activity;
        }

        return null;
    }

    public override KeyAndActivity GetNextKeyAndActivity()
    {
        if (m_peekKeyAndActivity == null)
        {
            PeekNextKeyAndActivity();
        }

        KeyAndActivity tmp = m_peekKeyAndActivity;
        m_peekKeyAndActivity = null;

        return tmp;
    }

    private KeyAndActivity m_peekKeyAndActivity;

    public override InternalActivity PeekNext()
    {
        KeyAndActivity ka = PeekNextKeyAndActivity();

        if (ka != null)
        {
            return ka.Activity;
        }

        return null;
    }

    private decimal m_bestComposite;
    private bool m_hasBestComposite;

    public override KeyAndActivity PeekNextKeyAndActivity()
    {
        if (m_peekKeyAndActivity == null)
        {
            if (m_dispatcherDefinition.OnlyDispatchBestComposites)
            {
                if (m_hasBestComposite)
                {
                    m_treeEtr.MoveNext();
                    BalancedCompositeDispatcherDefinition.Key key = (BalancedCompositeDispatcherDefinition.Key)m_treeEtr.Current.Value.Key;

                    if (m_hasBestComposite = m_bestComposite == key.Composite)
                    {
                        m_peekKeyAndActivity = m_treeEtr.Current.Value;
                    }
                }
            }
            else if (m_enforceKeepSuccessor)
            {
                if (!m_exclusiveKeepSuccessorActive)
                {
                    while (m_treeEtr.MoveNext())
                    {
                        if (EnforceKeepSuccessor(Resource, m_time, m_treeEtr.Current.Value.Activity) != KeepSuccessorResult.Skip)
                        {
                            m_peekKeyAndActivity = m_treeEtr.Current.Value;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (m_treeEtr.MoveNext())
                {
                    m_peekKeyAndActivity = m_treeEtr.Current.Value;
                }
            }
        }

        return m_peekKeyAndActivity;
    }

    private bool m_enforceKeepSuccessor;
    private bool m_exclusiveKeepSuccessorActive;

    private long m_time;

    /// <summary>
    /// Only perform KeepSuccessorNoInterrupt processing if the dispatcher has changed since the last time BeginDispatch was called.
    /// It's not necessary to process the code in the block controlled by this variable's conditional unless something has
    /// been added to or removed from the dispatcher.
    /// This value is set to true whenever something has been added to or removed from the dispatcher and
    /// is set to false when KeepSuccessorNoInterrupt processing has occurred.
    /// This decreased the length of an MRP optimize for a recording set by about 45 seconds.
    /// </summary>
    private bool m_dispatcherChangedSinceLastKeepSuccesssorNoInterruptProcessing;

    public override void BeginDispatch(long a_time, bool a_optimize, bool a_enforceKeepSuccessors)
    {
        m_time = a_time;
        m_enforceKeepSuccessor = a_enforceKeepSuccessors;
        m_exclusiveKeepSuccessorActive = false;

        m_treeEtr.Reset();

        if (m_dispatcherDefinition.OnlyDispatchBestComposites)
        {
            if (m_treeEtr.MoveNext())
            {
                m_peekKeyAndActivity = m_treeEtr.Current.Value;
                BalancedCompositeDispatcherDefinition.Key key = (BalancedCompositeDispatcherDefinition.Key)m_peekKeyAndActivity.Key;
                m_bestComposite = key.Composite;
            }
        }
        else if (m_dispatcherChangedSinceLastKeepSuccesssorNoInterruptProcessing)
        {
            if (m_enforceKeepSuccessor)
            {
                if (Resource.CapacityType == SchedulerDefinitions.InternalResourceDefs.capacityTypes.SingleTasking)
                {
                    Resource res = (Resource)Resource;
                    ResourceBlockList.Node block = res.Blocks.Last;
                    if (block != null)
                    {
                        InternalActivity predAct = block.Data.Activity;
                        InternalOperation predOpn = predAct.Operation;
                        if (predOpn.TryToEnforceKeepSuccessor && predOpn.SuccessorProcessing == SchedulerDefinitions.OperationDefs.successorProcessingEnumeration.KeepSuccessorNoInterrupt)
                        {
                            InternalOperation sucOpn = predOpn.Successors[0].Successor.Operation;
                            InternalActivity sucAct = sucOpn.Activities.GetByIndex(0);
                            AVLTree<ConstantCompositeKey, KeyAndActivity>.TreeNode n = m_tree.Find(CreateStaticCompositeValue(sucAct));
                            if (n != null)
                            {
                                m_exclusiveKeepSuccessorActive = true;
                                m_peekKeyAndActivity = n.Value;
                            }
                        }
                    }
                }
            }

            m_dispatcherChangedSinceLastKeepSuccesssorNoInterruptProcessing = false;
        }
    }

    public override DispatcherDefinition DispatcherDefinition => m_dispatcherDefinition;

    public override object Clone()
    {
        return new ConstantCompositeDispatcher(this);
    }

    protected override Enumerator GetInternalActivityTemplateEnumerator()
    {
        return new ConstantCompositeDispatcherEnumerator(this);
    }

    internal override Enumerator GetEnumeratorOfActivitiesOnDispatcher()
    {
        return GetInternalActivityTemplateEnumerator();
    }

    /// <summary>
    /// Call this function if the data changes may have affected the value of the keys. For instance,
    /// </summary>
    //internal void Updatekeys(long a_simClock)
    //{
    //    BeginDispatch(a_simClock, false);
    //    // Removing all the activities and adding them  back should cause the composite values to be recalculated.
    //    InternalActivity act;
    //    List<InternalActivity> acts = new ();
    //    while ((act = GetNext()) != null)
    //    {
    //        act.StaticCompositeSet = false;
    //        acts.Add(act);
    //    }

    //    for (int i = 0; i < acts.Count; ++i)
    //    {
    //        Add(acts[i]);
    //    }
    //}

    internal void Test(long a_simclock) { }


    /// <summary>
    /// Whether this dispatcher contains the activity.
    /// This has better lookup performance than Contains(BaseId) because it uses the static composite value as the key.
    /// </summary>
    /// <returns></returns>
    internal override bool Contains(InternalActivity a_act)
    {
        ConstantCompositeKey staticCompositeKey = CreateStaticCompositeValue(a_act);
        return m_tree.ContainsKey(staticCompositeKey);
    }
}