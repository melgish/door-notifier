namespace DoorNotifier.Sensor;

public interface ISensorClient
{
    /// <summary>
    /// How often to poll for changes
    /// </summary>
    /// <value>The default value is 1 minute.</value>
    TimeSpan Interval { get; }

    /// <summary>
    /// Gets current state of garage door from sensor.
    /// </summary>
    /// <returns>Either CLOSED, OPEN, or UNKNOWN</returns>
    Task<string> GetAsync();
}