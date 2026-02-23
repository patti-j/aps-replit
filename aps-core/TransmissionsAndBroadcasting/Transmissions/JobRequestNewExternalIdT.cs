using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Triggers the request for a new unique Job ExternalId.
/// </summary>
public class JobRequestNewExternalIdT : JobBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 445;

    #region IPTSerializable Members
    public JobRequestNewExternalIdT(IReader reader)
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

    public JobRequestNewExternalIdT() { }

    public JobRequestNewExternalIdT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "External Job ID Requested".Localize();
}