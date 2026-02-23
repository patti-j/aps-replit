using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Interface for all objects that are immediate children of Department, such as Resource.
/// Used to provide the necessary key information to UI functions.
/// </summary>
public interface IDepartmentChild
{
    string PlantName { get; }

    BaseId PlantId { get; }

    string DepartmentName { get; }

    BaseId DepartmentId { get; }
}