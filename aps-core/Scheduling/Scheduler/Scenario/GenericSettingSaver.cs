using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.PackageDefinitions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Stores SettingData objects in a dictionary
/// </summary>
public class GenericSettingSaver : ISettingsManager, IPTSerializable
{
    protected Dictionary<string, SettingData> m_settingsDict = new ();
    /// <summary>
    /// A dictionary to hold a collection of settings as ISettingData so we are able to
    /// know the type of implementing setting when auditing
    /// </summary>
    private readonly Dictionary<string, ISettingData> m_settingsContainer = new ();
    
    public event Action<ISettingsManager, string> SettingSavedEvent;

    public GenericSettingSaver()
    {
        m_settingsDict = new Dictionary<string, SettingData>();
    }

    public GenericSettingSaver(GenericSettingSaver a_settingSaver)
    {
        m_settingsDict = a_settingSaver.m_settingsDict.Clone();
    }

    public GenericSettingSaver(IReader a_reader)
    {
        //Use the new collection storage method
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int settingsCount);
            for (int i = 0; i < settingsCount; i++)
            {
                SettingData setting = new (a_reader);
                if (!m_settingsDict.TryAdd(setting.Key, setting))
                {
                    DebugException.ThrowInDebug($"{setting.Key} settings was added twice in the workspace dictionary. Skipped the second entry in deserialization.");
                }
            }
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_settingsDict.Count);

        foreach (SettingData settingData in m_settingsDict.Values)
        {
            settingData.Serialize(a_writer);
        }
    }

    public virtual int UniqueId => 920;

    public void SaveSetting(SettingData a_setting, bool a_fireSettingSaved)
    {
        if (string.IsNullOrWhiteSpace(a_setting.Key))
        {
            DebugException.ThrowInDebug("SettingData being saved without a key defined");
            //Don't save the setting, it will cause errors that will be harder to identify than if the settings don't get saved.
            return;
        }

        //TODO: Update how this logs IDs, since the key cannot be used as a BaseId.
        //PropertyChanges propertyChanges = m_scenarioDataChanges.SystemSettingChanges.PropertyChangeHistory;

        a_setting.LastSaveTime = DateTime.UtcNow;
        a_setting.UserId = SystemController.CurrentUserId;
        //propertyChanges.AddPropertyChange(new BaseId(a_setting.Key), a_setting.SettingCaption, a_setting.ToJson());

        m_settingsDict[a_setting.Key] = a_setting;
        
        if (a_fireSettingSaved)
        {
            SettingSavedEvent?.Invoke(this, a_setting.Key);
        }
    }

    public void SaveSetting(ISettingData a_setting)
    {
        SaveSetting(new SettingData(a_setting), true);

        if (!m_settingsContainer.TryAdd(a_setting.SettingKey, a_setting))
        {
            m_settingsContainer[a_setting.SettingKey] = a_setting;
        }
    }

    public void SaveSetting(SettingData a_setting, IScenarioDataChanges a_dataChanges, bool a_fireSettingSaved) 
    {
        if (m_settingsContainer.TryGetValue(a_setting.Key, out ISettingData setting))
        {
            ISettingData updatedSetting = SettingDataUtilities.ConstructSetting(setting, a_setting);

            AuditEntry audit = new AuditEntry(new BaseId(setting.UniqueId),new BaseId(setting.UniqueId) , setting, updatedSetting);
            
            SaveSetting(a_setting, a_fireSettingSaved);
            a_dataChanges.AuditEntry(audit);

            m_settingsContainer[a_setting.Key] = updatedSetting;
        }
    }

    public SettingData LoadSetting(string a_moduleId)
    {
        if (string.IsNullOrEmpty(a_moduleId))
        {
            throw new PTValidationException();
        }

        if (m_settingsDict.TryGetValue(a_moduleId, out SettingData setting))
        {
            return setting;
        }

        return null;
    }

    public T LoadSetting<T>(string a_key) where T : ISettingData
    {
        try
        {
            SettingData data = LoadSetting(a_key);
            T constructedSetting = SettingDataUtilities.ConstructSetting<T>(data);
            if (constructedSetting is ISettingRequiresInitialization setting)
            {
                setting.Initialize(a_key);
            }
            m_settingsContainer.TryAdd(a_key, constructedSetting);

            return constructedSetting;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public T LoadSetting<T>(T a_setting) where T : ISettingData
    {
        try
        {
            return LoadSetting<T>(a_setting.SettingKey);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void DeleteSetting(string a_key)
    {
        m_settingsDict.Remove(a_key);
        SettingSavedEvent?.Invoke(this, a_key);
    }

    public void DeleteSetting(ISettingData a_setting)
    {
        DeleteSetting(a_setting.SettingKey);
    }

    public void OverwriteWorkspaceDictionary(Dictionary<string, SettingData> a_settingsDictionary, bool a_resetOtherSettings)
    {
        foreach (KeyValuePair<string, SettingData> setting in a_settingsDictionary)
        {
            SaveSetting(setting.Value, true);
        }

        if (a_resetOtherSettings)
        {
            List<SettingData> settingData = new ();
            foreach (KeyValuePair<string, SettingData> currentSetting in m_settingsDict)
            {
                if (!a_settingsDictionary.ContainsKey(currentSetting.Key))
                {
                    settingData.Add(currentSetting.Value);
                }
            }

            foreach (SettingData data in settingData)
            {
                DeleteSetting(data.Key);
                SaveSetting(data, true);
            }
        }
    }

    public Dictionary<string, SettingData> SettingDictionary
    {
        get => m_settingsDict.Clone();
        set => m_settingsDict = value.Clone();
    }
}