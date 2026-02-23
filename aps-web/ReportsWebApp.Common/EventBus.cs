using System.Runtime.CompilerServices;

namespace ReportsWebApp.Common;

public class EventBus
{
    private List<EventBusListener> listeners = new List<EventBusListener>();

    /// <summary>
    /// Posts an IEvent to the EventBus, async listeners will spawn a new non-awaited Task when Invoked while sync listeners block and execute when invoked 
    /// </summary>
    /// <param name="event"></param>
    public async Task PostEventAsync(IEvent @event)
    {
        PostEventSync(@event);
    }

    public void PostEventSync(IEvent @event)
    {
        lock (listeners)
        {
            List<Exception> exceptions = new List<Exception>();
            foreach (EventBusListener listener in listeners)
            {
                if (listener.EventTypes.Any(t => t.Equals(@event.GetType())))
                {
                    if (listener is AsyncEventBusListener)
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                listener.Invoke(@event);
                            }
                            catch (Exception e)
                            {
                                throw; //I think this will log correctly? TODO: verify this logs
                            }
                        });
                    }
                    else
                    {
                        try
                        {
                            listener.Invoke(@event);
                        }
                        catch (Exception e)
                        {
                            exceptions.Add(e);
                        }    
                    }
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
    
    internal void AddListener(EventBusListener listener)
    {
        lock (listeners)
            listeners.Add(listener);
    }

    internal void RemoveListener(EventBusListener listener)
    {
        lock (listeners)
            listeners.Remove(listener);
    }
    
    public static EventBus Main { get; } = new EventBus();
}

/// <summary>
/// Basic Event Bus Listener. when constructed it subscribes to an EventBus and listens for the specified Event Types.
/// when a matching Event Type is posted to the EventBus the listeners callback is invoked. if your callback is expected
/// to block for a long period of time (>~20ms) please use the <c>AsyncEventBusListener</c>
/// </summary>
public class EventBusListener : IDisposable
{
    public delegate void EventCallback(IEvent @event); 
    private EventBus m_eventBus;
    private EventCallback m_callback;
    public Type[] EventTypes { get; private set; }
    public EventBusListener(EventBus eventBus, EventCallback invokeCallback, params Type[] eventTypes)
    {
        EventTypes = eventTypes;
        m_eventBus = eventBus;
        m_callback = invokeCallback;
        eventBus.AddListener(this);
    }
    
    public EventBusListener(EventCallback invokeCallback, params Type[] eventTypes)
    {
        EventTypes = eventTypes;
        m_eventBus = EventBus.Main;
        m_callback = invokeCallback;
        m_eventBus.AddListener(this);
    }

    internal virtual void Invoke(IEvent @event)
    {
        m_callback(@event);
    }

    public void Dispose()
    {
        m_eventBus.RemoveListener(this);
    }
}

/// <summary>
/// Like the <c>EventBusListener</c> but callback is expected to block for a while. Callback is Invoked on a newly created Task. **DO NOT** use this unless you expect your
/// callback to block for some time. Overusing this could result in Task explosion and saturation of the Task Scheduler. If we need many async listeners
/// switch the implementation of this to use a <c>Channel&lt;T&gt;</c> for processing tasks in a queued manner.
/// </summary>
public class AsyncEventBusListener : EventBusListener
{
    public AsyncEventBusListener(EventBus eventBus, EventCallback invokeCallback, params Type[] eventTypes) : base(eventBus, invokeCallback, eventTypes) { }
    public AsyncEventBusListener(EventCallback invokeCallback, params Type[] eventTypes) : base(invokeCallback, eventTypes) { }

    internal override void Invoke(IEvent @event)
    {
        Task.Run(() => { base.Invoke(@event); });
    }
}

//only exists for strong typing.
public interface IEvent 
{
    
}

public record PlanningAreaStatusList(List<PlanningAreaStatus> StatusList, int ServerId);
public record PlanningAreaStatus(string PlanningAreaKey, EServiceState state);
public class PAStatusUpdateEvent(PlanningAreaStatusList a_statusList) : IEvent
{
    public PlanningAreaStatusList PlanningAreaStatusList { get; private set; } = a_statusList;
}

public class PlanningAreaShutdownEvent(string token) : IEvent
{
    public string Token { get; set; } = token;
}
public record ActionStatusList(List<ActionStatus> StatusList);
public record ActionStatus(Guid TransactionId, string RequestStatus, string? ErrorMessage, DateTime? UpdatedDateTime);
public class ActionStatusUpdateEvent(ActionStatusList a_StatusList) : IEvent
{
    public ActionStatusList ActionStatusList { get; private set; } = a_StatusList;
}
