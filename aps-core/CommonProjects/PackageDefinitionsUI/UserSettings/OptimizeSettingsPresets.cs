using PT.APSCommon.Extensions;
using PT.PackageDefinitions;
using PT.Scheduler;

namespace PT.PackageDefinitionsUI.UserSettings;

public class OptimizeSettingsPreset
{
    public string PresetKey { get; set; }
    public OptimizeSettings OptimizeSettings { get; set; }

    public OptimizeSettingsPreset(string a_presetKey, OptimizeSettings a_optimizeSettings)
    {
        PresetKey = a_presetKey;
        OptimizeSettings = a_optimizeSettings;
    }
}

public class OptimizeSettingsPresets : ISettingData, ICloneable
{
    private readonly Dictionary<string, OptimizeSettingsPreset> m_optimizeSettingsPresets = new ();
    private readonly Dictionary<string, OptimizeSettingsPreset> m_compressSettingsPresets = new ();
    public static readonly string DefaultPresetKey = "Default".Localize();

    private string m_activeOptimizePreset;
    private string m_activeCompressPreset;

    public OptimizeSettingsPresets()
    {
        m_activeCompressPreset = DefaultPresetKey;
        m_activeOptimizePreset = DefaultPresetKey;
    }

    public OptimizeSettingsPresets(IReader a_reader)
    {
        a_reader.Read(out m_activeOptimizePreset);
        a_reader.Read(out int optimizePresetCount);
        for (int i = 0; i < optimizePresetCount; i++)
        {
            a_reader.Read(out string presetKey);
            OptimizeSettings settings = new (a_reader);
            m_optimizeSettingsPresets.Add(presetKey, new OptimizeSettingsPreset(presetKey, settings));
        }

        a_reader.Read(out m_activeCompressPreset);
        a_reader.Read(out int compressPresetCount);
        for (int i = 0; i < compressPresetCount; i++)
        {
            a_reader.Read(out string presetKey);
            OptimizeSettings settings = new (a_reader);
            m_compressSettingsPresets.Add(presetKey, new OptimizeSettingsPreset(presetKey, settings));
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_activeOptimizePreset);
        a_writer.Write(m_optimizeSettingsPresets.Count);
        foreach ((string presetKey, OptimizeSettingsPreset preset) in m_optimizeSettingsPresets)
        {
            a_writer.Write(presetKey);
            preset.OptimizeSettings.Serialize(a_writer);
        }

        a_writer.Write(m_activeCompressPreset);
        a_writer.Write(m_compressSettingsPresets.Count);
        foreach ((string presetKey, OptimizeSettingsPreset preset) in m_compressSettingsPresets)
        {
            a_writer.Write(presetKey);
            preset.OptimizeSettings.Serialize(a_writer);
        }
    }

    public OptimizeSettings GetOptimizeSettingsPreset(string a_presetKey)
    {
        if (m_optimizeSettingsPresets.TryGetValue(a_presetKey, out OptimizeSettingsPreset preset))
        {
            return preset.OptimizeSettings;
        }

        //This shouldn't happen
        return new OptimizeSettings();
    }

    public OptimizeSettingsPreset GetActivePresetOptimizeSettings()
    {
        if (m_optimizeSettingsPresets.TryGetValue(m_activeOptimizePreset, out OptimizeSettingsPreset preset))
        {
            return preset;
        }

        return GetDefaultOptimizePreset();
    }

    private OptimizeSettingsPreset GetDefaultOptimizePreset()
    {
        if (m_compressSettingsPresets.TryGetValue(DefaultPresetKey, out OptimizeSettingsPreset defaultPreset))
        {
            return defaultPreset;
        }

        OptimizeSettingsPreset newDefaultPreset = new (DefaultPresetKey, new OptimizeSettings());
        m_compressSettingsPresets.Add(DefaultPresetKey, newDefaultPreset);

        return newDefaultPreset;
    }

