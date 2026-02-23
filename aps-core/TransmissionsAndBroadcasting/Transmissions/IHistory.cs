using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Can be implemented in a Transmission to enable a more informative History to be created.
/// </summary>
public interface IHistory
{
    /// <summary>
    /// Describes the details of the Transmission.
    /// </summary>
    string Description { get; }
}

public interface IHistoryScenarioId : IHistory
{
    /// <summary>
    /// The Id of the Scenario in which the object resides.
    /// </summary>
    BaseId ScenarioId { get; }
}

/// <summary>
/// Defines history information for a Job specific Transmission.
/// </summary>
public interface IHistoryJobId : IHistoryScenarioId
{
    BaseId JobId { get; }
}

/// <summary>
/// Defines history information for a Transmission that affects one or more Jobs.
/// </summary>
public interface IHistoryJobIds : IHistoryScenarioId
{
    BaseIdList JobIds { get; }
}