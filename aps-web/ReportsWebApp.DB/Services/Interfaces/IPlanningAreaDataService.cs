using ReportsWebApp.Common;
using ReportsWebApp.DB.Models;

public interface IPlanningAreaDataService
{
    /// <summary>
    /// Selects a planning area and updates the favorite planning area for the specified user and company.
    /// </summary>
    /// <param name="planningArea">The planning area details to select.</param>
    /// <param name="companyId">The ID of the company.</param>
    /// <param name="userId">The ID of the user selecting the planning area.</param>
    Task SelectPlanningAreaAsync(PADetails planningArea, int companyId, int userId);

    /// <summary>
    /// Retrieves scenarios for a given company ID and user ID.
    /// Scenarios are fetched from multiple databases associated with analytical data.
    /// </summary>
    /// <param name="companyId">The ID of the company.</param>
    /// <param name="userId">The ID of the user (not used in current logic, but included for future use).</param>
    /// <returns>A list of scenarios retrieved from the associated databases.</returns>
    Task<List<Scenario>> GetScenariosByCompanyIdAndUserIdAsync(int companyId, int userId);

    /// <summary>
    /// Retrieves the selected planning area by company ID and user ID.
    /// If the favorite planning area is not found, throws an exception.
    /// </summary>
    /// <param name="companyId">The ID of the company.</param>
    /// <param name="userId">The ID of the user requesting the favorite planning area.</param>
    /// <returns>The selected planning area details.</returns>
    Task<PADetails> GetSelectedPlanningAreaByCompanyIdAndUserIdAsync(int companyId, int userId);

    /// Checks if a given name and version combination is a duplicate, excluding a specific ID.
    /// </summary>
    /// <param name="a_paDetails"></param>
    /// <returns>True if a duplicate exists, otherwise false.</returns>
    Task<bool> IsPlanningAreaNameDuplicateAsync(PADetails a_paDetails);

    /// <summary>
    /// Retrieves all planning areas associated with a specific company ID.
    /// </summary>
    /// <param name="companyId">The ID of the company.</param>
    /// <returns>A list of planning area details.</returns>
    Task<List<PADetails>> GetPlanningAreasByCompanyIdAsync(int companyId);
    
    Task<List<PADetails>> GetPlanningAreasByManagingCompanyIdAsync(int companyId);

    /// <summary>
    /// Retrieves all planning areas that are backups of the provided planning area
    /// </summary>
    /// <param name="planningArea">The Planning area to search for backups of.</param>
    /// <returns>A list of planning area details.</returns>
    Task<List<PADetails>> GetBackups(PADetails planningArea);

    /// <summary>
    /// Retrieves PA status data. Does not load related entities, so more suitable for frequent checks.
    /// </summary>
    /// <param name="planningAreaId"></param>
    /// <returns></returns>
    Task<PlanningAreaLiteModel> GetPlanningAreaStatus(int planningAreaId);
    
    /// <summary>
    /// Retrieves PA status data by server. Does not load related entities, so more suitable for frequent checks.
    /// </summary>
    /// <param name="serverId"></param>
    /// <param name="companyId"></param>
    /// <returns></returns>
    Task<List<PlanningAreaLiteModel>> GetPlanningAreaStatusesForServerAsync(int serverId);

    /// <summary>
    /// Get planning areas that can be logged in for a user.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<List<PADetails>> GetPlanningAreasForUserAsync(User user);

    /// <summary>
    /// Saves the planning area details. If the area already exists, it updates the existing one; otherwise, it creates a new one.
    /// </summary>
    /// <param name="planningAreaDetails">The planning area details to save.</param>
    Task SaveAsync(PADetails planningAreaDetails);

    /// <summary>
    /// Saves the planning area details, deleting any existing pa with the same identifier. This is used for import/migration from server manager
    /// </summary>
    /// <param name="planningAreaDetails">The planning area details to save.</param>
    Task OverwriteAsync(PADetails planningAreaDetails);

    /// <summary>
    /// Deletes the specified planning area.
    /// </summary>
    /// <param name="planningAreaDetails">The planning area details to delete.</param>
    Task HardDeletePlanningAreaAsync(PADetails planningAreaDetails);

