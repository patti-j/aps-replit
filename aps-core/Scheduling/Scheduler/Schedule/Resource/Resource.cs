using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Attributes;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.Block.UsageDetails;
using PT.Scheduler.Schedule.Operation;
using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using System.ComponentModel;
using System.Globalization;

namespace PT.Scheduler;

/// <summary>
/// Represents a standard machine.
/// </summary>
public partial class Resource : InternalResource, IPTSerializable
{
    #region IPTSerializable Members
    public Resource(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader, a_idGen)
    {
        if (a_reader.VersionNumber >= 12320)
        {
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_maxQtyPerCycle);

            a_reader.Read(out m_postActivityRestSpan);
            m_flags = new BoolVector32(a_reader);

            m_blocks = new ResourceBlockList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
        }
        else if (a_reader.VersionNumber >= 12106)
        {
            a_reader.Read(out string m_compatibilityGroup);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_maxQtyPerCycle);

            a_reader.Read(out m_postActivityRestSpan);
            m_flags = new BoolVector32(a_reader);

            m_blocks = new ResourceBlockList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
        }
        else if (a_reader.VersionNumber >= 682)
        {
            a_reader.Read(out string m_compatibilityGroup);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_maxQtyPerCycle);

            a_reader.Read(out m_postActivityRestSpan);
            m_flags = new BoolVector32(a_reader);

            m_blocks = new ResourceBlockList(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_minQty);
        a_writer.Write(m_maxQty);
        a_writer.Write(m_minQtyPerCycle);
        a_writer.Write(m_maxQtyPerCycle);

        a_writer.Write(m_postActivityRestSpan);
        m_flags.Serialize(a_writer);

        m_blocks.Serialize(a_writer);

        a_writer.Write(m_minVolume);
        a_writer.Write(m_maxVolume);
    }

    public new const int UNIQUE_ID = 320;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(PlantManager a_plantManager)
    {
        ResourceBlockList.Node node = m_blocks.First;

        while (node != null)
        {
            ResourceBlock rb = node.Data;

            rb.ScheduledResource = this;
            rb.RestoreReferences(a_plantManager);

            node = node.Next;
        }
    }

    // [BATCH_CODE]

    #region RestoreReferences2: Only one of the two below should be called sometime after RestoreReferences.
    internal void RestoreReferences_2_batches(Dictionary<BaseId, Batch> a_batchDictionary)
    {
        ResourceBlockList.Node node = m_blocks.First;
        while (node != null)
        {
            ResourceBlock rb = node.Data;
            rb.RestoreReferences_2_batches(a_batchDictionary, node);
            node = node.Next;
        }
    }
    #endregion
    #endregion

    #region Construction
    public Resource(BaseId a_id, Department a_w, ReadyActivitiesDispatcher a_dispatcher, ShopViewResourceOptions a_resourceOptions)
        : base(a_id, a_w, a_dispatcher, a_resourceOptions) { }

    /// <summary>
    /// Sets the field values for an ERP transmission.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="wcn"></param>
    /// <param name="plant"></param>
    public Resource(BaseId a_id, ResourceT.Resource a_m, Department a_w, ReadyActivitiesDispatcher a_dispatcher, ShopViewResourceOptions a_resourceOptions)
        : base(a_id, a_w, a_dispatcher, a_resourceOptions) { }

    public class MachineException : PTException
    {
        public MachineException(string a_e) : base(a_e) { }
    }

    public Resource(Resource a_origRes, BaseId a_newId, ScenarioDetail a_sd)
        : base(a_origRes, a_newId, a_sd)
    {
        Common.Cloning.PrimitiveCloning.PrimitiveClone(a_origRes,
            this,
            typeof(Resource),
            Common.Cloning.PrimitiveCloning.Depth.Shallow,
            Common.Cloning.PrimitiveCloning.OtherIncludedTypes.All);
        m_blocks = new ResourceBlockList(); //Don't reference the same blocks.
    }

    /// <summary>
    /// This is used for maintaining CustomOrderedListOptimized, don't use to instantiate an instance for other use.
    /// </summary>
    internal Resource() { }
    #endregion

    #region Shared Properties

    private long m_postActivityRestSpan;

    /// <summary>
    /// Specifies the amount of idle time to schedule between Activities to provide time for rest or miscellaneous tasks.
    /// </summary>
    [Browsable(false)] //Not yet
    public TimeSpan PostActivityRestSpan
    {
        get => new(m_postActivityRestSpan);
        set => m_postActivityRestSpan = value.Ticks;
    }

    private BoolVector32 m_flags;
    private const int c_sequentialIdx = 0;
    private const int c_ReservedForMoveSequenceIdx = 1;
    private const int c_showActivityBoardIdx = 2;
    private const int c_storageAreaResourceIdx = 3;

    public const string SEQUENTIAL = "Sequential"; //Must match property name!

    // *LRH* This property should be deleted after Conveyor resource have been created.
    /// <summary>
    /// This property only applies to infinite resources.
    /// Makes sure that every activity scheduled finishes on or after the last scheduled block time.
    /// In the case where the activity being scheduled would have been scheduled to complete before the last block that was scheduled the scheduled finish time is
    /// adjusted so that the block is scheduled to finish at the same time as the last scheduled block.
    /// This property was added to help model conveyors. Model the minimum length of time an activity must stay on the conveyor (transfer time) using a Infinite
    /// sequential resource. And model the unloading of the conveyor as a finite resource. You may also need to place the 2 resources used to model the conveyor
    /// and predecessor resource in the same cell. Setup the unloading resource and operation so that the length of the unload matches the length of the load.
    /// </summary>
    public bool Sequential
    {
        get => m_flags[c_sequentialIdx];

        internal set
        {
            if (m_flags[c_sequentialIdx] != value)
            {
                m_flags[c_sequentialIdx] = value;
            }
        }
    }

    /// <summary>
    /// During a Move, while this is true, only activities that are being moved can be scheduled on the resource.
    /// For instance if 10 blocks were moved to a resource, this would be set to true
    /// </summary>
    internal bool ReservedForMoveSequence
    {
        get => m_flags[c_ReservedForMoveSequenceIdx];
        set => m_flags[c_ReservedForMoveSequenceIdx] = value;
    }

    /// <summary>
    /// Returns whether this resource should be displayed differently for the ActivityBoard
    /// </summary>
    public bool ShowInActivityBoard
    {
        get => true; //for testing
        //get { return m_flags[c_showActivityBoardIdx]; }
        internal set => m_flags[c_showActivityBoardIdx] = value;
    }

    public bool StorageAreaResource
    {
        get => m_flags[c_storageAreaResourceIdx];
        internal set => m_flags[c_storageAreaResourceIdx] = value;
    }

    public const string MinQty_CriticalFieldChangeName = "MinQty";
    private decimal m_minQty;

    /// <summary>
    /// For the Resource to be considered eligible for an Activity the Activity RequiredFinishQty must be at least this amount.
    /// </summary>
    public decimal MinQty
    {
        get => m_minQty;
        internal set
        {
            if (m_minQty != value)
            {
                m_minQty = value;
            }
        }
    }

    public const string MaxQty_CriticalFieldChangeName = "MaxQty";
    private decimal m_maxQty = decimal.MaxValue;

    /// <summary>
    /// For the Resource to be considered eligible for an Activity the Activity RequiredFinishQty must be less than or equal to this amount.
    /// </summary>
    public decimal MaxQty
    {
        get => m_maxQty;
        internal set
        {
            if (m_maxQty != value)
            {
                m_maxQty = value;
            }
        }
    }
    public bool MaxQuantityConstrained => m_maxQty != decimal.MaxValue && m_maxQty != 0m;
    public bool MinQuantityConstrained => m_minQty != decimal.MinValue && m_minQty != 0m;

    public const string MinQtyPerCycle_CriticalFieldChangeName = "MinQtyPerCycle";
    private decimal m_minQtyPerCycle;

    /// <summary>
    /// For the Resource to be considered eligible for an Operation the Operation's QtyPerCyle must be at least this amount.
    /// This is often used for batch processes where a Resource can hold a certain volume and there is a desire to use smaller Resources for smaller orders to avoid wasting capacity.
    /// </summary>
    public decimal MinQtyPerCycle
    {
        get => m_minQtyPerCycle;
        internal set => m_minQtyPerCycle = value;
    }

    public const string MaxQtyPerCycle_CriticalFieldChangeName = "MaxQtyPerCycle";
    private decimal m_maxQtyPerCycle = decimal.MaxValue;

    /// <summary>
    /// For the Resource to be considered eligible for an Operation the Operation's QtyPerCyle must be less than or equal to this amount.
    /// This is often used for batch processes where a Resource can hold a certain volume and that volume cannot be exceeded.
    /// </summary>
    public decimal MaxQtyPerCycle
    {
        get => m_maxQtyPerCycle;
        internal set => m_maxQtyPerCycle = value;
    }

    private decimal m_minVolume;

    /// <summary>
    /// For the Resource to be considered eligible for an Activity the Operation's ProductVolume must be at least this amount.
    /// </summary>
    public decimal MinVolume
    {
        get => m_minVolume;
        internal set => m_minVolume = value;
    }

    public bool MinVolumeConstrained => m_minVolume != decimal.MinValue && m_minVolume != 0m;
    public bool MaxVolumeConstrained => m_maxVolume != decimal.MaxValue && m_maxVolume != 0m;

    private decimal m_maxVolume;

    /// <summary>
    /// For the Resource to be considered eligible for an Activity the Operation's ProductVolume must be less than or equal to this amount.
    /// </summary>
    public decimal MaxVolume
    {
        get => m_maxVolume;
        internal set => m_maxVolume = value;
    }
    #endregion Shared Properties

    #region Transmission functionality
    public bool Receive(ResourceIdBaseT a_t, ScenarioDetail a_sd, ISystemLogger a_errorReporter, IScenarioDataChanges a_dataChanges)
    {
        bool criticalChange = BaseResourceReceive(a_t);

        return criticalChange;
    }

    protected override bool CriticalFieldChange(string a_propertyName)
    {
        if (a_propertyName == MinQty_CriticalFieldChangeName || a_propertyName == MaxQty_CriticalFieldChangeName || a_propertyName == MinQtyPerCycle_CriticalFieldChangeName || a_propertyName == MaxQtyPerCycle_CriticalFieldChangeName)
        {
            return true;
        }

        return base.CriticalFieldChange(a_propertyName);
    }

    /// <summary>
    /// </summary>
    /// <param name="m"></param>
    /// <param name="w"></param>
    /// <returns>Whether any critcal updates were made. Critical update are changes that require the scheduler be time adjusted.</returns>
    internal bool Update(ResourceT.Resource a_m, Department a_w, UserFieldDefinitionManager a_udfManager, ScenarioDetail a_scenarioDetail, ResourceT a_resourceImportT, ISystemLogger a_errorReporter, IScenarioDataChanges a_dataChanges)
    {
        if (StorageAreaResource)
        {
            //Storage Area resources must be single tasking and have no capabilities
            if (a_m.CapacityType != InternalResourceDefs.capacityTypes.SingleTasking)
            {
                throw new PTValidationException("3134", [ExternalId]);
            }

            if (a_m.Capabilities.Count > 0)
            {
                throw new PTValidationException("3135", [ExternalId]);
            }
        }

        bool updated = base.Update(a_m, a_udfManager, a_w, a_scenarioDetail, a_resourceImportT, a_errorReporter, a_dataChanges);

        if (a_m.PostActivityRestSpanSet && PostActivityRestSpan != a_m.PostActivityRestSpan)
        {
            updated = true;
            PostActivityRestSpan = a_m.PostActivityRestSpan;
        }

        if (a_m.MinQtySet && MinQty != a_m.MinQty)
        {
            updated = true;
            MinQty = a_m.MinQty;
            a_dataChanges.FlagEligibilityChanges(Id);
        }

        if (a_m.MaxQtySet && MaxQty != a_m.MaxQty)
        {
            updated = true;
            MaxQty = a_m.MaxQty;
            a_dataChanges.FlagEligibilityChanges(Id);
        }

        if (a_m.MinQtyPerCycleSet && MinQtyPerCycle != a_m.MinQtyPerCycle)
        {
            updated = true;
            MinQtyPerCycle = a_m.MinQtyPerCycle;
            a_dataChanges.FlagEligibilityChanges(Id);
        }

        if (a_m.MaxQtyPerCycleSet && MaxQtyPerCycle != a_m.MaxQtyPerCycle)
        {
            updated = true;
            MaxQtyPerCycle = a_m.MaxQtyPerCycle;
            a_dataChanges.FlagEligibilityChanges(Id);
        }

        if (a_m.SequentialSet && Sequential != a_m.Sequential)
        {
            updated = true;
            Sequential = a_m.Sequential;
        }

        if (a_m.CellSet)
        {
            if (a_m.CellExternalId == "") //remove the Cell association if there is one
            {
                if (Cell != null)
                {
                    Cell.RemoveResourceAssociation(this);
                    DissassociateCell();
                    updated = true;
                }
            }
            else
            {
                Cell cell = a_scenarioDetail.CellManager.GetByExternalId(a_m.CellExternalId);
                if (cell != null) //found the cell
                {
                    if (Cell != null && Cell.Id != cell.Id) //already in a different Cell so remove association from the current Cell
                    {
                        Cell.RemoveResourceAssociation(this);
                        DissassociateCell();
                        updated = true;
                    }

                    if (Cell == null)
                    {
                        cell.AddResourceAssociation(this);
                        AssociateCell(cell);
                        updated = true;
                        a_dataChanges.FlagEligibilityChanges(Id);
                    }
                }
                else
                {
                    throw new PTValidationException("2760", new object[] { a_m.CellExternalId });
                }
            }
        }

        if (a_m.MinVolumeSet && a_m.MinVolume != MinVolume)
        {
            MinVolume = a_m.MinVolume;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        if (a_m.MaxVolumeSet && a_m.MaxVolume != MaxVolume)
        {
            MaxVolume = a_m.MaxVolume;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        return updated;
    }

    internal bool Edit(ScenarioDetail a_scenarioDetail, UserFieldDefinitionManager a_udfManager, ResourceEdit a_resEdit, ResourceEditT a_resEditT, ISystemLogger a_errorReporter, IScenarioDataChanges a_dataChanges)
    {
        if (StorageAreaResource)
        {
            //Storage Area resources must be single tasking and have no capabilities
            if (a_resEdit.CapacityType != InternalResourceDefs.capacityTypes.SingleTasking)
            {
                throw new PTValidationException("3134", [ExternalId]);
            }
        }

        bool updated = base.Edit(a_scenarioDetail, a_udfManager, a_resEdit, a_resEditT, a_errorReporter, a_dataChanges);
        
        if (a_resEdit.MinQtySet && MinQty != a_resEdit.MinQty)
        {
            MinQty = a_resEdit.MinQty;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        if (a_resEdit.MaxQtySet && MaxQty != a_resEdit.MaxQty)
        {
            MaxQty = a_resEdit.MaxQty;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        if (a_resEdit.MinQtyPerCycleSet && MinQtyPerCycle != a_resEdit.MinQtyPerCycle)
        {
            MinQtyPerCycle = a_resEdit.MinQtyPerCycle;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        if (a_resEdit.MaxQtyPerCycleSet && MaxQtyPerCycle != a_resEdit.MaxQtyPerCycle)
        {
            MaxQtyPerCycle = a_resEdit.MaxQtyPerCycle;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        if (a_resEdit.SequentialSet && Sequential != a_resEdit.Sequential)
        {
            updated = true;
            Sequential = a_resEdit.Sequential;
        }

        if (a_resEdit.CellExternalIdSet)
        {
            if (a_resEdit.CellExternalId == "") //remove the Cell association if there is one
            {
                if (Cell != null)
                {
                    Cell.RemoveResourceAssociation(this);
                    DissassociateCell();
                    updated = true;
                }
            }
            else
            {
                Cell cell = a_scenarioDetail.CellManager.GetByExternalId(a_resEdit.CellExternalId);
                if (cell != null) //found the cell
                {
                    if (Cell != null && Cell.Id != cell.Id) //already in a different Cell so remove association from the current Cell
                    {
                        Cell.RemoveResourceAssociation(this);
                        DissassociateCell();
                        updated = true;
                    }

                    if (Cell == null)
                    {
                        cell.AddResourceAssociation(this);
                        AssociateCell(cell);
                        updated = true;
                        a_dataChanges.FlagEligibilityChanges(Id);
                    }
                }
                else
                {
                    throw new PTValidationException("2760", new object[] { a_resEdit.CellExternalId });
                }
            }
        }

        if (a_resEdit.MinVolumeSet && a_resEdit.MinVolume != MinVolume)
        {
            MinVolume = a_resEdit.MinVolume;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        if (a_resEdit.MaxVolumeSet && a_resEdit.MaxVolume != MaxVolume)
        {
            MaxVolume = a_resEdit.MaxVolume;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        return updated;
    }

    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "Resource";

    /// <summary>
    /// If more than the Plant's Bottleneck Threshold of Activites on the Resource's schedule are Capacity Bottlenecked then the Resource is flagged as a Bottleneck.
    /// </summary>
    public override bool Bottleneck => BottleneckPercent > Department.Plant.BottleneckThreshold;

    public int GetBottleneckActivityCount()
    {
        if (Blocks.Count == 0)
        {
            return 0;
        }

        ResourceBlockList.Node node = Blocks.First;
        int bottleneckedActivityCount = 0;
        while (node != null)
        {
            ResourceBlock machineBlock = node.Data;
            if (machineBlock.Batch.FirstActivity.Timing == BaseActivityDefs.onTimeStatuses.CapacityBottleneck)
            {
                bottleneckedActivityCount++;
            }

            node = node.Next;
        }

        return bottleneckedActivityCount;
    }

    /// <summary>
    /// The percent of the Resource's scheduled Activities that are Capacity Bottlenecked.
    /// </summary>
    public override decimal BottleneckPercent
    {
        get
        {
            int bottleneckActivityCount = GetBottleneckActivityCount();
            if (bottleneckActivityCount == 0)
            {
                return 0;
            }

            return bottleneckActivityCount / (decimal)Blocks.Count * 100;
        }
    }
    #endregion

    #region Properties
    private readonly ResourceBlockList m_blocks = new();

    [Browsable(false)]
    [DoNotAuditProperty]
    public ResourceBlockList Blocks => m_blocks;

    public ResourceKey GetResourceKey()
    {
        return new ResourceKey(Plant.Id, DepartmentId, Id);
    }

    public Tuple<string, string, string> GetExternalKey()
    {
        return new Tuple<string, string, string>(Plant.ExternalId, Department.ExternalId, ExternalId);
    }
    #endregion

    internal void Deleting(ScenarioDetail a_sd, bool a_removeCapabilities, bool a_removeCapacityIntervals, bool a_removeRecurringCapacityIntervals, bool a_removeAllowedHelpers)
    {
        base.Deleting(a_sd);

        //Remove product rules for this resource
        a_sd.ProductRuleManager.DeletingResource(Id);
        a_sd.CompressSettings.LimitToResources.Remove(Id);

        ResourceBlockList.Node currentNode = Blocks.First;
        List<Job> jobs = new();

        while (currentNode != null)
        {
            ResourceBlock mb = currentNode.Data;
            jobs.Add(mb.Batch.FirstActivity.Operation.ManufacturingOrder.Job);
            currentNode = currentNode.Next;
        }

        Unschedule(jobs, a_sd);

        //Remove capability references
        if (a_removeCapabilities)
        {
            for (int capI = 0; capI < NbrCapabilities; capI++)
            {
                Capability capability = GetCapabilityByIndex(capI);
                capability.RemoveResourceAssociation(this);
            }
        }

        //Remove Capacity Interval references
        if (a_removeCapacityIntervals)
        {
            for (int i = 0; i < CapacityIntervals.Count; i++)
            {
                CapacityInterval ci = CapacityIntervals[i];
                RemoveCapacityInterval(ci);
            }
        }

        if (a_removeRecurringCapacityIntervals)
        {
            for (int i = 0; i < RecurringCapacityIntervals.Count; i++)
            {
                RecurringCapacityInterval rci = RecurringCapacityIntervals[i];
                RemoveRecurringCapacityInterval(rci);
            }
        }

        //Remove Allowed Helper references
        if (a_removeAllowedHelpers)
        {
            if (Plant.AllowedHelperManager.GetRelationshipCount() > 0) // Don't bother if there are no helpers defined.
            {
                Dictionary<BaseId, Resource> resourceDictionary = new();
                List<Resource> resourceList = a_sd.PlantManager.GetResourceList();
                foreach (Resource res in resourceList)
                {
                    resourceDictionary.Add(res.Id, res);
                }

                //Remove this resource since it is being deleted, but hasn't been removed yet.
                resourceDictionary.Remove(Id);
                Plant.AllowedHelperManager.HandleDeletedResources(resourceDictionary);
            }
        }

        a_sd.ResourceConnectorManager.DeletingResource(this);

        a_sd.WarehouseManager.DeletingResources(this);
    }

    private void Unschedule(List<Job> a_jobs, ScenarioDetail a_sd)
    {
        if (a_jobs.Count > 0)
        {
            a_sd.JobsUnscheduled();

            for (int jI = 0; jI < a_jobs.Count; ++jI)
            {
                a_jobs[jI].Unschedule();
            }
        }
    }

    #region Update
    /// <summary>
    /// When disassociating a capability always call this function in place of operating directly on the capability manager.
    /// MOs whose resource requirements can no longer be fullfilled by this change are unscheduled.
    /// </summary>
    /// <param name="c">The capability to disassociate.</param>
    internal override bool DisassociateCapability(Capability a_c, ProductRuleManager a_productRuleManager)
    {
        base.DisassociateCapability(a_c, a_productRuleManager);
        HashSet<Job> unscheduleJobs = new();

        ResourceBlockList.Node currentNode = Blocks.First;
        while (currentNode != null)
        {
            ResourceBlock rb = currentNode.Data;
            IEnumerator<InternalActivity> activitiesIterator = rb.Batch.GetEnumerator();
            while (activitiesIterator.MoveNext())
            {
                InternalActivity activity = activitiesIterator.Current;
                ResourceRequirement rr = activity.Operation.ResourceRequirements.GetByIndex(rb.ResourceRequirementIndex);

                if (rr != null && rr.UsesCapability(a_c))
                {
                    Job job = activity.Operation.ManufacturingOrder.Job;
                    if (!unscheduleJobs.Contains(job))
                    {
                        unscheduleJobs.Add(job);
                    }
                }
            }

            currentNode = currentNode.Next;
        }

        HashSet<Job>.Enumerator unschedJobsEnumerator = unscheduleJobs.GetEnumerator();
        bool jobsAffected = false;
        while (unschedJobsEnumerator.MoveNext())
        {
            Job job = unschedJobsEnumerator.Current;
            job.Unschedule();
            job.ComputeEligibilityAndUnscheduleIfIneligible(a_productRuleManager); //TODO: This may be unnecessary since compute eligibility is called again later
            jobsAffected = true;
        }

        return jobsAffected;
    }
    #endregion

    #region Capacity Plan
    internal enum capacityEventTypes
    {
        //Note: The order of these enums is important and is used to sort events
        EnterBucket = 0,
        LeaveOnline,
        EnterOnline,
        LeaveBlock,
        EnterBlock,
        LeaveLastBucket
    }

    internal class CapacityEvent : IComparable
    {
        internal CapacityEvent(DateTime a_eventTime)
        {
            m_eventTime = a_eventTime.Ticks;
        }

        internal long m_eventTime;
        protected capacityEventTypes m_intervalType;

#if DEBUG
        /// <summary>
        /// This is for debugging so you can see the actual time.
        /// </summary>
        public DateTimeOffset DisplayEventTime => new DateTime(m_eventTime).ToDisplayTime();

        protected string GetToString(CapacityEvent a_ce)
        {
            return string.Format("{0} {1}", DisplayEventTime, a_ce.GetType().Name);
        }

        public override string ToString()
        {
            return GetToString(this);
        }
#endif

        #region IComparable Members
        public int CompareTo(object obj)
        {
            CapacityEvent c = (CapacityEvent)obj;

            if (m_eventTime < c.m_eventTime)
            {
                return -1;
            }

            if (m_eventTime == c.m_eventTime)
            {
                if (m_intervalType < c.IntervalType)
                {
                    return -1;
                }

                if (m_intervalType > c.IntervalType)
                {
                    return 1;
                }

                return 0;
            }

            return 1;
        }
        #endregion

        /// <summary>
        /// Enum type used for sorting and identification
        /// </summary>
        public capacityEventTypes IntervalType => m_intervalType;
    }

    internal class OnlineCapacityEvent : CapacityEvent
    {
        internal OnlineCapacityEvent(DateTime a_eventTime, ResourceCapacityInterval a_rci) : base(a_eventTime)
        {
            m_rci = a_rci;
        }

        private readonly ResourceCapacityInterval m_rci;

        public decimal IntervalNbrOfPeople => m_rci.NbrOfPeople;

        public bool IsOvertime => m_rci.Overtime;

        public long GetIntervalCost(CapacityEvent a_nextEvent, long a_lastEnterBucketTicks, long a_lastEnterOnlineTicks, decimal a_maxNbrPeopleSumInThisOnlineInterval, Resource a_Resource)
        {
            decimal hourlyCost;
            if (IsOvertime)
            {
                hourlyCost = a_Resource.OvertimeHourlyCost;
            }
            else
            {
                hourlyCost = a_Resource.StandardHourlyCost;
            }

            long ticksOnlineInCurrentBucket = a_nextEvent.m_eventTime - Math.Max(a_lastEnterBucketTicks, a_lastEnterOnlineTicks);
            //return TimeSpan.FromHours(Math.Ceiling(new TimeSpan(ticksOnlineInCurrentBucket * (long)a_maxNbrPeopleSumInThisOnlineInterval).TotalHours * hourlyCost)).Ticks;
            // 20147.04.14: in the conversion to decimal, the commented out line above was rewritten as the lines below.
            decimal hrs = (decimal)new TimeSpan(ticksOnlineInCurrentBucket * (long)a_maxNbrPeopleSumInThisOnlineInterval).TotalHours;
            decimal cost = hrs * hourlyCost;
            cost = decimal.Ceiling(cost);
            TimeSpan ts = TimeSpan.FromHours((double)cost);

            return ts.Ticks;
        }
    }

    internal class EnterOnlineCapacityEvent : OnlineCapacityEvent
    {
        internal EnterOnlineCapacityEvent(DateTime a_eventTime, ResourceCapacityInterval a_rci)
            : base(a_eventTime, a_rci)
        {
            m_intervalType = capacityEventTypes.EnterOnline;
        }
    }

    internal class LeaveOnlineCapacityEvent : OnlineCapacityEvent
    {
        internal LeaveOnlineCapacityEvent(DateTime a_eventTime, ResourceCapacityInterval a_rci)
            : base(a_eventTime, a_rci)
        {
            m_intervalType = capacityEventTypes.LeaveOnline;
        }
    }

    internal class EnterBucketCapacityEvent : CapacityEvent
    {
        internal EnterBucketCapacityEvent(DateTime a_eventTime)
            : base(a_eventTime)
        {
            m_intervalType = capacityEventTypes.EnterBucket;
        }
    }

    internal class LeaveLastBucketCapacityEvent : CapacityEvent
    {
        internal LeaveLastBucketCapacityEvent(DateTime a_eventTime)
            : base(a_eventTime)
        {
            m_intervalType = capacityEventTypes.LeaveLastBucket;
        }
    }

    internal class BlockCapacityEvent : CapacityEvent
    {
        internal BlockCapacityEvent(DateTime a_eventTime, ResourceBlock a_block) : base(a_eventTime)
        {
            m_block = a_block;
        }

        private readonly ResourceBlock m_block;

        public BlockKey BlockId => m_block.GetKey();

        internal decimal GetNbrPeopleScaledByAttentionPctForBlock(decimal a_capacityIntervalNbrPeople)
        {
            decimal nbrPeople;
            if (m_block.Activity.PeopleUsage == InternalActivityDefs.peopleUsages.UseAllAvailable)
            {
                nbrPeople = a_capacityIntervalNbrPeople;
            }
            else if (m_block.Activity.PeopleUsage == InternalActivityDefs.peopleUsages.UseMultipleOfSpecifiedNbr)
            {
                nbrPeople = Math.Floor(a_capacityIntervalNbrPeople / m_block.Activity.NbrOfPeople) * m_block.Activity.NbrOfPeople;
            }
            else //use specified
            {
                nbrPeople = m_block.Activity.NbrOfPeople;
            }

            return nbrPeople * m_block.SatisfiedRequirement.AttentionPercent / 100;
        }
    }

    internal class EnterBlockCapacityEvent : BlockCapacityEvent
    {
        internal EnterBlockCapacityEvent(DateTime a_eventTime, ResourceBlock a_block)
            : base(a_eventTime, a_block)
        {
            m_intervalType = capacityEventTypes.EnterBlock;
        }
    }

    internal class LeaveBlockCapacityEvent : BlockCapacityEvent
    {
        internal LeaveBlockCapacityEvent(DateTime a_eventTime, ResourceBlock a_block)
            : base(a_eventTime, a_block)
        {
            m_intervalType = capacityEventTypes.LeaveBlock;
        }
    }

    /// <summary>
    /// Keeps track of the NbrOfPeople during the capacity calculations.
    /// </summary>
    internal class NbrOfPeopleTracker
    {
        private readonly Dictionary<BlockKey, BlockCapacityEvent> m_runningBlockDict;
        private decimal NbrPeopleCapacity;
        private bool usageChanged;
        private decimal NbrOfPeopleUsage;
        private decimal MaxNbrOfPeople;

        internal NbrOfPeopleTracker()
        {
            m_runningBlockDict = new Dictionary<BlockKey, BlockCapacityEvent>();
        }

        /// <summary>
        /// Add an enter block event
        /// </summary>
        public void AddBlock(BlockCapacityEvent a_event)
        {
            m_runningBlockDict.Add(a_event.BlockId, a_event);
            usageChanged = true;
        }

        /// <summary>
        /// Update current capacity
        /// </summary>
        public void UpdateCapacity(OnlineCapacityEvent a_event)
        {
            if (a_event is EnterOnlineCapacityEvent)
            {
                NbrPeopleCapacity += a_event.IntervalNbrOfPeople;
            }
            else if (a_event is LeaveOnlineCapacityEvent)
            {
                NbrPeopleCapacity = 0;
                MaxNbrOfPeople = 0;
            }

            usageChanged = true;
        }

        /// <summary>
        /// Add a leave block event
        /// </summary>
        public void RemoveBlock(BlockCapacityEvent a_event)
        {
            m_runningBlockDict.Remove(a_event.BlockId);
            usageChanged = true;
        }

        /// <summary>
        /// Calculate the current NbrOfPeopleUsage and MaxNbrOfPeopleUsage
        /// </summary>
        public decimal GetTotalNbrOfPeopleUsage()
        {
            if (usageChanged)
            {
                NbrOfPeopleUsage = 0;
                foreach (KeyValuePair<BlockKey, BlockCapacityEvent> valuePair in m_runningBlockDict)
                {
                    NbrOfPeopleUsage += valuePair.Value.GetNbrPeopleScaledByAttentionPctForBlock(NbrPeopleCapacity);
                }

                if (NbrOfPeopleUsage > MaxNbrOfPeople)
                {
                    MaxNbrOfPeople = NbrOfPeopleUsage;
                }
            }

            usageChanged = false;
            return NbrOfPeopleUsage;
        }

        public decimal GetMaxNbrOfPeople()
        {
            GetTotalNbrOfPeopleUsage();
            return Math.Ceiling(MaxNbrOfPeople);
        }
    }

    private bool FilterUsage(MainResourceDefs.usageEnum a_usageType, MainResourceDefs.usageEnum a_operationUsageEnum)
    {
        if (a_usageType == MainResourceDefs.usageEnum.Unspecified)
        {
            return true;
        }

        return a_usageType == a_operationUsageEnum;
    }
    /// <param name="a_start">The time for the first bucket to start.</param>
    /// <param name="a_endInclusive">The last time that must be included.  The last bucket may run past this time.</param>
    /// <param name="a_bucketLength">The time between the start and end of each bucket.</param>
    internal TimeBucketList GetBucketedUsage(DateTime a_start, DateTime a_endInclusive, TimeSpan a_bucketLength, CapacityInfoBase.GroupChooser a_chooser, MainResourceDefs.usageEnum a_usageType = MainResourceDefs.usageEnum.Unspecified)
    {
        TimeBucketList capacityBuckets = new(a_start, a_endInclusive, a_bucketLength);
        //Add the various types of events
        //Block events -- enter/leave
        ResourceBlockList.Node node = m_blocks.First;
        while (node != null)
        {
            ResourceBlock block = node.Data;

            OperationCapacityProfile profile = block.CapacityProfile;

            if (!block.Batch.ZeroLength && a_chooser.ChooseBlock(block))
            {
                foreach ((MainResourceDefs.usageEnum operationUsageEnum, OperationCapacity operationCapacity) in profile.CapacityEnumerator())
                {
                    if (block.StartDateTime.Ticks > capacityBuckets.End.Ticks)
                    {
                        break;
                    }

                    if (operationCapacity.StartDate.Ticks >= capacityBuckets.Start.Ticks && FilterUsage(a_usageType, operationUsageEnum))
                    {
                        capacityBuckets.AddTime(operationCapacity.StartDate.Ticks, operationCapacity.TotalCapacityTicks);
                    }
                    else if (operationCapacity.StartDate.Ticks < capacityBuckets.Start.Ticks && operationCapacity.EndTicks <= capacityBuckets.End.Ticks && FilterUsage(a_usageType,operationUsageEnum))
                    {
                        long totalRemainingCapacityTicks = (operationCapacity.EndTicks - capacityBuckets.Start.Ticks);
                        capacityBuckets.AddTime(capacityBuckets.Start.Ticks, totalRemainingCapacityTicks);
                    }
                }
            }

            node = node.Next;
        }

        return capacityBuckets;
    }

    /// <summary>
    /// Adds to the list Blocks that are scheduled to either start or end in the specified bucket.
    /// </summary>
    public override void AddBlocksInBucket(ref ResourceBlockList r_list, DateTime a_start, DateTime a_endInclusive, TimeSpan a_bucketLength, int a_bucket, CapacityInfoBase.GroupChooser a_chooser)
    {
        long bucketStartDT = a_start.Ticks + a_bucketLength.Ticks * a_bucket;
        long bucketEndDT = bucketStartDT + a_bucketLength.Ticks;
        DateTime tempStart = new (bucketStartDT);
        DateTime tempEnd = new (bucketEndDT);
        //Iterate through the blocks adding any that start or end in the bucket or that span the bucket.
        ResourceBlockList.Node node = m_blocks.First;
        while (node != null)
        {
            ResourceBlock block = node.Data;
            if (a_chooser.ChooseBlock(block))
            {
                if (!(block.EndTicks < bucketStartDT || block.StartTicks > bucketEndDT))
                {
                    r_list.Add(block, null);
                }
            }

            node = node.Next;
        }
    }

    /// <summary>
    /// Returns bucketed production quantities.
    /// </summary>
    internal override decimal[] GetBucketedProduction(DateTime a_start, DateTime a_endInclusive, TimeSpan a_bucketLength, CapacityInfoBase.GroupChooser a_chooser)
    {
        TimeBucketList productionBuckets = new(a_start, a_endInclusive, a_bucketLength);
        decimal[] productionUnits = new decimal[productionBuckets.Count];
        ResourceBlockList.Node node = m_blocks.First;
        while (node != null)
        {
            ResourceBlock block = node.Data;
            OperationCapacityProfile profile = block.CapacityProfile;

            if (!block.Batch.ZeroLength && a_chooser.ChooseBlock(block))
            {
                if (block.StartDateTime.Ticks > productionBuckets.End.Ticks)
                {
                    break;
                }

                InternalOperation internalOperation = block.Batch.FirstActivity.Operation;

                foreach (Product operationProduct in internalOperation.Products)
                {
                    if (operationProduct.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.AtOperationRunStart)
                    {
                        long ticks = internalOperation.ScheduledStartTicks + internalOperation.SetupSpanTicks;
                        AddProductionUnit(productionUnits, operationProduct.TotalOutputQty, productionBuckets.End.Ticks, ticks, a_start, a_bucketLength);
                    }
                    else if (operationProduct.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.ByProductionCycle)
                    {
                        decimal cycle = operationProduct.TotalOutputQty / internalOperation.QtyPerCycle;
                        long ticks = internalOperation.StartDateTime.Ticks + internalOperation.SetupSpanTicks;
                        for (int i = 0; i < cycle; i++)
                        {
                            ticks += internalOperation.CycleSpanTicks;
                            AddProductionUnit(productionUnits, internalOperation.QtyPerCycle, productionBuckets.End.Ticks, ticks, a_start, a_bucketLength);
                        }
                    }
                    else if ((operationProduct.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.AtOperationResourcePostProcessingEnd || operationProduct.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.DuringStorage))
                    {
                        long ticks = internalOperation.EndOfRunTicks + internalOperation.ProductionInfo.PostProcessingSpanTicks;
                        AddProductionUnit(productionUnits, operationProduct.TotalOutputQty, productionBuckets.End.Ticks, ticks, a_start, a_bucketLength);
                    }
                    else if ((operationProduct.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.AtOperationRunEnd || operationProduct.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.DuringPostProcessing))
                    {
                        long ticks = internalOperation.EndOfRunDate.Ticks;
                        AddProductionUnit(productionUnits, operationProduct.TotalOutputQty, productionBuckets.End.Ticks, ticks, a_start, a_bucketLength);
                    }
                    else if (operationProduct.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.AtStorageEnd)
                    {
                        long ticks = internalOperation.EndOfRunTicks + internalOperation.ProductionInfo.PostProcessingSpanTicks + internalOperation.ProductionInfo.StorageSpanTicks;
                        AddProductionUnit(productionUnits, operationProduct.TotalOutputQty, productionBuckets.End.Ticks, ticks, a_start, a_bucketLength);
                    }

                }

            }
            node = node.Next;
        }

        return productionUnits;
    }

    private static void AddProductionUnit(decimal[] a_productionUnits, decimal a_quantity, long a_bucketEndTicks, long a_ticks, DateTime a_start, TimeSpan a_bucketLength)
    {
        if (a_ticks > a_bucketEndTicks)
        {
            return;
        }
        int index = (int)((new DateTime(a_ticks) - a_start) / a_bucketLength);

        if (index < a_productionUnits.Length && index >= 0)
        {
            a_productionUnits[index] += a_quantity;
        }
    }
    /// <summary>
    /// Returns a TimeBucketList for Wasted Capacity by iterating through the scheduled usage TimeBucketList to find an online capacity interval at those time and check to specify if the profile of blocks
    /// spanning across those interval can be scheduled otherwise mark those slots as wasted/unutilized
    /// </summary>
    /// <param name="a_start">The time for the first bucket to start.</param>
    /// <param name="a_endInclusive">The last time that must be included.  The last bucket may run past this time.</param>
    /// <param name="a_bucketLength">The time between the start and end of each bucket.</param>
    /// <param name="a_chooser"></param>
    /// <returns></returns>
    internal TimeBucketList GetBucketedWastedCapacity(DateTime a_start, DateTime a_endInclusive, TimeSpan a_bucketLength, CapacityInfoBase.GroupChooser a_chooser)
    {
        TimeBucketList capacityBuckets = new(a_start, a_endInclusive, a_bucketLength);
        SortedList<int,(MainResourceDefs.usageEnum, OperationCapacity)> blockCapacityProfiles = new SortedList<int, (MainResourceDefs.usageEnum, OperationCapacity)>();
        DateTime time = a_start;

        ResourceBlockList.Node node = m_blocks.First;
        int blockCapacityIdx = 0;
        while (node != null)
        {
            ResourceBlock block = node.Data;

            OperationCapacityProfile profile = block.CapacityProfile;

            if (!block.Batch.ZeroLength && a_chooser.ChooseBlock(block))
            {
                foreach ((MainResourceDefs.usageEnum operationUsageEnum, OperationCapacity operationCapacity) in profile.CapacityEnumerator())
                {
                    if (block.StartDateTime > capacityBuckets.End)
                    {
                        break;
                    }

                    if (operationCapacity.StartDate.Ticks >= capacityBuckets.Start.Ticks)
                    {
                        blockCapacityProfiles.Add(blockCapacityIdx, (operationUsageEnum, operationCapacity));
                        blockCapacityIdx++;
                    }
                }
            }

            node = node.Next;
        }

        //Loop through all the capacity buckets created
        for (int i = 0; i < capacityBuckets.Count; i++)
        {
            //Gets all the online or overtime capacity intervals within the start and end dates
            foreach (ResourceCapacityInterval resourceCapacityInterval in capacityProfile.ProfileIntervals.FindAllActiveIntervalWithinDateRange(time, time.Add(a_bucketLength)))
            {
                //Loop through the collection to use end date of the operation and it's manufacturing step at the time
                foreach ((int idx, (MainResourceDefs.usageEnum usage, OperationCapacity operationCapacity)) in blockCapacityProfiles)
                {
                    //if the start date time of the capacity interval is earlier or equal to the operation end date time but cannot schedule the specified manufacturing process step
                    //at that time, then mark that capacity interval at that point as wasted or un-utilized
                    if (resourceCapacityInterval.StartDateTime <= operationCapacity.EndDate.ToDateTime() && !resourceCapacityInterval.CanScheduleProductionUsage(usage))
                    {
                        capacityBuckets[i] = resourceCapacityInterval.GetDuration();
                    }
                }
            }
            time = time.Add(a_bucketLength);

        }

        return capacityBuckets;
    }

    internal TimeBucketList GetBucketedPeopleUtilization(DateTime a_start, DateTime a_endInclusive, TimeSpan a_bucketLength, CapacityInfoBase.GroupChooser a_chooser, MainResourceDefs.usageEnum a_usageType = MainResourceDefs.usageEnum.Unspecified)
    {
        // Create a TimeBucketList for the utilization ratios.
        TimeBucketList peopleUtilizationBuckets = new(a_start, a_endInclusive, a_bucketLength);
        // Build a list of blocks with their OperationCapacity and NbrOfPeopleAdjustedWorkContent.
        SortedList<int, (MainResourceDefs.usageEnum, OperationCapacity operationCapacity, ResourceBlock block)> blockPeopleProfiles = new SortedList<int, (MainResourceDefs.usageEnum, OperationCapacity, ResourceBlock)>();
        DateTime time = a_start;
        ResourceBlockList.Node node = m_blocks.First;
        int blockProfileIdx = 0;

        // Gather data from each block.
        while (node != null)
        {
            ResourceBlock block = node.Data;
            if (!block.Batch.ZeroLength && a_chooser.ChooseBlock(block))
            {
                // Retrieve the adjusted work content (ensure this property is correctly exposed on the Activity).
                OperationCapacityProfile profile = block.CapacityProfile;
                foreach ((MainResourceDefs.usageEnum operationUsageEnum, OperationCapacity operationCapacity) in profile.CapacityEnumerator())
                {
                    // Only consider blocks that start within our bucket range.
                    if (block.StartDateTime > peopleUtilizationBuckets.End)
                    {
                        break;
                    }
                    if (operationCapacity.StartDate.Ticks >= peopleUtilizationBuckets.Start.Ticks && FilterUsage(a_usageType, operationUsageEnum))
                    {
                        blockPeopleProfiles.Add(blockProfileIdx, (operationUsageEnum, operationCapacity, block));
                        blockProfileIdx++;
                    }
                }
            }
            node = node.Next;
        }

        //Loop through all the capacity buckets created
        for (int i = 0; i < peopleUtilizationBuckets.Count; i++)
        {
            DateTime rangeEnd = new DateTime(time.Ticks + a_bucketLength.Ticks);
            //Gets all the online or overtime capacity intervals within the start and end dates
            foreach (ResourceCapacityInterval resourceCapacityInterval in capacityProfile.ProfileIntervals.FindAllActiveIntervalWithinDateRange(time, rangeEnd))
            {
                //Loop through the collection to use end date of the operation and it's manufacturing step at the time
                if (blockPeopleProfiles.Count == 0)
                {
                    continue;
                }
                foreach ((int idx, (MainResourceDefs.usageEnum usage, OperationCapacity operationCapacity, ResourceBlock resourceBlock)) in blockPeopleProfiles)
                {
                    ResourceBlock nbrOfPeopleBlock = resourceBlock;
                    if (resourceCapacityInterval.StartDateTime <= operationCapacity.StartDate.ToDateTime() && resourceCapacityInterval.EndDateTime >= operationCapacity.EndDate.ToDateTime() && resourceCapacityInterval.CanScheduleProductionUsage(usage))
                    {
                        //Since setup is always performed by one nbrPeople regardless of the specified people usage we use 1 in that case
                        decimal nbrPeopleScaledByAttentionPctForBlock = usage == MainResourceDefs.usageEnum.Setup ? 1 : GetNbrPeopleScaledByAttentionPctForBlock(nbrOfPeopleBlock, resourceCapacityInterval.NbrOfPeople);
                        decimal operationCapacityTotalCapacityTicks = (operationCapacity.TotalCapacityTicks / operationCapacity.CapacityRatio);

                        // Convert scaled capacity ticks by nbrPeopleScaledByAttentionPctForBlock into a TimeSpan representing the capacity used during that bucket
                        TimeSpan timeSpan = new TimeSpan((long)(operationCapacityTotalCapacityTicks * nbrPeopleScaledByAttentionPctForBlock));
                        peopleUtilizationBuckets[i] += timeSpan;
                    }
                }
            }
            time = rangeEnd;
        }
        return peopleUtilizationBuckets;
    }

    internal decimal GetNbrPeopleScaledByAttentionPctForBlock(ResourceBlock a_block, decimal a_capacityIntervalNbrPeople)
    {
        if (a_block == null)
        {
            throw new ArgumentNullException(nameof(a_block));
        }

        decimal nbrPeople;
        if (a_block.Batch.FirstActivity.PeopleUsage == InternalActivityDefs.peopleUsages.UseAllAvailable)
        {
            nbrPeople = a_capacityIntervalNbrPeople;
        }
        else if (a_block.Batch.FirstActivity.PeopleUsage == InternalActivityDefs.peopleUsages.UseMultipleOfSpecifiedNbr)
        {
            nbrPeople = Math.Floor(a_capacityIntervalNbrPeople / a_block.Batch.FirstActivity.NbrOfPeople) * a_block.Batch.FirstActivity.NbrOfPeople;
        }
        else //use specified
        {
            nbrPeople = a_block.Batch.FirstActivity.NbrOfPeople;
        }

        return nbrPeople * (a_block.SatisfiedRequirement.AttentionPercent / 100);
    }
    #endregion

    #region Campaigns
    public List<Campaign> GetCampaigns(DateTime a_dateLimit)
    {
        List<Campaign> campaigns = new ();
        ResourceBlockList.Node node = Blocks.First;
        List<DateTime> cleanoutDates = new ();
        //Find all planned cleanouts
        for (int i = 0; i < ResourceCapacityIntervals.Count; i++)
        {
            if (ResourceCapacityIntervals[i] != null && ResourceCapacityIntervals[i].ClearChangeovers)
            {
                cleanoutDates.Add(ResourceCapacityIntervals[i].StartDateTime);
            }
        }

        Campaign campaign = null;
        while (node != null)
        {
            if (node.Data.StartDateTime > a_dateLimit)
            {
                break;
            }

            string moProduct = node.Data.Batch.FirstActivity.Operation.ManufacturingOrder.ProductName;
            if (string.IsNullOrEmpty(moProduct))
            {
                campaign = null;
                node = node.Next;
                continue;
            }

            if (campaign == null || moProduct != campaign.Description)
            {
                //Start a new campaign
                campaign = GetNewCampaign(node.Data);
                campaigns.Add(campaign);
            }
            else
            {
                //add to the previous campaign
                campaign.Add(node.Data);
            }

            if (node.next == null)
            {
                break;
            }

            //Check if a cleanout would break the campaign
            //Note, it might be worth removing items from the list if they are in the past
            foreach (DateTime cleanoutDate in cleanoutDates)
            {
                if (cleanoutDate >= node.Data.EndDateTime && cleanoutDate <= node.next.Data.StartDateTime)
                {
                    campaign = null;
                }
            }

            node = node.Next;
        }

        return campaigns;
    }

    private static Campaign GetNewCampaign(ResourceBlock a_block)
    {
        Campaign campaign = new (a_block);
        campaign.FillColor = a_block.Batch.FirstActivity.Operation.ManufacturingOrder.ProductColor;
        return campaign;
    }
    #endregion Campaigns

    /// <summary>
    /// Returns the DateTime of the first non-offline CI or RCI.
    /// </summary>
    /// <param name="pt"></param>
    /// <returns></returns>
    public DateTime FindFirstOnlineIntervalStartOnOrAfterPoint(DateTime a_pt)
    {
        DateTime firstRciOnlineStart = RecurringCapacityIntervals.FindFirstOnlineAtOrAfterPointInTime(a_pt);
        DateTime firstCiOnlineStart = CapacityIntervals.FindOnlineAtOrAfterPointInTime(a_pt);
        return new DateTime(Math.Min(firstRciOnlineStart.Ticks, firstCiOnlineStart.Ticks));
    }

    /// <summary>
    /// Finds the start and end date/time of Online Capacity Interval covering the specified point.
    /// </summary>
    /// <param name="shiftStart">Start of shift.</param>
    /// <param name="shiftEnd">End of shift</param>
    /// <returns>True if an Online interval is found covering the point.  False otherwise.</returns>
    public bool FindOnlineInvervalCoveringPoint(DateTime a_point,
                                                out DateTime o_shiftStart,
                                                out DateTime o_shiftEnd,
                                                out string o_shiftName,
                                                out string o_shiftDescription,
                                                out decimal o_shiftNbrOfPeople,
                                                out string o_shiftType)
    {
        if (RecurringCapacityIntervals.FindExpansionCoveringPoint(a_point, out RecurringCapacityInterval rci, out RecurringCapacityInterval.RCIExpansion rciExpansion))
        {
            o_shiftStart = rciExpansion.Start;
            o_shiftEnd = rciExpansion.End;
            o_shiftName = rci.Name;
            o_shiftDescription = rci.Description;
            o_shiftNbrOfPeople = rci.NbrOfPeople;
            o_shiftType = GetShiftTypeTextFromCapacityInterval(rci);
            return true;
        }

        if (CapacityIntervals.FindOnlineIntervalCoveringPoint(a_point, out CapacityInterval ci))
        {
            o_shiftStart = ci.StartDateTime;
            o_shiftEnd = ci.EndDateTime;
            o_shiftName = ci.Name;
            o_shiftDescription = ci.Description;
            o_shiftNbrOfPeople = ci.NbrOfPeople;
            o_shiftType = GetShiftTypeTextFromCapacityInterval(ci);
            return true;
        }

        o_shiftStart = PTDateTime.MaxDateTime;
        o_shiftEnd = PTDateTime.MaxDateTime;
        o_shiftName = null;
        o_shiftDescription = null;
        o_shiftNbrOfPeople = -1;
        o_shiftType = "Normal Online";
        return false;
    }

    /// <summary>
    /// This function is used when publishing to classify a capacity interval as some type of shift.
    /// SQL only takes in string/varchar in this case so this function generates a string based on
    /// properties of the capacity interval that is passed in. 
    /// </summary>
    /// <param name="a_capacityInterval">The capacity interval used to determine the shift type string</param>
    /// <returns></returns>
    static string GetShiftTypeTextFromCapacityInterval(CapacityInterval a_capacityInterval)
    {
        // There's most likely other shift types that we should add
        if (a_capacityInterval.Active)
        {
            if (a_capacityInterval.UseOnlyWhenLate && a_capacityInterval.Overtime)
            {
                return "Potential Overtime";
            }

            if (a_capacityInterval.Overtime)
            {
                return "Overtime";
            }

            if (a_capacityInterval.UsedForClean && !a_capacityInterval.UsedForRun)
            {
                return "Cleanout";
            }

            return "Normal Online";
        }
        if (!a_capacityInterval.Active)
        {
            if (a_capacityInterval.CleanOutSetups && a_capacityInterval.PreventOperationsFromSpanning)
            {
                return "Maintenance";
            }
            if (a_capacityInterval.PreventOperationsFromSpanning)
            {
                return "Holiday";
            }

            return "Offline";
        }

        return "";
    }

    internal BlockCapacityIntervalDetails GetScheduleDetailsByCapacityIntervalGroupSetupProcessingAndPostProcessing(ResourceBlock a_rb)
    {
        List<BlockIntervalTypeCapacityIntervalDetails> scheduleDetailsList = new ();

        GetSetupByCapacityInterval(a_rb, scheduleDetailsList);
        GetProductionByCapacityInterval(a_rb, scheduleDetailsList);
        GetPostProcessingByCapacityInterval(a_rb, scheduleDetailsList);

        return new BlockCapacityIntervalDetails(scheduleDetailsList);
    }

    private List<BlockIntervalTypeCapacityIntervalDetails> GetScheduleDetailsByCapacityInterval(ResourceBlock a_rb)
    {
        List<BlockIntervalTypeCapacityIntervalDetails> scheduleDetailsList = new ();

        GetSetupByCapacityInterval(a_rb, scheduleDetailsList);
        GetProductionByCapacityInterval(a_rb, scheduleDetailsList);
        GetPostProcessingByCapacityInterval(a_rb, scheduleDetailsList);

        return scheduleDetailsList;
    }

    private void GetSetupByCapacityInterval(ResourceBlock rb, List<BlockIntervalTypeCapacityIntervalDetails> a_scheduleDetailsList)
    {
        InternalActivity act = rb.Batch.FirstActivity;
        ResourceRequirement rr = act.Operation.ResourceRequirements.GetByIndex(rb.ResourceRequirementIndex);
        if (rr.ContainsUsage(MainResourceDefs.usageEnum.Setup))
        {
            GetScheduleDetailsByCapacityInterval(rb.StartTicks, act.GetScheduledEndOfSetupTicks(), BlockIntervalType.Setup, a_scheduleDetailsList);
        }
    }

    private void GetPostProcessingByCapacityInterval(ResourceBlock rb, List<BlockIntervalTypeCapacityIntervalDetails> a_scheduleDetailsList)
    {
        InternalActivity act = rb.Batch.FirstActivity;
        ResourceRequirement rr = act.Operation.ResourceRequirements.GetByIndex(rb.ResourceRequirementIndex);
        if (rr.ContainsUsage(MainResourceDefs.usageEnum.PostProcessing))
        {
            GetScheduleDetailsByCapacityInterval(act.ScheduledEndOfRunTicks, rb.EndTicks, BlockIntervalType.PostProcessing, a_scheduleDetailsList);
        }
    }

    /// <summary>
    /// For each capacity interval, return the start, end, and quantity completed. Originally done to help with Maximus's bucketing by day problems.
    /// </summary>
    private void GetScheduleDetailsByCapacityInterval(long a_start, long a_end, BlockIntervalType a_intervalType, List<BlockIntervalTypeCapacityIntervalDetails> a_scheduleDetailsList)
    {
        if (a_end > a_start && capacityProfile.ProfileIntervals.Count > 0)
        {
            int startIdx = capacityProfile.ProfileIntervals.FindIdx(a_start);
            ResourceCapacityIntervalsCollection rcic = capacityProfile.ProfileIntervals;

            for (int i = startIdx; i < rcic.Count; ++i)
            {
                ResourceCapacityInterval rci = rcic[i];

                if (rci.Active)
                {
                    long capacityStart = Math.Max(a_start, rci.StartDate);
                    long capacityEnd = Math.Min(a_end, rci.EndDate);

                    //If this is a tank resource, post processing could go beyond a rci, so it should stop now since all of the last rci was counted.
                    if (rci.StartDate > a_end && IsTank)
                    {
                        break;
                    }

#if DEBUG
                    if (rci.StartDate > a_end)
                    {
                        throw new Exception("The search for RCIs has gone too far.");
                    }
#endif

                    BlockIntervalTypeCapacityIntervalDetails scheduleDetails = new (a_intervalType, i, capacityStart, capacityEnd, 0);
                    a_scheduleDetailsList.Add(scheduleDetails);

                    ResourceCapacityInterval.ContainmentType epct = rci.ContainsEndPoint(a_end);
                    if (epct == ResourceCapacityInterval.ContainmentType.Contains)
                    {
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// For each capacity interval, return the start, end, and quantity completed. Originally done to help with Maximus's bucketing by day problems.
    /// </summary>
    private void GetProductionByCapacityInterval(ResourceBlock rb, List<BlockIntervalTypeCapacityIntervalDetails> a_scheduleDetailsList)
    {
        InternalActivity act = rb.Batch.FirstActivity;
        ResourceRequirement rr = act.Operation.ResourceRequirements.GetByIndex(rb.ResourceRequirementIndex);
        if (!rr.ContainsUsage(MainResourceDefs.usageEnum.Run))
        {
            //No Production on this block
            return;
        }

        long runEnd = rb.Batch.ProcessingEndTicks;
        long runStart = runEnd - (rb.Batch.ProcessingEndTicks - rb.Batch.SetupEndTicks);

        if (runStart < runEnd)
        {
            int startIdx = capacityProfile.ProfileIntervals.FindIdx(runStart);

            if (startIdx >= 0)
            {
                RequiredSpan processingSpan;
                long requiredNumberOfCycles;
                decimal requiredFinishQty;
                rb.Batch.FirstActivity.CalculateProcessingTimeSpanOnResource(this, out processingSpan, out requiredNumberOfCycles, out requiredFinishQty);

                if (processingSpan.TimeSpanTicks == 0)
                {
                    return;
                }

                long adjustedCycleSpan;
                if (processingSpan.Overrun)
                {
                    adjustedCycleSpan = 0;
                }
                else
                {
                    adjustedCycleSpan = processingSpan.TimeSpanTicks / requiredNumberOfCycles;
                }

                ResourceCapacityIntervalsCollection rcic = capacityProfile.ProfileIntervals;
                decimal capacityRemaining = 0;
                decimal remainingQty = requiredFinishQty;

                ResourceOperation ro = rb.Batch.FirstActivity.Operation as ResourceOperation;

                for (int i = startIdx; i < rcic.Count; ++i)
                {
                    ResourceCapacityInterval rci = rcic[i];
                    decimal cycles = 0;
                    if (rci.Active)
                    {
                        long capacityStart = Math.Max(runStart, rci.StartDate);
                        long capacityEnd = Math.Min(runEnd, rci.EndDate);

                        InternalActivity activity = rb.Batch.FirstActivity;
                        capacityRemaining += CalculateCapacity(capacityStart, capacityEnd, activity.PeopleUsage, activity.NbrOfPeople, rci, out bool capacityAdjusted, out decimal _);
                        if (adjustedCycleSpan != 0)
                        {
                            if (i == startIdx && ro.TimeBasedReporting)
                            {
                                capacityRemaining += activity.ReportedRun % adjustedCycleSpan;
                            }

                            cycles = capacityRemaining / adjustedCycleSpan;
                            //  decimal cycles = Math.Truncate(capacityRemaining / adjustedCycleSpan);  JMC changed to use partial cycles for Maximus.
                            capacityRemaining %= adjustedCycleSpan;
                        }

                        decimal qty = InternalOperation.PlanningScrapPercentAdjustedQty(activity.GetResourceProductionInfo(rb.ScheduledResource).PlanningScrapPercent, activity.GetResourceProductionInfo(rb.ScheduledResource).QtyPerCycle * cycles);

                        if (qty > remainingQty)
                        {
                            qty = remainingQty;
                            remainingQty = 0;
                        }
                        else
                        {
                            remainingQty -= qty;
                        }

                        if (ro.WholeNumberSplits)
                        {
                            qty = Math.Round(qty, 0, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            qty = Math.Round(qty, 2, MidpointRounding.AwayFromZero); //otherwise looks bad. TODO make customizable.
                        }

                        BlockIntervalTypeCapacityIntervalDetails scheduleDetails = new (BlockIntervalType.Processing, i, capacityStart, capacityEnd, qty);
                        a_scheduleDetailsList.Add(scheduleDetails);

                        ResourceCapacityInterval.ContainmentType epct = rci.ContainsEndPoint(runEnd);
                        if (epct == ResourceCapacityInterval.ContainmentType.Contains)
                        {
                            break;
                        }
                    }

#if DEBUG
                    if (rci.StartDate > runEnd)
                    {
                        throw new Exception("The search for RCIs has gone too far.");
                    }
#endif
                }
            }
        }
    }

    /// <summary>
    /// Calculates whether at least one activity is eligible based on RequiredFinishQty
    /// </summary>
    /// <param name="a_op"></param>
    public bool IsCapableBasedOnMinMaxQtys(InternalOperation a_op, ProductRuleManager a_prs)
    {
        if (a_op.Split)
        {
            foreach (InternalActivity activity in a_op.Activities)
            {
                if (IsCapableBasedOnMinMaxQtys(activity, a_op.AutoSplitInfo, a_prs))
                {
                    //Only one activity needs to be eligible.
                    return true;
                }
            }

            //No activities were eligible
            return false;
        }

        return IsCapableBasedOnMinMaxQtys(a_op.Activities.GetByIndex(0), a_op.AutoSplitInfo, a_prs);
    }

    /// <summary>
    /// Calculates whether the activity is eligible based on RequiredFinishQty
    /// </summary>
    /// <param name="a_act"></param>
    /// <param name="a_opAutoSplitInfo"></param>
    public bool IsCapableBasedOnMinMaxQtys(InternalActivity a_act, AutoSplitInfo a_opAutoSplitInfo, ProductRuleManager a_prs)
    {
        //When split type is manual, allow to override constraints so users can manually split the operation once scheduled
        if (a_opAutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.Manual)
        {
            return true;
        }

        decimal minQty = MinQty;
        bool minQtyConstrained = MinQuantityConstrained;

        decimal maxQty = MaxQty;
        bool maxQtyConstrained = MaxQuantityConstrained;

        //Check for Product Rule override
        if (a_act.Operation.Products.PrimaryProduct is Product primaryProduct)
        {
            ProductRule productRule = a_prs.GetProductRule(Id, primaryProduct.Item.Id, a_act.Operation.ProductCode);
            if (productRule != null)
            {
                if (productRule.UseMinQty)
                {
                    minQty = productRule.MinQty;
                    minQtyConstrained = productRule.MinQty != 0m && productRule.MinQty != decimal.MinValue;
                }

                if (productRule.UseMaxQty)
                {
                    maxQty = productRule.MaxQty;
                    maxQtyConstrained = productRule.MaxQty != 0m && productRule.MaxQty != decimal.MaxValue;
                }
            }
        }

        //Check if resources are actually constrained by Max Qty
        if (!minQtyConstrained && !maxQtyConstrained)
        {
            return true;
        }

        if (minQtyConstrained && a_act.RequiredFinishQty < minQty)
        {
            //Can't scheduler here as the Resource Min Qty exceeds the activity qty
            return false;
        }

        if (!maxQtyConstrained || a_act.RequiredFinishQty <= maxQty)
        {
            return true;
        }

        //Check whether this operation could be autosplit and still be eligible
        if (a_opAutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.ResourceQtyCapacity)
        {
            return ValidateMinMaxQtyAutoSplit(a_opAutoSplitInfo, minQty, maxQty);
        }

        if (a_opAutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.PredecessorMaterials 
            || a_opAutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.PredecessorQuantityRatio)
        {
            if (a_act.Operation.Predecessors.Count == 0)
            {
                return false;
            }

            ResourceOperation predOp = a_act.Operation.Predecessors[0].Predecessor.Operation;
            if (predOp.AutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.None)
            {
                return false;
            }

            //Pred op can split, allow this op to at least attempt to schedule
            return true;
        }

        //No activities were eligible
        return false;
    }

    private bool ValidateMinMaxQtyAutoSplit(AutoSplitInfo a_autoSplitInfo, decimal a_minQty, decimal a_maxQty)
    {
        if (a_autoSplitInfo.UseMinAutoSplitAmount && a_autoSplitInfo.MinAutoSplitAmount > a_maxQty)
        {
            return false;
        }

        if (a_autoSplitInfo.UseMaxAutoSplitAmount && a_autoSplitInfo.MaxAutoSplitAmount < a_minQty)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// [Bool] returns if all operation attribute numbers are within the resources attribute ranges
    /// </summary>
    /// <param name="a_op">Operation</param>
    /// <param name="a_exception">Optional exception used during move validation to provide more detailed messages</param>
    /// <returns></returns>
    public bool IsCapableBasedOnAttributeNumberRange(InternalOperation a_op)
    {
        if (FromToRanges != null)
        {
            for (int attI = 0; attI < a_op.Attributes.Count; attI++)
            {
                OperationAttribute opAttribute = a_op.Attributes[attI];
                RangeLookup.FromRangeSet fromRangeSet = FromToRanges.Find(opAttribute.PTAttribute.Name);
                if (fromRangeSet != null)
                {
                    if (fromRangeSet.EligibilityConstraint)
                    {
                        RangeLookup.FromRange fromRange = fromRangeSet.Find(opAttribute.Number);
                        if (fromRange == null)
                        {
                            return false; //Attribute is listed for the Resource but the Operations's Attribute Number is not in the valid ranges.
                        }
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the total cost of cleans scheduled on the resource
    /// by multiplying the StandardHourCost by the total clean hours
    /// </summary>
    /// <returns></returns>
    public decimal GetCleanCosts()
    {
        if (Blocks.Count == 0)
        {
            return 0;
        }

        ResourceBlockList.Node node = Blocks.First;
        decimal cleanHrs = 0;
        while (node != null)
        {
            ResourceBlock block = node.Data;
            cleanHrs += (decimal)(block.Batch.FirstActivity.ScheduledCleanSpan.TotalHours + block.Batch.FirstActivity.ReportedClean);
            node = node.Next;
        }

        return StandardHourlyCost * cleanHrs;
    }

    #region Attribute Dipatch Helpers
    //TODO: MOVE TO Resource in the Simulation project with the Setup Number/Code stuff.
    #endregion Attribute Dipatch Helpers

    protected override bool IsActiveSettableToFalse(out string o_msg)
    {
        if (Blocks.Count > 0)
        {
            const int c_maxJobsToAppendToMsg = 25;

            System.Text.StringBuilder sb = new();

            sb.AppendFormat("Resource {0} can't be made inactive because it still has {1} block(s) scheduled on it. These blocks need to be transferred to other resources or unscheduled prior to making the resource inactive.".Localize(), ToString(), Blocks.Count);
            sb.AppendLine();

            if (Blocks.Count > c_maxJobsToAppendToMsg)
            {
                sb.AppendLine("Some of the jobs scheduled on this resource include:".Localize());
            }
            else
            {
                sb.AppendLine("The Jobs scheduled on this resource include:".Localize());
            }

            int maxAppendCount = Math.Min(c_maxJobsToAppendToMsg, Blocks.Count);
            ResourceBlockList.Node currentBlockNode = Blocks.First;
            HashSet<string> jobsAppended = new ();

            while (currentBlockNode != null)
            {
                Batch batch = currentBlockNode.Data.Batch;
                IEnumerator<InternalActivity> activitiesEtr = batch.GetEnumerator();

                while (activitiesEtr.MoveNext())
                {
                    string jobString = activitiesEtr.Current.Job.ToString();
                    if (!jobsAppended.Contains(jobString))
                    {
                        jobsAppended.Add(jobString);
                        sb.AppendFormat("{0}; ", jobString);
                        sb.AppendLine();

                        if (jobsAppended.Count == maxAppendCount)
                        {
                            goto AppendJobExit;
                        }
                    }
                }

                currentBlockNode = currentBlockNode.Next;
            }

        AppendJobExit:

            o_msg = sb.ToString();
            return false;
        }

        o_msg = null;
        return true;
    }

    #region Server Checkpoint Values
    /// <summary>
    /// Calculate checksums used to help validate whether the schedule on a client is the same as on the server.
    /// </summary>
    /// <param name="aStartAndEndSums">The sum of all the start and stop times for all the blocks.</param>
    /// <param name="aResourceJobOperationCombos">For every block the sum of resource*job*operation. The values of the three objects are based on their Ids.</param>
    /// <param name="aBlockCount"></param>
    internal void CalculateChecksums(bool a_checksumDiagnosticsEnabled, out long o_startAndEndSums, out long o_resourceJobOperationCombos, out int o_blockCount, System.Text.StringBuilder a_sbDescription)
    {
        o_startAndEndSums = 0;
        o_resourceJobOperationCombos = 0;
        o_blockCount = m_blocks.Count;
        long resourceIdValue = Id.Value;

        ResourceBlockList.Node currentNode = m_blocks.First;

        while (currentNode != null)
        {
            ResourceBlock rb = currentNode.Data;
            o_startAndEndSums += rb.StartTicks;
            o_startAndEndSums += rb.EndTicks;

            o_resourceJobOperationCombos += resourceIdValue * rb.Activity.Operation.ManufacturingOrder.Job.Id.Value * rb.Activity.Operation.Id.Value;

            InternalActivity act = rb.Batch.FirstActivity;
            CultureInfo enUsCultureInfo = new CultureInfo("en-US");

            if (a_checksumDiagnosticsEnabled)
            {
                a_sbDescription.Append($"Resource ExternalId: {ExternalId} | ");
                a_sbDescription.Append($"Job ExternalId: {act.Operation.Job.ExternalId.PadLeft(30)} | ");
                a_sbDescription.Append($"MO ExternalId: {act.Operation.ManufacturingOrder.ExternalId.PadLeft(30)} | ");
                a_sbDescription.Append($"Operation ExternalId:{act.Operation.ExternalId.PadLeft(30)} | ");
                a_sbDescription.Append($"Activity ExternalId: {act.ExternalId.PadLeft(30)} | ");
                a_sbDescription.Append($"StartDateTime: {rb.Batch.StartDateTime.ToStringUniversalFormatUsCulture().PadLeft(30)} | ");
                a_sbDescription.Append($"RunStartDateTime: {rb.Batch.SetupFinishedDateTime.ToStringUniversalFormatUsCulture().PadLeft(30)} | ");
                a_sbDescription.Append($"PostProcessingStartDate: {rb.Batch.ProcessingEndDateTime.ToStringUniversalFormatUsCulture().PadLeft(30)} | ");
                a_sbDescription.Append($"StorageStartDate: {rb.Batch.PostProcessingEndDateTime.ToStringUniversalFormatUsCulture().PadLeft(30)} | ");
                a_sbDescription.Append($"Batch Clean Start: {rb.Batch.PostProcessingEndDateTime.ToStringUniversalFormatUsCulture().PadLeft(30)} | ");
                a_sbDescription.Append($"EndDateTime: {rb.Batch.EndDateTime.ToStringUniversalFormatUsCulture().PadLeft(30)} | ");
                a_sbDescription.Append($"RequiredFinishedQty: {act.RequiredFinishQty.ToString(enUsCultureInfo).PadLeft(30)}");
                a_sbDescription.Append(Environment.NewLine);
            }

            currentNode = currentNode.Next;
        }
    }
    #endregion

    /// <summary>
    /// Capacity information in bucket form for analysis
    /// </summary>
    /// <param name="start"></param>
    /// <param name="endInclusive"></param>
    /// <param name="bucketLength"></param>
    /// <param name="chooser"></param>
    /// <returns></returns>
    public ResourceCapacityInfo GetCapacityInfo(DateTime start, DateTime endInclusive, TimeSpan bucketLength, CapacityInfoBase.GroupChooser chooser, MainResourceDefs.usageEnum a_usage)
    {
        return new ResourceCapacityInfo(start, endInclusive, bucketLength, this, chooser, a_usage);
    }

    internal bool IsThereGapOfOnlineTimeBetweenEndDateAndStartDateOfSuccessiveActivities(long a_previousActEndTicks, long a_nextActStartDate)
    {
        return ResultantCapacity.IsThereGapOfOnlineTimeBetweenEndDateAndStartDateOfSuccessiveActivities(a_previousActEndTicks, a_nextActStartDate);
    }

    internal void DeletingStorageArea()
    {
        //The storage area that links to this resource is being deleted, so reset the flag
        StorageAreaResource = false;
    }
}