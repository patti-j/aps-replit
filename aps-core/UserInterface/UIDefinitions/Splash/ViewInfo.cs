using System.Drawing;

using DevExpress.Utils.Drawing;
using DevExpress.Utils.Text;

namespace PT.UIDefinitions.Splash;

public class ViewInfo
{
    public ViewInfo()
    {
        StatusStage = string.Empty;
        WarningStage = string.Empty;
        VersionStage = string.Empty;
    }

    public string StatusStage { get; set; }
    public string WarningStage { get; set; }
    public string VersionStage { get; set; }
    public string InstanceName { get; set; }

    // #4396d8 This is the hex for the color Sarah wanted
    // The original white looks better on the new splash screen, but maybe we'll use this color again
    // in the future for some other text. 
    //private Color c_PlanetTogetherBlue = Color.FromArgb(67, 150, 216);

    #region Version
    public PointF CalcVersionLabelPoint(GraphicsCache a_cache, Rectangle a_bounds)

    {
        int yOffset = (int)(a_bounds.Height * .92);
        int xOffset = (int)(a_bounds.Width * .80);
        Size size = TextUtils.GetStringSize(a_cache.Graphics, InstanceNameAndVersionText, VersionFont);
        if (xOffset + size.Width > a_bounds.Width)
        {
            xOffset = a_bounds.Width - size.Width - a_bounds.Width / 10; // subtracting by a tenth here is for padding  
        }

        return new Point(xOffset, yOffset);
    }

    private Font m_versionFont;

    public Font VersionFont
    {
        get
        {
            if (m_versionFont == null)
            {
                m_versionFont = new Font("Noto Sans", 14, FontStyle.Bold);
            }

            return m_versionFont;
        }
    }

    private Brush m_versionBrush;

    public Brush VersionBrush
    {
        get
        {
            if (m_versionBrush == null)
            {
                m_versionBrush = new SolidBrush(Color.White);
            }

            return m_versionBrush;
        }
    }

    public string InstanceNameAndVersionText => string.Format("{0} | {1}", InstanceName, VersionStage);
    #endregion

    #region Warning
    public PointF CalcWarningLabelPoint(GraphicsCache a_cache, Rectangle a_bounds)
    {
        int yOffset = (int)(a_bounds.Height * .88);
        WarningStage = BreakStringToFitSplash(WarningStage, a_cache, a_bounds, WarningFont);
        Size size = TextUtils.GetStringSize(a_cache.Graphics, WarningText, WarningFont);
        return new Point(a_bounds.Width / 2 - size.Width / 2, yOffset);
    }

    private Font m_warningFont;

    public Font WarningFont
    {
        get
        {
            if (m_warningFont == null)
            {
                m_warningFont = new Font("Noto Sans", 14);
            }

            return m_warningFont;
        }
    }

    private Brush m_warningBrush;

    public Brush WarningBrush
    {
        get
        {
            if (m_warningBrush == null)
            {
                m_warningBrush = new SolidBrush(Color.Red);
            }

            return m_warningBrush;
        }
    }

    public string WarningText => string.Format("{0}", WarningStage);
    #endregion

    #region Status
    public PointF CalcStatusLabelPoint(GraphicsCache a_cache, Rectangle a_bounds)
    {
        int yOffset = (int)(a_bounds.Height * .4);
        StatusStage = BreakStringToFitSplash(StatusStage, a_cache, a_bounds, StatusFont);
        Size size = TextUtils.GetStringSize(a_cache.Graphics, StatusText, StatusFont);
        return new Point(a_bounds.Width / 2 - size.Width / 2, yOffset);
    }

    private Font m_statusFont;

    public Font StatusFont
    {
        get
        {
            if (m_statusFont == null)
            {
                m_statusFont = new Font("Noto Sans", 20);
            }

            return m_statusFont;
        }
    }

    private Brush m_statusBrush;

    public Brush StatusBrush
    {
        get
        {
            if (m_statusBrush == null)
            {
                m_statusBrush = new SolidBrush(Color.White);
            }

            return m_statusBrush;
        }
    }

    public string StatusText => string.Format("{0}", StatusStage);
    #endregion

    #region Utility
    private string BreakStringToFitSplash(string a_message, GraphicsCache a_cache, Rectangle a_bounds, Font a_font)
    {
        //Used to determine number of spaces when padding. Using a dash as a space returns a size of 0.
        int spaceSize = TextUtils.GetStringSize(a_cache.Graphics, "-", a_font).Width;


        List<string> lines = new ();
        lines.Add("");
        //Keep adding strings until you read 80% of the bounds of the rectangle.  Then break
        foreach (string s in a_message.Split(' '))
        {
            string lineWithNextString = lines[lines.Count - 1] + s + " ";

            int lineWithNextStringSize = TextUtils.GetStringSize(a_cache.Graphics, lineWithNextString, a_font).Width;
            int lineWithoutNextStringSize = TextUtils.GetStringSize(a_cache.Graphics, lines[lines.Count - 1], a_font).Width;
            if (lineWithNextStringSize > .8 * a_bounds.Width)
            {
                //Pad current line and then add string to a new line
                int numberOfSpaces = (a_bounds.Width - lineWithoutNextStringSize) / spaceSize / 2;
                int length = lines[lines.Count - 1].Length;
                lines[lines.Count - 1] = lines[lines.Count - 1].PadLeft(length + numberOfSpaces);

                lines.Add(s);
            }
            else
            {
                lines[lines.Count - 1] = lineWithNextString;
            }
        }

        return string.Join("\n", lines);
    }
    #endregion
}