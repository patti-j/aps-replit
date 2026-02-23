using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Job related transmissions.
/// </summary>
public abstract class JobIdBaseT : JobBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 79;

    #region IPTSerializable Members
    public JobIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            jobId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        jobId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId jobId;

    protected JobIdBaseT() { }

    protected JobIdBaseT(BaseId scenarioId, BaseId jobId)
        : base(scenarioId)
    {
        this.jobId = jobId;
    }
}