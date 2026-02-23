using System.Collections.Concurrent;

namespace PT.PackageDefinitions;

public class WorkspaceSettingsCollector
{
    private readonly ConcurrentBag<SettingData> m_settingsList = new ();

    public void SaveSetting(SettingData a_data)
    {
        m_settingsList.Add(a_data);
    }

    public IEnumerable<SettingData> SavedSettings()
    {
        foreach (SettingData settingData in m_settingsList)
        {
            yield return settingData;
        }
    }
}