    /// <summary>
    /// Deletes the specified planning area, but only if the specified PA is a backup. Makes no changes and returns false if the PA is active
    /// </summary>
    /// <param name="planningAreaDetails">The planning area details to delete.</param>
    /// <returns>True if the PA was deleted, false otherwise</returns>
    Task<bool> DeleteBackupAsync(PADetails planningAreaDetails);

    /// <summary>
    /// Deletes the specified planning area.
    /// </summary>
    /// <param name="planningAreaDetails">The planning area details to delete.</param>
    Task DeletePlanningAreaAsync(PADetails planningAreaDetails);

    Task<List<PlanningAreaTag>> GetGroupsForCompanyAsync(int companyId);

    /// <summary>
    /// Saves the planning area tag. If the area already exists, it updates the existing one; otherwise, it creates a new one.
    /// </summary>
    /// <param name="tag">The planning area tag to save.</param>
    Task<PlanningAreaTag> AddOrUpdateTagAsync(PlanningAreaTag tag);

    /// <summary>
    /// Saves the planning area tag. If the area already exists, it updates the existing one; otherwise, it creates a new one.
    /// </summary>
    /// <param name="tag">The planning area tag to save.</param>
    Task UpdateStatusAsync(PADetails model, string status);

    /// <summary>
    /// Deletes the specified planning area tag.
    /// </summary>
    /// <param name="tag">The planning area tag to delete.</param>
    Task DeleteTagAsync(PlanningAreaTag tag);

    /// <summary>
    /// Retrieves the planning area details by its ID from the database.
    /// </summary>
    /// <param name="planningAreaId">The planning area ID.</param>
    /// <returns>The planning area details.</returns>
    Task<PADetails> GetPlanningAreaByIdAsync(int planningAreaId);

    /// <summary>
    /// Updates the planning area by serializing its settings.
    /// </summary>
    /// <param name="pa">The planning area details.</param>
    /// <returns>The updated planning area details.</returns>
    PADetails UpdatePlanningAreaJSON(PADetails pa);

    /// <summary>
    /// Updates the 'current' pa to be a backup of 'toRestore,' and updates 'toRestore' to be active
    /// </summary>
    /// <param name="current">The PA that should become a backup</param>
    /// <param name="toRestore">The PA that should be restored</param>
    /// <returns></returns>
    public Task RestoreBackup(PADetails current, PADetails toRestore);

    /// <summary>
    /// Waits until a Planning Area has a specific state, or the timeout has elapsed
    /// </summary>
    /// <param name="pa">The planning area to check</param>
    /// <param name="state">The state to wait for</param>
    /// <param name="timeout">The maximum time to wait (default 30s)</param>
    /// <param name="interval">How frequently to check (default twice per second)</param>
    /// <returns>True when the PA's state has been set to the provided state. False if the timeout elapsed.</returns>
    Task<bool> WaitForPAStatus(PADetails pa, EServiceState state, int timeout = 30000, int interval = 500);

    Task<DateTime?> GetPublishDate(int companyId);
    Task<Dictionary<string, int>?> GetTotalErrors(PADetails pa, int a_retryCount);
    Task<List<PlanningAreaDataService.ErrorRow>> GetLogs(PADetails pa, string category, int start, int amt, int a_retryCount);

    Task<List<PADetails>> GetAllBackupsForCompany(int companyId);
    Task CreateFolder(User editor, PlanningAreaLocation parent, string name);
    Task MoveFolder(User editor, PlanningAreaLocation a_location, PlanningAreaLocation? newParent);
    Task MoveFolder(User editor, PADetails pa, PlanningAreaLocation? newParent);
    Task DeleteFolder(User editor, PlanningAreaLocation a_location);
    Task<List<PlanningAreaLocation>> GetFolders(int serverId);
    Task CreateRootFolder(User editor, CompanyServer server, string name);
    Task RenameFolder(User editor, PlanningAreaLocation a_location, string name);
    Task<List<PADetails>> GetPlanningAreasByUsingCompany(int companyId);
}