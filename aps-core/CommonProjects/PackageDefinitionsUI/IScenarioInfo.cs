using System.Windows.Forms;

using PT.APSCommon;
using PT.Common.Delegates;
using PT.PackageDefinitions;
using PT.PackageDefinitions.Settings;
using PT.PackageDefinitionsUI.DataSources;
using PT.Scheduler;
using PT.Scheduler.TransmissionDispatchingAndReception;
using PT.SchedulerData;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.Transmissions;
using PT.UIDefinitions.Interfaces;

namespace PT.PackageDefinitionsUI;

public interface IScenarioInfo : ICompareScenarioInfo, IUserPermissionsManager, IScenarioUndoInfo
{
    /// <summary>
    /// Marks the beginning of a simulation
    /// </summary>
    event Action<ScenarioDetail.SimulationType, ScenarioBaseT, BaseId, DateTime> SimulationStart;

    /// <summary>
    /// Marks the end of a simulation
    /// </summary>
    event Action SimulationComplete;

    /// <summary>
    /// Occurs when a simulation action is cancelled
    /// <para>But fired before the schedule is restored/action is undone </para>
    /// </summary>
    event Action SimulationBeginCancellation;
    /// <summary>
    /// Occurs when a simulation action is cancelled
    /// <para>But fired after the schedule is restored or action is undone </para>
    /// </summary>
    event Action SimulationCancelled;

    /// <summary>
    /// Marks a simulation milestone
    /// </summary>
    event Action<SimulationProgress.Status, decimal> SimulationProgress;

    /// <summary>
    /// Marks the change of the Clock Date
    /// </summary>
    event Action<DateTime> ClockDateChanged;

    /// <summary>
    /// Marks the start of a Scenario Data Publish
    /// </summary>
    event Action<ScenarioBaseT, BaseId, DateTime> PublishStart;

    /// <summary>
    /// Marks the completions of a Scenario Data Publish
    /// </summary>
    event Action<DateTime> PublishComplete;

    /// <summary>
    /// Marks the completions of a Move simulation
    /// </summary>
    event Action<object> MoveComplete;

    /// <summary>
    /// When Scenario Data has changed
    /// </summary>
    event Action<IScenarioDataChanges> DataChanged;

    /// <summary>
    /// When Scenario level settings have changed
    /// </summary>
    event Action<ISettingsManager, string, BaseId> ScenarioSettingChanged;

    /// <summary>
    /// When a transmission has been processed
    /// </summary>
    event Action<PTTransmission, TimeSpan, bool, Exception> TransmissionProcessed;

    /// <summary>
    /// When a transmission has been received
    /// </summary>
    event Action<PTTransmission, List<QueuedTransmissionData>> TransmissionReceived;

    /// <summary>
    /// When a Scenario Detail transmission has been processed
    /// </summary>
    event Action<PTTransmission> ScenarioDetailTransmissionProcessed;

    /// <summary>
    /// The active scenario has been switched. Data should be reloaded using the new active scenario id
    /// </summary>
    event Action<Scenario, ScenarioEvents> ScenarioClosed;

    /// <summary>
    /// When a Scenario has been activated
    /// </summary>
    event Action<Scenario, ScenarioDetail, ScenarioEvents> ScenarioActivated;

    /// <summary>
    /// When the Scenario Data has been locked
    /// </summary>
    event Action ScenarioDataLocked;

    /// <summary>
    /// When the Scenario Data becomes available again
    /// </summary>
    event Action ScenarioDataUnlocked;

    /// <summary>
    /// When the Scenario enters or exits a Read-only state
    /// </summary>
    event Action<bool> ScenarioDataReadonlyChanged;

    /// <summary>
    /// When a CTP action has occurred
    /// </summary>
    event Action<ScenarioBaseT, Transmissions.CTP.Ctp, Job, Exception> CtpEvent;

    /// <summary>
    /// When a UDF Definition has been added, updated, or deleted
    /// </summary>
    event Action<UserField.EUDFObjectType> UDFDataChangesEvent;
    
