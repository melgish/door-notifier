using DoorNotifier.Notify;
using DoorNotifier.Sensor;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace DoorNotifier.Tests;

public class WorkerTests
{
    private readonly FakeLogger<WorkerService> _fakeLogger;
    private readonly Mock<INotifyClient> _mockNotifyClient;
    private readonly Mock<ISensorClient> _mockSensorClient;

    public WorkerTests()
    {
        _fakeLogger = new();
        _mockSensorClient = new();
        _mockSensorClient.Setup(s => s.Interval).Returns(TimeSpan.FromSeconds(0.1));

        _mockNotifyClient = new();
        _mockNotifyClient.Setup(n => n.After).Returns(TimeSpan.FromSeconds(0.5));
        _mockNotifyClient
            .Setup(n => n.PostAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoorIsClosed_ShouldNotNotify()
    {
        var worker = new WorkerService(
          _fakeLogger,
          _mockSensorClient.Object,
          _mockNotifyClient.Object
        );

        _mockSensorClient.Setup(s => s.GetAsync()).ReturnsAsync(SensorClient.CLOSED);

        // Act
        await worker.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));
        await worker.StopAsync(CancellationToken.None);

        // Assert
        _mockSensorClient.Verify(n => n.GetAsync(), Times.AtLeast(10));
        _mockNotifyClient.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoorIsOpenTooLong_ShouldNotifyOnce()
    {
        var worker = new WorkerService(
          _fakeLogger,
          _mockSensorClient.Object,
          _mockNotifyClient.Object
        );

        _mockSensorClient.Setup(s => s.GetAsync()).ReturnsAsync(SensorClient.OPEN);

        // Act
        await worker.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));
        await worker.StopAsync(CancellationToken.None);

        // Assert
        _mockSensorClient.Verify(n => n.GetAsync(), Times.AtLeast(10));
        _mockNotifyClient.Verify(n => n.PostAsync(SensorClient.OPEN), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoorIsClosedAfterBeingOpen_ShouldNotify()
    {
        var worker = new WorkerService(
          _fakeLogger,
          _mockSensorClient.Object,
          _mockNotifyClient.Object
        );

        _mockSensorClient.Setup(s => s.GetAsync()).ReturnsAsync(SensorClient.OPEN);

        // Act
        await worker.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));
        _mockSensorClient.Setup(s => s.GetAsync()).ReturnsAsync(SensorClient.CLOSED);
        await Task.Delay(TimeSpan.FromSeconds(1));
        await worker.StopAsync(CancellationToken.None);

        // Assert
        _mockSensorClient.Verify(n => n.GetAsync(), Times.AtLeast(20));
        _mockNotifyClient.Verify(n => n.PostAsync(SensorClient.OPEN), Times.Once);
        _mockNotifyClient.Verify(n => n.PostAsync(SensorClient.CLOSED), Times.Once);
    }
}
