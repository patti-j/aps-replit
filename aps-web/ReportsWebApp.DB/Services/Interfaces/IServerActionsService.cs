using ReportsWebApp.DB.Models;
using static ReportsWebApp.DB.Services.ServerActionsService;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface IServerActionsService
{
    /// <summary>
    /// Adds new records in the db for any new permissions
    /// </summary>
    /// <param name="permissions">The permissions to add</param>
    Task<Guid> RequestServerAction(int serverId, string command, string jsonParams, User requester);
    Task<bool> RequestServerAddPA(int serverId, User requester, CreatePlanningAreaRequest additionalParams);
    Task<bool> RequestServerCopyPA(int serverId, User requester, CopyPlanningAreaRequest additionalParams);
    Task<bool> RequestServerStartPA(int serverId, User requester, InstanceKey additionalParams);
    Task<bool> RequestServerStopPA(int serverId, User requester, InstanceKey additionalParams);
    Task<bool> RequestServerDeletePA(int serverId, User requester, InstanceKey additionalParams);
    Task<ActionCompletion> WaitForServerToDeletePA(int serverId, User requester, InstanceKey additionalParams, int timeout = 30000, int interval = 500);
    Task<bool> RequestServerRestartPA(int serverId, User requester, InstanceKey additionalParams);
    Task<bool> RequestPASettingsUpdate(int serverId, User requester, InstanceSettingsUpdateRequest additionalParams, bool ignoreOtherActions = false);
    Task<bool> RequestServerUpgrade(int serverId, User requester);
    Task<bool> RequestServerRestart(int serverId, User requester);
    Task<bool> RequestServerUpgradePA(int serverId, User requester, UpgradePlanningAreaRequest additionalParams);
    Task<ActionCompletion> WaitForActionCompletion(Guid transactionId, int timeout = 30000, int interval = 500);
}