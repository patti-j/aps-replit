using Newtonsoft.Json;
using PT.APSCommon;

namespace PT.PackageDefinitions;

public class SettingData : IPTSerializable
{
    public string Key;
    public byte[] Data;
    public string Description;
    public string SettingsGroup;
    public string SettingsGroupCategory;
    public string SettingCaption;
    public DateTime LastSaveTime = DateTime.UtcNow;
    public BaseId UserId = BaseId.NULL_ID;

    private const int UNIQUE_ID = 903;

    public SettingData(IReader a_reader)
    {
        //Use the new collection storage method
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out Key);
            a_reader.Read(out Data);
            a_reader.Read(out LastSaveTime);
            a_reader.Read(out Description);
            a_reader.Read(out SettingsGroup);
            a_reader.Read(out SettingsGroupCategory);
            a_reader.Read(out SettingCaption);
            UserId = new BaseId(a_reader);
        }
    }

    /// <summary>
    /// Create a SettingData without using the ISettingData interface. This may be required if the interface is not accessible
    /// </summary>
    public SettingData(string a_moduleId, byte[] a_data, string a_description, string a_settingsGroup, string a_settingsCategory, string a_settingCaption)
    {
        Key = a_moduleId;
        Data = a_data;
        Description = a_description;
        SettingsGroup = a_settingsGroup;
        SettingsGroupCategory = a_settingsCategory;
        SettingCaption = a_settingCaption;
    }

    public SettingData(ISettingData a_setting)
    {
        Key = a_setting.SettingKey;
        Description = a_setting.Description;
        SettingsGroup = a_setting.SettingsGroup;
        SettingsGroupCategory = a_setting.SettingsGroupCategory;
        SettingCaption = a_setting.SettingCaption;

        using (BinaryMemoryWriter binaryMemoryWriter = new ())
        {
            a_setting.Serialize(binaryMemoryWriter);
            Data = binaryMemoryWriter.GetBuffer();
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Key);
        a_writer.Write(Data);
        a_writer.Write(LastSaveTime);
        a_writer.Write(Description);
        a_writer.Write(SettingsGroup);
        a_writer.Write(SettingsGroupCategory);
        a_writer.Write(SettingCaption);
        UserId.Serialize(a_writer);
    }

    public int UniqueId => UNIQUE_ID;
    public string ToJson() => JsonConvert.SerializeObject(this);
}