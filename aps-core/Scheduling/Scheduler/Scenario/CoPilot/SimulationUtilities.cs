using PT.APSCommon;
using PT.Scheduler.CoPilot.InsertJobs;
using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot;

/// <summary>
/// This class provides functionality common between CoPilot simulations and managers
/// </summary>
internal class CoPilotSimulationUtilities
{
    private readonly IPackageManager m_packageManager;

    internal CoPilotSimulationUtilities(IPackageManager a_packageManager)
    {
        m_packageManager = a_packageManager;
    }

    /// <summary>
    /// Copies the scenario into byte[] containers for use in the RuleSeek simulation.
    /// This is not a scenario that will be sent in a transmission.
    /// </summary>
    internal void CopyAndStoreScenario(Scenario a_scenario, out byte[] o_sdByteArray, out byte[] o_ssByteArray)
    {
        a_scenario.CopyAndStoreInByteArrays(out o_sdByteArray, out o_ssByteArray);
    }

    /// <summary>
    /// Copies a scenario from the byte[] containers previously generated in the constructor.
    /// This is not a scenario that will be sent in a transmission.
    /// </summary>
    internal Scenario CopySimScenario(byte[] a_sdByteArrayContainer, byte[] a_ssByteArrayContainer)
    {
        Scenario.CopyFromSerializedByteArray(out ScenarioDetail sdNew, out ScenarioSummary ssNew, a_sdByteArrayContainer, a_ssByteArrayContainer);

        Scenario newScenario = new (BaseId.NULL_ID, null, sdNew, ssNew, m_packageManager);
        newScenario.InitializeUndoSet();
        //TODO: Refactor this so that InitializeUndoSet is done in the constructor instead
        return newScenario;
    }

    /// <summary>
    /// Creates an expedite transmission from the list of Jobs and time
    /// </summary>
    /// <param name="a_job"></param>
    /// <param name="a_expediteTimeTicks"></param>
    /// <returns></returns>
    internal Transmissions.ScenarioDetailExpediteJobsT CreateExpediteT(List<JobToInsert> a_jobsList, long a_expediteTimeTicks)
    {
        BaseIdList jobIdsToExpedite = new ();
        for (int i = 0; i < a_jobsList.Count; i++)
        {
            jobIdsToExpedite.Add(a_jobsList[i].Id);
        }

        Transmissions.ScenarioDetailExpediteJobsT expediteT = new(BaseId.NULL_ID, jobIdsToExpedite, Transmissions.ScenarioDetailExpediteBaseT.ExpediteStartDateType.SpecificDateTime, a_expediteTimeTicks, false, false, false);
        //expediteT.ResToScheduleEligibleLeadActivitiesOn = a_targetResource.Id;
        return expediteT;
    }

    /// <summary>
    /// Creates an expedite transmission from the JobID and time
    /// </summary>
    /// <param name="a_job"></param>
    /// <param name="a_expediteTimeTicks"></param>
    /// <returns></returns>
    internal Transmissions.ScenarioDetailExpediteJobsT CreateExpediteT(BaseId a_jobsList, long a_expediteTimeTicks)
    {
        BaseIdList jobIdsToExpedite = new ();
        jobIdsToExpedite.Add(a_jobsList);
        Transmissions.ScenarioDetailExpediteJobsT expediteT = new (BaseId.NULL_ID, jobIdsToExpedite, Transmissions.ScenarioDetailExpediteBaseT.ExpediteStartDateType.SpecificDateTime, a_expediteTimeTicks, false, false, false);
        //expediteT.ResToScheduleEligibleLeadActivitiesOn = a_targetResource.Id;
        return expediteT;
    }

    /// <summary>
    /// Removes all unscheduled jobs that are not in the exclusions list.
    /// </summary>
    /// <param name="a_scenario"></param>
    /// <param name="a_exlcusions"></param>
    internal bool RemoveUnusedJobsForSimulation(Scenario a_scenario, BaseIdList a_exlcusions)
    {
        ScenarioDetail sd;

        using (a_scenario.ScenarioDetailLock.EnterWrite(out sd))
        {
            List<BaseId> jobsToInsertList = new ();
            foreach (BaseId baseId in a_exlcusions)
            {
                jobsToInsertList.Add(baseId);
            }

            BaseIdList jobsToRemove = new ();
            for (int i = 0; i < sd.JobManager.Count; i++)
            {
                Job currentJob = sd.JobManager.GetByIndex(i);
                if (!currentJob.Scheduled)
                {
                    if (!jobsToInsertList.Contains(currentJob.Id))
                    {
                        jobsToRemove.Add(currentJob.Id);
                    }
                }
            }

            if (jobsToRemove.Count > 0)
            {
                Transmissions.JobDeleteJobsT jobsDeleteT = new (BaseId.NULL_ID, jobsToRemove);
                sd.Receive(jobsDeleteT, new ScenarioDataChanges());
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Values set by the InsertJobsSimulationManager and used in the InsertJobsSimulations.
/// </summary>
internal class InsertJobsSimSettings
{
    internal InsertJobsSimSettings()
    {
        CurrentPhase = InsertJobsSimulationManager.SimulationPhase.ScheduleOnOrBeforeNeedDate;
    }

    public long StartingTotalJobLateness = 0;
    public List<Tuple<BaseId, long>> StartingTotalAnchorDrift = new ();
    public bool UseKpi;
    public string KpiName;
    public bool KpiIsLowerBetter;
    public List<JobToInsert> JobSetToExpedite;
    public InsertJobsSimulationManager.SimulationPhase CurrentPhase;
    public bool CheckAlternatePaths;
    public bool CheckKpiThreshold;
    public decimal KpiThreshold;
    public decimal CurrentKpi;
    public int MaxSimulations;
    public InsertJobsSimulationTypes SimType;
    public bool ConstrainByReleaseDate;
    public bool NoAnchorDrift;
}