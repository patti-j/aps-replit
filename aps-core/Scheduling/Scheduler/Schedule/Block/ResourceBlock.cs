using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.PTMath;
using PT.Database;
using PT.PackageDefinitions.Settings.PublishOptions;
using PT.Scheduler.Schedule.Block.UsageDetails;
using PT.SchedulerDefinitions;
using System.ComponentModel;

namespace PT.Scheduler;

/// <summary>
/// Represents the work of one Resource on an Activity or Subcontract Operation.
/// </summary>
public partial class ResourceBlock : Block, IInterval, IPTSerializable
{
    public new const int UNIQUE_ID = 328;

    #region IPTSerializable Members
    public ResourceBlock(IReader a_reader)
        : base(a_reader)
    {
        // [BATCH_CODE]

        if (a_reader.VersionNumber >= 12415)
        {
            m_referenceInfo = new ReferenceInfo();

            if (Scheduled)
            {
                ResourceKey rk = new (a_reader);
                m_referenceInfo.m_resourceKey = rk;
            }

            m_referenceInfo.m_batchKey = new BaseId(a_reader);

            m_capacityProfile = new OperationCapacityProfile(a_reader);
        }
        else if (a_reader.VersionNumber >= 600)
        {
            m_referenceInfo = new ReferenceInfo();

            if (Scheduled)
            {
                ResourceKey rk = new (a_reader);
                m_referenceInfo.m_resourceKey = rk;
            }

            m_referenceInfo.m_batchKey = new BaseId(a_reader);

            m_capacityProfile = new OperationCapacityProfile();
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        //GetKey().Serialize(writer);
        if (Scheduled)
        {
            ResourceKey rk = new (ScheduledResource.PlantId, ScheduledResource.DepartmentId, ScheduledResource.Id);
            rk.Serialize(a_writer);
        }

        Batch.Id.Serialize(a_writer);
        m_capacityProfile.Serialize(a_writer);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    private ReferenceInfo m_referenceInfo;

    private class ReferenceInfo
    {
        internal BlockKey m_blockKey_onlyValidInPreBatchScenarios;
        internal ResourceKey m_resourceKey;

        internal BaseId m_batchKey; // [BATCH]
    }

    internal void RestoreReferences(PlantManager a_plantManager)
    {
        Plant plant = a_plantManager.GetById(m_referenceInfo.m_resourceKey.Plant);
        Department department = plant.Departments.GetById(m_referenceInfo.m_resourceKey.Department);
        Resource resource = department.Resources.GetById(m_referenceInfo.m_resourceKey.Resource);

        ResourceBlockList.Node node = resource.Blocks.First;
        bool found = false;
        while (node != null)
        {
            //*LRH*Optimize*There's a faster way to find this information.
            ResourceBlock resourceBlock = node.Data;
            if (resourceBlock == this)
            {
                MachineBlockListNode = node;
                found = true;
                break;
            }

            node = node.Next;
        }

        if (!found)
        {
            throw new PTException("MachineBlockListNode wasn't found.");
        }
    }

    // [BATCH_CODE]

    #region RestoreReferences2: Only one of the two below should be called sometime after RestoreReferences().
    /// <summary>
    /// Restore remaining references that couldn't be restored during RestoreReferences().
    /// </summary>
    /// <param name="a_batchDictionary"></param>
    /// <param name="a_blockNode"></param>
    internal void RestoreReferences_2_batches(Dictionary<BaseId, Batch> a_batchDictionary, ResourceBlockList.Node a_blockNode)
    {
        SetBatch(a_batchDictionary[m_referenceInfo.m_batchKey]);
        Batch.RestoreReferences2(Id.Value, a_blockNode, ScheduledResource);

        base.RestoreReferences(Batch.FirstActivity.Operation);

        m_referenceInfo = null;
    }
    #endregion
    #endregion

    #region Properties
    [Obsolete("7/27/2010: This is no longer valid. The blocks batch activities must be checked individually. This function will be deleted soon.")]
    public bool Locked => Activity.ResourceRequirementLocked(ResourceRequirementIndex);

    /// <summary>
    /// The first activity in the batch this block is part of.
    /// </summary>
    [Browsable(false)]
    [Obsolete("Use references to batch instead. This may be deleted soon. It might also be renamed FirstBatchActivity.")]
    public InternalActivity Activity =>
        // [BATCH_CODE]
        Batch.FirstActivity;

    /// <summary>
    /// There was an error accessing the batch or related object because the batch reference was null.
    /// </summary>
    public class NullBatchException : PTHandleableException { }

    private Resource m_scheduledResource;

    /// <exception cref="NullBatchException" accessor="get">Batch reference has not been established..</exception>
    [Browsable(false)]

    /// <summary>
    /// The resource the block is scheduled on.
    /// </summary>
    public Resource ScheduledResource
    {
        get
        {
            // [BATCH_CODE]
            if (m_scheduledResource == null)
            {
                throw new NullBatchException();
            }

            return m_scheduledResource;
        }
        set => m_scheduledResource = value;
    }

    public long StartTicks { get; private set; }

    [Browsable(false)]
    /// <summary>
    /// The time the block is scheduled to end.
    /// </summary>
    public long EndTicks { get; internal set; }

    public bool Intersection(long a_start, long a_end)
    {
        return Interval.Intersection(StartTicks, EndTicks, a_start, a_end);
    }

    public bool Intersection(IInterval a_interval)
    {
        return Intersection(a_interval.StartTicks, a_interval.EndTicks);
    }

    /// <summary>
    /// When the resource is scheduled to start working (either setup or run).
    /// </summary>
    public DateTime StartDateTime => new (StartTicks);

    /// <summary>
    /// When the resource is scheduled to finish working.  Does NOT include TransferTime.
    /// </summary>
    public DateTime EndDateTime => new (EndTicks);

    /// <summary>
    /// The calendar time between the Scheduled Start and Scheduled End.
    /// </summary>
    public TimeSpan Duration => EndDateTime.Subtract(StartDateTime);

    [Browsable(false)]
    public bool Contains(long date)
    {
        return StartTicks <= date && EndTicks > date;
    }

    /// <summary>
    /// Name of the resource that the Block is scheduled on.
    /// </summary>
    public string ResourceUsed
    {
        get
        {
            if (ScheduledResource == null)
            {
                return "";
            }

            return ScheduledResource.Name;
        }
    }

    //BaseResource capabilityOverride = null;
    ///// <summary>
    ///// Overrides Capability if specified.  This specifies the resource that the Block should be scheduled on.
    ///// </summary>
    //[System.ComponentModel.Browsable(false)]
    //public BaseResource CapabilityOverride
    //{
    //    get { return capabilityOverride; }
    //    set { capabilityOverride = value; }
    //}

    /// <summary>
    /// Special summary or troubleshooting information.
    /// </summary>
    [Browsable(false)]
    public override string Analysis
    {
        get
        {
            string analysis = "";
            if (EndTicks < StartTicks)
            {
                analysis += Environment.NewLine + "\t\t\ttENDS BEFORE IT STARTS".Localize();
            }

            if (StartTicks == EndTicks)
            {
                analysis += Environment.NewLine + "\t\t\tZERO LENGTH".Localize();
            }

            if (Duration.TotalDays > 7)
            {
                analysis += string.Format("{1}\t\t\tLONG: Duration is {0} days".Localize(), Math.Round(Duration.TotalDays, 2), Environment.NewLine);
            }

            if (Activity.ResourceRequirementLocked(ResourceRequirementIndex))
            {
                analysis += Environment.NewLine + "\t\tLocked".Localize();
            }

            return analysis;
        }
    }
    #endregion Properties

    [Obsolete("This may be deleted or change soon for batches changes.")]
    public override BlockKey GetKey()
    {
        return new BlockKey(Activity.Operation.ManufacturingOrder.Job.Id,
            Activity.Operation.ManufacturingOrder.Id,
            Activity.Operation.Id,
            Activity.Id,
            Id);
    }

    /// <summary>
    /// The hours scheduled on a Resource times its standard hourly cost.
    /// </summary>
    /// <returns></returns>
    public override decimal GetResourceCost()
    {
        decimal resourceCost = 0;
        if (ScheduledResource != null)
        {
            resourceCost += ScheduledResource.StandardHourlyCost * (decimal)Activity.WorkContent.TotalHours;
        }

        return resourceCost;
    }

    /// <summary>
    /// The interest cost
    /// </summary>
    /// <returns></returns>
    public decimal GetCarryingCost()
    {
        decimal cost = 0;

        if (ScheduledResource != null)
        {
            decimal interest = ScheduledResource.Plant.DailyInterestRate * (decimal)Activity.WorkContent.TotalDays; // daily rate * number of days
            cost = (decimal)Activity.WorkContent.TotalHours * ScheduledResource.StandardHourlyCost; // total cost value
            cost = cost * interest;
        }

        return cost;
    }

    #region Scheduling
    internal void Unschedule(ResourceBlockList.Node a_node)
    {
        if (ScheduledResource != null) //***LRH***SIM***Take a close look at this. //JMC Added this since it was causing an error on the next line due to being null
        {
            ScheduledResource.Unschedule(a_node);
        }

        ScheduledResource = null;
        m_scheduled = false;
        SetBatch(null);
    }

    internal void Unschedule()
    {
        Unschedule(m_machineBlockListNode);
    }

    private ResourceBlockList.Node m_machineBlockListNode;

    internal ResourceBlockList.Node MachineBlockListNode
    {
        get => m_machineBlockListNode;

        set => m_machineBlockListNode = value;
    }

    /// <summary>
    /// Returns true if the Block could be moved to the proposed start time.  If false, then constraints are specified.
    /// </summary>
    public bool CanMoveToProposedTime(DateTime a_newStart, out bool o_moReleaseDateConstraint)
    {
        bool constrained = Activity.Operation.ManufacturingOrder.GetConstrainedReleaseTicks(out long constrainedRelease);
        o_moReleaseDateConstraint = constrained && constrainedRelease > a_newStart.Ticks;

        return !o_moReleaseDateConstraint;
    }
    #endregion

    internal void PopulateJobDataSet(ref JobDataSet r_dataSet, InternalActivity a_activity)
    {
        JobDataSet._ResourceUsageRow row = r_dataSet._ResourceUsage.New_ResourceUsageRow();
        row.JobExternalId = a_activity.Operation.ManufacturingOrder.Job.ExternalId;
        row.MoExternalId = a_activity.Operation.ManufacturingOrder.ExternalId;
        row.OpExternalId = a_activity.Operation.ExternalId;
        row.ActivityExternalId = a_activity.ExternalId;
        row.Id = Id.ToBaseType();
        row.Duration = Duration.TotalHours;
        row.Locked = Locked;
        row.Resource = ResourceUsed;
        row.ScheduledEnd = EndDateTime.ToDisplayTime().ToDateTime();
        row.ScheduledStart = StartDateTime.ToDisplayTime().ToDateTime();

        r_dataSet._ResourceUsage.Add_ResourceUsageRow(row);
    }

    #region PT Database
    public void PtDbPopulate(ScenarioDetail a_sd, ref PtDbDataSet r_dataSet, PtDbDataSet.JobActivitiesRow a_jobActivitiesRow, PTDatabaseHelper a_dbHelper)
    {
        bool publishBlockIntervals;
        using (a_sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
        {
            publishBlockIntervals = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPublishDataLimits()).PublishBlockIntervals;
        }
        bool includeBlockIntervals = publishBlockIntervals || a_dbHelper.PublishAllData;

        //Check if there is a ScheduledResource.  If it's not scheduled then there won't be.
        if (ScheduledResource != null)
        {
            //Add Block row
            PtDbDataSet.JobResourceBlocksRow jobBlocksRow = r_dataSet.JobResourceBlocks.AddJobResourceBlocksRow(
                a_jobActivitiesRow.PublishDate,
                a_jobActivitiesRow.InstanceId,
                Activity.Operation.ManufacturingOrder.Job.Id.ToBaseType(),
                Activity.Operation.ManufacturingOrder.Id.ToBaseType(),
                Activity.Operation.Id.ToBaseType(),
                Activity.Id.ToBaseType(),
                Id.ToBaseType(),
                Batch.Id.ToBaseType(),
                ScheduledResource.Department.Plant.Id.ToBaseType(),
                ScheduledResource.Department.Id.ToBaseType(),
                ScheduledResource.Id.ToBaseType(),
                a_dbHelper.AdjustPublishTime(StartDateTime),
                a_dbHelper.AdjustPublishTime(EndDateTime),
                Locked,
                Sequence,
                RunNbr,
                SatisfiedRequirement.Id.ToBaseType(),
                Duration.TotalHours,
                CalcLaborCost(),
                CalcMachineCost(),
                ResourceRequirementIndex,
                Scheduled,
                Batched
            );

            PtDbDataSet.ReportBlocksRow reportBlocksRow = r_dataSet.ReportBlocks.AddReportBlocksRow(
                a_jobActivitiesRow.PublishDate,
                a_jobActivitiesRow.InstanceId,
                Activity.Operation.ManufacturingOrder.Job.Id.ToBaseType(),
                Activity.Operation.ManufacturingOrder.Id.ToBaseType(),
                Activity.Operation.Id.ToBaseType(),
                Activity.Id.ToBaseType(),
                Id.ToBaseType(),
                ScheduledResource.Department.Plant.Id.ToBaseType(),
                ScheduledResource.Department.Id.ToBaseType(),
                ScheduledResource.Id.ToBaseType(),
                a_dbHelper.AdjustPublishTime(StartDateTime),
                a_dbHelper.AdjustPublishTime(EndDateTime),
                Locked,
                Sequence,
                RunNbr,
                SatisfiedRequirement.Id.ToBaseType(),
                ScheduledResource.PlantName,
                ScheduledResource.Department.Name,
                ScheduledResource.Name,
                ScheduledResource.Description,
                a_dbHelper.AdjustPublishTime(Batch.SetupFinishedDateTime),
                a_dbHelper.AdjustPublishTime(Activity.ScheduledEndOfRunDate),
                Activity.Operation.ManufacturingOrder.Job.Name,
                Activity.Operation.ManufacturingOrder.Name,
                Activity.Operation.Name,
                a_dbHelper.AdjustPublishTime(Activity.Operation.ManufacturingOrder.Job.NeedDateTime),
                Activity.RequiredFinishQty,
                Activity.Operation.ManufacturingOrder.Job.Customers.GetCustomerExternalIdsList(),
                Activity.Operation.ManufacturingOrder.Job.Priority,
                a_dbHelper.AdjustPublishTime(Activity.Operation.NeedDate),
                Activity.ScheduledSetupSpan.TotalHours,
                Activity.ScheduledProductionSpan.TotalHours,
                Activity.ScheduledPostProcessingSpan.TotalHours,
                Duration.Ticks,
                Activity.Operation.ManufacturingOrder.ProductName,
                Activity.Operation.ManufacturingOrder.Job.Description,
                Activity.Operation.Description,
                Activity.GetOtherResourcesUsed(this),
                Activity.Slack.TotalDays,
                Activity.ReportedGoodQty,
                Activity.ReportedScrapQty,
                Activity.PercentFinished,
                Activity.ProductionStatus.ToString(),
                Activity.Late,
                Activity.Anchored,
                Activity.Operation.MaterialList,
                SatisfiedRequirement.RequiredCapabilitiesList,
                Activity.Operation.SetupNumber,
                Activity.Operation.OnHold,
                Activity.Operation.HoldReason,
                a_dbHelper.AdjustPublishTime(Activity.Operation.HoldUntil),
                Activity.Operation.ManufacturingOrder.Job.Notes,
                Activity.Operation.Notes,
                Activity.Operation.UOM,
                Activity.Operation.StandardHours,
                Activity.Operation.Attributes.AttributesSummary,
                Activity.ReportedSetupSpan.TotalHours,
                Activity.ReportedRunSpan.TotalHours,
                Activity.ReportedPostProcessingSpan.TotalHours,
                Activity.NbrOfPeople,
                Activity.Comments,
                Activity.Comments2,
                Activity.Operation.ManufacturingOrder.ProductDescription,
                a_dbHelper.AdjustPublishTime(Activity.JitStartDate)
            );

            if (includeBlockIntervals)
            {
                //Add Block Intervals
                BlockCapacityIntervalDetails prodnInterval = GetScheduleDetailsByCapacityIntervalGroupeSetupProcessingAndPostProcessing();
                BlockIntervalDetails cid = new ();
                int i = 0;
                while (prodnInterval.GetNextCapacityIntervalDetails(cid))
                {
                    cid.PtDbPopulate(ref r_dataSet, this, i, jobBlocksRow, a_dbHelper);
                    ++i;
                }
            }
        }
    }

    private int runNbr;

    /// <summary>
    /// The 1-based index number of this block on it's resource for the day it's scheduled to start.
    /// ONLY USE DURING PTDBExport or else it's not updated.
    /// </summary>
    public int RunNbr
    {
        get => runNbr;
        set => runNbr = value;
    }

    private int sequence;

    /// <summary>
    /// The 1-based index number of this block on it's resource.
    /// ONLY USE DURING PTDBExport or else it's not updated.
    /// </summary>
    public int Sequence
    {
        get => sequence;
        set => sequence = value;
    }

    /// <summary>
    /// Names of Resources used other than the Resource used by this Block.
    /// </summary>
    public string OtherResourcesUsed => Activity.GetOtherResourcesUsed(this);
    #endregion

    #region Debug
    public override string ToString()
    {
        return de;
    }

    [Browsable(false)]
    public string de
    {
        get
        {
            string s = string.Format("ResourceName '{0}'; ActivityCount={1}; Start '{2}'; End '{3}'; ", m_scheduledResource.Name, Batch.ActivitiesCount, DateTimeHelper.ToLocalTimeFromUTCTicks(StartTicks), DateTimeHelper.ToLocalTimeFromUTCTicks(EndTicks));
            return s;
        }
    }
    #endregion

    /// <summary>
    /// If some processing time is scheduled then for each capacity interval used the start time, stop time, and amount of material produced are returned.
    /// </summary>
    public BlockCapacityIntervalDetails GetScheduleDetailsByCapacityIntervalGroupeSetupProcessingAndPostProcessing()
    {
        if (ScheduledResource != null)
        {
            return ScheduledResource.GetScheduleDetailsByCapacityIntervalGroupSetupProcessingAndPostProcessing(this);
        }

        return new BlockCapacityIntervalDetails(new List<BlockIntervalTypeCapacityIntervalDetails>());
    }

    internal Batch m_batch; // [BATCH]

    /// <summary>
    /// </summary>
    /// <exception cref="NullBatchException" accessor="get">Batch reference has not been established.</exception>
    public Batch Batch
    {
        get
        {
            // [BATCH_CODE]
            if (m_batch == null)
            {
                throw new NullBatchException();
            }

            return m_batch;
        }
    }

    /// <summary>
    /// Set the batch that this block was scheduled for.
    /// </summary>
    /// <param name="a_batch"></param>
    private void SetBatch(Batch a_batch)
    {
        m_batch = a_batch;
        if (m_batch != null)
        {
            StartTicks = m_batch.GetUsageStart(ResourceRequirementIndex);
            EndTicks = m_batch.GetUsageEnd(ResourceRequirementIndex);
        }
    }

    private void SetBatchPreEnhancement(Batch a_batch, InternalActivity a_act)
    {
        m_batch = a_batch;
        StartTicks = a_act.ScheduledStartDate.Ticks;
        EndTicks = a_act.ScheduledEndDate.Ticks;
    }

    /// <summary>
    /// Whether the Activity has been Batched on this Resource using the Operation Batch Code.
    /// </summary>
    public bool Batched => m_batch != null;
    
    private OperationCapacityProfile m_capacityProfile;

    public OperationCapacityProfile CapacityProfile => m_capacityProfile;

    public TimeSpan CalculateOnlineCapacityTime()
    {
        long setupTime = m_capacityProfile.SetupProfile.Sum(x => x.Duration.Ticks);
        long productionTime = m_capacityProfile.ProductionProfile.Sum(x => x.Duration.Ticks);
        long postProcessingTime = m_capacityProfile.PostprocessingProfile.Sum(x => x.Duration.Ticks);
        long storageTime = m_capacityProfile.StorageProfile.Sum(x => x.Duration.Ticks);
        long cleanTime = m_capacityProfile.CleanProfile.Sum(x => x.Duration.Ticks);
        return TimeSpan.FromTicks(setupTime + productionTime + postProcessingTime + storageTime + cleanTime);
    }

    public TimeSpan CalculateOnlineOvertimeCapacityTime()
    {
        long setupTime = m_capacityProfile.SetupProfile.Where(x => x.Overtime).Sum(x => x.Duration.Ticks);
        long productionTime = m_capacityProfile.ProductionProfile.Where(x => x.Overtime).Sum(x => x.Duration.Ticks);
        long postProcessingTime = m_capacityProfile.PostprocessingProfile.Where(x => x.Overtime).Sum(x => x.Duration.Ticks);
        long storageTime = m_capacityProfile.StorageProfile.Where(x => x.Overtime).Sum(x => x.Duration.Ticks);
        long cleanTime = m_capacityProfile.CleanProfile.Where(x => x.Overtime).Sum(x => x.Duration.Ticks);
        return TimeSpan.FromTicks(setupTime + productionTime + postProcessingTime + storageTime + cleanTime);
    }

    /// <summary>
    /// Calculate the cost of running a block on a machine.
    /// </summary>
    internal decimal CalcMachineCost()
    {
        Resource scheduledResource = ScheduledResource;

        if (!scheduledResource.IsLabor() && !scheduledResource.IsSubcontract())
        {
            return CapacityProfile.CalcTotalCost();
        }

        return 0m;
    }

    /// <summary>
    /// Calculate the subcontractor cost of a block.
    /// </summary>
    internal decimal CalcSubcontractorCost()
    {
        if (ScheduledResource.IsSubcontract())
        {
            return CapacityProfile.CalcTotalCost();
        }

        return 0m;
    }

    /// <summary>
    /// Calculate the cost of labor of a block.
    /// </summary>
    internal decimal CalcLaborCost()
    {
        if (ScheduledResource.IsLabor())
        {
            return CapacityProfile.CalcTotalCost();
        }

        return 0m;
    }
}