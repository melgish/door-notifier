using System.Net.Mime;

using Microsoft.Extensions.Options;

namespace DoorNotifier.Sensor;

internal interface ISensorClient
{
    /// <summary>
    /// Gets current state of garage door from sensor.
    /// </summary>
    /// <returns>Either CLOSED, OPEN, or UNKNOWN</returns>
    Task<string> GetAsync();
}

/// <summary>
/// Initializes a new instance
/// </summary>
/// <param name="logger">.NET Logger</param>
/// <param name="options">Options with URL and Interval values</param>
/// <param name="httpClient">Client for making Get requests</param>
internal sealed class SensorClient : ISensorClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SensorClient> _logger;

    // Possible states from door sensor..
    public const string CLOSED = "CLOSED";
    public const string OPEN = "OPEN";
    public const string UNKNOWN = "UNKNOWN";

    /// <summary>
    /// Initializes a new instance of the <see cref="SensorClient"/> class.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="httpClient"></param>
    public SensorClient(
        IOptions<SensorOptions> options,
        ILogger<SensorClient> logger,
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = options.Value.Uri;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new(MediaTypeNames.Text.Plain));
        _logger = logger;
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