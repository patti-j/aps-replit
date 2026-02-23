using System.Drawing;

namespace PT.APSCommon.Brands;

/// <summary>
/// Brand used when there is no special branding.
/// </summary>
public class PlanetTogetherBrand : Brand
{
    public override string Name => "Standard";

    /// <summary>
    /// Return branding icon for PlanetTogether
    /// </summary>
    /// <returns></returns>
    public override Icon GetBrandIcon()
    {
        return Properties.Resources.PT_icon_black;
    }

    /// <summary>
    /// This is displayed in the user interface.
    /// </summary>
    public override string LongName => "PlanetTogether APS";

    public override string SupportEmailAddress => "support@PlanetTogether.com";

    public override string HomeWebSiteAddress => "www.PlanetTogether.com";

    public override string SupportWebSiteAddress => "http://support.planettogether.com";

    public override string MaintenanceInfoWebAddress => SupportWebSiteAddress;

    public override string SolutionOverviewWebAddress => HomeWebSiteAddress;

    public override string LicensesWebAddress => "http://planettogether.com/contact/";

    public override string CustomProgrammingWebAddress => "http://home.planettogether.com/services/";

    public override string CrystalReportsRuntimeWebAddress => "https://wiki.scn.sap.com/wiki/pages/viewpage.action?original_fqdn=wiki.sdn.sap.com&pageId=56787567";

    public override string ReferralProgramWebAddress => "http://home.planettogether.com/partners/";

    public override string OptionsIncludeAddinsUrl => SolutionOverviewWebAddress;

    public override string OptionsIncludeKPIsUrl => SolutionOverviewWebAddress;
    public override string OptionsIncludeOptimizerUrl => SolutionOverviewWebAddress;

    public override string OptionsIncludeBottleneckSchedulingUrl => SolutionOverviewWebAddress;
    public override string OptionsIncludeExpressMRPUrl => SolutionOverviewWebAddress;

    public override string OptionsIncludeBufferManagementUrl => SolutionOverviewWebAddress;

    public override string OptionsIncludeCoPilotUrl => SolutionOverviewWebAddress;

    public override string HomeScreenHomePageUrl => "http://home.planettogether.com/";

    public override string HomeScreenLearningPageUrl => HomeScreenHomePageUrl + "Learning";

    public override string HomeScreenServicesPageUrl => CustomProgrammingWebAddress;

    public override string HomeScreenSupportPageUrl => SupportWebSiteAddress;

    public override string HomeScreenDownloadPageUrl => HomeScreenHomePageUrl + "Download";

    public override string HomeScreenKnowledgePageUrl => HomeScreenHomePageUrl + "Knowledge";

    /// <summary>
    /// The name of the folder in the UI project where the Brand files are stored.  Must match the actual project folder names.
    /// </summary>
    public override string BrandFolderName => "Standard";
}