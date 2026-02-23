using System.Windows.Forms;

using DevExpress.XtraEditors;

using PT.ComponentLibrary;
using PT.ComponentLibrary.Forms;

using Timer = System.Windows.Forms.Timer;

namespace PT.UI;

/// <summary>
/// Summary description for ClosingDialog.
/// </summary>
internal class RestartPromptDialog : PTStyledForm
{
    private LabelControl label1;
    private SimpleButton DontExitBTN;
    private SimpleButton ExitNowBTN;
    private Timer timer1;
    private ImageList imageList1;
    private PanelControl panel1;
    private LabelControl labelControl1;
    private System.ComponentModel.IContainer components;

    internal RestartPromptDialog()
    {
        InitializeComponent();

        timer1.Enabled = true;
        TimeLeft = Convert.ToInt32(TotalTime.TotalSeconds);

        Localize();
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeFormIncludingCaption(this);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        //DisposeUtilities.DisposeImages(ExitNowBTN);
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RestartPromptDialog));
        this.label1 = new DevExpress.XtraEditors.LabelControl();
        this.DontExitBTN = new DevExpress.XtraEditors.SimpleButton();
        this.ExitNowBTN = new DevExpress.XtraEditors.SimpleButton();
        this.imageList1 = new System.Windows.Forms.ImageList(this.components);
        this.timer1 = new System.Windows.Forms.Timer(this.components);
        this.panel1 = new DevExpress.XtraEditors.PanelControl();
        this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
        ((System.ComponentModel.ISupportInitialize)(this.panel1)).BeginInit();
        this.panel1.SuspendLayout();
        this.SuspendLayout();
        // 
        // label1
        // 
        this.label1.Appearance.BackColor = System.Drawing.Color.Transparent;
        this.label1.Appearance.Options.UseBackColor = true;
        this.label1.Location = new System.Drawing.Point(108, 52);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(61, 13);
        this.label1.TabIndex = 0;
        this.label1.Text = "Restarting in";
        // 
        // DontExitBTN
        // 
        this.DontExitBTN.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.DontExitBTN.Location = new System.Drawing.Point(58, 81);
        this.DontExitBTN.Name = "DontExitBTN";
        this.DontExitBTN.Size = new System.Drawing.Size(91, 28);
        this.DontExitBTN.TabIndex = 1;
        this.DontExitBTN.Text = "&Keep working";
        this.DontExitBTN.Click += new System.EventHandler(this.DontExitBTN_Click);
        // 
        // ExitNowBTN
        // 
        this.ExitNowBTN.ImageOptions.Image = global::PT.UI.Properties.Resources.ExitRed_16;
        this.ExitNowBTN.Location = new System.Drawing.Point(171, 81);
        this.ExitNowBTN.Name = "ExitNowBTN";
        this.ExitNowBTN.Size = new System.Drawing.Size(107, 28);
        this.ExitNowBTN.TabIndex = 2;
        this.ExitNowBTN.Text = "&Restart Now";
        this.ExitNowBTN.Click += new System.EventHandler(this.ExitNowBTN_Click);
        // 
        // imageList1
        // 
        this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
        this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
        this.imageList1.Images.SetKeyName(0, "");
        // 
        // timer1
        // 
        this.timer1.Interval = 1000;
        this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
        // 
        // panel1
        // 
        this.panel1.Controls.Add(this.labelControl1);
        this.panel1.Controls.Add(this.label1);
        this.panel1.Controls.Add(this.ExitNowBTN);
        this.panel1.Controls.Add(this.DontExitBTN);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel1.Location = new System.Drawing.Point(0, 0);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(330, 132);
        this.panel1.TabIndex = 8;
        // 
        // labelControl1
        // 
        this.labelControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
        this.labelControl1.Appearance.Options.UseBackColor = true;
        this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
        this.labelControl1.Location = new System.Drawing.Point(12, 7);
        this.labelControl1.Name = "labelControl1";
        this.labelControl1.Size = new System.Drawing.Size(313, 39);
        this.labelControl1.TabIndex = 7;
        this.labelControl1.Text = "A restart is pending, but not required. The program will restart automatically if" +
                                  " idle.  Press \'Keep working\' to postpone the restart.";
        // 
        // RestartPromptDialog
        // 
        this.AcceptButton = this.ExitNowBTN;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.CancelButton = this.DontExitBTN;
        this.ClientSize = new System.Drawing.Size(330, 132);
        this.Controls.Add(this.panel1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "RestartPromptDialog";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Restart Pending";
        ((System.ComponentModel.ISupportInitialize)(this.panel1)).EndInit();
        this.panel1.ResumeLayout(false);
        this.panel1.PerformLayout();
        this.ResumeLayout(false);
    }
    #endregion

    private void ExitNowBTN_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void DontExitBTN_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Ignore;
        Close();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (TimeLeft <= 0)
        {
            timer1.Enabled = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            TimeLeft -= 1;
        }
    }

    private TimeSpan TotalTime => TimeSpan.FromMinutes(1);

    private int m_timeLeft = 3;

    private int TimeLeft
    {
        get => m_timeLeft;
        set
        {
            m_timeLeft = value;
            label1.Text = string.Format("Restarting in {0}...", value);
        }
    }

    internal bool ForceRestart
    {
        set
        {
            DontExitBTN.Enabled = !value;
            if (value)
            {
                labelControl1.Text = "A restart is pending, and is required. The program will restart automatically. Please finish in progress work.";
            }
        }
    }
}