using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Extensions;
using PT.SchedulerDefinitions.UserSettingTemplates;

namespace PT.SchedulerDefinitions.PermissionTemplates;

public class PlantPermissionSet : IPTSerializable, ICloneable
{
    #region IPTSerializable
    public PlantPermissionSet(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            m_id = new BaseId(a_reader);
            a_reader.Read(out m_name);
            a_reader.Read(out m_autoGrantNewPermissions);

            a_reader.Read(out int allowedCount);
            for (int i = 0; i < allowedCount; i++)
            {
                BaseId plantId = new (a_reader);
                HashSet<string> hashSet = new ();
                m_plantAllowedPermissions.Add(plantId, hashSet);
                a_reader.Read(out int hashCount);
                for (int hashI = 0; hashI < hashCount; hashI++)
                {
                    a_reader.Read(out string permissionKey);
                    hashSet.Add(permissionKey);
                }
            }

            a_reader.Read(out int deniedCount);
            for (int i = 0; i < deniedCount; i++)
            {
                BaseId plantId = new (a_reader);
                HashSet<string> hashSet = new ();
                m_plantDeniedPermissions.Add(plantId, hashSet);
                a_reader.Read(out int hashCount);
                for (int hashI = 0; hashI < hashCount; hashI++)
                {
                    a_reader.Read(out string permissionKey);
                    hashSet.Add(permissionKey);
                }
            }
        }
        else
        {
            m_id = new BaseId(a_reader);
            m_autoGrantNewPermissions = true;
            a_reader.Read(out m_name);
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                new PlantPermissions(a_reader);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_id.Serialize(a_writer);
        a_writer.Write(m_name);

        a_writer.Write(m_autoGrantNewPermissions);

        a_writer.Write(m_plantAllowedPermissions.Count);
        foreach (KeyValuePair<BaseId, HashSet<string>> pair in m_plantAllowedPermissions)
        {
            pair.Key.Serialize(a_writer);
            a_writer.Write(pair.Value);
        }

        a_writer.Write(m_plantDeniedPermissions.Count);
        foreach (KeyValuePair<BaseId, HashSet<string>> pair in m_plantDeniedPermissions)
        {
            pair.Key.Serialize(a_writer);
            a_writer.Write(pair.Value);
        }
    }

    public int UniqueId => UNIQUE_ID;

    private const int UNIQUE_ID = 827;
    #endregion

    /// <summary>
    /// Construct a new Permission for use in updating settings
    /// </summary>
    /// <param name="a_id"></param>
    /// <param name="a_name"></param>
    public PlantPermissionSet(BaseId a_id, string a_name)
    {
        m_id = a_id;
        m_name = a_name;
    }

    /// <summary>
    /// Construct a new default permissions set
    /// </summary>
    public PlantPermissionSet(BaseId a_id)
    {
        m_id = a_id;
        m_name = "Administration".Localize();
        //Start with Admin permissions

        AutoGrantNewPermissions = true;
    }

    /// <summary>
    /// Construct a new default permissions set
    /// </summary>
    public PlantPermissionSet(PlantPermissionSet a_originalSet, BaseId a_id)
    {
        m_id = a_id;
        Update(a_originalSet);
    }

    private BaseId m_id;

    public BaseId Id => m_id;

    public void GenerateId(BaseId a_id)
    {
        m_id = a_id;
    }

    private string m_name;

    public string Name => m_name;

    private readonly Dictionary<BaseId, HashSet<string>> m_plantAllowedPermissions = new ();
    private readonly Dictionary<BaseId, HashSet<string>> m_plantDeniedPermissions = new ();

