using PT.APSCommon.Extensions;
using PT.PackageDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Sets the ScenarioOptions for all Scenarios in the System.
/// </summary>
public class SystemSettingDataT : PTTransmission, IPTSerializable
{
    public const int UNIQUE_ID = 922;

    #region IPTSerializable Members
    public SystemSettingDataT(IReader a_reader)
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

    public SystemSettingDataT() { }

    public SystemSettingDataT(SettingData a_settingData)
    {
        SettingData = a_settingData;
    }

    public SystemSettingDataT(ISettingData a_settingData)
    {
        SettingData = new SettingData(a_settingData);
    }

    public override string Description => SettingData.Description.Localize();
}