using DoorNotifier.Sensor;
using DoorNotifier.Notify;

namespace DoorNotifier;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISensorClient _sensorClient;
    private readonly INotifyClient _notifyClient;

    private bool _wasClosed;
    private bool _wasNotified;
    private DateTime _lastChange;

    public Worker(
        ILogger<Worker> logger,
        ISensorClient sensorClient,
        INotifyClient notifyClient
    )
    {
        _logger = logger;
        _sensorClient = sensorClient;
        _notifyClient = notifyClient;
    }

    private async Task CheckDoorStateAsync()
    {
        var wasClosed = _wasClosed;
        var lastChange = _lastChange;
        var wasNotified = _wasNotified;

        var now = DateTime.UtcNow;
        var elapsed = now - lastChange;

        var payload = await _sensorClient.GetAsync();
        _logger.LogInformation(LogEvent.DoorState, "Door state {Description} {Elapsed:g}", payload, elapsed);
        var isClosed = payload == SensorClient.CLOSED;
        var isChange = wasClosed != isClosed;

        if (isChange)
        {
            _logger.LogDebug(LogEvent.DoorChanged, "Door state has changed");
            _wasClosed = isClosed;
            _lastChange = now;
            _wasNotified = false;
        }

        if (isClosed && isChange && wasNotified)
        {
            _logger.LogDebug(LogEvent.DoorClosed, "Open door was closed");
            // Send notification that OPEN door has become closed.
            await _notifyClient.PostAsync(payload);
            return;
        }

        if (!isClosed && !wasNotified && elapsed >= _notifyClient.After)
        {
            _logger.LogDebug(LogEvent.DoorLeftOpen, "Door was left open");
            _wasNotified = true;
            await _notifyClient.PostAsync(payload);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(LogEvent.PollingStarted, "Polling {Description:g}", _sensorClient.Interval);
        try
        {
            // Before going into the loop, load initial values.
            var payload = await _sensorClient.GetAsync();
            _lastChange = DateTime.UtcNow;
            _wasClosed = payload == SensorClient.CLOSED;

            using var timer = new PeriodicTimer(_sensorClient.Interval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CheckDoorStateAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(LogEvent.ShuttingDown, "Shutting down");
        }
        catch (Exception ex)
        {
            _logger.LogError(LogEvent.WorkerError, ex, "Worker Error {Description}", ex.GetBaseException().Message);
        }
    }
}
