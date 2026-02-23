using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Plant by copying the specified Plant.
/// </summary>
public class PlantCopyT : PlantBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 90;

    #region IPTSerializable Members
    public PlantCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            originalId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        originalId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId originalId; //Id of the Plant to copy.

    public PlantCopyT() { }

    public PlantCopyT(BaseId scenarioId, BaseId originalId)
        : base(scenarioId)
    {
        this.originalId = originalId;
    }

    public override string Description => "Plant copied";
}