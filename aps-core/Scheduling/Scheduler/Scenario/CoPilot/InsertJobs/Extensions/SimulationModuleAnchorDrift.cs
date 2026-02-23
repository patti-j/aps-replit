using PT.APSCommon;

namespace PT.Scheduler.CoPilot.InsertJobs;

internal class SimulationModuleAnchorDrift : ISimulationModuleImpactAnalysis
{
    public SimulationModuleAnchorDrift(List<Tuple<BaseId, long>> a_startingAnchorDrift)
    {
        m_startingAnchorDrift = a_startingAnchorDrift;
    }

    private readonly List<Tuple<BaseId, long>> m_startingAnchorDrift = new ();

    public bool AnalyzeImpact(List<Job> a_expeditedJobList, ScenarioDetail a_sd)
    {
        List<Tuple<BaseId, long>> currentAnchorDrift = a_sd.JobManager.CalculateJobAnchorDrift();

        //Loop through all jobs and return false if any of the new anchor drift is larger than the starting drift.
        foreach (Tuple<BaseId, long> anchorSet in currentAnchorDrift)
        {
            //If the job being inserted is anchored, don't count it
            bool isExpeditedJob = false;
            foreach (Job job in a_expeditedJobList)
            {
                if (job.Id == anchorSet.Item1)
                {
                    isExpeditedJob = true;
                    break;
                }
            }

            if (isExpeditedJob)
            {
                continue;
            }

            foreach (Tuple<BaseId, long> startingSet in m_startingAnchorDrift)
            {
                if (startingSet.Item1 == anchorSet.Item1)
                {
                    if (anchorSet.Item2 > startingSet.Item2)
                    {
                        return false;
                    }

                    break;
                }
            }
        }

        return true;
    }
}