using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Plant related transmissions.
/// </summary>
public abstract class PlantIdBaseT : PlantBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 94;

    #region IPTSerializable Members
    public PlantIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            plantId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        plantId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId plantId;

    protected PlantIdBaseT() { }

    protected PlantIdBaseT(BaseId scenarioId, BaseId plantId)
        : base(scenarioId)
    {
        this.plantId = plantId;
    }
}