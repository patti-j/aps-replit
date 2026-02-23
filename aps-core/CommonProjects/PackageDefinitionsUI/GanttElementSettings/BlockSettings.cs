using System.Drawing;

using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class BlockSettings : ISettingData, ICloneable
{
    public const int UNIQUE_ID = 917;

    public string SettingKey => "BlockSettings_Labeling";
    public string Description => "TODO:";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => "Block Labels";
    public string SettingCaption => "Block label settings";

    #region IPTSerializable Members
    public BlockSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 11000)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_tooltipScript);
            a_reader.Read(out m_blockScript);
            a_reader.Read(out int textDirection);
            m_textDirection = (ETextDirections)textDirection;
            a_reader.Read(out m_padding);
            Read(a_reader, out m_labelFont);
            a_reader.Read(out m_unreviewedColor);
        }
        else
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_tooltipScript);
            a_reader.Read(out m_blockScript);
            a_reader.Read(out int textDirection);
            m_textDirection = (ETextDirections)textDirection;
            a_reader.Read(out m_padding);
            Read(a_reader, out m_labelFont);
        }
    }

    public BlockSettings()
    {
        Initialize();
    }

    public BlockSettings(BlockSettings a_settings)
    {
        m_tooltipScript = a_settings.TooltipScript;
        m_blockScript = a_settings.BlockScript;
        m_bools[c_highlightUnreviewed] = a_settings.HighlightUnreviewedJobs;
        m_bools[c_noClipping] = a_settings.NoClipping;
        m_bools[c_useBlockLabel] = a_settings.ShowBlockLabelAlways;
        m_bools[c_fastDraw] = a_settings.UseFastDraw;
        m_bools[c_solidColors] = a_settings.UseSolidColors;
        m_bools[c_wrapText] = a_settings.WrapText;
        m_labelFont = a_settings.LabelFont;
        m_unreviewedColor = a_settings.UnreviewedColor;
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(m_tooltipScript);
        a_writer.Write(m_blockScript);
        a_writer.Write((int)m_textDirection);
        a_writer.Write(m_padding);
        Write(a_writer, m_labelFont);
        a_writer.Write(m_unreviewedColor);
    }

    /// <summary>
    /// Removed from IReader to avoid requiring System.Drawing library in common libraries
    /// </summary>
    private static void Read(IReader a_reader, out Font a_labelFont)
    {
        a_reader.Read(out string fontFamily);
        a_reader.Read(out bool bold);
        a_reader.Read(out bool italic);
        a_reader.Read(out bool strikeout);
        a_reader.Read(out bool underline);
        FontStyle style = FontStyle.Regular;
        if (bold)
        {
            style = style | FontStyle.Bold;
        }

        if (italic)
        {
            style = style | FontStyle.Italic;
        }

        if (strikeout)
        {
            style = style | FontStyle.Strikeout;
        }

        if (underline)
        {
            style = style | FontStyle.Underline;
        }

        a_reader.Read(out float size);
        a_labelFont = new Font(fontFamily, size, style);
    }

    /// <summary>
    /// Removed from IWriter to avoid requiring System.Drawing library in common libraries
    /// </summary>
    private static void Write(IWriter a_writer, Font a_data)
    {
        a_writer.Write(a_data.FontFamily.Name);
        a_writer.Write(a_data.Bold);
        a_writer.Write(a_data.Italic);
        a_writer.Write(a_data.Strikeout);
        a_writer.Write(a_data.Underline);
        a_writer.Write(a_data.Size);
    }

    private Color m_defaultUnreviewedColor => Color.Fuchsia;

    private void Initialize()
    {
        m_tooltipScript = TooltipScript;
        m_blockScript = BlockScript;
        m_bools[c_highlightUnreviewed] = HighlightUnreviewedJobs;
        m_bools[c_noClipping] = NoClipping;
        m_bools[c_useBlockLabel] = ShowBlockLabelAlways;
        m_bools[c_fastDraw] = UseFastDraw;
        m_bools[c_solidColors] = UseSolidColors;
        m_bools[c_wrapText] = WrapText;
        m_labelFont = LabelFont;
        m_unreviewedColor = m_defaultUnreviewedColor;
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region Block Properties
    private BoolVector32 m_bools;

    private const short c_useBlockLabel = 0;
    private const short c_wrapText = 1;
    private const short c_fastDraw = 2;
    private const short c_highlightUnreviewed = 3;
    private const short c_solidColors = 4;
    private const short c_noClipping = 5;

    private string m_tooltipScript = "Slack: [Activity.Slack]";

    /// <summary>
    /// Script used for generating Tooltips for Gantt Blocks.
    /// </summary>
    public string TooltipScript
    {
        get => m_tooltipScript;
        set => m_tooltipScript = value;
    }

    private string m_blockScript = "[Job.Name] [ResourceOperation.Name]";

    /// <summary>
    /// Script used for generating Labels for Gantt Blocks.
    /// </summary>
    public string BlockScript
    {
        get => m_blockScript;
        set => m_blockScript = value;
    }

    private Color m_unreviewedColor;

    public Color UnreviewedColor
    {
        get => m_unreviewedColor;
        set => m_unreviewedColor = value;
    }

    public bool ShowBlockLabelAlways
    {
        get => m_bools[c_useBlockLabel];
        set => m_bools[c_useBlockLabel] = value;
    }

    /// <summary>
    /// Whether to highlight Jobs in the Gantt that are not Reviewed.
    /// </summary>
    public bool HighlightUnreviewedJobs
    {
        get => m_bools[c_highlightUnreviewed];
        set => m_bools[c_highlightUnreviewed] = value;
    }

    /// <summary>
    /// Whether to speed up gantt drawing by suppressing Labels and fancy coloring.
    /// </summary>
    public bool UseFastDraw
    {
        get => m_bools[c_fastDraw];
        set => m_bools[c_fastDraw] = value;
    }

    /// <summary>
    /// Whether to wrap lines of Label text.
    /// </summary>
    public bool WrapText
    {
        get => m_bools[c_wrapText];
        set => m_bools[c_wrapText] = value;
    }

    /// <summary>
    /// Whether to clip label text that would overflow the block.
    /// </summary>
    public bool NoClipping
    {
        get => false; //noClipping; } 2017.03.20: removed because this was rarely used and whatever use it had should be done in a better way.
        set => m_bools[c_noClipping] = value;
    }

    /// <summary>
    /// Use solid colors for Gantt Blocks and Capacity Intervals instead of dithered colors.
    /// </summary>
    public bool UseSolidColors
    {
        get => m_bools[c_solidColors];
        set => m_bools[c_solidColors] = value;
    }

    public enum ETextDirections { Horizontal = 0, Vertical, Automatic }

    private ETextDirections m_textDirection = ETextDirections.Automatic;

    /// <summary>
    /// The direction that text displays in in Labels.
    /// </summary>
    public ETextDirections TextDirection
    {
        get => m_textDirection;
        set => m_textDirection = value;
    }

    private float m_padding = 2.00f;

    /// <summary>
    /// The pixels ofset to draw from the top and left.
    /// </summary>
    public float Padding
    {
        get => m_padding;
        set => m_padding = value;
    }

    private Font m_labelFont = new ("Verdana", 10);

    public Font LabelFont
    {
        get => m_labelFont;
        set => m_labelFont = value;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    public BlockSettings Clone()
    {
        return (BlockSettings)MemberwiseClone();
    }
    #endregion
}