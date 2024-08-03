using System.ComponentModel.DataAnnotations;

namespace DoorNotifier.Worker;

internal sealed class WorkerOptions
{
    /// <summary>
    /// How long to wait after door is opened to send
    /// notification.
    /// </summary>
    /// <value>The default value is 1 hour.</value>
    [Required]
    public TimeSpan After { get; init; } = TimeSpan.FromHours(1);
    /// <summary>
    /// How often to poll for changes in door state.
    /// </summary>
    /// <value>The default value is 1 minute</value>
    [Required]
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

}