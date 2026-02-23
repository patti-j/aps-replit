using PT.APSCommon;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Summary description for ManufacturingOrderKey.
/// </summary>
public class ManufacturingOrderKey : IPTSerializable
{
    public const int UNIQUE_ID = 179;

    #region IPTSerializable Members
    public ManufacturingOrderKey(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            jobId = new BaseId(reader);
            moId = new BaseId(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        jobId.Serialize(writer);
        moId.Serialize(writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ManufacturingOrderKey(BaseId jobId, BaseId moId)
    {
        this.jobId = jobId;
        this.moId = moId;
    }

    private readonly BaseId jobId;

    public BaseId JobId => jobId;

    private readonly BaseId moId;

    public BaseId MOId => moId;
}