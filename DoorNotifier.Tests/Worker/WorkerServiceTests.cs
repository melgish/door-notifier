using DoorNotifier.Notify;
using DoorNotifier.Sensor;
using DoorNotifier.Worker;

using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Moq;

namespace DoorNotifier.Tests;

public sealed class WorkerTests
{
    private readonly FakeLogger<WorkerService> _logger;
    private readonly Mock<INotifyClient> _mockNotifyClient;
    private readonly Mock<ISensorClient> _mockSensorClient;
    private readonly IOptions<WorkerOptions> _options;
    private readonly CancellationTokenSource _tokenSource;

    private readonly WorkerService _workerService;

    public WorkerTests()
    {
        _logger = new();

        _mockSensorClient = new(MockBehavior.Strict);
        _mockSensorClient
            .Setup(s => s.GetAsync())
            .ReturnsAsync(SensorClient.CLOSED);

        _mockNotifyClient = new(MockBehavior.Strict);
        _mockNotifyClient
            .Setup(n => n.PostAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _options = Options.Create(new WorkerOptions
        {
            Interval = TimeSpan.FromSeconds(0.1),
            After = TimeSpan.FromSeconds(0.3)
        });

        _tokenSource = new();

        _workerService = new(
            _options,
            _logger,
            _mockSensorClient.Object,
            _mockNotifyClient.Object
        );
    }

    private async Task RunFor(TimeSpan timeSpan)
    {
        await _workerService.StartAsync(_tokenSource.Token);
        await Task.Delay(timeSpan);
        await _workerService.StopAsync(_tokenSource.Token);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoorIsClosed_ShouldNotifyOnce()
    {
        // Arrange
        // Door starts open, and goes closed
        var calls = 0;
        _mockSensorClient
          .Setup(s => s.GetAsync())
          .ReturnsAsync(() => (0 == calls++) ? SensorClient.OPEN :SensorClient.CLOSED);

        // Act
        await RunFor(TimeSpan.FromSeconds(1));

        // Assert
        // 7 is enough to cover 'After'
        _mockSensorClient.Verify(s => s.GetAsync(), Times.AtLeast(7));
        _mockNotifyClient.Verify(n => n.PostAsync(SensorClient.CLOSED), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoorIsOpened_ShouldNotifyEveryAfter()
    {
        // Door starts open, and stays that way.
        _mockSensorClient
          .Setup(s => s.GetAsync())
          .ReturnsAsync(SensorClient.OPEN);

        // Act
        await RunFor(TimeSpan.FromSeconds(1));

        // Assert
        // 7 is enough to cover 'After'
        _mockSensorClient.Verify(s => s.GetAsync(), Times.AtLeast(7));
        _mockNotifyClient.Verify(n => n.PostAsync(SensorClient.OPEN), Times.AtLeast(2));
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnexpectedError_ShouldLogError()
    {
        // Something bad happens
        _mockSensorClient
          .Setup(s => s.GetAsync())
          .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        await RunFor(TimeSpan.FromSeconds(1));

        // Assert
        Assert.Matches("Unexpected", _logger.LatestRecord.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenShutDown_ShouldStop()
    {
        // Arrange
        _tokenSource.Cancel();

        // Act
        await RunFor(TimeSpan.FromSeconds(1));

        // Assert
        Assert.Matches("Polling", _logger.LatestRecord.Message);
    }
}