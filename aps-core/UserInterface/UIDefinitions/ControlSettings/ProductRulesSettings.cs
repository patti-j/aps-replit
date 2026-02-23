using PT.PackageDefinitions;

namespace PT.UIDefinitions.ControlSettings;

public class ProductRulesSettings : ISettingData
{
    #region IPTSerializable Members
    public ProductRulesSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 618)
        {
            m_bools = new BoolVector32(a_reader);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
    }

    public int UniqueId => 0; //TODO:
    #endregion

    private BoolVector32 m_bools;

    private const short c_generateCapabilitiesIdx = 0;

    public ProductRulesSettings() { }

    public bool GenerateCapabilities
    {
        get => m_bools[c_generateCapabilitiesIdx];
        set => m_bools[c_generateCapabilitiesIdx] = value;
    }

    public string SettingKey => "ProductRulesSettings";
    public string SettingCaption => "Product Rules Settings";
    public string Description => "Settings for the product rules management tile";
    public string SettingsGroup => "Product Rules";
    public string SettingsGroupCategory => "Product Rules";
}