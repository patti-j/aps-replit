using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for changing an existing Resource.
/// </summary>
public class ScenarioDetailSetCapabilityResourcesT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 134;

    #region IPTSerializable Members
    public ScenarioDetailSetCapabilityResourcesT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            capabilityId = new BaseId(reader);
            machines = new ResourceKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        capabilityId.Serialize(writer);
        machines.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId capabilityId;
    public ResourceKeyList machines;

    public ScenarioDetailSetCapabilityResourcesT() { }

    public ScenarioDetailSetCapabilityResourcesT(BaseId scenarioId, BaseId capabilityId, ResourceKeyList machines)
        : base(scenarioId)
    {
        this.capabilityId = capabilityId;
        this.machines = machines;
    }

    public override string Description => "Resources assigned to Capabilities";
}