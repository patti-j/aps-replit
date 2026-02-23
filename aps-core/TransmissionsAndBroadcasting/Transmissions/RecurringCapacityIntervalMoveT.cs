using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Transmission for moving an existing RecurringCapacityInterval to a new time and/or resource.
/// </summary>
public class RecurringCapacityIntervalMoveT : RecurringCapacityIntervalIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 108;

    #region IPTSerializable Members
    public RecurringCapacityIntervalMoveT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int val;
            reader.Read(out val);
            newResourceType = (resourceTypes)val;
            reader.Read(out val);
            oldResourceType = (resourceTypes)val;

            reader.Read(out newStartTime);

            newPlantId = new BaseId(reader);
            newDepartmentId = new BaseId(reader);
            newResourceId = new BaseId(reader);
            oldPlantId = new BaseId(reader);
            oldDepartmentId = new BaseId(reader);
            oldResourceId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write((int)newResourceType);
        writer.Write((int)oldResourceType);

        writer.Write(newStartTime);

        newPlantId.Serialize(writer);
        newDepartmentId.Serialize(writer);
        newResourceId.Serialize(writer);
        oldPlantId.Serialize(writer);
        oldDepartmentId.Serialize(writer);
        oldResourceId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId newPlantId;
    public BaseId newDepartmentId;
    public BaseId newResourceId;
    public resourceTypes newResourceType;
    public BaseId oldPlantId;
    public BaseId oldDepartmentId;
    public BaseId oldResourceId;
    public resourceTypes oldResourceType;
    public DateTime newStartTime;

    public RecurringCapacityIntervalMoveT() { }

    public RecurringCapacityIntervalMoveT(BaseId scenarioId,
                                          BaseId capacityIntervalId,
                                          BaseId oldPlantId,
                                          BaseId oldDepartmentId,
                                          BaseId oldResourceId,
                                          resourceTypes oldResourceType,
                                          BaseId newPlantId,
                                          BaseId newDepartmentId,
                                          BaseId newResourceId,
                                          resourceTypes newResourceType,
                                          DateTime newStartTime)
        : base(scenarioId, new List<BaseId> { capacityIntervalId })
    {
        this.newPlantId = newPlantId;
        this.newDepartmentId = newDepartmentId;
        this.newResourceId = newResourceId;
        this.newResourceType = newResourceType;
        this.oldPlantId = oldPlantId;
        this.oldDepartmentId = oldDepartmentId;
        this.oldResourceId = oldResourceId;
        this.oldResourceType = oldResourceType;
        this.newStartTime = newStartTime;
    }

    public override string Description => "Capacity Interval moved";
}