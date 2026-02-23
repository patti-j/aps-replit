using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages ths scenario's list of ShopViewResourceOption objects.
/// </summary>
public class ShopViewResourceOptionsManager : IPTSerializable
{
    #region IPTSerializable
    public const int UNIQUE_ID = 499;

    public ShopViewResourceOptionsManager(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out nextId);
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ShopViewResourceOptions options = new (reader);
                m_list.Add(options.id, options);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(nextId);
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).Serialize(writer);
        }
    }

    public int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(PlantManager plants)
    {
        for (int plantI = 0; plantI < plants.Count; plantI++)
        {
            Plant p = plants[plantI];
            for (int deptI = 0; deptI < p.Departments.Count; deptI++)
            {
                Department d = p.Departments[deptI];
                for (int resI = 0; resI < d.Resources.Count; resI++)
                {
                    Resource r = d.Resources[resI];
                    r.RestoreShopViewOptionsReferences(this);
                }
            }
        }
    }
    #endregion

    public ShopViewResourceOptionsManager()
    {
        //Add a default one
        ShopViewResourceOptions option = new ("Default".Localize());
        Add(option);
    }

    internal ShopViewResourceOptions DefaultOptions => GetByIndex(0);

    #region List Maintenance
    private readonly SortedList<BaseId, ShopViewResourceOptions> m_list = new ();

    private void Add(ShopViewResourceOptions options)
    {
        options.id = GetNextId();
        m_list.Add(options.id, options);
    }

    public int Count => m_list.Count;

    public ShopViewResourceOptions this[BaseId optionId] => m_list[optionId];

    public ShopViewResourceOptions GetByIndex(int index)
    {
        return m_list.Values[index];
    }

    private void Remove(BaseId id)
    {
        m_list.Remove(id);
    }

    private void Clear()
    {
        m_list.Clear();
    }

    private long nextId;

    private BaseId GetNextId()
    {
        BaseId returnId = new (nextId);
        nextId++;
        return returnId;
    }
    #endregion

    #region Transmission Processing
    public void Receive(ShopViewOptionsAssignmentT t, PlantManager plants)
    {
        //Set the options for each Resource
        for (int i = 0; i < t.ResourceOptionsAssignmentCount; i++)
        {
            ShopViewOptionsAssignmentT.ResourceOptionsAssignment assignment = t.GetResourceAssignment(i);
            ShopViewResourceOptions options = this[assignment.optionsId];
            if (options != null)
            {
                ResourceKey key = new (assignment.plantId, assignment.deptId, assignment.resourceId);
                Resource resource = plants.GetResource(key);
                if (resource != null)
                {
                    resource.ShopViewResourceOptions = options;
                    resource.ShopViewResourceOptionsRefId = options.id;
                }
            }
        }
    }

    public void Receive(ShopViewResourceOptionBaseT t, ScenarioDetail sd, IScenarioDataChanges a_dataChanges)
    {
        if (t is ShopViewResourceOptionNewT)
        {
            AddNew((ShopViewResourceOptionNewT)t, a_dataChanges);
        }
        else if (t is ShopViewResourceOptionDeleteT)
        {
            Delete((ShopViewResourceOptionDeleteT)t, sd.PlantManager, a_dataChanges);
        }
        else if (t is ShopViewResourceOptionUpdateT)
        {
            Update((ShopViewResourceOptionUpdateT)t, a_dataChanges);
        }
        else if (t is ShopViewResourceOptionCopyT)
        {
            Copy((ShopViewResourceOptionCopyT)t, a_dataChanges);
        }
    }

    private void AddNew(ShopViewResourceOptionNewT t, IScenarioDataChanges a_dataChanges)
    {
        Add(t.Options);
        a_dataChanges.ShopViewResourceOptionsChanges.AddedObject(t.Options.id);
    }

    private void Delete(ShopViewResourceOptionDeleteT t, PlantManager plants, IScenarioDataChanges a_dataChanges)
    {
        if (Count == 1)
        {
            throw new PTHandleableException("2311");
        }

        ShopViewResourceOptions option = this[t.id];
        if (option == null)
        {
            throw new PTHandleableException("2312", new object[] { t.id.ToString() });
        }

        //Get the default option set for Resources that are using this set
        ShopViewResourceOptions possibleDefault = DefaultOptions;
        for (int i = 0; i < Count; i++)
        {
            if (possibleDefault.id != option.id)
            {
                break;
            }

            possibleDefault = GetByIndex(i);
        }

        //Need to set all Resource's that use it to the default Option set.
        SwapResourceOptions(option, possibleDefault, plants);
        Remove(option.id);
        a_dataChanges.ShopViewResourceOptionsChanges.DeletedObject(option.id);
    }

    private void SwapResourceOptions(ShopViewResourceOptions oldOptions, ShopViewResourceOptions newOptions, PlantManager plants)
    {
        for (int pI = 0; pI < plants.Count; pI++)
        {
            Plant p = plants[pI];
            for (int dI = 0; dI < p.Departments.Count; dI++)
            {
                Department d = p.Departments[dI];
                for (int rI = 0; rI < d.Resources.Count; rI++)
                {
                    Resource r = d.Resources[rI];
                    if (r.ShopViewResourceOptions.id == oldOptions.id)
                    {
                        r.ShopViewResourceOptions = newOptions;
                    }
                }
            }
        }
    }

    private void Update(ShopViewResourceOptionUpdateT t, IScenarioDataChanges a_dataChanges)
    {
        ShopViewResourceOptions option = this[t.id];

        if (option == null)
        {
            throw new PTHandleableException("2329", new object[] { t.id.ToString() });
        }

        option.Update(t.Options, true);
        a_dataChanges.ShopViewResourceOptionsChanges.UpdatedObject(option.id);
    }

    private void Copy(ShopViewResourceOptionCopyT t, IScenarioDataChanges a_dataChanges)
    {
        ShopViewResourceOptions option = this[t.id];
        if (option == null)
        {
            throw new PTHandleableException("2329", new object[] { t.id.ToString() });
        }

        string newName = string.Format("Copy of {0}".Localize(), option.Name);
        ShopViewResourceOptions copyOption = new (newName);
        copyOption.Update(option, false);
        Add(copyOption);
        a_dataChanges.ShopViewResourceOptionsChanges.UpdatedObject(copyOption.id);
    }
    #endregion
}

