using PT.APSCommon;
using PT.Common.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation.Scheduler.AlternatePaths;

/// <summary>
/// This class handles when there are multiple predecessors with transfer spans set
/// For the common case of a single predecessor, a single member variable is used
/// For more than 1 predecessor, a list is used to store the transfers
/// Transfers will be Merged so that there will only be one transfer info for each end point
/// </summary>
public class TransferInfoCollection : IPTSerializable
{
    private List<TransferInfo> m_transferInfoList;
    private TransferInfo m_singleTransferInfo; //For operations with only a single predecessor or single constraining transfer.
    private TransferInfo m_latestConstraint = new TransferInfo(); //The transfer that was most constraining on the last optimize.
    private long m_latestConstraintTime = PTDateTime.MinDateTime.Ticks; //The transfer that was most constraining on the last optimize.

    #region Serialization
    internal TransferInfoCollection(IReader a_reader)
    {
        a_reader.Read(out m_latestConstraintTime);

        a_reader.Read(out bool singleTransferInfoSet);
        if (singleTransferInfoSet)
        {
            m_singleTransferInfo = new TransferInfo(a_reader);
        }
        else
        {
            a_reader.Read(out bool transferInfoListNotNull);
            if (transferInfoListNotNull)
            {
                m_transferInfoList = new List<TransferInfo>();
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    m_transferInfoList.Add(new TransferInfo(a_reader));
                }
            }
        }
    }
    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_latestConstraintTime);

        bool singleTransferInfoSet = m_singleTransferInfo != null && m_singleTransferInfo.Set;
        a_writer.Write(singleTransferInfoSet);
        if (singleTransferInfoSet)
        {
            m_singleTransferInfo.Serialize(a_writer);
        }
        else
        {
            bool transferInfoListSet = m_transferInfoList?.Count > 0;
            a_writer.Write(transferInfoListSet);
            if (transferInfoListSet)
            {
                a_writer.Write(true);
                a_writer.Write(m_transferInfoList.Count);
                foreach (TransferInfo transferInfo in m_transferInfoList)
                {
                    transferInfo.Serialize(a_writer);
                }
            }
        }
    }

    public int UniqueId => 6921;
    #endregion
    internal TransferInfoCollection()
    {

    }

    public bool Set => (m_singleTransferInfo != null && m_singleTransferInfo.Set) || m_transferInfoList != null;

    internal void Merge(TransferInfo a_info)
    {

        if (m_transferInfoList == null) //Only one transfer so far
        {
            if (m_singleTransferInfo == null || !m_singleTransferInfo.Set)
            {
                //This is the first transfer
                m_singleTransferInfo = a_info;
            }
            else if (a_info.TransferSpanEndPoint == m_singleTransferInfo.TransferSpanEndPoint)
            {
                //Only the most constraining transfer info should be used
                if (a_info.MaxTransferEndTime > m_singleTransferInfo.MaxTransferEndTime)
                {
                    //Same endpoint, but longer duration, replace the existing transfer
                    m_singleTransferInfo = a_info;
                }
            }
            else
            {
                //We have different transfer end points, we need to store both
                m_transferInfoList = new List<TransferInfo>();
                m_transferInfoList.Add(m_singleTransferInfo);
                m_transferInfoList.Add(a_info);
                m_singleTransferInfo = new TransferInfo(); //Reset, we are now using the list
            }
        }
        else
        {
            //We already have more than one transfer, try to merge each
            for (var i = 0; i < m_transferInfoList.Count; i++)
            {
                TransferInfo existingTransfer = m_transferInfoList[i];
                if (a_info.TransferSpanEndPoint == existingTransfer.TransferSpanEndPoint)
                {
                    //Only the most constraining transfer info should be used
                    if (a_info.MaxTransferEndTime > existingTransfer.MaxTransferEndTime)
                    {
                        //Same endpoint, but longer duration, replace the existing transfer
                        m_transferInfoList[i] = a_info;
                    }

                    //We only need to store one transfer for each point. It was either replaced or skipped
                    return;
                }
            }

            //None of the existing points have the same transfer point, add this new one
            m_transferInfoList.Add(a_info);
        }
    }

    public List<(TransferInfo, RequiredCapacity, bool)> CalculateTransferCapacity(SchedulableInfo a_si, InternalActivity a_act)
    {
        List<(TransferInfo, RequiredCapacity, bool)> calculatedCapacityList = new();
        BaseId predActId = BaseId.NULL_ID;

        if (a_act.Operation.Split && a_act.Operation.AutoSplitInfo.PredecessorSplitMappings != null)
        {
            if (a_act.Operation.AutoSplitInfo.PredecessorSplitMappings.TryGetValue(a_act.Id, out BaseId predSplitId))
            {
                predActId = predSplitId;
            }
        }

        if (m_singleTransferInfo != null && m_singleTransferInfo.Set)
        {
            (bool, RequiredCapacity) capacity = CalculateCapacity(m_singleTransferInfo, a_si, predActId);
            calculatedCapacityList.Add((m_singleTransferInfo, capacity.Item2, capacity.Item1));
        }
        else
        {
            foreach (TransferInfo transferInfo in m_transferInfoList)
            {
                (bool, RequiredCapacity) capacity = CalculateCapacity(transferInfo, a_si, predActId);
                calculatedCapacityList.Add((transferInfo, capacity.Item2, capacity.Item1));
            }
        }

        return calculatedCapacityList;
    }

    private (bool, RequiredCapacity) CalculateCapacity(TransferInfo a_transferInfo, SchedulableInfo a_si, BaseId a_predActId)
    {
        RequiredCapacity transferRc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, RequiredSpanPlusSetup.s_notInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
        bool constraint = false;
        switch (a_transferInfo.TransferSpanEndPoint)
        {
            case OperationDefs.EOperationTransferPoint.StartOfOperation:
                if (a_si.m_scheduledStartDate < a_transferInfo.GetTransferEndForActivity(a_predActId))
                {
                    //This should not happen, as it would be constrained by the initial transfer release calculation
                    constraint = true;
                }
                break;

            case OperationDefs.EOperationTransferPoint.EndOfSetup:
                transferRc = new RequiredCapacity(a_si.m_requiredAdditionalCleanBeforeSpan, a_si.m_requiredSetupSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                if (a_si.m_setupFinishDate < a_transferInfo.GetTransferEndForActivity(a_predActId))
                {
                    constraint = true;
                }

                break;

            case OperationDefs.EOperationTransferPoint.EndOfRun:
                transferRc = new RequiredCapacity(a_si.m_requiredAdditionalCleanBeforeSpan, a_si.m_requiredSetupSpan, a_si.m_requiredProcessingSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                if (a_si.m_productionFinishDate < a_transferInfo.GetTransferEndForActivity(a_predActId))
                {
                    constraint = true;
                }

                break;

            case OperationDefs.EOperationTransferPoint.EndOfPostProcessing:
                transferRc = new RequiredCapacity(a_si.m_requiredAdditionalCleanBeforeSpan, a_si.m_requiredSetupSpan, a_si.m_requiredProcessingSpan, a_si.m_requiredPostProcessingSpan, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                if (a_si.m_requiredStorageSpan.TimeSpanTicks < a_transferInfo.GetTransferEndForActivity(a_predActId))
                {
                    constraint = true;
                }

                break;

            //Note Clean is intentionally not handled here. That is not a valid transfer configuration
            case OperationDefs.EOperationTransferPoint.EndOfOperation:
                transferRc = new RequiredCapacity(a_si.m_requiredAdditionalCleanBeforeSpan, a_si.m_requiredSetupSpan, a_si.m_requiredProcessingSpan, a_si.m_requiredPostProcessingSpan, a_si.m_requiredStorageSpan, RequiredSpanPlusClean.s_notInit);
                if (a_si.m_requiredStorageSpan.TimeSpanTicks < a_transferInfo.GetTransferEndForActivity(a_predActId))
                {
                    constraint = true;
                }

                break;
        }

        return (constraint, transferRc);
    }

    /// <summary>
    /// Returns the most constraining transfer, if set.
    /// </summary>
    public TransferInfo GetConstrainingTransfer()
    {
        if (m_latestConstraint.Set)
        {
            return m_latestConstraint;
        }

        if (m_singleTransferInfo != null && m_singleTransferInfo.Set)
        {
            return m_singleTransferInfo;
        }

        if (m_transferInfoList != null)
        {
            //TODO: How do we return more than one?
            return m_transferInfoList[0];
        }

        return new TransferInfo();
    }

    public void TrackLatestConstraint(TransferInfo a_transferInfo)
    {
        m_latestConstraint = a_transferInfo;
    }
}


public class TransferInfo : IPTSerializable
{
    public long MaxTransferEndTime => m_activityTransferEndTimes.Count > 0 ? m_activityTransferEndTimes.Values.Max() : m_defaultTransferEndTime;
    private readonly long m_defaultTransferEndTime;
    public readonly OperationDefs.EOperationTransferPoint TransferSpanEndPoint;

    public bool Set
    {
        get => m_boolVector32 [c_setIdx];
    }


    private readonly Dictionary<BaseId, long> m_activityTransferEndTimes = new Dictionary<BaseId, long>();
    private BoolVector32 m_boolVector32 = new BoolVector32();
    private const int c_setIdx = 0;
    #region Serialization
    public TransferInfo(IReader a_reader)
    {
        m_boolVector32 = new BoolVector32(a_reader);
        a_reader.Read(out int count);
        for (int i = 0; i < count; i++)
        {
            BaseId actId = new BaseId(a_reader);
            a_reader.Read(out long endTicks);

            m_activityTransferEndTimes.Add(actId, endTicks);
        }

        a_reader.Read(out m_defaultTransferEndTime);
        a_reader.Read(out short value);

        TransferSpanEndPoint = (OperationDefs.EOperationTransferPoint)value;
    }

    public void Serialize(IWriter a_writer)
    {
        m_boolVector32.Serialize(a_writer);
        a_writer.Write(m_activityTransferEndTimes.Count);
        foreach ((BaseId baseId, long endTicks) in m_activityTransferEndTimes)
        {
            baseId.Serialize(a_writer);
            a_writer.Write(endTicks);
        }
        a_writer.Write(m_defaultTransferEndTime);
        a_writer.Write((short)TransferSpanEndPoint);
    }

    public int UniqueId => 6920;

    #endregion

    //For Split operations
    internal TransferInfo(Dictionary<BaseId, long> a_activityTransferEnds, OperationDefs.EOperationTransferPoint a_endPoint)
    {
        m_activityTransferEndTimes.MergeWithOverride(a_activityTransferEnds);
        m_defaultTransferEndTime = a_activityTransferEnds.Values.Max();
        TransferSpanEndPoint = a_endPoint;
        m_boolVector32[c_setIdx] = true;
    }

    internal TransferInfo(long a_transferEndTime, OperationDefs.EOperationTransferPoint a_endPoint)
    {
        m_defaultTransferEndTime = a_transferEndTime;
        TransferSpanEndPoint = a_endPoint;
        m_boolVector32[c_setIdx] = true;
    }

    public TransferInfo()
    {
        m_defaultTransferEndTime = 0;
        TransferSpanEndPoint = OperationDefs.EOperationTransferPoint.StartOfOperation;
        m_boolVector32[c_setIdx] = false;
    }



    public long GetTransferEndForActivity(BaseId a_id)
    {
        if (m_activityTransferEndTimes.TryGetValue(a_id, out long transferEndTicks))
        {
            return transferEndTicks;
        }

        return m_defaultTransferEndTime;
    }
}