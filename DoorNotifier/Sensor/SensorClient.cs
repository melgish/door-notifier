using Microsoft.Extensions.Options;

namespace DoorNotifier.Sensor;

/// <summary>
/// Initializes a new instance
/// </summary>
/// <param name="logger">.NET Logger</param>
/// <param name="options">Options with URL and Interval values</param>
/// <param name="httpClient">Client for making Get requests</param>
public sealed class SensorClient(
    ILogger<SensorClient> logger,
    IOptions<SensorOptions> options,
    HttpClient httpClient
) : ISensorClient
{
    // Possible states from door sensor..
    public const string CLOSED = "CLOSED";
    public const string OPEN = "OPEN";
    public const string UNKNOWN = "UNKNOWN";

    /// <summary>
    /// Gets interval from client options
    /// </summary>
    public TimeSpan Interval => options.Value.Interval;

    /// <summary>
    /// Gets current state of garage door from sensor.
    /// </summary>
    /// <returns>Either CLOSED, OPEN, or UNKNOWN</returns>
    public async Task<string> GetAsync()
    {
        try
        {
            return await httpClient.GetStringAsync(string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogWarning(LogEvent.GetStatusFailed, ex, "Failed to get door status");
            return UNKNOWN;
        }
    }
}