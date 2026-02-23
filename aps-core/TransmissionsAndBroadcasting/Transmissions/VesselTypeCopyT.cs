using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new VesselType by copying the specified VesselType.
/// </summary>
public class VesselTypeCopyT : VesselTypeBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 165;

    #region IPTSerializable Members
    public VesselTypeCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            originalId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        originalId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId originalId; //Id of the VesselType to copy.

    public VesselTypeCopyT() { }

    public VesselTypeCopyT(BaseId scenarioId, BaseId plantId, BaseId departmentId, BaseId originalId)
        : base(scenarioId, plantId, departmentId)
    {
        this.originalId = originalId;
    }

    public override string Description => "Vessel Type copied";
}