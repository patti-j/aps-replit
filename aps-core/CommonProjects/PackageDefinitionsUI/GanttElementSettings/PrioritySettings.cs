using System.Drawing;

using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Interfaces;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class PrioritySettings : ISettingData
{
    public PrioritySettings(IMainForm a_mainForm)
    {
        Script = "";
        Show = false;
        Priority = 6;
        MinHeight = 0;
        ProportionalHeightWeight = 25;
        IDynamicSkin theme = a_mainForm.CurrentBrand.ActiveTheme;
        m_lowRangeMinColor = ColorUtils.CalculateColor(Color.White, theme.PriorityLow, 200);
        m_lowRangeMaxColor = theme.PriorityLow;
        m_mediumRangeMinColor = ColorUtils.CalculateColor(Color.White, theme.PriorityMedium, 200);
        m_mediumRangeMaxColor = theme.PriorityMedium;
        m_highRangeMinColor = ColorUtils.CalculateColor(Color.White, theme.PriorityHigh, 200);
        m_highRangeMaxColor = theme.PriorityHigh;
    }

    #region IPTSerializable
    public PrioritySettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 11000)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);

            a_reader.Read(out m_lowRangeEnd);
            a_reader.Read(out m_mediumRangeEnd);
            a_reader.Read(out m_highRangeEnd);

            a_reader.Read(out m_lowRangeMinColor);
            a_reader.Read(out m_lowRangeMaxColor);

            a_reader.Read(out m_mediumRangeMinColor);
            a_reader.Read(out m_mediumRangeMaxColor);

            a_reader.Read(out m_highRangeMinColor);
            a_reader.Read(out m_highRangeMaxColor);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
        }
        else
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Script);
        a_writer.Write(Show);

        a_writer.Write(LowRangeEnd);
        a_writer.Write(MediumRangeEnd);
        a_writer.Write(HighRangeEnd);

        a_writer.Write(LowRangeMinColor);
        a_writer.Write(LowRangeMaxColor);

        a_writer.Write(MediumRangeMinColor);
        a_writer.Write(MediumRangeMaxColor);

        a_writer.Write(HighRangeMinColor);
        a_writer.Write(HighRangeMaxColor);

        a_writer.Write(Priority);
        a_writer.Write(MinHeight);
        a_writer.Write(ProportionalHeightWeight);
    }

    #region Range Properties
    private int m_lowRangeEnd = 10;

    public int LowRangeEnd
    {
        get => m_lowRangeEnd;
        set => m_lowRangeEnd = value;
    }

    private int m_mediumRangeEnd = 20;

    public int MediumRangeEnd
    {
        get => m_mediumRangeEnd;
        set => m_mediumRangeEnd = value;
    }

    private int m_highRangeEnd = 30;

    public int HighRangeEnd
    {
        get => m_highRangeEnd;
        set => m_highRangeEnd = value;
    }
    #endregion

    #region Color blending properties
    private Color m_lowRangeMinColor;

    public Color LowRangeMinColor
    {
        get => m_lowRangeMinColor;
        set => m_lowRangeMinColor = value;
    }

    private Color m_lowRangeMaxColor;

    public Color LowRangeMaxColor
    {
        get => m_lowRangeMaxColor;
        set => m_lowRangeMaxColor = value;
    }

    private Color m_mediumRangeMinColor;

    public Color MediumRangeMinColor
    {
        get => m_mediumRangeMinColor;
        set => m_mediumRangeMinColor = value;
    }

    private Color m_mediumRangeMaxColor;

    public Color MediumRangeMaxColor
    {
        get => m_mediumRangeMaxColor;
        set => m_mediumRangeMaxColor = value;
    }

    private Color m_highRangeMinColor;

    public Color HighRangeMinColor
    {
        get => m_highRangeMinColor;
        set => m_highRangeMinColor = value;
    }

    private Color m_highRangeMaxColor;

    public Color HighRangeMaxColor
    {
        get => m_highRangeMaxColor;
        set => m_highRangeMaxColor = value;
    }
    #endregion

    public string Script;
    public bool Show;
    public uint Priority;
    public uint MinHeight;
    public uint ProportionalHeightWeight;
    #endregion

    public int UniqueId => 904;

    public string SettingKey => StaticSettingKey;
    public string Description => "TODO:"; //TODO: Implement this
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SegmentSettings;
    public string SettingCaption => "Priority settings";
    public static string StaticSettingKey => "segmentSettings_Priority";
}