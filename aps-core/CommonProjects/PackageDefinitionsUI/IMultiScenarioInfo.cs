using System.Drawing;

using PT.APSCommon;
using PT.PackageDefinitions.Settings;
using PT.Scheduler;
using PT.Transmissions;

namespace PT.PackageDefinitionsUI;

public interface IMultiScenarioInfo : IScenarioInfo
{
    /// <summary>
    /// When a Scenario has been deleted.
    /// The bool, when true, indicates that the scenario is being unloaded/closed instead of it being a full delete.
    /// Almost everything except for the notification slide will work the same for a delete vs an unload.
    /// </summary>
    event Action<Scenario, ScenarioEvents, ScenarioUndoEvents, ScenarioBaseT, bool> ScenarioDeleted;

    /// <summary>
    /// Throw when a ScenarioDelete fails to complete. Currently, the only
    /// reason is due to failing validation.
    /// Parameters passed when invoked are instigatorId, scenarioId, and errorMessage
    /// </summary>
    event Action<BaseId, BaseId, string> ScenarioDeleteFailed;

    /// <summary>
    /// When a Scenario has been modified
    /// </summary>
    event Action<ScenarioChangeT> ScenarioChanged;

    /// <summary>
    /// When a new Scenario has been created
    /// </summary>
    event Action<BaseId, ScenarioBaseT> ScenarioCreated;
    
    /// <summary>
    /// When a Scenario has been reloaded. This event should only fire for the user that requested the reload
    /// </summary>
    event Action<BaseId, ScenarioBaseT> ScenarioReloaded;

    /// <summary>
    /// When a Scenario has failed to be replaced. This event should only fire for the user that requested the replace.
    /// </summary>
    event Action<BaseId, ScenarioReplaceT> ScenarioReplaceFailed;

    /// <summary>
    /// The active scenario properties have changed and should be updated.
    /// Note: No scenario detail data is changed
    /// </summary>
    event Action<long> ScenarioPropertiesChanged;

    /// <summary>
    /// Active Scenario name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Active Scenario color
    /// </summary>
    Color ScenarioColor { get; }

    /// <summary>
    /// Active Scenario Planning Settings-- Production flag and Isolate options
    /// </summary>
    ScenarioPlanningSettings PlanningSettings { get; }

    /// <summary>
    /// Active Scenario Planning Settings-- Production flag and Isolate options
    /// </summary>
    IntegrationConfigMappingSettings IntegrationSettings { get; }

    /// <summary>
    /// List of Scenario Contexts
    /// </summary>
    List<ScenarioContextPlus> ScenarioContexts { get; }

    BaseId ActiveScenarioId { get; }

    bool IsScenarioLoaded(BaseId a_scenarioId);
}