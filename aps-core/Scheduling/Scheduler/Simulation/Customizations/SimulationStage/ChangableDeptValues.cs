namespace PT.Scheduler;

/// <summary>
/// MO Attributes that can be set by Customizations.
/// </summary>
public class ChangableDeptValues
{
    private ValueSetter<long> m_frozenSpanTicksSetter;

    public bool FrozenSpanSet => m_frozenSpanTicksSetter.Set;

    public long FrozenSpanTicks
    {
        get => m_frozenSpanTicksSetter.Value;
        set => m_frozenSpanTicksSetter.Value = value;
    }
}