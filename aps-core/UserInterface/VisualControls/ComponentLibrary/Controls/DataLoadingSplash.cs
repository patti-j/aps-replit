using System.Windows.Forms;

using PT.APSCommon.Extensions;

namespace PT.ComponentLibrary.Controls;

/// <summary>
/// This class is used as a placeholder while a UI control is loading in the background.
/// It displays an animated loading image and loading text.
/// Usage: Create a new instance of this control. Add it to the parent object's controls. Set the DockStyle to Fill.
/// When the background control is done loading, dispose of this control.
/// </summary>
public partial class DataLoadingSplash : UserControl
{
    private static readonly string s_loadingText = "Loading Data...".Localize();
    private static readonly string s_connectingText = "Connecting...".Localize();

    public DataLoadingSplash()
    {
        InitializeComponent();
        pictureBox_LoadingAnimation.Image = APSCommon.Properties.Resources.LoadCircle;
        ultraLabel_LoadingText.Text = s_loadingText;
        ultraButton_Cancel.Visible = false;
    }

    public bool AllowCancel
    {
        set
        {
            ultraButton_Cancel.Visible = value;
            Refresh();
        }
    }

    /// <summary>
    /// Change the loading text
    /// </summary>
    /// <param name="a_loadingMessage"></param>
    public void SetLoadingText(string a_loadingMessage)
    {
        ultraLabel_LoadingText.Text = a_loadingMessage.Localize();
    }

    private void ultraButton_Cancel_Click(object sender, EventArgs e)
    {
        ultraButton_Cancel.Visible = false;
        SetLoadingText("Canceling...");
        Canceled?.Invoke();
    }

    public delegate void CanceledDelegate();

    public event CanceledDelegate Canceled;

    public void UpdateProgress(int a_percentComplete)
    {
        if (a_percentComplete <= 0)
        {
            ultraLabel_LoadingText.Text = s_connectingText;
        }
        else
        {
            ultraLabel_LoadingText.Text = s_loadingText + $" {a_percentComplete} %";
        }
    }
}