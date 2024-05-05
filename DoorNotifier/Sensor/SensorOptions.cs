using System.ComponentModel.DataAnnotations;

namespace DoorNotifier.Sensor;

public sealed class SensorOptions
{
    /// <summary>
    /// Settings key
    /// </summary>
    public const string Sensor = "Sensor";

    /// <summary>
    /// How often to poll for changes.
    /// </summary>
    /// <value>The default value is 1 minute</value>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// The web address to query.
    /// </summary>
    [Required]
    public Uri Uri { get; set; } = null!;
}
