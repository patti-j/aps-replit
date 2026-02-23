using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Plant in the specified Scenario using default values.
/// </summary>
public class PlantDefaultT : PlantBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 91;

    #region IPTSerializable Members
    public PlantDefaultT(IReader reader)
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

    public PlantDefaultT() { }

    public PlantDefaultT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Plant Created".Localize();
}