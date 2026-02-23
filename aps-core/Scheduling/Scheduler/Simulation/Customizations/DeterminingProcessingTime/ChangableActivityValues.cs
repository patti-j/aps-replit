namespace PT.Scheduler;

/// <summary>
/// Used by the RequiredCapacityCustomizations to allow specification of which values should be changed.
/// </summary>
public class ChangableActivityValues
{
    private ValueSetter<TimeSpan> m_setupSpanSetter;

    public bool SetupSpanSet => m_setupSpanSetter.Set;

    public TimeSpan SetupSpan
    {
        get => m_setupSpanSetter.Value;
        set => m_setupSpanSetter.Value = value;
    }

    private ValueSetter<TimeSpan> m_cycleSpanSetter;

    public bool CycleSpanSet => m_cycleSpanSetter.Set;

    public TimeSpan CycleSpan
    {
        get => m_cycleSpanSetter.Value;
        set => m_cycleSpanSetter.Value = value;
    }

    private ValueSetter<string> m_commentsSetter;

    public bool CommentsSet => m_commentsSetter.Set;

    public string Comments
    {
        get => m_commentsSetter.Value;
        set => m_commentsSetter.Value = value;
    }
}