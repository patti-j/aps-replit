namespace PT.Scheduler.Simulation.Customizations;

/// <summary>
/// Stores pending data changes to the InternalActivity that can be applied at once.
/// </summary>
public class ActivityValueChanges
{
    private readonly InternalActivity m_act;
    private ValueSetter<string> m_comments;
    private ValueSetter<string> m_comments2;
    private ValueSetter<TimeSpan> m_postProcessingSpanChange;

    public ActivityValueChanges(InternalActivity a_act)
    {
        m_act = a_act;
        m_comments = new ValueSetter<string>();
        m_comments2 = new ValueSetter<string>();
        m_postProcessingSpanChange = new ValueSetter<TimeSpan>();
    }

    public string Comments
    {
        get => m_comments.Value;
        set => m_comments.Value = value;
    }

    public string Comments2
    {
        get => m_comments2.Value;
        set => m_comments2.Value = value;
    }

    public TimeSpan PostProcessingSpanChange
    {
        get => m_postProcessingSpanChange.Value;
        set => m_postProcessingSpanChange.Value = value;
    }

    /// <summary>
    /// Update the object with the pending changes
    /// </summary>
    internal void ExecuteChanges()
    {
        if (m_comments.Set)
        {
            m_act.Comments = m_comments.Value;
        }

        if (m_comments2.Set)
        {
            m_act.Comments2 = m_comments2.Value;
        }

        if (m_postProcessingSpanChange.Set)
        {
            m_act.Operation.PostProcessingSpan += m_postProcessingSpanChange.Value;
        }
    }
}