using System.Net;

using DoorNotifier.Sensor;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

namespace DoorNotifier.Tests.Sensor;

public class SensorClientTests
{
    private readonly FakeLogger<SensorClient> _fakeLogger;
    private readonly HttpClient _httpClient;
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly IOptions<SensorOptions> _options;


    public SensorClientTests()
    {
        _mockHandler = new();
        _httpClient = new(_mockHandler.Object);
        _fakeLogger = new();
        _options = Options.Create(new SensorOptions { Uri = new("http://127.0.0.1/test-topic") });
    }

    private SensorClient CreateSensorClient()
    {
        return new SensorClient(_options, _fakeLogger, _httpClient);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
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
              StatusCode = statusCode,
              Content = new StringContent(content)
          });
    }


    [Fact]
    public async Task GetAsync_SuccessfulRequest_ReturnsResult()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "OPEN");
        var client = CreateSensorClient();

        // Act
        var result = await client.GetAsync();

        // Assert
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

}