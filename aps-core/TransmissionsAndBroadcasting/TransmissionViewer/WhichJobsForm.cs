using System.Windows.Forms;

namespace TransmissionViewer;

/// <summary>
/// Summary description for WhichJobsForm.
/// </summary>
public class WhichJobsForm : Form
{
    private Button okBTN;
    private Button cancelBTN;
    private GroupBox groupBox1;
    private RadioButton showSelectJobsBTN;
    private TextBox externalIdContainsTB;
    private RadioButton showAllJobsBTN;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private readonly System.ComponentModel.Container components = null;

    public WhichJobsForm()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();

        //
        // TODO: Add any constructor code after InitializeComponent call
        //
    }

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
        this.okBTN = new System.Windows.Forms.Button();
        this.cancelBTN = new System.Windows.Forms.Button();
        this.groupBox1 = new System.Windows.Forms.GroupBox();
        this.externalIdContainsTB = new System.Windows.Forms.TextBox();
        this.showSelectJobsBTN = new System.Windows.Forms.RadioButton();
        this.showAllJobsBTN = new System.Windows.Forms.RadioButton();
        this.groupBox1.SuspendLayout();
        this.SuspendLayout();
        // 
        // okBTN
        // 
        this.okBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.okBTN.Location = new System.Drawing.Point(464, 217);
        this.okBTN.Name = "okBTN";
        this.okBTN.Size = new System.Drawing.Size(63, 20);
        this.okBTN.TabIndex = 0;
        this.okBTN.Text = "&OK";
        this.okBTN.Click += new System.EventHandler(this.okBTN_Click);
        // 
        // cancelBTN
        // 
        this.cancelBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.cancelBTN.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.cancelBTN.Location = new System.Drawing.Point(531, 217);
        this.cancelBTN.Name = "cancelBTN";
        this.cancelBTN.Size = new System.Drawing.Size(62, 20);
        this.cancelBTN.TabIndex = 0;
        this.cancelBTN.Text = "&Cancel";
        this.cancelBTN.Click += new System.EventHandler(this.cancelBTN_Click);
        // 
        // groupBox1
        // 
        this.groupBox1.Controls.Add(this.externalIdContainsTB);
        this.groupBox1.Controls.Add(this.showSelectJobsBTN);
        this.groupBox1.Controls.Add(this.showAllJobsBTN);
        this.groupBox1.Location = new System.Drawing.Point(13, 21);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(480, 138);
        this.groupBox1.TabIndex = 4;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Jobs to Show";
        // 
        // externalIdContainsTB
        // 
        this.externalIdContainsTB.Location = new System.Drawing.Point(53, 55);
        this.externalIdContainsTB.Name = "externalIdContainsTB";
        this.externalIdContainsTB.Size = new System.Drawing.Size(100, 20);
        this.externalIdContainsTB.TabIndex = 5;
        // 
        // showSelectJobsBTN
        // 
        this.showSelectJobsBTN.Checked = true;
        this.showSelectJobsBTN.Location = new System.Drawing.Point(20, 28);
        this.showSelectJobsBTN.Name = "showSelectJobsBTN";
        this.showSelectJobsBTN.Size = new System.Drawing.Size(373, 21);
        this.showSelectJobsBTN.TabIndex = 4;
        this.showSelectJobsBTN.TabStop = true;
        this.showSelectJobsBTN.Text = "Show all Jobs whose External Id contains this text (case insensitive):";
        // 
        // showAllJobsBTN
        // 
        this.showAllJobsBTN.Location = new System.Drawing.Point(20, 90);
        this.showAllJobsBTN.Name = "showAllJobsBTN";
        this.showAllJobsBTN.Size = new System.Drawing.Size(373, 21);
        this.showAllJobsBTN.TabIndex = 4;
        this.showAllJobsBTN.Text = "Show all Jobs";
        // 
        // WhichJobsForm
        // 
        this.AcceptButton = this.okBTN;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.CancelButton = this.cancelBTN;
        this.ClientSize = new System.Drawing.Size(608, 248);
        this.Controls.Add(this.groupBox1);
        this.Controls.Add(this.okBTN);
        this.Controls.Add(this.cancelBTN);
        this.Name = "WhichJobsForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Which Jobs Form";
        this.groupBox1.ResumeLayout(false);
        this.groupBox1.PerformLayout();
        this.ResumeLayout(false);
    }
    #endregion

    private void okBTN_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void cancelBTN_Click(object sender, EventArgs e)
    {
        Close();
    }

    public bool ShowAllJobs => showAllJobsBTN.Checked;

    public string ExternalIdContains => externalIdContainsTB.Text;
}