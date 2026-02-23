using DevExpress.Utils.Svg;

using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI.Interfaces;

public interface IBrand
{
    /// <summary>
    /// This is displayed in the user interface.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Abstract function to get icon based on branding
    /// </summary>
    System.Drawing.Icon Icon { get; }

    SvgImage LogoSmall { get; }
    SvgImage LogoLarge { get; }

    /// <summary>
    /// List of allowed themes
    /// </summary>
    List<string> AllowedThemes { get; }

    /// <summary>
    /// This is displayed in the user interface.
    /// </summary>
    string LongName { get; }

    string SupportEmailAddress { get; }

    string HomeWebSiteAddress { get; }

    string SupportWebSiteAddress { get; }

    string MaintenanceInfoWebAddress { get; }

    string SolutionOverviewWebAddress { get; }

    string LicensesWebAddress { get; }

    string CustomProgrammingWebAddress { get; }

    string CrystalReportsRuntimeWebAddress { get; }

    string ReferralProgramWebAddress { get; }

    Type GetSplashControlType();

    // The PTSplashScreen displays the instance name, but if the splash
    // control doesn't display it, it can just be ignored.
    ISplashControlInterface GetSplashControl(string a_instanceName = "");

    /// <summary>
    /// Active theme
    /// </summary>
    IDynamicSkin ActiveTheme { get; }

    /// <summary>
    /// Call this to dispose the Icon when MainForm is closing
    /// </summary>
    void Dispose();
}