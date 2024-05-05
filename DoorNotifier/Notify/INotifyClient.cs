namespace DoorNotifier.Notify;

public interface INotifyClient
{
    /// <summary>
    /// How long to wait after door is opened to send
    /// notification.
    /// </summary>
    /// <value>The default value is 1 hour.</value>
    TimeSpan After { get; }

    /// <summary>
    /// Send the notification.
    /// </summary>
    /// <param name="doorState">The current state of the garage door.</param>
    Task PostAsync(string doorState);
}