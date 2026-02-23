using System.Text;

using PT.APSCommon;
using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

// [BATCH_CODE]
public partial class Batch
{
    internal Batch(BaseId a_id, SchedulableInfo a_si, ResourceSatiator[] a_rrSatiators, Resource a_primaryRes)
        : this(a_id, a_rrSatiators, a_primaryRes)
    {
        m_si = a_si;

        ZeroLength = a_si.m_zeroLength;

        m_startTicks = a_si.m_scheduledStartDate;
        m_setupSpan = a_si.m_requiredSetupSpan;
        m_setupEndTicks = a_si.m_setupFinishDate;
        m_processingSpan = a_si.m_requiredProcessingSpan;
        m_processingEndTicks = a_si.m_productionFinishDate;
        m_postProcessingSpan = a_si.m_requiredPostProcessingSpan;
        m_postProcessingEndTicks = a_si.m_postProcessingFinishDate;
        m_cleanSpan = a_si.m_requiredCleanAfterSpan;
        m_cleanEndTicks = a_si.m_cleanFinishDate; //This field may be updated later if sequence dependent clean is added when the subsequent operation schedules                  
        m_storageSpan = a_si.m_requiredStorageSpan;
        m_storageEndTicks = a_si.m_storageFinishDate;
        m_endTicks = a_si.m_finishDate; // This field's vlaue may be updated later depending on whether this is a tank operation (their finish dates aren't known until all the material has been removed).

        m_batchType = a_primaryRes.BatchType;

        switch (m_batchType)
        {
            case MainResourceDefs.batchType.Percent:
                m_percentRemaining = 1;
                break;

            case MainResourceDefs.batchType.Volume:
                m_volumeRemaining = a_primaryRes.BatchVolume;
                break;
        }

        #if DEBUG
        //			m_zdeConstructor = "Simulation";
        #endif
        PrimaryResource = a_primaryRes;
    }

    /// <summary>
    /// Only used when converting a serialized old batch into a new TankBatch
    /// </summary>
    /// <param name="a_sourceBatch"></param>
    protected Batch(Batch a_sourceBatch)
    {
        Id = a_sourceBatch.Id;
        m_referenceInfo = a_sourceBatch.m_referenceInfo;
        m_activities = a_sourceBatch.m_activities;
        m_bools = new BoolVector32(a_sourceBatch.m_bools);
        ZeroLength = a_sourceBatch.ZeroLength;
        StartTicks = a_sourceBatch.StartTicks;
        m_setupSpan = a_sourceBatch.m_setupSpan;
        SetupCapacitySpan = a_sourceBatch.SetupCapacitySpan;
        SetupEndTicks = a_sourceBatch.SetupEndTicks;
        m_processingSpan = a_sourceBatch.m_processingSpan;
        ProcessingCapacitySpan = a_sourceBatch.ProcessingCapacitySpan;
        ProcessingEndTicks = a_sourceBatch.ProcessingEndTicks;
        PostProcessingSpan = a_sourceBatch.PostProcessingSpan;
        PostProcessingEndTicks = a_sourceBatch.PostProcessingEndTicks;
        CleanSpan = a_sourceBatch.CleanSpan;
        CleanEndTicks = a_sourceBatch.CleanEndTicks;
        StorageSpan = a_sourceBatch.StorageSpan;
        m_storageEndTicks = a_sourceBatch.EndOfStorageTicks;
        EndTicks = a_sourceBatch.EndTicks;
        m_volumeRemaining = a_sourceBatch.m_volumeRemaining;
        m_percentRemaining = a_sourceBatch.m_percentRemaining;
        m_batchType = a_sourceBatch.m_batchType;
        m_si = a_sourceBatch.m_si;
        m_rrSatiators = a_sourceBatch.m_rrSatiators;
        PrimaryResource = a_sourceBatch.PrimaryResource;
        m_rrBlockNodes = a_sourceBatch.m_rrBlockNodes;
        m_scheduledCount = a_sourceBatch.m_scheduledCount;
        #if DEBUG
        m_zdeId = a_sourceBatch.m_zdeId;
        m_zdeUnscheduledActivities = a_sourceBatch.m_zdeUnscheduledActivities;
        #endif
    }

