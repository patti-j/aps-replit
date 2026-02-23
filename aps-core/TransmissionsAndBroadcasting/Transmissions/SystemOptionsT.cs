using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Sets the ScenarioOptions for all Scenarios in the System.
/// </summary>
public class SystemOptionsT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 150;

    #region IPTSerializable Members
    public SystemOptionsT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            ScenarioOptions = new ScenarioOptions(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        ScenarioOptions.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioOptions ScenarioOptions;

    public SystemOptionsT() { }

    public SystemOptionsT(ScenarioOptions a_scenarioOptions)
    {
        ScenarioOptions = a_scenarioOptions;
    }

    public override string Description => "System Settings Saved".Localize();
}