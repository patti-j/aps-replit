namespace PT.Scheduler;

/// <summary>
/// These controls are used to monitor, flag and adjust incoming data to
/// prevent bad data from disrupting the schedule and to aid in the repair
/// of bad data at the source.
/// </summary>
public class DataInspectorControls
{
    #region Declarations
    public enum responses { Reject = 0, Flag, Adjust }

    public enum diagnoses { OK = 0, Suspect, AutoAdjusted }

    /// <summary>
    /// Property Categories for DataInspectorControls
    /// </summary>
    public class PropertyCategories
    {
        //Categories
        public const string _JOB = "Job";
        public const string _MO = "Maufacturing Order";
        public const string _OPERATION = "Operation";
        public const string _RESULTS = "Results";
    }
    #endregion Declarations

    #region Properties
    private TimeSpan maxCycleTime = new (1, 0, 0);

    /// <summary>
    /// Operation CycleTime must be no more than this value.
    /// </summary>
    public TimeSpan MaxCycleTime
    {
        get => maxCycleTime;
        set => maxCycleTime = value;
    }

    private decimal maxRequiredQty = 100000;

    /// <summary>
    /// MO RequiredQty must be no more than this value.
    /// </summary>
    public decimal MaxRequiredQty
    {
        get => maxRequiredQty;
        set => maxRequiredQty = value;
    }

    private TimeSpan maxSetupSpan = new (1, 0, 0);

    /// <summary>
    /// Operation SetupSpan must be no more than this value.
    /// </summary>
    public TimeSpan MaxSetupSpan
    {
        get => maxSetupSpan;
        set => maxSetupSpan = value;
    }

    private TimeSpan minCycleTime = new (0, 0, 0, 1);

    /// <summary>
    /// Operation CycleTime must be at least this value.
    /// </summary>
    public TimeSpan MinCycleTime
    {
        get => minCycleTime;
        set => minCycleTime = value;
    }

    private int minNbrMaterials;

    /// <summary>
    /// Each MO must have at least this many Material Requirements.
    /// </summary>
    public int MinNbrMaterials
    {
        get => minNbrMaterials;
        set => minNbrMaterials = value;
    }

    private int minNbrOperations = 1;

    /// <summary>
    /// Each MO must have at least this many Operations.
    /// </summary>
    public int MinNbrOperations
    {
        get => minNbrOperations;
        set => minNbrOperations = value;
    }

    private decimal minRequiredQty = 1;

    /// <summary>
    /// MO RequiredQty must be at least this value.
    /// </summary>
    public decimal MinRequiredQty
    {
        get => minRequiredQty;
        set => minRequiredQty = value;
    }

    private bool onlyLinearRoutings;

    /// <summary>
    /// If true, then an MO is suspect if it has a routing where any Operation has more than one successor or more than one predecessor.
    /// </summary>
    public bool OnlyLinearRoutings
    {
        get => onlyLinearRoutings;
        set => onlyLinearRoutings = value;
    }

    private responses response = responses.Flag;

    /// <summary>
    /// Specifies the action to take in response to finding suspect data.
    /// </summary>
    public responses Response
    {
        get => response;
        set => response = value;
    }
    #endregion
}