using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// For finishing an Activity internally in the UI.
/// </summary>
public class InternalActivityFinishT : ActivityIdBaseT
{
    public new const int UNIQUE_ID = 444;

    #region IPTSerializable Members
    public InternalActivityFinishT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12532)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out int val);
            productionStatus = (InternalActivityDefs.productionStatuses)val;
            reader.Read(out reportedSetupSpan);
            reader.Read(out reportedRunSpan);
            reader.Read(out reportedPostProcessingSpan);
            reader.Read(out m_reportedCleanSpan);
            reader.Read(out m_reportedStorageSpan); // new in 12522
            reader.Read(out m_reportedCleanOutGrade);
            reader.Read(out reportedScrapQty);
            reader.Read(out reportedGoodQty);
            reader.Read(out reportedSpansAreIncremental);
            reader.Read(out reportedQtiesAreIncremental);
            reader.Read(out paused);
            reader.Read(out val);
            peopleUsage = (InternalActivityDefs.peopleUsages)val;
            reader.Read(out nbrOfPeople);
            reader.Read(out comments);
            reader.Read(out reportedStartDateTicks);
            reader.Read(out m_reportedProcessingStartTicks);
            reader.Read(out m_reportedProcessingEndTicks);
            reader.Read(out reportedFinishDateTicks);
            reader.Read(out m_reportedEndOfPostProcessingTicks);
            reader.Read(out m_reportedEndOfStorageTicks);
            reader.Read(out finishPredecessors);
            reader.Read(out int materialIssuesCount);
            for (int miI = 0; miI < materialIssuesCount; miI++)
            {
                materialIssues.Add(new MaterialIssue(reader));
            }

            reader.Read(out comments2);
            reader.Read(out m_batchAmount);
            reader.Read(out m_nowFinishUtcTime);
        }
        #region 12523
        else if (reader.VersionNumber >= 12522)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out int val);
            productionStatus = (InternalActivityDefs.productionStatuses)val;
            reader.Read(out reportedSetupSpan);
            reader.Read(out reportedRunSpan);
            reader.Read(out reportedPostProcessingSpan);
            reader.Read(out m_reportedCleanSpan);
            reader.Read(out m_reportedStorageSpan); // new in 12522
            reader.Read(out m_reportedCleanOutGrade);
            reader.Read(out reportedScrapQty);
            reader.Read(out reportedGoodQty);
            reader.Read(out reportedSpansAreIncremental);
            reader.Read(out reportedQtiesAreIncremental);
            reader.Read(out paused);
            reader.Read(out val);
            peopleUsage = (InternalActivityDefs.peopleUsages)val;
            reader.Read(out nbrOfPeople);
            reader.Read(out comments);
            reader.Read(out reportedStartDateTicks);
            reader.Read(out reportedFinishDateTicks);
            reader.Read(out m_reportedEndOfPostProcessingTicks);
            reader.Read(out m_reportedEndOfStorageTicks);
            reader.Read(out finishPredecessors);
            reader.Read(out int materialIssuesCount);
            for (int miI = 0; miI < materialIssuesCount; miI++)
            {
                materialIssues.Add(new MaterialIssue(reader));
            }

            reader.Read(out comments2);
            reader.Read(out m_batchAmount);
            reader.Read(out m_nowFinishUtcTime);
        }
        #endregion
        else if (reader.VersionNumber >= 12522)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out int val);
            productionStatus = (InternalActivityDefs.productionStatuses)val;
            reader.Read(out reportedSetupSpan);
            reader.Read(out reportedRunSpan);
            reader.Read(out reportedPostProcessingSpan);
            reader.Read(out m_reportedCleanSpan);
            reader.Read(out m_reportedStorageSpan); // new in 12522
            reader.Read(out m_reportedCleanOutGrade);
            reader.Read(out reportedScrapQty);
            reader.Read(out reportedGoodQty);
            reader.Read(out reportedSpansAreIncremental); 
            reader.Read(out reportedQtiesAreIncremental); 
            reader.Read(out paused); 
            reader.Read(out val);
            peopleUsage = (InternalActivityDefs.peopleUsages)val;
            reader.Read(out nbrOfPeople);
            reader.Read(out comments);
            reader.Read(out reportedStartDateTicks);
            reader.Read(out reportedFinishDateTicks);
            reader.Read(out finishPredecessors);
            reader.Read(out int materialIssuesCount);
            for (int miI = 0; miI < materialIssuesCount; miI++)
            {
                materialIssues.Add(new MaterialIssue(reader));
            }

            reader.Read(out comments2);
            reader.Read(out m_batchAmount);
            reader.Read(out m_nowFinishUtcTime);
        }
        else if (reader.VersionNumber >= 12500)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out int val);
            productionStatus = (InternalActivityDefs.productionStatuses)val;
            reader.Read(out reportedSetupSpan);
            reader.Read(out reportedRunSpan);
            reader.Read(out reportedPostProcessingSpan);
            reader.Read(out m_reportedCleanSpan);
            reader.Read(out m_reportedCleanOutGrade);
            reader.Read(out reportedScrapQty);
            reader.Read(out reportedGoodQty);
            reader.Read(out reportedSpansAreIncremental); //new in 50
            reader.Read(out reportedQtiesAreIncremental); //new in 50
            reader.Read(out paused); //new in 53
            reader.Read(out val);
            peopleUsage = (InternalActivityDefs.peopleUsages)val;
            reader.Read(out nbrOfPeople);
            reader.Read(out comments);
            reader.Read(out reportedStartDateTicks);
            reader.Read(out reportedFinishDateTicks);
            reader.Read(out finishPredecessors);
            reader.Read(out int materialIssuesCount);
            for (int miI = 0; miI < materialIssuesCount; miI++)
            {
                materialIssues.Add(new MaterialIssue(reader));
            }

            reader.Read(out comments2);
            reader.Read(out m_batchAmount);
            reader.Read(out m_nowFinishUtcTime);
        }
        else if (reader.VersionNumber >= 12439)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out int val);
            productionStatus = (InternalActivityDefs.productionStatuses)val;
            reader.Read(out reportedSetupSpan);
            reader.Read(out reportedRunSpan);
            reader.Read(out reportedPostProcessingSpan);
            reader.Read(out m_reportedCleanSpan);
            reader.Read(out m_reportedCleanOutGrade);
            reader.Read(out reportedScrapQty);
            reader.Read(out reportedGoodQty);
            reader.Read(out reportedSpansAreIncremental); //new in 50
            reader.Read(out reportedQtiesAreIncremental); //new in 50
            reader.Read(out paused); //new in 53
            reader.Read(out val);
            peopleUsage = (InternalActivityDefs.peopleUsages)val;
            reader.Read(out nbrOfPeople);
            reader.Read(out comments);
            reader.Read(out reportedStartDateTicks);
            reader.Read(out m_reportedProcessingStartTicks);
            reader.Read(out m_reportedProcessingEndTicks);
            reader.Read(out reportedFinishDateTicks);
            reader.Read(out finishPredecessors);
            reader.Read(out int materialIssuesCount);
            for (int miI = 0; miI < materialIssuesCount; miI++)
            {
                materialIssues.Add(new MaterialIssue(reader));
            }

            reader.Read(out comments2);
            reader.Read(out m_batchAmount);
            reader.Read(out m_nowFinishUtcTime);
        }
        else if (reader.VersionNumber >= 12416)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out int val);
            productionStatus = (InternalActivityDefs.productionStatuses)val;
            reader.Read(out reportedSetupSpan);
            reader.Read(out reportedRunSpan);
            reader.Read(out reportedPostProcessingSpan);
            reader.Read(out m_reportedCleanSpan);
            reader.Read(out m_reportedCleanOutGrade);
            reader.Read(out reportedScrapQty);
            reader.Read(out reportedGoodQty);
            reader.Read(out reportedSpansAreIncremental); //new in 50
            reader.Read(out reportedQtiesAreIncremental); //new in 50
            reader.Read(out paused); //new in 53
            reader.Read(out val);
            peopleUsage = (InternalActivityDefs.peopleUsages)val;
            reader.Read(out nbrOfPeople);
            reader.Read(out comments);
            reader.Read(out reportedStartDateTicks);
            reader.Read(out reportedFinishDateTicks);
            reader.Read(out finishPredecessors);
            reader.Read(out int materialIssuesCount);
            for (int miI = 0; miI < materialIssuesCount; miI++)
            {
                materialIssues.Add(new MaterialIssue(reader));
            }

            reader.Read(out comments2);
            reader.Read(out m_batchAmount);
            reader.Read(out m_nowFinishUtcTime);
        }
        else if (reader.VersionNumber >= 745)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out int val);
            productionStatus = (InternalActivityDefs.productionStatuses)val;
            reader.Read(out reportedSetupSpan);
            reader.Read(out reportedRunSpan);
            reader.Read(out reportedPostProcessingSpan);
            reader.Read(out reportedScrapQty);
            reader.Read(out reportedGoodQty);
            reader.Read(out reportedSpansAreIncremental); //new in 50
            reader.Read(out reportedQtiesAreIncremental); //new in 50
            reader.Read(out paused); //new in 53
            reader.Read(out val);
            peopleUsage = (InternalActivityDefs.peopleUsages)val;
            reader.Read(out nbrOfPeople);
            reader.Read(out comments);
            reader.Read(out reportedStartDateTicks);
            reader.Read(out reportedFinishDateTicks);
            reader.Read(out finishPredecessors);
            reader.Read(out int materialIssuesCount);
            for (int miI = 0; miI < materialIssuesCount; miI++)
            {
                materialIssues.Add(new MaterialIssue(reader));
            }

            reader.Read(out comments2);
            reader.Read(out m_batchAmount);
            reader.Read(out m_nowFinishUtcTime);
        }
        else if (reader.VersionNumber >= 744)
        {
            m_bools = new BoolVector32(reader);

            int val;
            reader.Read(out val);
            productionStatus = (InternalActivityDefs.productionStatuses)val;
            reader.Read(out reportedSetupSpan);
            reader.Read(out reportedRunSpan);
            reader.Read(out reportedPostProcessingSpan);
            reader.Read(out reportedScrapQty);
            reader.Read(out reportedGoodQty);
            reader.Read(out bool finish);
            reader.Read(out reportedSpansAreIncremental); //new in 50
            reader.Read(out reportedQtiesAreIncremental); //new in 50
            reader.Read(out paused); //new in 53
            reader.Read(out val);
            peopleUsage = (InternalActivityDefs.peopleUsages)val;
            reader.Read(out nbrOfPeople);
            reader.Read(out comments);
            reader.Read(out reportedStartDateTicks);
            reader.Read(out reportedFinishDateTicks);
            reader.Read(out finishPredecessors);
            int materialIssuesCount;
            reader.Read(out materialIssuesCount);
            for (int miI = 0; miI < materialIssuesCount; miI++)
            {
                materialIssues.Add(new MaterialIssue(reader));
            }

            reader.Read(out comments2);
            reader.Read(out m_batchAmount);
        }
        else if (reader.VersionNumber >= 716)
        {
            m_bools = new BoolVector32(reader);

            int val;
            reader.Read(out val);
            productionStatus = (InternalActivityDefs.productionStatuses)val;
            reader.Read(out reportedSetupSpan);
            reader.Read(out reportedRunSpan);
            reader.Read(out reportedPostProcessingSpan);
            reader.Read(out reportedScrapQty);
            reader.Read(out reportedGoodQty);
            reader.Read(out bool finish);
            reader.Read(out reportedSpansAreIncremental); //new in 50
            reader.Read(out reportedQtiesAreIncremental); //new in 50
            reader.Read(out paused); //new in 53
            reader.Read(out val);
            peopleUsage = (InternalActivityDefs.peopleUsages)val;
            reader.Read(out nbrOfPeople);
            reader.Read(out comments);
            reader.Read(out reportedStartDateTicks);
            reader.Read(out reportedFinishDateTicks);
            reader.Read(out finishPredecessors);
            int materialIssuesCount;
            reader.Read(out materialIssuesCount);
            for (int miI = 0; miI < materialIssuesCount; miI++)
            {
                materialIssues.Add(new MaterialIssue(reader));
            }

            reader.Read(out comments2);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        a_writer.Write((int)productionStatus);
        a_writer.Write(reportedSetupSpan);
        a_writer.Write(reportedRunSpan);
        a_writer.Write(reportedPostProcessingSpan);
        a_writer.Write(m_reportedCleanSpan);
        a_writer.Write(m_reportedStorageSpan); //new in 12522
        a_writer.Write(m_reportedCleanOutGrade);
        a_writer.Write(reportedScrapQty);
        a_writer.Write(reportedGoodQty);
        a_writer.Write(reportedSpansAreIncremental); //new in 50
        a_writer.Write(reportedQtiesAreIncremental); //new in 50
        a_writer.Write(paused); //new in 53
        a_writer.Write((int)peopleUsage);
        a_writer.Write(nbrOfPeople);
        a_writer.Write(comments);
        a_writer.Write(reportedStartDateTicks);
        a_writer.Write(m_reportedProcessingStartTicks);
        a_writer.Write(m_reportedProcessingEndTicks);
        a_writer.Write(reportedFinishDateTicks);
        a_writer.Write(m_reportedEndOfPostProcessingTicks);
        a_writer.Write(m_reportedEndOfStorageTicks);
        a_writer.Write(finishPredecessors);
        a_writer.Write(materialIssues.Count);
        for (int miI = 0; miI < materialIssues.Count; miI++)
        {
            materialIssues[miI].Serialize(a_writer);
        }

        a_writer.Write(comments2);
        a_writer.Write(m_batchAmount);
        a_writer.Write(m_nowFinishUtcTime);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public InternalActivityFinishT() { }

    public InternalActivityFinishT(BaseId scenarioId,
                                   BaseId jobId,
                                   BaseId manufacturingOrderId,
                                   BaseId operationId,
                                   BaseId activityId)
        : base(scenarioId,
            jobId,
            manufacturingOrderId,
            operationId,
            activityId)
    {
        this.activityId = activityId;
        m_nowFinishUtcTime = DateTime.UtcNow;
    }

    public InternalActivityFinishT(BaseId scenarioId,
                                   BaseId jobId,
                                   BaseId manufacturingOrderId,
                                   BaseId operationId,
                                   BaseId activityId,
                                   decimal a_activityNbrOfPeople,
                                   InternalActivityDefs.peopleUsages a_activityPeopleUsage,
                                   bool a_actActivityManualUpdateOnly)
        : base(scenarioId,
            jobId,
            manufacturingOrderId,
            operationId,
            activityId)
    {
        this.activityId = activityId;
        nbrOfPeople = a_activityNbrOfPeople;
        peopleUsage = a_activityPeopleUsage;
        ActivityManualUpdateOnly = a_actActivityManualUpdateOnly;
        m_nowFinishUtcTime = DateTime.UtcNow;
    }

    #region Bools
    private BoolVector32 m_bools;
    private const int PeopleUsageSetIdx = 0;
    private const int NbrOfPeopleSetIdx = 1;
    private const short c_allocateMaterialFromOnHand = 2;
    private const short c_releaseProductToWarehouse = 3;
    private const short c_activityManualUpdateOnlyIdx = 4;

    public bool NbrOfPeopleIsSet => m_bools[NbrOfPeopleSetIdx];

    public bool PeopleUsageIsSet => m_bools[PeopleUsageSetIdx];
    #endregion Bools

    private InternalActivityDefs.productionStatuses productionStatus;

    public InternalActivityDefs.productionStatuses ProductionStatus
    {
        get => productionStatus;
        set => productionStatus = value;
    }

    private bool paused;

    public bool Paused
    {
        get => paused;
        set => paused = value;
    }

    private bool finishPredecessors;

    /// <summary>
    /// If true then finish predecessor Operations at expected values.
    /// </summary>
    public bool FinishPredecessors
    {
        get => finishPredecessors;
        set => finishPredecessors = value;
    }

    private InternalActivityDefs.peopleUsages peopleUsage = InternalActivityDefs.peopleUsages.UseAllAvailable;

    /// <summary>
    /// Determines how many people are allocated to an Activity in the schedule.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public InternalActivityDefs.peopleUsages PeopleUsage
    {
        get => peopleUsage;
        set
        {
            peopleUsage = value;
            m_bools[PeopleUsageSetIdx] = true;
        }
    }

    private decimal nbrOfPeople = 1;

    /// <summary>
    /// If PeopleUsage is set to UseSpecifiedNbr then this is the maximum number of people that will be allocated to the Activity.
    /// Fewer than this number will be allocated during time periods over which the Primary Resource's Nbr Of People is less than this value.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal NbrOfPeople
    {
        get => nbrOfPeople;
        set
        {
            if (value <= 0)
            {
                throw new PTValidationException("2050");
            }

            nbrOfPeople = value;
            m_bools[NbrOfPeopleSetIdx] = true;
        }
    }

    private string comments;

    /// <summary>
    /// Text that can be entered by operators or loaded from bar code systems.
    /// </summary>
    public string Comments
    {
        get => comments;
        set => comments = value;
    }

    private string comments2;

    /// <summary>
    /// Text that can be entered by operators or loaded from bar code systems.
    /// </summary>
    public string Comments2
    {
        get => comments2;
        set => comments2 = value;
    }

    private TimeSpan reportedSetupSpan;

    public TimeSpan ReportedSetupSpan
    {
        get => reportedSetupSpan;
        set => reportedSetupSpan = value;
    }

    private TimeSpan reportedRunSpan;

    public TimeSpan ReportedRunSpan
    {
        get => reportedRunSpan;
        set => reportedRunSpan = value;
    }

    private TimeSpan reportedPostProcessingSpan;

    public TimeSpan ReportedPostProcessingSpan
    {
        get => reportedPostProcessingSpan;
        set => reportedPostProcessingSpan = value;
    }
    private TimeSpan m_reportedStorageSpan;

    public TimeSpan ReportedStorageSpan
    {
        get => m_reportedStorageSpan;
        set => m_reportedStorageSpan = value;
    }

    private TimeSpan m_reportedCleanSpan;
    /// <summary>
    /// Clean time reported to have been spent so far in ticks.
    /// </summary>
    public TimeSpan ReportedCleanSpan
    {
        get => m_reportedCleanSpan;
        set => m_reportedCleanSpan = value;
    }


    private int m_reportedCleanOutGrade;
    public int ReportedCleanoutGrade
    {
        get => m_reportedCleanOutGrade;
        set => m_reportedCleanOutGrade = value;
    }

    private decimal reportedScrapQty;

    public decimal ReportedScrapQty
    {
        get => reportedScrapQty;
        set => reportedScrapQty = value;
    }

    private decimal reportedGoodQty;

    public decimal ReportedGoodQty
    {
        get => reportedGoodQty;
        set => reportedGoodQty = value;
    }

    private bool reportedSpansAreIncremental;

    /// <summary>
    /// If true then all Reported Spans are added to the current total for the Activity.  Otherwise, they are treated as replacement values.
    /// </summary>
    public bool ReportedSpansAreIncremental
    {
        get => reportedSpansAreIncremental;
        set => reportedSpansAreIncremental = value;
    }

    private bool reportedQtiesAreIncremental;

    /// <summary>
    /// If true then all Reported Quantities are added to the current total for the Activity.  Otherwise, they are treated as replacement values.
    /// </summary>
    public bool ReportedQtiesAreIncremental
    {
        get => reportedQtiesAreIncremental;
        set => reportedQtiesAreIncremental = value;
    }

    private long reportedFinishDateTicks;

    /// <summary>
    /// The default value is the current time.
    /// </summary>
    public long ReportedFinishDateTicks
    {
        get => reportedFinishDateTicks;
        set => reportedFinishDateTicks = value;
    }

    private long reportedStartDateTicks;

    /// <summary>
    /// The default value is the current time.
    /// </summary>
    public long ReportedStartDateTicks
    {
        get => reportedStartDateTicks;
        set => reportedStartDateTicks = value;
    }
    
    private long m_reportedEndOfPostProcessingTicks;

    /// <summary>
    /// The default value is the current time.
    /// </summary>
    public long ReportedEndOfPostProcessingTicks
    {
        get => m_reportedEndOfPostProcessingTicks;
        set => m_reportedEndOfPostProcessingTicks = value;
    }
    private long m_reportedEndOfStorageTicks;

    /// <summary>
    /// The default value is the current time.
    /// </summary>
    public long ReportedEndOfStorageTicks
    {
        get => m_reportedEndOfStorageTicks;
        set => m_reportedEndOfStorageTicks = value;
    }

    private long m_reportedProcessingStartTicks;
    /// <summary>
    /// The default value is the current time.
    /// </summary>
    public long ReportedProcessingStartDateTicks
    {
        get => m_reportedProcessingStartTicks;
        set => m_reportedProcessingStartTicks = value;
    }

    private long m_reportedProcessingEndTicks;
    /// <summary>
    /// The default value is the current time.
    /// </summary>
    public long ReportedProcessingEndDateTicks
    {
        get => m_reportedProcessingEndTicks;
        set => m_reportedProcessingEndTicks = value;
    }


    private decimal m_batchAmount;

    public decimal BatchAmount
    {
        get => m_batchAmount;
        set => m_batchAmount = value;
    }

    private readonly List<MaterialIssue> materialIssues = new ();

    public List<MaterialIssue> MaterialIssues => materialIssues;

    public bool AllocateMaterialFromOnHand
    {
        get => m_bools[c_allocateMaterialFromOnHand];
        set => m_bools[c_allocateMaterialFromOnHand] = value;
    }

    public bool ReleaseProductToWarehouse
    {
        get => m_bools[c_releaseProductToWarehouse];
        set => m_bools[c_releaseProductToWarehouse] = value;
    }

    public bool ActivityManualUpdateOnly
    {
        get => m_bools[c_activityManualUpdateOnlyIdx];
        set => m_bools[c_activityManualUpdateOnlyIdx] = value;
    }

    public override string Description => "Activity Status updated";

    private readonly DateTime m_nowFinishUtcTime;

    /// <summary>
    /// The default value is the current time.
    /// </summary>
    public DateTime NowFinishUtcTime => m_nowFinishUtcTime;
}