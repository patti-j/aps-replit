using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.Extensions;

namespace PT.SchedulerDefinitions.PermissionTemplates;

public class UserPermissionSetManager : IPTSerializable
{
    private readonly BaseIdGenerator m_idGen;

    #region IPTSerializable
    public UserPermissionSetManager(IReader a_reader, BaseIdGenerator a_idGen)
    {
        m_idGen = a_idGen;
        if (a_reader.VersionNumber >= 12000)
        {
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_plantPermissions.Add(new PlantPermissionSet(a_reader));
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_userPermissions.Add(new UserPermissionSet(a_reader));
            }

            m_defaultUserPermissionSetId = new BaseId(a_reader);
            m_defaultPlantPermissionSetId = new BaseId(a_reader);
        }
        else
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                m_plantPermissions.Add(new PlantPermissionSet(a_reader));
            }

            //Version 716
            if (m_plantPermissions.Count == 0)
            {
                PlantPermissionSet initialPlantSet = new (a_idGen.NextID());
                m_plantPermissions.Add(initialPlantSet);
                m_defaultPlantPermissionSetId = initialPlantSet.Id;
            }
            else
            {
                m_defaultPlantPermissionSetId = m_plantPermissions[0].Id;
            }

            UserPermissionSet initialSet = new (a_idGen.NextID());
            m_userPermissions.Add(initialSet);
            m_defaultUserPermissionSetId = initialSet.Id;
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_plantPermissions);
        a_writer.Write(m_userPermissions);
        m_defaultUserPermissionSetId.Serialize(a_writer);
        m_defaultPlantPermissionSetId.Serialize(a_writer);
    }

    public BaseId DefaultAdminGroupBaseId;
    public BaseId DefaultViewOnlyGroupBaseId;

    public int UniqueId => UNIQUE_ID;

    private const int UNIQUE_ID = 826;
    #endregion

    public UserPermissionSetManager(BaseIdGenerator a_idGen)
    {
        m_idGen = a_idGen;
        // TODO: Since I've added DefaultAdminGroupBaseID, maybe it's safer to use that instead now?
        // I think the first set being the default admin group is dependent on the order in CreateDefaultUserPermissionSets()

        CreateDefaultPlantPermissionSets(a_idGen);
        m_defaultPlantPermissionSetId = m_plantPermissions[0].Id;
    }

    #region Default PermissionSet Allowed Groups
    // Currently, these two sets are the only ones used across all default UserPermissionSet permissions. More can be added in future if need be.
    //TODO: Figure out a better way of doing this other than hard coding. Can't reference PermissionsGroupConstants here though...

    /// <summary>
    /// The set of all permission groups a UserPermissionSet might allow by default. This is used by privileged permission sets.
    /// </summary>
    private readonly string[] m_allowedPermissionGroups_all = { "Scheduling", "Planning", "Modeling", "Import", "Publish", "Undo Redo", "Plants", "Miscellaneous", "Boards", "Capacity", "Main Bar Controls" };

    /// <summary>
    /// The set of permission groups for a UserPermissionSet that are allowed by default for a viewonly permission set (currently ViewOnly and Analytics).
    /// </summary>
    private readonly string[] m_allowedPermissionGroups_boardOnly = { "Boards" };
    #endregion

    private void CreateDefaultUserPermissionSets(BaseIdGenerator a_idGen, List<(string, string)> a_permissionList)
    {
        UserPermissionSet admin = new (a_idGen.NextID());
        m_userPermissions.Add(admin);
        DefaultAdminGroupBaseId = admin.Id;
        AddDefaultGroupPermissionsForPermissionSet(admin, a_permissionList, m_allowedPermissionGroups_all);

        CreateMasterSchedulerGroup(a_idGen, a_permissionList);
        CreateSchedulerGroup(a_idGen, a_permissionList);
        CreatePlannerGroup(a_idGen, a_permissionList);
        CreateAnalyticsGroup(a_idGen, a_permissionList);
        CreateViewOnlyGroup(a_idGen, a_permissionList);

        m_defaultUserPermissionSetId = m_userPermissions[0].Id; //The first set should be the default admin group
    }

    private void CreateMasterSchedulerGroup(BaseIdGenerator a_idGen, List<(string, string)> a_permissionList)
    {
        UserPermissionSet masterScheduler = new (a_idGen.NextID())
        {
            Name = "Master Scheduler".Localize(),
            AdministerUsers = false,
            AllowChangesThatOverrideOtherUserActions = true,
            AutoGrantNewPermissions = true,
            MaintainSystemSettings = true,
            CanManageScenarios = true
        };

        AddDefaultGroupPermissionsForPermissionSet(masterScheduler, a_permissionList, m_allowedPermissionGroups_all);
        //TODO: we can't reference UI projects from scheduler projects
        //masterScheduler.AddPermission(UserPermissionKeys.Users, false);

        m_userPermissions.Add(masterScheduler);
    }

    private void CreateSchedulerGroup(BaseIdGenerator a_idGen, List<(string, string)> a_permissionList)
    {
        UserPermissionSet scheduler = new (a_idGen.NextID())
        {
            Name = "Scheduler".Localize(),
            AdministerUsers = false,
            AllowChangesThatOverrideOtherUserActions = true,
            AutoGrantNewPermissions = false,
            MaintainSystemSettings = false,
            CanManageScenarios = true
        };

        AddDefaultGroupPermissionsForPermissionSet(scheduler, a_permissionList, m_allowedPermissionGroups_all);

        //TODO: we can't reference UI projects from scheduler projects
        //scheduler.AddPermission(UserPermissionKeys.Users, false);
        //scheduler.AddPermission(UserPermissionKeys.ShowUnavailableUndoActions, false);
        //scheduler.AddPermission(UserPermissionKeys.ControlAddIns, false);
        //scheduler.AddPermission(UserPermissionKeys.ImportMapping, false);

        m_userPermissions.Add(scheduler);
    }

    private void CreatePlannerGroup(BaseIdGenerator a_idGen, List<(string, string)> a_permissionList)
    {
        UserPermissionSet planner = new (a_idGen.NextID())
        {
            Name = "Planner".Localize(),
            AdministerUsers = false,
            AllowChangesThatOverrideOtherUserActions = true,
            AutoGrantNewPermissions = false,
            MaintainSystemSettings = false,
            CanManageScenarios = true
        };

        AddDefaultGroupPermissionsForPermissionSet(planner, a_permissionList, m_allowedPermissionGroups_all);

        //TODO: we can't reference UI projects from scheduler projects
        //planner.AddPermission(UserPermissionKeys.Users, false);
        //planner.AddPermission(UserPermissionKeys.ShowUnavailableUndoActions, false);
        //planner.AddPermission(UserPermissionKeys.ControlAddIns, false);
        //planner.AddPermission(UserPermissionKeys.ImportMapping, false);

        //TODO: Maybe add more permission elements for editing specific objects like sales orders, purchase orders, etc. Permissions that would set this group apart from the Scheduler group.

        m_userPermissions.Add(planner);
    }

    private void CreateAnalyticsGroup(BaseIdGenerator a_idGen, List<(string, string)> a_permissionList)
    {
        UserPermissionSet analytics = new (a_idGen.NextID())
        {
            Name = "Analytics".Localize(),
            AdministerUsers = false,
            AllowChangesThatOverrideOtherUserActions = false,
            AutoGrantNewPermissions = false,
            MaintainSystemSettings = false,
            CanManageScenarios = false
        };

        AddDefaultGroupPermissionsForPermissionSet(analytics, a_permissionList, m_allowedPermissionGroups_boardOnly);

        //TODO: we can't reference UI projects from scheduler projects
        //analytics.AddPermission(UserPermissionKeys.Analytics, true);
        //analytics.AddPermission(UserPermissionKeys.PublishToAnalytics, true);
        //analytics.AddPermission(UserPermissionKeys.ScenarioHistory, true);
        //analytics.AddPermission(UserPermissionKeys.Metrics, true);
        //analytics.AddPermission(UserPermissionKeys.Forecast, true);
        //analytics.AddPermission(UserPermissionKeys.BufferManagement, true);
        //analytics.AddPermission(UserPermissionKeys.ReportDesigner, true);

        m_userPermissions.Add(analytics);
    }

    private void CreateViewOnlyGroup(BaseIdGenerator a_idGen, List<(string, string)> a_permissionList)
    {
        UserPermissionSet viewOnly = new (a_idGen.NextID())
        {
            Name = "View Only".Localize(),
            AdministerUsers = false,
            AllowChangesThatOverrideOtherUserActions = false,
            AutoGrantNewPermissions = false,
            MaintainSystemSettings = false,
            CanManageScenarios = false
        };

        AddDefaultGroupPermissionsForPermissionSet(viewOnly, a_permissionList, m_allowedPermissionGroups_boardOnly);

        DefaultViewOnlyGroupBaseId = viewOnly.Id;

        m_userPermissions.Add(viewOnly);
    }

    /// <summary>
    /// Explicitly sets all permissions loaded from packages, filtering out groups of these permissions which the UserPermissionSet is not configured to allow.
    /// </summary>
    /// <param name="a_permissionSet">The <see cref="UserPermissionSet" /> to initialize permissions for.</param>
    /// <param name="a_permissionList">All permissions loaded from packages</param>
    /// <param name="a_allowedGroupNames">The collection of Groups that this user permission set should have allowed.</param>
    private static void AddDefaultGroupPermissionsForPermissionSet(UserPermissionSet a_permissionSet, List<(string, string)> a_permissionList, string[] a_allowedGroupNames)
    {
        foreach ((string group, string key) permission in a_permissionList)
        {
            bool allowed = a_allowedGroupNames.Contains(permission.group);

            a_permissionSet.AddPermission(permission.key, allowed);
        }
    }

    private void CreateDefaultPlantPermissionSets(BaseIdGenerator a_idGen)
    {
        PlantPermissionSet initialPlantSet = new (a_idGen.NextID());
        m_plantPermissions.Add(initialPlantSet);
    }

    private readonly List<PlantPermissionSet> m_plantPermissions = new ();
    private readonly List<UserPermissionSet> m_userPermissions = new ();

    public UserPermissionSet GetUserPermissionSet(BaseId a_templateId)
    {
        foreach (UserPermissionSet permission in m_userPermissions)
        {
            if (permission.Id == a_templateId)
            {
                return permission;
            }
        }

        return GetDefaultUserPermissionSet();
    }

    public UserPermissionSet GetUserPermissionSetByName(string a_name)
    {
        foreach (UserPermissionSet permission in m_userPermissions)
        {
            if (permission.Name == a_name)
            {
                return permission;
            }
        }

        return null;
    }

    public List<PlantPermissionSet> GetNewPlantPermissions()
    {
        List<PlantPermissionSet> sets = new ();
        foreach (PlantPermissionSet permission in m_plantPermissions)
        {
            if (permission.Id == BaseId.NULL_ID)
            {
                sets.Add(permission);
            }
        }

        return sets;
    }

    public PlantPermissionSet AddPlantPermissionSet(PlantPermissionSet a_permissionSet, bool a_isDefault)
    {
        if (GetPlantPermissionSetByName(a_permissionSet.Name) != null)
        {
            throw new PTValidationException("3065", new object[] { a_permissionSet.Name });
        }

        BaseId nextId = m_idGen.NextID();

        PlantPermissionSet newPlantPermissionSet = new (a_permissionSet, nextId);
        m_plantPermissions.Add(newPlantPermissionSet);

        if (a_isDefault)
        {
            m_defaultPlantPermissionSetId = nextId;
        }

        return newPlantPermissionSet;
    }

    public void AddUserPermissionSet(UserPermissionSet a_permissionSet, BaseId a_nextId, bool a_isDefault)
    {
        UserPermissionSet newUserSet = new (a_permissionSet, a_nextId);
        m_userPermissions.Add(newUserSet);

        if (a_isDefault)
        {
            m_defaultUserPermissionSetId = a_nextId;
        }
    }

    public Dictionary<BaseId, string> GetPlantPermissionDescriptions()
    {
        Dictionary<BaseId, string> dictionary = new ();
        foreach (PlantPermissionSet permission in m_plantPermissions)
        {
            dictionary.Add(permission.Id, permission.Name);
        }

        return dictionary;
    }

    public PlantPermissionSet GetPlantPermissionSet(BaseId a_templateId)
    {
        foreach (PlantPermissionSet permission in m_plantPermissions)
        {
            if (permission.Id == a_templateId)
            {
                return permission;
            }
        }

        return new PlantPermissionSet(BaseId.NULL_ID);
    }

    public PlantPermissionSet GetPlantPermissionSetByName(string a_name)
    {
        foreach (PlantPermissionSet permission in m_plantPermissions)
        {
            if (permission.Name == a_name)
            {
                return permission;
            }
        }

        return null;
    }

    public List<PlantPermissionSet> GetPlantPermissions()
    {
        return m_plantPermissions.ShallowCopy();
    }

    public List<UserPermissionSet> GetUserPermissions()
    {
        return m_userPermissions.ShallowCopy();
    }

    public void UpdateUserPermission(UserPermissionSet a_permissionSet, bool a_isDefault)
    {
        if (a_isDefault)
        {
            m_defaultUserPermissionSetId = a_permissionSet.Id;
        }

        foreach (UserPermissionSet permissionSet in m_userPermissions)
        {
            if (permissionSet.Id == a_permissionSet.Id)
            {
                permissionSet.Update(a_permissionSet);
                return;
            }
        }

        throw new PTValidationException("4461", new object[] { a_permissionSet.Id });
    }

    public void DeleteUserPermissionSet(BaseId a_permissionSetId, BaseId a_replacementId)
    {
        foreach (UserPermissionSet permissionSet in m_userPermissions)
        {
            if (permissionSet.Id == a_permissionSetId)
            {
                if (m_userPermissions.Count <= 1)
                {
                    throw new PTValidationException("4462", new object[] { a_permissionSetId });
                }

                if (permissionSet.AdministerUsers &&
                    GetAdministrateUsersPermissionSetCount() <= 1)
                {
                    throw new PTValidationException("4463", new object[] { a_permissionSetId });
                }

                if (DefaultUserPermissionSetId == permissionSet.Id)
                {
                    m_defaultUserPermissionSetId = a_replacementId;
                }

                m_userPermissions.Remove(permissionSet);
                return;
            }
        }

        throw new PTValidationException("4464", new object[] { a_permissionSetId });
    }

    public UserPermissionSet GetDefaultUserPermissionSet()
    {
        foreach (UserPermissionSet permissionSet in m_userPermissions)
        {
            if (permissionSet.Id == m_defaultUserPermissionSetId)
            {
                return permissionSet;
            }
        }

        throw new PTException("4465");
    }

    /// <summary>
    /// Iterates through the collection of UserPermissionSet to see how many of them
    /// can administrate users (it's a bool property of the object).
    /// </summary>
    /// <returns>int, count of UserPermissionSets with AdministerUsers set to true</returns>
    private int GetAdministrateUsersPermissionSetCount()
    {
        int count = 0;
        foreach (UserPermissionSet userPermissionSet in m_userPermissions)
        {
            if (userPermissionSet.AdministerUsers)
            {
                count++;
            }
        }

        return count;
    }

    private BaseId m_defaultUserPermissionSetId;

    public BaseId DefaultUserPermissionSetId => m_defaultUserPermissionSetId;

    public void UpdateUserPlantPermissionSet(PlantPermissionSet a_permissionSet, bool a_isDefault)
    {
        if (a_isDefault)
        {
            m_defaultPlantPermissionSetId = a_permissionSet.Id;
        }

        foreach (PlantPermissionSet permissionSet in m_plantPermissions)
        {
            if (permissionSet.Id == a_permissionSet.Id)
            {
                permissionSet.Update(a_permissionSet);
                return;
            }
        }

        throw new PTValidationException("4461", new object[] { a_permissionSet.Id });
    }

    public void DeleteUserPlantPermissionSet(BaseId a_permissionSetId, BaseId a_replacementId)
    {
        foreach (PlantPermissionSet permissionSet in m_plantPermissions)
        {
            if (permissionSet.Id == a_permissionSetId)
            {
                if (m_plantPermissions.Count <= 1)
                {
                    throw new PTValidationException("4462", new object[] { a_permissionSetId });
                }

                if (DefaultPlantPermissionSetId == permissionSet.Id)
                {
                    m_defaultPlantPermissionSetId = a_replacementId;
                }

                m_plantPermissions.Remove(permissionSet);
                return;
            }
        }

        throw new PTValidationException("4464", new object[] { a_permissionSetId });
    }

    public PlantPermissionSet GetDefaultPlantPermissionSet()
    {
        foreach (PlantPermissionSet permissionSet in m_plantPermissions)
        {
            if (permissionSet.Id == m_defaultPlantPermissionSetId)
            {
                return permissionSet;
            }
        }

        throw new PTException("4465");
    }

    private BaseId m_defaultPlantPermissionSetId;

    public BaseId DefaultPlantPermissionSetId => m_defaultPlantPermissionSetId;

    public void CreateDefaultUserPermissionSets(List<(string, string)> a_permissionList)
    {
        CreateDefaultUserPermissionSets(m_idGen, a_permissionList);
    }

    /// <summary>
    /// Sync the permissions loaded from packages into this group
    /// They will be auto-granted based on the AutoGrantNewPermissions setting
    /// </summary>
    public void ValidateLoadedPackagePermissions(IEnumerable<(string Group, string Key)> a_permissions)
    {
        foreach (UserPermissionSet permissionSet in m_userPermissions)
        {
            permissionSet.ValidateLoadedPackagePermissions(a_permissions);
        }
    }
}