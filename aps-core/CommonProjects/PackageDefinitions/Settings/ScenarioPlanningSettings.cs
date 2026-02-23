using System.Drawing;

namespace PT.PackageDefinitions.Settings;

public class ScenarioPlanningSettings : ISettingData, ICloneable
{
    public ScenarioPlanningSettings(IReader a_reader)
    {
        m_bools = new BoolVector32(a_reader);
        a_reader.Read(out ScenarioColor);
        if (ScenarioColor.ToArgb() == Color.Empty.ToArgb())
        {
            ScenarioColor = Color.FromArgb(13, 149, 211);
        }
    }

    public ScenarioPlanningSettings()
    {
        ScenarioColor = Color.FromArgb(13, 149, 211);
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(ScenarioColor);
    }

    private BoolVector32 m_bools;

    private const short c_productionIdx = 0;
    private const short c_isolateFromImport = 1;
    private const short c_isolateFromClockAdvance = 2;
    private const short c_compareScenario = 3;

    /// <summary>
    /// The Scenario Color
    /// </summary>
    public Color ScenarioColor;

    /// <summary>
    /// Whether this is a Production Scenario
    /// </summary>
    public bool Production
    {
        get => m_bools[c_productionIdx];
        set => m_bools[c_productionIdx] = value;
    }

    /// <summary>
    /// Isolates this Scenario from Data Import when an Import has been triggered on all Scenarios.
    /// </summary>
    [Obsolete("No longer used as part of IntegrationV2, check if scenario has no config mapping set.")]
    public bool IsolateFromImport
    {
        get => m_bools[c_isolateFromImport];
        set => m_bools[c_isolateFromImport] = value;
    }

    /// <summary>
    /// Isolates this Scenario from Clock Advance when a Clock Advance has been triggered on all Scenarios
    /// </summary>
    public bool IsolateFromClockAdvance
    {
        get => m_bools[c_isolateFromClockAdvance];
        set => m_bools[c_isolateFromClockAdvance] = value;
    }

    /// <summary>
    /// Whether this Scenario need to be compared
    /// </summary>
    public bool CompareScenario
    {
        get => m_bools[c_compareScenario];
        set => m_bools[c_compareScenario] = value;
    }

    public override bool Equals(object a_settings)
    {
        if (a_settings is ScenarioPlanningSettings sps)
        {
            return Production == sps.Production && ScenarioColor == sps.ScenarioColor && IsolateFromClockAdvance == sps.IsolateFromClockAdvance && IsolateFromImport == sps.IsolateFromImport && CompareScenario == sps.CompareScenario;
        }

        return false;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    public ScenarioPlanningSettings Clone()
    {
        return (ScenarioPlanningSettings)MemberwiseClone();
    }

    public int UniqueId => 1020;

    public string SettingKey => Key;
    public static string Key => "scenarioSetting_ScenarioPlanningSettings";
    public string Description => "Scenario Planning Settings";
    public string SettingsGroup => SettingGroupConstants.ScenariosGroup;
    public string SettingsGroupCategory => SettingGroupConstants.ScenarioPlanningSettings;
    public string SettingCaption => "Plannings settings";
}