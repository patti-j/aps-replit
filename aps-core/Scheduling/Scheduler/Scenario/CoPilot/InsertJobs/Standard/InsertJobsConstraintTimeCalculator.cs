using PT.Common.Exceptions;
using PT.Scheduler.PackageDefs;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// This class stores and uses Modules to determine the datetime constraints for InsertJobs.
/// Modules are added to this class during simulation initialization.
/// </summary>
internal class InsertJobsConstraintTimeCalculator
{
    private readonly List<IConstraintTimeCalculatorModule> m_moduleList = new ();
    private byte[] m_sdByteArray;
    private byte[] m_ssByteArray;
    private readonly IPackageManager m_packageManager;

    public InsertJobsConstraintTimeCalculator(IPackageManager a_packageManager)
    {
        m_packageManager = a_packageManager;
    }

    /// <summary>
    /// Each TimeCalculator module is processed to determine the earliest schedule date. The max of all module return values is used.
    /// </summary>
    public long CalculateMinScheduleDate(Scenario a_workingScenario, List<JobToInsert> a_jobIdToExpedite)
    {
        if (m_moduleList.Count == 0)
        {
            throw new PTException("No min constraint time calculator modules added");
        }

        //The working scenario is stored incase the module needs to make a copy for simulations.
        CoPilotSimulationUtilities simUtilities = new (m_packageManager);
        simUtilities.CopyAndStoreScenario(a_workingScenario, out m_sdByteArray, out m_ssByteArray);
        long earliestExpediteTime = 0;
        for (int i = 0; i < m_moduleList.Count; i++)
        {
            long moduleCalculation = m_moduleList[i].CalculateMinTime(a_workingScenario, a_jobIdToExpedite, m_sdByteArray, m_ssByteArray, m_packageManager);
            if (moduleCalculation == long.MaxValue)
            {
                //Return the last calculation since no values can be larger. This means that the job cannot be scheduled.
                return moduleCalculation;
            }

            earliestExpediteTime = Math.Max(earliestExpediteTime, moduleCalculation);
        }

        return earliestExpediteTime;
    }

    /// <summary>
    /// Each TimeCalculator module is processed to determine the latest schedule date. The min of all module return values is used.
    /// </summary>
    public long CalculateMaxScheduleDate(Scenario a_workingScenario, List<JobToInsert> a_jobIdToExpedite)
    {
        if (m_moduleList.Count == 0)
        {
            throw new PTException("No max contraint time calculator modules added");
        }

        //The working scenario is stored incase the module needs to make a copy for simulations.
        CoPilotSimulationUtilities simUtilities = new (m_packageManager);
        simUtilities.CopyAndStoreScenario(a_workingScenario, out m_sdByteArray, out m_ssByteArray);

        long maxExpediteTime = long.MaxValue;
        for (int i = 0; i < m_moduleList.Count; i++)
        {
            long moduleCalculation = m_moduleList[i].CalculateMaxTime(a_workingScenario, a_jobIdToExpedite, m_sdByteArray, m_ssByteArray);
            maxExpediteTime = Math.Min(maxExpediteTime, moduleCalculation);
        }

        return maxExpediteTime;
    }

    public void AddModule(IConstraintTimeCalculatorModule a_module)
    {
        m_moduleList.Add(a_module);
    }

    public void RemoveModuleByType(IConstraintTimeCalculatorModule a_module)
    {
        for (int i = 0; i < m_moduleList.Count; i++)
        {
            if (a_module.GetType().Name == m_moduleList[i].GetType().Name)
            {
                m_moduleList.RemoveAt(i);
                return;
            }
        }
    }
}