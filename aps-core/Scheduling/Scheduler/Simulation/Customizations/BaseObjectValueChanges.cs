namespace PT.Scheduler.Simulation.Customizations;

/// <summary>
/// Stores common data changes that mirror the BaseObject class values
/// </summary>
public class BaseObjectValueChanges
{
    private ValueSetter<string> m_name;
    private ValueSetter<string> m_description;
    private ValueSetter<string> m_notes;
    private readonly BaseObject m_object;

    internal BaseObjectValueChanges(BaseObject a_object)
    {
        m_object = a_object;
    }

    public string Name
    {
        get => m_name.Value;
        set => m_name.Value = value;
    }

    public string Description
    {
        get => m_description.Value;
        set => m_description.Value = value;
    }

    public string Notes
    {
        get => m_notes.Value;
        set => m_notes.Value = value;
    }

    /// <summary>
    /// Update the object with the pending changes
    /// </summary>
    internal void Execute()
    {
        if (m_name.Set)
        {
            m_object.Name = m_name.Value;
        }

        if (m_description.Set)
        {
            m_object.Description = m_description.Value;
        }

        if (m_notes.Set)
        {
            m_object.Notes = m_notes.Value;
        }
    }
}