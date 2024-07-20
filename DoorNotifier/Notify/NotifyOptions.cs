using System.ComponentModel.DataAnnotations;

namespace DoorNotifier.Notify;

public sealed record NotifyOptions
{
    /// <summary>
    /// Settings key.
    /// </summary>
    public const string Notify = "Notify";

    /// <summary>
    /// How long to wait after door is opened to send
    /// notification.
    /// </summary>
    /// <value>The default value is 1 hour.</value>
    public TimeSpan After { get; init; } = TimeSpan.FromHours(1);

    /// <summary>
    /// The NTFY address/topic for posting notifications.
    /// </summary>
    [Required]
    public Uri Uri { get; init; } = null!;

    /// <summary>
    /// NTFY authentication token.
    /// </summary>
    [Required]
    public string Token { get; init; } = string.Empty;
}
