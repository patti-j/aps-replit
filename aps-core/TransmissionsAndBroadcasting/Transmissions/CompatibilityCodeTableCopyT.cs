using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Copy a specific Table.
/// </summary>
public class CompatibilityCodeTableCopyT : CompatibilityCodeTableIdBaseT
{
    #region IPTSerializable Members
    public CompatibilityCodeTableCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 5054;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CompatibilityCodeTableCopyT() { }

    public CompatibilityCodeTableCopyT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    public override string Description => "Compatibility Code Table copied";
}