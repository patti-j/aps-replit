using System.Collections.Concurrent;

using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// This class caches impact analysis information based on job id. Individual impact information can be requested. If it has not been calculated yet, the entire Impact for the specified job is calculated
/// and cached.
/// Threadsafe
/// </summary>
public class CachedSnapshotComparison
{
    private readonly JobInfoList m_before;
    private readonly JobInfoList m_after;
    private readonly ConcurrentDictionary<BaseId, ImpactInfo> m_impactCache = new ();
    private readonly string m_comparisonDescription;

    public CachedSnapshotComparison(JobInfoList a_before, JobInfoList a_after, string a_description)
    {
        m_before = a_before;
        m_after = a_after;
        m_comparisonDescription = a_description;
    }

    public string ComparisonDescription => m_comparisonDescription;

    public ImpactInfo GetJobImpactValues(BaseId a_jobId)
    {
        ImpactInfo info = GetJobInfosForJob(a_jobId);

        return info;
    }

    /// <summary>
    /// Finds the Job infos for the specified ID.
    /// </summary>
    /// <returns>Whether both a before and after info were found</returns>
    private ImpactInfo GetJobInfosForJob(BaseId a_jobId)
    {
        ImpactInfo info;
        if (m_impactCache.TryGetValue(a_jobId, out info))
        {
            return info;
        }

        //Need to calculate the Impact
        JobInfo beforeInfo = null;
        JobInfo afterInfo = null;

        bool valueFound = false;

        for (int i = 0; i < m_before.Count; i++)
        {
            if (m_before[i].Id == a_jobId)
            {
                beforeInfo = m_before[i];
                valueFound = true;
                break;
            }
        }

        if (!valueFound)
        {
            m_impactCache.TryAdd(a_jobId, null);
            return null;
        }

        valueFound = false;
        for (int i = 0; i < m_after.Count; i++)
        {
            if (m_after[i].Id == a_jobId)
            {
                afterInfo = m_after[i];
                valueFound = true;
                break;
            }
        }

        if (!valueFound)
        {
            m_impactCache.TryAdd(a_jobId, null);
            return null;
        }

        //Both infos exist, cache the Impact
        ImpactInfo newImpact = new (beforeInfo, afterInfo);
        m_impactCache.TryAdd(a_jobId, newImpact);
        return newImpact;
    }
}

public class ImpactInfo
{
    public bool Late, Later, LessLate, OnTime, Earlier, LessEarly, NewlyUnscheduled, NewlyScheduled, LeftFrozenSpan, EnteredFrozenSpan;
    public TimeSpan LatenessChange;

    internal ImpactInfo(JobInfo a_beforeInfo, JobInfo a_afterInfo)
    {
        Late = a_afterInfo.Scheduled && a_beforeInfo.Scheduled && a_afterInfo.Late && !a_beforeInfo.Late;
        Later = a_afterInfo.Scheduled && a_beforeInfo.Scheduled && a_afterInfo.Late && a_beforeInfo.Late && a_afterInfo.Lateness > a_beforeInfo.Lateness;
        LessLate = a_afterInfo.Scheduled && a_beforeInfo.Scheduled && a_afterInfo.Late && a_beforeInfo.Late && a_afterInfo.Lateness < a_beforeInfo.Lateness;
        OnTime = a_afterInfo.Scheduled && a_beforeInfo.Scheduled && !a_afterInfo.Late && a_beforeInfo.Late;
        Earlier = a_afterInfo.Scheduled && a_beforeInfo.Scheduled && !a_afterInfo.Late && !a_beforeInfo.Late && a_afterInfo.Lateness < a_beforeInfo.Lateness;
        LessEarly = a_afterInfo.Scheduled && a_beforeInfo.Scheduled && !a_afterInfo.Late && !a_beforeInfo.Late && a_afterInfo.Lateness > a_beforeInfo.Lateness;
        NewlyUnscheduled = !a_afterInfo.Scheduled && a_beforeInfo.Scheduled;
        NewlyScheduled = a_afterInfo.Scheduled && !a_beforeInfo.Scheduled;
        LeftFrozenSpan = !a_afterInfo.ScheduledInFrozenSpan && a_beforeInfo.ScheduledInFrozenSpan;
        EnteredFrozenSpan = a_afterInfo.ScheduledInFrozenSpan && !a_beforeInfo.ScheduledInFrozenSpan;
        LatenessChange = a_afterInfo.Lateness - a_beforeInfo.Lateness;
    }
}