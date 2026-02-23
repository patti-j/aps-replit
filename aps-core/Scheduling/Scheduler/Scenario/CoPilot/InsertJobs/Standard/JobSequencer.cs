using PT.APSCommon;
using PT.Scheduler.PackageDefs;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// This class stores and uses modules added during simulation initialization to alter its functionality.
/// It handles creating and managing the list of Job BaseIds used in InsertJobs.
/// BaseIds are used instead of referenfces because the scenario will be copied and the references would be lost.
/// </summary>
internal class JobSequencer
{
    private readonly List<IJobSequencerFilter> m_filterList;
    private readonly List<JobToInsert> m_jobList;
    private CoPilotSimulationUtilities m_simutiltiies;
    private IJobSequencerGroupingLogic m_groupingLogic;
    private IJobSequencerSortingLogic m_sortingLogic;

    internal JobSequencer(IPackageManager a_packageManager)
    {
        m_filterList = new List<IJobSequencerFilter>();
        m_jobList = new List<JobToInsert>();
        m_simutiltiies = new CoPilotSimulationUtilities(a_packageManager);
    }

    public void SetGroupingLogic(IJobSequencerGroupingLogic a_groupingLogic)
    {
        m_groupingLogic = a_groupingLogic;
    }

    public void AddFilter(IJobSequencerFilter a_filter)
    {
        m_filterList.Add(a_filter);
    }

    public void SetPrioritySortingLogic(IJobSequencerSortingLogic a_sortingLogic)
    {
        m_sortingLogic = a_sortingLogic;
    }

    public bool IsJobFiltered(Job a_jobToAdd, out string o_filterReason)
    {
        for (int i = 0; i < m_filterList.Count; i++)
        {
            bool filtered = m_filterList[i].ShouldExludeJob(a_jobToAdd, out o_filterReason);
            if (filtered)
            {
                return true;
            }
        }

        o_filterReason = "";
        return false;
    }

    /// <summary>
    /// Builds a list of job BaseIds that need to be inserted (new and not DoNotSchedule).
    /// Jobs are sorted by earliest need date ascending.
    /// </summary>
    public List<Tuple<BaseId, string>> CreateJobSet(Scenario a_workingScenario, BaseIdList a_jobSourceList)
    {
        m_jobList.Clear();
        List<Tuple<BaseId, string>> excludedReasonsList = new ();
        List<Job> workingJobList = new ();
        ScenarioDetail newSd;
        using (a_workingScenario.ScenarioDetailLock.EnterRead(out newSd))
        {
            foreach (BaseId baseId in a_jobSourceList)
            {
                Job currentJob = newSd.JobManager.GetById(baseId);
                if (currentJob != null)
                {
                    string filterReason;
                    if (!IsJobFiltered(currentJob, out filterReason))
                    {
                        workingJobList.Add(currentJob);
                    }
                    else if (filterReason != "")
                    {
                        excludedReasonsList.Add(new Tuple<BaseId, string>(currentJob.Id, filterReason));
                    }
                }
            }
        }

        m_sortingLogic.SortByPriority(workingJobList);

        for (int i = 0; i < workingJobList.Count; i++)
        {
            m_jobList.Add(new JobToInsert(workingJobList[i].Id));
        }

        return excludedReasonsList;
    }

    /// <summary>
    /// Returns a list of job Ids to insert in the next simulation.
    /// </summary>
    public List<JobToInsert> GetNextExpediteSet()
    {
        return m_groupingLogic.GetNextExpediteSet(m_jobList);
    }

    /// <summary>
    /// Removes the current set from the list
    /// </summary>
    public void RemoveCurrentSet()
    {
        m_groupingLogic.RemoveCurrentSet(m_jobList);
    }

    /// <summary>
    /// Returns the remaining sets of jobs to insert.
    /// </summary>
    public int GetCount()
    {
        return m_groupingLogic.GetRemainingSetsCount(m_jobList);
    }
}

/// <summary>
/// Holds information that goes along with the Job. This information can be used in the simulation.
/// </summary>
internal class JobToInsert
{
    public BaseId Id;

    public JobToInsert(BaseId a_id)
    {
        Id = a_id;
    }
}