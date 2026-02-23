using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new CapacityInterval by copying the specified CapacityInterval to the specified Resource.
/// </summary>
public class CapacityIntervalCopyToResourceT : CapacityIntervalIdBaseT, IPTSerializable
{
    public override string Description => "Capacity Interval copied to Resource";

    public new const int UNIQUE_ID = 591;

    #region IPTSerializable Members
    public CapacityIntervalCopyToResourceT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            newPlantId = new BaseId(reader);
            newDepartmentId = new BaseId(reader);
            newResourceId = new BaseId(reader);

            originalPlantId = new BaseId(reader);
            originalDepartmentId = new BaseId(reader);
            originalResourceId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        newPlantId.Serialize(writer);
        newDepartmentId.Serialize(writer);
        newResourceId.Serialize(writer);

        originalPlantId.Serialize(writer);
        originalDepartmentId.Serialize(writer);
        originalResourceId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId newPlantId;
    public BaseId newDepartmentId;
    public BaseId newResourceId;

    public BaseId originalPlantId;
    public BaseId originalDepartmentId;
    public BaseId originalResourceId;

    public CapacityIntervalCopyToResourceT() { }

    public CapacityIntervalCopyToResourceT(BaseId scenarioId,
                                           BaseId originalId,
                                           BaseId newPlantId,
                                           BaseId newDepartmentId,
                                           BaseId newResourceId,
                                           BaseId originalPlantId,
                                           BaseId originalDepartmentId,
                                           BaseId originalResourceId)
        : base(scenarioId, new List<BaseId> { originalId })
    {
        this.newPlantId = newPlantId;
        this.newDepartmentId = newDepartmentId;
        this.newResourceId = newResourceId;

        this.originalPlantId = originalPlantId;
        this.originalDepartmentId = originalDepartmentId;
        this.originalResourceId = originalResourceId;
    }
}