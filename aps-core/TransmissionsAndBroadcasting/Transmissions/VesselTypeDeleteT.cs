using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the VesselType (and all of its Resources).
/// </summary>
public class VesselTypeDeleteT : VesselTypeIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 168;

    #region IPTSerializable Members
    public VesselTypeDeleteT(IReader reader)
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

    public VesselTypeDeleteT() { }

    public VesselTypeDeleteT(BaseId scenarioId, BaseId plantId, BaseId departmentId, BaseId vesselTypeId)
        : base(scenarioId, plantId, departmentId, vesselTypeId) { }

    public override string Description => "Vessel Type deleted";
}