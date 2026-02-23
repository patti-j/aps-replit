using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// For creating an BaseResource via ERP transmission.
/// </summary>
public class BaseCapability : PTObjectBase, IPTSerializable
{
    public new const int UNIQUE_ID = 208;

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

    public BaseCapability() { }

    public BaseCapability(string externalId, string name, string description, string notes, string userFields)
        : base(externalId, name, description, notes, userFields) { }
}