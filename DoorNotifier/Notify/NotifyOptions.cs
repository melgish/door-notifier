using System.ComponentModel.DataAnnotations;

namespace DoorNotifier.Notify;

public sealed class NotifyOptions
{
    /// <summary>
    /// Settings key
    /// </summary>
    public const string Notify = "Notify";

    /// <summary>
    /// How long to wait after door is opened to send
    /// notification.
    /// </summary>
    public TimeSpan After { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// The NTFY address/topic for posting notifications.
    /// </summary>
    [Required]
    public Uri Uri { get; set; } = null!;

    /// <summary>
    /// NTFY authentication token
    /// </summary>
    [Required]
    public string Token { get; set; } = null!;
}
