namespace TransmissionViewer;

/// <summary>
/// Summary description for MultiViewer.
/// </summary>
public class MultiViewerForm : System.Windows.Forms.Form
{
    private System.Windows.Forms.TextBox findTextBox;
    private System.Windows.Forms.Button findBTN;
    private System.Windows.Forms.RichTextBox richTextBox1;
    private System.Windows.Forms.Button findFromTop;
    private DevExpress.XtraEditors.LabelControl resultLabel;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private readonly System.ComponentModel.Container components = null;

    public MultiViewerForm()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();

        //
        // TODO: Add any constructor code after InitializeComponent call
        //
    }

    public void Populate(string displayText)
    {
        richTextBox1.Text = displayText;
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
        this.findTextBox = new System.Windows.Forms.TextBox();
        this.findBTN = new System.Windows.Forms.Button();
        this.richTextBox1 = new System.Windows.Forms.RichTextBox();
        this.findFromTop = new System.Windows.Forms.Button();
        this.resultLabel = new DevExpress.XtraEditors.LabelControl();
        this.SuspendLayout();
        // 
        // findTextBox
        // 
        this.findTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.findTextBox.Location = new System.Drawing.Point(7, 452);
        this.findTextBox.Name = "findTextBox";
        this.findTextBox.Size = new System.Drawing.Size(120, 20);
        this.findTextBox.TabIndex = 4;
        this.findTextBox.TextChanged += new System.EventHandler(this.findTextBox_TextChanged);
        // 
        // findBTN
        // 
        this.findBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.findBTN.Location = new System.Drawing.Point(133, 452);
        this.findBTN.Name = "findBTN";
        this.findBTN.Size = new System.Drawing.Size(63, 20);
        this.findBTN.TabIndex = 3;
        this.findBTN.Text = "Find &Next";
        this.findBTN.Click += new System.EventHandler(this.findBTN_Click);
        // 
        // richTextBox1
        // 
        this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.richTextBox1.Location = new System.Drawing.Point(7, 0);
        this.richTextBox1.Name = "richTextBox1";
        this.richTextBox1.Size = new System.Drawing.Size(634, 438);
        this.richTextBox1.TabIndex = 6;
        this.richTextBox1.Text = "";
        // 
        // findFromTop
        // 
        this.findFromTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.findFromTop.Location = new System.Drawing.Point(200, 452);
        this.findFromTop.Name = "findFromTop";
        this.findFromTop.Size = new System.Drawing.Size(93, 20);
        this.findFromTop.TabIndex = 3;
        this.findFromTop.Text = "Find From &Top";
        this.findFromTop.Click += new System.EventHandler(this.findFromTop_Click);
        // 
        // resultLabel
        // 
        this.resultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.resultLabel.Location = new System.Drawing.Point(307, 455);
        this.resultLabel.Name = "resultLabel";
        this.resultLabel.Size = new System.Drawing.Size(83, 14);
        this.resultLabel.TabIndex = 7;
        // 
        // MultiViewerForm
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(648, 480);
        this.Controls.Add(this.resultLabel);
        this.Controls.Add(this.richTextBox1);
        this.Controls.Add(this.findTextBox);
        this.Controls.Add(this.findBTN);
        this.Controls.Add(this.findFromTop);
        this.Name = "MultiViewerForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Transmission File Contents";
        this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        this.ResumeLayout(false);
        this.PerformLayout();
    }
    #endregion

    private int searchFrom;

    private void findBTN_Click(object sender, EventArgs e)
    {
        Find();
    }

    private void findTextBox_TextChanged(object sender, EventArgs e)
    {
        FindFromTop();
    }

    private void FindFromTop()
    {
        searchFrom = 0;
        Find();
    }

    private void Find()
    {
        string searchText = findTextBox.Text;
        if (searchText.Length > 0)
        {
            int findStart = -1;
            findStart = richTextBox1.Find(searchText, searchFrom, richTextBox1.Text.Length - 1, System.Windows.Forms.RichTextBoxFinds.None);
            if (findStart != -1)
            {
                resultLabel.Text = "";
                richTextBox1.Select(findStart, searchText.Length);
                richTextBox1.SelectionColor = System.Drawing.Color.Red;
                richTextBox1.HideSelection = false;
                searchFrom = findStart + 1;
            }
            else
            {
                resultLabel.Text = "(not found)";
            }
        }
        else
        {
            resultLabel.Text = "";
        }
    }

    private void findFromTop_Click(object sender, EventArgs e)
    {
        FindFromTop();
    }
}