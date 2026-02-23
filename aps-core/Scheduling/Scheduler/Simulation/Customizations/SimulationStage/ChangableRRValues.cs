namespace PT.Scheduler;

public class ChangableRRValues
{
    private ValueSetter<Resource> m_defaultResourceSetter;

    public bool DefaultResourceSet => m_defaultResourceSetter.Set;

    public Resource DefaultResource
    {
        get => m_defaultResourceSetter.Value;
        set => m_defaultResourceSetter.Value = value;
    }
}