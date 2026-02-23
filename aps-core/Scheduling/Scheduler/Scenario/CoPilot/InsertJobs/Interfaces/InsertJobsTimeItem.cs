namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// A shared class for InsertJobs. Multiple modules can use this class to store and retrieve only the information required.
/// This class is shared to allow for multiple class to implement the same Interface.
/// </summary>
internal class InsertJobsTimeItem
{
    public long Ticks;
    public int PathIndex;
    public int PathPreference;

    public InsertJobsTimeItem(long a_ticks)
    {
        Ticks = a_ticks;
    }

    public InsertJobsTimeItem(long a_ticks, int a_pathIdx, int a_pathPreference)
    {
        Ticks = a_ticks;
        PathIndex = a_pathIdx;
        PathPreference = a_pathPreference;
    }
}