using System.ComponentModel;
using System.Text;

using PT.APSCommon;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for BaseActivity.
/// </summary>
public abstract partial class BaseActivity : ExternalBaseIdObject, IPTSerializable
{
    #region IPTSerializable Members
    protected BaseActivity(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12433)
        {
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_splitId);
            m_baseActivityFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_anchorDateTicks);
            a_reader.Read(out m_reportedFinishDateTicks);
            a_reader.Read(out m_reportedStartDateTicks);
            a_reader.Read(out m_reportedProcessingStartTicks);

            a_reader.Read(out bool haveActualResourcesUsed);
            if (haveActualResourcesUsed)
            {
                m_actualResourcesUsed = new ResourceKeyList(a_reader);
            }

            m_compositeScores = new ();
            a_reader.Read(out int resScoresCount);
            for (int i = 0; i < resScoresCount; i++)
            {
                BaseId resId = new BaseId(a_reader);
                a_reader.Read(out int factorsCount);

                List<FactorScore> resFactorScores = new();
                for (int j = 0; j < factorsCount; j++)
                {
                    FactorScore factorScore = new FactorScore(a_reader);
                    resFactorScores.Add(factorScore);
                }

                m_compositeScores.Add(resId, resFactorScores);
            }

            m_resourcesOptimizationScore = new();
            a_reader.Read(out resScoresCount);
            for (int i = 0; i < resScoresCount; i++)
            {
                BaseId resId = new BaseId(a_reader);
                a_reader.Read(out decimal totalScore);

                m_resourcesOptimizationScore.Add(resId, totalScore);
            }

            a_reader.Read(out m_batchAmount);
        }
        #region 12002
        else if (a_reader.VersionNumber >= 12002)
        {
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_splitId);
            m_baseActivityFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_anchorDateTicks);
            a_reader.Read(out m_reportedFinishDateTicks);
            a_reader.Read(out m_reportedStartDateTicks);
            a_reader.Read(out m_reportedProcessingStartTicks);

            bool haveActualResourcesUsed;
            a_reader.Read(out haveActualResourcesUsed);
            if (haveActualResourcesUsed)
            {
                m_actualResourcesUsed = new ResourceKeyList(a_reader);
            }

            a_reader.Read(out decimal resourcesOptimizationScore);// this never made sense
            a_reader.Read(out int factorCount);
            for (int i = 0; i < factorCount; i++)
            {
                new FactorScore(a_reader);// this also never made sense
            }

            a_reader.Read(out m_batchAmount);
        }
        #endregion

        #region 729
        else if (a_reader.VersionNumber >= 729)
        {
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_splitId);
            m_baseActivityFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_anchorDateTicks);
            a_reader.Read(out m_reportedFinishDateTicks);
            a_reader.Read(out m_reportedStartDateTicks);
            a_reader.Read(out m_reportedProcessingStartTicks);

            bool haveActualResourcesUsed;
            a_reader.Read(out haveActualResourcesUsed);
            if (haveActualResourcesUsed)
            {
                m_actualResourcesUsed = new ResourceKeyList(a_reader);
            }

            a_reader.Read(out decimal resourcesOptimizationScore);// this never made sense
            a_reader.Read(out string m_optimizationScoreDetails);
            a_reader.Read(out m_batchAmount);
        }
        #endregion

        if (a_reader.VersionNumber < 12200)
        {
            //For backwards compatibility to fix the dates on jobs that were saved from the UI during DST
            if (ReportedStartDate < PTDateTime.MinDateTime.Add(TimeSpan.FromDays(1)))
            {
                ReportedStartDateTicks = PTDateTime.MinDateTimeTicks;
            }

            if (ReportedFinishDate < PTDateTime.MinDateTime.Add(TimeSpan.FromDays(1)))
            {
                ReportedFinishDateTicks = PTDateTime.MinDateTimeTicks;
            }

            if (ReportedProcessingStartDateTime < PTDateTime.MinDateTime.Add(TimeSpan.FromDays(1)))
            {
                ReportedProcessingStartTicks = PTDateTime.MinDateTimeTicks;
            }
        }

        // this field didn't have a default value. This is to make sure old scenarios load correctly
        m_reportedProcessingStartTicks = PTDateTime.GetValidDateTime(m_reportedProcessingStartTicks);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_requiredFinishQty);
        a_writer.Write(m_splitId);
        m_baseActivityFlags.Serialize(a_writer);
        a_writer.Write(m_anchorDateTicks);
        a_writer.Write(m_reportedFinishDateTicks);
        a_writer.Write(m_reportedStartDateTicks);
        a_writer.Write(m_reportedProcessingStartTicks);

        a_writer.Write(m_actualResourcesUsed != null);
        m_actualResourcesUsed?.Serialize(a_writer);

        a_writer.Write(m_compositeScores.Count);
        foreach ((BaseId resId, List<FactorScore> factorsList) in m_compositeScores)
        {
            resId.Serialize(a_writer);
            a_writer.Write(factorsList.Count);
            foreach (FactorScore factorScore in factorsList)
            {
                factorScore.Serialize(a_writer);
            }

        }
        
        a_writer.Write(m_resourcesOptimizationScore.Count);
        foreach ((BaseId resId, decimal totalScore) in m_resourcesOptimizationScore)
        {
            resId.Serialize(a_writer);
            a_writer.Write(totalScore);
        }

        a_writer.Write(m_batchAmount);
    }

    public new const int UNIQUE_ID = 1;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public BaseActivity(long a_externalIdPTDefaultNbr, BaseId a_id)
        : base(a_externalIdPTDefaultNbr, a_id) { }

    public BaseActivity(BaseId a_id, ScenarioDetail a_sd, JobT.BaseActivity a_jobTBaseActivity)
        : base(a_jobTBaseActivity)
    {
        Id = a_id;

        RequiredFinishQty = a_sd.ScenarioOptions.RoundQty(a_jobTBaseActivity.RequiredFinishQty);

        ReportedStartDateTicks = a_jobTBaseActivity.ReportedStartDateTicks;
        ReportedFinishDateTicks = a_jobTBaseActivity.ReportedFinishDateTicks;
        BatchAmount = a_jobTBaseActivity.BatchAmount;

        if (a_jobTBaseActivity.AnchorSet)
        {
            if (a_jobTBaseActivity.Anchor)
            {
                ExternalAnchor(a_jobTBaseActivity.AnchorStartDate.Ticks);
            }
            else
            {
                SetAnchor(false);
                AnchorDateHasBeenSet = true; //this must be set so when this activity is used to update the current activity, we can check whether to update anchor status.
            }
        }
    }

    public BaseActivity(BaseId a_id, BaseActivity a_sourceActivity)
        : base(a_id, a_sourceActivity)
    {
        RequiredFinishQty = a_sourceActivity.RequiredFinishQty;
    }
    #endregion

    #region Shared Properties
    private BoolVector32 m_baseActivityFlags;

    private decimal m_requiredFinishQty;

    /// <summary>
    /// The total quantity of good parts this activity needs to produce.
    /// </summary>
    [Required(true)]
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal RequiredFinishQty
    {
        get => m_requiredFinishQty;
        internal set => m_requiredFinishQty = value;
    }

    /// <summary>
    /// This is the RequiredFinishQty adjusted to take into consideration the most constraining predecessor operation.
    /// </summary>
    public abstract decimal ExpectedFinishQty { get; }
    #endregion

    #region Flag indexes
    private const int FeasibleIdx = 0;
    private const int JumpableIdx = 1;
    private const int AnchorIdx = 2;

    private const int ScheduledIdx = 3;

    //const int ZeroLengthIdx = 4;
    private const int AnchorDateHasBeenSetIdx = 5;
    private const int ConnectionViolatedIdx = 6;
    #endregion

    #region Properties
    /// <summary>
    /// If the Activity is scheduled before any of its Predecessor Operations or Materials will be available then it is considered Infeasible.  Otherwise it is Feasible.  There are various controls that
    /// allow Infeasible Actvities to result such as when Materials or Operations are marked as non-constraints.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    [Browsable(false)] //not yet
    public bool Feasible
    {
        get => m_baseActivityFlags[FeasibleIdx];
        private set => m_baseActivityFlags[FeasibleIdx] = value;
    }

    /// <summary>
    /// Whether the activity can be jumped by its right neighbors during a move. An example of when a block may be become jumpable is when its predecessor is moved so that it completes after the currently
    /// scheduled start time of the activity. In the move algorithm we mark the activity being moved as jumpable to trigger the possible jumps within the simulation. For internal use only.
    /// </summary>
    [Browsable(false)]
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool Jumpable
    {
        get => m_baseActivityFlags[JumpableIdx];
        private set => m_baseActivityFlags[JumpableIdx] = value;
    }

    /// <summary>
    /// If true then Optimizations will attempt to start the Activity as close as possible to the AnchorStartDate.
    /// If manually moved or Expedited, Anchored Activities are re-anchored to their new Start Date.
    /// If moved with in Exact mode, successor Activities are re-anchored at their new Start Date only if it is earlier than their Anchor Start Date.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public bool Anchored
    {
        get => m_baseActivityFlags[AnchorIdx];
        private set => m_baseActivityFlags[AnchorIdx] = value;
    }

    /// <summary>
    /// The date and time on which the Activity is Anchored (if it is marked as Anchored).
    /// </summary>
    public DateTime AnchorStartDate
    {
        get
        {
            if (Anchored && AnchorDateHasBeenSet)
            {
                return new DateTime(m_anchorDateTicks);
            }

            return PTDateTime.MinDateTime;
        }
    }

    /// <summary>
    /// If Anchored and Scheduled, this is the Scheduled Start Date minus the Anchored Start Date.  This indicates by how much the Activity has "drifted" from its Anchor date.
    /// Otherwise this value is zero.
    /// </summary>
    public TimeSpan AnchorDrift
    {
        get
        {
            if (Anchored && Scheduled)
            {
                return ScheduledStartDate.Subtract(AnchorStartDate);
            }

            return new TimeSpan(0);
        }
    }

    /// <summary>
    /// Indicates whether the InternalActivity is currently Scheduled.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public virtual bool Scheduled
    {
        get => m_baseActivityFlags[ScheduledIdx];
        internal set => m_baseActivityFlags[ScheduledIdx] = value;
    }

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public bool ConnectionViolated
    {
        get => m_baseActivityFlags[ConnectionViolatedIdx];
        internal set => m_baseActivityFlags[ConnectionViolatedIdx] = value;
    }

    /// <summary>
    /// Whether the Activity is scheduled to start after the MaxDelayRequiredStartBy date.
    /// </summary>
    public bool MaxDelayViolation
    {
        get
        {
            if (Scheduled)
            {
                if (GetScheduledStartTicks() > MaxDelayRequiredStartBy.Ticks)
                {
                    return true;
                }

                if (ScheduledFinishDateTicks < MaxDelayRequiredEndAfter.Ticks)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// The date/time by which the Activity must be scheduled to start in order to avoid violating the Max Delay limit of any predecessor Operations.
    /// </summary>
    public abstract DateTime MaxDelayRequiredStartBy { get; }

    /// <summary>
    /// The date/time by which the Activity must be scheduled to end in order to avoid violating the Max Delay limit of any successor Operations.
    /// </summary>
    public abstract DateTime MaxDelayRequiredEndAfter { get; }

    /// <summary>
    /// The amount of time the Activity can be delayed and still start before the MaxDelayRequiredStartBy date.
    /// Negative values indicate that the Activity is starting too late.
    /// </summary>
    public TimeSpan MaxDelaySlack
    {
        get
        {
            if (Scheduled)
            {
                return new TimeSpan(MaxDelayRequiredStartBy.Ticks - GetScheduledStartTicks());
            }

            return new TimeSpan(0); //Scheduled Start will return an error if unscheduled
        }
    }

    private int m_splitId;

    /// <summary>
    /// Activities that were split from the same source InternalActivity have the same splitId.  This is used for unsplitting. For internal use only. -1 if not from a split
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public int SplitId
    {
        get => m_splitId;
        private set => m_splitId = value;
    }

    public abstract DateTime ScheduledStartDate { get; }

    /// <summary>
    /// The scheduled start date of this activity.
    /// </summary>
    internal abstract long GetScheduledStartTicks();

    public DateTime ScheduledEndDate => new (ScheduledFinishDateTicks);

    internal abstract long ScheduledFinishDateTicks { get; }

    public abstract TimeSpan Slack { get; }

    public abstract TimeSpan LeftLeeway { get; }

    public abstract TimeSpan RightLeeway { get; }

    public abstract TimeSpan ResourceTransferSpan { get; }

    public abstract BaseActivityDefs.onTimeStatuses Timing { get; }

    /// <summary>
    /// An Activity is considered to be a bottleneck if it is late and the
    /// Operation has no Predecessors that are Late.  Without moving the Activity earlier the Job cannot be ontime.
    /// </summary>
    public bool Bottleneck =>
        Timing == BaseActivityDefs.onTimeStatuses.CapacityBottleneck ||
        Timing == BaseActivityDefs.onTimeStatuses.MaterialBottleneck ||
        Timing == BaseActivityDefs.onTimeStatuses.ReleaseDateBottleneck;

    internal long m_reportedFinishDateTicks = PTDateTime.MinDateTime.Ticks;

    public bool ReportedFinishDateSet => m_reportedFinishDateTicks > PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// This only has meaning when the activity has been finished.
    /// </summary>
    internal long ReportedFinishDateTicks
    {
        get => m_reportedFinishDateTicks;

        set => m_reportedFinishDateTicks = value;
    }

    /// <summary>
    /// This only has meaning when the BaseActivity.Finished flag has been set.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public DateTime ReportedFinishDate => new (m_reportedFinishDateTicks);

    private long m_reportedStartDateTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// Only has meaning once some hours have been reported on Setup, Run, or PostProcessing.
    /// The Scenario Clock Date/Time when hours are first reported.
    /// </summary>
    public long ReportedStartDateTicks
    {
        get => m_reportedStartDateTicks;
        internal set => m_reportedStartDateTicks = value;
    }

    /// <summary>
    /// This only has meaning when Setup, Run, or PostProcessing Hours have been reported.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public DateTime ReportedStartDate
    {
        get
        {
            if (!ReportedStartDateSet)
            {
                return ReportedFinishDate; //For older versions where this was not set.
            }

            return new DateTime(m_reportedStartDateTicks);
        }
        //set { ReportedStarDateTicks = value.Ticks; }
    }

    [Browsable(false)]
    public bool ReportedStartDateSet => m_reportedStartDateTicks > PTDateTime.MinDateTime.Ticks;

    private long m_reportedProcessingStartTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// The time when a batch was started can be reported.
    /// </summary>
    public long ReportedProcessingStartTicks
    {
        get => m_reportedProcessingStartTicks;
        internal set => m_reportedProcessingStartTicks = value;
    }

    public bool ReportedProcessingStartDateSet => ReportedProcessingStartTicks > PTDateTime.MinDateTime.Ticks;

    public DateTime ReportedProcessingStartDateTime
    {
        get => new (ReportedProcessingStartTicks);
        internal set => ReportedProcessingStartTicks = value.Ticks;
    }

    internal virtual void ResetProductionStatus()
    {
        ReportedStartDateTicks = PTDateTime.MinDateTime.Ticks;
        ReportedFinishDateTicks = PTDateTime.MinDateTime.Ticks;
    }

    private ResourceKeyList m_actualResourcesUsed;

    /// <summary>
    /// For Finished Activities these are the Resources that were if any.  Null if finished while unscheduled.
    /// </summary>
    public ResourceKeyList ActualResourcesUsed => m_actualResourcesUsed;

    protected void SetActualResourcesUsed(ResourceKeyList list)
    {
        m_actualResourcesUsed = list;
    }

    protected Dictionary<BaseId, decimal> m_resourcesOptimizationScore = new ();

    private decimal m_batchAmount;
    /// <summary>
    /// Alternative field to use for batching
    /// </summary>
    public decimal BatchAmount
    {
        get => m_batchAmount;
        internal set => m_batchAmount = value;
    }
    #endregion

    internal string GetScheduledResourceScoreDetailsBase(BaseId a_scheduledResId)
    {
        #if DEBUG
        decimal totalForValidation = 0;
        #endif
        if (CompositeScores.Count == 0)
        {
            return string.Empty;
        }

        //Calculate the percentages for each score.

        IReadOnlyList<FactorScore> factorScoresList = GetFactorScoresForResource(a_scheduledResId);
        if (factorScoresList.Count == 0)
        {
            return string.Empty;
        }
        #if DEBUG
        totalForValidation = factorScoresList.Sum(f => f.Score);
        if (totalForValidation != 0 && -totalForValidation != TotalScoreBeforeResourceAdjustment)
        {
            //throw new PT.APSCommon.PTValidationException("2139");
        }
        #endif


        //Create the strings based on the scores, sorted by their contribution percentage.
        List<FactorScore> orderedScores = factorScoresList.OrderByDescending(f => f.PctOfTotal).ToList();
        StringBuilder builder = new ();
        for (int i = 0; i < orderedScores.Count; i++)
        {
            FactorScore score = orderedScores[i];
            builder.AppendLine(string.Format("{0}: {1:N3} ({2:#0.#%})", score.FactorName, score.Score, score.PctOfTotal)); //show scores that increases selection as positive for understandability.
        }

        return builder.ToString();
    }

    #region ERP transmission status update
    /// <summary>
    /// Call this function before handling a JobT or some other transmission that updates the status of jobs.
    /// It resets the activity variables that indicate the type of updates that have occurred.
    /// </summary>
    internal virtual void ResetERPStatusUpdateVariables() { }
    #endregion

    #region Update
    /// <summary>
    /// Base update function.
    /// </summary>
    /// <returns>Whether any activity properties were changed</returns>
    internal virtual BaseActivityUpdateResults Update(ScenarioDetail a_sd, BaseActivity a_baseActivityTemp, bool a_preserveRequiredQty, bool a_erpUpdate, bool a_activityManualUpdateOnly, IScenarioDataChanges a_dataChanges, BaseId a_jobId, out bool o_updated)
    {
        o_updated = false;
        BaseActivityUpdateResults changes = new ();
        bool flagConstraintChanges = false;
        bool flagProductionChanges = false;
        bool isScheduled = Scheduled;
        if (!a_preserveRequiredQty && a_baseActivityTemp.RequiredFinishQty != RequiredFinishQty)
        {
            RequiredFinishQty = a_sd.ScenarioOptions.RoundQty(a_baseActivityTemp.RequiredFinishQty);
            changes.RequiredFinishQtyChanged = true;
            o_updated = true;
            flagProductionChanges = true;
            a_dataChanges.FlagEligibilityChanges(a_jobId);
        }

        //added for Trinity.  Anchoring everything externally to schedule at specified date.  Need it to update.
        if (a_baseActivityTemp.AnchorDateHasBeenSet)
        {
            if (a_baseActivityTemp.Anchored)
            {
                if (!Anchored || AnchorDateTicks != a_baseActivityTemp.AnchorDateTicks)
                {
                    ExternalAnchor(a_baseActivityTemp.AnchorDateTicks);
                    o_updated = true;
                    flagConstraintChanges = true;
                }
            }
            else if (Anchored)
            {
                SetAnchor(false);
                o_updated = true;
            }
        }

        if (ReportedStartDateTicks != a_baseActivityTemp.ReportedStartDate.Ticks && (!a_erpUpdate || (a_erpUpdate && !a_activityManualUpdateOnly)))
        {
            ReportedStartDateTicks = a_baseActivityTemp.ReportedStartDate.Ticks;
            if (ReportedStartDateTicks != 0 && ReportedStartDate != DateTime.MinValue)
            {
                changes.ReportedStartDateChanged = true;
                o_updated = true;
                flagConstraintChanges = true;
            }
        }

        if (ReportedFinishDateTicks != a_baseActivityTemp.ReportedFinishDateTicks && (!a_erpUpdate || (a_erpUpdate && !a_activityManualUpdateOnly)))
        {
            ReportedFinishDateTicks = a_baseActivityTemp.ReportedFinishDateTicks;
            if (ReportedFinishDateTicks != 0 && ReportedFinishDate != PTDateTime.MinDateTime)
            {
                changes.ReportedFinishDateChanged = true;
                o_updated = true;
                flagProductionChanges = true;
            }
        }

        if (isScheduled)
        {
            if (flagConstraintChanges)
            {
                a_dataChanges.FlagConstraintChanges(a_jobId);
            }

            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(a_jobId);
            }
        }

        return changes;
    }

    public bool Edit(ScenarioDetail a_sd, ActivityEdit a_edit, IScenarioDataChanges a_dataChanges, BaseId a_jobId)
    {
        bool changes = false;

        if (a_edit.RequiredFinishQtySet && RequiredFinishQty != a_edit.RequiredFinishQty)
        {
            RequiredFinishQty = a_sd.ScenarioOptions.RoundQty(a_edit.RequiredFinishQty);
            changes = true;
            a_dataChanges.FlagProductionChanges(a_jobId);
            a_dataChanges.FlagEligibilityChanges(a_jobId);
        }

        if (a_edit.ReportedStarDateTicksSet && a_edit.ReportedStartDateTicks != 0 
                                            && a_edit.ReportedStartDateTicks != PTDateTime.MinDateTicks 
                                            && a_edit.ReportedStartDateTicks != DateTime.MinValue.Ticks)
        {
            ReportedStartDateTicks = a_edit.ReportedStartDateTicks;
            changes = true;
            a_dataChanges.FlagConstraintChanges(a_jobId);
        }

        if (a_edit.AnchorStartDateSet && m_anchorDateTicks != a_edit.AnchorDate.Ticks)
        {
            m_anchorDateTicks = a_edit.AnchorDate.Ticks;
            changes = true;
            a_dataChanges.FlagConstraintChanges(a_jobId);
        }

        return changes;
    }

    //TODO: This should be removed when the activity update process is cleaned up
    internal class BaseActivityUpdateResults
    {
        public bool RequiredFinishQtyChanged;
        public bool ReportedStartDateChanged;
        public bool ReportedFinishDateChanged;

        public bool AnyChanges => RequiredFinishQtyChanged | ReportedStartDateChanged | ReportedFinishDateChanged;
    }

    protected void SetValuesFromSplitOriginal(BaseActivity a_originalActivity)
    {
        if (a_originalActivity.Anchored)
        {
            ExternalAnchor(a_originalActivity.AnchorStartDate.Ticks);
        }
        else
        {
            SetAnchor(false);
        }
    }
    #endregion

    #region Cost
    public abstract decimal LaborCost { get; }

    public abstract decimal MachineCost { get; }

    public abstract decimal SubcontractCost { get; }
    #endregion Cost
}
