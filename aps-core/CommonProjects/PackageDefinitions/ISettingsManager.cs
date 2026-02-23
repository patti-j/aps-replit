using PT.SchedulerDefinitions;

namespace PT.PackageDefinitions;

public interface ISettingsManager
{
    /// <summary>
    /// A setting with the provided key has been saved
    /// </summary>
    event Action<ISettingsManager, string> SettingSavedEvent;

    /// <summary>
    /// Save setting data
    /// </summary>
    void SaveSetting(SettingData a_setting, bool a_fireSettingSaved);

    /// <summary>
    /// Save setting data from a serializable setting class
    /// </summary>
    void SaveSetting(ISettingData a_setting);
    /// <summary>
    /// Save setting data from a serializable setting class
    /// </summary>
    void SaveSetting(SettingData a_setting, IScenarioDataChanges a_dataChanges, bool a_fireSettingSaved){}

    /// <summary>
    /// Load setting data by key
    /// </summary>
    SettingData LoadSetting(string a_moduleId);

    /// <summary>
    /// Load a settings object by type and key
    /// </summary>
    T LoadSetting<T>(string a_key) where T : ISettingData;

    /// <summary>
    /// Load a setting data by class
    /// </summary>
    T LoadSetting<T>(T a_setting) where T : ISettingData;

    void DeleteSetting(string a_key);
    void DeleteSetting(ISettingData a_setting);
}