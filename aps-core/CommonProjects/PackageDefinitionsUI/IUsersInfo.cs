using System.Collections.Concurrent;

using PT.APSCommon;
using PT.PackageDefinitionsUI.UserSettings;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;

namespace PT.PackageDefinitionsUI;

public interface IUsersInfo
{
    /// <summary>
    /// Event for when the users' list is updated
    /// </summary>
    event Action<IScenarioDataChanges> UserDataChanged;

    /// <summary>
    /// Event for when a user modifies Optimize Presets
    /// </summary>
    event Action<string, string> OptimizePresetSaved;

    /// <summary>
    /// Event for when a user modifies Compress Presets
    /// </summary>
    event Action<string, string> CompressPresetSaved;

    /// <summary>
    /// Event for when a user switches Active Optimize Presets
    /// </summary>
    event Action<string> ActiveOptimizeSettingsPresetSwitched;

    /// <summary>
    /// Event for when a user switches Active Compress Presets
    /// </summary>
    event Action<string> ActiveCompressSettingsPresetSwitched;

    /// <summary>
    /// Get readable user name by Id
    /// </summary>
    /// <param name="a_userId"></param>
    /// <returns></returns>
    string GetUserName(BaseId a_userId);

    /// <summary>
    /// Cached reference to UseScenarioOptimize setting
    /// </summary>
    bool CurrentUserUseScenarioOptimizeSettings { get; }

    /// <summary>
    /// Thread-safe dictionary of users
    /// </summary>
    ConcurrentDictionary<BaseId, string> UsersList { get; }

    /// <summary>
    /// Get User Permission Sets;
    /// </summary>
    /// <returns></returns>
    HashSet<UserPermissionSet> GetUserPermissionSets();

    /// <summary>
    /// Get User Permission Sets;
    /// </summary>
    /// <returns></returns>
    HashSet<PlantPermissionSet> GetPlantPermissionSets();

    /// <summary>
    /// Get the user's permission group Id
    /// </summary>
    /// <param name="a_userId"></param>
    /// <returns></returns>
    BaseId GetUserPermissionSetId(BaseId a_userId);

    /// <summary>
    /// Saves currently active Optimize Settings Preset
    /// </summary>
    /// <param name="a_presetKey"></param>
    /// <param name="a_optimizeSettings"></param>
    void SaveUserOptimizeSettings(string a_presetKey, string a_controlName, OptimizeSettings a_optimizeSettings);

    /// <summary>
    /// Saves currently active Compress Settings Preset
    /// </summary>
    /// <param name="a_presetKey"></param>
    /// <param name="a_compressSettings"></param>
    void SaveUserCompressSettings(string a_presetKey, string a_controlName, OptimizeSettings a_compressSettings);

    /// <summary>
    /// Get the active Optimize Settings Preset
    /// </summary>
    /// <returns></returns>
    OptimizeSettingsPreset GetActivePresetOptimizeSettings();

    /// <summary>
    /// Get the active Compress Settings Preset
    /// </summary>
    /// <returns></returns>
    OptimizeSettingsPreset GetActivePresetCompressSettings();

    /// <summary>
    /// Get the list of Optimize Settings Preset keys
    /// </summary>
    /// <returns></returns>
    List<string> GetOptimizeSettingsPresetKeys();

    /// <summary>
    /// Create a new Optimize Settings Preset
    /// </summary>
    /// <param name="a_presetKey"></param>
    void CreateOptimizeSettingsPreset(string a_presetKey);

    /// <summary>
    /// Rename an existing Optimize Settings Preset
    /// </summary>
    /// <param name="a_presetKeyToRename">The old preset key</param>
    /// <param name="a_newPresetKey">The new preset key</param>
    void RenameOptimizeSettingsPreset(string a_presetKeyToRename, string a_newPresetKey);

    /// <summary>
    /// Make a copy of an existing Optimize Settings Preset
    /// </summary>
    /// <param name="a_presetKeyToCopy"></param>
    /// <param name="a_newPresetKey"></param>
    void CopyOptimizeSettingsPreset(string a_presetKeyToCopy, string a_newPresetKey);

    /// <summary>
    /// Remove an Optimize Settings Preset
    /// </summary>
    /// <param name="a_presetKey"></param>
    /// <returns></returns>
    bool RemoveOptimizeSettingsPreset(string a_presetKey);

    /// <summary>
    /// Switch the active Optimize Settings Preset
    /// </summary>
    /// <param name="a_presetKey"></param>
    void SwitchActiveOptimizeSettingsPreset(string a_presetKey);

    /// <summary>
    /// Get the list of Compress Settings Preset keys
    /// </summary>
    /// <returns></returns>
    List<string> GetCompressSettingsPresetKeys();

    /// <summary>
    /// Create a new Compress Settings Preset
    /// </summary>
    /// <param name="a_presetKey"></param>
    void CreateCompressSettingsPreset(string a_presetKey);

    /// <summary>
    /// Rename an existing Compress Settings Preset
    /// </summary>
    /// <param name="a_presetKeyToRename"></param>
    /// <param name="a_newPresetKey"></param>
    void RenameCompressSettingsPreset(string a_presetKeyToRename, string a_newPresetKey);

    /// <summary>
    /// Make a copy of an existing Compress Settings Preset
    /// </summary>
    /// <param name="a_presetKeyToCopy"></param>
    /// <param name="a_newPresetKey"></param>
    void CopyCompressSettingsPreset(string a_presetKeyToCopy, string a_newPresetKey);

    /// <summary>
    /// Remove a Compress Settings Preset
    /// </summary>
    /// <param name="a_presetKey"></param>
    /// <returns></returns>
    bool RemoveCompressSettingsPreset(string a_presetKey);

    /// <summary>
    /// Switch the active Compress Settings Preset
    /// </summary>
    /// <param name="a_presetKey"></param>
    void SwitchActiveCompressSettingsPreset(string a_presetKey);

    /// <summary>
    /// Uses the ID to find the group's CanManageScenario and returns it
    /// </summary>
    /// <param name="a_currentUserId"></param>
    /// <returns>Returns the current user's permission group's CanManageScenarios</returns>
    bool GetUserGroupCanManageScenarios(BaseId a_currentUserId);

    /// <summary>
    /// Uses the use ID to find the if the user's group has permission to administrate users and returns it
    /// </summary>
    /// <param name="a_currentUserId"></param>
    /// <returns></returns>
    bool CanUserAdministrateUsers(BaseId a_currentUserId);

    /// <summary>
    /// Uses the user ID to find the group's MaintainSystemSettings property and returns it
    /// </summary>
    /// <param name="a_currentUserId"></param>
    /// <returns></returns>
    bool CanUserMaintainSystemSettings(BaseId a_currentUserId);

    /// <summary>
    /// Uses the user ID to find the group's AllowChangesThatOverrideOtherUserActions property and returns it
    /// </summary>
    /// <param name="a_currentUserId"></param>
    /// <returns></returns>
    bool GetUserGroupAllowOverrideOtherUserActions(BaseId a_currentUserId);

    BaseId CurrentUserId { get; }
}