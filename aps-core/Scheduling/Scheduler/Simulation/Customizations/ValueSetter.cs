namespace PT.Scheduler;

/// <summary>
/// Used to set and track whether a value has been set.
/// When the Value member has been set,
/// </summary>
/// <typeparam name="Ty"></typeparam>
internal struct ValueSetter<Ty>
{
    private bool m_set;

    public bool Set
    {
        get => m_set;
        private set => m_set = value;
    }

    private Ty m_ty;

    public Ty Value
    {
        get => m_ty;
        set
        {
            Set = true;
            m_ty = value;
        }
    }
}