using System.Drawing;
using System.Windows.Forms;

using Timer = System.Windows.Forms.Timer;

namespace PT.ComponentLibrary;

/// <summary>
/// Summary description for TipWindow.
/// </summary>
public sealed class TipWindow : PTBaseControl
{
    private TextBox m_textBox;

    private readonly Timer m_hoverTimer = new ();

    [Obsolete("Use ToolTipWindow if possible. Merge this class functionality with GanttDotNet.ToolTipWindow")]
    public TipWindow()
    {
        // This call is required by the Windows.Forms Form Designer.
        InitializeComponent();

        BackColor = SystemColors.Info; //default

        Deactivate();
        m_hoverTimer.Enabled = false;
        //this.timer1.Enabled=false;
        m_hoverTimer.Tick += new EventHandler(hoverTimer_Tick);
        m_hoverTimer.Interval = 800; //milliseconds
    }

    protected override void OnPaint(PaintEventArgs a_e)
    {
        base.OnPaint(a_e);
        using (Brush b = new SolidBrush(BackColor))
        {
            a_e.Graphics.FillRectangle(b, ClientRectangle);
        }

        using (Pen p = new (Color.Black))
        {
            Rectangle borderRect = ClientRectangle;
            borderRect.Width = borderRect.Width - 1;
            borderRect.Height = borderRect.Height - 1;
            a_e.Graphics.DrawRectangle(p, borderRect);
        }

        using (Brush textBrush = new SolidBrush(ForeColor))
        {
            a_e.Graphics.DrawString(Text, TextFont, textBrush, new RectangleF(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height));
        }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool a_disposing)
    {
        m_hoverTimer.Dispose();
        base.Dispose(a_disposing);
    }

    #region Component Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.m_textBox = new System.Windows.Forms.TextBox();
        this.SuspendLayout();
        // 
        // textBox
        // 
        this.m_textBox.BackColor = System.Drawing.SystemColors.Info;
        this.m_textBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.m_textBox.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.m_textBox.Location = new System.Drawing.Point(0, 0);
        this.m_textBox.Multiline = true;
        this.m_textBox.Name = "m_textBox";
        this.m_textBox.ReadOnly = true;
        this.m_textBox.Size = new System.Drawing.Size(168, 32);
        this.m_textBox.TabIndex = 3;
        this.m_textBox.Visible = false;
        this.m_textBox.WordWrap = false;
        this.m_textBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TipWindow_MouseMove);
        // 
        // TipWindow
        // 
        this.BackColor = System.Drawing.SystemColors.Info;
        this.Controls.Add(this.m_textBox);
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Name = "TipWindow";
        this.Size = new System.Drawing.Size(168, 56);
        this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TipWindow_MouseMove);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
    #endregion

    private int m_maxWidth;
    private int m_maxHeight;

    public void SetMaxSize(Size a_maxSize)
    {
        m_maxWidth = a_maxSize.Width;
        m_maxHeight = a_maxSize.Height;
        CalcuateSize();
    }

    public void SetText(string a_aText, int a_aMaxWidth, int a_aMaxHeight)
    {
        m_maxWidth = a_aMaxWidth;
        m_maxHeight = a_aMaxHeight;
        Text = a_aText;
    }

    private void CalcuateSize()
    {
        if (m_textBox.Text.Trim().Length == 0)
        {
            Size = new Size(10, 5); //size for empty box so user can see there's nothing to show.
        }
        else
        {
            Graphics g = CreateGraphics();
            int newHeight = Convert.ToInt32(GetTextLinesCount() * TextFont.GetHeight(g));
            Height = Math.Min(newHeight, m_maxHeight);
            //float pixelsPerPoint = g.DpiX / 72f;
            int newWidth = 0;
            //Set width to show longest line
            System.Collections.IEnumerator e = m_textBox.Lines.GetEnumerator();
            while (e.MoveNext())
            {
                int lineLength = Convert.ToInt32(g.MeasureString((string)e.Current, TextFont).Width);
                if (newWidth < lineLength)
                {
                    newWidth = Math.Min(lineLength, m_maxWidth);
                }
            }

            newWidth++;
            Width = newWidth;
        }
    }

    public void Clear()
    {
        Text = "";
    }

    public override string Text
    {
        get => m_textBox.Text;
        set
        {
            m_textBox.Text = value;
            CalcuateSize();
            m_textBox.Refresh();
            Refresh();
        }
    }

    public void Deactivate()
    {
        //this.timer1.Enabled=false;
        m_hoverTimer.Enabled = false;
        Visible = false;
        m_hoverObject = null;
    }

    public int GetTextLinesCount()
    {
        return m_textBox.Lines.Length;
    }

    public Font TextFont
    {
        get => m_theme.TooltipFont;
        set => m_textBox.Font = value;
    }

    private void TipWindow_MouseMove(object sender, MouseEventArgs a_e)
    {
        Deactivate(); //don't want the mouse over the tip window.  It may not have a chance to hide before this happens.
    }

    #region Hover
    private TimeSpan m_hoverDelay = TimeSpan.FromMilliseconds(2000);

    public TimeSpan HoverDelay
    {
        get => m_hoverDelay;
        set => m_hoverDelay = value;
    }

    private DateTime m_hoverObjectLastSetTime;
    private object m_hoverObject;

    public object HoverObject => m_hoverObject;

    /// <summary>
    /// If this is the same object set for more than Hover Delay then the Tooltip becomes visible. (Provided that the Tip Window is Enabled.)
    /// </summary>
    public void SetHoverObject(object a_newHoverObject)
    {
        if (a_newHoverObject != null && a_newHoverObject != m_hoverObject)
        {
            //Enable since setting this so must want to use it
            //this.timer1.Enabled=true;
            m_hoverTimer.Enabled = true;

            m_hoverObject = a_newHoverObject;
            m_hoverObjectLastSetTime = DateTime.Now;
        }
    }

    //If a tick occurs then enough time has passed on the same object and the tip window should be shown.
    private void hoverTimer_Tick(object sender, EventArgs a_e)
    {
        if (DateTime.Now.Subtract(m_hoverObjectLastSetTime) > HoverDelay)
        {
            AboutToShow?.Invoke(this);
            m_hoverTimer.Enabled = false;
            Visible = true;
        }
    }

    public delegate void AboutToShowHandler(TipWindow a_tipWindow);

    public event AboutToShowHandler AboutToShow;
    #endregion Hover
}