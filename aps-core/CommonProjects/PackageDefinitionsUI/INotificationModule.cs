using System.Windows.Forms;

using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI;

public interface INotificationModule
{
    List<INotificationElement> GetNotificationElements(IScenarioInfo a_scenarioInfo);
}

public interface INotificationElement : IPackageElement
{
    Control NotificationSlide { get; }

    string ElementKey { get; }

    PackageEnums.ENotificationUrgency Urgency { get; }

    /// <summary>
    /// Fire event to hide notification tile from notification bar in scenario viewer
    /// </summary>
    event Action<INotificationElement> RemoveNotification;

    /// <summary>
    /// Fire event to show notification tile in the notification bar in scenario viewer,
    /// send boolean to display bar automatically if it was hidden
    /// </summary>
    event Action<INotificationElement, bool> ShowNotification;
}