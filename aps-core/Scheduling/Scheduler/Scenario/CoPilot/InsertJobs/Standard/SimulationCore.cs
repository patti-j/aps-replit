using PT.Common.Exceptions;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// Functionality used during InsertJobs simulations.  The functionality is separated into modules so that it can be changed or added to in the future.
/// </summary>
internal class SimulationCore
{
    public enum SimulationCoreResult { Complete, Skip }

    private readonly List<ISimulationModuleImpactAnalysis> m_impactAnalyzerList = new ();
    private readonly List<ISimulationCoreInsertPreProcessing> m_preProcessingList = new ();

    /// <summary>
    /// Checks all added modules to see if the current schedule is acceptable. Will only return true if all modules accept the current schedule.
    /// </summary>
    public bool AnalyzeImpact(List<Job> a_expediteJobList, ScenarioDetail a_sd)
    {
        for (int i = 0; i < m_impactAnalyzerList.Count; i++)
        {
            bool currentResult = m_impactAnalyzerList[i].AnalyzeImpact(a_expediteJobList, a_sd);
            if (!currentResult)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// All InsertPreProcessing modules will be run just before the actual expedite occurs.
    /// </summary>
    public void InsertPreProcessing(ScenarioDetail a_sd, List<JobToInsert> a_jobsIdsToInsert, IExpediteTimesList a_insertTimes)
    {
        for (int i = 0; i < m_preProcessingList.Count; i++)
        {
            m_preProcessingList[i].PerformPreProcessing(a_sd, a_jobsIdsToInsert, a_insertTimes);
        }
    }

    /// <summary>
    /// Adds a Simulation module. Any simulation type can be added here as long as it inherits from a ISimulationCoreType interface.
    /// An exception will be thrown if the type is not known.
    /// </summary>
    public void AddModule(object a_simModule)
    {
        if (a_simModule is ISimulationModuleImpactAnalysis)
        {
            m_impactAnalyzerList.Add(a_simModule as ISimulationModuleImpactAnalysis);
        }
        else if (a_simModule is ISimulationCoreInsertPreProcessing)
        {
            m_preProcessingList.Add(a_simModule as ISimulationCoreInsertPreProcessing);
        }
        else
        {
            throw new PTException("Invalid Simulation Module type: " + a_simModule.GetType());
        }
    }
}