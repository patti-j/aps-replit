using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Plant related transmissions.
/// </summary>
public class PlantSetResourceDeptT : PlantBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 516;

    #region IPTSerializable Members
    public PlantSetResourceDeptT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            oldPlantId = new BaseId(reader);
            oldDeptId = new BaseId(reader);
            resourceId = new BaseId(reader);
            newPlantId = new BaseId(reader);
            newDeptId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        oldPlantId.Serialize(writer);
        oldDeptId.Serialize(writer);
        resourceId.Serialize(writer);
        newPlantId.Serialize(writer);
        newDeptId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId oldPlantId;
    public BaseId oldDeptId;
    public BaseId resourceId;
    public BaseId newPlantId;
    public BaseId newDeptId;

    public PlantSetResourceDeptT() { }

    public PlantSetResourceDeptT(BaseId scenarioId,
                                 BaseId oldPlantId,
                                 BaseId oldDeptId,
                                 BaseId resourceId,
                                 BaseId newPlantId,
                                 BaseId newDeptId)
        : base(scenarioId)
    {
        this.oldPlantId = oldPlantId;
        this.oldDeptId = oldDeptId;
        this.resourceId = resourceId;
        this.newPlantId = newPlantId;
        this.newDeptId = newDeptId;
    }

    public override string Description => "Resource Department assigned";
}