namespace PT.Scheduler.Simulation.Customizations;

/// <summary>
/// Stores common data changes that mirror the BaseObject class values
/// </summary>
public class BaseOperationValueChanges
{
    private ValueSetter<bool> m_onHold;
    private ValueSetter<long> m_holdUntil;
    private ValueSetter<string> m_holdReason;
    private readonly BaseOperation m_object;

    public BaseOperationValueChanges(BaseOperation a_operation)
    {
        m_object = a_operation;
    }

    public bool OnHold
    {
        get => m_onHold.Value;
        set => m_onHold.Value = value;
    }

    public long HoldUntil
    {
        get => m_holdUntil.Value;
        set => m_holdUntil.Value = value;
    }

    public string HoldReason
    {
        get => m_holdReason.Value;
        set => m_holdReason.Value = value;
    }

    /// <summary>
    /// Update the object with the pending changes
    /// </summary>
    internal void ExecuteChanges()
    {
        if (m_onHold.Set)
        {
            m_object.OnHold = m_onHold.Value;
        }

        if (m_holdUntil.Set)
        {
            m_object.HoldUntil = new DateTime(m_holdUntil.Value);
        }

        if (m_holdReason.Set)
        {
            m_object.HoldReason = m_holdReason.Value;
        }
    }
}