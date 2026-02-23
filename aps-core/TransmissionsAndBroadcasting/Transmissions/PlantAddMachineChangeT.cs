using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Transmission for adding a rESOURCE to a Plant.
/// </summary>
public class PlantAddResourceT : PlantIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 87;

    #region IPTSerializable Members
    public PlantAddResourceT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            machineId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        machineId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly BaseId machineId;

    public PlantAddResourceT() { }

    public PlantAddResourceT(BaseId scenarioId, BaseId plantId, BaseId machineId)
        : base(scenarioId, plantId)
    {
        this.machineId = machineId;
    }

    public override string Description => "Resource added to Plant";
}