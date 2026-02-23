using System.Windows.Forms;

using PT.APSCommon.Extensions;
using PT.Transmissions;

namespace TransmissionViewer;

/// <summary>
/// Summary description for Form1.
/// </summary>
public class Form1 : Form
{
    private OpenFileDialog openFileDialog1;
    private DevExpress.XtraTab.XtraTabControl tabControl;
    private DevExpress.XtraTab.XtraTabPage tabSummary;
    private PropertyGrid propertyGrid1;
    private DevExpress.XtraTab.XtraTabPage tabDetails;
    private RichTextBox detailsRTB;
    //private List<Transmission> m_transmissions;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private readonly System.ComponentModel.Container components = null;

    public Form1()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();

        //
        // TODO: Add any constructor code after InitializeComponent call
        //
    }

    public Form1(string[] args)
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();

        if (args.Length == 1)
        {
            string fileName = args[0];
            TransmissionReader tr = new (fileName);
            Transmission t = tr.GetTransmission();
            PopulateSingleTViewer(fileName, t, tr.GetText());
        }
        //else if (args.Length == 2)
        //{
        //    if (args[0] == "file")
        //    {
        //        string folderName = args[1];
        //        DirectoryInfo directory = new DirectoryInfo(folderName);
        //        FileInfo[] files = directory.GetFiles();
        //        TransmissionReader transmissionReader;
        //        List<Transmission> transmissions = new List<Transmission>();
        //        foreach (FileInfo file in files)
        //        {
        //            if (file.FullName.Contains("scenario"))
        //            {
        //                continue;
        //            }
        //            transmissionReader = new (file.FullName);
        //            Transmission transmission = transmissionReader.GetTransmission();
        //            if (transmission is UserErrorT userErrorT)
        //            {
        //                if (userErrorT.message != "Object reference not set to an instance of an object.")
        //                {
        //                    PopulateSingleTViewer(file.Name, transmission, transmissionReader.GetText());
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}
    }

    private void PopulateSingleTViewer(string filename, Transmission t, string transmissionText)
    {
        Text = string.Format("{0} ({1})", t.GetType().Name.Localize(), filename);
//			if(o is ScenarioDetailAnchorJobsT)
//			{
//				ScenarioDetailAnchorJobsT j=(ScenarioDetailAnchorJobsT)o;
//				PT.Scheduler.BaseIdList.Node node=j.Jobs.First;
//				System.Text.StringBuilder builder=new System.Text.StringBuilder();
//				while(node!=null)
//				{
//					builder.Append(String.Format("Job Id={0}",node.Data.ToString()));
//					node=node.Next;
//				}
//
//				PopulatePropertiesGrid(j);
//				PopulateRichTextBox(builder.ToString());
//			}
//			else

        PopulatePropertiesGrid(t);
        PopulateRichTextBox(transmissionText);
    }

    private void PopulatePropertiesGrid(object o)
    {
        propertyGrid1.SelectedObject = o;
    }

    private void PopulateRichTextBox(string text)
    {
        detailsRTB.Text = text;
    }

    #region Disposal
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
        this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
        this.tabControl = new DevExpress.XtraTab.XtraTabControl();
        this.tabSummary = new DevExpress.XtraTab.XtraTabPage();
        this.tabDetails = new DevExpress.XtraTab.XtraTabPage();
        this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
        this.detailsRTB = new System.Windows.Forms.RichTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.tabControl)).BeginInit();
        this.tabControl.SuspendLayout();
        this.tabSummary.SuspendLayout();
        this.tabDetails.SuspendLayout();
        this.SuspendLayout();
        // 
        // openFileDialog1
        // 
        this.openFileDialog1.Filter = "\"Transmissions|*.bin\"";
        // 
        // tabControl
        // 
        this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabControl.Location = new System.Drawing.Point(0, 0);
        this.tabControl.Name = "tabControl";
        this.tabControl.SelectedTabPage = this.tabSummary;
        this.tabControl.Size = new System.Drawing.Size(688, 512);
        this.tabControl.TabIndex = 1;
        this.tabControl.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[]
        {
            this.tabSummary,
            this.tabDetails
        });
        // 
        // tabSummary
        // 
        this.tabSummary.Controls.Add(this.propertyGrid1);
        this.tabSummary.Name = "tabSummary";
        this.tabSummary.Size = new System.Drawing.Size(686, 487);
        this.tabSummary.Text = "Summary";
        // 
        // tabDetails
        // 
        this.tabDetails.Controls.Add(this.detailsRTB);
        this.tabDetails.Name = "tabDetails";
        this.tabDetails.Size = new System.Drawing.Size(686, 487);
        this.tabDetails.Text = "Details";
        // 
        // propertyGrid1
        // 
        this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.propertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
        this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
        this.propertyGrid1.Name = "propertyGrid1";
        this.propertyGrid1.Size = new System.Drawing.Size(686, 487);
        this.propertyGrid1.TabIndex = 2;
        // 
        // detailsRTB
        // 
        this.detailsRTB.Dock = System.Windows.Forms.DockStyle.Fill;
        this.detailsRTB.Location = new System.Drawing.Point(0, 0);
        this.detailsRTB.Name = "detailsRTB";
        this.detailsRTB.Size = new System.Drawing.Size(686, 487);
        this.detailsRTB.TabIndex = 1;
        this.detailsRTB.Text = "";
        // 
        // Form1
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(688, 512);
        this.Controls.Add(this.tabControl);
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        ((System.ComponentModel.ISupportInitialize)(this.tabControl)).EndInit();
        this.tabControl.ResumeLayout(false);
        this.tabSummary.ResumeLayout(false);
        this.tabDetails.ResumeLayout(false);
        this.ResumeLayout(false);
    }
    #endregion
    #endregion
}