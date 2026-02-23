using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Sets the ScenarioPublishOptions for all Scenarios in the System.
/// </summary>
public class SystemPublishOptionsT : ScenarioBaseT
{
    public const int UNIQUE_ID = 614;

    #region IPTSerializable Members
    public SystemPublishOptionsT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_publishOptions = new ScenarioPublishOptions(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_publishOptions.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly ScenarioPublishOptions m_publishOptions;

    public SystemPublishOptionsT() { }

    public SystemPublishOptionsT(ScenarioPublishOptions a_scenarioOptions)
    {
        m_publishOptions = a_scenarioOptions;
    }

    public ScenarioPublishOptions PublishOptions => m_publishOptions;

    public override string Description => "Publish Options Saved".Localize();
}