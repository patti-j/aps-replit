using PT.APSCommon;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for IReadyActivitiesDispatcher.
/// </summary>
public abstract partial class ReadyActivitiesDispatcher : ICloneable, IEnumerable<InternalActivity>
{
    public virtual void Clear()
    {
    }

    public abstract void Add(InternalActivity a_activity, long a_sequenceHeadStartEndDate);

    public abstract bool HasReadyActivity { get; }

    /// <summary>
    /// The number of activities in the dispatcher.
    /// </summary>
    internal abstract int Count { get; }

    public abstract bool Remove(InternalActivity a_activity);

    /// <summary>
    /// Get the next activitiy off the dispatcher.
    /// </summary>
    /// <returns></returns>
    public abstract InternalActivity GetNext();

    public abstract KeyAndActivity GetNextKeyAndActivity();

    /// <summary>
    /// Peek at the next activity on the dispatcher.
    /// </summary>
    /// <returns></returns>
    public abstract InternalActivity PeekNext();

    public abstract KeyAndActivity PeekNextKeyAndActivity();

    public abstract void BeginDispatch(long a_time, bool a_optimize, bool a_enforceKeepSuccessors);

    public abstract DispatcherDefinition DispatcherDefinition { get; }

    public virtual void EndDispatch() { }

    private InternalResource m_resource;

    internal InternalResource Resource
    {
        get
        {
            #if DEBUG
            if (m_resource == null)
            {
                throw new PTException("This ReadyActivitiesDispatcher is not associated with a resource.");
            }
            #endif
            return m_resource;
        }

        set { m_resource = value; }
    }

    #region ICloneable Members
    public abstract object Clone();
    #endregion

    #region Statics
    protected enum KeepSuccessorResult { Normal, Skip, Exclusive }

    protected static KeepSuccessorResult EnforceKeepSuccessor(InternalResource a_res, long a_time, InternalActivity a_sucAct)
    {
        //If Scheduling a split
        if (a_sucAct.Operation.AutoSplitInfo.AutoSplitType != OperationDefs.EAutoSplitType.None 
            && a_sucAct.Operation.Split && !a_sucAct.Operation.Scheduled)
        {
            InternalActivity lastSchedAct = a_sucAct.Operation.GetLatestEndingScheduledActivity();
            if (a_sucAct.Operation.AutoSplitInfo.KeepSplitsOnSameResource)
            {
                if (lastSchedAct.Batch.PrimaryResource.Id == a_res.Id)
                {
                    return KeepSuccessorResult.Exclusive;
                }

                return KeepSuccessorResult.Skip;
            }
        }

        if (a_sucAct.Operation.TryToEnforcePredecessorsKeepSuccessor)
        {
            AlternatePath.AssociationCollection predecessors = a_sucAct.Operation.AlternatePathNode.Predecessors;
            InternalOperation predOpn = predecessors[0].Predecessor.Operation;
            InternalActivity predAct = predOpn.Activities.GetByIndex(0);

            if (predAct.PrimaryResourceRequirementBlock != null)
            {
                long maxWaitDate = predAct.ScheduledEndDate.Ticks;
                if (predOpn.SuccessorProcessing != OperationDefs.successorProcessingEnumeration.NoPreference)
                {
                    maxWaitDate += predOpn.KeepSuccessorsTimeLimit.Ticks;
                }
                else if (predecessors[0].OverlapType != InternalOperationDefs.overlapTypes.NoOverlap)
                {
                    //TODO: What if there are multiple precedessors?
                    return KeepSuccessorResult.Normal;
                }

                if (a_time <= maxWaitDate)
                {
                    Resource predRes = predAct.PrimaryResourceRequirementBlock.ScheduledResource;

                    if (predRes.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
                    {
                        // Determine whether the operation can be performed on this machine.
                        if (a_sucAct.ResReqsEligibilityNarrowedDuringSimulation[0].Contains(predRes.PlantId)) // The successor must have resource eligible in the predecessor's plant in order to aply KeepSuccessor.
                        {
                            EligibleResourceSet eligibleResourceSet = a_sucAct.ResReqsEligibilityNarrowedDuringSimulation[0][predRes.PlantId];

                            if (eligibleResourceSet != null)
                            {
                                if (eligibleResourceSet.Contains(predRes))
                                {
                                    if (predRes != a_res)
                                    {
                                        // Don't take this operation into consideration because it's stuck to another resource for the time being.
                                        return KeepSuccessorResult.Skip;
                                    }

                                    if (predOpn.SuccessorProcessing == OperationDefs.successorProcessingEnumeration.KeepSuccessorNoInterrupt)
                                    {
                                        return KeepSuccessorResult.Exclusive;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return KeepSuccessorResult.Normal;
    }
    #endregion

    #region IEnumerable<InternalActivity>
    /// <summary>
    /// Scan the dispathcer in the order is which activities will be considered for scheduling.
    /// The enumerator will: throw an exception if current is called after MoveNext() returns false, isn't thread safe, and becomes invalid if the collection changes.
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
        return GetInternalActivityTemplateEnumerator();
    }

    /// <summary>
    /// The interface method this function is required for could not be made abstract due to the C# language.
    /// This must be overridden to provide the template functionality of: public IEnumerator<InternalActivity> GetEnumerator()
    /// </summary>
    /// <returns></returns>
    protected abstract Enumerator GetInternalActivityTemplateEnumerator();

    IEnumerator<InternalActivity> IEnumerable<InternalActivity>.GetEnumerator()
    {
        return GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    #region
    /// <summary>
    /// Scan the dispatcher of all activities that have currently
    /// </summary>
    /// <returns></returns>
    internal abstract Enumerator GetEnumeratorOfActivitiesOnDispatcher();
    #endregion

    //internal abstract bool Contains(BaseId a_actId); Other override has better performance
    internal abstract bool Contains(InternalActivity a_act);
}