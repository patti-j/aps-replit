namespace PT.Scheduler;

/// <summary>
/// MO Attributes that can be set by Customizations.
/// </summary>
public class ChangableMOValues
{
    private ValueSetter<bool> m_MoNeedDateSetter;

    public bool MoNeedDateSet => m_MoNeedDateSetter.Set;

    public bool MoNeedDate
    {
        get => m_MoNeedDateSetter.Value;
        set => m_MoNeedDateSetter.Value = value;
    }

    private ValueSetter<long> m_NeedDateTicksSetter;

    public bool NeedDateTicksSet => m_NeedDateTicksSetter.Set;

    public long NeedDateTicks
    {
        get => m_NeedDateTicksSetter.Value;
        set => m_NeedDateTicksSetter.Value = value;
    }

    private ValueSetter<decimal> m_requiredQtySetter;

    public bool RequiredQtySet => m_requiredQtySetter.Set;

    public decimal RequiredQty
    {
        get => m_requiredQtySetter.Value;
        set => m_requiredQtySetter.Value = value;
    }

    private ValueSetter<SchedulerDefinitions.UserFieldList> m_userFieldListSetter;

    public bool UserFieldListSet => m_userFieldListSetter.Set;

    public SchedulerDefinitions.UserFieldList UserFields
    {
        get => m_userFieldListSetter.Value;
        set => m_userFieldListSetter.Value = value;
    }
}