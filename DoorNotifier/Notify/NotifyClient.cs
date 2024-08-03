using System.Net.Mime;
using System.Text;

using Microsoft.Extensions.Options;

namespace DoorNotifier.Notify;

internal interface INotifyClient
{
    /// <summary>
    /// Send the notification.
    /// </summary>
    /// <param name="doorState">The current state of the garage door.</param>
    Task PostAsync(string doorState);
}

/// <summary>
/// NotifyClient class is responsible for sending notifications about the state of the garage door.
/// </summary>
internal sealed class NotifyClient : INotifyClient
{
    private readonly ILogger<NotifyClient> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotifyClient"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to log any errors or warnings.</param>
    /// <param name="options">The options instance containing the notification settings.</param>
    /// <param name="httpClient">The HTTP client instance to send the notification request.</param>
    public NotifyClient(
        IOptions<NotifyOptions> options,
        ILogger<NotifyClient> logger,
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = options.Value.Uri;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new(MediaTypeNames.Application.Json));
        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", options.Value.Token);
        _logger = logger;
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