using System.Text.RegularExpressions;
using System.Windows.Forms;

using PT.ERPTransmissions;
using PT.Transmissions;

namespace TransmissionViewer;

/// <summary>
/// Summary description for OpenStartupForm.
/// </summary>
public class OpenStartupForm : Form
{
    private MenuStrip mainMenu1;
    private ToolStripMenuItem menuItem1;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private readonly System.ComponentModel.Container components = null;

    public OpenStartupForm()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();

//			this.defaultDirectory=Application.StartupPath;
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

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.mainMenu1 = new System.Windows.Forms.MenuStrip();
        this.menuItem1 = new System.Windows.Forms.ToolStripMenuItem();
        // 
        // mainMenu1
        // 
        this.mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] { this.menuItem1 });
        // 
        // menuItem1
        // 
        this.menuItem1.Text = "Open Transmission Files...";
        this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
        // 
        // OpenStartupForm
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
        this.ClientSize = new System.Drawing.Size(432, 134);
        //this.Menu = this.mainMenu1;
        this.Name = "OpenStartupForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "APS Transmission Viewer";
    }
    #endregion

    private string defaultDirectory = "C:\\_PlanetTogether\\Data\\ServiceData\\Data\\Recordings";

    private void menuItem1_Click(object sender, EventArgs e)
    {
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            using (OpenFileDialog dlg = new ())
            {
                dlg.Filter = "Transmission Files|*.bin";
                dlg.InitialDirectory = defaultDirectory;
                dlg.Multiselect = true;
                dlg.Title = "Select Transmission Files to View";
                DialogResult result = dlg.ShowDialog();
                if (result != DialogResult.Cancel)
                {
                    if (dlg.FileNames.Length > 0)
                    {
                        defaultDirectory = Path.GetDirectoryName(dlg.FileNames[0]);
                        ShowMultipleFiles(dlg.FileNames);
                    }
                }
            }
        }
        catch (Exception err)
        {
            Cursor.Current = Cursors.Default;
            MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        Cursor.Current = Cursors.Default;
    }

    private const string header = "\n\r************************* NEXT TRANSMISSION *****************************";

    private void ShowMultipleFiles(string[] filenames)
    {
        System.Text.StringBuilder builder = new ();
        for (int i = 0; i < filenames.Length; i++)
        {
            string fileName = filenames[i];
            TransmissionReader tr = new (fileName);
            builder.Append(header);
            Transmission t = tr.GetTransmission();
            if (t is JobT)
            {
                using (WhichJobsForm dlg = new ())
                {
                    dlg.ShowDialog();
                    if (dlg.DialogResult == DialogResult.OK)
                    {
                        if (dlg.ShowAllJobs)
                        {
                            builder.Append(GetAllJobsText((JobT)t));
                        }
                        else
                        {
                            builder.Append(GetJobsText((JobT)t, dlg.ExternalIdContains));
                        }
                    }
                }
            }
            else
            {
                builder.Append(tr.GetText());
            }
        }

        MultiViewerForm mvf = new ();
        mvf.Populate(builder.ToString());
        mvf.Show();
    }

    private string GetJobsText(JobT t, string externalIdContains)
    {
        System.Text.StringBuilder builder = new ();
        for (int i = 0; i < t.Count; i++)
        {
            JobT.Job job = t[i];
            if (Regex.IsMatch(job.ExternalId, externalIdContains, RegexOptions.IgnoreCase))
            {
                builder.Append(job);
            }
        }

        return builder.ToString();
    }

    private string GetAllJobsText(JobT t)
    {
        //TODO
        return "";
    }
}