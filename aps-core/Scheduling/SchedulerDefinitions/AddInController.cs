namespace PT.SchedulerDefinitions;

/// <summary>
/// Controls whether an AddIn is Enabled or Disabled.
/// </summary>
public class AddInController : IPTSerializable
{
    #region IPTSerializable Members
    public AddInController(IReader reader)
    {
        if (reader.VersionNumber >= 376)
        {
            reader.Read(out _name);
            reader.Read(out _enabled);
            reader.Read(out _adminControlled);
        }
        else
        {
            reader.Read(out _name);
            reader.Read(out _enabled);
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(_name);
        writer.Write(_enabled);
        writer.Write(_adminControlled);
    }

    public const int UNIQUE_ID = 719;

    public int UniqueId => UNIQUE_ID;
    #endregion

    public AddInController(string aName, bool aEnabled, bool aAdminControlled)
    {
        _name = aName;
        _enabled = aEnabled;
        _adminControlled = aAdminControlled;
    }

    private string _name;

    /// <summary>
    /// The Name of the AddIn. Corresponds with the Name in ICustomizationBase
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    private bool _enabled;

    /// <summary>
    /// If Enabled then the AddIn will be used.  Otherwise it's ignored.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    private bool _adminControlled;

    /// <summary>
    /// Whether only Admin users can enable/disable.
    /// </summary>
    public bool AdminControlled
    {
        get => _adminControlled;
        set => _adminControlled = value;
    }
}

/// <summary>
/// List of AddInControllers for a ScenarioDetail.
/// </summary>
public class AddInControllerList : IPTSerializable
{
    #region IPTSerializable Members
    public AddInControllerList(IReader reader)
    {
        int count;
        reader.Read(out count);
        for (int i = 0; i < count; i++)
        {
            _list.Add(new AddInController(reader));
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(_list.Count);
        for (int i = 0; i < _list.Count; i++)
        {
            _list[i].Serialize(writer);
        }
    }

    public const int UNIQUE_ID = 720;

    public int UniqueId => UNIQUE_ID;
    #endregion

    public AddInControllerList() { }

    private readonly List<AddInController> _list = new ();

    public AddInController this[int index] => _list[index];

    public int Count => _list.Count;

    public void Add(AddInController aAddinController)
    {
        _list.Add(aAddinController);
    }

    public void Remove(AddInController aAddinController)
    {
        _list.Remove(aAddinController);
    }
}