    /// <summary>
    /// Used when you are basing a batch on an existing batch, such as when splitting a block.
    /// </summary>
    /// <param name="a_id"></param>
    /// <param name="a_si"></param>
    /// <param name="a_sourceBatch">Used to help initialize a copy of this batch.</param>
    /// <param name="a_primaryRes"></param>
    internal Batch(BaseId a_id, SchedulableInfo a_si, Batch a_sourceBatch, Resource a_primaryRes)
        : this(a_id, a_si, a_sourceBatch.m_rrSatiators, a_primaryRes) { }

    /// <summary>
    /// This was written only for versions of scenarios that precede the batch enhancement.
    /// </summary>
    /// <param name="a_id">The is to assign to this batch.</param>
    /// <param name="a_rrSatiators">
    /// If called during an optimize, this resource array must have been prepopulated with the resources that will satisfy each resource requirement.
    /// If called during the load of a scenario whose version preceeded the batch enhancement(each activity will have a single batch created for it), the size must have been initialized to the number of
    /// resource requirements, but the values in the array can all be null; they'll be set as loading and restore references progresses.
    /// </param>
    internal Batch(BaseId a_id, ResourceSatiator[] a_rrSatiators, Resource a_primaryRes) :
        base(a_id)
    {
        #if DEBUG
        AssignId();
        #endif
        PrimaryResource = a_primaryRes;
        m_rrSatiators = a_rrSatiators;
        m_rrBlockNodes = new ResourceBlockList.Node[a_rrSatiators.Length];
        m_setupSpan = RequiredSpanPlusSetup.s_notInit;
        m_processingSpan = RequiredSpan.NotInit;
        m_postProcessingSpan = RequiredSpan.NotInit;
    }

    /// <summary>
    /// Simulation only.
    /// </summary>
    internal SchedulableInfo m_si; // [BATCH]

    /// <summary>
    /// Simulation only. There's a corresponding object in this array for each resource requirement.
    /// </summary>
    internal ResourceSatiator[] m_rrSatiators; // [BATCH]

    /// <summary>
    /// </summary>
    /// <param name="a_rrIdx">The index of the RR satiator of the operation.</param>
    /// <param name="a_res"></param>
    internal void SetRRSatiator(int a_rrIdx, Resource a_res)
    {
        m_rrSatiators[a_rrIdx] = new ResourceSatiator(a_res);
    }

    /// <summary>
    /// Returns a copy of the Resource Requirement satiators.
    /// </summary>
    /// <returns></returns>
    internal ResourceSatiator[] GetRRSatiatorsCopy()
    {
        ResourceSatiator[] copy = new ResourceSatiator[m_rrSatiators.Length];
        Array.Copy(m_rrSatiators, copy, m_rrSatiators.Length);
        return copy;
    }

    /// <summary>
    /// Get a Resource Requirement satiator.
    /// </summary>
    /// <param name="a_rrIdx">Get the resource requirement satiator for this resource requirement index</param>
    /// <returns></returns>
    internal ResourceSatiator GetRRSatiator(int a_rrIdx)
    {
        return m_rrSatiators[a_rrIdx];
    }

    public Resource PrimaryResource { get; private set; }

    /// <summary>
    /// There's a corresponding object in this array for each resource requirement.
    /// </summary>
    internal ResourceBlockList.Node[] m_rrBlockNodes; // [BATCH]

    internal void SetBlockNode(long a_rrIdx, ResourceBlockList.Node a_blockNode)
    {
        m_rrBlockNodes[a_rrIdx] = a_blockNode;

        InternalActivity act = GetFirstActivity();
        if (a_rrIdx == act.Operation.ResourceRequirements.PrimaryResourceRequirementIndex)
        {
            PrimaryResource = a_blockNode.Data.ScheduledResource;
        }
    }