    /// <summary>
    /// This event is raised in the event that a checksum fails to validate that the client
    /// and server are in sync. This prompts the client to restart.
    /// </summary>
    event Action<Guid, string, BaseId> ScenarioDesynced;

    /// <summary>
    /// This event is raised when a scenario is being promoted to a Production Scenario
    /// <para>Parameter 1: Production Scenario Id</para>
    /// <para>Parameter 2: Promoted Scenario Id</para>
    /// </summary>
    event Action<BaseId, BaseId> BeginScenarioConversion;
    /// <summary>
    /// This event is raised when the Server is done promoting a scenario to be the Production Scenario
    /// <para>Parameter 1: Production Scenario Id</para>
    /// <para>Parameter 2: Instigator Id</para>
    /// </summary>
    event Action<BaseId, BaseId> ScenarioConversionComplete;

    /// <summary>
    /// Whether the Scenario is a Read-only state
    /// </summary>
    bool ScenarioIsInReadonly { get; }

    /// <summary>
    /// Whether the Scenario data is currently locked
    /// </summary>
    bool ScenarioIsDataLocked { get; }

    /// <summary>
    /// A reference to a control (Main Form) that we can use to invoke back on the main thread
    /// </summary>
    Control InvokeControl { get; }

    /// <summary>
    /// A reference to the Message Provider interface that can be used to log system or error messages
    /// </summary>
    IMessageProvider MessageProvider { get; }

    /// <summary>
    /// Scenario level settings
    /// </summary>
    ScenarioPermissionSettings ScenarioPermissions { get; }

    /// <summary>
    /// Scenario Options
    /// </summary>
    ScenarioOptions SystemSettings { get; }

    /// <summary>
    /// When Scenario Options have changed
    /// </summary>
    event Action<ScenarioOptions> SystemSettingsChanged;

    /// <summary>
    /// The active Scenario interface to access object data sources
    /// </summary>
    IClientScenarioData ActiveClientScenarioData { get; }

    /// <summary>
    /// New KPI data has been saved
    /// </summary>
    event VoidDelegate KpiSnapshotsUpdated;

    /// <summary>
    /// Get ClientScenarioData's for comparable scenarios
    /// </summary>
    /// <returns></returns>
    List<IClientScenarioData> GetComparableClientScenarioDataList();

    /// <summary>
    /// Get the ScenarioDetail cache lock object
    /// </summary>
    ScenarioDetailCacheLock ScenarioDetailCacheLock { get; }

    /// <summary>
    /// When User or Plant Permissions have changed
    /// </summary>
    event Action UserPermissionsUpdated;

    ScenarioDataLock DataLock { get; }
}

public interface ICompareScenarioInfo
{
    /// <summary>
    /// List of Scenarios that need to be used to compare Scenario Data has been modified
    /// </summary>
    event Action<BaseId, bool> ComparableScenariosListModified;

    /// <summary>
    /// Scenario Data has changed for one of the Scenarios that is being compared
    /// </summary>
    event Action<BaseId> ComparableScenarioDataChanged;

    /// <summary>
    /// Active Scenario Id
    /// </summary>
    BaseId ScenarioId { get; }
}

public interface IScenarioController
{
    void LoadScenarioData();

    IScenarioInfo GetScenarioInfo();
    void UpdateUserPermissions(UserPermissionSet a_permissions, PlantPermissionSet a_plantPermissions, ScenarioDetail a_sd);

    /// <summary>
    /// This event is raised when an user's last accessible scenario is somehow
    /// rendered un-accessible (changing permissions or deleting scenarios).
    /// I was really hoping to be able to do something better with this, but right now,
    /// when this event is fired, we just close the program.
    /// There's a lot of background processes that depends on having an active scenario.
    /// Ideally, we'd block the background processes when no scenarios are accessible,
    /// then unblock them and load various data in when a scenario becomes accessible.
    /// </summary>
    event Action NoAccessibleScenarios;

    /// <summary>
    /// No scenario events or data are needed, likely the system is shutting down
    /// </summary>
    void UnloadAllScenarios();
}