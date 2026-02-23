using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Interface for all objects that are immediate children of Plant, such as Department.
/// Used to provide the necessary key information to UI functions.
/// </summary>
public interface IPlantChild
{
    string PlantName { get; }

    BaseId PlantId { get; }
}