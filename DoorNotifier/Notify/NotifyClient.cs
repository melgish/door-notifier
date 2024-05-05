using Microsoft.Extensions.Options;
using System.Text;

namespace DoorNotifier.Notify;

/// <summary>
/// NotifyClient class is responsible for sending notifications about the state of the garage door.
/// </summary>
public sealed class NotifyClient : INotifyClient
{
    private readonly ILogger<NotifyClient> _logger;
    private readonly NotifyOptions _options;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Get how long to wait for notification.
    /// </summary>
    /// <value>
    /// The time span after which the notification will be sent.
    /// </value>
    public TimeSpan After => _options.After;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotifyClient"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to log any errors or warnings.</param>
    /// <param name="options">The options instance containing the notification settings.</param>
    /// <param name="httpClient">The HTTP client instance to send the notification request.</param>
    public NotifyClient(
        ILogger<NotifyClient> logger,
        IOptions<NotifyOptions> options,
        HttpClient httpClient
    )
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Send the notification.
    /// </summary>
    /// <param name="doorState">The current state of the garage door.</param>
    public async Task PostAsync(string doorState)
    {
        try
        {
            var message = $"The Garage Door is {doorState}";
            var content = new StringContent(message, Encoding.UTF8, "text/plain");
            var rs = await _httpClient.PostAsync(string.Empty, content);
            if (!rs.IsSuccessStatusCode)
            {
                _logger.LogWarning(LogEvent.SendStatusCode, "Failed to send status {Description}", rs.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(LogEvent.SendStatusFailed, ex, "Failed to send status {Description}", ex.GetBaseException().Message);
        }
    }
}