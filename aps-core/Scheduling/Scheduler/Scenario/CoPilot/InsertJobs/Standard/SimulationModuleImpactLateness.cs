namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// Analyzes scenario data and returns the appropriate simulation result.
/// </summary>
internal class SimulationModuleImpactNoLateness : ISimulationModuleImpactAnalysis
{
    public SimulationModuleImpactNoLateness(long a_startingLateness)
    {
        m_startingLateness = a_startingLateness;
    }

    private readonly long m_startingLateness;

    public bool AnalyzeImpact(List<Job> a_expeditedJobList, ScenarioDetail a_sd)
    {
        //Caluate and record changes
        long totolJobLateness = a_sd.JobManager.CalcTotalJobLateness();
        if (totolJobLateness <= m_startingLateness)
        {
            //The job was expedited and no additional jobs are later
            return true;
        }

        return false;
    }
}

/// <summary>
/// Analyzes scenario data and returns the appropriate simulation result.
/// </summary>
internal class SimulationModuleImpactNoOtherLateness : ISimulationModuleImpactAnalysis
{
    public SimulationModuleImpactNoOtherLateness(long a_startingLateness)
    {
        m_startingLateness = a_startingLateness;
    }

    private readonly long m_startingLateness;

    public bool AnalyzeImpact(List<Job> a_expeditedJobList, ScenarioDetail a_sd)
    {
        //Caluate and record changes
        long totolJobLateness = a_sd.JobManager.CalcTotalJobLateness();
        bool noNewJobLatenessExists = totolJobLateness <= m_startingLateness;
        //Make sure the job has scheduled. It is possible that it failed to schedule or there was an error.
        if (noNewJobLatenessExists)
        {
            //The job was expedited and no additional jobs are later
            return true;
        }

        //Check if scheduled past planning horizon. If so, accept the result.
        long earliestScheduledTime = long.MaxValue;
        for (int i = 0; i < a_expeditedJobList.Count; i++)
        {
            earliestScheduledTime = Math.Min(earliestScheduledTime, a_expeditedJobList[i].ScheduledStartDate.Ticks);
        }

        if (earliestScheduledTime >= a_sd.GetPlanningHorizonEnd().Ticks)
        {
            return true;
        }

        //Check if any other jobs are late.
        long expediteJobLateness = 0;
        for (int i = 0; i < a_expeditedJobList.Count; i++)
        {
            expediteJobLateness += Math.Max(0, a_expeditedJobList[i].Lateness.Ticks);
        }

        if (totolJobLateness <= m_startingLateness + expediteJobLateness)
        {
            //The job was expedited and only it was late.
            return true;
        }

        return false;
    }
}

/// <summary>
/// Analyzes scenario data and returns the appropriate simulation result.
/// </summary>
internal class SimulationModuleImpactJobScheduled : ISimulationModuleImpactAnalysis
{
    public bool AnalyzeImpact(List<Job> a_expeditedJobList, ScenarioDetail a_sd)
    {
        for (int i = 0; i < a_expeditedJobList.Count; i++)
        {
            if (!a_expeditedJobList[i].Scheduled)
            {
                return false;
            }
        }

        return true;
    }
}