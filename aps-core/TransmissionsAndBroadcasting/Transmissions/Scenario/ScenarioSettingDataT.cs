using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.PackageDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Sets the ScenarioOptions for all Scenarios in the System.
/// </summary>
public class ScenarioSettingDataT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 921;

    #region IPTSerializable Members
    public ScenarioSettingDataT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            SettingData = new SettingData(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        SettingData.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public readonly SettingData SettingData;

    public ScenarioSettingDataT() { }

    public ScenarioSettingDataT(SettingData a_settingData, BaseId a_scenarioId) : base(a_scenarioId)
    {
        SettingData = a_settingData;
    }

    public override string Description => SettingData.Description.Localize();
}