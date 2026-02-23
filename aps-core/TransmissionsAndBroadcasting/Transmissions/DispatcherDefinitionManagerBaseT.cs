using PT.APSCommon;

namespace PT.Transmissions;

public abstract class DispatcherDefinitionManagerBaseT : ScenarioIdBaseT, IPTSerializable
{
    public override string Description => "Optimize rule updated";

    #region IPTSerializable Members
    public DispatcherDefinitionManagerBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }
    #endregion

    protected DispatcherDefinitionManagerBaseT() { }

    protected DispatcherDefinitionManagerBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}