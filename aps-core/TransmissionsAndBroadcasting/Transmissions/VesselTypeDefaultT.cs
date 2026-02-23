using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new VesselType in the specified Scenario using default values.
/// </summary>
public class VesselTypeDefaultT : VesselTypeBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 166;

    #region IPTSerializable Members
    public VesselTypeDefaultT(IReader reader)
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

    public VesselTypeDefaultT() { }

    public VesselTypeDefaultT(BaseId scenarioId, BaseId plantId, BaseId departmentId)
        : base(scenarioId, plantId, departmentId) { }

    public override string Description => "Vessel Type created";
}