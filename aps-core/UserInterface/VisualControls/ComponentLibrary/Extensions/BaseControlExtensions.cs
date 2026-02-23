using System.Drawing;

using DevExpress.Utils;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ButtonPanel;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;

using PT.APSCommon.Extensions;
using PT.APSCommon.Packages;
using PT.APSCommon.Windows;

using ImageLocation = DevExpress.XtraEditors.ImageLocation;

namespace PT.ComponentLibrary.Extensions;

public static class BaseControlExtensions
{
    #region Simple Button Extensions
    public static void SetImage(this SimpleButton a_button, string a_imageKey)
    {
        a_button.ImageOptions.SvgImage = PtImageCache.GetImage(a_imageKey);
        a_button.AllowFocus = false;
        a_button.ShowFocusRectangle = DefaultBoolean.False;

        if (a_button.ImageOptions.SvgImageSize == Size.Empty)
        {
            int topPadding;
            int bottomPadding;
            int leftPadding;
            int rightPadding;
            //Find padding
            if (a_button.Padding.All == 0)
            {
                //Set default to 10%
                topPadding = bottomPadding = leftPadding = rightPadding = Convert.ToInt32(Math.Ceiling(a_button.Height * .1));
            }
            else
            {
                topPadding = a_button.Padding.Top * 2;
                bottomPadding = a_button.Padding.Bottom * 2;
                leftPadding = a_button.Padding.Left * 2;
                rightPadding = a_button.Padding.Right * 2;
            }

            int height = a_button.Size.Height - topPadding - bottomPadding;
            int width = a_button.Size.Width - leftPadding - rightPadding;
            if (height > 0 && width > 0)
            {
                if (height > width)
                {
                    a_button.ImageOptions.SvgImageSize = new Size(width, width); //keep square
                }
                else
                {
                    a_button.ImageOptions.SvgImageSize = new Size(height, height); //keep square
                }
            }
        }
    }

    public static void SetToolTip(this SimpleButton a_button, string a_title, string a_content)
    {
        a_button.ResetSuperTip();

        SuperToolTip superTip = new ();
        superTip.Items.AddTitle(a_title);
        superTip.Items.Add(a_content);

        a_button.SuperTip = superTip;
    }

    public static void SetToolTip(this EditorButton a_button, string a_title, string a_content)
    {
        a_button.ResetSuperTip();

        SuperToolTip superTip = new ();
        superTip.Items.AddTitle(a_title);
        superTip.Items.Add(a_content);

        a_button.SuperTip = superTip;
    }

    public static bool ContainsName(this XtraTabPageCollection a_tabCollection, string a_tabName, out XtraTabPage o_tabPage)
    {
        o_tabPage = null;

        foreach (XtraTabPage page in a_tabCollection)
        {
            if (page.Name == a_tabName)
            {
                o_tabPage = page;
                break;
            }
        }

        return o_tabPage != null;
    }

    public enum EDialogButtonAction
    {
        SaveAndClose,
        Confirm,
        Cancel,
        Revert,
        Save
    }

    /// <summary>
    /// Use a preset styling for dialog buttons
    /// </summary>
    /// <param name="a_button"></param>
    /// <param name="a_buttonType"></param>
    public static void SetPresetDialogButtonStyling(this SimpleButton a_button, EDialogButtonAction a_buttonType)
    {
        a_button.ResetSuperTip();
        string title;
        string content;

        a_button.ImageOptions.Location = ImageLocation.MiddleCenter;
        a_button.Text = string.Empty;

        switch (a_buttonType)
        {
            case EDialogButtonAction.Save:
                a_button.SetImage("accept");
                title = "Save".Localize();
                content = "Click to save".Localize();
                break;
            case EDialogButtonAction.SaveAndClose:
                a_button.SetImage("accept");
                title = "Save".Localize();
                content = "Click to save and close".Localize();
                break;
            case EDialogButtonAction.Confirm:
                a_button.SetImage("accept");
                title = "Confirm".Localize();
                content = "Click to confirm".Localize();
                break;
            case EDialogButtonAction.Cancel:
                a_button.SetImage("close");
                title = "Cancel".Localize();
                content = "Click to cancel".Localize();
                break;
            case EDialogButtonAction.Revert:
                a_button.SetImage("revert");
                title = "Revert".Localize();
                content = "Click to revert".Localize();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(a_buttonType), a_buttonType, null);
        }

        SuperToolTip superTip = new ();
        superTip.Items.AddTitle(title);
        superTip.Items.Add(content);

        a_button.SuperTip = superTip;
    }
    #endregion

    public static void SetImage(this CustomHeaderButton a_button, string a_imageKey)
    {
        a_button.ImageOptions.SvgImage = PtImageCache.GetImage(a_imageKey);
    }

    public static void SetHelpButton(this GroupControl a_groupControl, string a_tooltipTile, string a_tooltipText)
    {
        a_groupControl.CustomHeaderButtons.BeginUpdate();
        a_groupControl.CustomHeaderButtonsLocation = GroupElementLocation.AfterText;

        CustomHeaderButton helpButton = new ();
        helpButton.UseCaption = false;
        helpButton.ImageOptions.SvgImage = PtImageCache.GetImage("help");
        helpButton.ImageOptions.SvgImageSize = new Size(16, 16);

        SuperToolTip rangesTip = new ();
        rangesTip.Items.AddTitle(a_tooltipTile.Localize());
        rangesTip.Items.Add(a_tooltipText.Localize());
        helpButton.SuperTip = rangesTip;

        a_groupControl.CustomHeaderButtons.Add(helpButton);
        a_groupControl.CustomHeaderButtons.EndUpdate();
    }

    public static void UpdateSettingsGroupBoxIcon(this GroupControl a_groupControl, string a_settingsImageKey, string a_toolTipTitle, string a_toolTipContent)
    {
        a_groupControl.CustomHeaderButtons.BeginUpdate();
        IBaseButton customButton = a_groupControl.CustomHeaderButtons[0];
        customButton.Properties.ImageOptions.SvgImage = PtImageCache.GetImage(a_settingsImageKey);

        SuperToolTip superTip = new ();
        superTip.Items.AddTitle(a_toolTipTitle);
        superTip.Items.Add(a_toolTipContent);
        customButton.Properties.SuperTip = superTip;
        a_groupControl.CustomHeaderButtons.EndUpdate();
    }
}