using PT.PackageDefinitions;

namespace PT.ScenarioControls.PackageHelpers;

public class BaseOptimizeRuleSetting
{
    public BaseOptimizeRuleSetting()
    {
        Weight = 0;
    }

    public BaseOptimizeRuleSetting(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out Weight);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(Weight);
    }

    public int UniqueId => 906;

    #region Property Accessors
    public decimal Weight;
    private BoolVector32 m_bools;
    private const short c_enabledIdx = 0;

    public bool Enabled
    {
        get => m_bools[c_enabledIdx];
        set => m_bools[c_enabledIdx] = value;
    }
    #endregion

    public string SettingsGroup => SettingGroupConstants.Optimize;
    public string SettingsGroupCategory => "Optimize Rules";
}