using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Capability by copying the specified Capability.
/// </summary>
public class CapabilityCopyT : CapabilityBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 30;

    #region IPTSerializable Members
    public CapabilityCopyT(IReader reader)
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

    public BaseId originalId; //Id of the Capability to copy.
    
    public CapabilityCopyT() { }

    public CapabilityCopyT(BaseId scenarioId, BaseId originalId)
        : base(scenarioId)
    {
        this.originalId = originalId;
    }

    public override string Description => "Capability copied";
}