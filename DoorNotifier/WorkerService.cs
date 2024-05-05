using DoorNotifier.Notify;
using DoorNotifier.Sensor;

namespace DoorNotifier;

public class WorkerService : BackgroundService
{
    private readonly ILogger<WorkerService> _logger;
    private readonly ISensorClient _sensorClient;
    private readonly INotifyClient _notifyClient;

    private bool _wasClosed;
    private DateTime _lastChange;

    public WorkerService(
        ILogger<WorkerService> logger,
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
        var now = DateTime.UtcNow;

        // Get the current state of the door.
        var payload = await _sensorClient.GetAsync();
        var isClosed = payload == SensorClient.CLOSED;
        var isChange = _wasClosed != isClosed;

        if (isChange) {
            _lastChange = now;
            _wasClosed = isClosed;
            // Notify on every change...
            _logger.LogDebug(LogEvent.DoorChanged, "Door state has changed to {Description}", payload);
            await _notifyClient.PostAsync(payload);
         }

        var elapsed = now - _lastChange;
        _logger.LogInformation(LogEvent.DoorState, "Door state {Description} {Elapsed:g}", payload, elapsed);
        if (!_wasClosed && elapsed > _notifyClient.After) {
            // Notify again when it has been open for a while.
            _logger.LogDebug(LogEvent.DoorLeftOpen, "Door was left open");
            // Restart the clock to notify again after another hour.
            _lastChange = now;
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
