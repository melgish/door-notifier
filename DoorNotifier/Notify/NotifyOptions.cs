using System.ComponentModel.DataAnnotations;

namespace DoorNotifier.Notify;

internal sealed record NotifyOptions
{
    /// <summary>
    /// Settings key.
    /// </summary>
    public const string Notify = "Notify";

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