using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Machine objects that are stored according to their MachineKeys (not their Id only).
/// </summary>
[Serializable]
public class ResourceList
{
    public class MachineKeyManagerException : ApplicationException
    {
        public MachineKeyManagerException(string message)
            : base(message) { }
    }

    private SortedList<ResourceKey, Resource> m_machines = new ();

    public Resource this[ResourceKey key] => m_machines[key];

    public Resource GetByIndex(int index)
    {
        return m_machines.Values[index];
    }

    public void Add(Resource m)
    {
        m_machines.Add(m.GetKey(), m);
    }

    public void Remove(ResourceKey key)
    {
        m_machines.Remove(key);
    }

    public int Count => m_machines.Count;
}