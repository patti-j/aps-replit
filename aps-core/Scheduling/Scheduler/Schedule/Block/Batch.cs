using System.Diagnostics;

using PT.APSCommon;
using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

// [BATCH_CODE]
public partial class Batch : BaseIdObject, IEnumerable<InternalActivity>
{
    #region Serialization
    internal Batch(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12521)
        {
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_volumeRemaining);

            a_reader.Read(out m_startTicks);
            m_setupSpan = new RequiredSpanPlusSetup(a_reader);
            a_reader.Read(out m_setupEndTicks);
            m_processingSpan = new RequiredSpan(a_reader);
            a_reader.Read(out m_processingEndTicks);
            m_postProcessingSpan = new RequiredSpan(a_reader);
            a_reader.Read(out m_postProcessingEndTicks);
            m_storageSpan = new RequiredSpan(a_reader);
            a_reader.Read(out m_storageEndTicks);
            m_cleanSpan = new RequiredSpanPlusClean(a_reader);
            a_reader.Read(out m_cleanEndTicks);
            a_reader.Read(out m_endTicks);

            m_referenceInfo = new ReferenceInfo();
            a_reader.Read(out int count);
            for (int i = 0; i < count; ++i)
            {
                ActivityKey ak = new (a_reader);
                m_referenceInfo.m_actIdList.Add(ak);
            }
        }
        else if (a_reader.VersionNumber >= 12302)
        {
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_volumeRemaining);

            a_reader.Read(out m_startTicks);
            m_setupSpan = new RequiredSpanPlusSetup(a_reader);
            a_reader.Read(out m_setupEndTicks);
            m_processingSpan = new RequiredSpan(a_reader);
            a_reader.Read(out m_processingEndTicks);
            m_postProcessingSpan = new RequiredSpan(a_reader);
            a_reader.Read(out m_postProcessingEndTicks);
            a_reader.Read(out m_endTicks);

            m_referenceInfo = new ReferenceInfo();
            a_reader.Read(out int count);
            for (int i = 0; i < count; ++i)
            {
                ActivityKey ak = new (a_reader);
                m_referenceInfo.m_actIdList.Add(ak);
            }

            m_cleanSpan = new RequiredSpanPlusClean(a_reader);
            a_reader.Read(out m_cleanEndTicks);
        }
        else if (a_reader.VersionNumber >= 12008)
        {
            a_reader.Read(out m_volumeRemaining);

            a_reader.Read(out m_startTicks);
            m_setupSpan = new RequiredSpanPlusSetup(a_reader);
            a_reader.Read(out m_setupEndTicks);
            m_processingSpan = new RequiredSpan(a_reader);
            a_reader.Read(out m_processingEndTicks);
            m_postProcessingSpan = new RequiredSpan(a_reader);
            a_reader.Read(out m_postProcessingEndTicks);
            a_reader.Read(out m_endTicks);

            m_bools = new BoolVector32(a_reader);

            m_referenceInfo = new ReferenceInfo();
            a_reader.Read(out int count);
            for (int i = 0; i < count; ++i)
            {
                ActivityKey ak = new (a_reader);
                m_referenceInfo.m_actIdList.Add(ak);
            }
        }
        else
        {
            if (a_reader.VersionNumber >= 676) // This scenario was written on or after the first version of this change in the development branch.
            {
                a_reader.Read(out m_volumeRemaining);

                a_reader.Read(out m_startTicks);
                m_setupSpan = new RequiredSpanPlusSetup(a_reader);
                a_reader.Read(out m_setupEndTicks);
                m_processingSpan = new RequiredSpan(a_reader);
                a_reader.Read(out m_processingEndTicks);
                m_postProcessingSpan = new RequiredSpan(a_reader);
                a_reader.Read(out m_postProcessingEndTicks);
                a_reader.Read(out long m_endOfStorageTicks);
                a_reader.Read(out m_endTicks);
                //if (m_endTicks > m_postProcessingEndTicks)
                //{
                //    //There is a storage component
                //    o_upgradeToTankBatchTiming = m_endOfStorageTicks;
                //}

                m_bools = new BoolVector32(a_reader);

                m_referenceInfo = new ReferenceInfo();
                int count;
                a_reader.Read(out count);
                for (int i = 0; i < count; ++i)
                {
                    ActivityKey ak = new (a_reader);
                    m_referenceInfo.m_actIdList.Add(ak);
                }
            }
        }

        // Create a temporary schedulableInfo from the values stored in the batch.
        // This is necessary so GetUsageStart and GetUsageEnd can be called
        // when the scenario is deserializing.
        m_si = new SchedulableInfo(m_startTicks == m_endTicks, m_startTicks, m_setupEndTicks, m_processingEndTicks, m_postProcessingEndTicks, m_storageEndTicks, m_cleanEndTicks, m_endTicks, null, RequiredSpanPlusClean.s_notInit, m_setupSpan, m_processingSpan, m_postProcessingSpan, m_storageSpan, m_cleanSpan, -1, -1, null);

        #if DEBUG
        //            m_zdeConstructor = "Reader";
        AssignId();
        #endif
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        #if DEBUG
        EmptyTest();
        #endif
        m_bools.Serialize(a_writer);

        a_writer.Write(m_volumeRemaining);
        a_writer.Write(m_startTicks);
        m_setupSpan.Serialize(a_writer);
        a_writer.Write(m_setupEndTicks);
        m_processingSpan.Serialize(a_writer);
        a_writer.Write(m_processingEndTicks);
        m_postProcessingSpan.Serialize(a_writer);
        a_writer.Write(m_postProcessingEndTicks);
        m_storageSpan.Serialize(a_writer);
        a_writer.Write(m_storageEndTicks);
        m_cleanSpan.Serialize(a_writer);
        a_writer.Write(m_cleanEndTicks);
        a_writer.Write(m_endTicks);

        #if DEBUG
        if (m_activities.Count == 0)
        {
            throw new Exception("An empty batch is being serialized");
        }
        #endif
        a_writer.Write(m_activities.Count);
        IEnumerator<InternalActivity> actEtr = GetEnumerator();
        while (actEtr.MoveNext())
        {
            InternalActivity act = actEtr.Current;
            ActivityKey ak = new (act.Job.Id, act.ManufacturingOrder.Id, act.Operation.Id, act.Id);
            ak.Serialize(a_writer);
        }
    }

    public new const int UNIQUE_ID = 608;

    public override int UniqueId => UNIQUE_ID;

    private ReferenceInfo m_referenceInfo;

    private class ReferenceInfo
    {
        internal readonly List<ActivityKey> m_actIdList = new ();
    }

    /// <summary>
    /// Restores references batch to the activities in the batch.
    /// </summary>
    /// <param name="a_jobs"></param>
    internal void RestoreReferences(JobManager a_jobs)
    {
        int nbrResReqs = int.MaxValue;
        for (int i = 0; i < m_referenceInfo.m_actIdList.Count; ++i)
        {
            ActivityKey ak = m_referenceInfo.m_actIdList[i];
            InternalActivity act = a_jobs.FindActivity(ak);

            nbrResReqs = Math.Min(nbrResReqs, act.Operation.ResourceRequirements.Count);
            Add(act, act.BatchAmount, false);
        }

        m_rrBlockNodes = new ResourceBlockList.Node[nbrResReqs];
        m_rrSatiators = new ResourceSatiator[nbrResReqs];

        m_referenceInfo = null;
    }

    /// <summary>
    /// Restore remaining references that couldn't be restored during RestoreReferences().
    /// </summary>
    /// <param name="a_rrIdx"></param>
    /// <param name="a_blockNode"></param>
    /// <param name="a_res"></param>
    internal void RestoreReferences2(long a_rrIdx, ResourceBlockList.Node a_blockNode, Resource a_res)
    {
        m_rrBlockNodes[a_rrIdx] = a_blockNode;
        m_rrSatiators[a_rrIdx] = new ResourceSatiator(a_res);

        InternalActivity act = GetFirstActivity();
        // This part of the initialization can only be done after the primary resource's block's references
        // has had it's references restored and the Batch.PrimarResource property has been initialized.
        if (a_rrIdx == act.Operation.ResourceRequirements.PrimaryResourceRequirementIndex)
        {
            PrimaryResource = a_blockNode.Data.ScheduledResource;

            // relink the activities to the fully restored batch.
            LinkedListNode<InternalActivity> node = m_activities.First;
            while (node != null)
            {
                InternalActivity a = node.Value;
                a.Batch_SetBatch(this, node);
                node = node.Next;
            }
        }
    }
    #endregion

    /// <summary>
    /// There's a corresponding object in this array for each resource requirement.
    /// </summary>
    // Selected linked list to make it easier to remove objects.
    private readonly LinkedList<InternalActivity> m_activities = new ();

    /// <summary>
    /// Get the first activity that's in the batch.
    /// </summary>
    public InternalActivity FirstActivity => m_activities.First.Value;

    public int ActivitiesCount => m_activities.Count;

    /// <summary>
    /// Iterate the Activities in the Batch and returns the Activity if found.  Returns null if not found.
    /// </summary>
    /// <param name="a_activityKey"></param>
    /// <returns></returns>
    public InternalActivity FindActivity(BaseId a_activityId)
    {
        IEnumerator<InternalActivity> iterator = GetEnumerator();
        while (iterator.MoveNext())
        {
            InternalActivity activity = iterator.Current;
            if (activity.Id == a_activityId)
            {
                return activity;
            }
        }

        return null;
    }

    /// <summary>
    /// There's one for each resource requirement.
    /// </summary>
    public int BlockCount => m_rrBlockNodes.Length;

    /// <summary>
    /// There's one for each resource requirement. Could be null.
    /// </summary>
    public ResourceBlock BlockAtIndex(int a_idx)
    {
        if (m_rrBlockNodes[a_idx] != null)
        {
            return m_rrBlockNodes[a_idx].Data;
        }

        return null;
    }

    public ResourceBlock PrimaryResourceBlock => BlockAtIndex(0);

    /// <summary>
    /// Note, this value can be null if the RR doesn't need to be scheduled.
    /// For instance, if the RR is only neededfor setup, but no setup is necessary,
    /// then the RR won't be scheduled.
    /// Get the block node scheduled for this batch for a resource requirement.
    /// </summary>
    /// <param name="a_rrIdx">The index of the resource requirement.</param>
    /// <returns>Can be null if the RR isn't necessary.</returns>
    internal ResourceBlockList.Node GetBlockNodeAtIndex(int a_rrIdx)
    {
        return m_rrBlockNodes[a_rrIdx];
    }

    #region Serializable Bools
    private BoolVector32 m_bools;

    private const int c_zeroLength = 0;

    [Obsolete("The individual timespans now have zero-length built into them. This field will be deleted soon.")]
    public bool ZeroLength
    {
        get => m_bools[c_zeroLength];
        private set => m_bools[c_zeroLength] = value;
    }
    #endregion

    private long m_startTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// The time the batch is scheduled to start.
    /// </summary>
    public long StartTicks
    {
        get => m_startTicks;
        set => m_startTicks = value;
    }

    private RequiredSpanPlusSetup m_setupSpan;

    /// <summary>
    /// The total capacity scheduled for setup. Note this is different than the wall clock time.
    /// The wall clock time may be more or less than this value.
    /// For instance if the resource had a set of efficiency factor of 0.5, The wall clock time
    /// would be half the length of the capacity span.
    /// </summary>
    public RequiredSpanPlusSetup SetupCapacitySpan
    {
        get => m_setupSpan;
        private set => m_setupSpan = value;
    }

    private long m_setupEndTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// When setup ends and Processing starts.
    /// </summary>
    public long SetupEndTicks
    {
        get => m_setupEndTicks;
        private set => m_setupEndTicks = value;
    }

    private RequiredSpan m_processingSpan;

    /// <summary>
    /// The time it takes to process the batch.
    /// </summary>
    public RequiredSpan ProcessingCapacitySpan
    {
        get => m_processingSpan;
        private set => m_processingSpan = value;
    }

    private long m_processingEndTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// The time processing the batch ends.
    /// </summary>
    public long ProcessingEndTicks
    {
        get => m_processingEndTicks;
        private set => m_processingEndTicks = value;
    }

    private RequiredSpan m_postProcessingSpan = RequiredSpan.NotInit;

    /// <summary>
    /// The length of Post Processing (additional time the resources are in use after processing ends).
    /// </summary>
    public RequiredSpan PostProcessingSpan
    {
        get => m_postProcessingSpan;
        internal set => m_postProcessingSpan = value;
    }

    private long m_postProcessingEndTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// The time post-processing is scheduled to end (and storage can begin).
    /// </summary>
    public long PostProcessingEndTicks
    {
        get => m_postProcessingEndTicks;
        protected set => m_postProcessingEndTicks = value;
    }

    private long m_storageEndTicks;

    /// <summary>
    /// Only set this field to indicate when the EndOfStorage has been determined. An additional Flag is set when this value is specified to indicate
    /// that the EndOfStorage has been set.
    /// </summary>
    public long EndOfStorageTicks
    {
        get { return m_storageEndTicks; }
        internal set
        {
            m_storageEndTicks = value;
        }
    }

    private RequiredSpan m_storageSpan = RequiredSpan.NotInit;

    /// <summary>
    /// The length of Scheduled storage.
    /// </summary>
    public RequiredSpan StorageSpan
    {
        get => m_storageSpan;
        internal set => m_storageSpan = value;
    }

    /// <summary>
    /// When all the material has been removed from the tank.
    /// </summary>
    public DateTime EndOfStorageDateTime => new (EndOfStorageTicks);

    private RequiredSpanPlusClean m_cleanSpan = RequiredSpanPlusClean.s_notInit;

    /// <summary>
    /// The length of Clean time.
    /// </summary>
    public RequiredSpanPlusClean CleanSpan
    {
        get => m_cleanSpan;
        internal set => m_cleanSpan = value;
    }

    private long m_cleanEndTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// The time post-processing is scheduled to end (and storage can begin).
    /// </summary>
    public long CleanEndTicks
    {
        get => m_cleanEndTicks;
        internal set => m_cleanEndTicks = value;
    }

    /// <summary>
    /// The total amount of time this batch uses. The setup and run portion of this span only includes online time. Other included spans are merely the difference between the end and start of the segment.
    /// </summary>
    public virtual long TotalSpanTicks => EndTicks - StartTicks;

    protected long m_endTicks;

    /// <summary>
    /// Either the end of post-processing or if material is stored in a tank resources, the time the tank is emptied or the end of tank post-processing.
    /// </summary>
    public long EndTicks
    {
        get => m_endTicks;
        // Tank finish dates aren't set until after all the material has been removed from the tank. In the case of tanks, this value is set
        // after all the material has been removed.
        internal set => m_endTicks = value;
    }

    private decimal m_volumeRemaining; // [BATCH]

    /// <summary>
    /// This is equal to the volume of the primary minus the volume used by activities assigned to it.
    /// </summary>
    internal decimal BatchVolumeRemaining
    {
        get
        {
            #if DEBUG
            EmptyTest();
            #endif
            return m_volumeRemaining;
        }
    }

    public decimal VolumeRemaining => m_volumeRemaining;

    private decimal m_percentRemaining;

    /// <summary>
    /// Max value is 1.0. Percent of space left in the batch. The percent an activity consumes of a batch equals RequiredFinishQuantity/Activity.QtyPerCycle.
    /// An activity where RequiredFinishQty>Activity.QtyPerCycle isn't eligible for scheduling on a MainResourceDefs.batchType.Percent because its percent would be greater than 100.
    /// Batch by percent is for similar activities and everything in the batch completes in 1 cycle.
    /// </summary>
    public decimal PercentRemaining => m_percentRemaining;

    internal decimal BatchPercentRemaining
    {
        get
        {
            #if DEBUG
            EmptyTest();
            #endif
            return m_percentRemaining;
        }
    }

    #region public DateTime read-only Accessors
    /// <summary>
    /// When the resource is scheduled to start working (either setup or run).
    /// </summary>
    public DateTime StartDateTime => new (m_startTicks);

    public DateTime SetupFinishedDateTime => new (m_setupEndTicks);

    public DateTime ProcessingEndDateTime => new (m_processingEndTicks);

    public TimeSpan PostProcessingTimeSpan => new (m_postProcessingSpan.TimeSpanTicks);

    public DateTime PostProcessingEndDateTime => new (m_postProcessingEndTicks);

    public TimeSpan CleanTimeSpan => new (m_cleanSpan.TimeSpanTicks);

    public DateTime CleanEndDateTime => new (m_cleanEndTicks);

    /// <summary>
    /// When the resource is scheduled to finish working.  Does NOT include TransferTime.
    /// </summary>
    public DateTime EndDateTime => new (m_endTicks);
    #endregion

    public Plant GetScheduledPlant()
    {
        Plant p = null;

        for (int blockNodesI = 0; blockNodesI < m_rrBlockNodes.Length; ++blockNodesI)
        {
            if (m_rrBlockNodes[blockNodesI] != null && m_rrBlockNodes[blockNodesI].Data != null)
            {
                p = m_rrBlockNodes[blockNodesI].Data.ScheduledResource.Plant;
                if (p != null)
                {
                    break;
                }
            }
        }

        return p;
    }

    /// <summary>
    /// Simulation only. Whether the resource that satisified the primary resource requirement is a batch resource.
    /// </summary>
    internal MainResourceDefs.batchType m_batchType; // [BATCH]

    public MainResourceDefs.batchType BatchType => m_batchType;
    
    #region IEnumerator
    public IEnumerator<InternalActivity> GetEnumerator()
    {
        return m_activities.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    internal InternalActivity GetFirstActivity()
    {
        if (m_activities.Count > 0)
        {
            return m_activities.First.Value;
        }

        return null;
    }

    /// <summary>
    /// Return the start time of where a Resource Requirements usageEnum's segment of the batch would start.
    /// This function will always return a value even if the segment's length is 0.
    /// If the length is 0, it will return the time when the usage would start.
    /// For instance if a batch doesn't have any setup, but the requirement's usage is Setup, it will return the start of the batch (which is where
    /// setup would start if there was setup).
    /// </summary>
    /// <param name="a_rrIdx">The index of a Resource Requirement's start time you want.</param>
    /// <returns>The start of the time of the segment of the batch.</returns>
    internal virtual long GetUsageStart(int a_rrIdx)
    {
        InternalActivity act = GetFirstActivity();
        ResourceRequirement rr = act.Operation.ResourceRequirements.GetByIndex(a_rrIdx);
        return m_si.GetUsageStart(rr);
    }

    /// <summary>
    /// Return the end time of where a Resource Requirements usageEnum's segment of the batch would end.
    /// This function will always return a value even if the segment's length is 0.
    /// For instance if a batch doesn't have any setup, but the requirement's usage is Setup, it will return the start of the batch (which is where
    /// setup would end if there was setup).
    /// </summary>
    /// <param name="a_rrIdx">The index of a Resource Requirement's end time you want.</param>
    /// <returns>The start of the time of the segment of the batch.</returns>
    internal virtual long GetUsageEnd(int a_rrIdx)
    {
        InternalActivity act = GetFirstActivity();
        ResourceRequirement rr = act.Operation.ResourceRequirements.GetByIndex(a_rrIdx);

        return m_si.GetUsageEnd(rr);
    }

    [Conditional("DEBUG")]
    protected void EndTest(long a_end)
    {
        if (a_end == 0)
        {
            Type type = GetType();
            string msg = "An unhandled TankBatch UsageEnum was encountered." + type.FullName;
            throw new Exception("msg.");
        }
    }

    /// <summary>
    /// Get a copy of the block used to satisfy the resource requirements. Each index
    /// corresponds to the index of a resource requirement.
    /// </summary>
    /// <returns></returns>
    private ResourceBlock[] GetRRBlocks()
    {
        ResourceBlock[] blocks = new ResourceBlock[m_rrBlockNodes.Length];
        for (int i = 0; i < m_rrBlockNodes.Length; ++i)
        {
            if (m_rrBlockNodes[i] != null)
            {
                blocks[i] = m_rrBlockNodes[i].Data;
            }
        }

        return blocks;
    }

    //RequiredSpan m_procActiveSpan = RequiredSpan.ZeroRequiredSpan;
    /// <summary>
    /// The amount of time Processing runs in the resource wall clock time; this is regardless of cycle efficiency, number of people, etc.
    /// </summary>
    //public RequiredSpan ProcessingActiveSpan
    //{
    //    get
    //    {
    //        return GetClockSpan(m_setupEndTicks, m_processingEndTicks, ref m_procActiveSpan);
    //    }
    //}

    //RequiredSpan m_activeSpan = RequiredSpan.ZeroRequiredSpan;
    /// <summary>
    /// The amount of time the batch runs for in resource wall clock time; that is time regardless of cycle efficiency, number of people, etc.
    /// </summary>
    //public RequiredSpan ActiveSpan
    //{
    //    get
    //    {
    //        if (m_activeSpan == RequiredSpan.ZeroRequiredSpan)
    //        {
    //            long processingClockSpan = PrimaryResource.FindCapacityBetweenStartAndFinish(StartTicks, EndTicks, m_activities.First.Value, false);
    //            m_activeSpan = new RequiredSpan(processingClockSpan, false);
    //        }
    //        return GetClockSpan(StartTicks, EndTicks, ref m_activeSpan);
    //    }
    //}

    //RequiredSpan GetClockSpan(long a_StartTicks, long a_endTicks, ref RequiredSpan a_span)
    //{
    //    if (m_activeSpan == RequiredSpan.ZeroRequiredSpan)
    //    {
    //        long processingClockSpan = PrimaryResource.FindCapacityBetweenStartAndFinish(a_StartTicks, a_endTicks, m_activities.First.Value, false);
    //        a_span = new RequiredSpan(processingClockSpan, false);
    //    }
    //    return a_span;
    //}

    /// <summary>
    /// Returns the block node corresponding to the specified resource
    /// This can be used if the resource is known but the resource requirement is not
    /// </summary>
    public ResourceBlockList.Node BlockForResource(BaseResource a_resource)
    {
        foreach (ResourceBlockList.Node node in m_rrBlockNodes)
        {
            //node can be null if the helper resource requirement was not needed during scheduling
            if (node == null)
            {
                continue;
            }

            if (node.Data.ScheduledResource == a_resource)
            {
                return node;
            }
        }

        return null;
    }
}