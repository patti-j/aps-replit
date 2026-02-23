using System.Drawing;

using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class BlockActualsSettings : ISettingData, ICloneable
{
    public const int UNIQUE_ID = 918;

    public string SettingKey => "BlockSettings_Actuals";
    public string Description => "TODO:";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.BlockSettings;
    public string SettingCaption => "Block Actual Settings";

    #region IPTSerializable Members
    public BlockActualsSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12023)
        {
            a_reader.Read(out m_actualStatusScript);
            a_reader.Read(out m_actualPerformanceScript);
            a_reader.Read(out m_finished);
            a_reader.Read(out m_partiallyFinished);
            a_reader.Read(out m_fast);
            a_reader.Read(out m_normal);
            a_reader.Read(out m_slow);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_minHeight);
            a_reader.Read(out m_proportionalHeightWeight);
            a_reader.Read(out m_show);
        }

        #region 11000
        else if (a_reader.VersionNumber >= 11000)
        {
            a_reader.Read(out m_actualStatusScript);
            a_reader.Read(out m_actualPerformanceScript);
            a_reader.Read(out m_finished);
            a_reader.Read(out m_partiallyFinished);
            a_reader.Read(out m_fast);
            a_reader.Read(out m_normal);
            a_reader.Read(out m_slow);
        }
        #endregion

        #region Pre-11000
        else
        {
            a_reader.Read(out m_actualStatusScript);
            a_reader.Read(out m_actualPerformanceScript);
        }
        #endregion
    }

    public BlockActualsSettings()
    {
        Initialize();
    }

    public BlockActualsSettings(BlockActualsSettings a_settings)
    {
        m_actualStatusScript = a_settings.ActualStatusScript;
        m_actualPerformanceScript = a_settings.ActualPerformanceScript;
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_actualStatusScript);
        a_writer.Write(m_actualPerformanceScript);
        a_writer.Write(m_finished);
        a_writer.Write(m_partiallyFinished);
        a_writer.Write(m_fast);
        a_writer.Write(m_normal);
        a_writer.Write(m_slow);
        a_writer.Write(m_priority);
        a_writer.Write(m_minHeight);
        a_writer.Write(m_proportionalHeightWeight);
        a_writer.Write(m_show);
    }

    //Actuals colors
    private Color ActualPerformanceFast => Color.Yellow;
    private Color ActualPerformanceNormal => Color.White;
    private Color ActualPerformanceSlow => Color.LightCoral;
    private Color ActualStatusFinished => Color.LightSteelBlue;
    private Color ActualStatusPartiallyFinished => Color.LightGreen;

    private void Initialize()
    {
        m_actualStatusScript = ActualStatusScript;
        m_actualPerformanceScript = ActualPerformanceScript;

        Finished = ActualStatusFinished;
        PartiallyFinished = ActualStatusPartiallyFinished;
        Fast = ActualPerformanceFast;
        Normal = ActualPerformanceNormal;
        Slow = ActualPerformanceSlow;
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    private string m_actualStatusScript = string.Empty;

    /// <summary>
    /// Script used for generating Labels for the Status segment of Actuals.
    /// </summary>
    public string ActualStatusScript
    {
        get => m_actualStatusScript;
        set => m_actualStatusScript = value;
    }

    private string m_actualPerformanceScript = string.Empty;

    /// <summary>
    /// Script used for generating Labels for the Performance segment of Actuals.
    /// </summary>
    public string ActualPerformanceScript
    {
        get => m_actualPerformanceScript;
        set => m_actualPerformanceScript = value;
    }

    private Color m_finished;

    public Color Finished
    {
        get => m_finished;
        set => m_finished = value;
    }

    private Color m_partiallyFinished;

    public Color PartiallyFinished
    {
        get => m_partiallyFinished;
        set => m_partiallyFinished = value;
    }

    private Color m_fast;

    public Color Fast
    {
        get => m_fast;
        set => m_fast = value;
    }

    private Color m_normal;

    public Color Normal
    {
        get => m_normal;
        set => m_normal = value;
    }

    private Color m_slow;

    public Color Slow
    {
        get => m_slow;
        set => m_slow = value;
    }

    private uint m_priority;

    public uint Priority
    {
        get => m_priority;
        set => m_priority = value;
    }

    private uint m_minHeight;

    public uint MinHeight
    {
        get => m_minHeight;
        set => m_minHeight = value;
    }

    private uint m_proportionalHeightWeight;

    public uint ProportionalHeightWeight
    {
        get => m_proportionalHeightWeight;
        set => m_proportionalHeightWeight = value;
    }

    private bool m_show;

    public bool Show
    {
        get => m_show;
        set => m_show = value;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    public BlockActualsSettings Clone()
    {
        return (BlockActualsSettings)MemberwiseClone();
    }
}