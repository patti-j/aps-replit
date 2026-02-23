using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the Plant (and all of its Resources).
/// </summary>
public class PlantDeleteT : PlantIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 93;

    #region IPTSerializable Members
    public PlantDeleteT(IReader reader)
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

    public PlantDeleteT() { }

    public PlantDeleteT(BaseId scenarioId, BaseId plantId)
        : base(scenarioId, plantId) { }

    public override string Description => "Plant deleted";
}