    /// <summary>
    /// Add an activity to this batch
    /// </summary>
    /// <param name="a_act"></param>
    /// <param name="a_batchAmount"></param>
    /// <param name="a_associateActWIthBatch">Whether to associate the activity with this batch by calling InternalActivity.Batch_SetBatch.</param>
    internal void Add(InternalActivity a_act, decimal a_batchAmount, bool a_associateActWIthBatch = true)
    {
        #if DEBUG
        EmptyTest();

        bool initialized = m_activities.Count > 0;
        #endif

        switch (m_batchType)
        {
            case MainResourceDefs.batchType.Percent:
            {
                decimal percent = a_batchAmount / a_act.GetResourceProductionInfo(PrimaryResource).QtyPerCycle;


                #if DEBUG
                if (initialized && m_percentRemaining < percent)
                {
                    throw new Exception("There isn't enough percent of the resource available to join the batch.");
                }
                #endif

                m_percentRemaining -= percent;
            }

                break;

            case MainResourceDefs.batchType.Volume:

                #if DEBUG
                if (initialized && m_volumeRemaining < a_batchAmount)
                {
                    throw new Exception("There isn't enough volume left of the resource to join the batch.");
                }
                #endif

                m_volumeRemaining -= a_batchAmount;

                #if DEBUG
                if (m_volumeRemaining < 0)
                {
                    throw new Exception("Batch.m_volumeRemaining < 0");
                }
                #endif

                break;
        }

        LinkedListNode<InternalActivity> node = m_activities.AddLast(a_act);
        if (a_associateActWIthBatch)
        {
            a_act.Batch_SetBatch(this, node);
        }
        // else some other code must associate the activity with the batch.
    }

    /// <summary>
    /// Whether all the activities have been removed from the batch.
    /// </summary>
    /// <returns></returns>
    internal bool Empty()
    {
        return m_scheduledCount == 0;
    }

    /// <summary>
    /// Unschedule an activity that's part of this batch.
    /// </summary>
    /// <param name="a_node">Where the activity is located in the batches list of activities.</param>
    /// <param name="a_remove">
    /// Whether the activity should be removed from the batches list of activities. During simulations, this list is used to try to hold activities that are part of the same batch together.
    /// For instance, in a move, the activities that are being moved would have this function called for them with this parameter set to true. The other activities would pass in false.
    /// </param>
    /// <returns>Returns the blocks the batch was scheduled on.</returns>
    internal ResourceBlock[] UnscheduleAct(LinkedListNode<InternalActivity> a_node, bool a_remove)
    {
        ResourceBlock[] resBlocks = null;

        if (m_scheduledCount == int.MaxValue)
        {
            m_scheduledCount = m_activities.Count;
        }

        --m_scheduledCount;

        if (m_scheduledCount == 0)
        {
            resBlocks = GetRRBlocks();
        }

        #if DEBUG

        if (m_scheduledCount < 0)
        {
            throw new Exception("Too many removed from batch.");
        }

        m_zdeUnscheduledActivities = new SortedDictionary<BaseId, InternalActivity>();

        if (m_zdeUnscheduledActivities.ContainsKey(a_node.Value.Id))
        {
            throw new Exception("The activity has already been removed.");
        }

        m_zdeUnscheduledActivities.Add(a_node.Value.Id, a_node.Value);

        #endif
        #if DEBUG

        m_zdeUnscheduledActivities = new SortedDictionary<BaseId, InternalActivity>();

        if (m_zdeUnscheduledActivities.ContainsKey(a_node.Value.Id))
        {
            throw new Exception("The activity has already been removed.");
        }

        m_zdeUnscheduledActivities.Add(a_node.Value.Id, a_node.Value);

        #endif

        if (a_remove)
        {
            m_activities.Remove(a_node);
        }

        return resBlocks;
    }

    /// <summary>
    /// The value of this variable is only initialized to the correct value when the first attempt to
    /// unschedule an activitity is made.
    /// If you look at the unschedule function. You'll see that it's possible for activities to still be
    /// associated with a batch object even after they've been unscheduled.
    /// </summary>
    private int m_scheduledCount = int.MaxValue;

