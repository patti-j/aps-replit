using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new CapacityInterval in the specified Scenario using default values.
/// </summary>
public class CapacityIntervalDefaultT : CapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 40;

    #region IPTSerializable Members
    public CapacityIntervalDefaultT(IReader reader)
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

    public CapacityIntervalDefaultT() { }

    public CapacityIntervalDefaultT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Capacity Created".Localize();
}