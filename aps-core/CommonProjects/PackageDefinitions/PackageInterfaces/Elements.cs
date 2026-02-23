using PT.APSCommon;

namespace PT.PackageDefinitions.PackageInterfaces;

public interface IUserPermissionElement : IPackageElement
{
    string GroupKey { get; }
    string PermissionKey { get; }
    string Caption { get; }
    string Description { get; }
}

/// <summary>
/// This enum is used to determine the interactivity of an element.
/// None means that it doesn't up show for the user at all.
/// ViewOnly and Edit are hopefully self-explanatory.
/// </summary>
public enum EUserAccess { None, UseGroupAccess, ViewOnly, Edit }

public static class PermissionsGroupConstants
{
    public static readonly string Scheduling = "Scheduling";
    public static readonly string Planning = "Planning";
    public static readonly string Modeling = "Modeling";
    public static readonly string Import = "Import";
    public static readonly string PublishGroup = "Publish";
    public static readonly string UndoRedoGroup = "Undo Redo";
    public static readonly string PlantsGroup = "Plants";
    public static readonly string MiscellaneousGroup = "Miscellaneous";
    public static readonly string BoardsGroup = "Boards";
    public static readonly string CapacityGroup = "Capacity";
    public static readonly string MainBarGroup = "Main Bar Controls";
}

public interface IPermissionsValidationElement : IPackageElement
{
    /// <summary>
    /// Checks to see if the instigator has view or edit permissions for the specified scenario
    /// </summary>
    /// <param name="a_instigatorId"></param>
    /// <param name="a_scenario"></param>
    /// <returns></returns>
    bool ValidatePermissions(BaseId a_instigatorId, object a_scenario);
}

public interface ILicenseValidationElement : IPackageElement
{
    /// <summary>
    /// Checks the scenario data for license limits
    /// </summary>
    /// <param name="a_scenario">Scenario class reference</param>
    /// <param name="a_scenarioDetail">ScenarioDetail class reference</param>
    /// <returns>Whether the data meets the limits</returns>
    bool ValidateData(object a_scenario, object a_scenarioDetail);

    /// <summary>
    /// Checks the transmission to determine if it should be processed
    /// </summary>
    /// <param name="a_transmission">Transmission class reference</param>
    /// <returns>Whether the transmission should be allowed when in read-only</returns>
    bool VerifyReadonlyTransmission(object a_transmission);

    /// <summary>
    /// Checks the transmission to determine if it should be processed
    /// </summary>
    /// <param name="a_transmission">Transmission class reference</param>
    /// <returns>Whether the transmission should be allowed when NOT in read-only</returns>
    bool VerifyTransmission(object a_transmission);

    /// <summary>
    /// Checks if a certain type of data is within the specified limit. True is good.
    /// </summary>
    /// <param name="a_dataCount"></param>
    /// <returns></returns>
    bool VerifyDataLimit(long a_dataCount)
    {
        return true;
    }

    /// <summary>
    /// String key reference to the element
    /// </summary>
    string DataKey { get; }

    /// <summary>
    /// Licensed value
    /// </summary>
    object LimitValue { get; }

    /// <summary>
    /// System value
    /// </summary>
    object CurrentValue { get; }
}