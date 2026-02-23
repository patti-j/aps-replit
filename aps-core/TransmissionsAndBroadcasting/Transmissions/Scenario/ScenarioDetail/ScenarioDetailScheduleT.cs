using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Scheduling or Unscheduling a list of Jobs.
/// </summary>
public class ScenarioDetailScheduleJobsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 133;

    #region IPTSerializable Members
    public ScenarioDetailScheduleJobsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out schedule);

            m_jobs = new BaseIdList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(schedule);

        m_jobs.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailScheduleJobsT() { }

    public ScenarioDetailScheduleJobsT(BaseId scenarioId, BaseIdList jobs, bool schedule)
        : base(scenarioId)
    {
        this.schedule = schedule;
        m_jobs = jobs;
    }

    private readonly BaseIdList m_jobs;
    public BaseIdList Jobs => m_jobs;
    private readonly bool schedule;

    public bool Schedule => schedule;

    public override string Description
    {
        get
        {
            string jobScheduleString = Schedule ? "Scheduled".Localize() : "Unscheduled".Localize();
            return Jobs.Count > 1 ? string.Format("Jobs {0} ({1})".Localize(), jobScheduleString, Jobs.Count) : string.Format("Job {0}".Localize(), jobScheduleString);
        }
    }
}