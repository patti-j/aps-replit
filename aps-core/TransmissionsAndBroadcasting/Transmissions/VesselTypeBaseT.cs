using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all VesselType related transmissions.
/// </summary>
public abstract class VesselTypeBaseT : BaseResourceT, IPTSerializable
{
    public new const int UNIQUE_ID = 163;

    #region IPTSerializable Members
    public VesselTypeBaseT(IReader reader)
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

    protected VesselTypeBaseT() { }

    protected VesselTypeBaseT(BaseId scenarioId, BaseId plantId, BaseId departmentId)
        : base(scenarioId, plantId, departmentId) { }
}