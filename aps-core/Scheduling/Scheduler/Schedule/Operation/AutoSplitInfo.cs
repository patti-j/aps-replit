using PT.APSCommon;
using PT.Common.Extensions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.Schedule.Operation;

public class AutoSplitInfo : IPTSerializable
{
    public AutoSplitInfo(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12423)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out short typeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)typeVal;
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out m_maxAutoSplitAmount);
            a_reader.Read(out int collectionCount);
            for (int i = 0; i < collectionCount; i++)
            {
                m_autoSplitOverrideCollection.Add(new BaseId(a_reader));
            }

            a_reader.Read(out collectionCount);

            for (int i = 0; i < collectionCount; i++)
            {
                BaseId actId = new BaseId(a_reader);
                BaseId predActId = new BaseId(a_reader);
                PredecessorSplitMappings.Add(actId, predActId);
            }
        }
        else if (a_reader.VersionNumber >= 12421)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out short typeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)typeVal;
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out m_maxAutoSplitAmount);
            a_reader.Read(out int collectionCount);
            for (int i = 0; i < collectionCount; i++)
            {
                m_autoSplitOverrideCollection.Add(new BaseId(a_reader));
            }
        }
        else if (a_reader.VersionNumber >= 12306)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out short typeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)typeVal;
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out m_maxAutoSplitAmount);
        }
        else
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out short typeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)typeVal;
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write((short)m_autoAutoSplitType);
        a_writer.Write(m_minAutoSplitAmount);
        a_writer.Write(m_maxAutoSplitAmount);
        a_writer.Write(m_autoSplitOverrideCollection.Count);

        foreach (BaseId activityId in m_autoSplitOverrideCollection)
        {
            activityId.Serialize(a_writer);
        }

        a_writer.Write(PredecessorSplitMappings.Count);

        foreach ((BaseId actId, BaseId predActId) in PredecessorSplitMappings)
        {
            actId.Serialize(a_writer);
            predActId.Serialize(a_writer);
        }
    }

    public AutoSplitInfo()
    {
        m_autoAutoSplitType = OperationDefs.EAutoSplitType.None;
    }

    public int UniqueId => 0; //TODO

    private OperationDefs.EAutoSplitType m_autoAutoSplitType;

    public OperationDefs.EAutoSplitType AutoSplitType
    {
        get => m_autoAutoSplitType;
        set => m_autoAutoSplitType = value;
    }

    private decimal m_minAutoSplitAmount;

    public decimal MinAutoSplitAmount
    {
        get => m_minAutoSplitAmount;
        set => m_minAutoSplitAmount = value;
    }

    public bool UseMinAutoSplitAmount => m_minAutoSplitAmount != 0 && m_minAutoSplitAmount != decimal.MinValue;

    private decimal m_maxAutoSplitAmount = int.MaxValue;

    public decimal MaxAutoSplitAmount
    {
        get => m_maxAutoSplitAmount;
        set => m_maxAutoSplitAmount = value;
    }

    public bool UseMaxAutoSplitAmount => m_maxAutoSplitAmount != 0m && m_maxAutoSplitAmount != decimal.MaxValue;

    public bool KeepSplitsOnSameResource
    {
        get => m_bools[c_keepSplitsOnSameResourceIdx];
        set => m_bools[c_keepSplitsOnSameResourceIdx] =  value;
    }

    /// <summary>
    /// A collection of activity baseIds whose SetupSpanOverride flag were set by auto-split
    /// </summary>
    private HashSet<BaseId> m_autoSplitOverrideCollection = new HashSet<BaseId>();
    /// <summary>
    /// Tracks the activity amongst the collection of activity whose SetupSpanOverride flag were set by an auto-split
    /// </summary>
    /// <param name="a_activityId">Id of the activity to track</param>
    internal void TrackActivity(BaseId a_activityId)
    {
        m_autoSplitOverrideCollection.AddIfNew(a_activityId);
    }
    /// <summary>
    /// Un-tracks the activity amongst the collection of activity whose SetupSpanOverride flag were set by an auto-split
    /// </summary>
    /// <param name="a_activityId">Id of the activity to un-track</param>
    internal void UnTrackActivity(BaseId a_activityId)
    {
        m_autoSplitOverrideCollection.Remove(a_activityId);
    }
    /// <summary>
    /// Checks and indicates if activity's SetupSpanOverride flag was set by an AutoSplit
    /// </summary>
    /// <param name="a_activityId"></param>
    /// <returns>True if the specified activity's SetupSpanOverride was set by an auto-split</returns>
    internal bool IsOverrideSet(BaseId a_activityId)
    {
        return m_autoSplitOverrideCollection.Contains(a_activityId);
    }
    private BoolVector32 m_bools;

    private const short c_operationIsAutoSplitIdx = 0;
    private const short c_keepSplitsOnSameResourceIdx = 1;

    #region SIMULATION
    internal void ResetSimulationStateVariables(InternalOperation a_op)
    {
        NewlySplitActivity = null;
        OriginalSplitSetupTime = -1;
        m_isScheduledInFrozenSpan = false;
    }

    internal bool CanAutoSplit(ScenarioDetail.SimulationType a_activeSimulationType, InternalOperation a_op, bool a_manualScheduleResource)
    {
        //We don't want to split or unsplit ops scheduled in the Frozen Span
        if (m_isScheduledInFrozenSpan)
        {
            return false;
        }

        //Can't be locked
        if (a_op.Locked != lockTypes.Unlocked)
        {
            return false;
        }

        if (a_manualScheduleResource)
        {
            //If it's already on a manual schedule resource, it will be left there and we don't want to change it.
            return false;
        }
        
        if (a_activeSimulationType is not ScenarioDetail.SimulationType.Optimize && (a_activeSimulationType is not ScenarioDetail.SimulationType.Expedite || !a_op.ManufacturingOrder.BeingExpedited))
        {
            return false;
        }

        //Once split, any started activities lock the operation in this split configuration.
        return a_op.MaxActivityProductionStatus < InternalActivityDefs.productionStatuses.Started;
    }

    internal bool CanAutoJoin(ScenarioDetail.SimulationType a_activeSimulationType, InternalOperation a_op, bool a_scheduledOnManualScheduleOnlyResource)
    {
        return CanAutoSplit(a_activeSimulationType, a_op, a_scheduledOnManualScheduleOnlyResource);
    }

    internal InternalActivity NewlySplitActivity;

    internal bool OperationIsAutoSplit
    {
        get => m_bools[c_operationIsAutoSplitIdx];
    }

    internal void Split()
    {
        m_bools[c_operationIsAutoSplitIdx] = true;
    }

    internal void Unsplit()
    {
        PredecessorSplitMappings.Clear();
        m_bools[c_operationIsAutoSplitIdx] = false;
    }

    /// <summary>
    /// The activity setup duration before it was split. This is used to restore the setup time if the split activity fails to schedule
    /// </summary>
    internal long OriginalSplitSetupTime = -1;

    /// <summary>
    /// A simulation collection of predecessor activities that have already been mapped for this operation.
    /// For example when splitting by predecessor materials, when a split occurs, the successor and predecessor source activity will be added.
    /// </summary>
    internal Dictionary<BaseId, BaseId> PredecessorSplitMappings = new();

    private bool m_isScheduledInFrozenSpan;
    #endregion

    public BaseId GetPredecessorActivityId(BaseId a_succActId)
    {
        if (PredecessorSplitMappings.TryGetValue(a_succActId, out BaseId predActId))
        {
            return predActId;
        }

        return BaseId.NULL_ID;
    }

    public void Update(JobT.ResourceOperation a_jobTInternalOperation)
    {
        m_autoAutoSplitType = a_jobTInternalOperation.SplitType;
        m_minAutoSplitAmount = a_jobTInternalOperation.MinAutoSplitAmount;
        m_maxAutoSplitAmount = a_jobTInternalOperation.MaxAutoSplitAmount;

        if (a_jobTInternalOperation.KeepSplitsOnSameResourceIsSet && KeepSplitsOnSameResource != a_jobTInternalOperation.KeepSplitsOnSameResource)
        {
            KeepSplitsOnSameResource = a_jobTInternalOperation.KeepSplitsOnSameResource;
        }
    }

    public bool Update(AutoSplitInfo a_sourceInfo)
    {
        bool criticalUpdate = false;
        if (m_autoAutoSplitType != a_sourceInfo.AutoSplitType)
        {
            m_autoAutoSplitType = a_sourceInfo.AutoSplitType;
            criticalUpdate = true;
        }

        if (m_minAutoSplitAmount != a_sourceInfo.MinAutoSplitAmount)
        {
            m_minAutoSplitAmount = a_sourceInfo.MinAutoSplitAmount;
            criticalUpdate = true;
        }

        if (m_maxAutoSplitAmount != a_sourceInfo.MaxAutoSplitAmount)
        {
            m_maxAutoSplitAmount = a_sourceInfo.MaxAutoSplitAmount;
            criticalUpdate = true;
        }

        if (KeepSplitsOnSameResource != a_sourceInfo.KeepSplitsOnSameResource)
        {
            KeepSplitsOnSameResource = a_sourceInfo.KeepSplitsOnSameResource;
            criticalUpdate = true;
        }

        return criticalUpdate;
    }

    public bool Edit(OperationEdit a_opEdit)
    {
        bool updated = false;
        if (a_opEdit.AutoSplitTypeIsSet && m_autoAutoSplitType != a_opEdit.AutoSplitType)
        {
            m_autoAutoSplitType = a_opEdit.AutoSplitType;
            updated = true;
        }

        if (a_opEdit.MinAutoSplitAmountIsSet && MinAutoSplitAmount != a_opEdit.MinAutoSplitAmount)
        {
            MinAutoSplitAmount = a_opEdit.MinAutoSplitAmount;
            updated = true;
        }

        if (a_opEdit.MaxAutoSplitAmountIsSet && MaxAutoSplitAmount != a_opEdit.MaxAutoSplitAmount)
        {
            MaxAutoSplitAmount = a_opEdit.MaxAutoSplitAmount;
            updated = true;
        }

        if (a_opEdit.KeepSplitsOnSameResourceIsSet && KeepSplitsOnSameResource != a_opEdit.KeepSplitsOnSameResource)
        {
            KeepSplitsOnSameResource = a_opEdit.KeepSplitsOnSameResource;
        }

        return updated;
    }

    public void SetInFrozenSpan()
    {
        m_isScheduledInFrozenSpan = true;
    }
}