using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all VesselType related transmissions.
/// </summary>
public abstract class VesselTypeIdBaseT : VesselTypeBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 169;

    #region IPTSerializable Members
    public VesselTypeIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            vesselTypeId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        vesselTypeId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId vesselTypeId;

    protected VesselTypeIdBaseT() { }

    protected VesselTypeIdBaseT(BaseId scenarioId, BaseId plantId, BaseId departmentId, BaseId vesselTypeId)
        : base(scenarioId, plantId, departmentId)
    {
        this.vesselTypeId = vesselTypeId;
    }
}