namespace PT.PackageDefinitions;

public class PackageEnums
{
    public enum EBlockEligibilityIndicatorTypes
    {
        /// <summary>
        /// Eligible based on calculated resource capability requirements met
        /// </summary>
        ResRequirement,
        /// <summary>
        /// Eligible based on alternate path criteria met
        /// </summary>
        PathRequirement
    }
    public enum EBlockEligibilityTypes
    {
        NotEligible,
        Eligible,
        NonHelperEligible,
        DefaultEligible,
        NotCalculated,
        SamePath,
        DifferentPath,
        SameAndDifferentPath,
        EligibleForAutoSplit
    }

    public enum ESplashScreenCommand { Description, WarningText, LocalizeImage, Brand }

    /// <summary>
    /// Whether the settings control is being used in the system settings control pane, the user settings control pane, or if it is part of a drop down container
    /// on the UI Ribbon Toolbar
    /// </summary>
    public enum EOptimizeAndCompressSettingsTypeLoader
    {
        /// <summary>
        /// The controls in in the system settings pane, uses scenario details settings
        /// </summary>
        System,

        /// <summary>
        /// The control is in the user settings pane, uses user settings
        /// </summary>
        User,

        /// <summary>
        /// The control is in the the UI Toolbar. User preferences determines whether to use scenario detail settings or user settings
        /// </summary>
        Active
    }

    /// <summary>
    /// Describes the type of expedite to perform
    /// </summary>
    public enum EJobExpediteType
    {
        /// <summary>
        /// Expedite to a date as soon as possible
        /// </summary>
        ASAP,

        /// <summary>
        /// Expedite past the frozen span
        /// </summary>
        PreserveFrozenSpan,

        /// <summary>
        /// Expedite past the stable span
        /// </summary>
        PreserveStableSpan,

        /// <summary>
        /// Expedite to the JIT start date
        /// </summary>
        JIT,

        /// <summary>
        /// Expedite to a specified date
        /// </summary>
        ToDate
    }

    public enum EBoardTabsMode
    {
        /// <summary>
        /// Never show board tabs
        /// </summary>
        None,

        /// <summary>
        /// Always show board tabs
        /// </summary>
        Show,

        /// <summary>
        /// Show board tabs only if more than one board is open in a group
        /// </summary>
        Smart
    }

    public enum ENotificationUrgency
    {
        None,
        Minor,
        Info,
        Alert,
        Critical
    }

    public enum EBarMenuSortByType { None, Priority, Alphabetical }

    public enum ELogTypes
    {
        Interface,
        System,
        External,
        Fatal,
        User,
        Misc,
        SchedulingWarnings,
        Notifications
    }

    public enum ESequencingFactorCalculationDependency
    {
        /// <summary>
        /// Scores are calculated once per simulation
        /// </summary>
        Constant,

        /// <summary>
        /// Scores are based on Activity values and are independent of the Resource itself
        /// </summary>
        TimeBased,

        /// <summary>
        /// Scores for each Activity are affected by what Resource they are scheduling on
        /// </summary>
        ResourceBased
    }

    public enum ESequencingFactorComplexity
    {
        Simple,
        Moderate,
        Complex
    }

    public enum ESequencingFactorEarlyWindowPenaltyScale
    {
        /// <summary>
        /// Don't incur any penalty
        /// </summary>
        Disabled,

        /// <summary>
        /// The penalty incurred on a sequencing factor declines linearly as its release approaches the end of the early window.
        /// </summary>
        Linear,

        /// <summary>
        /// The penalty incurred on a sequencing factor declines logarithmically as its release approaches the end of the early window.
        /// </summary>
        Logarithmic,

        /// <summary>
        /// The penalty incurred on a sequencing factor declines exponentially as its release approaches the end of the early window.
        /// </summary>
        Exponential
    }
}