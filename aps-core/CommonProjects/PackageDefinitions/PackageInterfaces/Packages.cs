using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace PT.PackageDefinitions.PackageInterfaces;

public interface IUsersPackage : ISchedulingPackage
{
    List<IPermissionModule> GetPermissionModules();

    List<IUserPreferencesModule> GetPreferencesModules();
}

public interface IUserPreferencesModule
{
    List<IUserPreferenceElement> GetPreferencesElements();
}

public interface IUserPreferenceElement : IPackageElement
{
    string Key { get; }
    ISettingData DefaultValue { get; }
}

public interface IAutomaticActionsPackage : ISchedulingPackage
{
    List<IAutomaticActionsModule> GetAutomaticActionsModules();
}

public interface IAutomaticActionsModule
{
    List<IAutomaticActionsElement> GetAutomaticActionsElements();
}

public interface IAutomaticActionsElement : IPackageElement, IPriorityElement
{
    /// <summary>
    /// Whether this action is enabled. Disabling it will stop any future actions
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    /// The primary schedule to execute, in cron format
    /// The format includes seconds so there can be 7 fields
    /// See https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/crontriggers.html for more details
    /// </summary>
    string CronSchedule { get; }

    /// <summary>
    /// A second schedule. The purpose is to allow creating additional triggers without overcomplicating it into a single cron statement
    /// Both cron schedules will be used simultaneously
    /// </summary>
    string CronSchedule2 { get; }

    /// <summary>
    /// The action has been triggered based on the schedule
    /// </summary>
    void RunAction();

    /// <summary>
    /// Something has changed on the action and the schedule needs to be updated
    /// For example the user set a new cron schedule
    /// </summary>
    event Action Reschedule;
}

public interface IApiPackage : ISchedulingPackage
{
    List<IApiModule> GetApiModules();
}

public interface IApiModule
{
    List<IApplicationPartElement> GetServiceParts();
}

public interface IApplicationPartElement : IPackageElement
{
    public ApplicationPart GetApplicationPart();
}