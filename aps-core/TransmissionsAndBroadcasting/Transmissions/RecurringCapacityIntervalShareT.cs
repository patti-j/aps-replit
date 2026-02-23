using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Transmission for sharing the specified RecurringCapacityInterval with another resource.
/// </summary>
public class RecurringCapacityIntervalShareT : RecurringCapacityIntervalIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 109;

    #region IPTSerializable Members
    public RecurringCapacityIntervalShareT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int val;
            reader.Read(out val);
            newResourceType = (resourceTypes)val;
            reader.Read(out val);
            oldResourceType = (resourceTypes)val;

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

    public RecurringCapacityIntervalShareT() { }

    public RecurringCapacityIntervalShareT(BaseId scenarioId,
                                           BaseId recurringCapacityIntervalId,
                                           BaseId newPlantId,
                                           BaseId newDepartmentId,
                                           BaseId newResourceId,
                                           resourceTypes newResourceType,
                                           BaseId oldPlantId,
                                           BaseId oldDepartmentId,
                                           BaseId oldResourceId,
                                           resourceTypes oldResourceType
    )
        : base(scenarioId, new List<BaseId> { recurringCapacityIntervalId })
    {
        this.newPlantId = newPlantId;
        this.newDepartmentId = newDepartmentId;
        this.newResourceId = newResourceId;
        this.newResourceType = newResourceType;

        this.oldPlantId = oldPlantId;
        this.oldDepartmentId = oldDepartmentId;
        this.oldResourceId = oldResourceId;
        this.oldResourceType = oldResourceType;
    }

    public override string Description => "Recurring Capacity Interval Resources updated";
}