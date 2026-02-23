using PT.PackageDefinitions;
using PT.UIDefinitions;

namespace PT.PackageDefinitionsUI;

public interface INavigationListenerModule
{
    List<INavigationListenerElement> GetNotificationElements();
}

/// <summary>
/// A listener event provides a simple way to attach to the UI navigation events
/// without having to have another object to be created.
/// All navigation listener events will be created at started.
/// These events can also intercept an event and prevent it from being sent to other
/// non listener events
/// </summary>
public interface INavigationListenerElement : IPackageElement
{
    //Lower values will be sent the event first
    int Priority { get; }

    /// <summary>
    /// If the event is set to Handled, it will be completed and no other listeners will see it
    /// </summary>
    /// <param name="a_event"></param>
    void ProcessNavigationEvent(UINavigationEvent a_event);
}