public class ShopViewSystemOptions : IPTSerializable
{
    #region IPTSerializable
    public const int UNIQUE_ID = 507;

    public ShopViewSystemOptions(IReader reader)
    {
        if (reader.VersionNumber >= 705)
        {
            //System options
            reader.Read(out autoLogout);
            reader.Read(out autoLogoutSpan);
            reader.Read(out usePasswords);
            reader.Read(out usePublishedScenario);
            reader.Read(out autoRefreshSpan);
        }
        else
        {
            //System options
            reader.Read(out autoLogout);
            reader.Read(out autoLogoutSpan);
            reader.Read(out usePasswords);
            reader.Read(out usePublishedScenario);
        }
    }

    public void Serialize(IWriter writer)
    {
        //System options
        writer.Write(autoLogout);
        writer.Write(autoLogoutSpan);
        writer.Write(usePasswords);
        writer.Write(usePublishedScenario);
        writer.Write(autoRefreshSpan);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public ShopViewSystemOptions() { }

    #region Transmissions
    public void Receive(ShopViewSystemOptionsUpdateT t)
    {
        AutoLogout = t.autoLogout;
        AutoLogoutSpan = t.autoLogoutSpan;
        UsePasswords = t.usePasswords;
        UsePublishedSceanario = t.usePublishedScenario;
        AutoRefreshSpan = t.autoRefreshSpan;
    }
    #endregion

    #region Properties
    private bool autoLogout = true;

    /// <summary>
    /// Whether to logout users after a period of inactivity.
    /// </summary>
    public bool AutoLogout
    {
        get => autoLogout;
        set => autoLogout = value;
    }

    private TimeSpan autoLogoutSpan = TimeSpan.FromMinutes(5);

    public TimeSpan AutoLogoutSpan
    {
        get => autoLogoutSpan;
        set => autoLogoutSpan = value;
    }

    private TimeSpan autoRefreshSpan = TimeSpan.FromMinutes(20);

    public TimeSpan AutoRefreshSpan
    {
        get => autoRefreshSpan;
        set => autoRefreshSpan = value;
    }

    private bool usePasswords = true;

    /// <summary>
    /// Whether to use passwords during login of Shop Views.
    /// </summary>
    public bool UsePasswords
    {
        get => usePasswords;
        set => usePasswords = value;
    }

    private bool usePublishedScenario;

    /// <summary>
    /// Whether the Published, instead of Live, Scenario should be used for Shop Views.
    /// </summary>
    public bool UsePublishedSceanario
    {
        get => usePublishedScenario;
        set => usePublishedScenario = value;
    }
    #endregion
}