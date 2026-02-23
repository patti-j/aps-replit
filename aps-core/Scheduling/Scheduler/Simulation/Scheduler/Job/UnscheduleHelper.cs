namespace PT.Scheduler;

public class UnscheduleHelper
{
    private readonly List<Job> m_unscheduledJobs = new ();

    //Key: Job.ExternalId
    //Used to make sure we are not unscheduling a job more than once
    private readonly HashSet<string> m_alreadyUnscheduledCheck = new ();

    public void AddJobToUnschedule(Job j)
    {
        if (j == null)
        {
            return;
        }

        //Only add to list if it has not previously been inserted
        if (!HasAlreadyBeenAdded(j))
        {
            m_unscheduledJobs.Add(j);
            m_alreadyUnscheduledCheck.Add(j.ExternalId);
        }
    }

    public bool UnscheduledJobsIsEmpty => m_unscheduledJobs.Count == 0;

    public Job GetNextJobToUnschedule()
    {
        Job j = m_unscheduledJobs[0];
        m_unscheduledJobs.RemoveAt(0);
        return j;
    }

    public bool HasAlreadyBeenAdded(Job a_j)
    {
        return m_alreadyUnscheduledCheck.Contains(a_j.ExternalId);
    }
}