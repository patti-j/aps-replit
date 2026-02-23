using PT.APSCommon.Serialization;
using PT.PackageDefinitions;

namespace PT.UI.UserSettings;

internal class MainFormLayout : ISettingData
{
    private readonly FormLayout m_formLayout;

    public MainFormLayout(IReader a_reader)
    {
        m_formLayout = new FormLayout(a_reader);
    }

    public void Serialize(IWriter a_writer)
    {
        m_formLayout.Serialize(a_writer);
    }

    public MainFormLayout(FormLayout a_formLayout)
    {
        m_formLayout = a_formLayout;
    }

    public MainFormLayout()
    {
        m_formLayout = new FormLayout("MainForm");
    }

    internal static string Key => "userPreference_MainFormLayout";

    public int UniqueId => 1028;
    public string SettingKey => Key;
    public string Description => "Saved client program position";
    public string SettingsGroup => SettingGroupConstants.LayoutSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.FormLayouts;
    public string SettingCaption => "Main form layout";

    public FormLayout Layout => m_formLayout;
}