using System.Windows.Forms;

using PT.APSCommon.Extensions;
using PT.APSCommon.Packages;
using PT.APSCommon.Windows;

namespace PT.ComponentLibrary.Controls;

public partial class NoDataSplash : UserControl
{
    public NoDataSplash()
    {
        InitializeComponent();

        pictureBox_NoDataImage.SvgImage = PtImageCache.GetImage("information");

        UILocalizationHelper.LocalizeControlsRecursively(Controls);
    }

    public void OverrideText(string a_text)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => SetTextOverride(a_text)));
        }
        else
        {
            SetTextOverride(a_text);
        }
    }

    public void SetLabelObjectText(string a_text)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => SetLabelObjectTextHelper(a_text)));
        }
        else
        {
            SetLabelObjectTextHelper(a_text);
        }
    }

    private void SetTextOverride(string a_text)
    {
        label_Text.Text = a_text.Localize();
    }

    private void SetLabelObjectTextHelper(string a_text)
    {
        label_Text.Text = string.Format("This Control Requires {0} Data In Order To Function".Localize(), a_text);
    }

    public void ShowSplash()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(ShowSplashHelper));
        }
        else
        {
            ShowSplashHelper();
        }
    }

    private void ShowSplashHelper()
    {
        Visible = true;
    }

    public void HideSplash()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(HideSplashHelper));
        }
        else
        {
            HideSplashHelper();
        }
    }

    private void HideSplashHelper()
    {
        Visible = false;
    }

    public void RefreshSplash()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(Refresh));
        }
        else
        {
            Refresh();
        }
    }

    public void SetMessage(string a_overrideMessage)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => { label_Text.Text = a_overrideMessage; }));
        }
        else
        {
            label_Text.Text = a_overrideMessage;
        }
    }
}