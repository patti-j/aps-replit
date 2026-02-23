using System.Windows.Forms;

namespace PT.ComponentLibrary.Controls;

/// <summary>
/// This class is used as a placeholder while a UI control is loading in the background.
/// It displays an animated loading image and loading text.
/// Usage: Create a new instance of this control. Add it to the parent object's controls. Set the DockStyle to Fill.
/// When the background control is done loading, dispose of this control.
/// </summary>
public partial class DataLoadingSplashLite : UserControl
{
    public DataLoadingSplashLite()
    {
        InitializeComponent();
        pictureBox_LoadingAnimation.Image = APSCommon.Properties.Resources.LoadCircle;
    }
}