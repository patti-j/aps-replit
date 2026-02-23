using PT.APSCommon;
using PT.PackageDefinitions.PackageInterfaces;

namespace PT.PackageDefinitions.Settings;

public class ScenarioPermissionSettings : ISettingData, ICloneable
{
    #region IPTSerializable
    public ScenarioPermissionSettings(IReader a_reader)
    {
        // This should have been 12040, but I accidentally skipped over two numbers. 
        if (a_reader.VersionNumber >= 12042)
        {
            m_bools = new BoolVector32(a_reader);
            m_ownerId = new BaseId(a_reader);
            m_editorsIds = new List<BaseId>();

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                m_editorsIds.Add(new BaseId(a_reader));
            }

            m_viewerIds = new List<BaseId>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_viewerIds.Add(new BaseId(a_reader));
            }

            m_editorGroupIds = new List<BaseId>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_editorGroupIds.Add(new BaseId(a_reader));
            }

            m_viewOnlyGroupIds = new List<BaseId>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_viewOnlyGroupIds.Add(new BaseId(a_reader));
            }

            m_groupAccessUserIds = new List<BaseId>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_groupAccessUserIds.Add(new BaseId(a_reader));
            }

            m_groupIdToAccessLevel = new Dictionary<BaseId, EUserAccess>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                BaseId userId = new (a_reader);
                a_reader.Read(out int enumVal);
                m_groupIdToAccessLevel.Add(userId, (EUserAccess)enumVal);
            }

            m_userIdToAccessLevel = new Dictionary<BaseId, EUserAccess>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                BaseId groupId = new (a_reader);
                a_reader.Read(out int enumVal);
                m_userIdToAccessLevel.Add(groupId, (EUserAccess)enumVal);
            }
        }
        else if (a_reader.VersionNumber >= 12037)
        {
            m_bools = new BoolVector32(a_reader);
            m_ownerId = new BaseId(a_reader);
            m_editorsIds = new List<BaseId>();

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                m_editorsIds.Add(new BaseId(a_reader));
            }

            m_viewerIds = new List<BaseId>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_viewerIds.Add(new BaseId(a_reader));
            }

            m_editorGroupIds = new List<BaseId>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_editorGroupIds.Add(new BaseId(a_reader));
            }

            m_viewOnlyGroupIds = new List<BaseId>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_viewOnlyGroupIds.Add(new BaseId(a_reader));
            }

            m_groupAccessUserIds = new List<BaseId>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_groupAccessUserIds.Add(new BaseId(a_reader));
            }

            //supporting older versions
            m_groupIdToAccessLevel = GenerateGroupToAccessDictionary(this);
            m_userIdToAccessLevel = GenerateUserToAccessDictionary(this);
        }
        else
        {
            m_bools = new BoolVector32(a_reader);
            m_ownerId = new BaseId(a_reader);
            m_editorsIds = new List<BaseId>();
            m_editorGroupIds = new List<BaseId>();
            m_viewOnlyGroupIds = new List<BaseId>();
            m_groupAccessUserIds = new List<BaseId>();

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                m_editorsIds.Add(new BaseId(a_reader));
            }

            m_viewerIds = new List<BaseId>();
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_viewerIds.Add(new BaseId(a_reader));
            }

            //supporting older versions
            m_groupIdToAccessLevel = GenerateGroupToAccessDictionary(this);
            m_userIdToAccessLevel = GenerateUserToAccessDictionary(this);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        m_ownerId.Serialize(a_writer);
        a_writer.Write(m_editorsIds.Count);
        foreach (BaseId baseId in m_editorsIds)
        {
            baseId.Serialize(a_writer);
        }

        a_writer.Write(m_viewerIds.Count);
        foreach (BaseId baseId in m_viewerIds)
        {
            baseId.Serialize(a_writer);
        }

        a_writer.Write(m_editorGroupIds.Count);
        foreach (BaseId baseId in m_editorGroupIds)
        {
            baseId.Serialize(a_writer);
        }

        a_writer.Write(m_viewOnlyGroupIds.Count);
        foreach (BaseId baseId in m_viewOnlyGroupIds)
        {
            baseId.Serialize(a_writer);
        }

        a_writer.Write(m_groupAccessUserIds.Count);
        foreach (BaseId baseId in m_groupAccessUserIds)
        {
            baseId.Serialize(a_writer);
        }

        a_writer.Write(m_groupIdToAccessLevel.Count);
        foreach ((BaseId baseId, EUserAccess groupAccess) in m_groupIdToAccessLevel)
        {
            baseId.Serialize(a_writer);
            a_writer.Write((int)groupAccess);
        }

        a_writer.Write(m_userIdToAccessLevel.Count);
        foreach ((BaseId baseId, EUserAccess userAccess) in m_userIdToAccessLevel)
        {
            baseId.Serialize(a_writer);
            a_writer.Write((int)userAccess);
        }
    }

    public int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 824;
    #endregion

    public ScenarioPermissionSettings(SettingData a_data)
    {
        using (BinaryMemoryReader reader = new (a_data.Data))
        {
            m_bools = new BoolVector32(reader);
            m_ownerId = new BaseId(reader);
            m_editorsIds = new List<BaseId>();

            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                m_editorsIds.Add(new BaseId(reader));
            }

            m_viewerIds = new List<BaseId>();
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_viewerIds.Add(new BaseId(reader));
            }

            m_editorGroupIds = new List<BaseId>();
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_editorGroupIds.Add(new BaseId(reader));
            }

            m_viewOnlyGroupIds = new List<BaseId>();
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_viewOnlyGroupIds.Add(new BaseId(reader));
            }

            //TODO: What happens here if we're reading in an older scenario?
            // Won't I already be at the end of the stream/reader?
            m_groupIdToAccessLevel = new Dictionary<BaseId, EUserAccess>();
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                BaseId userId = new (reader);
                reader.Read(out int enumVal);
                m_groupIdToAccessLevel.Add(userId, (EUserAccess)enumVal);
            }

            m_userIdToAccessLevel = new Dictionary<BaseId, EUserAccess>();
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                BaseId groupId = new (reader);
                reader.Read(out int enumVal);
                m_userIdToAccessLevel.Add(groupId, (EUserAccess)enumVal);
            }
        }
    }

    /// <summary>
    /// Default Constructor
    /// </summary>
    public ScenarioPermissionSettings()
    {
        m_editorsIds = new List<BaseId>();
        m_viewerIds = new List<BaseId>();
        m_editorGroupIds = new List<BaseId>();
        m_viewOnlyGroupIds = new List<BaseId>();
        m_groupAccessUserIds = new List<BaseId>();
        m_userIdToAccessLevel = new Dictionary<BaseId, EUserAccess>();
        m_groupIdToAccessLevel = new Dictionary<BaseId, EUserAccess>();
        m_bools = new BoolVector32();
        Shared = true;
    }

    private BoolVector32 m_bools;
    private const short c_shared = 0;

    private BaseId m_scenarioId;

    private BaseId m_ownerId;

    // These 5 lists are no longer used, but I think we need to keep them for backwards compatibility
    private readonly List<BaseId> m_editorsIds;
    private readonly List<BaseId> m_viewerIds;
    private readonly List<BaseId> m_editorGroupIds;
    private readonly List<BaseId> m_viewOnlyGroupIds;

    private readonly List<BaseId> m_groupAccessUserIds;

    // We could most likely just use one dictionary since I think User Ids and 
    // User Group Ids are supposed to be disjoint sets, but this will make the 
    // code a bit easier to read. 
    private Dictionary<BaseId, EUserAccess> m_userIdToAccessLevel;
    private Dictionary<BaseId, EUserAccess> m_groupIdToAccessLevel;

    // If a scenario is Shared, then users in EditorUsers and ViewOnlyUsers can interact with the 
    // scenario according what collection they're in. If Shared is false, then only the owner can interact with
    // the scenario. 
    // TODO: Correct the comments to the dictionary stuff once the swap is made
    // However, user permissions settings can supersede scenario permission settings. 
    public bool Shared
    {
        get => m_bools[c_shared];
        set => m_bools[c_shared] = value;
    }

    /// <summary>
    /// Uses User Id to find the user's Access Level
    /// </summary>
    public Dictionary<BaseId, EUserAccess> UserIdToAccessLevel
    {
        get => m_userIdToAccessLevel;
        set => m_userIdToAccessLevel = value;
    }

    /// <summary>
    /// Uses Group Id to find the group's Access Level
    /// </summary>
    public Dictionary<BaseId, EUserAccess> GroupIdToAccessLevel
    {
        get => m_groupIdToAccessLevel;
        set => m_groupIdToAccessLevel = value;
    }

    #region "Legacy Lists Properties"
    // TODO: Verify these thoughts
    // I'm pretty sure we still need the underlying lists for legacy support, 
    // but I don't think we need these properties anymore?
    //public List<BaseId> EditorGroups
    //{
    //    get => m_editorGroupIds;
    //    set => m_editorGroupIds = value;
    //}

    //public List<BaseId> ViewOnlyGroups
    //{
    //    get => m_viewOnlyGroupIds;
    //    set => m_viewOnlyGroupIds = value;
    //}

    //public List<BaseId> EditorUsers
    //{
    //    get => m_editorsIds;
    //    set => m_editorsIds = value;
    //}

    //public List<BaseId> ViewOnlyUsers
    //{
    //    get => m_viewerIds;
    //    set => m_viewerIds = value;
    //}

    //public List<BaseId> GroupAccessUsers
    //{
    //    get => m_groupAccessUserIds;
    //    set => m_groupAccessUserIds = value;
    //}

    /// <summary>
    /// Generates the user to access dictionary from the permissions lists.
    /// </summary>
    public Dictionary<BaseId, EUserAccess> GenerateUserToAccessDictionary(ScenarioPermissionSettings a_settings)
    {
        Dictionary<BaseId, EUserAccess> userToAccessLevel = new ();

        // If it exists, just update the value. The order of these if statements 
        // determines the precedences of the settings so Edit > ViewOnly > GroupAccess

        //null checks here are due to legacy support
        if (a_settings.m_groupAccessUserIds != null)
        {
            foreach (BaseId userId in a_settings.m_groupAccessUserIds)
            {
                userToAccessLevel.Add(userId, EUserAccess.UseGroupAccess);
            }
        }

        if (a_settings.m_viewerIds != null)
        {
            foreach (BaseId userId in a_settings.m_viewerIds)
            {
                if (userToAccessLevel.ContainsKey(userId))
                {
                    userToAccessLevel.Remove(userId);
                }

                userToAccessLevel.Add(userId, EUserAccess.ViewOnly);
            }
        }

        if (a_settings.m_editorsIds != null)
        {
            foreach (BaseId userId in a_settings.m_editorsIds)
            {
                if (userToAccessLevel.ContainsKey(userId))
                {
                    userToAccessLevel.Remove(userId);
                }

                userToAccessLevel.Add(userId, EUserAccess.Edit);
            }
        }

        return userToAccessLevel;
    }

    /// <summary>
    /// Generates the group to access level dictionary from the given ScenarioPermissionSetting's lists.
    /// </summary>
    /// <param name="a_settings"></param>
    /// <returns></returns>
    public Dictionary<BaseId, EUserAccess> GenerateGroupToAccessDictionary(ScenarioPermissionSettings a_settings)
    {
        Dictionary<BaseId, EUserAccess> groupToAccessLevel = new ();

        // If it exists, just update the value. The order of these if statements 
        // determines the precedences of the settings so Edit > ViewOnly > GroupAccess

        //null checks are for legacy support
        if (a_settings.m_viewOnlyGroupIds != null)
        {
            foreach (BaseId groupId in a_settings.m_viewOnlyGroupIds)
            {
                groupToAccessLevel.Add(groupId, EUserAccess.ViewOnly);
            }
        }

        if (a_settings.m_editorGroupIds != null)
        {
            foreach (BaseId groupId in a_settings.m_editorGroupIds)
            {
                if (groupToAccessLevel.ContainsKey(groupId))
                {
                    groupToAccessLevel.Remove(groupId);
                }

                groupToAccessLevel.Add(groupId, EUserAccess.Edit);
            }
        }

        return groupToAccessLevel;
    }
    #endregion

    /// <summary>
    /// Whether a specific Permissions Group has Edit Access
    /// </summary>
    /// <param name="a_permissionGroupId"></param>
    /// <returns></returns>
    public bool GroupCanEdit(BaseId a_permissionGroupId)
    {
        return MatchGroupAccessLevel(a_permissionGroupId, EUserAccess.Edit);
    }

    /// <summary>
    /// Whether a specific Permissions Group has View Only access
    /// </summary>
    /// <param name="a_permissionGroupId"></param>
    /// <returns></returns>
    public bool GroupCanView(BaseId a_permissionGroupId)
    {
        if (GroupCanEdit(a_permissionGroupId))
        {
            return true;
        }

        return MatchGroupAccessLevel(a_permissionGroupId, EUserAccess.ViewOnly);
    }

    /// <summary>
    /// Just sees if the group's permission matches the passed in permission
    /// Does not do anything fancy like check if Edit supersedes ViewOnly etc.
    /// which is why it's a private function!
    /// </summary>
    /// <param name="a_permissionGroupId"></param>
    /// <param name="a_requiredAccess"></param>
    /// <returns></returns>
    private bool MatchGroupAccessLevel(BaseId a_permissionGroupId, EUserAccess a_requiredAccess)
    {
        if (m_groupIdToAccessLevel.TryGetValue(a_permissionGroupId, out EUserAccess groupAccess))
        {
            return groupAccess == a_requiredAccess;
        }

        // Just giving the group a default value since it doesn't exist in the dictionary
        m_groupIdToAccessLevel.Add(a_permissionGroupId, c_DefaultGroupAccess);
        return a_requiredAccess == c_DefaultGroupAccess;
    }

    /// <summary>
    /// User needs to use Permissions as defined on the assigned Permissions Group
    /// </summary>
    /// <param name="a_userId"></param>
    /// <returns></returns>
    public bool UseGroupAccess(BaseId a_userId)
    {
        if (m_userIdToAccessLevel.TryGetValue(a_userId, out EUserAccess userAccess))
        {
            return userAccess == EUserAccess.UseGroupAccess;
        }

        // return true here so that it defaults the user to using group access
        return true;
    }

    /// <summary>
    /// Verify the User has Edit Permissions
    /// </summary>
    /// <param name="a_userId"></param>
    /// <param name="a_permissionGroupId"></param>
    /// <returns></returns>
    public bool CanUserEdit(BaseId a_userId, BaseId a_permissionGroupId)
    {
        //User is the Owner or the User is Server
        if (OwnerId == a_userId || a_userId == BaseId.NULL_ID || a_userId == BaseId.ERP_ID)
        {
            return true;
        }

        if (!Shared)
        {
            return false;
        }

        InitializeUserIfNecessary(a_userId);
        return MatchUserAccessLevel(a_userId, a_permissionGroupId, EUserAccess.Edit);
    }

    /// <summary>
    /// Verify the User has View-only Permissions
    /// </summary>
    /// <param name="a_userId"></param>
    /// <param name="a_permissionGroupId"></param>
    /// <returns></returns>
    public bool CanUserView(BaseId a_userId, BaseId a_permissionGroupId)
    {
        //Check if User has higher level permissions
        if (CanUserEdit(a_userId, a_permissionGroupId))
        {
            return true;
        }

        if (!Shared)
        {
            return false;
        }

        InitializeUserIfNecessary(a_userId);
        return MatchUserAccessLevel(a_userId, a_permissionGroupId, EUserAccess.ViewOnly);
    }

    public bool IsViewOnly(BaseId a_userId, BaseId a_permissionGroupId)
    {
        return !CanUserEdit(a_userId, a_permissionGroupId) && CanUserView(a_userId, a_permissionGroupId);
    }

    /// <summary>
    /// Default permission for a group which has no permission explicitly set in <see cref="GroupIdToAccessLevel" />.
    /// This will not consider any unique traits of a specific group - to test one specifically, use methods that take group ids like <see cref="GroupCanEdit" /> or <see cref="GroupCanView" />.
    /// </summary>
    public static EUserAccess c_DefaultGroupAccess => EUserAccess.None;

    /// <summary>
    /// Default permission for a user which has no permission explicitly set in <see cref="UserIdToAccessLevel" />.
    /// This will not consider any unique traits of a specific user - notably, for instance, whether it is the owner of the scenario.
    /// To test a specific scenario, use methods that take user ids like <see cref="CanUserEdit" /> or <see cref="CanUserView" />.
    /// </summary>
    public static EUserAccess c_DefaultUserAccess => EUserAccess.UseGroupAccess;

    /*
     * The UserManager does not have access to scenarioInfo, and I don't feel like it should.
     * It is not a scenario specific object, and this separation should hopefully reduce timing issues.
     * However, this means that I cannot insert users into a scenario's set of users with access
     * when new users are created. The idea is that when a new user looks up their scenario
     * permissions for the first time, they won't exist in the dictionary, so just add them
     * when this happens.
     */
    /// <summary>
    /// Checks if the User's Id exists within the settings list of users, and if not, adds the user
    /// and defaults to giving them UseGroupAccess. See comments for more detail
    /// </summary>
    /// <param name="a_userId"></param>
    private void InitializeUserIfNecessary(BaseId a_userId)
    {
        if (!m_userIdToAccessLevel.ContainsKey(a_userId))
        {
            m_userIdToAccessLevel.Add(a_userId, c_DefaultUserAccess);
        }
    }

    /// <summary>
    /// Just sees if the group's permission matches the passed in permission
    /// Does not do anything fancy like check if Edit supersedes ViewOnly etc.
    /// which is why it's a private function! Don't pass in EUserAccess.UseGroupAccess
    /// or EUserAccess.None for a_requiredAccessLevel!
    /// </summary>
    /// <param name="a_userId">The Id of the user</param>
    /// <param name="a_permissionGroupId">The Id of the user's User Group</param>
    /// <param name="a_requiredAccessLevel">The access level that the user needs to have</param>
    /// <returns>True if the user has the required access level, false otherwise</returns>
    /// Passing in EUserAccess.UseGroupAccess and EUserAccess.None for a_requiredAccessLevel
    /// don't really make sense to me, but I don't think it's worth writing in code to handle them.
    private bool MatchUserAccessLevel(BaseId a_userId, BaseId a_permissionGroupId, EUserAccess a_requiredAccessLevel)
    {
        // Could just directly access the item collection with the key since
        // we already checked for existence before calling this function. 
        if (m_userIdToAccessLevel.TryGetValue(a_userId, out EUserAccess userAccess))
        {
            if (userAccess == EUserAccess.UseGroupAccess)
            {
                if (m_groupIdToAccessLevel.TryGetValue(a_permissionGroupId, out EUserAccess groupAccess))
                {
                    if (groupAccess == a_requiredAccessLevel)
                    {
                        return true;
                    }
                }
            }
            else if (userAccess == a_requiredAccessLevel)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Does not take into consideration the user group permissions
    /// </summary>
    /// <returns>A list of BaseId for all users that have View or Edit access to the scenario</returns>
    public IEnumerable<BaseId> GetUsersWithViewOrEditAccess()
    {
        IList<BaseId> users = new List<BaseId>();
        foreach ((BaseId userId, EUserAccess userAccess) in m_userIdToAccessLevel)
        {
            switch (userAccess)
            {
                case EUserAccess.None:
                case EUserAccess.UseGroupAccess:
                    // We do not know what the user's group Id is within this class,
                    // and I don't think it's worth trying to track it. 
                    break;
                case EUserAccess.ViewOnly:
                case EUserAccess.Edit:
                    users.Add(userId);
                    break;
            }
        }

        return users;
    }

    [Obsolete("Needs Group Id")]
    public bool CanEdit(BaseId a_userId)
    {
        if (OwnerId == a_userId || a_userId == BaseId.NULL_ID || a_userId == BaseId.ERP_ID)
        {
            return true;
        }

        if (Shared)
        {
            return m_editorsIds.Contains(a_userId);
        }

        return false;
    }

    [Obsolete("Needs Group Id")]
    public bool CanView(BaseId a_userId)
    {
        if (CanEdit(a_userId))
        {
            return true;
        }

        if (Shared)
        {
            return m_viewerIds.Contains(a_userId);
        }

        return false;
    }

    [Obsolete("Needs Group Id")]
    public bool IsReadOnly(BaseId a_userId)
    {
        return !CanEdit(a_userId) && CanView(a_userId);
    }

    public string SettingKey => Key;

    public static string Key => "scenarioSettings_Permissions";

    public string SettingCaption => "Scenario Permissions";
    public string Description => "Scenario Permissions";
    public string SettingsGroup => SettingGroupConstants.ScenariosGroup;
    public string SettingsGroupCategory => SettingGroupConstants.ScenarioPermissions;

    public BaseId ScenarioId
    {
        get => m_scenarioId;
        set => m_scenarioId = value;
    }

    public BaseId OwnerId
    {
        get => m_ownerId;
        set => m_ownerId = value;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    public ScenarioPermissionSettings Clone()
    {
        return (ScenarioPermissionSettings)MemberwiseClone();
    }
}