    public void AddOptimizePreset(string a_presetKey, OptimizeSettings a_optimizeSettings)
    {
        m_optimizeSettingsPresets.Add(a_presetKey, new OptimizeSettingsPreset(a_presetKey, a_optimizeSettings));
    }

    public void SaveOptimizePreset(string a_presetKey, OptimizeSettings a_optimizeSettings)
    {
        if (m_optimizeSettingsPresets.TryGetValue(a_presetKey, out OptimizeSettingsPreset optimizeSettingsPreset))
        {
            optimizeSettingsPreset.OptimizeSettings.Update(a_optimizeSettings);
        }
        else
        {
            AddOptimizePreset(a_presetKey, a_optimizeSettings);
        }
    }

    public bool RemoveOptimizePreset(string a_presetKey)
    {
        return m_optimizeSettingsPresets.Remove(a_presetKey);
    }

    public List<string> GetOptimizeSettingsPresetKeys()
    {
        return m_optimizeSettingsPresets.Keys.ToList();
    }

    public void SwitchActiveOptimizeSettingsPreset(string a_presetKey)
    {
        m_activeOptimizePreset = a_presetKey;
    }

    public OptimizeSettings GetCompressSettingsPreset(string a_presetKey)
    {
        if (m_compressSettingsPresets.TryGetValue(a_presetKey, out OptimizeSettingsPreset preset))
        {
            return preset.OptimizeSettings;
        }

        //This shouldn't happen
        return new OptimizeSettings();
    }

    private OptimizeSettingsPreset GetDefaultCompressPreset()
    {
        if (m_compressSettingsPresets.TryGetValue(DefaultPresetKey, out OptimizeSettingsPreset defaultPreset))
        {
            return defaultPreset;
        }

        OptimizeSettingsPreset newDefaultPreset = new (DefaultPresetKey, new OptimizeSettings());
        m_compressSettingsPresets.Add(DefaultPresetKey, newDefaultPreset);

        return newDefaultPreset;
    }

    public OptimizeSettingsPreset GetActivePresetCompressSettings()
    {
        if (m_compressSettingsPresets.TryGetValue(m_activeCompressPreset, out OptimizeSettingsPreset preset))
        {
            return preset;
        }

        return GetDefaultCompressPreset();
    }

    public void AddCompressPreset(string a_presetKey, OptimizeSettings a_compressSettings)
    {
        m_compressSettingsPresets.Add(a_presetKey, new OptimizeSettingsPreset(a_presetKey, a_compressSettings));
    }

    public void SaveCompressPreset(string a_presetKey, OptimizeSettings a_compressSettings)
    {
        if (m_compressSettingsPresets.TryGetValue(a_presetKey, out OptimizeSettingsPreset compressSettingsPreset))
        {
            compressSettingsPreset.OptimizeSettings.Update(a_compressSettings);
        }
        else
        {
            AddCompressPreset(a_presetKey, a_compressSettings);
        }
    }

    public bool RemoveCompressPreset(string a_presetKey)
    {
        return m_compressSettingsPresets.Remove(a_presetKey);
    }

    public List<string> GetCompressSettingsPresetKeys()
    {
        return m_compressSettingsPresets.Keys.ToList();
    }

    public void SwitchActiveCompressSettingsPreset(string a_presetKey)
    {
        m_activeCompressPreset = a_presetKey;
    }

    #region ISettingsData and ICloneable interface members
    public int UniqueId => 1040;
    public string SettingKey => "settingData_OptimizeSettingsPresets";
    public string SettingCaption => SettingGroupConstants.PersonalSettingsGroup;
    public string Description => "Personalized Optimize Settings Presets".Localize();
    public string SettingsGroup => SettingGroupConstants.OptimizeAndCompressSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.OptimizeAndCompressSettingsGroup;

    object ICloneable.Clone()
    {
        return Clone();
    }

    public OptimizeSettingsPresets Clone()
    {
        return (OptimizeSettingsPresets)MemberwiseClone();
    }
    #endregion
}