    internal bool Contains(long date)
    {
        return StartTicks <= date && EndTicks > date;
    }

    internal bool Contains(InternalActivity a_act)
    {
        IEnumerator<InternalActivity> etr = m_activities.GetEnumerator();
        while (etr.MoveNext())
        {
            if (etr.Current == a_act)
            {
                return true;
            }
        }

        return false;
    }

    #if DEBUG
    /// <summary>
    /// No more additions can be made to the batch and the quantity isn't correct.
    /// The batch must be rescheduled.
    /// </summary>
    internal long m_zdeId;

    private static long s_zdeLastId;

//		string m_zdeConstructor;
    private SortedDictionary<BaseId, InternalActivity> m_zdeUnscheduledActivities;

    /// <summary>
    /// Assign an unique id to the batch for debugging purposes.
    /// </summary>
    private void AssignId()
    {
        m_zdeId = ++s_zdeLastId;
    }
    #endif

    #if DEBUG
    internal void EmptyTest()
    {
        if (Empty())
        {
            throw new Exception("The batch is empty.");
        }
    }
    #endif

    /// <summary>
    /// Get the earliest frozen span among all the resources scheduled for the Batch.
    /// </summary>
    /// <returns></returns>
    internal long GetEarliestFrozenSpan()
    {
        long endOfFrozenZone = 0;

        for (int blockNodeI = 0; blockNodeI < m_rrBlockNodes.Length; ++blockNodeI)
        {
            if (m_rrBlockNodes[blockNodeI] != null && m_rrBlockNodes[blockNodeI].Data != null)
            {
                long resEndOfFrozenZone = m_rrBlockNodes[blockNodeI].Data.ScheduledResource.Department.FrozenSpanTicks;
                endOfFrozenZone = Math.Max(endOfFrozenZone, resEndOfFrozenZone);
            }
        }

        return endOfFrozenZone;
    }

    /// <summary>
    /// Get the earliest stable span among all the resources scheduled for the Batch.
    /// </summary>
    /// <returns></returns>
    internal long GetEarliestStableSpan()
    {
        long endOfFrozenZone = 0;

        for (int blockNodeI = 0; blockNodeI < m_rrBlockNodes.Length; ++blockNodeI)
        {
            if (m_rrBlockNodes[blockNodeI] != null && m_rrBlockNodes[blockNodeI].Data != null)
            {
                Department scheduledResourceDepartment = m_rrBlockNodes[blockNodeI].Data.ScheduledResource.Department;
                long resEndOfFrozenZone = scheduledResourceDepartment.FrozenSpanTicks + scheduledResourceDepartment.Plant.StableSpanTicks;
                endOfFrozenZone = Math.Max(endOfFrozenZone, resEndOfFrozenZone);
            }
        }

        return endOfFrozenZone;
    }

