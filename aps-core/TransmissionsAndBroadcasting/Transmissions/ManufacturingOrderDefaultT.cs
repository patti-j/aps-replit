using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new ManufacturingOrder in the specified Scenario using default values.
/// </summary>
public class ManufacturingOrderDefaultT : ManufacturingOrderBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 83;

    #region IPTSerializable Members
    public ManufacturingOrderDefaultT(IReader reader)
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

    public ManufacturingOrderDefaultT() { }

    public ManufacturingOrderDefaultT(BaseId scenarioId, BaseId jobId)
        : base(scenarioId, jobId) { }

    public override string Description => "ManufacturingOrder created";
}