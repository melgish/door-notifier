using DoorNotifier.Notify;
using DoorNotifier.Sensor;

using Microsoft.Extensions.Options;

namespace DoorNotifier.Worker;

internal sealed class WorkerService(
    IOptions<WorkerOptions> options,
    ILogger<WorkerService> logger,
    ISensorClient sensorClient,
    INotifyClient notifyClient
) : BackgroundService
{
    private bool _wasClosed;
    private DateTime _lastChange;

    private async Task SetInitialValuesAsync()
    {
        // Before going into the loop, load initial values.
        var payload = await sensorClient.GetAsync();
        _lastChange = DateTime.UtcNow;
        _wasClosed = payload == SensorClient.CLOSED;
    }

    private async Task CheckDoorStateAsync()
    {
        var now = DateTime.UtcNow;

        // Get the current state of the door.
        var payload = await sensorClient.GetAsync();
        var isClosed = payload == SensorClient.CLOSED;
        var isChange = _wasClosed != isClosed;

        if (isChange)
        {
            _lastChange = now;
            _wasClosed = isClosed;
            // Notify on every change...
            logger.LogDebug(LogEvent.DoorChanged, "Door state has changed to {Description}", payload);
            await notifyClient.PostAsync(payload);
        }

        var elapsed = now - _lastChange;
        logger.LogInformation(LogEvent.DoorState, "Door state {Description} {Elapsed:g}", payload, elapsed);
        if (!_wasClosed && elapsed > options.Value.After)
        {
            // Notify again when it has been open for a while.
            logger.LogDebug(LogEvent.DoorLeftOpen, "Door was left open");
            // Restart the clock to notify again after another hour.
            _lastChange = now;
            await notifyClient.PostAsync(payload);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(LogEvent.PollingStarted, "Polling {Description:g}", options.Value.Interval);
        try
        {
            using var timer = new PeriodicTimer(options.Value.Interval);
            await SetInitialValuesAsync();
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckDoorStateAsync();
                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation(LogEvent.ShuttingDown, "Shutting down");
        }
        catch (Exception ex)
        {
            logger.LogError(LogEvent.WorkerError, ex, "Worker Error {Description}", ex.GetBaseException().Message);
        }
    }
}