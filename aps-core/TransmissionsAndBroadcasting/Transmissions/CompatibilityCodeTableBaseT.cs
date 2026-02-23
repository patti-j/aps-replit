using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base class for all SetupCodeTable related Transmissions.
/// </summary>
public class CompatibilityCodeTableBaseT : ScenarioIdBaseT
{
    #region IPTSerializable Members
    public CompatibilityCodeTableBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public const int UNIQUE_ID = 5050;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CompatibilityCodeTableBaseT() { }

    public CompatibilityCodeTableBaseT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Compatibility Code Table updated";
}