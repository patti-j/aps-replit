using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Delete a specific table.
/// </summary>
public class CompatibilityCodeTableDeleteT : CompatibilityCodeTableIdBaseT
{
    #region IPTSerializable Members
    public CompatibilityCodeTableDeleteT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 5053;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CompatibilityCodeTableDeleteT() { }

    public CompatibilityCodeTableDeleteT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    public override string Description => "Compatibility Code Table deleted";
}