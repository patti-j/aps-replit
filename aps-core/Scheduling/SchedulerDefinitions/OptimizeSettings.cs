using PT.APSCommon;
using PT.Common.IO;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Stores the user-specific settings for an Optimize.
/// </summary>
public class OptimizeSettings : ICloneable, IPTSerializable, ISetBoolsInitializer
{
    public const int UNIQUE_ID = 181;

    #region IPTSerializable Members
    public OptimizeSettings(IReader reader)
    {
        //NOTE:  When adding new fields remember to add them to the Equals() method override below too!!
        if (reader.VersionNumber >= 13005)
        {
            m_bools = new BoolVector32(reader);
            m_optimizeSetBools = new BoolVector32(reader);
            m_mrpSetBools = new BoolVector32(reader);

            int val;
            reader.Read(out val);
            m_startTime = (ETimePoints)val;
            reader.Read(out val);
            resourceScope = (resourceScopes)val;
            reader.Read(out excludeDrums);
            reader.Read(out excludeNonDrums);
            reader.Read(out excludeUnscheduledJobs);
            reader.Read(out excludeOnHoldJobs);
            reader.Read(out excludePlannedJobs);
            reader.Read(out excludeEstimateJobs);
            reader.Read(out onlyMyResources);
            reader.Read(out val);
            dispatcherSource = (dispatcherSources)val;
            reader.Read(out jitSlackTicks);
            reader.Read(out allowActivityReassignment);
            reader.Read(out enforceConWipLimits);
            reader.Read(out m_specificStartTime);

            plantToInclude = new BaseId(reader);
            globalDispatcherId = new BaseId(reader);

            reader.Read(out val);
            m_mrpCutOff = (EMrpCutOff)val;
            reader.Read(out m_mrpCutOffDuration);

            reader.Read(out short setSubJobNeedDatePoint);
            SetSubJobNeedDatePointMrp = (ScenarioOptions.ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;

            reader.Read(out short mrpStartType);
            m_mrpStartType = (EMrpStartType)mrpStartType;
            reader.Read(out m_mrpSpecificStartDate);

            reader.Read(out short endTimeVal);
            m_endTime = (ETimePoints)endTimeVal;
            reader.Read(out m_specificEndTime);
            m_limitToResources = new BaseIdList(reader);
        }
        else if (reader.VersionNumber >= 12204)
        {
            m_bools = new BoolVector32(reader);
            m_optimizeSetBools = new BoolVector32(reader);
            m_mrpSetBools = new BoolVector32(reader);

            int val;
            reader.Read(out val);
            m_startTime = (ETimePoints)val;
            reader.Read(out val);
            resourceScope = (resourceScopes)val;
            reader.Read(out excludeDrums);
            reader.Read(out excludeNonDrums);
            reader.Read(out excludeUnscheduledJobs);
            reader.Read(out excludeOnHoldJobs);
            reader.Read(out excludePlannedJobs);
            reader.Read(out excludeEstimateJobs);
            reader.Read(out onlyMyResources);
            reader.Read(out val);
            dispatcherSource = (dispatcherSources)val;
            reader.Read(out int _);
            reader.Read(out jitSlackTicks);
            reader.Read(out allowActivityReassignment);
            reader.Read(out enforceConWipLimits);
            reader.Read(out m_specificStartTime);

            plantToInclude = new BaseId(reader);
            globalDispatcherId = new BaseId(reader);

            reader.Read(out val);
            m_mrpCutOff = (EMrpCutOff)val;
            reader.Read(out m_mrpCutOffDuration);

            reader.Read(out short setSubJobNeedDatePoint);
            SetSubJobNeedDatePointMrp = (ScenarioOptions.ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;

            reader.Read(out short mrpStartType);
            m_mrpStartType = (EMrpStartType)mrpStartType;
            reader.Read(out m_mrpSpecificStartDate);

            reader.Read(out short endTimeVal);
            m_endTime = (ETimePoints)endTimeVal;
            reader.Read(out m_specificEndTime);
            m_limitToResources = new BaseIdList(reader);
        }
        else if (reader.VersionNumber >= 12057)
        {
            m_bools = new BoolVector32(reader);
            m_optimizeSetBools = new BoolVector32(reader);
            m_mrpSetBools = new BoolVector32(reader);

            int val;
            reader.Read(out val);
            m_startTime = (ETimePoints)val;
            reader.Read(out val);
            resourceScope = (resourceScopes)val;
            reader.Read(out excludeDrums);
            reader.Read(out excludeNonDrums);
            reader.Read(out excludeUnscheduledJobs);
            reader.Read(out excludeOnHoldJobs);
            reader.Read(out excludePlannedJobs);
            reader.Read(out excludeEstimateJobs);
            reader.Read(out onlyMyResources);
            reader.Read(out val);
            dispatcherSource = (dispatcherSources)val;
            reader.Read(out int _);
            reader.Read(out jitSlackTicks);
            reader.Read(out allowActivityReassignment);
            reader.Read(out enforceConWipLimits);
            reader.Read(out m_specificStartTime);

            plantToInclude = new BaseId(reader);
            globalDispatcherId = new BaseId(reader);

            reader.Read(out val);
            m_mrpCutOff = (EMrpCutOff)val;
            reader.Read(out m_mrpCutOffDuration);

            reader.Read(out short setSubJobNeedDatePoint);
            SetSubJobNeedDatePointMrp = (ScenarioOptions.ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;

            reader.Read(out short mrpStartType);
            m_mrpStartType = (EMrpStartType)mrpStartType;
            reader.Read(out m_mrpSpecificStartDate);
        }
        else if (reader.VersionNumber >= 12031)
        {
            m_bools = new BoolVector32(reader);
            m_optimizeSetBools = new BoolVector32(reader);
            m_mrpSetBools = new BoolVector32(reader);

            int val;
            reader.Read(out val);
            m_startTime = (ETimePoints)val;
            reader.Read(out val);
            resourceScope = (resourceScopes)val;
            reader.Read(out excludeDrums);
            reader.Read(out excludeNonDrums);
            reader.Read(out excludeUnscheduledJobs);
            reader.Read(out excludeOnHoldJobs);
            reader.Read(out excludePlannedJobs);
            reader.Read(out excludeEstimateJobs);
            reader.Read(out onlyMyResources);
            reader.Read(out val);
            dispatcherSource = (dispatcherSources)val;
            reader.Read(out int _);
            reader.Read(out jitSlackTicks);
            reader.Read(out allowActivityReassignment);
            reader.Read(out enforceConWipLimits);
            reader.Read(out m_specificStartTime);

            plantToInclude = new BaseId(reader);
            globalDispatcherId = new BaseId(reader);

            reader.Read(out val);
            m_mrpCutOff = (EMrpCutOff)val;
            reader.Read(out m_mrpCutOffDuration);

            reader.Read(out short setSubJobNeedDatePoint);
            SetSubJobNeedDatePointMrp = (ScenarioOptions.ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;
        }
        else if (reader.VersionNumber >= 12000) // 696 reader for backwards compatibility in older v12 builds.
        {
            int val;
            reader.Read(out val);
            m_startTime = (ETimePoints)val;
            reader.Read(out val);
            resourceScope = (resourceScopes)val;
            reader.Read(out excludeDrums);
            reader.Read(out excludeNonDrums);
            reader.Read(out excludeUnscheduledJobs);
            reader.Read(out excludeOnHoldJobs);
            reader.Read(out excludePlannedJobs);
            reader.Read(out excludeEstimateJobs);
            reader.Read(out onlyMyResources);
            reader.Read(out val);
            dispatcherSource = (dispatcherSources)val;
            reader.Read(out int _);
            reader.Read(out jitSlackTicks);
            reader.Read(out allowActivityReassignment);
            reader.Read(out enforceConWipLimits);
            reader.Read(out m_specificStartTime);

            plantToInclude = new BaseId(reader);
            globalDispatcherId = new BaseId(reader);

            m_bools = new BoolVector32(reader);
            reader.Read(out val);
            m_mrpCutOff = (EMrpCutOff)val;
            reader.Read(out m_mrpCutOffDuration);

            reader.Read(out short setSubJobNeedDatePoint);
            SetSubJobNeedDatePointMrp = (ScenarioOptions.ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;
        }

        #region 756
        else if (reader.VersionNumber >= 757)
        {
            int val;
            reader.Read(out val);
            m_startTime = (ETimePoints)val;
            reader.Read(out val);
            resourceScope = (resourceScopes)val;
            reader.Read(out excludeDrums);
            reader.Read(out excludeNonDrums);
            reader.Read(out excludeUnscheduledJobs);
            reader.Read(out excludeOnHoldJobs);
            reader.Read(out excludePlannedJobs);
            reader.Read(out excludeEstimateJobs);
            reader.Read(out onlyMyResources);
            reader.Read(out val);
            dispatcherSource = (dispatcherSources)val;
            reader.Read(out int _);
            reader.Read(out jitSlackTicks);
            reader.Read(out allowActivityReassignment);
            reader.Read(out enforceConWipLimits);
            reader.Read(out m_specificStartTime);

            plantToInclude = new BaseId(reader);
            globalDispatcherId = new BaseId(reader);

            m_bools = new BoolVector32(reader);
            reader.Read(out val);
            m_mrpCutOff = (EMrpCutOff)val;
            reader.Read(out m_mrpCutOffDuration);

            reader.Read(out short setSubJobNeedDatePoint);
            SetSubJobNeedDatePointMrp = (ScenarioOptions.ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;

            reader.Read(out short mrpStartType);
            m_mrpStartType = (EMrpStartType)mrpStartType;
            reader.Read(out m_mrpSpecificStartDate);
        }
        #endregion 756

        #region 696
        else if (reader.VersionNumber >= 696)
        {
            int val;
            reader.Read(out val);
            m_startTime = (ETimePoints)val;
            reader.Read(out val);
            resourceScope = (resourceScopes)val;
            reader.Read(out excludeDrums);
            reader.Read(out excludeNonDrums);
            reader.Read(out excludeUnscheduledJobs);
            reader.Read(out excludeOnHoldJobs);
            reader.Read(out excludePlannedJobs);
            reader.Read(out excludeEstimateJobs);
            reader.Read(out onlyMyResources);
            reader.Read(out val);
            dispatcherSource = (dispatcherSources)val;
            reader.Read(out int _);
            reader.Read(out jitSlackTicks);
            reader.Read(out allowActivityReassignment);
            reader.Read(out enforceConWipLimits);
            reader.Read(out m_specificStartTime);

            plantToInclude = new BaseId(reader);
            globalDispatcherId = new BaseId(reader);

            m_bools = new BoolVector32(reader);
            reader.Read(out val);
            m_mrpCutOff = (EMrpCutOff)val;
            reader.Read(out m_mrpCutOffDuration);

            reader.Read(out short setSubJobNeedDatePoint);
            SetSubJobNeedDatePointMrp = (ScenarioOptions.ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;
        }
        #endregion 696
    }

    public void Serialize(IWriter writer)
    {
        m_bools.Serialize(writer);
        m_optimizeSetBools.Serialize(writer);
        m_mrpSetBools.Serialize(writer);

        writer.Write((int)m_startTime);
        writer.Write((int)resourceScope);
        writer.Write(excludeDrums);
        writer.Write(excludeNonDrums);
        writer.Write(excludeUnscheduledJobs);
        writer.Write(excludeOnHoldJobs);
        writer.Write(excludePlannedJobs);
        writer.Write(excludeEstimateJobs);
        writer.Write(onlyMyResources);
        writer.Write((int)dispatcherSource);
        writer.Write(jitSlackTicks);
        writer.Write(allowActivityReassignment);
        writer.Write(enforceConWipLimits);
        writer.Write(m_specificStartTime);

        plantToInclude.Serialize(writer);
        globalDispatcherId.Serialize(writer);

        writer.Write((int)m_mrpCutOff);
        writer.Write(MrpCutoffDateDuration);
        writer.Write((short)SetSubJobNeedDatePointMrp);

        writer.Write((short)m_mrpStartType);
        writer.Write(m_mrpSpecificStartDate);

        writer.Write((short)m_endTime);
        writer.Write(m_specificEndTime);
        m_limitToResources.Serialize(writer);
    }

    public void InitializeSetBools()
    {
        m_mrpSetBools.Clear();
        m_optimizeSetBools.Clear();
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region Bool Vectors
    /// <summary>
    /// Optimize and compress IsSet Bools
    /// </summary>
    private BoolVector32 m_optimizeSetBools;

    private const int c_startTimeIsSetIdx = 0;
    private const int c_specificStartTimeIsSetIdx = 1;
    private const int c_resourceScopeIsSetIdx = 2;
    private const int c_plantToIncludeIsSetIdx = 3;
    private const int c_excludeDrumsIsSetIdx = 4;
    private const int c_excludeNonDrumsIsSetIdx = 5;
    private const int c_excludeUnscheduledJobsIsSetIdx = 6;
    private const int c_excludeOnHoldJobsIsSetIdx = 7;
    private const int c_excludePlannedJobsIsSetIdx = 8;
    private const int c_excludeEstimateJobsIsSetIdx = 9;
    private const int c_excludeNewJobsIsSetIdx = 10;
    private const int c_onlyMyResourcesIsSetIdx = 11;
    private const int c_dispatcherSourceIsSetIdx = 12;
    private const int c_globalDispatcherIsSetIdx = 13;
    // unused index 14;
    private const int c_jitSlackDaysIsSetIdx = 15;
    private const int c_mrpCutoffIsSetIdx = 16;
    private const int c_mrpCutoffDateDurationIsSetIdx = 17;
    private const int c_allowActivityReassignmentIsSetIdx = 18;
    private const int c_enforceWipLimitsIsSetIdx = 19;
    private const int c_useResourceCapacityForHeadStartIsSetIdx = 20;
    private const int c_endTimeIsSetIdx = 21;
    private const int c_specificEndTimeIsSetIdx = 22;
    private const int c_limitToResourcesIsSetIdx = 23;
    private const int c_combineHeadStartBuffersIsSetIdx = 24;

    /// <summary>
    /// MRP IsSet Bools
    /// </summary>
    private BoolVector32 m_mrpSetBools;

    private const int c_mrpSetNeedDateIsSetIdx = 0;
    private const int c_mrpUseJitDatesIsSetIdx = 1;
    private const int c_runMrpDuringOptimizationsIsSetIdx = 2;
    private const int c_regenerateJobsIsSetIdx = 3;
    private const int c_regeneratePurchaseOrdersIsSetIdx = 4;
    private const int c_preserveJobsInStableSpanIsSetIdx = 5;
    private const int c_preserveReleasedJobsIsSetIdx = 6;
    private const int c_preserveFirmJobsIsSetIdx = 7;
    private const int c_preservePlannedJobsIsSetIdx = 8;
    private const int c_preserveEstimateJobsIsSetIdx = 9;
    private const int c_preserveMrpGeneratedJobsIsSetIdx = 10;
    private const int c_preservePrintedJobsIsSetIdx = 11;
    private const int c_preserveCtpJobsIsSetIdx = 12;
    private const int c_preserveAnchoredJobsIsSetIdx = 13;
    private const int c_preserveLockedJobsIsSetIdx = 14;
    private const int c_obsoletemoBatchingByBatchGroupEnabledIsSetIdx = 15;
    private const int c_autoJoinManufacturingOrdersIsSetIdx = 16;
    private const int c_netChangeMrpIsSetIdx = 17;
    private const int c_mrpJitCompressIsSetIdx = 18;
    private const int c_mrpConsumeForecastsIsSetIdx = 19;
    private const int c_mrpStartTypeIsSetIdx = 20;
    private const int c_mrpSpecificStartDateIsSetIdx = 21;
    private const int c_sourceSupplyFromFirmedOrdersIsSetIdx = 22;
    private const int c_enableLotPeggingIsSetIdx = 23;
    #endregion

    public enum ETimePoints
    {
        CurrentPTClock = 0,
        EndOfFrozenZone,
        EndOfStableZone,
        SpecificDateTime,
        EndOfPlanningHorizon,
        EntireSchedule, //Including past planning horizon
        EndOfShortTerm
    }

    public enum resourceScopes
    {
        All = 0,

        /// <summary>
        /// Code that uses this should be converted to OnePlant and this enumeration value should be renamed to Unused. Product Backlog 3385 was created for this change.
        /// </summary>
        ConvertToOnePlant,
        OnePlant
    }

    public enum dispatcherSources
    {
        OneRule = 0,
        NormalRules,
        ExperimentalRuleOne,
        ExperimentalRuleTwo,
        ExperimentalRuleThree,
        ExperimentalRuleFour
    }

    public enum EMrpCutOff { ShortTerm = 0, PlanningHorizon, None, SpecificDuration }

    public enum EMrpStartType { FirstDemand = 0, Clock, ShortTerm, SpecificDate }

    public OptimizeSettings() { }

    private ETimePoints m_startTime = ETimePoints.EndOfFrozenZone;

    public ETimePoints StartTime
    {
        get => m_startTime;
        set
        {
            m_startTime = value;
            m_optimizeSetBools[c_startTimeIsSetIdx] = true;
        }
    }

    private ETimePoints m_endTime = ETimePoints.EndOfPlanningHorizon;

    public ETimePoints EndTime
    {
        get => m_endTime;
        set
        {
            m_endTime = value;
            m_optimizeSetBools[c_endTimeIsSetIdx] = true;
        }
    }

    private DateTime m_specificStartTime = PTDateTime.UtcNow.RemoveSeconds().ToDateTime();

    public DateTime SpecificStartTime
    {
        get => m_specificStartTime;
        set
        {
            m_specificStartTime = value;
            m_optimizeSetBools[c_specificStartTimeIsSetIdx] = true;
        }
    }

    private DateTime m_specificEndTime = PTDateTime.UtcNow.RemoveSeconds().ToDateTime();

    public DateTime SpecificEndTime
    {
        get => m_specificEndTime;
        set
        {
            m_specificEndTime = value;
            m_optimizeSetBools[c_specificEndTimeIsSetIdx] = true;
        }
    }

    private resourceScopes resourceScope = resourceScopes.All;

    public resourceScopes ResourceScope
    {
        get => resourceScope;
        set
        {
            resourceScope = value;
            m_optimizeSetBools[c_resourceScopeIsSetIdx] = true;
        }
    }

    private BaseId plantToInclude = BaseId.NULL_ID;

    public BaseId PlantToInclude
    {
        get => plantToInclude;
        set => plantToInclude = value;
    }

    private bool excludeDrums;

    /// <summary>
    /// Not used in Simulation.
    /// </summary>
    public bool ExcludeDrums
    {
        get => excludeDrums;
        set
        {
            excludeDrums = value;
            m_optimizeSetBools[c_excludeDrumsIsSetIdx] = true;
        }
    }

    private bool excludeNonDrums;

    /// <summary>
    /// Not used in simulation.
    /// </summary>
    public bool ExcludeNonDrums
    {
        get => excludeNonDrums;
        set
        {
            excludeNonDrums = value;
            m_optimizeSetBools[c_excludeNonDrumsIsSetIdx] = true;
        }
    }

    private bool excludeUnscheduledJobs;

    public bool ExcludeUnscheduledJobs
    {
        get => excludeUnscheduledJobs;
        set
        {
            excludeUnscheduledJobs = value;
            m_optimizeSetBools[c_excludeUnscheduledJobsIsSetIdx] = true;
        }
    }

    private bool excludeOnHoldJobs;

    public bool ExcludeOnHoldJobs
    {
        get => excludeOnHoldJobs;
        set
        {
            excludeOnHoldJobs = value;
            m_optimizeSetBools[c_excludeOnHoldJobsIsSetIdx] = true;
        }
    }

    private bool excludePlannedJobs;

    public bool ExcludePlannedJobs
    {
        get => excludePlannedJobs;
        set
        {
            excludePlannedJobs = value;
            m_optimizeSetBools[c_excludePlannedJobsIsSetIdx] = true;
        }
    }

    private bool excludeEstimateJobs;

    public bool ExcludeEstimateJobs
    {
        get => excludeEstimateJobs;
        set
        {
            excludeEstimateJobs = value;
            m_optimizeSetBools[c_excludeEstimateJobsIsSetIdx] = true;
        }
    }

    public bool ExcludeNewJobs
    {
        get => m_bools[ExcludeNewJobsIdx];
        set
        {
            m_bools[ExcludeNewJobsIdx] = value;
            m_optimizeSetBools[c_excludeNewJobsIsSetIdx] = true;
        }
    }

    private bool onlyMyResources;

    /// <summary>
    /// Not used in simulation.
    /// </summary>
    public bool OnlyMyResources
    {
        get => onlyMyResources;
        set
        {
            onlyMyResources = value;
            m_optimizeSetBools[c_onlyMyResourcesIsSetIdx] = true;
        }
    }

    private dispatcherSources dispatcherSource = dispatcherSources.NormalRules;

    public dispatcherSources DispatcherSource
    {
        get => dispatcherSource;
        set
        {
            dispatcherSource = value;
            m_optimizeSetBools[c_dispatcherSourceIsSetIdx] = true;
        }
    }

    private BaseId globalDispatcherId = new ("-1");

    public BaseId GlobalDispatcherId
    {
        get => globalDispatcherId;
        set
        {
            globalDispatcherId = value;
            m_optimizeSetBools[c_globalDispatcherIsSetIdx] = true;
        }
    }

    /// <summary>
    /// Not used by simulation, the ticks version is.
    /// </summary>
    public double JitSlackDays
    {
        get
        {
            TimeSpan jitSlackTS = new (JITSlackTicks);
            return jitSlackTS.TotalDays;
        }

        set
        {
            TimeSpan jitSlackTS = TimeSpan.FromDays(value);
            JITSlackTicks = jitSlackTS.Ticks;
            m_optimizeSetBools[c_jitSlackDaysIsSetIdx] = true;
        }
    }

    private long jitSlackTicks = TimeSpan.TicksPerDay * 7;

    public long JITSlackTicks
    {
        get => jitSlackTicks;
        private set => jitSlackTicks = value;
    }

    private EMrpCutOff m_mrpCutOff = EMrpCutOff.None;
    private TimeSpan m_mrpCutOffDuration = TimeSpan.FromDays(180);

    /// <summary>
    /// MRP ignores requirements after this period.
    /// </summary>
    public EMrpCutOff MrpCutoff
    {
        get => m_mrpCutOff;
        set
        {
            m_mrpCutOff = value;
            m_optimizeSetBools[c_mrpCutoffIsSetIdx] = true;
        }
    }

    /// <summary>
    /// MRP ignores requirements after this duration from the clock.
    /// </summary>
    public TimeSpan MrpCutoffDateDuration
    {
        get => m_mrpCutOffDuration;
        set
        {
            m_mrpCutOffDuration = value;
            m_optimizeSetBools[c_mrpCutoffDateDurationIsSetIdx] = true;
        }
    }

    private EMrpStartType m_mrpStartType = EMrpStartType.FirstDemand;
    private DateTime m_mrpSpecificStartDate = PTDateTime.MinDateTime;

    /// <summary>
    /// MRP ignores requirements after this period.
    /// </summary>
    public EMrpStartType MrpStartType
    {
        get => m_mrpStartType;
        set
        {
            m_mrpStartType = value;
            m_mrpSetBools[c_mrpStartTypeIsSetIdx] = true;
        }
    }

    /// <summary>
    /// MRP ignores requirements after this duration from the clock.
    /// </summary>
    public DateTime MrpSpecificStartDate
    {
        get => m_mrpSpecificStartDate;
        set
        {
            m_mrpSpecificStartDate = value;
            m_mrpSetBools[c_mrpSpecificStartDateIsSetIdx] = true;
        }
    }

    private bool allowActivityReassignment = true;

    /// <summary>
    /// Not used in simulation.
    /// </summary>
    public bool AllowActivityReassignment
    {
        get => allowActivityReassignment;
        set
        {
            allowActivityReassignment = value;
            m_optimizeSetBools[c_allowActivityReassignmentIsSetIdx] = true;
        }
    }

    private bool enforceConWipLimits;

    /// <summary>
    /// Not used in simulation.
    /// </summary>
    public bool EnforceConWipLimits
    {
        get => enforceConWipLimits;
        set
        {
            enforceConWipLimits = value;
            m_optimizeSetBools[c_enforceWipLimitsIsSetIdx] = true;
        }
    }

    private BoolVector32 m_bools;
    private const int RegenerateJobsIdx = 0;
    private const int PreserveJobsInStableSpanIdx = 1;
    private const int PreserveFirmJobsIdx = 2;
    private const int PreserveMrpGeneratedJobsIdx = 3;
    private const int PreserveAnchoredJobsIdx = 4;
    private const int PreserveLockedJobsIdx = 5;
    private const int c_enableLotPeggingIdx = 6;
    private const int PreservePrintedJobsIdx = 7;
    private const int PreserveCtpJobsIdx = 8;

    private const int obsoleteIdx = 9; //was MoBatchingByBatchGroupEnabledIdx
     
    //const int UseLegacyMRPProcessIdx = 10; // not used any more
    private const int RunMrpDuringOptimizationsIdx = 11;
    private const int RegeneratePurchaseOrdersIdx = 12;
    private const int PreservePlannedJobsIdx = 13;
    private const int PreserveEstimateJobsIdx = 14;

    private const int PreserveReleasedJobsIdx = 15;

    //const int DEPRECATED = 16;
    private const int AutoJoinManufacturingOrdersIdx = 17;
    private const int NetChangeMRPIdx = 18;
    private const int ExcludeNewJobsIdx = 19;
    private const int c_mrpJitCompress = 20;
    private const int c_mrpSetNeedDate = 21;
    private const int c_mrpUseJitDates = 22;
    private const int c_mrpConsumeForecasts = 23;
    private const int c_useResourceCapacityForHeadstartIdx = 24;
    private const int c_sourceSupplyFromFirmedOrdersIdx = 25;
    private const int c_combineHeadStartBuffersIdx = 26;

    /// <summary>
    /// If true, MRP will set sub component job dates based on parent jobs. This causes MRP to run an additional
    /// Optimize. Unlike normal MRP, the need dates will be set after material constraints have been turned on.
    /// </summary>
    public ScenarioOptions.ESubJobNeedDateResetPoint SetSubJobNeedDatePointMrp
    {
        get => m_setSubJobNeedDatePointMrp;
        set
        {
            m_setSubJobNeedDatePointMrp = value;
            m_mrpSetBools[c_mrpSetNeedDateIsSetIdx] = true;
        }
    }

    /// <summary>
    /// If true, MRP will use JIT Start Date as the adjustment date for Material Requirement demands, otherwise MRP will use
    /// ScheduledStartDate.
    /// </summary>
    public bool MrpUseJitDates
    {
        get => m_bools[c_mrpUseJitDates];
        set
        {
            m_bools[c_mrpUseJitDates] = value;
            m_mrpSetBools[c_mrpUseJitDatesIsSetIdx] = true;
        }
    }

    // whether to consume forecasts before MRP.
    public bool MrpConsumeForecasts
    {
        get => m_bools[c_mrpConsumeForecasts];
        set
        {
            m_bools[c_mrpConsumeForecasts] = value;
            m_mrpSetBools[c_mrpConsumeForecastsIsSetIdx] = true;
        }
    }

    public bool RunMrpDuringOptimizations
    {
        get => m_bools[RunMrpDuringOptimizationsIdx];
        set
        {
            m_bools[RunMrpDuringOptimizationsIdx] = value;
            m_mrpSetBools[c_runMrpDuringOptimizationsIsSetIdx] = true;
        }
    }

    public bool RegenerateJobs
    {
        get => m_bools[RegenerateJobsIdx];
        set
        {
            m_bools[RegenerateJobsIdx] = value;
            m_mrpSetBools[c_regenerateJobsIsSetIdx] = true;
        }
    }

    public bool RegeneratePurchaseOrders
    {
        get => m_bools[RegeneratePurchaseOrdersIdx];
        set
        {
            m_bools[RegeneratePurchaseOrdersIdx] = value;
            m_mrpSetBools[c_regeneratePurchaseOrdersIsSetIdx] = true;
        }
    }

    public bool PreserveJobsInStableSpan
    {
        get => m_bools[PreserveJobsInStableSpanIdx];
        set
        {
            m_bools[PreserveJobsInStableSpanIdx] = value;
            m_mrpSetBools[c_preserveJobsInStableSpanIsSetIdx] = true;
        }
    }

    public bool PreserveReleasedJobs
    {
        get => m_bools[PreserveReleasedJobsIdx];
        set
        {
            m_bools[PreserveReleasedJobsIdx] = value;
            m_mrpSetBools[c_preserveReleasedJobsIsSetIdx] = true;
        }
    }

    public bool PreserveFirmJobs
    {
        get => m_bools[PreserveFirmJobsIdx];
        set
        {
            m_bools[PreserveFirmJobsIdx] = value;
            m_mrpSetBools[c_preserveFirmJobsIsSetIdx] = true;
        }
    }

    public bool PreservePlannedJobs
    {
        get => m_bools[PreservePlannedJobsIdx];
        set
        {
            m_bools[PreservePlannedJobsIdx] = value;
            m_mrpSetBools[c_preservePlannedJobsIsSetIdx] = true;
        }
    }

    public bool PreserveEstimateJobs
    {
        get => m_bools[PreserveEstimateJobsIdx];
        set
        {
            m_bools[PreserveEstimateJobsIdx] = value;
            m_mrpSetBools[c_preserveEstimateJobsIsSetIdx] = true;
        }
    }

    public bool PreserveMrpGeneratedJobs
    {
        get => m_bools[PreserveMrpGeneratedJobsIdx];
        set
        {
            m_bools[PreserveMrpGeneratedJobsIdx] = value;
            m_mrpSetBools[c_preserveMrpGeneratedJobsIsSetIdx] = true;
        }
    }

    public bool PreservePrintedJobs
    {
        get => m_bools[PreservePrintedJobsIdx];
        set
        {
            m_bools[PreservePrintedJobsIdx] = value;
            m_mrpSetBools[c_preservePrintedJobsIsSetIdx] = true;
        }
    }

    public bool PreserveCtpJobs
    {
        get => m_bools[PreserveCtpJobsIdx];
        set
        {
            m_bools[PreserveCtpJobsIdx] = value;
            m_mrpSetBools[c_preserveCtpJobsIsSetIdx] = true;
        }
    }

    public bool PreserveAnchoredJobs
    {
        get => m_bools[PreserveAnchoredJobsIdx];
        set
        {
            m_bools[PreserveAnchoredJobsIdx] = value;
            m_mrpSetBools[c_preserveAnchoredJobsIsSetIdx] = true;
        }
    }

    public bool PreserveLockedJobs
    {
        get => m_bools[PreserveLockedJobsIdx];
        set
        {
            m_bools[PreserveLockedJobsIdx] = value;
            m_mrpSetBools[c_preserveLockedJobsIsSetIdx] = true;
        }
    }
    
    public bool NetChangeMRP
    {
        get => m_bools[NetChangeMRPIdx];
        set
        {
            m_bools[NetChangeMRPIdx] = value;
            m_mrpSetBools[c_netChangeMrpIsSetIdx] = true;
        }
    }

    public bool MrpJitCompress
    {
        get => m_bools[c_mrpJitCompress];
        set
        {
            m_bools[c_mrpJitCompress] = value;
            m_mrpSetBools[c_mrpJitCompressIsSetIdx] = true;
        }
    }

    public bool AutoJoinMOs
    {
        get => m_bools[AutoJoinManufacturingOrdersIdx];
        set
        {
            m_bools[AutoJoinManufacturingOrdersIdx] = value;
            m_mrpSetBools[c_autoJoinManufacturingOrdersIsSetIdx] = true;
        }
    }

    public bool UseResourceCapacityForHeadstart
    {
        get => m_bools[c_useResourceCapacityForHeadstartIdx];
        set
        {
            m_bools[c_useResourceCapacityForHeadstartIdx] = value;
            m_optimizeSetBools[c_useResourceCapacityForHeadStartIsSetIdx] = true;
        }
    }

    private BaseIdList m_limitToResources = new ();
    private ScenarioOptions.ESubJobNeedDateResetPoint m_setSubJobNeedDatePointMrp;

    public BaseIdList LimitToResources
    {
        get => m_limitToResources;
        set
        {
            m_limitToResources = value;
            m_optimizeSetBools[c_limitToResourcesIsSetIdx] = true;
        }
    }

    public bool CombineHeadStartBuffers
    {
        get => m_bools[c_combineHeadStartBuffersIdx];
        set
        {
            m_bools[c_combineHeadStartBuffersIdx] = value;
            m_optimizeSetBools[c_combineHeadStartBuffersIsSetIdx] = true;
        }
    }

    /// <summary>
    /// </summary>
    /// <returns>Whether the settings specify limited resources and this resource id is a limited resource.</returns>
    public bool ResourceInScope(BaseId a_id)
    {
        return m_limitToResources.Count == 0 || m_limitToResources.Contains(a_id);
    }

    public bool SourceFromFirmOrders
    {
        get => m_bools[c_sourceSupplyFromFirmedOrdersIdx];
        set
        {
            m_bools[c_sourceSupplyFromFirmedOrdersIdx] = value;
            m_mrpSetBools[c_sourceSupplyFromFirmedOrdersIsSetIdx] = true;
        }
    }    
    
    public bool EnableLotPegging
    {
        get => m_bools[c_enableLotPeggingIdx];
        set
        {
            m_bools[c_enableLotPeggingIdx] = value;
            m_mrpSetBools[c_enableLotPeggingIsSetIdx] = true;
        }
    }

    #region Optimize And Compress IsSet Properties
    public bool StartTimeIsSet => m_optimizeSetBools[c_startTimeIsSetIdx];

    public bool EndTimeIsSet => m_optimizeSetBools[c_endTimeIsSetIdx];

    public bool SpecificStartTimeIsSet => m_optimizeSetBools[c_specificStartTimeIsSetIdx];

    public bool SpecificEndTimeIsSet => m_optimizeSetBools[c_specificEndTimeIsSetIdx];

    public bool ResourcesScopeIsSet => m_optimizeSetBools[c_resourceScopeIsSetIdx];

    public bool PlantToIncludeIsSet => m_optimizeSetBools[c_plantToIncludeIsSetIdx];

    public bool ExcludeDrumsIsSet => m_optimizeSetBools[c_excludeDrumsIsSetIdx];

    public bool ExclueNonDrumsIsSet => m_optimizeSetBools[c_excludeNonDrumsIsSetIdx];

    public bool ExcludeUnscheduledJobsIsSet => m_optimizeSetBools[c_excludeUnscheduledJobsIsSetIdx];

    public bool ExcludeOnHoldJobsIsSet => m_optimizeSetBools[c_excludeOnHoldJobsIsSetIdx];

    public bool ExcludePlannedJobsIsSet
    {
        get => m_optimizeSetBools[c_excludePlannedJobsIsSetIdx];
        set => m_optimizeSetBools[c_excludePlannedJobsIsSetIdx] = value;
    }

    public bool ExcludeEstimateJobsIsSet => m_optimizeSetBools[c_excludeEstimateJobsIsSetIdx];

    public bool ExcludeNewJobsIsSet => m_optimizeSetBools[c_excludeNewJobsIsSetIdx];

    public bool OnlyMyResourcesIsSet => m_optimizeSetBools[c_onlyMyResourcesIsSetIdx];

    public bool DispatcherSourcesIsSet => m_optimizeSetBools[c_dispatcherSourceIsSetIdx];

    public bool GlobalDispatcherIsSet => m_optimizeSetBools[c_globalDispatcherIsSetIdx];

    public bool JitSlackDaysIsSet => m_optimizeSetBools[c_jitSlackDaysIsSetIdx];

    public bool MrpCutoffIsSet => m_optimizeSetBools[c_mrpCutoffIsSetIdx];

    public bool MrpCutoffDateDurationIsSet => m_optimizeSetBools[c_mrpCutoffDateDurationIsSetIdx];

    public bool AllowActivityReassignmentIsSet => m_optimizeSetBools[c_allowActivityReassignmentIsSetIdx];

    public bool EnforceWipLimitsIsSet => m_optimizeSetBools[c_enforceWipLimitsIsSetIdx];

    public bool UseResourceCapacityForHeadStartIsSet => m_optimizeSetBools[c_useResourceCapacityForHeadStartIsSetIdx];

    public bool LimitToResourcesIsSet => m_optimizeSetBools[c_limitToResourcesIsSetIdx];
    public bool CombineHeadStartBuffersIsSet => m_optimizeSetBools[c_combineHeadStartBuffersIsSetIdx];
    #endregion

    #region MRP IsSet Properties
    public bool MrpSetNeedDateIsSet => m_mrpSetBools[c_mrpSetNeedDateIsSetIdx];

    public bool MrpUseJitDatesIsSet => m_mrpSetBools[c_mrpUseJitDatesIsSetIdx];

    public bool RunMrpDuringOptimizationsIsSet => m_mrpSetBools[c_runMrpDuringOptimizationsIsSetIdx];

    public bool RegenerateJobsIsSet => m_mrpSetBools[c_regenerateJobsIsSetIdx];

    public bool RegeneratePurchaseOrdersIsSet => m_mrpSetBools[c_regeneratePurchaseOrdersIsSetIdx];

    public bool PreserveJobsInStableSpanIsSet => m_mrpSetBools[c_preserveJobsInStableSpanIsSetIdx];

    public bool PreserveReleasedJobsIsSet => m_mrpSetBools[c_preserveReleasedJobsIsSetIdx];

    public bool PreserveFirmJobsIsSet => m_mrpSetBools[c_preserveFirmJobsIsSetIdx];

    public bool PreservePlannedJobsIsSet => m_mrpSetBools[c_preservePlannedJobsIsSetIdx];

    public bool PreserveEstimateJobsIsSet => m_mrpSetBools[c_preserveEstimateJobsIsSetIdx];

    public bool PreserveMrpGeneratedJobsIsSet => m_mrpSetBools[c_preserveMrpGeneratedJobsIsSetIdx];

    public bool PreservePrintedJobsIsSet => m_mrpSetBools[c_preservePrintedJobsIsSetIdx];

    public bool PreserveCtpJobsIsSet => m_mrpSetBools[c_preserveCtpJobsIsSetIdx];

    public bool PreserveAnchoredJobsIsSet => m_mrpSetBools[c_preserveAnchoredJobsIsSetIdx];

    public bool PreserveLockedJobsIsSet => m_mrpSetBools[c_preserveLockedJobsIsSetIdx];
    
    public bool AutoJoinManufacturingOrdersIsSet => m_mrpSetBools[c_autoJoinManufacturingOrdersIsSetIdx];

    public bool NetChangeMRPIsSet => m_mrpSetBools[c_netChangeMrpIsSetIdx];

    public bool MrpJitCompressIsSet => m_mrpSetBools[c_mrpJitCompressIsSetIdx];

    public bool MrpConsumeForecastsIsSet => m_mrpSetBools[c_mrpConsumeForecastsIsSetIdx];
    public bool MrpStartTypeIsSet => m_mrpSetBools[c_mrpStartTypeIsSetIdx];
    public bool MrpSpecificStartDateIsSet => m_mrpSetBools[c_mrpSpecificStartDateIsSetIdx];
    public bool SourceFromFirmOrdersIsSet => m_mrpSetBools[c_sourceSupplyFromFirmedOrdersIsSetIdx];
    public bool EnableLotPeggingIsSet => m_mrpSetBools[c_enableLotPeggingIsSetIdx];
    #endregion

    object ICloneable.Clone()
    {
        return Clone();
    }

    public OptimizeSettings Clone()
    {
        return (OptimizeSettings)MemberwiseClone();
    }

    public override bool Equals(object a_obj)
    {
        if (a_obj == null)
        {
            return false;
        }

        if (a_obj is OptimizeSettings o)
        {
            return m_startTime == o.m_startTime &&
                   resourceScope == o.resourceScope &&
                   excludeDrums == o.excludeDrums &&
                   excludeNonDrums == o.excludeNonDrums &&
                   excludeUnscheduledJobs == o.excludeUnscheduledJobs &&
                   excludeOnHoldJobs == o.excludeOnHoldJobs &&
                   excludePlannedJobs == o.excludePlannedJobs &&
                   excludeEstimateJobs == o.excludeEstimateJobs &&
                   onlyMyResources == o.onlyMyResources &&
                   dispatcherSource == o.dispatcherSource &&
                   JITSlackTicks == o.JITSlackTicks &&
                   allowActivityReassignment == o.allowActivityReassignment &&
                   enforceConWipLimits == o.enforceConWipLimits &&
                   m_specificStartTime == o.m_specificStartTime &&
                   plantToInclude == o.plantToInclude &&
                   globalDispatcherId == o.globalDispatcherId &&
                   MrpCutoff == o.MrpCutoff &&
                   MrpCutoffDateDuration == o.MrpCutoffDateDuration &&
                   AutoJoinMOs == o.AutoJoinMOs &&
                   SetSubJobNeedDatePointMrp == o.SetSubJobNeedDatePointMrp &&
                   m_bools.Equals(o.m_bools) &&
                   MrpStartType == o.MrpStartType &&
                   MrpSpecificStartDate == o.MrpSpecificStartDate &&
                   m_specificEndTime == o.m_specificEndTime &&
                   m_endTime == o.m_endTime &&
                   m_limitToResources == o.m_limitToResources &&
                   CombineHeadStartBuffers == o.CombineHeadStartBuffers; //This implements IEquatable
        }

        throw new ApplicationException("Invalid comparison between OptimizeSettings and another type");
    }

    [Obsolete("This method does nothing")]
    public void AdvancingClock(TimeSpan a_clockAdvanceBy) { }

    /// <summary>
    /// For Backwards compatibility with earlier version of V12.
    /// </summary>
    /// <param name="a_optimizeSetBools"></param>
    /// <param name="a_mrpSetBools"></param>
    public void SetIsSetBools(BoolVector32 a_optimizeSetBools, BoolVector32 a_mrpSetBools)
    {
        m_optimizeSetBools = a_optimizeSetBools;
        m_mrpSetBools = a_mrpSetBools;
    }

    public void SetBackwardsCompatibilityForStartEndTimes()
    {
        //This is required because OptimizeSettings are used for both optimize and compress.
        //Since Compress used the opposite start times before serialization 12204, we need to reverse them
        EndTime = StartTime;
        SpecificEndTime = SpecificStartTime;
        StartTime = ETimePoints.CurrentPTClock; //Set the normal default value
    }

    public void Update(OptimizeSettings a_optimizeSettings)
    {
        #region Optimize Settings
        if (a_optimizeSettings.StartTimeIsSet)
        {
            StartTime = a_optimizeSettings.StartTime;
        }

        if (a_optimizeSettings.SpecificStartTimeIsSet)
        {
            SpecificStartTime = a_optimizeSettings.SpecificStartTime;
        }

        if (a_optimizeSettings.ResourcesScopeIsSet)
        {
            ResourceScope = a_optimizeSettings.ResourceScope;
        }

        if (a_optimizeSettings.PlantToIncludeIsSet)
        {
            PlantToInclude = a_optimizeSettings.PlantToInclude;
        }

        if (a_optimizeSettings.ExcludeDrumsIsSet)
        {
            ExcludeDrums = a_optimizeSettings.ExcludeDrums;
        }

        if (a_optimizeSettings.ExclueNonDrumsIsSet)
        {
            ExcludeNonDrums = a_optimizeSettings.ExcludeNonDrums;
        }

        if (a_optimizeSettings.ExcludeUnscheduledJobsIsSet)
        {
            ExcludeUnscheduledJobs = a_optimizeSettings.ExcludeUnscheduledJobs;
        }

        if (a_optimizeSettings.ExcludeOnHoldJobsIsSet)
        {
            ExcludeOnHoldJobs = a_optimizeSettings.ExcludeOnHoldJobs;
        }

        if (a_optimizeSettings.ExcludePlannedJobsIsSet)
        {
            ExcludePlannedJobs = a_optimizeSettings.ExcludePlannedJobs;
        }

        if (a_optimizeSettings.ExcludeEstimateJobsIsSet)
        {
            ExcludeEstimateJobs = a_optimizeSettings.ExcludeEstimateJobs;
        }

        if (a_optimizeSettings.ExcludeNewJobsIsSet)
        {
            ExcludeNewJobs = a_optimizeSettings.ExcludeNewJobs;
        }

        if (a_optimizeSettings.OnlyMyResourcesIsSet)
        {
            OnlyMyResources = a_optimizeSettings.OnlyMyResources;
        }

        if (a_optimizeSettings.DispatcherSourcesIsSet)
        {
            DispatcherSource = a_optimizeSettings.DispatcherSource;
        }

        if (a_optimizeSettings.GlobalDispatcherIsSet)
        {
            GlobalDispatcherId = a_optimizeSettings.GlobalDispatcherId;
        }

        if (a_optimizeSettings.JitSlackDaysIsSet)
        {
            JitSlackDays = a_optimizeSettings.JitSlackDays;
        }

        if (a_optimizeSettings.UseResourceCapacityForHeadStartIsSet)
        {
            UseResourceCapacityForHeadstart = a_optimizeSettings.UseResourceCapacityForHeadstart;
        }

        if (a_optimizeSettings.AllowActivityReassignmentIsSet)
        {
            AllowActivityReassignment = a_optimizeSettings.AllowActivityReassignment;
        }

        if (a_optimizeSettings.EnforceWipLimitsIsSet)
        {
            EnforceConWipLimits = a_optimizeSettings.EnforceConWipLimits;
        }

        if (a_optimizeSettings.EndTimeIsSet)
        {
            EndTime = a_optimizeSettings.EndTime;
        }

        if (a_optimizeSettings.SpecificEndTimeIsSet)
        {
            SpecificEndTime = a_optimizeSettings.SpecificEndTime;
        }

        if (a_optimizeSettings.LimitToResourcesIsSet)
        {
            LimitToResources = a_optimizeSettings.LimitToResources;
        }

        if (a_optimizeSettings.CombineHeadStartBuffersIsSet)
        {
            CombineHeadStartBuffers = a_optimizeSettings.CombineHeadStartBuffers;
        }
        #endregion

        #region MRP Optimize Settings
        if (a_optimizeSettings.MrpSetNeedDateIsSet)
        {
            SetSubJobNeedDatePointMrp = a_optimizeSettings.SetSubJobNeedDatePointMrp;
        }

        if (a_optimizeSettings.MrpUseJitDatesIsSet)
        {
            MrpUseJitDates = a_optimizeSettings.MrpUseJitDates;
        }

        if (a_optimizeSettings.RegenerateJobsIsSet)
        {
            RegenerateJobs = a_optimizeSettings.RegenerateJobs;
        }

        if (a_optimizeSettings.RegeneratePurchaseOrdersIsSet)
        {
            RegeneratePurchaseOrders = a_optimizeSettings.RegeneratePurchaseOrders;
        }

        if (a_optimizeSettings.PreserveReleasedJobsIsSet)
        {
            PreserveReleasedJobs = a_optimizeSettings.PreserveReleasedJobs;
        }

        if (a_optimizeSettings.PreserveFirmJobsIsSet)
        {
            PreserveFirmJobs = a_optimizeSettings.PreserveFirmJobs;
        }

        if (a_optimizeSettings.PreservePlannedJobsIsSet)
        {
            PreservePlannedJobs = a_optimizeSettings.PreservePlannedJobs;
        }

        if (a_optimizeSettings.PreserveEstimateJobsIsSet)
        {
            PreserveEstimateJobs = a_optimizeSettings.PreserveEstimateJobs;
        }

        if (a_optimizeSettings.PreserveMrpGeneratedJobsIsSet)
        {
            PreserveMrpGeneratedJobs = a_optimizeSettings.PreserveMrpGeneratedJobs;
        }

        if (a_optimizeSettings.PreservePrintedJobsIsSet)
        {
            PreservePrintedJobs = a_optimizeSettings.PreservePrintedJobs;
        }

        if (a_optimizeSettings.PreserveJobsInStableSpanIsSet)
        {
            PreserveJobsInStableSpan = a_optimizeSettings.PreserveJobsInStableSpan;
        }

        if (a_optimizeSettings.PreserveCtpJobsIsSet)
        {
            PreserveCtpJobs = a_optimizeSettings.PreserveCtpJobs;
        }

        if (a_optimizeSettings.PreserveAnchoredJobsIsSet)
        {
            PreserveAnchoredJobs = a_optimizeSettings.PreserveAnchoredJobs;
        }

        if (a_optimizeSettings.PreserveLockedJobsIsSet)
        {
            PreserveLockedJobs = a_optimizeSettings.PreserveLockedJobs;
        }

        if (a_optimizeSettings.MrpCutoffIsSet)
        {
            MrpCutoff = a_optimizeSettings.MrpCutoff;
        }

        if (a_optimizeSettings.MrpCutoffDateDurationIsSet)
        {
            MrpCutoffDateDuration = a_optimizeSettings.MrpCutoffDateDuration;
        }

        if (a_optimizeSettings.NetChangeMRPIsSet)
        {
            NetChangeMRP = a_optimizeSettings.NetChangeMRP;
        }

        if (a_optimizeSettings.MrpJitCompressIsSet)
        {
            MrpJitCompress = a_optimizeSettings.MrpJitCompress;
        }

        if (a_optimizeSettings.MrpConsumeForecastsIsSet)
        {
            MrpConsumeForecasts = a_optimizeSettings.MrpConsumeForecasts;
        }

        if (a_optimizeSettings.MrpStartTypeIsSet)
        {
            MrpStartType = a_optimizeSettings.MrpStartType;
        }

        if (a_optimizeSettings.MrpSpecificStartDateIsSet)
        {
            MrpSpecificStartDate = a_optimizeSettings.MrpSpecificStartDate;
        }

        if (a_optimizeSettings.SourceFromFirmOrdersIsSet)
        {
            SourceFromFirmOrders = a_optimizeSettings.SourceFromFirmOrders;
        }        
        
        if (a_optimizeSettings.EnableLotPeggingIsSet)
        {
            EnableLotPegging = a_optimizeSettings.EnableLotPegging;
        }
        #endregion

        //Now that the settings have been updated, clear the isSetBools.
        InitializeSetBools();
    }
}