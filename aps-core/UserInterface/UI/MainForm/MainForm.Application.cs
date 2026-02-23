using System.Drawing;

using DevExpress.Utils;
using DevExpress.XtraEditors;

using PT.APSCommon.Packages;
using PT.APSCommon.Windows;

namespace PT.UI;

partial class MainForm
{
    public static Utilities.CommandLineArgumentsHelper CommandLineArguments { get; set; }

    public static void SetDefaultToolTipValues()
    {
        ToolTipController.DefaultController.AutoPopDelay = 12000;
        ToolTipController.DefaultController.InitialDelay = 650;
        ToolTipController.DefaultController.ToolTipType = ToolTipType.SuperTip;
    }

    public static void SetDefaultSkinInfo()
    {
        DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = "Modern";
        DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("Modern", "Modern Light");
        WindowsFormsSettings.CompactUIMode = DefaultBoolean.True;
        WindowsFormsSettings.DefaultFont = new Font("Verdana", 8.25f);
    }

    public static void RegisterImages()
    {
        PtImageCache.RegisterImage("splash-eap", PackageDefinitionsUI.Properties.Resources.PlanetTogetherSplashscreen);
        PtImageCache.RegisterImage("error", PackageDefinitionsUI.Properties.Resources.error);
        PtImageCache.RegisterImage("help", PackageDefinitionsUI.Properties.Resources.help);
        PtImageCache.RegisterImage("infoBubble", PackageDefinitionsUI.Properties.Resources.infoBubble);
        PtImageCache.RegisterImage("information", PackageDefinitionsUI.Properties.Resources.information);
        PtImageCache.RegisterImage("warningSign", PackageDefinitionsUI.Properties.Resources.warningSign);
    }

    /// <summary>
    /// Used to set AppModelId so that taskbar icons can be grouped
    /// </summary>
    //internal static class SafeNativeMethods
    //{
    //    [DllImport("shell32.dll")]
    //    public static extern int SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);
    //}

    /// <summary>
    /// Used to set AppModelId so that taskbar icons can be grouped
    /// </summary>
    //public static int SetApplicationUserModelId(string a_appId)
    //{
    //    // check for Windows 7
    //    Version version = Environment.OSVersion.Version;
    //    if ((version.Major > 6) || (version.Major == 6 && version.Minor >= 1))
    //    {
    //        return SafeNativeMethods.SetCurrentProcessExplicitAppUserModelID(a_appId);
    //    }
    //    return -1;
    //}
}