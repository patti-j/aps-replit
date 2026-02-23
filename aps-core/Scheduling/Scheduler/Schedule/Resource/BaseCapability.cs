using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Superclass for other Capabilities that are used for specifiying which Resources are Eligible for a ResourceRequirement.
/// </summary>
public abstract class BaseCapability : BaseObject, IPTSerializable
{
    public new const int UNIQUE_ID = 2;

    #region IPTSerializable Members
    public BaseCapability(IReader reader)
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

    protected BaseCapability(BaseId id, PT.ERPTransmissions.BaseCapability capability)
        : base(id, capability) { }

    protected BaseCapability(BaseId id)
        : base(id) { }
}