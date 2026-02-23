using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all ManufacturingOrder related transmissions.
/// </summary>
public abstract class ManufacturingOrderBaseT : JobIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 80;

    #region IPTSerializable Members
    public ManufacturingOrderBaseT(IReader reader)
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

    protected ManufacturingOrderBaseT() { }

    protected ManufacturingOrderBaseT(BaseId scenarioId, BaseId jobId)
        : base(scenarioId, jobId) { }
}