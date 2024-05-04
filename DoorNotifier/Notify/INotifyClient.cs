namespace DoorNotifier.Notify;

public interface INotifyClient
{
    TimeSpan After { get; }

    Task PostAsync(string doorState);
}