    /// <summary>
    /// Returns a validator class that is capable of determining whether the user has a specified permission
    /// </summary>
    /// <param name="a_plant"></param>
    public PlantPermissionValidator GetPlantPermissionsValidator(BaseId a_plant)
    {
        //If autogranting permissions, all we need to provide is the list of denied permissions, Otherwise only the list of allowed permissions matters

        Dictionary<BaseId, HashSet<string>> dict = AutoGrantNewPermissions ? m_plantDeniedPermissions : m_plantAllowedPermissions;

        if (dict.TryGetValue(a_plant, out HashSet<string> permissions))
        {
            return new PlantPermissionValidator(permissions, AutoGrantNewPermissions);
        }

        return new PlantPermissionValidator(new HashSet<string>(), AutoGrantNewPermissions);
    }

    /// <summary>
    /// Returns the permission keys in this group
    /// </summary>
    /// <param name="a_plant"></param>
    /// <param name="a_allowed">Whether to return the list of allowed permissions or the denied permissions</param>
    /// <returns></returns>
    public HashSet<string> GetPermissions(BaseId a_plant, bool a_allowed)
    {
        Dictionary<BaseId, HashSet<string>> dict = a_allowed ? m_plantAllowedPermissions : m_plantDeniedPermissions;
        if (dict.TryGetValue(a_plant, out HashSet<string> permissions))
        {
            return permissions;
        }

        return new HashSet<string>();
    }

    /// <summary>
    /// Adds a new permission to one of the permissions lists, for the specified plant
    /// </summary>
    public void AddPermission(BaseId a_plant, string a_permission, bool a_allowed)
    {
        Dictionary<BaseId, HashSet<string>> dict = a_allowed ? m_plantAllowedPermissions : m_plantDeniedPermissions;
        if (dict.TryGetValue(a_plant, out HashSet<string> permissionSet))
        {
            permissionSet.AddIfNew(a_permission);
        }
        else
        {
            HashSet<string> newSet = new ();
            newSet.Add(a_permission);
            dict.Add(a_plant, newSet);
        }
    }

    /// <summary>
    /// Return whether a permission in unconfigured in a given plant.
    /// </summary>
    /// <param name="a_plantId"></param>
    /// <param name="a_permissionKey"></param>
    /// <returns></returns>
    public bool ContainsUnConfiguredPermission(BaseId a_plantId, string a_permissionKey)
    {
        bool allowedListContainsPermission = false;
        bool deniedListContainsPermission = false;

        if (m_plantAllowedPermissions.TryGetValue(a_plantId, out HashSet<string> allowedPermissions))
        {
            allowedListContainsPermission = allowedPermissions.Contains(a_permissionKey);
        }

        if (m_plantDeniedPermissions.TryGetValue(a_plantId, out HashSet<string> deniedPermissions))
        {
            deniedListContainsPermission = deniedPermissions.Contains(a_permissionKey);
        }

        return !allowedListContainsPermission && !deniedListContainsPermission;
    }

    public void Update(PlantPermissionSet a_permissionSet)
    {
        m_name = a_permissionSet.Name;
        m_autoGrantNewPermissions = a_permissionSet.AutoGrantNewPermissions;

        //Update dictionaries
        m_plantAllowedPermissions.Clear();
        m_plantDeniedPermissions.Clear();

        foreach (KeyValuePair<BaseId, HashSet<string>> keyValuePair in a_permissionSet.m_plantAllowedPermissions)
        {
            m_plantAllowedPermissions.Add(keyValuePair.Key, keyValuePair.Value);
        }

        foreach (KeyValuePair<BaseId, HashSet<string>> keyValuePair in a_permissionSet.m_plantDeniedPermissions)
        {
            m_plantDeniedPermissions.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }

    private bool m_autoGrantNewPermissions;

    /// <summary>
    /// Group permission for whether users in this group are automatically granted new, unconfigured permissions
    /// </summary>
    public bool AutoGrantNewPermissions
    {
        get => m_autoGrantNewPermissions;
        set => m_autoGrantNewPermissions = value;
    }

    public object Clone()
    {
        return new PlantPermissionSet(this, Id);
    }
}