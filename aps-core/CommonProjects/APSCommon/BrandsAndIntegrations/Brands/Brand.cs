namespace PT.APSCommon.Brands;

/// <summary>
/// Base Class fr all Brands.
/// </summary>
public abstract class Brand
{
    public enum EBrandCode { PlanetTogether, Zemeter, ProductionOne, DemandSolutions }

    /// <summary>
    /// Abstract function to get icon based on branding
    /// </summary>
    public abstract System.Drawing.Icon GetBrandIcon();

    /// <summary>
    /// This is displayed in the user interface.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// This is displayed in the user interface.
    /// </summary>
    public abstract string LongName { get; }

    public abstract string SupportEmailAddress { get; }

    public abstract string HomeWebSiteAddress { get; }

    public abstract string SupportWebSiteAddress { get; }

    public abstract string MaintenanceInfoWebAddress { get; }

    public abstract string SolutionOverviewWebAddress { get; }

    public abstract string LicensesWebAddress { get; }

    public abstract string CustomProgrammingWebAddress { get; }

    public abstract string CrystalReportsRuntimeWebAddress { get; }

    public abstract string ReferralProgramWebAddress { get; }

    public abstract string OptionsIncludeAddinsUrl { get; }
    public abstract string OptionsIncludeKPIsUrl { get; }
    public abstract string OptionsIncludeOptimizerUrl { get; }
    public abstract string OptionsIncludeBottleneckSchedulingUrl { get; }
    public abstract string OptionsIncludeExpressMRPUrl { get; }
    public abstract string OptionsIncludeBufferManagementUrl { get; }
    public abstract string OptionsIncludeCoPilotUrl { get; }

    //HomeScreen
    public abstract string HomeScreenHomePageUrl { get; }
    public abstract string HomeScreenLearningPageUrl { get; }
    public abstract string HomeScreenServicesPageUrl { get; }
    public abstract string HomeScreenSupportPageUrl { get; }
    public abstract string HomeScreenDownloadPageUrl { get; }
    public abstract string HomeScreenKnowledgePageUrl { get; }

    /// <summary>
    /// The name of the folder in the UI project where the Brand files are stored.  Must match the actual project folder names.
    /// </summary>
    public abstract string BrandFolderName { get; }

    //Brands -- these must match what's specified in the XML file for BrandName.  This is not necessarily what's displayed in the UI.

    //public const string APSBrand="Standard";
    public static readonly string ZemeterBrand = "Zemeter";

    //public const string MAS500Brand = "MAS 500";
    //public const string O2Brand = "O2";
    public static readonly string ProductionOneBrand = "ProductionOne";

    //public const string DynamicsNAVBrand = "Dynamics NAV";
    //public const string DynamicsGPBrand = "Dynamics GP";
    public static readonly string DemandSolutionsBrand = "Demand Solutions";
}