using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all VesselTypes in the specified Scenario (and all of their Resources).
/// </summary>
public class VesselTypeDeleteAllT : VesselTypeBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 167;

    #region IPTSerializable Members
    public VesselTypeDeleteAllT(IReader reader)
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

    public VesselTypeDeleteAllT() { }

    public VesselTypeDeleteAllT(BaseId scenarioId, BaseId plantId, BaseId departmentId)
        : base(scenarioId, plantId, departmentId) { }

    public override string Description => "Deleted all Vessel Types";
}