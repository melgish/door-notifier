using DoorNotifier.Sensor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;

namespace DoorNotifier.Tests;

public class SensorClientTests
{
    private readonly FakeLogger<SensorClient> _fakeLogger;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<SensorOptions>> _mockOptions;
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly SensorOptions _options;

    public SensorClientTests()
    {
        _fakeLogger = new();
        _options = new()
        {
            Interval = TimeSpan.FromMinutes(5),
            Uri = new Uri("http://127.0.0.1/test-topic")
        };
        _mockOptions = new();
        _mockOptions.Setup(o => o.Value).Returns(_options);
        _mockHandler = new();
        _httpClient = new(_mockHandler.Object) { BaseAddress = _options.Uri };
    }

    [Fact]
    public void Interval_ShouldReturnOptionsValue()
    {
        var client = CreateSensorClient();

        Assert.Equal(_options.Interval, client.Interval);
    }

    [Fact]
    public async Task GetAsync_SuccessfulRequest_ReturnsResult()
    {
        _mockHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
          )
          .ReturnsAsync(new HttpResponseMessage
          {
              StatusCode = HttpStatusCode.OK,
              Content = new StringContent("OPEN")
          });

        var client = CreateSensorClient();
        var result = await client.GetAsync();

        Assert.Equal("OPEN", result);
    }

    [Fact]
    public async Task GetAsync_FailedStatusCode_LogsWarning()
    {
        var statusCode = HttpStatusCode.BadGateway;

        _mockHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
          )
          .ReturnsAsync(new HttpResponseMessage
          {
              StatusCode = statusCode,
          });

        var client = CreateSensorClient();
        var result = await client.GetAsync();

        Assert.Equal("UNKNOWN", result);
        Assert.Equal(LogLevel.Warning, _fakeLogger.LatestRecord.Level);
    }

    [Fact]
    public async Task GetAsync_Exception_LogsWarning()
    {
        _mockHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
          )
          .ThrowsAsync(new Exception("Network Error"));

        var client = CreateSensorClient();
        var result = await client.GetAsync();

        Assert.Equal("UNKNOWN", result);
        Assert.Equal(LogLevel.Warning, _fakeLogger.LatestRecord.Level);
    }

    private SensorClient CreateSensorClient()
    {
        return new SensorClient(_fakeLogger, _mockOptions.Object, _httpClient);
    }
}
