using PT.PackageDefinitions;

namespace PT.UIDefinitions.ControlSettings;

public class MappingWizardSettings : IPTSerializable, ISettingData
{
    #region IPTSerializable Members
    private BoolVector32 m_bools;

    public MappingWizardSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 636)
        {
            a_reader.Read(out m_savePreference);
        }
    }

    public MappingWizardSettings()
    {
        m_savePreference = (short)EAutoSaveOption.AlwaysSave;
    }

    private short m_savePreference;

    public EAutoSaveOption SavePreference
    {
        get => (EAutoSaveOption)m_savePreference;
        set => m_savePreference = (short)value;
    }

    public enum EAutoSaveOption { AlwaysSave, Prompt, NeverSave }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_savePreference);
    }

    public int UniqueId => 1025;
    #endregion

    public string SettingKey => "MappingWizardSettings";
    public string Description => "Integration Settings";
    public string SettingsGroup => SettingGroupConstants.SystemSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.MappingWizardSettings;
    public string SettingCaption => "Import mappings";
}