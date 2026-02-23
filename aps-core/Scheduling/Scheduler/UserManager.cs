using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;

using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.PackageDefinitions;
using PT.PackageDefinitions.DTOs;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitions.Settings;
using PT.PackageDefinitions.UserWorkspaces;
using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using PT.Transmissions.User;

namespace PT.Scheduler;

/// <summary>
/// Summary description for UserManager.
/// </summary>
public class UserManager : BaseObjectManager<User>, IUserManager
{
    #region IPTSerializable Members
    internal UserManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        #region 12558
        if (a_reader.VersionNumber >= 13000)
        {
            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                User u = new(a_reader);
                Add(u);
            }

            m_userPermissionSetManager = new UserPermissionSetManager(a_reader, a_idGen);
            m_workspaceTemplatesManager = new WorkspaceTemplatesManager(a_reader);
        }
        #endregion
        #region 12503
        else if (a_reader.VersionNumber >= 12503)
        {
            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                User u = new(a_reader);
                Add(u);
            }

            new TransmissionSequencer(a_reader);
            m_userPermissionSetManager = new UserPermissionSetManager(a_reader, a_idGen);
            m_workspaceTemplatesManager = new WorkspaceTemplatesManager(a_reader);
        }
        #endregion
        #region 12210
        else if (a_reader.VersionNumber >= 12210)
        {
            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                User u = new (a_reader);
                Add(u);
            }

            new TransmissionSequencer(a_reader);
            m_userPermissionSetManager = new UserPermissionSetManager(a_reader, a_idGen);
            m_workspaceTemplatesManager = new WorkspaceTemplatesManager(a_reader);
        }
        #endregion
        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                User u = new (a_reader);
                Add(u);
            }

            new TransmissionSequencer(a_reader);
            new ScenarioOptions(a_reader);
            m_userPermissionSetManager = new UserPermissionSetManager(a_reader, a_idGen);
            m_workspaceTemplatesManager = new WorkspaceTemplatesManager(a_reader);
        }
        #endregion
        else
        {
            //666
            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                User u = new (a_reader);
                Add(u);
            }

            new TransmissionSequencer(a_reader);
            new ScenarioOptions(a_reader);
            m_userPermissionSetManager = new UserPermissionSetManager(a_reader, a_idGen);

            //If upgrading from v11, initialize all users with default user/plant permissions IDs
            foreach (User user in this)
            {
                user.UserPermissionSetId = m_userPermissionSetManager.DefaultUserPermissionSetId;
                user.PlantPermissionsId = m_userPermissionSetManager.DefaultPlantPermissionSetId;
            }

            //Load instance workspaces
            m_workspaceTemplatesManager = new WorkspaceTemplatesManager();
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_userPermissionSetManager.Serialize(a_writer);
        m_workspaceTemplatesManager.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 352;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal virtual void RestoreReferences(ISystemLogger a_erm, string a_workspacesPath, PackageManager a_packageManager, UserFieldDefinitionManager a_udfManager)
    {
        //Restore loaded permissions
        SynchronizePermissionModules(a_packageManager);

        m_errorReporter = a_erm;
        for (int i = 0; i < Count; i++)
        {
            User u = this[i];
            if (u.Id == BaseId.NULL_ID)
            {
                //This is a server user and does not user permissions.
                continue;
            }

            u.RestoreReferences(m_userPermissionSetManager, a_udfManager);
        }

        if (PTSystem.Server)
        {
            m_workspaceTemplatesManager.LoadInstanceWorkspaces(a_workspacesPath, a_erm);
        }
    }
    #endregion

    #region Declarations
    public static readonly string PT_SERVER_NAME = "Copilot";
    protected ISystemLogger m_errorReporter;
    private readonly WorkspaceTemplatesManager m_workspaceTemplatesManager;
    internal WorkspaceTemplatesManager WorkspaceTemplatesManager => m_workspaceTemplatesManager;
    protected readonly UserPermissionSetManager m_userPermissionSetManager;
    #endregion

    public UserPermissionSetManager PermissionSetManager => m_userPermissionSetManager;

    #region Construction
    internal UserManager(ISystemLogger a_erm, BaseIdGenerator a_idGen, string a_defaultWorkspacesPath)
        : base(a_idGen)
    {
        m_errorReporter = a_erm;
        m_userPermissionSetManager = new UserPermissionSetManager(a_idGen);

        m_workspaceTemplatesManager = new WorkspaceTemplatesManager();
        m_workspaceTemplatesManager.LoadInstanceWorkspaces(a_defaultWorkspacesPath, a_erm);
    }

    internal void InitNewUserManager(IPackageManager a_packageManager)
    {
        SynchronizePermissionModules(a_packageManager);

        User u = new (BaseId.ServerId, m_userPermissionSetManager.GetDefaultUserPermissionSet().Id, m_userPermissionSetManager.GetDefaultPlantPermissionSet().Id);
        u.Name = PT_SERVER_NAME;
        Add(u);

        u = new User(NextID(), m_userPermissionSetManager.GetDefaultUserPermissionSet().Id, m_userPermissionSetManager.GetDefaultPlantPermissionSet().Id);
        u.Name = "admin";

        Add(u);
    }
    #endregion

    #region User Edit Functions
    internal void CreateUser(IScenarioDataChanges a_dataChanges)
    {
        User u = new (NextID(), m_userPermissionSetManager.GetDefaultUserPermissionSet().Id, m_userPermissionSetManager.GetDefaultPlantPermissionSet().Id);
        Add(u);
        a_dataChanges.AuditEntry(new AuditEntry(u.Id, u), true);  // added=true
        a_dataChanges.UserChanges.AddedObject(u.Id);
    }

    public int GetAdminCount()
    {
        int count = 0;
        foreach (User u in this)
        {
            if (u.Active)
            {
                UserPermissionSet permissionSet = GetUserPermissionSetById(u.UserPermissionSetId);
                if (permissionSet.AdministerUsers)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Returns the first Administrator user in the list.
    /// </summary>
    /// <returns></returns>
    public User GetAdministrator()
    {
        foreach (User u in this)
        {
            if (!u.AppUser && u.Active && u.Id != BaseId.NULL_ID) //Null_Id is server user
            {
                UserPermissionSet permissionSet = GetUserPermissionSetById(u.UserPermissionSetId);
                if (permissionSet.AdministerUsers)
                {
                    return u;
                }
            }
        }

        return GetByIndex(0);
    }

    /// <summary>
    /// Returns all Administrator users in the list.
    /// </summary>
    /// <returns></returns>
    public List<User> GetAdministrators()
    {
        List<User> admins = new ();

        foreach (User u in this)
        {
            if (u.Active && u.Id != BaseId.NULL_ID) //Null_Id is server user
            {
                UserPermissionSet permissionSet = GetUserPermissionSetById(u.UserPermissionSetId);
                if (permissionSet.AdministerUsers)
                {
                    admins.Add(u);
                }
            }
        }

        return admins;
    }

    internal void AddCopy(IScenarioDataChanges a_dataChanges, UserCopyT a_copyT)
    {
        // Validate instigator has AdministerUsers permission
        User instigator = GetById(a_copyT.Instigator);
        if (instigator != null)
        {
            UserPermissionSet permSet = m_userPermissionSetManager.GetUserPermissionSet(instigator.UserPermissionSetId);
            if (!permSet.AdministerUsers)
            {
                return;
            }
        }

        User u = AddCopyInternal(a_copyT);
        a_dataChanges.AuditEntry(new AuditEntry(u.Id, u), true);  // added=true
        a_dataChanges.UserChanges.AddedObject(u.Id);
    }

    private User AddCopyInternal(UserCopyT t)
    {
        ValidateCopy(t);
        User u = GetById(t.originalId);
        User newUser = u.Clone();
        //newUser.ExternalId = GenerateNewUserExternalId(u.Id);
        return AddCopy(u, newUser, NextID());
    }

    internal void Delete(IScenarioDataChanges a_dataChanges, UserDeleteT a_deleteT)
    {
        // Validate instigator has AdministerUsers permission
        User instigator = GetById(a_deleteT.Instigator);
        if (instigator != null)
        {
            UserPermissionSet permSet = m_userPermissionSetManager.GetUserPermissionSet(instigator.UserPermissionSetId);
            if (!permSet.AdministerUsers)
            {
                return;
            }
        }

        ValidateExistence(a_deleteT.UserToBeDelete);
        User user = ValidateDelete(a_deleteT);
        a_dataChanges.AuditEntry(new AuditEntry(user.Id, user), false, true);  // deleted=true
        Remove(user);
        a_dataChanges.UserChanges.DeletedObject(user.Id);
    }

    internal void DeleteAll(UserBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        // Validate instigator has AdministerUsers permission
        User instigator = GetById(a_t.Instigator);
        if (instigator != null)
        {
            UserPermissionSet permSet = m_userPermissionSetManager.GetUserPermissionSet(instigator.UserPermissionSetId);
            if (!permSet.AdministerUsers)
            {
                return;
            }
        }

        for (int i = Count - 1; i >= 0; i--)
        {
            User u = this[i];
            //Don't delete the last Administrator or the instigator or if already marked as deleted.
            if (!(u.Id == a_t.Instigator || /*u.Administrator && */GetAdminCount() < 2))
            {
                UserDeleteT deleteT = new (u.Id);
                deleteT.Instigator = a_t.Instigator;
                Remove(u);
                a_dataChanges.UserChanges.DeletedObject(u.Id);
            }
        }
    }

    /// <summary>
    /// Updates one or more users already existing and managed by the user manager.
    /// </summary>
    /// <param name="a_editT"></param>
    /// <param name="a_dataChanges"></param>
    internal void UpdateUsers(UserEditT a_editT, IScenarioDataChanges a_dataChanges)
    {
        // Validate instigator has AdministerUsers permission
        User instigator = GetById(a_editT.Instigator);
        if (instigator != null)
        {
            UserPermissionSet permSet = m_userPermissionSetManager.GetUserPermissionSet(instigator.UserPermissionSetId);
            if (!permSet.AdministerUsers)
            {
                return;
            }
        }

        foreach (UserEdit edit in a_editT)
        {
            User user = edit.BaseIdSet ? GetById(edit.Id) : GetByExternalId(edit.ExternalId);
            if (user != null)
            {
                // Create audit entry BEFORE updating to capture pre-update state
                AuditEntry userAuditEntry = new AuditEntry(user.Id, user);
                user.Update(edit, m_userPermissionSetManager);
                a_dataChanges.AuditEntry(userAuditEntry);  // Compare() auto-detects changes
                a_dataChanges.UserChanges.UpdatedObject(user.Id);
            }
        }
    }

    // TODO: Could a seasoned core dev confirm this summary is accurate?
    /// <summary>
    /// Bulk updates/deletes a collection of users. Incoming users may not match existing data in the user manager
    /// (they may be e.g. pulled in externally from an import) and require addition reconciliation compared to <see cref="UpdateUsers" />.
    /// </summary>
    /// <param name="a_t"></param>
    /// <param name="a_dataChanges"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="PTValidationException"></exception>
    internal void BulkUpdateUsers(UserFieldDefinitionManager a_udfManager, UserT a_t, IScenarioDataChanges a_dataChanges)
    {
        Hashtable transmissionUserIds = new ();

        for (int i = 0; i < a_t.Count; i++)
        {
            UserT.User tUser = a_t[i];

            User u;
            if (tUser.IdSet)
            {
                u = GetById(tUser.Id);
                if (u == null)
                {
                    throw new ValidationException("2772", new object[] { tUser.Id });
                }
            }
            else
            {
                u = GetByExternalId(tUser.ExternalId);
                if (u != null)
                {
                    //When importing users by external Id, we don't update existing users. 
                    continue;
                }
            }

            if (u != null)
            {
                if (tUser.NameSet)
                {
                    //Validate, duplicate login values should not be allowed
                    User userByLogin = GetByName(tUser.Name);
                    if (userByLogin != null && userByLogin != u)
                    {
                        //User name is being updated to one that already exists
                        //TODO: This could be annoying, since the user that has this login name could also be changed during this import. We could check
                        throw new PTValidationException("2945", new object[] { userByLogin.Name, tUser.ExternalId });
                    }
                }

                // Create audit entry BEFORE updating to capture pre-update state
                AuditEntry userAuditEntry = new AuditEntry(u.Id, u);
                if (u.Update(a_udfManager, a_t, tUser, this))
                {
                    a_dataChanges.AuditEntry(userAuditEntry);  // Compare() auto-detects changes
                    a_dataChanges.UserChanges.UpdatedObject(u.Id);
                }

                if (!transmissionUserIds.Contains(u.Id))
                {
                    transmissionUserIds.Add(u.Id, null);
                }
            }
            else
            {
                //Validate new user has valid login. Don't allow multiple users with the same login ('Name' property).
                if (!tUser.NameSet)
                {
                    throw new PTValidationException("2944", new object[] { tUser.ExternalId });
                }

                User userByLogin = GetByName(tUser.Name);
                if (userByLogin != null)
                {
                    //Duplicate login names
                    throw new PTValidationException("2945", new object[] { userByLogin.Name, tUser.ExternalId });
                }

                u = new User(NextID(), m_userPermissionSetManager.GetDefaultUserPermissionSet().Id, m_userPermissionSetManager.GetDefaultPlantPermissionSet().Id);
                Add(u);
                a_dataChanges.UserChanges.AddedObject(u.Id);
                try
                {
                    u.Update(a_udfManager, a_t, tUser, this);
                    if (!transmissionUserIds.Contains(u.Id))
                    {
                        transmissionUserIds.Add(u.Id, null);
                    }
                }
                catch (Exception)
                {
                    Remove(u);
                    throw;
                }
            }
        }

        if (a_t.AutoDeleteMode)
        {
            for (int i = 0; i < Count; i++)
            {
                User u = this[i];
                if (!transmissionUserIds.Contains(u.Id))
                {
                    //BaseId.NULL_ID is server user; we don't want to remove the server user
                    if (GetAdminCount() <= 2 || u.Id == BaseId.NULL_ID)
                    {
                        continue;
                    }

                    Remove(u);
                    a_dataChanges.UserChanges.DeletedObject(u.Id);
                }
            }
        }
    }
    #endregion

    #region Transmissions
    internal User ValidateUserExistence(BaseId a_userId)
    {
        return ValidateExistence(a_userId);
    }

    private void ValidateCopy(UserCopyT t)
    {
        ValidateExistence(t.originalId);
    }

    internal User ValidateDelete(UserDeleteT t)
    {
        User u = GetById(t.UserToBeDelete);

        if (GetAdminCount() < 2 &&
            m_userPermissionSetManager.GetUserPermissionSet(u.UserPermissionSetId).AdministerUsers)
        // checks if the user being deleted is in an admin group
        {
            throw new TransmissionValidationException(t, "2658");
        }

        if (u.Id == t.Instigator)
        {
            throw new TransmissionValidationException(t, "2659");
        }

        return u;
    }

    internal void UpdateUserPermissionSet(UserPermissionSet a_permissionSet, bool a_isDefault, IScenarioDataChanges a_changes)
    {
        if (a_permissionSet.Id == BaseId.NEW_OBJECT_ID)
        {
            BaseId newPermissionSetId = IdGen.NextID();
            m_userPermissionSetManager.AddUserPermissionSet(a_permissionSet, newPermissionSetId, a_isDefault);
            a_changes.PermissionChanges.AddedObject(newPermissionSetId);
            a_changes.AuditEntry(new AuditEntry(newPermissionSetId, a_permissionSet), true);
        }
        else
        {
            AuditEntry permissionAuditEntry = new AuditEntry(a_permissionSet.Id, a_permissionSet);
            m_userPermissionSetManager.UpdateUserPermission(a_permissionSet, a_isDefault);
            a_changes.PermissionChanges.UpdatedObject(a_permissionSet.Id);
            a_changes.AuditEntry(permissionAuditEntry);
        }
    }

    internal void DeleteUserPermissionSet(UserPermissionSet a_permissionSet, BaseId a_replacementId, IScenarioDataChanges a_changes)
    {
        foreach (User user in this)
        {
            if (user.UserPermissionSetId == a_permissionSet.Id)
            {
                a_changes.UserChanges.UpdatedObject(user.Id);
                user.UserPermissionSetId = a_replacementId;
            }
        }

        a_changes.AuditEntry(new AuditEntry(a_permissionSet.Id, a_permissionSet), false, true);
        a_changes.PermissionChanges.DeletedObject(a_permissionSet.Id);
        m_userPermissionSetManager.DeleteUserPermissionSet(a_permissionSet.Id, a_replacementId);
    }

    internal void UpdateOrAddUserPlantPermissionSet(PlantPermissionSet a_permissionSet, bool a_isDefault, IScenarioDataChanges a_changes)
    {
        if (a_permissionSet.Id == BaseId.NEW_OBJECT_ID)
        {
            PlantPermissionSet newPermissionSet = m_userPermissionSetManager.AddPlantPermissionSet(a_permissionSet, a_isDefault);
            a_changes.PlantPermissionChanges.AddedObject(newPermissionSet.Id);
            a_changes.AuditEntry(new AuditEntry(newPermissionSet.Id, newPermissionSet), true);
        }
        else
        {
            AuditEntry plantPermissionAuditEntry = new AuditEntry(a_permissionSet.Id, a_permissionSet);
            m_userPermissionSetManager.UpdateUserPlantPermissionSet(a_permissionSet, a_isDefault);
            a_changes.PlantPermissionChanges.UpdatedObject(a_permissionSet.Id);
            a_changes.AuditEntry(plantPermissionAuditEntry);
        }
    }

    internal void DeleteUserPlantPermissionSet(PlantPermissionSet a_permissionSet, BaseId a_replacementId, IScenarioDataChanges a_changes)
    {
        foreach (User user in this)
        {
            if (user.PlantPermissionsId == a_permissionSet.Id)
            {
                user.PlantPermissionsId = a_replacementId;
                a_changes.UserChanges.UpdatedObject(user.Id);
            }
        }

        a_changes.AuditEntry(new AuditEntry(a_permissionSet.Id, a_permissionSet), false, true);
        m_userPermissionSetManager.DeleteUserPlantPermissionSet(a_permissionSet.Id, a_replacementId);
        a_changes.PlantPermissionChanges.DeletedObject(a_permissionSet.Id);
    }
    #endregion

    #region UserManagerEvent Caller
    internal class UserManagerEventCaller
    {
        private readonly List<UserEventArgsBase> m_eventsToFire = new ();

        internal void CallEvents(UserManagerEvents a_umEvents)
        {
            foreach (UserEventArgsBase eventToFire in m_eventsToFire)
            {
                eventToFire.CallEvent(a_umEvents);
            }
        }

        internal void AddEvent(UserEventArgsBase a_eventArgs)
        {
            m_eventsToFire.Add(a_eventArgs);
        }

        internal abstract class UserEventArgsBase
        {
            internal UserEventArgsBase(User a_u, UserManager a_um, PTTransmissionBase a_t)
            {
                m_u = a_u;
                m_um = a_um;
                m_t = a_t;
            }

            protected User m_u;
            protected UserManager m_um;
            protected PTTransmissionBase m_t;

            public abstract void CallEvent(UserManagerEvents a_umEvents);
        }

        internal class UserLogOnArgs : UserEventArgsBase
        {
            internal UserLogOnArgs(User a_u, UserManager a_um, UserLogOnT a_t)
                : base(a_u, a_um, a_t) { }

            public override void CallEvent(UserManagerEvents a_umEvents)
            {
                m_um.CacheServerConnections();
                a_umEvents.FireUserLogOnEvent(m_u, m_um, (UserLogOnT)m_t);
            }
        }

        internal class UserChatArgs : UserEventArgsBase
        {
            internal UserChatArgs(User a_u, UserChatT a_t)
                : base(a_u, null, a_t) { }

            public override void CallEvent(UserManagerEvents a_umEvents)
            {
                a_umEvents.FireUserChatEvent(m_u, (UserChatT)m_t);
            }
        }

        internal class UserAdminLogoffArgs : UserEventArgsBase
        {
            internal UserAdminLogoffArgs(User a_u, UserAdminLogOffT a_t)
                : base(a_u, null, a_t) { }

            public override void CallEvent(UserManagerEvents a_umEvents)
            {
                a_umEvents.FireUserAdminLogoffEvent(m_u, (UserAdminLogOffT)m_t);
            }
        }

        internal class UserPermissionsUpdatedArgs : UserEventArgsBase
        {
            internal UserPermissionsUpdatedArgs(UserManager a_um)
                : base(null, a_um, null) { }

            public override void CallEvent(UserManagerEvents a_umEvents)
            {
                a_umEvents.FireUserPermissionsUpdatedEvent();
            }
        }
    }
    #endregion

    /// <summary>
    /// Use this instead of GetByName().
    /// </summary>
    /// <returns>The first active user with the specified name, or null</returns>
    public User GetUserByName(string a_name)
    {
        for (int i = 0; i < Count; i++)
        {
            User u = this[i];
            if (u.Active && string.Compare(u.Name, a_name, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return u;
            }
        }

        return null;
    }

    private User GetActiveUserById(BaseId a_id)
    {
        for (int i = 0; i < Count; i++)
        {
            User user = this[i];
            if (user.Active && user.Id == a_id)
            {
                return user;
            }
        }

        return null;
    }

    public PlantPermissionSet GetPlantPermissionSetByName(string a_name)
    {
        return m_userPermissionSetManager.GetPlantPermissionSetByName(a_name);
    }

    public PlantPermissionSet GetPlantPermissionSetById(BaseId a_templateId)
    {
        return m_userPermissionSetManager.GetPlantPermissionSet(a_templateId);
    }

    public List<PlantPermissionSet> GetPlantPermissionSets()
    {
        return m_userPermissionSetManager.GetPlantPermissions();
    }

    public UserPermissionSet GetUserPermissionSetByName(string a_name)
    {
        return m_userPermissionSetManager.GetUserPermissionSetByName(a_name);
    }

    public UserPermissionSet GetUserPermissionSetById(BaseId a_templateId)
    {
        return m_userPermissionSetManager.GetUserPermissionSet(a_templateId);
    }

    public List<UserPermissionSet> GetUserPermissionSets()
    {
        return m_userPermissionSetManager.GetUserPermissions();
    }

    public Dictionary<string, byte[]> GetWorkspaceDictionary()
    {
        return m_workspaceTemplatesManager.GetWorkspaceDictionary();
    }

    public IEnumerable<byte[]> GetDefaultInstanceWorkspaces()
    {
        return m_workspaceTemplatesManager.DefaultWorkspaceCollection;
    }

    public bool ValidateScenarioEditPermissions(BaseId a_userId, ScenarioPermissionSettings a_scenarioPermissionSettings)
    {
        UserPermissionSet userPermissionSet = m_userPermissionSetManager.GetUserPermissionSet(a_userId);
        return a_scenarioPermissionSettings.CanUserEdit(a_userId, userPermissionSet.Id);
    }

    public BaseId GetDefaultAdministrationGroupId()
    {
        return m_userPermissionSetManager.DefaultAdminGroupBaseId;
    }

    public BaseId GetDefaultViewOnlyGroupId()
    {
        return m_userPermissionSetManager.DefaultViewOnlyGroupBaseId;
    }

    public BaseId FindUserPermissionSetIdUsingUserId(BaseId a_userId)
    {
        User user = GetById(a_userId);
        if (user != null)
        {
            return user.UserPermissionSetId;
        }
        else
        {
            return BaseId.NULL_ID;
        }
    }

    public (PTUserScenarioStartupPreferences.EScenarioStartupPreferenceType, BaseId) GetUserScenarioStartupInformation(BaseId a_userId)
    {
        User user = GetById(a_userId);
        if (user.UserPreferenceInfo == null)
        {
            return (PTUserScenarioStartupPreferences.EScenarioStartupPreferenceType.LoadLastSession, BaseId.NULL_ID);
        }

        PTUserScenarioStartupPreferences userScenarioStartupPreferences = new PTUserScenarioStartupPreferences();
        ScenarioPlanningPreferences userScenarioPlanningPreferences = new ScenarioPlanningPreferences();
        using (BinaryMemoryReader reader = new (user.UserPreferenceInfo))
        {
            GenericSettingSaver userPreferences = new GenericSettingSaver(reader);
            userScenarioStartupPreferences = userPreferences.LoadSetting(userScenarioStartupPreferences);
            userScenarioPlanningPreferences = userPreferences.LoadSetting(userScenarioPlanningPreferences);
        }
        return (userScenarioStartupPreferences.ScenarioStartupPreferenceType, userScenarioPlanningPreferences.LastActiveScenarioId);
    }

    public object GetUserById(BaseId a_userId)
    {
        return GetById(a_userId);
    }

    public string GetUserName(BaseId a_userId)
    {
        User user = GetActiveUserById(a_userId);
        if (user != null)
        {
            return user.Name;
        }

        return "Unknown";
    }

    #region ICopyTable
    public override Type ElementType => typeof(User);
    #endregion

    /// <summary>
    /// Returns the server admin user
    /// </summary>
    public User GetServerAdmin(string a_preferredName)
    {
        //Get a user from one of the admin permission groups
        List<UserPermissionSet> permissionSets = m_userPermissionSetManager.GetUserPermissions();
        foreach (UserPermissionSet permissionSet in permissionSets)
        {
            if (permissionSet.AdministerUsers)
            {
                //Find the preferred user
                User byName = GetByName(a_preferredName);
                if (byName != null && byName.Active && byName.UserPermissionSetId == permissionSet.Id)
                {
                    return byName;
                }

                //Preferred user not found, find the first admin
                foreach (User user in this)
                {
                    if (user.Active && user.UserPermissionSetId == permissionSet.Id)
                    {
                        return user;
                    }
                }
            }
        }

        throw new PTValidationException("User Permissions are corrupt. There is no admin user");
    }

    internal int CountActiveAdminUsers()
    {
        int count = 0;
        foreach (User user in this)
        {
            if (user.Active && user.Id != BaseId.NULL_ID) //Null_Id is server user
            {
                if (m_userPermissionSetManager.GetUserPermissionSet(user.UserPermissionSetId).AdministerUsers)
                {
                    count++;
                }
            }
        }

        #if DEBUG
        if (count <= 0)
        {
            throw new PTException("Invalid active admin users. This should always be greater than 0");
        }
        #endif

        return count;
    }

    public bool IsUserAdmin(BaseId a_id)
    {
        User user = GetById(a_id);
        return m_userPermissionSetManager.GetUserPermissionSet(user.UserPermissionSetId).AdministerUsers;
    }

    public bool CanUserManageAllScenarios(BaseId a_id)
    {
        User user = GetById(a_id);
        return m_userPermissionSetManager.GetUserPermissionSet(user.UserPermissionSetId).CanManageScenarios;
    }

    private bool firstLoadComplete;
    /// <summary>
    /// Gets number of connections to server for a_user
    /// </summary>
    /// <param name="a_user"></param>
    /// <returns></returns>
    public int GetServerConnections(User a_user)
    {
        lock (m_lock)
        {
            if (!firstLoadComplete)
            {
                firstLoadComplete = true;
                CacheServerConnections();
            }

            if (m_connectedUserDatas == null || m_connectedUserDatas.Length == 0)
            {
                // Load attempted, but no data. Return a dummy val and hope the next regular poll does better.
                return -1;
            }

            ConnectedUserData[] thisUserData = m_connectedUserDatas
                                               .Where(d => d.Id == a_user.Id.Value)
                                               .ToArray();

            return thisUserData.Length == 1 ? thisUserData[0].ActiveConnections : 0;
        }
    }

    private ConnectedUserData[] m_connectedUserDatas;
    private readonly object m_lock = new ();



    private System.Timers.Timer m_connectedUserDataPollTimer;
    private void SetupServerConnectionsPollingTimer()
    {
        if (m_connectedUserDataPollTimer == null)
        {
            m_connectedUserDataPollTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            m_connectedUserDataPollTimer.Elapsed += new System.Timers.ElapsedEventHandler((sender, e) => CacheServerConnections());
            m_connectedUserDataPollTimer.AutoReset = true;
            m_connectedUserDataPollTimer.Enabled = true;
        }
    }

    /// <summary>
    /// Caches the connections when user logs on or logs off
    /// </summary>
    private void CacheServerConnections()
    {
        lock (m_lock)
        {
            SetupServerConnectionsPollingTimer();
            m_connectedUserDatas = SystemController.ClientSession.MakeGetRequest<ConnectedUserData[]>("GetLoggedInUserData", "api/SystemService");
        }
    }

    public Dictionary<long, UserPermissionDto> GetScenarioPermissionsForUser(User a_user)
    {
        lock (m_userScenarioPermissionsLock)
        {
            if (m_userScenarioPermissions.IsEmpty)
            {
                // Lazy load on first use
                CacheUserScenarioPermissionsFromServer();
            }

            if (m_userScenarioPermissions.TryGetValue(a_user.Id.Value, out Dictionary<long, UserPermissionDto>  scenarioPermissionsForUser))
            {
                return scenarioPermissionsForUser;
            }
            else
            {
                // Pull from server and reattempt to find
                CacheUserScenarioPermissionsFromServer();

                if (m_userScenarioPermissions.TryGetValue(a_user.Id.Value, out scenarioPermissionsForUser))
                {
                    return m_userScenarioPermissions[a_user.Id.Value];
                }
                else
                {
                    // TODO: This method is currently only being used to populate readonly grid data. If its use extends, we should actually throw here.
                    DebugException.ThrowInDebug("Attempted to get a user permission set for a user that does not exist on the server.");

                    return new Dictionary<long, UserPermissionDto>();
                }
            }
        }
    }

    private readonly ConcurrentDictionary<long, Dictionary<long, UserPermissionDto>> m_userScenarioPermissions = new ();
    private readonly object m_userScenarioPermissionsLock = new();

    /// <summary>
    /// Gets and caches all UserScenarioPermissions from the server. This should be called if a fundamental permissions change was made across users, e.g. a permission group is altered.
    /// </summary>
    private void CacheUserScenarioPermissionsFromServer()
    {
        SetupScenarioPermissionsPollingTimer();

        ScenarioPermissionsResponse allUserScenarioPermissions;
        lock (m_userScenarioPermissionsLock)
        {
            allUserScenarioPermissions = SystemController.ClientSession.MakeGetRequest<ScenarioPermissionsResponse>(
                "GetScenarioPermissions", null);

            // Structure permissions by user.
            // This is more processing than we should have to do to an API response, but it needs to be built using Group Permissions -
            // we can't just get ScenarioPermissionSettings.GetUserPermissionSets() as it doesn't reflect the full logic of ScenarioPermissions.
            // TODO: Restructure API to get each user's actual scenario permission, and *then* build the tree structure we're reordering here, so it could be ordered only once, by use case.
            // TODO: This would also allow us to expose accurate single-user apis
            Dictionary<long, Dictionary<long, UserPermissionDto>> scenarioPermissionsByUserId = new ();

            foreach (ScenarioPermissionSet scenarioPermissionSet in allUserScenarioPermissions.ScenarioPermissionSets.Values)
            {
                foreach (GroupedScenarioPermissions groupedScenarioPermissions in scenarioPermissionSet.GroupScenarioPermissions)
                {
                    foreach (UserPermissionDto userPermissionsForScenario in groupedScenarioPermissions.UserPermissions.Values)
                    {
                        if (!scenarioPermissionsByUserId.ContainsKey(userPermissionsForScenario.UserId))
                        {
                            // Add new dict for this user
                            scenarioPermissionsByUserId[userPermissionsForScenario.UserId] = new Dictionary<long, UserPermissionDto>();
                        }
                        
                        // Add user's permission for this scenario
                        scenarioPermissionsByUserId[userPermissionsForScenario.UserId].Add(scenarioPermissionSet.ScenarioId, userPermissionsForScenario);
                    }
                }
            }

            // Now that permissions are grouped by user, cache them.
            foreach (KeyValuePair<long, Dictionary<long, UserPermissionDto>> userPermissions in scenarioPermissionsByUserId)
            {
                m_userScenarioPermissions.AddOrUpdate(userPermissions.Key, userPermissions.Value,
                    (key, oldValue) => userPermissions.Value);
            }
        }
    }

    private System.Timers.Timer m_userPermissionsPollTimer;

    private void SetupScenarioPermissionsPollingTimer()
    {
        if (m_userPermissionsPollTimer == null)
        {
            m_userPermissionsPollTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            m_userPermissionsPollTimer.Elapsed += new System.Timers.ElapsedEventHandler((sender, e) => CacheUserScenarioPermissionsFromServer());
            m_userPermissionsPollTimer.AutoReset = true;
            m_userPermissionsPollTimer.Enabled = true;
        }
    }

    public void EnableJitLoginForUser(string a_userName)
    {
        User userByName = GetUserByName(a_userName);
        if (userByName == null)
        {
            userByName = new User(NextID(), m_userPermissionSetManager.GetDefaultUserPermissionSet().Id, m_userPermissionSetManager.GetDefaultPlantPermissionSet().Id);
            userByName.Name = a_userName;
            userByName.Active = false;
            Add(userByName);
        }

        userByName.UserLocked = false;

        userByName.JitLoginExpiration = DateTime.UtcNow.Add(TimeSpan.FromHours(1));
    }

    /// <summary>
    /// Add and update permissions that were loaded from packages.
    /// </summary>
    /// <param name="a_packageManager"></param>
    internal void SynchronizePermissionModules(IPackageManager a_packageManager)
    {
        List<(string, string)> permissionList = new ();
        foreach (IPermissionModule module in a_packageManager.GetPermissionModules())
        {
            foreach (IUserPermissionElement userPermission in module.GetUserPermissions())
            {
                permissionList.Add((userPermission.GroupKey, userPermission.PermissionKey));
            }
        }

        if (m_userPermissionSetManager.GetUserPermissions().Any())
        {
            m_userPermissionSetManager.ValidateLoadedPackagePermissions(permissionList);
        }
        else
        {
            //For new UserManagers
            m_userPermissionSetManager.CreateDefaultUserPermissionSets(permissionList);
        }
    }
}
