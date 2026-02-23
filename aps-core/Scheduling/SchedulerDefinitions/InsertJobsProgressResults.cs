namespace PT.SchedulerDefinitions;

/// <summary>
/// Stores CoPilot settings for InsertNewJobs simulation. This is used in ScenarioManager and Modified in UI.
/// </summary>
public class InsertJobsProgressResults
{
    public const int UNIQUE_ID = 781;

    //InsertJobs TODO: Change names to BaseIds. Add class for the lists.
    public InsertJobsProgressResults()
    {
        m_jobNamesList = new List<string>();
        m_isLateList = new List<bool>();
        m_jobNotScheduledNamesList = new List<string>();
    }

    public virtual int UniqueId => UNIQUE_ID;

    /// <summary>
    /// Adds a job that has been scheduled and a bool for wheter it is late.
    /// </summary>
    /// <param name="a_jobName"></param>
    /// <param name="a_isJobLate"></param>
    public void AddJobScheduledResult(string a_jobName, bool a_isJobLate)
    {
        lock (m_listLock)
        {
            m_jobNamesList.Add(a_jobName);
            m_isLateList.Add(a_isJobLate);
        }
    }

    public void AddJobNotScheduledResult(string a_jobname)
    {
        lock (m_listLock)
        {
            m_jobNotScheduledNamesList.Add(a_jobname);
        }
    }

    public List<Tuple<string, bool>> GetJobsScheduledList()
    {
        List<Tuple<string, bool>> jobsList = new ();
        lock (m_listLock)
        {
            for (int i = 0; i < m_jobNamesList.Count; i++)
            {
                jobsList.Add(new Tuple<string, bool>(m_jobNamesList[i], m_isLateList[i]));
            }
        }

        return jobsList;
    }

    /// <summary>
    /// The number of jobs that the InsertJobs simulations is going to attempt to insert.
    /// </summary>
    public int TotalJobsToInsert;

    /// <summary>
    /// Gets the current number of progress events.
    /// This can be checked to determine if a new event has been added without getting the full list.
    /// </summary>
    public int ProgressResultsCount
    {
        get
        {
            lock (m_listLock)
            {
                return m_jobNamesList.Count + m_jobNotScheduledNamesList.Count;
            }
        }
    }

    private readonly List<string> m_jobNamesList;
    private readonly List<bool> m_isLateList;
    private readonly List<string> m_jobNotScheduledNamesList;
    private readonly object m_listLock = new ();
}