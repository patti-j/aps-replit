using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Contains data to be used for comparing scenarios at different points in time to analyze the impact of changes.
/// </summary>
[Serializable]
public class JobInfo : IPTSerializable
{
    public const int UNIQUE_ID = 348;

    #region IPTSerializable Members
    public JobInfo(IReader reader)
    {
        if (reader.VersionNumber >= 647)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out m_lateness);

            m_id = new BaseId(reader);
        }
        else if (reader.VersionNumber >= 491)
        {
            string depricatedString;
            int depricatedint;
            DateTime depricatedDateTime;
            m_bools = new BoolVector32(reader);
            reader.Read(out depricatedString);
            reader.Read(out depricatedString);
            reader.Read(out depricatedint);
            reader.Read(out depricatedint);

            reader.Read(out depricatedDateTime);
            reader.Read(out depricatedDateTime);
            reader.Read(out m_lateness);

            m_id = new BaseId(reader);
        }
        else if (reader.VersionNumber >= 1)
        {
            string depricatedString;
            int depricatedint;
            DateTime depricatedDateTime;
            reader.Read(out depricatedString);
            reader.Read(out depricatedString);
            reader.Read(out depricatedint);
            bool temp;
            reader.Read(out temp);
            m_bools[c_depricatedHotIdx] = temp;
            reader.Read(out temp);
            m_bools[c_tardyIdx] = temp;
            reader.Read(out depricatedint);

            reader.Read(out depricatedDateTime);
            reader.Read(out depricatedDateTime);
            reader.Read(out m_lateness);

            m_id = new BaseId(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        m_bools.Serialize(writer);
        writer.Write(m_lateness);

        m_id.Serialize(writer);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public JobInfo(Job a_job, ScenarioDetail a_sd)
    {
        m_id = a_job.Id;
        Late = a_job.Late;
        m_lateness = a_job.Late ? a_job.Lateness : -a_job.Earliness;
        Scheduled = a_job.Scheduled;
        m_materialShortageList.AddShortagesForJob(a_job, a_sd);
        ScheduledInFrozenSpan = IsScheduledInFrozenSpan(a_job, a_sd);
    }

    private static bool IsScheduledInFrozenSpan(Job a_job, ScenarioDetail a_sd)
    {
        if (a_job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
        {
            //Check department specific frozen spans.
            List<BaseOperation> operations = a_job.GetOperations();
            foreach (BaseOperation operation in operations)
            {
                if (operation.Scheduled)
                {
                    DateTime startDate = operation.ScheduledStartDate;
                    List<InternalResource> scheduledResources = (operation as InternalOperation).GetResourcesScheduled();
                    foreach (InternalResource scheduledResource in scheduledResources)
                    {
                        if (a_sd.ClockDate + scheduledResource.Department.FrozenSpan > startDate)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    #region Properties
    private BoolVector32 m_bools;

    private const short c_depricatedHotIdx = 0;
    private const short c_tardyIdx = 1;
    private const short c_unscheduledIdx = 2;
    private const short c_scheduledInFrozenSpanIdx = 3;

    private BaseId m_id;

    /// <summary>
    /// Job Id
    /// </summary>
    public BaseId Id => m_id;

    /// <summary>
    /// True if the Job ends after its NeedDateTime.
    /// </summary>
    public bool Late
    {
        get => m_bools[c_tardyIdx];
        private set => m_bools[c_tardyIdx] = value;
    }

    private readonly TimeSpan m_lateness;

    /// <summary>
    /// The ScheduledEndDateTime minus the NeedDateTime.
    /// </summary>
    public TimeSpan Lateness => m_lateness;

    /// <summary>
    /// Indicates the likelihood that the work will be executed.	For display and custom Algorithms.
    /// </summary>
    public bool Scheduled
    {
        get => m_bools[c_unscheduledIdx];
        private set => m_bools[c_unscheduledIdx] = value;
    }

    public bool ScheduledInFrozenSpan
    {
        get => m_bools[c_scheduledInFrozenSpanIdx];
        private set => m_bools[c_scheduledInFrozenSpanIdx] = value;
    }
    #endregion Properties

    /// <summary>
    /// Returns true if one or more of the values differ between the JobInfo and the specified JobInfo.
    /// </summary>
    public bool Changed(JobInfo a_compareTo)
    {
        return Late != a_compareTo.Late || Lateness != a_compareTo.Lateness || !MaterialShortages.Equals(a_compareTo.MaterialShortages);
    }

    private readonly MaterialShortageList m_materialShortageList = new ();

    /// <summary>
    /// The list of Material Shortages for this Job.
    /// </summary>
    public MaterialShortageList MaterialShortages => m_materialShortageList;

    /// <summary>
    /// Stores a list of Materials Shortages for a Job and provides methods for comparing them.
    /// </summary>
    public class MaterialShortageList
    {
        private readonly Dictionary<BaseId, MaterialShortage> _materialShortages = new ();

        public void AddShortagesForJob(Job job, ScenarioDetail sd)
        {
            List<MaterialRequirement.MaterialShortage> materialShortages = job.GetMaterialShortages(sd);
            for (int msI = 0; msI < materialShortages.Count; msI++)
            {
                MaterialRequirement.MaterialShortage shortage = materialShortages[msI];
                if (!_materialShortages.ContainsKey(shortage.Material.Id))
                {
                    _materialShortages.Add(shortage.Material.Id, new MaterialShortage(shortage));
                }
            }
        }

        private bool Contains(MaterialShortage aShortage)
        {
            return _materialShortages.ContainsKey(aShortage.MaterialRequirementId);
        }

        private MaterialShortage this[BaseId aMaterialRequirementId] => _materialShortages[aMaterialRequirementId];

        public int Count => _materialShortages.Count;

        public Dictionary<BaseId, MaterialShortage>.Enumerator GetEnumerator()
        {
            return _materialShortages.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MaterialShortageList))
            {
                return false;
            }

            MaterialShortageList newList = (MaterialShortageList)obj;
            return GetNewShortages(newList).Count == 0 && GetDeletedShortages(newList).Count == 0 && GetShortagesCleared(this, newList).Count == 0;
        }

        /// <summary>
        /// Returns a list of Shortages that are in the aUpdatedShortages but not in the current list it is being compared to.
        /// </summary>
        public List<MaterialShortage> GetNewShortages(MaterialShortageList aUpdatedShortages)
        {
            return GetShortagesCleared(aUpdatedShortages, this);
        }

        /// <summary>
        /// Returns a list of Shortages that are in the current list being compared to but not in aUpdatedShortages.
        /// </summary>
        public List<MaterialShortage> GetDeletedShortages(MaterialShortageList aUpdatedShortages)
        {
            return GetShortagesCleared(this, aUpdatedShortages);
        }

        /// <summary>
        /// Returns a list of Shortages that are in aShortageListA but not in aShortageListB.
        /// </summary>
        public List<MaterialShortage> GetShortagesCleared(MaterialShortageList aShortageListA, MaterialShortageList aShortageListB)
        {
            List<MaterialShortage> inAbutNotB = new ();

            Dictionary<BaseId, MaterialShortage>.Enumerator inAShortagesEnumerator = aShortageListA.GetEnumerator();
            while (inAShortagesEnumerator.MoveNext())
            {
                MaterialShortage shortageA = inAShortagesEnumerator.Current.Value;
                if (!aShortageListB.Contains(shortageA))
                {
                    inAbutNotB.Add(shortageA);
                }
            }

            return inAbutNotB;
        }

        /// <summary>
        /// Returns a list of differences between matching Shortages.
        /// </summary>
        public List<MaterialShortageDiff> GetShortagesDiffs(MaterialShortageList aShortageListInitial, MaterialShortageList aShortageListUpdated)
        {
            List<MaterialShortageDiff> diffs = new ();

            Dictionary<BaseId, MaterialShortage>.Enumerator inAShortagesEnumerator = aShortageListInitial.GetEnumerator();
            while (inAShortagesEnumerator.MoveNext())
            {
                MaterialShortage shortageA = inAShortagesEnumerator.Current.Value;
                if (aShortageListUpdated.Contains(shortageA))
                {
                    MaterialShortage shortageB = aShortageListUpdated[shortageA.MaterialRequirementId];
                    diffs.Add(new MaterialShortageDiff(shortageA, shortageB));
                }
            }

            return diffs;
        }
    }

    /// <summary>
    /// A material requirement that is not currently satisfied.
    /// </summary>
    public class MaterialShortage
    {
        public MaterialShortage(MaterialRequirement.MaterialShortage aShortage)
        {
            _jobId = aShortage.Operation.ManufacturingOrder.Job.Id;
            _mrId = aShortage.Material.Id;
            _materialName = aShortage.Material.MaterialName;
            _moName = aShortage.Operation.ManufacturingOrder.Name;
            _opName = aShortage.Operation.Name;
            _totalRequiredQty = aShortage.Material.UnConsumedIssuedQty;
            _qtyPlanned = aShortage.QtyPlanned;
            _jitStartDate = aShortage.Operation.DbrJitStartDate;
            _scheduledUsageDate = aShortage.Operation.StartDateTime;
            _earliestDelivery = aShortage.EarliestDelivery;
        }

        #region Properties
        private readonly BaseId _jobId;

        /// <summary>
        /// The Id of the Job for the Material Requirement for which the shortage exists.
        /// </summary>
        public BaseId JobId => _jobId;

        private readonly BaseId _mrId;

        /// <summary>
        /// The Id of the Material Requirement for which the shortage exists.
        /// </summary>
        public BaseId MaterialRequirementId => _mrId;

        private readonly string _materialName;

        /// <summary>
        /// The material that is short.
        /// </summary>
        public string MaterialName => _materialName;

        private readonly string _moName;

        /// <summary>
        /// The Name of the Manufacturing Order to consume the material.
        /// </summary>
        public string ManufacturingOrderName => _moName;

        private readonly string _opName;

        /// <summary>
        /// The Name of the Operation to consume the material.
        /// </summary>
        public string OperationName => _opName;

        private readonly decimal _totalRequiredQty;

        /// <summary>
        /// The total quantity that is needed for the Item for a single Material Requirement.
        /// </summary>
        public decimal TotalRequiredQty => _totalRequiredQty;

        private readonly decimal _qtyPlanned;

        /// <summary>
        /// The quantity currently covered by On-Hand inventory or scheduled receipts.
        /// </summary>
        public decimal QtyPlanned => _qtyPlanned;

        /// <summary>
        /// The TotalQtyRequired minus the QtyPlanned.
        /// </summary>
        public decimal QtyShort => TotalRequiredQty - QtyPlanned;

        /// <summary>
        /// The earlier of the JIT Start Date and the Scheduled Usage Date.
        /// </summary>
        public DateTimeOffset NeedByDate => new (PTDateTime.Min(JITStartDate, ScheduledUsageDate).Ticks, TimeSpan.Zero);

        private readonly DateTime _jitStartDate;

        /// <summary>
        /// The JIT Start date of the Operation to consume the material.
        /// </summary>
        public DateTime JITStartDate => _jitStartDate;

        private readonly DateTime _scheduledUsageDate;

        /// <summary>
        /// The scheduled start date of the Operation to consume the material.
        /// </summary>
        public DateTime ScheduledUsageDate => _scheduledUsageDate;

        private readonly DateTime _earliestDelivery;

        /// <summary>
        /// /// If the Material Requirement must be satisfied from one Warehouse then this is based on the Warehouse's leadtime for the Item.  Otherwise it's based on the shortest lead time possible from any
        /// Warehouses that stock the Item.</param>
        /// </summary>
        public DateTime EarliestDelivery => _earliestDelivery;

        /// <summary>
        /// How late the material is expected to be based on when it's needed and when it can be available.
        /// The Earliest Delivery minus the Need by Date.
        /// </summary>
        /// <returns></returns>
        public TimeSpan ExpectedLateness => new (EarliestDelivery.Ticks - NeedByDate.Ticks);

        /// <summary>
        /// Whether the NeedBy date is before the Earliest Delivery date.
        /// </summary>
        public bool WithinLeadTime => NeedByDate.Ticks < EarliestDelivery.Ticks;
        #endregion Properties
    }

    /// <summary>
    /// Contains the differences between two Material Shortages.
    /// Compares: TotalRequiredQty, QtyPlanned, QtyShort, JITStartDate, ScheduledUsageQty, EarliestDelivery, WithinLeadTime, and ExpectedLateness.
    /// </summary>
    public class MaterialShortageDiff
    {
        internal MaterialShortageDiff(MaterialShortage aInitialShortage, MaterialShortage aNewShortage)
        {
            _jobId = aInitialShortage.JobId;
            _mrId = aInitialShortage.MaterialRequirementId;
            _materialName = aInitialShortage.MaterialName;
            _moName = aInitialShortage.ManufacturingOrderName;
            _opName = aInitialShortage.OperationName;

            _totalRequiredQtyChange = aNewShortage.TotalRequiredQty - aInitialShortage.TotalRequiredQty;
            _qtyPlannedChange = aNewShortage.QtyPlanned - aInitialShortage.QtyPlanned;
            _qtyShortChange = aNewShortage.TotalRequiredQty - aInitialShortage.TotalRequiredQty;
            _jitStartDateChange = aNewShortage.JITStartDate.Subtract(aInitialShortage.JITStartDate);
            _schedUsageDateChange = aNewShortage.ScheduledUsageDate.Subtract(aInitialShortage.ScheduledUsageDate);
            _earliestDeliveryChange = aNewShortage.EarliestDelivery.Subtract(aInitialShortage.EarliestDelivery);
            _movedWithinLeadTime = aNewShortage.WithinLeadTime && !aInitialShortage.WithinLeadTime;
            _movedOutsideLeadTime = !aNewShortage.WithinLeadTime && aInitialShortage.WithinLeadTime;
            _expectedLatenessChange = aNewShortage.ExpectedLateness.Subtract(aInitialShortage.ExpectedLateness);
        }

        private readonly BaseId _jobId;

        /// <summary>
        /// The Id of the Job for the Material Requirement for which the shortage exists.
        /// </summary>
        public BaseId JobId => _jobId;

        private readonly BaseId _mrId;

        /// <summary>
        /// The Id of the Material Requirement for which the shortage exists.
        /// </summary>
        public BaseId MaterialRequirementId => _mrId;

        private readonly string _materialName;

        /// <summary>
        /// The material that is short.
        /// </summary>
        public string MaterialName => _materialName;

        private readonly string _moName;

        /// <summary>
        /// The Name of the Manufacturing Order to consume the material.
        /// </summary>
        public string ManufacturingOrderName => _moName;

        private readonly string _opName;

        /// <summary>
        /// The Name of the Operation to consume the material.
        /// </summary>
        public string OperationName => _opName;

        private readonly decimal _totalRequiredQtyChange;

        public decimal TotalRequiredQtyChange => _totalRequiredQtyChange;

        private readonly decimal _qtyPlannedChange;

        public decimal QtyPlannedChange => _qtyPlannedChange;

        private readonly decimal _qtyShortChange;

        public decimal QtyShortChange => _qtyShortChange;

        private readonly TimeSpan _jitStartDateChange;

        public TimeSpan JITStartDateChange => _jitStartDateChange;

        private readonly TimeSpan _schedUsageDateChange;

        public TimeSpan ScheduledUsageDateChange => _schedUsageDateChange;

        private readonly TimeSpan _earliestDeliveryChange;

        public TimeSpan EarliestDeliveryChange => _earliestDeliveryChange;

        private readonly bool _movedWithinLeadTime;

        public bool MovedWithinLeadTime => _movedWithinLeadTime;

        private readonly bool _movedOutsideLeadTime;

        public bool MovedOutsideLeadTime => _movedOutsideLeadTime;

        private readonly TimeSpan _expectedLatenessChange;

        public TimeSpan ExpectedLatenessChange => _expectedLatenessChange;
    }
}

/// <summary>
/// Stores an ArrayList of JobInfos.
/// </summary>
[Serializable]
public class JobInfoList : ICopyTable, IPTSerializable
{
    public const int UNIQUE_ID = 347;

    #region IPTSerializable Members
    public JobInfoList(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out currentIndexer);
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                JobInfo j = new (reader);
                Add(j);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif

        writer.Write(currentIndexer);
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public JobInfoList() { }

    private List<JobInfo> jobInfos = new ();

    public JobInfo Add(JobInfo s)
    {
        jobInfos.Add(s);
        return s;
    }

    public void RemoveAt(int index)
    {
        jobInfos.RemoveAt(index);
    }

    public JobInfo this[int index] => jobInfos[index];

    private int currentIndexer;

    /// <summary>
    /// Call before using Next() to reset to the top of the list.
    /// </summary>
    public void ResetIndex()
    {
        currentIndexer = 0;
    }

    /// <summary>
    /// Returns the next JobInfo based on the indexer that is reset by ResetIndexer().  Returns null if at the end.
    /// </summary>
    /// <returns></returns>
    public JobInfo Next()
    {
        if (currentIndexer >= jobInfos.Count)
        {
            return null;
        }

        JobInfo j = jobInfos[currentIndexer];
        currentIndexer++;
        return j;
    }

    #region ICopyTable
    public Type ElementType => typeof(JobInfo);

    public object GetRow(int index)
    {
        return jobInfos[index];
    }

    public int Count => jobInfos.Count;
    #endregion ICopyTable
}