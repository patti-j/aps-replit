using PT.APSCommon;
using PT.PackageDefinitions.Settings;

namespace PT.Transmissions;

/// <summary>
/// Sets the Hold Settings for the Scenario.
/// </summary>
public class ScenarioDetailHoldSettingsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 452;

    #region IPTSerializable Members
    public ScenarioDetailHoldSettingsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_holdSettings = new HoldSettings(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_holdSettings.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailHoldSettingsT() { }

    public ScenarioDetailHoldSettingsT(BaseId a_scenarioId, HoldSettings a_holdSettings)
        : base(a_scenarioId)
    {
        m_holdSettings = a_holdSettings;
    }

    private readonly HoldSettings m_holdSettings;

    public HoldSettings HoldSettings => m_holdSettings;

    public override string Description => "Hold Settings saved";
}