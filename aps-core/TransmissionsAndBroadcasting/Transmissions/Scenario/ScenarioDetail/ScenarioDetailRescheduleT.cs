using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Rescheduling a list of Jobs.
/// </summary>
public class ScenarioDetailRescheduleJobsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 132;

    #region IPTSerializable Members
    public ScenarioDetailRescheduleJobsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_jobs = new BaseIdList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_jobs.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailRescheduleJobsT() { }

    public ScenarioDetailRescheduleJobsT(BaseId scenarioId, BaseIdList jobs)
        : base(scenarioId)
    {
        m_jobs = jobs;
    }

    private readonly BaseIdList m_jobs;
    public BaseIdList Jobs => m_jobs;

    public override string Description => "Reschedule Jobs";
}

/// <summary>
/// Transmission for Rescheduling a list of MOs.
/// </summary>
public class ScenarioDetailRescheduleMOsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 280;

    #region IPTSerializable Members
    public ScenarioDetailRescheduleMOsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            mos = new MOKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        mos.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailRescheduleMOsT() { }

    public ScenarioDetailRescheduleMOsT(BaseId scenarioId, MOKeyList mos)
        : base(scenarioId)
    {
        this.mos = mos;
    }

    private readonly MOKeyList mos;

    public MOKeyList MOs => mos;

    public override string Description => "Reschedule ManufacturingOrders";
}