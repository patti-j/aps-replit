using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Deleting Jobs.
/// </summary>
public class JobDeleteJobsT : JobBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 78;

    #region IPTSerializable Members
    public JobDeleteJobsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            jobs = new BaseIdList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        jobs.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public JobDeleteJobsT() { }

    public JobDeleteJobsT(BaseId scenarioId, BaseIdList jobs)
        : base(scenarioId)
    {
        this.jobs = jobs;
    }

    private readonly BaseIdList jobs;

    public BaseIdList Jobs => jobs;

    public override string Description => string.Format("Jobs deleted ({0})".Localize(), jobs.Count);
}