using System.Text.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.DB.Services
{
    public class ServerActionsService : IServerActionsService
    {
        private readonly IDbContextFactory<DbReportsContext> _dbFactory;

        public ServerActionsService(IDbContextFactory<DbReportsContext> factory)
        {
            _dbFactory = factory;
        }

        private DbReportsContext GetDbContext()
        {
            // Resolve a new scoped DbContext using the IServiceProvider
            return _dbFactory.CreateDbContext();
        }

        /// <summary>
        /// Adds new records in the db for any new permissions
        /// </summary>
        /// <param name="permissions">The permissions to add</param>
        public async Task<ServerManagerActionRequest> CancelServerAction(ServerManagerActionRequest action)
        {
            using var context = GetDbContext();

            var dbAction = context.ServerManagerActionRequests.FirstOrDefault(x => x.TransactionId == action.TransactionId);

            if (dbAction.RequestStatus == EActionRequestStatuses.Processing.ToString() || dbAction.RequestStatus == EActionRequestStatuses.New.ToString())
            {
                dbAction.RequestStatus = EActionRequestStatuses.Cancelled.ToString();
                dbAction.ErrorMessage = "Request was cancelled prior to completion.";
                await context.SaveChangesAsync();

                return dbAction;
            }

            return null;
        }

        /// <summary>
        /// Adds new records in the db for any new permissions
        /// </summary>
        /// <param name="permissions">The permissions to add</param>
        public async Task<Guid> RequestServerAction(int serverId, string command, string? jsonParams, User requester)
        {
            using var context = GetDbContext();
            var action = new ServerManagerActionRequest()
            {
                TransactionId = Guid.NewGuid(),
                ServerId = serverId,
                Action = command,
                ParameterJson = jsonParams ?? "",
                RequestedBy = requester.Email,
                RequestStatus = EActionRequestStatuses.New.ToString(),
                ErrorMessage = null,
                RequestedDateTime = DateTime.UtcNow,
                UpdatedDateTime = null,
            };

            context.ServerManagerActionRequests.Add(action);

            await context.SaveChangesAsync();
            return action.TransactionId;
        }

        /// <summary>
        /// Checks if a there is an action with matching params already pending
        /// </summary>
        /// <typeparam name="T">The type of Params object to deserialize to</typeparam>
        /// <param name="serverId">The Id of the server to which the action is being sent</param>
        /// <param name="type">The server action type of the action</param>
        /// <param name="paramObj">The params object to compare against</param>
        /// <returns>True if a similar action is found, false otherwise</returns>
        public async Task<Guid?> GetExistingAction<T>(int serverId, EServerActionTypes type, T paramObj)
        {
            using var context = GetDbContext();

            // Start with all "New" and "Processing" actions of this type for this server
            var similarActions = await context.ServerManagerActionRequests
                    .Where(x => x.ServerId == serverId && x.Action == type.ToString() &&
                          (x.RequestStatus == EActionRequestStatuses.New.ToString() ||
                           x.RequestStatus == EActionRequestStatuses.Processing.ToString())).ToListAsync();

            // Find all actions that are "processing" and more than an hour old. These have likely been aborted or failed to report success
            var oldActions = similarActions.Where(x => x.RequestStatus == EActionRequestStatuses.Processing.ToString() &&
                                                       (x.UpdatedDateTime == null || x.UpdatedDateTime < DateTime.UtcNow.AddHours(-1))).ToList();
            foreach (var oldAction in oldActions)
            {
                // Update the action to an error state, so that it does not prevent new actions from creating
                oldAction.RequestStatus = EActionRequestStatuses.Error.ToString();
                oldAction.UpdatedDateTime = DateTime.UtcNow;
                oldAction.ErrorMessage = "This action was picked up by the server, but never completed.";

                context.Update(oldAction);

                // This action is no longer "Processing", so it should be ignored
                similarActions.Remove(oldAction);
            }

            await context.SaveChangesAsync();

            // If params is null, this action is server specific, so all actions of the same type are similar
            if (paramObj == null)
            {
                return similarActions.OrderByDescending(x => x.RequestedDateTime).Select(x => x.TransactionId).FirstOrDefault();
            }

            // Otherwise, check if params object is equal to the other
            foreach (var action in similarActions)
            {
                try
                {
                    T otherParamObj = JsonSerializer.Deserialize<T>(action.ParameterJson);
                    if (paramObj.Equals(otherParamObj))
                    {
                        return action.TransactionId;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to parse JSON for Server Action:\n" + ex);
                } 
            }

            // No matching actions found
            return null;
        }

        /// <summary>
        /// A non-generic version of CheckForSimilarActions that just checks if any other actions of the same type are pending
        /// Used for parameterless actions
        /// </summary>
        /// <param name="serverId">The Id of the server to which the action is being sent</param>
        /// <param name="type">The server action type of the action</param>
        /// <returns>True if a similar action is found, false otherwise</returns>
        public async Task<bool> GetExistingAction(int serverId, EServerActionTypes type)
        {
            using var context = GetDbContext();

            // Find any "New" and "Processing" actions of this type for this server
            var similarActionsExist = await context.ServerManagerActionRequests
                  .Where(x => x.ServerId == serverId && x.Action == type.ToString() &&
                              (x.RequestStatus == EActionRequestStatuses.New.ToString() ||
                               x.RequestStatus == EActionRequestStatuses.Processing.ToString())).AnyAsync();


            return similarActionsExist;
        }

        public async Task<bool> RequestServerAddPA(int serverId, User requester, CreatePlanningAreaRequest additionalParams)
        {
            if (null != await GetExistingAction(serverId, EServerActionTypes.AddPlanningArea, additionalParams))
            {
                return false;
            }
            string json = JsonSerializer.Serialize(additionalParams);
            await RequestServerAction(serverId, EServerActionTypes.AddPlanningArea.ToString(), json, requester);
            return true;
        }

        public async Task<bool> RequestServerCopyPA(int serverId, User requester, CopyPlanningAreaRequest additionalParams)
        {
            if (null != await GetExistingAction(serverId, EServerActionTypes.CopyPlanningArea, additionalParams))
            {
                return false;
            }
            string json = JsonSerializer.Serialize(additionalParams);
            await RequestServerAction(serverId, EServerActionTypes.CopyPlanningArea.ToString(), json, requester);
            return true;
        }

        public async Task<bool> RequestServerStartPA(int serverId, User requester, InstanceKey additionalParams)
        {
            if (null != await GetExistingAction(serverId, EServerActionTypes.StartPlanningArea, additionalParams))
            {
                return false;
            }
            string json = JsonSerializer.Serialize(additionalParams);
            await RequestServerAction(serverId, EServerActionTypes.StartPlanningArea.ToString(), json, requester);
            return true;
        }

        public async Task<bool> RequestServerRestartPA(int serverId, User requester, InstanceKey additionalParams)
        {
            if (null != await GetExistingAction(serverId, EServerActionTypes.RestartPlanningArea, additionalParams))
            {
                return false;
            }
            string json = JsonSerializer.Serialize(additionalParams);
            await RequestServerAction(serverId, EServerActionTypes.RestartPlanningArea.ToString(), json, requester);
            return true;
        }

        public async Task<Guid> RequestPlanningAreaLogs(int serverId, User requester, InstanceKey additionalParams)
        {
            string json = JsonSerializer.Serialize(additionalParams);
            
            return await RequestServerAction(serverId, EServerActionTypes.GetLogs.ToString(), json, requester);
        }

        public async Task<bool> RequestPASettingsUpdate(int serverId, User requester, InstanceSettingsUpdateRequest additionalParams, bool ignoreOtherActions = false)
        {
            if (!ignoreOtherActions && null != await GetExistingAction(serverId, EServerActionTypes.UpdatePlanningAreaSettings, additionalParams))
            {
                return false;
            }
            string json = JsonSerializer.Serialize(additionalParams);
            await RequestServerAction(serverId, EServerActionTypes.UpdatePlanningAreaSettings.ToString(), json, requester);
            return true;
        }

        public async Task<bool> RequestServerStopPA(int serverId, User requester, InstanceKey additionalParams)
        {
            if (null != await GetExistingAction(serverId, EServerActionTypes.StopPlanningArea, additionalParams))
            {
                return false;
            }
            string json = JsonSerializer.Serialize(additionalParams);
            await RequestServerAction(serverId, EServerActionTypes.StopPlanningArea.ToString(), json, requester);
            return true;
        }

        public async Task<bool> RequestServerDeletePA(int serverId, User requester, InstanceKey additionalParams)
        {
            if (null != await GetExistingAction(serverId, EServerActionTypes.DeletePlanningArea, additionalParams))
            {
                return false;
            }
            string json = JsonSerializer.Serialize(additionalParams);
            await RequestServerAction(serverId, EServerActionTypes.DeletePlanningArea.ToString(), json, requester);
            return true;
        }

        public async Task<ActionCompletion> WaitForServerToDeletePA(int serverId, User requester, InstanceKey additionalParams, int timeout = 30000, int interval = 500)
        {
            var existingId = await GetExistingAction(serverId, EServerActionTypes.DeletePlanningArea, additionalParams);
            if (null != existingId)
            {
                return new ActionCompletion(false, false, "Action already exists.", existingId);
            }
            string json = JsonSerializer.Serialize(additionalParams);
            var trans = await RequestServerAction(serverId, EServerActionTypes.DeletePlanningArea.ToString(), json, requester);
            return await WaitForActionCompletion(trans, timeout, interval);
        }

        public async Task<bool> RequestServerUpgradePA(int serverId, User requester, UpgradePlanningAreaRequest additionalParams)
        {
            if (null != await GetExistingAction(serverId, EServerActionTypes.UpgradePlanningArea, additionalParams))
            {
                return false;
            }
            string json = JsonSerializer.Serialize(additionalParams);
            await RequestServerAction(serverId, EServerActionTypes.UpgradePlanningArea.ToString(), json, requester);
            return true;
        }

        public async Task<bool> RequestServerUpgrade(int serverId, User requester)
        {
            if (await GetExistingAction(serverId, EServerActionTypes.UpgradeServerAgent))
            {
                return false;
            }
            await RequestServerAction(serverId, EServerActionTypes.UpgradeServerAgent.ToString(), null, requester);
            return true;
        }

        public async Task<bool> RequestServerRestart(int serverId, User requester)
        {
            if (await GetExistingAction(serverId, EServerActionTypes.RestartServer))
            {
                return false;
            }
            await RequestServerAction(serverId, EServerActionTypes.RestartServer.ToString(), null, requester);
            return true;
        }

        /// <summary>
        /// Get a list of server actions filtered by optional parameters.
        /// </summary>
        public async Task<List<ServerManagerActionRequest>> GetActionsAsync(int companyId, int? serverId = null, string? action = null, string? status = null, PADetails pa = null)
        {
            using var context = GetDbContext();
            var query = context.ServerManagerActionRequests
                               .Include(s => s.Server)
                               .Where(x => x.Server.ManagingCompany.Id == companyId)
                               .AsQueryable();

            if (serverId.HasValue)
            {
                query = query.Where(a => a.ServerId == serverId.Value);
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action == action);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.RequestStatus == status);
            }

            if (pa != null)
            {
                //query = query.Where(a => a.)
            }

            return await query
                .OrderByDescending(a => a.RequestedDateTime) // Sort by most recent first
                .ToListAsync();
        }

        public record ActionCompletion(bool isSuccess, bool isTimeout = false, string error = "", Guid? exsitingId = null);
        public async Task<ActionCompletion> WaitForActionCompletion(Guid transactionId, int timeout = 30000, int interval = 500)
        {
            var time = DateTime.UtcNow;
            while (time.AddMilliseconds(timeout) > DateTime.UtcNow)
            {
                // Create dbContext new each cycle so that it will retrieve updated data
                await using DbReportsContext dbContext = GetDbContext();
                var action = dbContext.ServerManagerActionRequests.FirstOrDefault(x => x.TransactionId == transactionId);
                if (action != null && action.RequestStatus == EActionRequestStatuses.Success.ToString())
                {
                    return new ActionCompletion(true);
                }
                if (action != null && action.RequestStatus == EActionRequestStatuses.Error.ToString())
                {
                    return new ActionCompletion(true, false, action.ErrorMessage);
                }

                await Task.Delay(interval);
            }
            return new ActionCompletion(false, true, "Timeout");
        }
    }
}
