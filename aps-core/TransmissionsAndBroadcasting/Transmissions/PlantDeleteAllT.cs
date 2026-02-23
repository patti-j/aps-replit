using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all Plants in the specified Scenario (and all of their Resources).
/// </summary>
public class PlantDeleteAllT : PlantBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 92;

    #region IPTSerializable Members
    public PlantDeleteAllT(IReader reader)
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

    public PlantDeleteAllT() { }

    public PlantDeleteAllT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "All Plants deleted";
}