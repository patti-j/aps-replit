using PT.PackageDefinitions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitionsUI;
using PT.Scheduler;

namespace PT.UI.UserSettings;

internal class UserPreferenceInfo : GenericSettingSaver, IUserPreferenceInfo, IPTSerializable
{
    #region IPTSerializable
    internal UserPreferenceInfo(IPackageManagerUI a_pm, GenericSettingSaver a_settingSaver) : base(a_settingSaver)
    {
        List<IUserPreferenceElement> elements = new ();
        List<IUserPreferencesModule> modules = a_pm.GetPreferenceModules();
        foreach (IUserPreferencesModule module in modules)
        {
            elements.AddRange(module.GetPreferencesElements());
        }

        List<string> elementKeys = new ();
        foreach (IUserPreferenceElement element in elements)
        {
            if (!m_settingsDict.ContainsKey(element.Key))
            {
                m_settingsDict.Add(element.Key, new SettingData(element.DefaultValue));
            }

            elementKeys.Add(element.Key);
        }

        //Remove dictionary entries that are not in the loaded elements
        //TODO: Re-evaluate this. Possible check this from a UI control.
        //TODO: Some preferences are saved by MainForm and cannot be loaded as a pacakge element.
        //List<string> elementKeysToRemoveList = new List<string>();
        //foreach (string existingSettingKey in m_settingsDict.Keys)
        //{
        //    if (!elementKeys.Contains(existingSettingKey))
        //    {
        //        elementKeysToRemoveList.Add(existingSettingKey);
        //    }
        //}

        //foreach (string key in elementKeysToRemoveList)
        //{
        //    m_settingsDict.Remove(key);
        //}
    }

    internal byte[] SerializeSettings()
    {
        byte[] data;
        using (BinaryMemoryWriter writer = new ())
        {
            Serialize(writer);
            data = writer.GetBuffer();
        }

        return data;
    }

    public new int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 824;
    #endregion
}