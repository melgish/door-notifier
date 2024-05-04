namespace DoorNotifier.Sensor;

public interface ISensorClient
{
    TimeSpan Interval { get; }

    Task<string> GetAsync();
}