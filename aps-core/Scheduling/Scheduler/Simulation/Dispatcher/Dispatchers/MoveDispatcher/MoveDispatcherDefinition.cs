using PT.APSCommon;

namespace PT.Scheduler;

public partial class MoveDispatcherDefinition : DispatcherDefinition
{
    public MoveDispatcherDefinition(BaseId a_id)
        : base(a_id) { }

    internal override ReadyActivitiesDispatcher CreateDispatcher()
    {
        return new MoveDispatcher(this);
    }

    private readonly KeyComparer m_comparer = new ();

    internal override IComparer<KeyAndActivity> Comparer => m_comparer;
}