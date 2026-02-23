using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Create a new Job.
/// </summary>
public abstract class JobNewT : JobBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 588;

    #region IPTSerializable Members
    public JobNewT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected JobNewT() { }

    protected JobNewT(BaseId scenarioId)
        : base(scenarioId) { }
}