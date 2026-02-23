using PT.APSCommon;

namespace PT.PackageDefinitions;

public interface IWorkspaceInfo : ISettingsManager
{
    /// <summary>
    /// The active workspace has been switched. Data should be reloaded using the new workspace
    /// </summary>
    event Action WorkspaceActivated;

    /// <summary>
    /// User's workspaces were modified
    /// </summary>
    event Action UserWorkspacesModified;

    /// <summary>
    /// Shared collection of workspaces was modified
    /// </summary>
    event Action SharedWorkspaceCollectionModified;

    /// <summary>
    /// Collect unsaved settings
    /// </summary>
    event Action<WorkspaceSettingsCollector> CollectUnsavedSettings;

    /// <summary>
    /// Get the active workspace name
    /// </summary>
    string WorkspaceName { get; }

    /// <summary>
    /// The DateTime when the active workspace was last saved
    /// </summary>
    DateTime LastSaveDate { get; }

    /// <summary>
    /// Get string list of workspace names from shared collection
    /// </summary>
    List<string> SharedWorkspacesList { get; }

    /// <summary>
    /// Get specific workspace container from shared collection
    /// </summary>
    /// <param name="a_workspaceName"></param>
    /// <returns></returns>
    byte[] GetSharedWorkspaceBytes(string a_workspaceName);

    /// <summary>
    /// Get specific workspace dictionary from shared collection
    /// </summary>
    /// <param name="a_workspaceName"></param>
    /// <returns></returns>
    Dictionary<string, SettingData> GetSharedWorkspaceDictionary(string a_workspaceName);

    /// <summary>
    /// Get a specific workspace settings dictionary
    /// </summary>
    /// <param name="a_workspaceName"></param>
    /// <returns></returns>
    Dictionary<string, SettingData> GetUserSettingsDictionary(string a_workspaceName);

    /// <summary>
    /// Get the list of user workspace names
    /// </summary>
    List<string> GetUserWorkspaceList();

    /// <summary>
    /// </summary>
    /// <param name="a_prefix"></param>
    /// <returns>An IEnumerable that contains the setting keys that start with the prefix passed in</returns>
    IEnumerable<string> GetSettingsKeysByPrefix(string a_prefix);

    /// <summary>
    /// Check whether a user workspace with the specified name already exists
    /// </summary>
    /// <returns></returns>
    bool ValidateUserWorkspaceExists(string a_workspaceName);

    /// <summary>
    /// Check whether a workspace with the specified name already exists in the shared server collection
    /// </summary>
    /// <returns></returns>
    bool ValidateSharedWorkspaceExists(string a_workspaceName);
}

public interface IWorkspaceController
{
    /// <summary>
    /// Get workspace info reference
    /// </summary>
    /// <returns></returns>
    IWorkspaceInfo GetWorkspaceInfo();

    /// <summary>
    /// Import a workspace to the user collection
    /// </summary>
    /// <param name="a_settingDictionary"></param>
    /// <param name="a_workspaceName"></param>
    /// <param name="a_overwrite"></param>
    /// <param name="a_resetOtherSettings"></param>
    /// <param name="a_baseIds"></param>
    /// <param name="a_applyImmediately"></param>
    void ImportWorkspace(Dictionary<string, SettingData> a_settingDictionary, string a_workspaceName, bool a_overwrite, bool a_resetOtherSettings, List<BaseId> a_baseIds, bool a_applyImmediately);

    /// <summary>
    /// Serialize the user workspaces
    /// </summary>
    /// <returns></returns>
    Dictionary<string, byte[]> SerializeWorkspaces();

    /// <summary>
    /// Delete a workspace from the user collection
    /// </summary>
    /// <param name="a_workspaceName"></param>
    void DeleteWorkspace(string a_workspaceName, bool a_fireImportCompleteEvent = true);

    /// <summary>
    /// Make a copy of a user workspace
    /// </summary>
    /// <param name="a_copyTuple"></param>
    /// <returns></returns>
    bool CopyWorkspace(string a_sourceWorkspace, string a_newWorkspaceName);

    /// <summary>
    /// Switch to another workspace
    /// </summary>
    /// <param name="a_workspaceName"></param>
    void SwitchWorkspace(string a_workspaceName);

    /// <summary>
    /// Create a new workspace
    /// </summary>
    /// <param name="a_name"></param>
    /// <returns></returns>
    bool CreateNewWorkspace(string a_name);

    /// <summary>
    /// Change the name of a workspace
    /// </summary>
    /// <param name="a_workspaceToRename"></param>
    /// <param name="a_newName"></param>
    void RenameWorkspace(string a_workspaceToRename, string a_newName);

    /// <summary>
    /// Request unsaved settings
    /// </summary>
    void SaveActiveWorkspace();
}