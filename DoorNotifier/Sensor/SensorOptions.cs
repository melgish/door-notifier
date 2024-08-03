using System.ComponentModel.DataAnnotations;

namespace DoorNotifier.Sensor;

internal sealed record SensorOptions
{
    /// <summary>
    /// The web address to query.
    /// </summary>
    [Required]
    public Uri Uri { get; set; } = default!;
}