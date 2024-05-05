using Microsoft.Extensions.Options;

namespace DoorNotifier.Sensor;

public class SensorClient : ISensorClient
{
    // Possible states from door sensor..
    public const string CLOSED = "CLOSED";
    public const string OPEN = "OPEN";
    public const string UNKNOWN = "UNKNOWN";

    private readonly ILogger<SensorClient> _logger;
    private readonly SensorOptions _options;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Gets interval from client options
    /// </summary>
    public TimeSpan Interval => _options.Interval;

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="logger">.NET Logger</param>
    /// <param name="options">Options with URL and Interval values</param>
    /// <param name="httpClient">Client for making Get requests</param>
    public SensorClient(
        ILogger<SensorClient> logger,
        IOptions<SensorOptions> options,
        HttpClient httpClient
    )
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets current state of garage door from sensor.
    /// </summary>
    /// <returns>Either CLOSED, OPEN, or UNKNOWN</returns>
    public async Task<string> GetAsync()
    {
        try
        {
            return await _httpClient.GetStringAsync(string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(LogEvent.GetStatusFailed, ex, "Failed to get door status");
            return UNKNOWN;
        }
    }
}