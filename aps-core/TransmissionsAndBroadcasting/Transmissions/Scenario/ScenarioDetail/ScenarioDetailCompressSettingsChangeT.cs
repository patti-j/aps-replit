using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Change a Scenario's Compress OptimizeSettings.
/// </summary>
public class ScenarioDetailCompressSettingsChangeT : ScenarioIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public ScenarioDetailCompressSettingsChangeT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12031)
        {
            Settings = new OptimizeSettings(a_reader);
            if (a_reader.VersionNumber < 12204)
            {
                //StartTime for compress used to be the end time
                Settings.SetBackwardsCompatibilityForStartEndTimes();
            }
        }
        else if (a_reader.VersionNumber >= 12000)
        {
            Settings = new OptimizeSettings(a_reader);
            BoolVector32 optimizeSetBools = new (a_reader);
            BoolVector32 mrpSetBools = new (a_reader);
            Settings.SetIsSetBools(optimizeSetBools, mrpSetBools);
            //StartTime for compress used to be the end time
            Settings.SetBackwardsCompatibilityForStartEndTimes();
        }
        else
        {
            Settings = new OptimizeSettings(a_reader);
            //StartTime for compress used to be the end time
            Settings.SetBackwardsCompatibilityForStartEndTimes();
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        Settings.Serialize(a_writer);
    }

    public const int UNIQUE_ID = 590;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public OptimizeSettings Settings;

    public ScenarioDetailCompressSettingsChangeT() { }

    public ScenarioDetailCompressSettingsChangeT(BaseId a_scenarioId, OptimizeSettings a_settings)
        : base(a_scenarioId)
    {
        Settings = a_settings;
    }

    public override string Description => "Compress settings updated";
}