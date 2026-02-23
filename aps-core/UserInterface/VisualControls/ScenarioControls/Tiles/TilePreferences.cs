using PT.PackageDefinitions;

namespace PT.ScenarioControls.Tiles;

public class TilePreferences : ISettingData
{
    private string m_settingKey = GlobalSettingKey;
    private BoolVector32 m_bools;

    public TilePreferences(IReader a_reader)
    {
        a_reader.Read(out m_settingKey);
        m_bools = new BoolVector32(a_reader);
    }

    public TilePreferences()
    {
        //Defaults
    }

    public TilePreferences(string a_settingKey) : this()
    {
        m_settingKey = a_settingKey;
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_settingKey);

        m_bools.Serialize(a_writer);
    }

    public int UniqueId => 1030;

    public string SettingKey
    {
        get => m_settingKey;
        set => m_settingKey = value;
    }

    private const short c_floatPaneIdx = 0;
    private const short c_inlineMenuDisplayModeIdx = 1;

    public bool OpenTileInWindow
    {
        get => m_bools[c_floatPaneIdx];
        set => m_bools[c_floatPaneIdx] = value;
    }

    public bool InlineMenuDisplayMode
    {
        get => m_bools[c_inlineMenuDisplayModeIdx];
        set => m_bools[c_inlineMenuDisplayModeIdx] = value;
    }

    public static readonly string GlobalSettingKey = "userPreferences_TileSettings";

    public string Description => "Tile Preference Settings";
    public string SettingsGroup => SettingGroupConstants.LayoutSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.WorkspaceViewSettings;
    public string SettingCaption => "Tile view preferences";
}