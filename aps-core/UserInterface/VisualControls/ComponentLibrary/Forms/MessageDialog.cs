using System.Drawing;
using System.Windows.Forms;

using PT.APSCommon.Extensions;
using PT.UIDefinitions;
using PT.UIDefinitions.Interfaces;

namespace PT.ComponentLibrary.Forms;

/// <summary>
/// Summary description for MessageDialog.
/// </summary>
public class MessageDialog : AutoFadeForm
{
    private System.ComponentModel.IContainer components;

    private readonly Icon m_infoIcon;
    private readonly Icon m_alertIcon;
    private DevExpress.XtraEditors.MemoEdit m_messageBox;
    private DevExpress.XtraEditors.PanelControl panelControl1;
    private DevExpress.XtraEditors.SimpleButton simpleButton_ViewLogs;
    private readonly Icon m_errorIcon;
    private readonly IErrorLogViewManager m_errorLogViewManager;

    private static readonly string s_errorLocalized = "error".Localize();
    private static readonly string s_logLocalized = "log".Localize();

    public MessageDialog(string caption, string message, MessageBoxIcon icon, bool autoCloseByTimeLapse, IErrorLogViewManager a_errorLogViewManager) : base(autoCloseByTimeLapse, "Message")
    {
        InitializeComponent();
        m_errorLogViewManager = a_errorLogViewManager;
        m_fadeDurationMs = 2000;
        //Load Icons
        m_infoIcon = Properties.Resources.InfoBlue24;
        m_alertIcon = Properties.Resources.ErrorYellow24;
        m_errorIcon = Properties.Resources.Error2Red24;

        SetMessage(caption, message, icon);

        UILocalizationHelper.LocalizeFormIncludingCaption(this);
    }

    public MessageDialog(string caption, string message, MessageBoxIcon icon, bool autoCloseByTimeLapse, Form aParentForm, IErrorLogViewManager a_errorLogViewManager)
        : this(caption, message, icon, autoCloseByTimeLapse, a_errorLogViewManager)
    {
        Owner = aParentForm;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_caption"></param>
    /// <param name="a_message"></param>
    /// <param name="a_icon"></param>
    public void SetMessage(string a_caption, string a_message, MessageBoxIcon a_icon)
    {
        string caption = a_caption.Localize();
        if (Text != caption)
        {
            Text = caption;
        }

        string messageLower = a_message.ToLower();
        simpleButton_ViewLogs.Visible = m_errorLogViewManager != null && messageLower.Contains(s_errorLocalized) && messageLower.Contains(s_logLocalized);
        m_messageBox.Text = a_message.Localize();

        switch (a_icon)
        {
            case MessageBoxIcon.Warning:
                Icon = m_alertIcon;
                break;
            case MessageBoxIcon.Error:
                Icon = m_errorIcon;
                break;
            default:
                Icon = m_infoIcon;
                break;
        }

        StartAutoHide();
    }

    public string Message => m_messageBox.Text;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (components != null)
            {
                components.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.m_messageBox = new DevExpress.XtraEditors.MemoEdit();
        this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
        this.simpleButton_ViewLogs = new DevExpress.XtraEditors.SimpleButton();
        ((System.ComponentModel.ISupportInitialize)(this.m_messageBox.Properties)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
        this.panelControl1.SuspendLayout();
        this.SuspendLayout();
        // 
        // m_messageBox
        // 
        this.m_messageBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.m_messageBox.EditValue = "";
        this.m_messageBox.Location = new System.Drawing.Point(2, 2);
        this.m_messageBox.Name = "m_messageBox";
        this.m_messageBox.Properties.AllowFocused = false;
        this.m_messageBox.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        this.m_messageBox.Properties.ReadOnly = true;
        this.m_messageBox.Properties.UseReadOnlyAppearance = false;
        this.m_messageBox.Size = new System.Drawing.Size(420, 115);
        this.m_messageBox.TabIndex = 3;
        this.m_messageBox.MouseEnter += new System.EventHandler(this.m_messageBox_MouseEnter);
        // 
        // panelControl1
        // 
        this.panelControl1.Controls.Add(this.m_messageBox);
        this.panelControl1.Controls.Add(this.simpleButton_ViewLogs);
        this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelControl1.Location = new System.Drawing.Point(0, 0);
        this.panelControl1.Name = "panelControl1";
        this.panelControl1.Size = new System.Drawing.Size(424, 142);
        this.panelControl1.TabIndex = 4;
        // 
        // simpleButton_ViewLogs
        // 
        this.simpleButton_ViewLogs.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.simpleButton_ViewLogs.Location = new System.Drawing.Point(2, 117);
        this.simpleButton_ViewLogs.Name = "simpleButton_ViewLogs";
        this.simpleButton_ViewLogs.Size = new System.Drawing.Size(420, 23);
        this.simpleButton_ViewLogs.TabIndex = 4;
        this.simpleButton_ViewLogs.Text = "View Message Logs";
        this.simpleButton_ViewLogs.Click += new System.EventHandler(this.simpleButton_ViewLogs_Click);
        // 
        // MessageDialog
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.ClientSize = new System.Drawing.Size(424, 142);
        this.Controls.Add(this.panelControl1);
        this.MinimizeBox = false;
        this.Name = "MessageDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "MessageDialog";
        this.TopMost = true;
        this.MouseEnter += new System.EventHandler(this.MessageDialog_MouseEnter);
        this.MouseLeave += new System.EventHandler(this.MessageDialog_MouseLeave);
        this.Move += new System.EventHandler(this.MessageDialog_Move);
        this.Resize += new System.EventHandler(this.MessageDialog_Resize);
        ((System.ComponentModel.ISupportInitialize)(this.m_messageBox.Properties)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
        this.panelControl1.ResumeLayout(false);
        this.ResumeLayout(false);
    }
    #endregion

    private void MessageDialog_MouseEnter(object sender, EventArgs e)
    {
        StopAutoHide();
    }

    private void MessageDialog_MouseLeave(object sender, EventArgs e)
    {
//			this.autoCloseTimer.Enabled=true;  //If the user is investigating then don't close it.  Let the user close it.
    }

    private void messageBox_Resize(object sender, EventArgs e)
    {
        Width = m_messageBox.Location.X + m_messageBox.Width + PTAppearance.BorderWidth;
        ClientSize = new Size(Width, m_messageBox.Location.Y + m_messageBox.Height + PTAppearance.BorderWidth);
    }

    private void m_messageBox_MouseEnter(object sender, EventArgs e)
    {
        StopAutoHide();
    }

    private void MessageDialog_Move(object sender, EventArgs e)
    {
        StopAutoHide();
    }

    private void MessageDialog_Resize(object sender, EventArgs e) { }

    private void simpleButton_ViewLogs_Click(object sender, EventArgs e)
    {
        m_errorLogViewManager.ShowErrorLog();
        simpleButton_ViewLogs.Visible = false;
        Close();
    }
}