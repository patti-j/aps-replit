using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Capability related transmissions.
/// </summary>
public abstract class CapabilityIdBaseT : CapabilityBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 34;

    #region IPTSerializable Members
    public CapabilityIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            machineCapabilityId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        machineCapabilityId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId machineCapabilityId;

    protected CapabilityIdBaseT() { }

    public CapabilityIdBaseT(BaseId scenarioId, BaseId machineCapabilityId)
        : base(scenarioId)
    {
        this.machineCapabilityId = machineCapabilityId;
    }
}