using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Change a Scenario's OptimizeSettings.
/// </summary>
public class ScenarioDetailOptimizeSettingsChangeT : ScenarioIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public ScenarioDetailOptimizeSettingsChangeT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12031)
        {
            Settings = new OptimizeSettings(a_reader);
        }
        else if (a_reader.VersionNumber >= 12000)
        {
            Settings = new OptimizeSettings(a_reader);
            BoolVector32 optimizeSetBools = new (a_reader);
            BoolVector32 mrpSetBools = new (a_reader);
            Settings.SetIsSetBools(optimizeSetBools, mrpSetBools);
        }
        else
        {
            Settings = new OptimizeSettings(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        Settings.Serialize(a_writer);
    }

    public const int UNIQUE_ID = 515;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public OptimizeSettings Settings;

    public ScenarioDetailOptimizeSettingsChangeT() { }

    public ScenarioDetailOptimizeSettingsChangeT(BaseId a_scenarioId, OptimizeSettings a_settings)
        : base(a_scenarioId)
    {
        Settings = a_settings;
    }

    public override string Description => "Optimize Settings Changed (Scenario)".Localize();
}