    /// <summary>
    /// Depending on how the parameter is setup, whether any blocks are in the resource's frozen or stable span.
    /// </summary>
    /// <param name="a_spanCalc">Must be configured for either OptimizeSettings.startTimes.EndOfFrozenZone or OptimizeSettings.startTimes.EndOfStableZone</param>
    /// <returns></returns>
    internal bool AnyBlocksInSpan(long a_clockDate, OptimizeSettings.ETimePoints a_spanType)
    {
        for (int blockI = 0; blockI < m_rrBlockNodes.Length; ++blockI)
        {
            if (m_rrBlockNodes[blockI] != null)
            {
                ResourceBlock block = m_rrBlockNodes[blockI].Data;
                Department scheduledDepartment = block.ScheduledResource.Department;
                long endOfSpan;

                switch (a_spanType)
                {
                    case OptimizeSettings.ETimePoints.EndOfFrozenZone:
                        endOfSpan = a_clockDate + scheduledDepartment.FrozenSpanTicks;
                        break;
                    case OptimizeSettings.ETimePoints.EndOfStableZone:
                        endOfSpan = a_clockDate + scheduledDepartment.FrozenSpanTicks + scheduledDepartment.Plant.StableSpanTicks;
                        break;
                    case OptimizeSettings.ETimePoints.EndOfPlanningHorizon:
                    case OptimizeSettings.ETimePoints.EntireSchedule:
                        endOfSpan = PTDateTime.MaxDateTicks;
                        break;

                    case OptimizeSettings.ETimePoints.CurrentPTClock:
                    case OptimizeSettings.ETimePoints.SpecificDateTime:
                    case OptimizeSettings.ETimePoints.EndOfShortTerm:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(a_spanType), a_spanType, null);
                }

                if (block.StartTicks < endOfSpan)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// For debugging purposes. Shows the id and number of activities scheduled in the batch.
    /// </summary>
    /// <returns></returns>
    #if DEBUG
    public override string ToString()
    {
        StringBuilder sb = new ();
        sb.AppendFormat("Id={0}; NbrOfActivities={1}; Start={2}; EndOfSetup={3}; EndProcessing={4}, EndPostProcessing={5}; EndStorage={6}; End={7}", m_zdeId, m_activities.Count, DateTimeHelper.ToLocalTimeFromUTCTicks(m_startTicks), DateTimeHelper.ToLocalTimeFromUTCTicks(m_setupEndTicks), DateTimeHelper.ToLocalTimeFromUTCTicks(m_processingEndTicks), DateTimeHelper.ToLocalTimeFromUTCTicks(m_postProcessingEndTicks), DateTimeHelper.ToLocalTimeFromUTCTicks(m_endTicks));
        return sb.ToString();
    }
    #endif

    internal void SetSimultaneousSequenceIdx(int a_scheduledIdx)
    {
        foreach (InternalActivity activity in m_activities)
        {
            activity.SimultaneousSequenceIdx = a_scheduledIdx;
        }
    }

    /// <summary>
    /// Returns the block node in this batch that is scheduled on provided resource
    /// </summary>
    /// <returns>Whether the node was found and scheduled in the batch</returns>
    public bool TryGetNodeByResourceId(BaseId a_resourceId, out ResourceBlockList.Node a_node)
    {
        foreach (ResourceBlockList.Node node in m_rrBlockNodes)
        {
            //node can be null if the helper resource requirement was not needed during scheduling
            if (node == null)
            {
                continue;
            }

            if (node.Data.ScheduledResource?.Id == a_resourceId)
            {
                a_node = node;
                return true;
            }
        }

        a_node = null;
        return false;
    }

    /// <summary>
    /// Extends the Clean duration and block end date by any new length due to the additional clean span
    /// </summary>
    /// <param name="a_newCleanRequirement">The clean span including cleanout grade</param>
    /// <param name="a_capacityProfile">The capacity profile for this clean. This should have been calculated from capacity profiles</param>
    internal virtual void MergeCleanout(RequiredSpanPlusClean a_newCleanRequirement, CapacityUsageProfile a_capacityProfile)
    {
        //Extend the ending of the batch to account for CIP as post processing
        RequiredSpanPlusClean newSpan = m_cleanSpan.Merge(a_newCleanRequirement, true);
        if (newSpan != m_cleanSpan)
        {
            //Change the Clean
            m_cleanEndTicks = a_capacityProfile.CapacityEndTicks;
            m_endTicks = m_cleanEndTicks;
            m_cleanSpan = newSpan;

            foreach (ResourceBlockList.Node node in m_rrBlockNodes)
            {
                //node can be null if the helper resource requirement was not needed during scheduling
                if (node == null)
                {
                    continue;
                }

                if (node.Data.SatisfiedRequirement.ContainsUsage(MainResourceDefs.usageEnum.Clean))
                {
                    node.Data.EndTicks = m_endTicks;
                    node.Data.CapacityProfile.Add(a_capacityProfile, MainResourceDefs.usageEnum.Clean);
                }
            }
        }
    }
}