using PT.SchedulerDefinitions;
using PT.Transmissions;
using PT.Transmissions.User;

namespace PT.Scheduler;

public class UserManagerEvents
{
    public delegate void TransmissionProcessedCompleteDelegate(PTTransmission a_t, TimeSpan a_processingTime, Exception a_e);

    public event TransmissionProcessedCompleteDelegate TransmissionProcessedEvent;

    internal void FireTransmissionProcessedEvent(PTTransmission a_t, TimeSpan a_processingTime, Exception a_e)
    {
        TransmissionProcessedEvent?.Invoke(a_t, a_processingTime, a_e);
    }

    public delegate void UserDefaultDelegate(User u, UserManager um, UserDefaultT t);

    public event UserDefaultDelegate UserDefaultEvent;

    internal void FireUserDefaultEvent(User a_u, UserManager a_um, UserDefaultT a_t)
    {
        UserDefaultEvent?.Invoke(a_u, a_um, a_t);
    }

    public delegate void UserCopyDelegate(User u, UserManager um, UserCopyT t);

    public event UserCopyDelegate UserCopyEvent;

    internal void FireUserCopyEvent(User a_u, UserManager a_um, UserCopyT a_t)
    {
        UserCopyEvent?.Invoke(a_u, a_um, a_t);
    }

    public delegate void UserLogOnDelegate(User u, UserManager um, UserLogOnT t);

    public event UserLogOnDelegate UserLogOnEvent;

    //TODO: This can probably be removed
    internal void FireUserLogOnEvent(User a_u, UserManager a_um, UserLogOnT a_t)
    {
        UserLogOnEvent?.Invoke(a_u, a_um, a_t);
    }

    public delegate void UserChatDelegate(User u, UserChatT t);

    public event UserChatDelegate UserChatEvent;

    internal void FireUserChatEvent(User a_u, UserChatT a_t)
    {
        UserChatEvent?.Invoke(a_u, a_t);
    }

    public Action UserPermissionsUpdatedEvent;

    internal void FireUserPermissionsUpdatedEvent()
    {
        UserPermissionsUpdatedEvent?.Invoke();
    }

    /// <summary>
    /// Data objects have been changed, update listeners with the changes
    /// </summary>
    public event Action<IScenarioDataChanges> ScenarioDataChangesEvent;

    internal void FireScenarioDataChangedEvent(IScenarioDataChanges a_dataChanges)
    {
        ScenarioDataChangesEvent?.Invoke(a_dataChanges);
    }

    public delegate void UserAdminLogoffDelegate(User a_u, UserAdminLogOffT a_t);

    public event UserAdminLogoffDelegate UserAdminLogOffEvent;

    internal void FireUserAdminLogoffEvent(User a_u, UserAdminLogOffT a_t)
    {
        UserAdminLogOffEvent?.Invoke(a_u, a_t);
    }
}