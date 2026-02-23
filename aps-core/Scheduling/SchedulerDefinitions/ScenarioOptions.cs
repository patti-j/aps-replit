using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.IO;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Contains settings that are common across all Scenarios.  They are set in ScenarioManager and kept in sync across Scenarios.
/// </summary>
public class ScenarioOptions : ICloneable, IPTDeserializable, ISetBoolsInitializer
{
    public const int UNIQUE_ID = 183;

    #region IPTSerializable Members
    public ScenarioOptions(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12537)
        {
            a_reader.Read(out int val);
            m_actionOnOperationBadCapability = (EActionOnOperationBadCapability)val;

            a_reader.Read(out m_keepFeasibleSpan);
            a_reader.Read(out m_maxHistoriesCount);
            a_reader.Read(out m_destructiveDbUpdate);
            a_reader.Read(out m_planningHorizon);
            a_reader.Read(out m_shortTermSpan);
            a_reader.Read(out m_fiscalYearEnd);

            a_reader.Read(out val);
            m_erpOverrideOfActivityUpdate = (EErpActivityUpdateOverrides)val;

            m_boolVector = new BoolVector32(a_reader);
            a_reader.Read(out m_precision);
            a_reader.Read(out m_jobLateThreshold);

            a_reader.Read(out m_trackActualsAgeLimit);
            a_reader.Read(out m_performingFastPercentage);
            a_reader.Read(out m_performingSlowPercentage);

            a_reader.Read(out val);
            m_unsatisfiableJobPathHandling = (EUnsatisfiableJobPathHandlingEnum)val;

            a_reader.Read(out m_simulationProgressReportFrequency);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out m_opLengthWarningLevel);
            a_reader.Read(out short setSubJobNeedDatePoint);
            m_setSubJobNeedDateResetPoint = (ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;

            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);
        }
        else if (a_reader.VersionNumber >= 12050)
        {
            a_reader.Read(out int val);
            m_actionOnOperationBadCapability = (EActionOnOperationBadCapability)val;

            a_reader.Read(out m_keepFeasibleSpan);
            a_reader.Read(out m_maxHistoriesCount);
            a_reader.Read(out m_destructiveDbUpdate);
            a_reader.Read(out m_planningHorizon);
            a_reader.Read(out m_shortTermSpan);
            a_reader.Read(out m_fiscalYearEnd);

            a_reader.Read(out val);
            m_erpOverrideOfActivityUpdate = (EErpActivityUpdateOverrides)val;

            m_boolVector = new BoolVector32(a_reader);
            a_reader.Read(out m_precision);
            a_reader.Read(out m_jobLateThreshold);

            a_reader.Read(out val);
            //m_timeSpanRoundingPlaces = (Rounding.EPlace)val;
            a_reader.Read(out m_trackActualsAgeLimit);
            a_reader.Read(out m_performingFastPercentage);
            a_reader.Read(out m_performingSlowPercentage);

            a_reader.Read(out val);
            m_unsatisfiableJobPathHandling = (EUnsatisfiableJobPathHandlingEnum)val;

            a_reader.Read(out m_simulationProgressReportFrequency);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out m_opLengthWarningLevel);
            a_reader.Read(out short setSubJobNeedDatePoint);
            m_setSubJobNeedDateResetPoint = (ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;

            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);
        }
        else if (a_reader.VersionNumber >= 12043)
        {
            int val;

            a_reader.Read(out val);
            m_actionOnOperationBadCapability = (EActionOnOperationBadCapability)val;

            a_reader.Read(out long m_frozenSpanTicks);
            LegacyFrozenSpan = new TimeSpan(m_frozenSpanTicks);
            a_reader.Read(out m_keepFeasibleSpan);
            a_reader.Read(out m_maxHistoriesCount);
            a_reader.Read(out m_destructiveDbUpdate);
            a_reader.Read(out m_planningHorizon);
            a_reader.Read(out m_shortTermSpan);
            a_reader.Read(out m_fiscalYearEnd);

            a_reader.Read(out val);
            m_erpOverrideOfActivityUpdate = (EErpActivityUpdateOverrides)val;

            m_boolVector = new BoolVector32(a_reader);
            a_reader.Read(out m_precision);
            a_reader.Read(out m_jobLateThreshold);

            a_reader.Read(out val);
            //m_timeSpanRoundingPlaces = (Rounding.EPlace)val;
            a_reader.Read(out m_trackActualsAgeLimit);
            a_reader.Read(out m_performingFastPercentage);
            a_reader.Read(out m_performingSlowPercentage);

            a_reader.Read(out val);
            m_unsatisfiableJobPathHandling = (EUnsatisfiableJobPathHandlingEnum)val;

            a_reader.Read(out m_simulationProgressReportFrequency);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out m_opLengthWarningLevel);
            a_reader.Read(out short setSubJobNeedDatePoint);
            m_setSubJobNeedDateResetPoint = (ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;

            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);
        }
        else if (a_reader.VersionNumber >= 12019)
        {
            int val;

            a_reader.Read(out val);
            m_actionOnOperationBadCapability = (EActionOnOperationBadCapability)val;

            a_reader.Read(out long m_frozenSpanTicks);
            LegacyFrozenSpan = new TimeSpan(m_frozenSpanTicks);
            a_reader.Read(out m_keepFeasibleSpan);
            a_reader.Read(out m_maxHistoriesCount);
            a_reader.Read(out m_destructiveDbUpdate);
            a_reader.Read(out m_planningHorizon);
            a_reader.Read(out m_shortTermSpan);
            a_reader.Read(out m_fiscalYearEnd);

            a_reader.Read(out val);
            m_erpOverrideOfActivityUpdate = (EErpActivityUpdateOverrides)val;

            m_boolVector = new BoolVector32(a_reader);
            a_reader.Read(out m_precision);
            a_reader.Read(out m_jobLateThreshold);

            a_reader.Read(out val);
            //m_timeSpanRoundingPlaces = (Rounding.EPlace)val;
            a_reader.Read(out m_trackActualsAgeLimit);
            a_reader.Read(out m_performingFastPercentage);
            a_reader.Read(out m_performingSlowPercentage);

            a_reader.Read(out val);
            m_unsatisfiableJobPathHandling = (EUnsatisfiableJobPathHandlingEnum)val;

            a_reader.Read(out m_simulationProgressReportFrequency);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out m_opLengthWarningLevel);
            a_reader.Read(out short setSubJobNeedDatePoint);
            m_setSubJobNeedDateResetPoint = (ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;
        }

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            int val;

            a_reader.Read(out val);
            m_actionOnOperationBadCapability = (EActionOnOperationBadCapability)val;

            a_reader.Read(out long m_frozenSpanTicks);
            LegacyFrozenSpan = new TimeSpan(m_frozenSpanTicks);
            a_reader.Read(out m_keepFeasibleSpan);
            a_reader.Read(out m_maxHistoriesCount);
            a_reader.Read(out m_destructiveDbUpdate);
            a_reader.Read(out m_planningHorizon);
            a_reader.Read(out m_shortTermSpan);
            a_reader.Read(out m_fiscalYearEnd);

            a_reader.Read(out val);
            m_erpOverrideOfActivityUpdate = (EErpActivityUpdateOverrides)val;

            a_reader.Read(out string m_scheduleReportCustomFieldGeneratorScript);
            m_boolVector = new BoolVector32(a_reader);
            a_reader.Read(out m_precision);
            a_reader.Read(out m_jobLateThreshold);

            a_reader.Read(out val);
            //m_timeSpanRoundingPlaces = (Rounding.EPlace)val;
            a_reader.Read(out m_trackActualsAgeLimit);
            a_reader.Read(out m_performingFastPercentage);
            a_reader.Read(out m_performingSlowPercentage);

            a_reader.Read(out val);
            m_unsatisfiableJobPathHandling = (EUnsatisfiableJobPathHandlingEnum)val;

            a_reader.Read(out m_simulationProgressReportFrequency);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out m_opLengthWarningLevel);
            a_reader.Read(out short setSubJobNeedDatePoint);
            m_setSubJobNeedDateResetPoint = (ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;
        }
        #endregion

        #region 736
        else if (a_reader.VersionNumber >= 736)
        {
            int val;

            a_reader.Read(out val);
            m_actionOnOperationBadCapability = (EActionOnOperationBadCapability)val;

            a_reader.Read(out long m_frozenSpanTicks);
            LegacyFrozenSpan = new TimeSpan(m_frozenSpanTicks);
            a_reader.Read(out m_keepFeasibleSpan);
            a_reader.Read(out m_maxHistoriesCount);
            a_reader.Read(out m_destructiveDbUpdate);
            a_reader.Read(out m_planningHorizon);
            a_reader.Read(out m_shortTermSpan);
            a_reader.Read(out m_fiscalYearEnd);

            a_reader.Read(out val);
            m_erpOverrideOfActivityUpdate = (EErpActivityUpdateOverrides)val;

            a_reader.Read(out string m_scheduleReportCustomFieldGeneratorScript);
            m_boolVector = new BoolVector32(a_reader);
            a_reader.Read(out m_precision);
            a_reader.Read(out m_jobLateThreshold);

            a_reader.Read(out val);
            //m_timeSpanRoundingPlaces = (Rounding.EPlace)val;
            a_reader.Read(out m_trackActualsAgeLimit);
            a_reader.Read(out m_performingFastPercentage);
            a_reader.Read(out m_performingSlowPercentage);

            a_reader.Read(out val);
            m_unsatisfiableJobPathHandling = (EUnsatisfiableJobPathHandlingEnum)val;

            a_reader.Read(out int passwordResetDays);
            a_reader.Read(out m_simulationProgressReportFrequency);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out m_opLengthWarningLevel);
            a_reader.Read(out short setSubJobNeedDatePoint);
            m_setSubJobNeedDateResetPoint = (ESubJobNeedDateResetPoint)setSubJobNeedDatePoint;
            a_reader.Read(out int maxNbrLogonAttempts);
            a_reader.Read(out TimeSpan autoLogoffTimeout);
            a_reader.Read(out string passwordRegEx);
        }
        #endregion
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((int)m_actionOnOperationBadCapability);
        a_writer.Write(m_keepFeasibleSpan);
        a_writer.Write(m_maxHistoriesCount);
        a_writer.Write(m_destructiveDbUpdate);

        a_writer.Write(m_planningHorizon);
        a_writer.Write(m_shortTermSpan);
        a_writer.Write(m_fiscalYearEnd);
        a_writer.Write((int)m_erpOverrideOfActivityUpdate);

        m_boolVector.Serialize(a_writer);

        a_writer.Write(m_precision);
        a_writer.Write(m_jobLateThreshold);

        a_writer.Write(m_trackActualsAgeLimit);
        a_writer.Write(m_performingFastPercentage);
        a_writer.Write(m_performingSlowPercentage);
        a_writer.Write((int)m_unsatisfiableJobPathHandling);

        a_writer.Write(m_simulationProgressReportFrequency);
        a_writer.Write(m_annualPercentageRate);
        a_writer.Write(m_opLengthWarningLevel);
        a_writer.Write((short)m_setSubJobNeedDateResetPoint);

        m_setBools.Serialize(a_writer);
        m_setBools2.Serialize(a_writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioOptions()
    {
        InitEnablementOptions();
    }

    public ScenarioOptions(
        EActionOnOperationBadCapability a_actionOnOperationBadCapability,
        TimeSpan a_opLengthWarningLevel,
        TimeSpan a_keepFeasibleSpan,
        TimeSpan a_planningHorizon,
        TimeSpan a_shortTermSpan,
        int a_maxHistoriesCount,
        bool a_destructiveUpdate,
        bool a_lockInFrozenZone,
        bool a_anchorInFrozenZone,
        bool a_commitOnAnchor,
        DateTime a_fiscalYearEnd,
        decimal a_apr,
        bool a_updateActivityFromErpEvenIfUpdatedManually,
        EErpActivityUpdateOverrides a_erpOverrideOfActivityUpdate,
        bool a_PreventErpJobChangesInStableSpan,
        bool a_PreventErpJobCancelInStableSpan,
        bool a_PreventErpJobChangesIfStarted,
        bool a_PreventErpJobCancelIfStarted,
        ESubJobNeedDateResetPoint a_SetSubJobNeedDates,
        bool a_SetSubJobPriorities,
        bool a_SetSubJobHotFlags,
        bool a_addPrefixToMrpJobs,
        bool a_AdjustNbrOrCyclesEnabled,
        bool a_JoinEnabled,
        bool a_ExpediteAfterSplit,
        bool a_SplitActivityEnabled,
        bool a_SplitJobEnabled,
        bool a_SplitMOEnabled,
        bool a_showMOBatchingTabOnOptimizeDialog,
        bool a_enforceMaxDelay,
        int a_Precision,
        TimeSpan a_JobLateThreshold,
        Rounding.EPlace a_timeSpanRoundingPlaces,
        TimeSpan a_trackActualsAgeLimit,
        int a_performingFastPercent,
        int a_performingSlowPercent,
        bool a_calculateBalancedCompositeScore,
        double a_simulationProgressFrequency,
        bool a_recalculateJITOnOptimizeEnabled,
        EUnsatisfiableJobPathHandlingEnum a_unsatisfiableJobPathHandling,
        bool a_allowChangeOfMaterialSuccessorSequenceOnMove,
        bool a_restoreMaterialConstraintsOnOptimizedActivities)
    {
        ActionOnOperationBadCapability = a_actionOnOperationBadCapability;
        OpLengthWarningLevel = a_opLengthWarningLevel;
        m_keepFeasibleSpan = a_keepFeasibleSpan;
        m_planningHorizon = a_planningHorizon;
        m_shortTermSpan = a_shortTermSpan;

        m_maxHistoriesCount = a_maxHistoriesCount;
        m_destructiveDbUpdate = a_destructiveUpdate;
        LockInFrozenZone = a_lockInFrozenZone;
        AnchorInFrozenZone = a_anchorInFrozenZone;
        CommitOnAnchor = a_commitOnAnchor;
        FiscalYearEnd = a_fiscalYearEnd;
        AnnualPercentageRate = a_apr;

        UpdateActivityFromErpEvenIfUpdatedManually = a_updateActivityFromErpEvenIfUpdatedManually;
        m_erpOverrideOfActivityUpdate = a_erpOverrideOfActivityUpdate;

        PreventErpJobChangesInStableSpan = a_PreventErpJobChangesInStableSpan;
        PreventErpJobCancelInStableSpan = a_PreventErpJobCancelInStableSpan;

        PreventErpJobChangesIfStarted = a_PreventErpJobChangesIfStarted;
        PreventErpJobCancelIfStarted = a_PreventErpJobCancelIfStarted;

        SetSubJobNeedDatePoint = a_SetSubJobNeedDates;
        SetSubJobPriorities = a_SetSubJobPriorities;
        SetSubJobHotFlags = a_SetSubJobHotFlags;
        AddPrefixToMrpJobs = a_addPrefixToMrpJobs;

        ChangeMOQty = a_AdjustNbrOrCyclesEnabled;
        JoinEnabled = a_JoinEnabled;
        ExpediteAfterSplit = a_ExpediteAfterSplit;
        SplitOperationEnabled = a_SplitActivityEnabled; //Isn't there supposed to be some difference between operation and activity?
        SplitJobEnabled = a_SplitJobEnabled;
        SplitMOEnabled = a_SplitMOEnabled;

        EnforceMaxDelay = a_enforceMaxDelay;

        Precision = a_Precision;
        m_jobLateThreshold = a_JobLateThreshold;
        TrackActualsAgeLimit = TrackActualsAgeLimit;

        TrackActualsAgeLimit = a_trackActualsAgeLimit;
        PerformingFastPercentage = a_performingFastPercent;
        PerformingSlowPercentage = a_performingSlowPercent;

        CalculateBalancedCompositeScore = a_calculateBalancedCompositeScore;

        SimulationProgressReportFrequency = (int)a_simulationProgressFrequency;

        RecalculateJITOnOptimizeEnabled = a_recalculateJITOnOptimizeEnabled;

        m_unsatisfiableJobPathHandling = a_unsatisfiableJobPathHandling;
        AllowChangeOfMaterialSuccessorSequenceOnMove = a_allowChangeOfMaterialSuccessorSequenceOnMove;

        RestoreMaterialConstraintsOnOptimizedActivities = a_restoreMaterialConstraintsOnOptimizedActivities;

        //If the set bool was flagged due to using the public accessor instead of the member variable.
        InitializeSetBools();
    }

    public void Update(ScenarioOptions a_newOptions)
    {
        if (a_newOptions.ActionOnOperationBadCapabilityIsSet && ActionOnOperationBadCapability != a_newOptions.ActionOnOperationBadCapability)
        {
            ActionOnOperationBadCapability = a_newOptions.ActionOnOperationBadCapability;
        }

        if (a_newOptions.OpLengthWarningLevelIsSet && OpLengthWarningLevel != a_newOptions.OpLengthWarningLevel)
        {
            OpLengthWarningLevel = a_newOptions.OpLengthWarningLevel;
        }

        if (a_newOptions.KeepFeasibleSpanIsSet && KeepFeasibleSpan != a_newOptions.KeepFeasibleSpan)
        {
            KeepFeasibleSpan = a_newOptions.KeepFeasibleSpan;
        }

        if (a_newOptions.PlanningHorizonIsSet && PlanningHorizon != a_newOptions.PlanningHorizon)
        {
            PlanningHorizon = a_newOptions.PlanningHorizon;
        }

        if (a_newOptions.ShortTermSpanIsSet && ShortTermSpan != a_newOptions.ShortTermSpan)
        {
            ShortTermSpan = a_newOptions.ShortTermSpan;
        }

        if (a_newOptions.MaxHistoriesCountIsSet && MaxHistoriesCount != a_newOptions.MaxHistoriesCount)
        {
            MaxHistoriesCount = a_newOptions.MaxHistoriesCount;
        }

        if (a_newOptions.DestructiveDbUpdateIsSet && DestructiveDbUpdate != a_newOptions.DestructiveDbUpdate)
        {
            DestructiveDbUpdate = a_newOptions.DestructiveDbUpdate;
        }

        if (a_newOptions.DestructiveDbUpdateIsSet && DestructiveDbUpdate != a_newOptions.DestructiveDbUpdate)
        {
            DestructiveDbUpdate = a_newOptions.DestructiveDbUpdate;
        }

        if (a_newOptions.FiscalYearEndIsSet && FiscalYearEnd != a_newOptions.FiscalYearEnd)
        {
            FiscalYearEnd = a_newOptions.FiscalYearEnd;
        }

        if (a_newOptions.AnnualPercentageRateIsSet && AnnualPercentageRate != a_newOptions.AnnualPercentageRate)
        {
            AnnualPercentageRate = a_newOptions.AnnualPercentageRate;
        }

        if (a_newOptions.ErpOverrideOfActivityUpdateIsSet && ErpOverrideOfActivityUpdate != a_newOptions.ErpOverrideOfActivityUpdate)
        {
            ErpOverrideOfActivityUpdate = a_newOptions.ErpOverrideOfActivityUpdate;
        }

        if (a_newOptions.SetSubJobNeedDatePointIsSet && SetSubJobNeedDatePoint != a_newOptions.SetSubJobNeedDatePoint)
        {
            SetSubJobNeedDatePoint = a_newOptions.SetSubJobNeedDatePoint;
        }

        if (a_newOptions.SetSubJobPrioritiesIsSet && SetSubJobPriorities != a_newOptions.SetSubJobPriorities)
        {
            SetSubJobPriorities = a_newOptions.SetSubJobPriorities;
        }

        if (a_newOptions.SetSubJobHotFlagsIsSet && SetSubJobHotFlags != a_newOptions.SetSubJobHotFlags)
        {
            SetSubJobHotFlags = a_newOptions.SetSubJobHotFlags;
        }

        if (a_newOptions.AddPrefixToMrpJobsIsSet && AddPrefixToMrpJobs != a_newOptions.AddPrefixToMrpJobs)
        {
            AddPrefixToMrpJobs = a_newOptions.AddPrefixToMrpJobs;
        }

        if (a_newOptions.ChangeMOQtyIsSet && ChangeMOQty != a_newOptions.ChangeMOQty)
        {
            ChangeMOQty = a_newOptions.ChangeMOQty;
        }

        if (a_newOptions.JoinEnabledIsSet && JoinEnabled != a_newOptions.JoinEnabled)
        {
            JoinEnabled = a_newOptions.JoinEnabled;
        }

        if (a_newOptions.ExpediteAfterSplitIsSet && ExpediteAfterSplit != a_newOptions.ExpediteAfterSplit)
        {
            ExpediteAfterSplit = a_newOptions.ExpediteAfterSplit;
        }

        if (a_newOptions.SplitOperationEnabledIsSet && SplitOperationEnabled != a_newOptions.SplitOperationEnabled)
        {
            SplitOperationEnabled = a_newOptions.SplitOperationEnabled;
        }

        if (a_newOptions.SplitJobEnabledIsSet && SplitJobEnabled != a_newOptions.SplitJobEnabled)
        {
            SplitJobEnabled = a_newOptions.SplitJobEnabled;
        }

        if (a_newOptions.SplitMOEnabledIsSet && SplitMOEnabled != a_newOptions.SplitMOEnabled)
        {
            SplitMOEnabled = a_newOptions.SplitMOEnabled;
        }

        if (a_newOptions.PrecisionIsSet && Precision != a_newOptions.Precision)
        {
            Precision = a_newOptions.Precision;
        }

        if (a_newOptions.EnforceMaxDelayIsSet && EnforceMaxDelay != a_newOptions.EnforceMaxDelay)
        {
            EnforceMaxDelay = a_newOptions.EnforceMaxDelay;
        }
        
        if (a_newOptions.JobLateThresholdIsSet && JobLateThreshold != a_newOptions.JobLateThreshold)
        {
            JobLateThreshold = a_newOptions.JobLateThreshold;
        }

        if (a_newOptions.TrackActualsAgeLimitIsSet && TrackActualsAgeLimit != a_newOptions.TrackActualsAgeLimit)
        {
            TrackActualsAgeLimit = a_newOptions.TrackActualsAgeLimit;
        }

        if (a_newOptions.PerformingFastPercentageIsSet && PerformingFastPercentage != a_newOptions.PerformingFastPercentage)
        {
            PerformingFastPercentage = a_newOptions.PerformingFastPercentage;
        }

        if (a_newOptions.PerformingSlowPercentageIsSet && PerformingSlowPercentage != a_newOptions.PerformingSlowPercentage)
        {
            PerformingSlowPercentage = a_newOptions.PerformingSlowPercentage;
        }

        if (a_newOptions.CalculateBalancedCompositeScoreIsSet && CalculateBalancedCompositeScore != a_newOptions.CalculateBalancedCompositeScore)
        {
            CalculateBalancedCompositeScore = a_newOptions.CalculateBalancedCompositeScore;
        }

        if (a_newOptions.SimulationProgressReportFrequencyIsSet && SimulationProgressReportFrequency != a_newOptions.SimulationProgressReportFrequency)
        {
            SimulationProgressReportFrequency = a_newOptions.SimulationProgressReportFrequency;
        }

        if (a_newOptions.RecalculateJITOnOptimizeEnabledIsSet && RecalculateJITOnOptimizeEnabled != a_newOptions.RecalculateJITOnOptimizeEnabled)
        {
            RecalculateJITOnOptimizeEnabled = a_newOptions.RecalculateJITOnOptimizeEnabled;
        }

        if (a_newOptions.UnsatisfiableJobPathHandlingIsSet && UnsatisfiableJobPathHandling != a_newOptions.UnsatisfiableJobPathHandling)
        {
            UnsatisfiableJobPathHandling = a_newOptions.UnsatisfiableJobPathHandling;
        }

        if (a_newOptions.PreventErpJobChangesInStableSpanIsSet && PreventErpJobChangesInStableSpan != a_newOptions.PreventErpJobChangesInStableSpan)
        {
            PreventErpJobChangesInStableSpan = a_newOptions.PreventErpJobChangesInStableSpan;
        }

        if (a_newOptions.UpdateActivityFromErpEvenIfUpdatedManuallyIsSet && UpdateActivityFromErpEvenIfUpdatedManually != a_newOptions.UpdateActivityFromErpEvenIfUpdatedManually)
        {
            UpdateActivityFromErpEvenIfUpdatedManually = a_newOptions.UpdateActivityFromErpEvenIfUpdatedManually;
        }

        if (a_newOptions.PreventErpJobCancelInStableSpanIsSet && PreventErpJobCancelInStableSpan != a_newOptions.PreventErpJobCancelInStableSpan)
        {
            PreventErpJobCancelInStableSpan = a_newOptions.PreventErpJobCancelInStableSpan;
        }

        if (a_newOptions.PreventErpJobChangesIfStartedIsSet && PreventErpJobChangesIfStarted != a_newOptions.PreventErpJobChangesIfStarted)
        {
            PreventErpJobChangesIfStarted = a_newOptions.PreventErpJobChangesIfStarted;
        }

        if (a_newOptions.PreventErpJobCancelIfStartedIsSet && PreventErpJobCancelIfStarted != a_newOptions.PreventErpJobCancelIfStarted)
        {
            PreventErpJobCancelIfStarted = a_newOptions.PreventErpJobCancelIfStarted;
        }
        if (a_newOptions.AnchorInFrozenZoneIsSet && AnchorInFrozenZone != a_newOptions.AnchorInFrozenZone)
        {
            AnchorInFrozenZone = a_newOptions.AnchorInFrozenZone;
        }
        if (a_newOptions.LockedInFrozenZoneIsSet && LockInFrozenZone != a_newOptions.LockInFrozenZone)
        {
            LockInFrozenZone = a_newOptions.LockInFrozenZone;
        }
        if (a_newOptions.CommitOnAnchorIsSet && CommitOnAnchor != a_newOptions.CommitOnAnchor)
        {
            CommitOnAnchor = a_newOptions.CommitOnAnchor;
        }
        if (a_newOptions.ShortTermSpanIsSet && ShortTermSpan != a_newOptions.ShortTermSpan)
        {
            ShortTermSpan = a_newOptions.ShortTermSpan;
        }
        if (a_newOptions.PlanningHorizonIsSet && PlanningHorizon != a_newOptions.PlanningHorizon)
        {
            PlanningHorizon = a_newOptions.PlanningHorizon;
        }
        if (a_newOptions.PreventAutoDeleteActualsIsSet && PreventAutoDeleteActuals != a_newOptions.PreventAutoDeleteActuals)
        {
            PreventAutoDeleteActuals = a_newOptions.PreventAutoDeleteActuals;
        }
        if (a_newOptions.AllowChangeOfMaterialSuccessorSequenceOnMoveIsSet && AllowChangeOfMaterialSuccessorSequenceOnMove != a_newOptions.AllowChangeOfMaterialSuccessorSequenceOnMove)
        {
            AllowChangeOfMaterialSuccessorSequenceOnMove = a_newOptions.AllowChangeOfMaterialSuccessorSequenceOnMove;
        }
        if (a_newOptions.RestoreMaterialConstraintsOnOptimizedActivitiesIsSet && RestoreMaterialConstraintsOnOptimizedActivities != a_newOptions.RestoreMaterialConstraintsOnOptimizedActivities)
        {
            RestoreMaterialConstraintsOnOptimizedActivities = a_newOptions.RestoreMaterialConstraintsOnOptimizedActivities;
        }
        if (a_newOptions.IgnoreMaterialConstraintsInFrozenSpanIsSet && IgnoreMaterialConstraintsInFrozenSpan != a_newOptions.IgnoreMaterialConstraintsInFrozenSpan)
        {
            IgnoreMaterialConstraintsInFrozenSpan = a_newOptions.IgnoreMaterialConstraintsInFrozenSpan;
        }

        //Now that the settings have been updated, clear out the IsSet Bools which get set during Update.
        InitializeSetBools();
    }

    public bool Equals(ScenarioOptions a_newOptions)
    {
        return
            m_actionOnOperationBadCapability == a_newOptions.m_actionOnOperationBadCapability &&
            m_opLengthWarningLevel == a_newOptions.m_opLengthWarningLevel &&
            m_keepFeasibleSpan == a_newOptions.m_keepFeasibleSpan &&
            m_planningHorizon == a_newOptions.m_planningHorizon &&
            m_shortTermSpan == a_newOptions.m_shortTermSpan &&
            m_maxHistoriesCount == a_newOptions.m_maxHistoriesCount &&
            m_destructiveDbUpdate == a_newOptions.m_destructiveDbUpdate &&
            m_fiscalYearEnd == a_newOptions.m_fiscalYearEnd &&
            m_annualPercentageRate == a_newOptions.m_annualPercentageRate &&
            m_erpOverrideOfActivityUpdate == a_newOptions.m_erpOverrideOfActivityUpdate &&
            m_boolVector.Equals(a_newOptions.m_boolVector) &&
            m_precision == a_newOptions.m_precision &&
            m_jobLateThreshold == a_newOptions.m_jobLateThreshold &&
            m_trackActualsAgeLimit == a_newOptions.m_trackActualsAgeLimit &&
            m_performingFastPercentage == a_newOptions.m_performingFastPercentage &&
            m_performingSlowPercentage == a_newOptions.m_performingSlowPercentage &&
            m_simulationProgressReportFrequency == a_newOptions.m_simulationProgressReportFrequency &&
            m_unsatisfiableJobPathHandling == a_newOptions.m_unsatisfiableJobPathHandling &&
            m_setSubJobNeedDateResetPoint == a_newOptions.m_setSubJobNeedDateResetPoint;
    }

    private void InitEnablementOptions()
    {
        ChangeMOQty = true;
        JoinEnabled = true;
        SplitOperationEnabled = true;
        SplitJobEnabled = true;
        SplitMOEnabled = true;
        ExpediteAfterSplit = true;
        RecalculateJITOnOptimizeEnabled = true;
        RestoreMaterialConstraintsOnOptimizedActivities = true;

        //If the set bool was flagged due to using the public accessor instead of the member variable.
        InitializeSetBools();
    }

    #region BoolVector32
    private BoolVector32 m_boolVector;

    private const int c_LockInFrozenZoneIdx = 0;
    private const int c_AnchorInFrozenZoneIdx = 1;
    private const int c_UpdateActivityFromErpEvenIfUpdatedManuallyIdx = 2;
    private const int c_PreventErpJobChangeInStableSpanIdx = 3;
    private const int c_PreventErpJobCancelInStableSpanIdx = 4;
    private const int c_PreventErpJobChangeIfStartedIdx = 5;

    private const int c_PreventErpJobCancelIfStartedIdx = 6;

    //const int c_TrackSubComponentSourceMOsIdx = 7; unused
    private const int c_SetSubJobPrioritiesIdx = 8;
    private const int c_SetSubJobHotFlagsIdx = 9;
    private const int c_SetSubJobNeedDatesIdx = 10; //Backwards compatibility

    private const int c_MOQtyEnabledIdx = 11;
    private const int c_JoinEnabledIdx = 12;
    private const int c_SplitOperationEnabledIdx = 13;
    private const int c_SplitJobEnabledIdx = 14;
    private const int c_SplitMOEnabledIdx = 15;

    //const int c_RoundImportedValuesIdx = 16; // unused
    private const int c_CommitOnAnchorIdx = 17;

    private const int c_ShowMOBatchingTabOnOptimizeDialogIdx = 18;

    private const int c_EnforceMaxDelayIdx = 19;

    //private const int c_enableAutoSelectAlternatePath = 20; //unused
    private const int c_calculateBalancedCompositeScoreIdx = 23;

    [Obsolete("Move Compress Predecessors setting has been moved to ActivityMoveSettings")]
    private const int c_moveCompressesPredOpnsEnabledObsolete = 24;

    private const int c_recalculateJITOnOptimizeEnabled = 25;

    private const int c_expediteAfterSplitIdx = 26;
    private const int c_ignoreMaterialConstraintsInFrozenSpanIdx = 27;
    private const int c_addPrefixToMrpJobs = 28;
    private const short c_preventAutoDeleteActualsIdx = 29;
    private const short c_allowChangeOfMaterialSuccessorSequenceOnMoveIdx = 30;
    private const short c_restoreMaterialConstraintsOnOptimizedActivitiesIdx = 31;
    #endregion

    /*
     * Add set bool index
     * Update Setter
     * Add IsSet
     * Check setting references to the public property
     */

    #region "IsSetBoolVector"
    /*
     * TODO: It'd be nice if we could split these values up a bit so the bool vectors could be given more descriptive
     * names for readability. However, since I don't currently know most of this stuff means, hopefully I'll have
     * the time and knowledge in the future to improve this.
     */
    private BoolVector32 m_setBools;
    private const short c_actionOnOperationBadCapabilityIsSetIdx = 0;
    private const short c_opLengthWarningLevelIsSetIdx = 1;
    private const short c_frozenSpanIsSetIdx = 2; // deprecated
    private const short c_keepFeasibleSpanIsSetIdx = 3;
    private const short c_planningHorizonIsSetIdx = 4;
    private const short c_shortTermSpanIsSetIdx = 5;
    private const short c_maxHistoriesCountIsSetIdx = 6;
    private const short c_destructiveDbUpdateIsSetIdx = 7;
    private const short c_lockInFrozenZoneIsSetIdx = 8;
    private const short c_anchorInFrozenZoneIsSetIdx = 9;
    private const short c_commitOnAnchorIsSetIdx = 10;
    private const short c_fiscalYearEndIsSetIdx = 11;
    private const short c_aprIsSetIdx = 12;
    private const short c_updateActivityFromErpEvenIfUpdatedManuallyIsSetIdx = 13;
    private const short c_erpOverrideOfActivityUpdateIsSetIdx = 14;
    private const short c_preventErpJobChangesInStableSpanIsSetIdx = 15;
    private const short c_preventErpJobCancelInStableSpanIsSetIdx = 16;
    private const short c_preventErpJobChangesIfStartedIsSetIdx = 17;
    private const short c_preventErpJobCancelIfStartedIsSetIdx = 18;
    private const short c_setSubJobNeedDatesIsSetIdx = 19;
    private const short c_setSubJobPrioritiesIsSetIdx = 20;
    private const short c_setSubJobHotFlagsIsSetIdx = 21;
    private const short c_addPrefixToMrpJobsIsSetIdx = 22;
    private const short c_MOQtyEnabledIsSetIdx = 23;
    private const short c_joinEnabledIsSetIdx = 24;
    private const short c_expediteAfterSplitIsSetIdx = 25;
    private const short c_splitOperationEnabledIsSetIdx = 26;
    private const short c_splitJobEnabledIsSetIdx = 27;
    private const short c_splitMOEnabledIsSetIdx = 28;
    private const short c_showMOBatchingTabOnOptimizeDialogIsSetIdx = 29;
    private const short c_enforceMaxDelayIsSetIdx = 30;
    private const short c_enableAutoSelectAlternatePathIsSetIdx = 31;

    private BoolVector32 m_setBools2;
    private const short c_precisionIsSetIdx = 0;
    private const short c_jobLateThresholdIsSetIdx = 1;
    //private const short c_timeSpanRoundingPlacesIsSetIdx = 2; //obsolete
    private const short c_trackActualsAgeLimitIsSetIdx = 3;
    private const short c_performingFastPercentIsSetIdx = 4;
    private const short c_performingSlowPercentIsSetIdx = 5;
    private const short c_calculateBalancedCompositeScoreIsSetIdx = 6;
    private const short c_simulationProgressFrequencyIsSetIdx = 7;
    private const short c_recalculateJITOnOptimizeEnabledIsSetIdx = 8;
    private const short c_unsatisfiableJobPathHandlingIsSetIdx = 9;
    private const short c_preventAutoDeleteActualsIsSetIdx = 10;
    private const short c_allowChangeOfMaterialSuccessorIsSetIdx = 11;
    private const short c_restoreMaterialConstraintsOnOptimizedActivitiesIsSetIdx = 12;
    private const short c_ignoreMaterialConstraintsInFrozenSpanIsSetIdx = 13;
    #endregion

    private EActionOnOperationBadCapability m_actionOnOperationBadCapability = EActionOnOperationBadCapability.RejectMO;

    public EActionOnOperationBadCapability ActionOnOperationBadCapability
    {
        get => m_actionOnOperationBadCapability;
        set
        {
            m_actionOnOperationBadCapability = value;
            m_setBools[c_actionOnOperationBadCapabilityIsSetIdx] = true;
        }
    }

    public bool ActionOnOperationBadCapabilityIsSet => m_setBools[c_actionOnOperationBadCapabilityIsSetIdx];

    private long m_opLengthWarningLevel = DefaultPlanningHorizon.Ticks;

    /// <summary>
    /// Operation's created with length that are longer than this cause a warning to be generated.
    /// Default is set to FrozenSpan Default.
    /// 0 means no warning.
    /// </summary>
    public long OpLengthWarningLevelTicks
    {
        get => m_opLengthWarningLevel;
        private set
        {
            m_opLengthWarningLevel = value;
            m_setBools[c_opLengthWarningLevelIsSetIdx] = true;
        }
    }

    public bool OpLengthWarningLevelIsSet => m_setBools[c_opLengthWarningLevelIsSetIdx];

    /// <summary>
    /// The length of Operations that are larger than this amount cause a warning to be generated.
    /// 0 means no warning.
    /// </summary>
    public TimeSpan OpLengthWarningLevel
    {
        get => TimeSpan.FromTicks(m_opLengthWarningLevel);
        set
        {
            m_opLengthWarningLevel = value.Ticks;
            m_setBools[c_opLengthWarningLevelIsSetIdx] = true;
        }
    }

    private TimeSpan m_keepFeasibleSpan = DefaultPlanningHorizon;

    /// <summary>
    /// Specifies the amount of time from the Clock time that all Constraints must be enforced to ensure a feasible schedule for production to follow.  Outside of this time period, predecessor and material
    /// constraints can be overriden if the resource is allowed to pre-empt such constraints.
    /// </summary>
    public TimeSpan KeepFeasibleSpan
    {
        get => m_keepFeasibleSpan;
        private set
        {
            m_keepFeasibleSpan = value;
            m_setBools[c_keepFeasibleSpanIsSetIdx] = true;
        }
    }

    public bool KeepFeasibleSpanIsSet => m_setBools[c_keepFeasibleSpanIsSetIdx];

    private int m_maxHistoriesCount = 100;

    /// <summary>
    /// The maximum number of Scenario Histories to store for each Scenario.  All Histories are shown in the UI, but only this many are stored.
    /// </summary>
    public int MaxHistoriesCount
    {
        get => m_maxHistoriesCount;
        private set
        {
            m_maxHistoriesCount = value;
            m_setBools[c_maxHistoriesCountIsSetIdx] = true;
        }
    }

    public bool MaxHistoriesCountIsSet => m_setBools[c_maxHistoriesCountIsSetIdx];

    #region Fiscal Year
    private DateTime m_fiscalYearEnd = new(PTDateTime.UtcNow.Year, 12, 31);

    /// <summary>
    /// The last day of this fiscal year in terms of accounting.  This is used by the KPI to calculate quarterly revenue.
    /// </summary>
    public DateTime FiscalYearEnd
    {
        get => m_fiscalYearEnd;
        set
        {
            m_fiscalYearEnd = value;
            m_setBools[c_fiscalYearEndIsSetIdx] = true;
        }
    }

    public bool FiscalYearEndIsSet => m_setBools[c_fiscalYearEndIsSetIdx];

    /// <summary>
    /// Set the Fiscal Year End by one year.
    /// </summary>
    public void AdvanceFiscalYearEnd()
    {
        FiscalYearEnd = FiscalYearEnd.AddYears(1);
    }

    /// <summary>
    /// If necessary, advances the Fiscal Year End into the current year.
    /// </summary>
    public void AdvanceFiscalYearEndIfNecessary(long a_newPtClock)
    {
        while (FiscalYearEnd.Ticks < a_newPtClock)
        {
            AdvanceFiscalYearEnd();
        }
    }

    public DateTime GetQ3End()
    {
        return FiscalYearEnd.AddMonths(-3);
    }

    public DateTime GetQ2End()
    {
        return GetQ3End().AddMonths(-3);
    }

    public DateTime GetQ1End()
    {
        return GetQ2End().AddMonths(-3);
    }

    /// <summary>
    /// The end date of the current Qtr based on the PT Clock.
    /// </summary>
    /// <returns></returns>
    public DateTime GetThisQtrEndDate(DateTime a_ptClock)
    {
        int yrsAdvanced = 0;
        while (a_ptClock > FiscalYearEnd)
        {
            AdvanceFiscalYearEnd();
            yrsAdvanced++;
        }

        DateTime q3End = GetQ3End();
        DateTime q2End = GetQ2End();
        DateTime q1End = GetQ1End();

        DateTime qtrEnd;
        if (a_ptClock > q3End)
        {
            qtrEnd = FiscalYearEnd;
        }
        else if (a_ptClock > q2End)
        {
            qtrEnd = q3End;
        }
        else if (a_ptClock > q1End)
        {
            qtrEnd = q2End;
        }
        else
        {
            qtrEnd = q1End;
        }

        if (yrsAdvanced > 0)
        {
            throw new PTHandleableException("2322", new object[] { yrsAdvanced });
        }

        return qtrEnd;
    }
    #endregion

    private decimal m_annualPercentageRate = 10;

    /// <summary>
    /// APR for calculating things like WIP carrying cost, etc.
    /// </summary>
    public decimal AnnualPercentageRate
    {
        get => m_annualPercentageRate;
        set
        {
            if (value > 100 || value < 0)
            {
                throw new PTHandleableException("APR should be a number between 0 and 100.");
            }

            m_annualPercentageRate = value;
            m_setBools[c_aprIsSetIdx] = true;
        }
    }

    public bool AnnualPercentageRateIsSet => m_setBools[c_aprIsSetIdx];

    /// <summary>
    /// APR / (100 * 365)
    /// </summary>
    public decimal DailyInterestRate => AnnualPercentageRate / 36500;

    public static readonly TimeSpan DefaultPlanningHorizon = TimeSpan.FromDays(93);
    private TimeSpan m_planningHorizon = DefaultPlanningHorizon;

    /// <summary>
    /// Anything beyond the Clock plus this Span is considered to be unimportant in the scheduling function.  The schedule can extend past this time but optimization algorithms are often designed to stop
    /// detailed planning past this time.  All reasources are considered to be continuously available beyond this time.
    /// </summary>
    public TimeSpan PlanningHorizon
    {
        get => m_planningHorizon;
        set
        {
            m_planningHorizon = value;
            m_setBools[c_planningHorizonIsSetIdx] = true;
        }
    }

    public bool PlanningHorizonIsSet => m_setBools[c_planningHorizonIsSetIdx];

    private TimeSpan m_shortTermSpan = new(14, 0, 0, 0);

    /// <summary>
    /// Time span from the System.Clock that constitutes the 'short range' schedule.  This can be used by various scheduling and statistical functions.
    /// </summary>
    public TimeSpan ShortTermSpan
    {
        get => m_shortTermSpan;
        set
        {
            m_shortTermSpan = value;
            m_setBools[c_shortTermSpanIsSetIdx] = true;
        }
    }

    public bool ShortTermSpanIsSet => m_setBools[c_shortTermSpanIsSetIdx];

    public decimal ShortTermSpanTotalDays => (decimal)m_shortTermSpan.TotalDays;

    /// <summary>
    /// If a change in Required Qty or Need Date is received for a Job scheduled to start within
    /// the Stable Span then the change is rejected and logged.
    /// </summary>
    public bool PreventErpJobChangesInStableSpan
    {
        get => m_boolVector[c_PreventErpJobChangeInStableSpanIdx];
        set
        {
            m_boolVector[c_PreventErpJobChangeInStableSpanIdx] = value;
            m_setBools[c_preventErpJobChangesInStableSpanIsSetIdx] = true;
        }
    }

    public bool PreventErpJobChangesInStableSpanIsSet => m_setBools[c_preventErpJobChangesInStableSpanIsSetIdx];

    /// <summary>
    /// If a Job scheduled to start within the Stable Span has its Cancel flag set by an ERP
    /// Transmission or is marked for Deletion by an ERP Transmission then the request is rejected
    /// and logged.
    /// </summary>
    public bool PreventErpJobCancelInStableSpan
    {
        get => m_boolVector[c_PreventErpJobCancelInStableSpanIdx];
        set
        {
            m_boolVector[c_PreventErpJobCancelInStableSpanIdx] = value;
            m_setBools[c_preventErpJobCancelInStableSpanIsSetIdx] = true;
        }
    }

    public bool PreventErpJobCancelInStableSpanIsSet => m_setBools[c_preventErpJobCancelInStableSpanIsSetIdx];

    /// <summary>
    /// If a change in Required Qty or Need Date is received for a Job that is Started
    /// then the change is rejected and logged.
    /// </summary>
    public bool PreventErpJobChangesIfStarted
    {
        get => m_boolVector[c_PreventErpJobChangeIfStartedIdx];
        set
        {
            m_boolVector[c_PreventErpJobChangeIfStartedIdx] = value;
            m_setBools[c_preventErpJobChangesIfStartedIsSetIdx] = true;
        }
    }

    public bool PreventErpJobChangesIfStartedIsSet => m_setBools[c_preventErpJobChangesIfStartedIsSetIdx];

    /// <summary>
    /// If a Job is Started and has its Cancel flag set by an ERP
    /// Transmission or is marked for Deletion by an ERP Transmission then the request is rejected
    /// and logged.
    /// </summary>
    public bool PreventErpJobCancelIfStarted
    {
        get => m_boolVector[c_PreventErpJobCancelIfStartedIdx];
        set
        {
            m_boolVector[c_PreventErpJobCancelIfStartedIdx] = value;
            m_setBools[c_preventErpJobCancelIfStartedIsSetIdx] = true;
        }
    }

    public bool PreventErpJobCancelIfStartedIsSet => m_setBools[c_preventErpJobCancelIfStartedIsSetIdx];

    public bool TrackSubComponentSourceMOs => SetSubJobNeedDatePoint != ESubJobNeedDateResetPoint.None || SetSubJobPriorities || SetSubJobHotFlags;

    private bool m_destructiveDbUpdate = true;

    /// <summary>
    /// Whether the SQL Database update should destructively overwrite the data in the database.  If not then the data will be updated instead.
    /// </summary>
    public bool DestructiveDbUpdate
    {
        get => m_destructiveDbUpdate;
        private set
        {
            m_destructiveDbUpdate = value;
            m_setBools[c_destructiveDbUpdateIsSetIdx] = true;
        }
    }

    public bool DestructiveDbUpdateIsSet => m_setBools[c_destructiveDbUpdateIsSetIdx];

    /// <summary>
    /// When set to true all the activities in the Frozen Zone are Locked following a simulation.
    /// </summary>
    public bool LockInFrozenZone
    {
        get => m_boolVector[c_LockInFrozenZoneIdx];
        set
        {
            m_boolVector[c_LockInFrozenZoneIdx] = value;
            m_setBools[c_lockInFrozenZoneIsSetIdx] = true;
        }
    }

    public bool LockedInFrozenZoneIsSet => m_setBools[c_lockInFrozenZoneIsSetIdx];

    /// <summary>
    /// When set to true all the activities in the Frozen Zone are Anchored following a simulation.
    /// </summary>
    public bool AnchorInFrozenZone
    {
        get => m_boolVector[c_AnchorInFrozenZoneIdx];
        set
        {
            m_boolVector[c_AnchorInFrozenZoneIdx] = value;
            m_setBools[c_anchorInFrozenZoneIsSetIdx] = true;
        }
    }

    public bool AnchorInFrozenZoneIsSet => m_setBools[c_anchorInFrozenZoneIsSetIdx];

    /// <summary>
    /// Whether to set the Operation Commit Dates when an Operation is Anchored or Re-anchored.
    /// </summary>
    public bool CommitOnAnchor
    {
        get => m_boolVector[c_CommitOnAnchorIdx];
        set
        {
            m_boolVector[c_CommitOnAnchorIdx] = value;
            m_setBools[c_commitOnAnchorIsSetIdx] = true;
        }
    }

    public bool CommitOnAnchorIsSet => m_setBools[c_commitOnAnchorIsSetIdx];

    /// <summary>
    /// Whether to update the Production Status of Activities based on ERP JobTs even if the Activities had their status updated internally previously.
    /// </summary>
    public bool UpdateActivityFromErpEvenIfUpdatedManually
    {
        get => m_boolVector[c_UpdateActivityFromErpEvenIfUpdatedManuallyIdx];
        set
        {
            m_boolVector[c_UpdateActivityFromErpEvenIfUpdatedManuallyIdx] = value;
            m_setBools[c_updateActivityFromErpEvenIfUpdatedManuallyIsSetIdx] = true;
        }
    }

    public bool UpdateActivityFromErpEvenIfUpdatedManuallyIsSet => m_setBools[c_updateActivityFromErpEvenIfUpdatedManuallyIsSetIdx];

    public bool ShowMOBatchingTabOnOptimizeDialog
    {
        get => m_boolVector[c_ShowMOBatchingTabOnOptimizeDialogIdx];
        set
        {
            m_boolVector[c_ShowMOBatchingTabOnOptimizeDialogIdx] = value;
            m_setBools[c_showMOBatchingTabOnOptimizeDialogIsSetIdx] = true;
        }
    }

    public bool ShowMOBatchingTabOnOptimizeDialogIsSet => m_setBools[c_showMOBatchingTabOnOptimizeDialogIsSetIdx];

    public enum EErpActivityUpdateOverrides { Never = 0, IfValuesAreGreater, Always }

    private EErpActivityUpdateOverrides m_erpOverrideOfActivityUpdate = EErpActivityUpdateOverrides.Never;

    /// <summary>
    /// When to allow values in a JobT activity to update values in an activity finished internally.
    /// </summary>
    public EErpActivityUpdateOverrides ErpOverrideOfActivityUpdate
    {
        get => m_erpOverrideOfActivityUpdate;
        set
        {
            m_erpOverrideOfActivityUpdate = value;
            m_setBools[c_erpOverrideOfActivityUpdateIsSetIdx] = true;
        }
    }

    public bool ErpOverrideOfActivityUpdateIsSet => m_setBools[c_erpOverrideOfActivityUpdateIsSetIdx];

    private TimeSpan m_jobLateThreshold;

    /// <summary>
    /// Jobs are only considered late if the Scheduled End is at least this much after the NeedDate.
    /// This can be used to ignore Jobs that are only slightly late and not really of interest.
    /// </summary>
    public TimeSpan JobLateThreshold
    {
        get => m_jobLateThreshold;
        set
        {
            m_jobLateThreshold = value;
            m_setBools2[c_jobLateThresholdIsSetIdx] = true;
        }
    }

    public bool JobLateThresholdIsSet => m_setBools2[c_jobLateThresholdIsSetIdx];

    private TimeSpan m_trackActualsAgeLimit = new(0);

    /// <summary>
    /// If greater than zero, then scheduled Resources are stored when Activities are Finished.
    /// CapacityIntervals that are end more than this duration earlier than the Clock are purged during Clock Advances.
    /// </summary>
    public TimeSpan TrackActualsAgeLimit
    {
        get => m_trackActualsAgeLimit;
        set
        {
            m_trackActualsAgeLimit = value;
            m_setBools2[c_trackActualsAgeLimitIsSetIdx] = true;
        }
    }

    public bool TrackActualsAgeLimitIsSet => m_setBools2[c_trackActualsAgeLimitIsSetIdx];

    public bool PreventAutoDeleteActuals
    {
        get => m_boolVector[c_preventAutoDeleteActualsIdx];
        set
        {
            m_boolVector[c_preventAutoDeleteActualsIdx] = value;
            m_setBools2[c_preventAutoDeleteActualsIsSetIdx] = true;
        }
    }

    public bool PreventAutoDeleteActualsIsSet => m_setBools2[c_preventAutoDeleteActualsIsSetIdx];

    private int m_performingFastPercentage;

    /// <summary>
    /// If reported times are more than this percent below the standard time then the operation is marked as fast performing.
    /// </summary>
    public int PerformingFastPercentage
    {
        get => m_performingFastPercentage;
        set
        {
            m_performingFastPercentage = value;
            m_setBools2[c_performingFastPercentIsSetIdx] = true;
        }
    }

    public bool PerformingFastPercentageIsSet => m_setBools2[c_performingFastPercentIsSetIdx];

    private int m_performingSlowPercentage;

    /// <summary>
    /// If reported times are more than this percent above the standard time then the operation is marked as Slow performing.
    /// </summary>
    public int PerformingSlowPercentage
    {
        get => m_performingSlowPercentage;
        set
        {
            m_performingSlowPercentage = value;
            m_setBools2[c_performingSlowPercentIsSetIdx] = true;
        }
    }

    public bool PerformingSlowPercentageIsSet => m_setBools2[c_performingSlowPercentIsSetIdx];

    /// <summary>
    /// How to handle jobs with paths that are unsatisfiable.
    /// </summary>
    public enum EUnsatisfiableJobPathHandlingEnum
    {
        /// <summary>
        /// Do not include the job in the optimize.
        /// </summary>
        ExcludeJob,

        /// <summary>
        /// If the job has some paths that are satisfiable, include the job in the optimize. Unsatisfiable paths will be ignored.
        /// </summary>
        OptimizeSatisfiablePaths
    }

    private EUnsatisfiableJobPathHandlingEnum m_unsatisfiableJobPathHandling;

    public EUnsatisfiableJobPathHandlingEnum UnsatisfiableJobPathHandling
    {
        get => m_unsatisfiableJobPathHandling;
        set
        {
            m_unsatisfiableJobPathHandling = value;
            m_setBools2[c_unsatisfiableJobPathHandlingIsSetIdx] = true;
        }
    }

    public bool UnsatisfiableJobPathHandlingIsSet => m_setBools2[c_unsatisfiableJobPathHandlingIsSetIdx];

    #region Simulation Progress
    public static readonly int MinProgressReportingPercentFrequency = 4;
    private const int c_defaultProgressReportingPercentFrequency = 0; // The default is to report progress in 1 percent increments.
    public static readonly int MaxProgressReportingPercentFrequency = 0;
    private double m_simulationProgressReportFrequency = c_defaultProgressReportingPercentFrequency;

    public int SimulationProgressReportFrequency
    {
        get => (int)m_simulationProgressReportFrequency;
        set
        {
            m_simulationProgressReportFrequency = value; //it use to cast value to a double, but I'm not sure why so I removed it
            m_setBools2[c_simulationProgressFrequencyIsSetIdx] = true;
        }
    }

    public bool SimulationProgressReportFrequencyIsSet => m_setBools2[c_simulationProgressFrequencyIsSetIdx] = true;
    #endregion

    #region MRP
    public enum ESubJobNeedDateResetPoint
    {
        None,
        EarlierOfJitAndScheduledStart,
        OperationJitStart,
        OperationScheduledStart,
        BottleneckedOperationScheduledStart,
        ProductionOverlap,
    }

    public bool SetSubJobPriorities
    {
        get => m_boolVector[c_SetSubJobPrioritiesIdx];
        set
        {
            m_boolVector[c_SetSubJobPrioritiesIdx] = value;
            m_setBools[c_setSubJobPrioritiesIsSetIdx] = true;
        }
    }

    public bool SetSubJobPrioritiesIsSet => m_setBools[c_setSubJobPrioritiesIsSetIdx];

    public bool SetSubJobHotFlags
    {
        get => m_boolVector[c_SetSubJobHotFlagsIdx];
        set
        {
            m_boolVector[c_SetSubJobHotFlagsIdx] = value;
            m_setBools[c_setSubJobHotFlagsIsSetIdx] = true;
        }
    }

    public bool SetSubJobHotFlagsIsSet => m_setBools[c_setSubJobHotFlagsIsSetIdx];

    public bool AddPrefixToMrpJobs
    {
        get => m_boolVector[c_addPrefixToMrpJobs];
        set
        {
            m_boolVector[c_addPrefixToMrpJobs] = value;
            m_setBools[c_addPrefixToMrpJobsIsSetIdx] = true;
        }
    }

    public bool AddPrefixToMrpJobsIsSet => m_setBools[c_addPrefixToMrpJobsIsSetIdx];

    private ESubJobNeedDateResetPoint m_setSubJobNeedDateResetPoint;

    /// <summary>
    /// This allows MRP to turn this feature on/off to help set need dates more accurately during MRP.
    /// </summary>
    /// <param name="a_value"></param>
    public void SetSubJobNeedDates_Set(ESubJobNeedDateResetPoint a_value)
    {
        SetSubJobNeedDatePoint = a_value;
    }

    public ESubJobNeedDateResetPoint SetSubJobNeedDatePoint
    {
        get => m_setSubJobNeedDateResetPoint;
        set
        {
            m_setSubJobNeedDateResetPoint = value;
            m_setBools[c_setSubJobNeedDatesIsSetIdx] = true;
        }
    }

    public bool SetSubJobNeedDatePointIsSet => m_setBools[c_setSubJobNeedDatesIsSetIdx];

    public bool ChangeMOQty
    {
        get => m_boolVector[c_MOQtyEnabledIdx];
        set
        {
            m_boolVector[c_MOQtyEnabledIdx] = value;
            m_setBools[c_MOQtyEnabledIsSetIdx] = true;
        }
    }

    public bool ChangeMOQtyIsSet => m_setBools[c_MOQtyEnabledIsSetIdx];

    public bool JoinEnabled
    {
        get => m_boolVector[c_JoinEnabledIdx];
        private set
        {
            m_boolVector[c_JoinEnabledIdx] = value;
            m_setBools[c_joinEnabledIsSetIdx] = true;
        }
    }

    public bool JoinEnabledIsSet => m_setBools[c_joinEnabledIsSetIdx];

    public bool SplitOperationEnabled
    {
        get => m_boolVector[c_SplitOperationEnabledIdx];
        private set
        {
            m_boolVector[c_SplitOperationEnabledIdx] = value;
            m_setBools[c_splitOperationEnabledIsSetIdx] = true;
        }
    }

    public bool SplitOperationEnabledIsSet => m_setBools[c_splitJobEnabledIsSetIdx];

    public bool SplitJobEnabled
    {
        get => m_boolVector[c_SplitJobEnabledIdx];
        private set
        {
            m_boolVector[c_SplitJobEnabledIdx] = value;
            m_setBools[c_splitJobEnabledIsSetIdx] = true;
        }
    }

    public bool SplitJobEnabledIsSet => m_setBools[c_splitJobEnabledIsSetIdx];

    public bool SplitMOEnabled
    {
        get => m_boolVector[c_SplitMOEnabledIdx];
        private set
        {
            m_boolVector[c_SplitMOEnabledIdx] = value;
            m_setBools[c_splitMOEnabledIsSetIdx] = true;
        }
    }

    public bool SplitMOEnabledIsSet => m_setBools[c_splitMOEnabledIsSetIdx];

    //Specifies if the MO should be expedited after a split or join.
    public bool ExpediteAfterSplit
    {
        get => m_boolVector[c_expediteAfterSplitIdx];
        private set
        {
            m_boolVector[c_expediteAfterSplitIdx] = value;
            m_setBools[c_expediteAfterSplitIsSetIdx] = true;
        }
    }

    public bool ExpediteAfterSplitIsSet => m_setBools[c_expediteAfterSplitIsSetIdx];

    public bool EnforceMaxDelay
    {
        get => m_boolVector[c_EnforceMaxDelayIdx];
        set
        {
            m_boolVector[c_EnforceMaxDelayIdx] = value;
            m_setBools[c_enforceMaxDelayIsSetIdx] = true;
        }
    }

    public bool EnforceMaxDelayIsSet => m_setBools[c_enforceMaxDelayIsSetIdx];

    public bool CalculateBalancedCompositeScore
    {
        get => m_boolVector[c_calculateBalancedCompositeScoreIdx];
        set
        {
            m_boolVector[c_calculateBalancedCompositeScoreIdx] = value;
            m_setBools2[c_calculateBalancedCompositeScoreIsSetIdx] = true;
        }
    }

    public bool CalculateBalancedCompositeScoreIsSet => m_setBools2[c_calculateBalancedCompositeScoreIsSetIdx];

    public bool RecalculateJITOnOptimizeEnabled
    {
        get => m_boolVector[c_recalculateJITOnOptimizeEnabled];
        set
        {
            m_boolVector[c_recalculateJITOnOptimizeEnabled] = value;
            m_setBools2[c_recalculateJITOnOptimizeEnabledIsSetIdx] = true;
        }
    }

    public bool RecalculateJITOnOptimizeEnabledIsSet => m_setBools2[c_recalculateJITOnOptimizeEnabledIsSetIdx];
    #endregion MRP

    #region ICloneable Members
    object ICloneable.Clone()
    {
        return Clone();
    }

    public ScenarioOptions Clone()
    {
        ScenarioOptions clone = this.CopyInMemory();
        clone.InitializeSetBools();
        return clone;
    }
    #endregion

    #region Rounding
    private const int c_maxPercision = 15;
    private int m_precision = 4;

    /// <summary>
    /// Throws ScenarioOptionsException if out of range.
    /// </summary>
    public int Precision
    {
        get => m_precision;

        set
        {
            if (value < 0 || value > c_maxPercision)
            {
                throw new ScenarioOptionsException("Precision must be between 0 and 15 inclusive.");
            }

            m_precision = value;
            m_setBools2[c_precisionIsSetIdx] = true;
        }
    }

    public bool PrecisionIsSet => m_setBools2[c_precisionIsSetIdx];

    /// This is the smallest positive double value.
    /// Numbers smaller than what can be represented with number of Precisions will round to zero.
    /// i.e. 3 digits -> cutoff 0.001
    public decimal Epsilon => 1 / new decimal(Math.Pow(10, Precision));

    public bool IsApproximatelyZero(decimal a_x)
    {
        return IsApproximatelyEqual(a_x, 0);
    }

    public bool IsApproximatelyZeroOrLess(decimal a_x)
    {
        return a_x < 0 || IsApproximatelyZero(a_x);
    }

    public bool IsStrictlyGreaterThanZero(decimal a_x)
    {
        return !IsApproximatelyZeroOrLess(a_x);
    }

    public bool IsApproximatelyEqual(decimal a_x, decimal a_y)
    {
        // If they are equal anyway, just return True. 
        if (a_x.Equals(a_y))
        {
            return true;
        }

        FloatingPoint fpMath = new();
        return fpMath.ApproximatelyEqual(a_x, a_y, Epsilon);
    }

    public bool IsApproximatelyEqual(double a_x, double a_y)
    {
        // If they are equal anyway, just return True. 
        if (a_x.Equals(a_y))
        {
            return true;
        }

        FloatingPoint fpMath = new();
        return fpMath.ApproximatelyEqual(a_x, a_y, Epsilon);
    }

    public double RoundQty(double a_d)
    {
        return Math.Round(a_d, Precision);
    }

    public decimal RoundQty(decimal a_d)
    {
        return Math.Round(a_d, Precision);
    }

    public decimal RoundQtyUp(decimal a_d)
    {
        return Math.Round(a_d, Precision, MidpointRounding.AwayFromZero);
    }

    public decimal RoundQtyWithZeroCheck(decimal a_d, decimal a_possibleEquivalent)
    {
        if (IsApproximatelyZero(a_d))
        {
            return a_possibleEquivalent;
        }

        return Math.Round(a_d, Precision);
    }
    #endregion

    public TimeSpan LegacyFrozenSpan;

    public bool AllowChangeOfMaterialSuccessorSequenceOnMove
    {
        get => m_boolVector[c_allowChangeOfMaterialSuccessorSequenceOnMoveIdx];
        set
        {
            m_boolVector[c_allowChangeOfMaterialSuccessorSequenceOnMoveIdx] = value;
            m_setBools2[c_allowChangeOfMaterialSuccessorIsSetIdx] = true;
        }
    }

    public bool AllowChangeOfMaterialSuccessorSequenceOnMoveIsSet => m_setBools2[c_allowChangeOfMaterialSuccessorIsSetIdx];

    public bool RestoreMaterialConstraintsOnOptimizedActivities
    {
        get => m_boolVector[c_restoreMaterialConstraintsOnOptimizedActivitiesIdx];
        set
        {
            m_boolVector[c_restoreMaterialConstraintsOnOptimizedActivitiesIdx] = value;
            m_setBools2[c_restoreMaterialConstraintsOnOptimizedActivitiesIsSetIdx] = true;
        }
    }

    public bool RestoreMaterialConstraintsOnOptimizedActivitiesIsSet => m_setBools2[c_restoreMaterialConstraintsOnOptimizedActivitiesIsSetIdx];

    /// <summary>
    /// When true, operations that start in the Frozen Span will ignore material constraints.
    /// </summary>
    public bool IgnoreMaterialConstraintsInFrozenSpan
    {
        get => m_boolVector[c_ignoreMaterialConstraintsInFrozenSpanIdx];
        set
        {
            m_boolVector[c_ignoreMaterialConstraintsInFrozenSpanIdx] = value;
            m_setBools2[c_ignoreMaterialConstraintsInFrozenSpanIsSetIdx] = true;
        }
    }

    public bool IgnoreMaterialConstraintsInFrozenSpanIsSet => m_setBools2[c_ignoreMaterialConstraintsInFrozenSpanIsSetIdx];

    public class ScenarioOptionsException : CommonException
    {
        public ScenarioOptionsException(string a_message)
            : base(a_message) { }
    }

    public void InitializeSetBools()
    {
        m_setBools.Clear();
        m_setBools2.Clear();
    }
}

public class ScenarioConstants
{
    public static string s_SCENARIO_DAT_FILE_EXTENSION = "scenarios.dat";
    public static string s_SCENARIO_DAT_FILE_EXTENSION_WILDCARD = "*" + s_SCENARIO_DAT_FILE_EXTENSION;
    public static string s_SCENARIO_DAT_FILE_EXTENSION_NEW = "." + s_SCENARIO_DAT_FILE_EXTENSION;
    public static string s_SCENARIO_DAT_FILE_EXTENSION_WILDCARD_NEW = "*" + s_SCENARIO_DAT_FILE_EXTENSION_NEW;
}
