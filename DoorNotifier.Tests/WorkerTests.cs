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
        _mockSensorClient = new(MockBehavior.Strict);
        // Using very short times here to make test run fast.
        _mockSensorClient.Setup(s => s.Interval).Returns(TimeSpan.FromSeconds(0.1));

        _mockNotifyClient = new(MockBehavior.Strict);
        _mockNotifyClient.Setup(n => n.After).Returns(TimeSpan.FromSeconds(0.3));
        _mockNotifyClient
            .Setup(n => n.PostAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoorIsClosed_ShouldNotifyOnce()
    {
        var worker = new WorkerService(
          _fakeLogger,
          _mockSensorClient.Object,
          _mockNotifyClient.Object
        );

        // Door starts open, and goes closed
        var once = true;
        _mockSensorClient
          .Setup(s => s.GetAsync())
          .Returns(() =>
          {
              if (once)
              {
                  once = false;
                  return Task.FromResult(SensorClient.OPEN);
              }
              return Task.FromResult(SensorClient.CLOSED);
          });


        await worker.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));
        await worker.StopAsync(CancellationToken.None);

        // Assert
        // 7 is enough to cover 'After'
        _mockSensorClient.Verify(s => s.GetAsync(), Times.AtLeast(7));
        _mockNotifyClient.Verify(n => n.PostAsync(SensorClient.CLOSED), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoorIsOpened_ShouldNotifyEveryfter()
    {
        var worker = new WorkerService(
          _fakeLogger,
          _mockSensorClient.Object,
          _mockNotifyClient.Object
        );

        // Door starts open, and stays that way.
        _mockSensorClient
          .Setup(s => s.GetAsync())
          .ReturnsAsync(SensorClient.OPEN);


        await worker.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));
        await worker.StopAsync(CancellationToken.None);

        // Assert
        // 7 is enough to cover 'After'
        _mockSensorClient.Verify(s => s.GetAsync(), Times.AtLeast(7));
        _mockNotifyClient.Verify(n => n.PostAsync(SensorClient.OPEN), Times.AtLeast(2));
    }
}
