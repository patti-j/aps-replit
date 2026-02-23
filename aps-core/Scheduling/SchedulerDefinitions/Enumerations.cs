namespace PT.SchedulerDefinitions;

/// <summary>
/// Different key types.
/// </summary>
public enum SystemIdTypes
{
    /// <summary>
    /// Checks the key against CpuId of the computer running the Instance.
    /// </summary>
    CpuId = 0,

    /// <summary>
    /// Checks the key against Windows product Id. This type of key is no longer issued.
    /// </summary>
    WindowsProductId = 1,

    /// <summary>
    /// Uses license service. It locks and Pings the license service using the unique serialcode provided.
    /// </summary>
    LicenseService = 2
}

/// <summary>
/// Editions of the license purchased. Each edition epecifies which options are on/off by default but can be overriden for each key
/// </summary>
public enum LicenseEdition
{
    /// <summary>
    /// An entry level tool for small to mid-sized companies
    /// </summary>
    Essentials,

    /// <summary>
    /// A powerful optimization engine for complex businesses
    /// </summary>
    Pro,

    /// <summary>
    /// A platform to standardize planning and optimization across plants and warehouses to achieve SuperPlant™ performance
    /// </summary>
    Enterprise,

    /// <summary>
    /// Small business focused product. No erp integrations provided. A manual entry and maintained system.
    /// </summary>
    Express
}

/// <summary>
/// How frequently checksum transmission are sent.
/// </summary>
public enum ChecksumFrequencyType
{
    /// <summary>
    /// Whenever ScenarioUndoCheckpoint has been sent. This depends on settings
    /// defined in SystemOptions
    /// </summary>
    Regular, //corresponds to disabled additional ChecksumDiagnostics setting on the server/instance  

    /// <summary>
    /// After every Simulation.
    /// </summary>
    ScheduleAdjustment, //Correspond to additional ChecksumDiagnostics setting on the server/instance
    /// <summary>
    /// 
    /// </summary>
    Diagnostics
}

/// <summary>
/// Reasons for why a RuleSeek Simulation can terminate. Used by the client to report status of the simulations.
/// </summary>
public enum RuleSeekEndReasons
{
    /// <summary>
    /// Incomming ERP transmission
    /// </summary>
    ERP,

    /// <summary>
    /// Settings were changed and the feature was Enabled
    /// </summary>
    Enabled,

    /// <summary>
    /// Settings were changed and the feature was dissabled
    /// </summary>
    Dissabled,

    /// <summary>
    /// An error has occured
    /// </summary>
    Error,

    /// <summary>
    /// A data changing scenario action was taken (such as move, job change, etc)
    /// </summary>
    ScenarioAction,

    /// <summary>
    /// Clock was advanced
    /// </summary>
    ClockAdvance,

    /// <summary>
    /// Live Scenario was changed
    /// </summary>
    LiveScenarioChanged,

    /// <summary>
    /// The System is starting.
    /// </summary>
    Startup,

    /// <summary>
    /// Copilot Settings were updated while the System was live. This excludes enabling/disabling, which is captured above.
    /// </summary>
    CopilotSettingsChanged
}

public enum ScenarioTypes
{
    Whatif = 0,
    [Obsolete("Replaced with ScenarioPlanningSettings.Production property. Enum maintained for other values (ie 'special scenario types') still in use. " +
              "Scenario.Type prop will treat Production scenarios as Live to maintain existing behavior. ")]
    Live,
    Published,
    RuleSeek,
    InsertJobs,
    Game,
    ShortTerm,
    Pruned
}

/// <summary>
/// Simulation statusses for CoPilot Simulations
/// </summary>
public enum CoPilotSimulationStatus
{
    /// <summary>
    /// The default state for when the simulations are not running.
    /// </summary>
    STOPPED,

    /// <summary>
    /// The simulations are starting up and running pre simulation initialization.
    /// </summary>
    INITIALIZING,

    /// <summary>
    /// The simulations are running
    /// </summary>
    RUNNING,

    /// <summary>
    /// The simulation is not running, and stopped due to an error.
    /// </summary>
    ERROR,

    /// <summary>
    /// The simulation has completed successfully.
    /// </summary>
    COMPLETED,

    /// <summary>
    /// The simulation was canceled either by user or system.
    /// </summary>
    CANCELED
}

/// <summary>
/// Simulation types for InsertJobs
/// </summary>
public enum InsertJobsSimulationTypes
{
    /// <summary>
    /// The simulation will insert all New jobs.
    /// </summary>
    StandardInsert,

    /// <summary>
    /// The simulation will insert jobs one at a time from a list
    /// </summary>
    InsertFromList,

    /// <summary>
    /// The simulation will insert the list of jobs as a group.
    /// </summary>
    InsertGroup
}

public enum TransmissionReceptionType { PerformAction, WaitForResult, Auto }

public enum MessageSeverity { Information, Warning, Critical }

public enum EExportDestinations
{
    None = 0,
    ToDatabase = 1,
    ToXML = 2,
    Custom = 3,
    Analytics = 4,
    BasedOnSystemOptions = 5 // This means we check for all and send if the system option is set
}

public enum EDispatcherOwner
{
    NotSet = 0, // If the Dispatcher's m_owner is set to this, then the dispatcher was created for backwards compatibility to maintain serialization,
    // and it should just be garbage collected after initialization. 
    ScenarioManager = 1,
    Scenario = 2
}