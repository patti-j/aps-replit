using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for changing an existing Resource.
/// </summary>
public class ScenarioDetailSetCapabilitiesT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 135;

    #region IPTSerializable Members
    public ScenarioDetailSetCapabilitiesT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            plantId = new BaseId(reader);
            departmentId = new BaseId(reader);
            machineId = new BaseId(reader);
            capabilityIds = new CapabilityKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        plantId.Serialize(writer);
        departmentId.Serialize(writer);
        machineId.Serialize(writer);
        capabilityIds.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId plantId;
    public BaseId departmentId;
    public BaseId machineId;

    public ScenarioDetailSetCapabilitiesT() { }

    public ScenarioDetailSetCapabilitiesT(BaseId scenarioId, BaseId plantId, BaseId departmentId, BaseId machineId, CapabilityKeyList capabilityIds)
        : base(scenarioId)
    {
        this.plantId = plantId;
        this.departmentId = departmentId;
        this.machineId = machineId;
        this.capabilityIds = capabilityIds;
    }

    private readonly CapabilityKeyList capabilityIds;

    public CapabilityKeyList CapabilityIds => capabilityIds;

    public override string Description => "Capabilities assigned to Resource";
}