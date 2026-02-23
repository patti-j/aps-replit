using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Holding or Unholding a list of Jobs.
/// </summary>
public class ScenarioDetailHoldJobsT : ScenarioIdBaseT, IPTSerializable
{
    public static readonly int UNIQUE_ID = 128;

    #region IPTSerializable Members
    public ScenarioDetailHoldJobsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out holdit);
            reader.Read(out holdReason);

            reader.Read(out holdUntilDate);

            m_jobs = new BaseIdList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(holdit);
        writer.Write(holdReason);

        writer.Write(holdUntilDate);

        m_jobs.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailHoldJobsT() { }

    /// <summary>
    /// HoldUntilDate is only used if holdIt is true.
    /// </summary>
    public ScenarioDetailHoldJobsT(BaseId scenarioId, BaseIdList jobs, bool holdit, DateTime holdUntilDate, string holdReason)
        : base(scenarioId)
    {
        this.holdit = holdit;
        this.holdReason = holdReason;
        this.holdUntilDate = holdUntilDate;
        m_jobs = jobs;
    }

    private readonly BaseIdList m_jobs;
    public BaseIdList Jobs => m_jobs;

    private readonly bool holdit;

    public bool Holdit => holdit;

    private readonly DateTime holdUntilDate;

    public DateTime HoldUntilDate => holdUntilDate;

    private readonly string holdReason;

    public string HoldReason => holdReason;

    public override string Description
    {
        get
        {
            string holdString = Holdit ? "Held" : "Freed";
            return Jobs.Count > 1 ? string.Format("Jobs {0} ({1})".Localize(), holdString, m_jobs.Count) : string.Format("Job {0}", holdString);
        }
    }
}